using System.Security.Cryptography;
using System.Text;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums.PartyMode;
using Melodee.Common.Models;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using SerilogTimings;

namespace Melodee.Common.Services;

/// <summary>
/// Service for managing party sessions.
/// </summary>
public sealed class PartySessionService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory)
    : ServiceBase(logger, cacheManager, contextFactory), IPartySessionService
{
    private const string CacheKeyTemplate = "urn:party:session:{0}";

    public async Task<OperationResult<PartySession>> CreateAsync(
        string name,
        int ownerUserId,
        string? joinCode,
        CancellationToken cancellationToken = default)
    {
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
        if (!configuration.GetValue<bool>(PartyModeOptions.SectionName, x => x.Enabled))
        {
            return new OperationResult<PartySession>("Party mode is not enabled.")
            {
                Type = OperationResponseType.AccessDenied,
                Data = null!
            };
        }

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var ownerUser = await scopedContext.Users.FindAsync([ownerUserId], cancellationToken).ConfigureAwait(false);
        if (ownerUser == null)
        {
            return new OperationResult<PartySession>($"User with ID {ownerUserId} not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        string? joinCodeHash = null;
        if (!string.IsNullOrEmpty(joinCode))
        {
            joinCodeHash = HashJoinCode(joinCode);
        }

        var session = new PartySession
        {
            Name = name,
            OwnerUserId = ownerUserId,
            JoinCodeHash = joinCodeHash,
            Status = PartySessionStatus.Active,
            QueueRevision = 1,
            PlaybackRevision = 1
        };

        var participant = new PartySessionParticipant
        {
            PartySessionId = session.Id,
            UserId = ownerUserId,
            Role = PartyRole.Owner,
            JoinedAt = SystemClock.Instance.GetCurrentInstant()
        };

        scopedContext.PartySessions.Add(session);
        scopedContext.PartySessionParticipants.Add(participant);

        var playbackState = new PartyPlaybackState
        {
            PartySessionId = session.Id,
            PositionSeconds = 0,
            IsPlaying = false
        };
        scopedContext.PartyPlaybackStates.Add(playbackState);

        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Logger.Information("[PartySessionService] Created party session {SessionName} (ID: {SessionId}) for user {UserId}",
            name, session.Id, ownerUserId);

        return new OperationResult<PartySession>(session);
    }

    public async Task<OperationResult<PartySession?>> GetAsync(
        Guid sessionApiKey,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var cacheKey = string.Format(CacheKeyTemplate, sessionApiKey);
        if (CacheManager.TryGet(cacheKey, out PartySession? cached))
        {
            return new OperationResult<PartySession?>(cached);
        }

        var session = await scopedContext.PartySessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session != null)
        {
            CacheManager.Set(cacheKey, session, TimeSpan.FromMinutes(5));
        }

        return new OperationResult<PartySession?>(session);
    }

    public async Task<OperationResult<PartySessionParticipant>> JoinAsync(
        Guid sessionApiKey,
        int userId,
        string? joinCode,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<PartySessionParticipant>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        if (session.Status == PartySessionStatus.Ended)
        {
            return new OperationResult<PartySessionParticipant>("Session has ended.")
            {
                Type = OperationResponseType.BadRequest
            };
        }

        var user = await scopedContext.Users.FindAsync([userId], cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return new OperationResult<PartySessionParticipant>($"User with ID {userId} not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        if (!string.IsNullOrEmpty(session.JoinCodeHash))
        {
            if (string.IsNullOrEmpty(joinCode) || HashJoinCode(joinCode) != session.JoinCodeHash)
            {
                return new OperationResult<PartySessionParticipant>("Invalid join code.")
                {
                    Type = OperationResponseType.Unauthorized
                };
            }
        }

        var existingParticipant = session.Participants.FirstOrDefault(p => p.UserId == userId);
        if (existingParticipant != null)
        {
            return new OperationResult<PartySessionParticipant>(existingParticipant);
        }

        var participant = new PartySessionParticipant
        {
            PartySessionId = session.Id,
            UserId = userId,
            Role = PartyRole.Listener,
            JoinedAt = SystemClock.Instance.GetCurrentInstant()
        };

        session.Participants.Add(participant);
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.RemoveByPrefix(string.Format(CacheKeyTemplate, sessionApiKey));

        Logger.Information("[PartySessionService] User {UserId} joined session {SessionId}", userId, session.Id);

        return new OperationResult<PartySessionParticipant>(participant);
    }

    public async Task<OperationResult<bool>> LeaveAsync(
        Guid sessionApiKey,
        int userId,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<bool>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        var participant = session.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant == null)
        {
            return new OperationResult<bool>("User is not a participant of this session.")
            {
                Type = OperationResponseType.BadRequest
            };
        }

        if (participant.Role == PartyRole.Owner)
        {
            return new OperationResult<bool>("Owner cannot leave. Use EndSession instead.")
            {
                Type = OperationResponseType.BadRequest
            };
        }

        session.Participants.Remove(participant);
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.RemoveByPrefix(string.Format(CacheKeyTemplate, sessionApiKey));

        Logger.Information("[PartySessionService] User {UserId} left session {SessionId}", userId, session.Id);

        return new OperationResult<bool>(true);
    }

    public async Task<OperationResult<bool>> EndAsync(
        Guid sessionApiKey,
        int requestingUserId,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<bool>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        if (session.OwnerUserId != requestingUserId)
        {
            return new OperationResult<bool>("Only the session owner can end the session.")
            {
                Type = OperationResponseType.Forbidden
            };
        }

        if (session.Status == PartySessionStatus.Ended)
        {
            return new OperationResult<bool>("Session is already ended.")
            {
                Type = OperationResponseType.BadRequest
            };
        }

        session.Status = PartySessionStatus.Ended;
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.RemoveByPrefix(string.Format(CacheKeyTemplate, sessionApiKey));

        Logger.Information("[PartySessionService] Session {SessionId} ended by user {UserId}", session.Id, requestingUserId);

        return new OperationResult<bool>(true);
    }

    public async Task<OperationResult<IEnumerable<PartySessionParticipant>>> GetParticipantsAsync(
        Guid sessionApiKey,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<IEnumerable<PartySessionParticipant>>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        var participants = await scopedContext.PartySessionParticipants
            .AsNoTracking()
            .Where(p => p.PartySessionId == session.Id)
            .Include(p => p.User)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new OperationResult<IEnumerable<PartySessionParticipant>>(participants);
    }

    public async Task<OperationResult<PartySessionParticipant?>> GetParticipantAsync(
        Guid sessionApiKey,
        int userId,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<PartySessionParticipant?>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        var participant = await scopedContext.PartySessionParticipants
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PartySessionId == session.Id && p.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        return new OperationResult<PartySessionParticipant?>(participant);
    }

    public async Task<OperationResult<PartyRole?>> GetUserRoleAsync(
        Guid sessionApiKey,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var participantResult = await GetParticipantAsync(sessionApiKey, userId, cancellationToken).ConfigureAwait(false);

        if (!participantResult.IsSuccess)
        {
            return new OperationResult<PartyRole?>(participantResult.Errors?.FirstOrDefault()?.Message ?? "Error")
            {
                Type = participantResult.Type
            };
        }

        return new OperationResult<PartyRole?>(participantResult.Data?.Role);
    }

    private static string HashJoinCode(string joinCode)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(joinCode));
        return Convert.ToHexString(hashBytes);
    }
}
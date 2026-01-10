using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services;

/// <summary>
/// Service for managing party playback state and operations.
/// </summary>
public sealed class PartyPlaybackService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory)
    : ServiceBase(logger, cacheManager, contextFactory), IPartyPlaybackService
{
    private const string CacheKeyTemplate = "urn:party:playback:{0}";

    public async Task<OperationResult<PartyPlaybackState?>> GetPlaybackStateAsync(
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
            return new OperationResult<PartyPlaybackState?>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        var cacheKey = string.Format(CacheKeyTemplate, sessionApiKey);
        if (CacheManager.TryGet(cacheKey, out PartyPlaybackState? cached))
        {
            return new OperationResult<PartyPlaybackState?>(cached);
        }

        var playbackState = await scopedContext.PartyPlaybackStates
            .AsNoTracking()
            .Include(x => x.CurrentQueueItem)
            .FirstOrDefaultAsync(x => x.PartySessionId == session.Id, cancellationToken)
            .ConfigureAwait(false);

        if (playbackState != null)
        {
            CacheManager.Set(cacheKey, playbackState, TimeSpan.FromSeconds(5));
        }

        return new OperationResult<PartyPlaybackState?>(playbackState);
    }

    public async Task<OperationResult<PartyPlaybackState>> UpdateFromHeartbeatAsync(
        Guid sessionApiKey,
        Guid? currentQueueItemApiKey,
        double positionSeconds,
        bool isPlaying,
        double? volume01,
        int endpointUserId,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .Include(x => x.PlaybackState)
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<PartyPlaybackState>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        if (session.PlaybackState == null)
        {
            return new OperationResult<PartyPlaybackState>("Playback state not found for session.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        session.PlaybackState.PositionSeconds = positionSeconds;
        session.PlaybackState.IsPlaying = isPlaying;
        session.PlaybackState.Volume = volume01;
        session.PlaybackState.CurrentQueueItemApiKey = currentQueueItemApiKey;
        session.PlaybackState.LastHeartbeatAt = SystemClock.Instance.GetCurrentInstant();
        session.PlaybackState.UpdatedByUserId = endpointUserId;

        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.RemoveByPrefix(string.Format(CacheKeyTemplate, sessionApiKey));

        Logger.Debug("[PartyPlaybackService] Updated playback state for session {SessionId}: position={Position}, playing={IsPlaying}",
            sessionApiKey, positionSeconds, isPlaying);

        return new OperationResult<PartyPlaybackState>(session.PlaybackState);
    }

    public async Task<OperationResult<PartyPlaybackState>> SetCurrentItemAsync(
        Guid sessionApiKey,
        Guid? queueItemApiKey,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .Include(x => x.PlaybackState)
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<PartyPlaybackState>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        if (session.PlaybackState == null)
        {
            return new OperationResult<PartyPlaybackState>("Playback state not found for session.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        session.PlaybackState.CurrentQueueItemApiKey = queueItemApiKey;
        session.PlaybackState.PositionSeconds = 0;
        session.PlaybackState.IsPlaying = false;

        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.RemoveByPrefix(string.Format(CacheKeyTemplate, sessionApiKey));

        Logger.Information("[PartyPlaybackService] Set current item to {ItemApiKey} for session {SessionId}",
            queueItemApiKey ?? Guid.Empty, sessionApiKey);

        return new OperationResult<PartyPlaybackState>(session.PlaybackState);
    }

    public async Task<OperationResult<PartyPlaybackState>> UpdateIntentAsync(
        Guid sessionApiKey,
        PlaybackIntent intent,
        double? positionSeconds,
        int requestingUserId,
        long expectedRevision,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .Include(x => x.PlaybackState)
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<PartyPlaybackState>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        if (session.PlaybackState == null)
        {
            return new OperationResult<PartyPlaybackState>("Playback state not found for session.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        if (session.PlaybackRevision != expectedRevision)
        {
            return new OperationResult<PartyPlaybackState>(
                $"Concurrent modification detected. Current revision: {session.PlaybackRevision}")
            {
                Type = OperationResponseType.Conflict
            };
        }

        switch (intent)
        {
            case PlaybackIntent.Play:
                session.PlaybackState.IsPlaying = true;
                if (positionSeconds.HasValue)
                {
                    session.PlaybackState.PositionSeconds = positionSeconds.Value;
                }
                break;

            case PlaybackIntent.Pause:
                session.PlaybackState.IsPlaying = false;
                if (positionSeconds.HasValue)
                {
                    session.PlaybackState.PositionSeconds = positionSeconds.Value;
                }
                break;

            case PlaybackIntent.Skip:
                var queueItems = await scopedContext.PartyQueueItems
                    .Where(x => x.PartySessionId == session.Id)
                    .OrderBy(x => x.SortOrder)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                var currentItemIndex = session.PlaybackState.CurrentQueueItemApiKey.HasValue
                    ? queueItems.FindIndex(x => x.ApiKey == session.PlaybackState.CurrentQueueItemApiKey)
                    : -1;

                PartyQueueItem? nextItem = null;
                if (currentItemIndex >= 0 && currentItemIndex < queueItems.Count - 1)
                {
                    nextItem = queueItems[currentItemIndex + 1];
                }
                else if (queueItems.Any())
                {
                    nextItem = queueItems.First();
                }

                session.PlaybackState.CurrentQueueItemApiKey = nextItem?.ApiKey;
                session.PlaybackState.PositionSeconds = 0;
                session.PlaybackState.IsPlaying = true;
                break;

            case PlaybackIntent.Seek:
                if (positionSeconds.HasValue && positionSeconds.Value >= 0)
                {
                    session.PlaybackState.PositionSeconds = positionSeconds.Value;
                }
                else
                {
                    return new OperationResult<PartyPlaybackState>("Invalid seek position.")
                    {
                        Type = OperationResponseType.BadRequest
                    };
                }
                break;
        }

        session.PlaybackState.UpdatedByUserId = requestingUserId;
        session.PlaybackRevision++;
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.RemoveByPrefix(string.Format(CacheKeyTemplate, sessionApiKey));

        Logger.Information("[PartyPlaybackService] Applied intent {Intent} for session {SessionId} by user {UserId}",
            intent, sessionApiKey, requestingUserId);

        return new OperationResult<PartyPlaybackState>(session.PlaybackState);
    }
}
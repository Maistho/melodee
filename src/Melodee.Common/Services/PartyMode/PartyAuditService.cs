using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services;

/// <summary>
/// Service for managing party session audit events.
/// </summary>
public interface IPartyAuditService
{
    Task<OperationResult<PartyAuditEvent>> LogEventAsync(
        int partySessionId,
        int userId,
        PartyAuditEventType eventType,
        string? payloadJson = null,
        CancellationToken cancellationToken = default);

    Task<OperationResult<IEnumerable<PartyAuditEvent>>> GetSessionAuditLogAsync(
        Guid sessionApiKey,
        int? take = null,
        CancellationToken cancellationToken = default);
}

public sealed class PartyAuditService(
    ILogger logger,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : IPartyAuditService
{
    public async Task<OperationResult<PartyAuditEvent>> LogEventAsync(
        int partySessionId,
        int userId,
        PartyAuditEventType eventType,
        string? payloadJson = null,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var auditEvent = new PartyAuditEvent
        {
            PartySessionId = partySessionId,
            UserId = userId,
            EventType = eventType,
            PayloadJson = payloadJson,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        scopedContext.PartyAuditEvents.Add(auditEvent);
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.Debug("Logged audit event {EventType} for session {SessionId} by user {UserId}",
            eventType, partySessionId, userId);

        return new OperationResult<PartyAuditEvent>(auditEvent);
    }

    public async Task<OperationResult<IEnumerable<PartyAuditEvent>>> GetSessionAuditLogAsync(
        Guid sessionApiKey,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<IEnumerable<PartyAuditEvent>>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        var query = scopedContext.PartyAuditEvents
            .AsNoTracking()
            .Where(x => x.PartySessionId == session.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Include(x => x.User);

        if (take.HasValue)
        {
            query = query.Take(take.Value) as IQueryable<PartyAuditEvent>;
        }

        var events = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        return new OperationResult<IEnumerable<PartyAuditEvent>>(events);
    }
}

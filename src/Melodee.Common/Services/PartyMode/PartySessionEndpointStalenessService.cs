using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Melodee.Common.Services;

/// <summary>
/// Service for detecting and managing endpoint staleness.
/// </summary>
public interface IPartySessionEndpointStalenessService
{
    /// <summary>
    /// Checks if an endpoint is stale.
    /// </summary>
    bool IsStale(PartySessionEndpoint endpoint);

    /// <summary>
    /// Gets all stale endpoints.
    /// </summary>
    Task<OperationResult<IEnumerable<PartySessionEndpoint>>> GetStaleEndpointsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks sessions with stale endpoints.
    /// </summary>
    Task<int> MarkSessionsWithStaleEndpointsAsync(CancellationToken cancellationToken = default);
}

public sealed class PartySessionEndpointStalenessService(
    ILogger<PartySessionEndpointStalenessService> logger,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : IPartySessionEndpointStalenessService
{
    private const int DefaultStaleThresholdSeconds = 30;

    public bool IsStale(PartySessionEndpoint endpoint)
    {
        if (!endpoint.LastSeenAt.HasValue)
        {
            return true;
        }

        return IsStaleByTimestamp(endpoint.LastSeenAt.Value);
    }

    public async Task<OperationResult<IEnumerable<PartySessionEndpoint>>> GetStaleEndpointsAsync(
        CancellationToken cancellationToken = default)
    {
        var threshold = SystemClock.Instance.GetCurrentInstant() - Duration.FromSeconds(DefaultStaleThresholdSeconds);

        await using var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var staleEndpoints = await scopedContext.PartySessionEndpoints
            .AsNoTracking()
            .Where(x => x.LastSeenAt.HasValue && x.LastSeenAt < threshold)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new OperationResult<IEnumerable<PartySessionEndpoint>> { Data = staleEndpoints };
    }

    public async Task<int> MarkSessionsWithStaleEndpointsAsync(CancellationToken cancellationToken = default)
    {
        var threshold = SystemClock.Instance.GetCurrentInstant() - Duration.FromSeconds(DefaultStaleThresholdSeconds);

        await using var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var sessionsWithStaleEndpoints = await scopedContext.PartySessions
            .Where(s => s.ActiveEndpointId.HasValue)
            .Select(s => new { s.Id, s.ActiveEndpoint })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var count = 0;
        foreach (var session in sessionsWithStaleEndpoints)
        {
            if (session.ActiveEndpoint != null && session.ActiveEndpoint.LastSeenAt < threshold)
            {
                logger.LogDebug("Session {SessionId} has stale endpoint {EndpointId}", session.Id, session.ActiveEndpoint.Id);
                count++;
            }
        }

        if (count > 0)
        {
            logger.LogInformation("Found {Count} sessions with stale endpoints", count);
        }

        return count;
    }

    private static bool IsStaleByTimestamp(Instant lastSeenAt)
    {
        var threshold = SystemClock.Instance.GetCurrentInstant() - Duration.FromSeconds(DefaultStaleThresholdSeconds);
        return lastSeenAt < threshold;
    }
}

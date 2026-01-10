using Melodee.Common.Services.Caching;
using Microsoft.Extensions.Logging;

namespace Melodee.Common.Services;

/// <summary>
/// Service for enforcing skip cooldown periods.
/// </summary>
public interface ISkipCooldownService
{
    Task<bool> CanSkipAsync(int sessionId, int userId, CancellationToken cancellationToken = default);
    Task RecordSkipAsync(int sessionId, int userId, CancellationToken cancellationToken = default);
}

public sealed class SkipCooldownService(
    ICacheManager cacheManager,
    ILogger<SkipCooldownService> logger) : ISkipCooldownService
{
    private static readonly TimeSpan SkipCooldown = TimeSpan.FromSeconds(10);
    private const string CacheKeyTemplate = "urn:party:skipcooldown:{0}:{1}";

    public async Task<bool> CanSkipAsync(int sessionId, int userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = string.Format(CacheKeyTemplate, sessionId, userId);
        var lastSkip = await cacheManager.GetAsync<DateTime?>(
            cacheKey,
            async () => await Task.FromResult<DateTime?>(null),
            cancellationToken,
            null);

        if (lastSkip == null)
        {
            return true;
        }

        var timeSinceLastSkip = DateTime.UtcNow - lastSkip.Value;
        var canSkip = timeSinceLastSkip >= SkipCooldown;

        if (!canSkip)
        {
            logger.LogDebug("Skip blocked for user {UserId} in session {SessionId}. {TimeRemaining} seconds remaining.",
                userId, sessionId, (SkipCooldown - timeSinceLastSkip).TotalSeconds);
        }

        return canSkip;
    }

    public async Task RecordSkipAsync(int sessionId, int userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = string.Format(CacheKeyTemplate, sessionId, userId);
        await cacheManager.GetAsync<DateTime>(
            cacheKey,
            async () => await Task.FromResult(DateTime.UtcNow),
            cancellationToken,
            SkipCooldown);

        logger.LogDebug("Skip recorded for user {UserId} in session {SessionId}", userId, sessionId);
    }
}

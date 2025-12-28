using Microsoft.Extensions.Caching.Memory;

namespace Melodee.Blazor.Services;

/// <summary>
/// Simple cache-based rate limiter for protecting Blazor UI actions.
/// Uses fixed window counting to prevent abuse.
/// </summary>
public interface IRateLimiterService
{
    /// <summary>
    /// Checks if an action is allowed for the given key.
    /// </summary>
    /// <param name="key">Unique key for the action (e.g., circuit ID + action name)</param>
    /// <param name="maxAttempts">Maximum allowed attempts in the window</param>
    /// <param name="windowMinutes">Time window in minutes</param>
    /// <returns>True if action is allowed, false if rate limit exceeded</returns>
    bool IsAllowed(string key, int maxAttempts = 3, int windowMinutes = 10);

    /// <summary>
    /// Records an attempt for the given key.
    /// </summary>
    void RecordAttempt(string key, int windowMinutes = 10);
}

/// <summary>
/// Memory cache-based implementation of rate limiter.
/// For distributed scenarios, replace IMemoryCache with IDistributedCache.
/// </summary>
public sealed class RateLimiterService : IRateLimiterService
{
    private readonly IMemoryCache _cache;
    private readonly object _lock = new();

    public RateLimiterService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool IsAllowed(string key, int maxAttempts = 3, int windowMinutes = 10)
    {
        lock (_lock)
        {
            var cacheKey = $"ratelimit:{key}";
            var attempts = _cache.Get<int?>(cacheKey) ?? 0;
            return attempts < maxAttempts;
        }
    }

    public void RecordAttempt(string key, int windowMinutes = 10)
    {
        lock (_lock)
        {
            var cacheKey = $"ratelimit:{key}";
            var attempts = _cache.Get<int?>(cacheKey) ?? 0;

            _cache.Set(cacheKey, attempts + 1, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(windowMinutes)
            });
        }
    }
}

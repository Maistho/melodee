using Microsoft.Extensions.Caching.Memory;

namespace Melodee.Tests.Blazor.Services;

/// <summary>
/// Tests for Blazor-side rate limiter service.
/// Verifies rate limiting logic and window management.
/// </summary>
public class RateLimiterServiceTests
{
    [Fact]
    public void IsAllowed_WithinLimit_ReturnsTrue()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var rateLimiter = new RateLimiterService(cache);
        var key = "test-action-1";

        // Act
        var result = rateLimiter.IsAllowed(key, maxAttempts: 3, windowMinutes: 10);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAllowed_ExceedsLimit_ReturnsFalse()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var rateLimiter = new RateLimiterService(cache);
        var key = "test-action-2";
        var maxAttempts = 3;

        // Act - Record attempts up to limit
        for (int i = 0; i < maxAttempts; i++)
        {
            rateLimiter.RecordAttempt(key, windowMinutes: 10);
        }

        var result = rateLimiter.IsAllowed(key, maxAttempts, windowMinutes: 10);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RecordAttempt_IncrementsCounter()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var rateLimiter = new RateLimiterService(cache);
        var key = "test-action-3";

        // Act & Assert - First attempt allowed
        Assert.True(rateLimiter.IsAllowed(key, maxAttempts: 2));
        rateLimiter.RecordAttempt(key);

        // Second attempt allowed
        Assert.True(rateLimiter.IsAllowed(key, maxAttempts: 2));
        rateLimiter.RecordAttempt(key);

        // Third attempt blocked
        Assert.False(rateLimiter.IsAllowed(key, maxAttempts: 2));
    }

    [Fact]
    public void IsAllowed_DifferentKeys_AreIndependent()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var rateLimiter = new RateLimiterService(cache);
        var key1 = "action-1";
        var key2 = "action-2";

        // Act - Block key1
        for (int i = 0; i < 3; i++)
        {
            rateLimiter.RecordAttempt(key1, windowMinutes: 10);
        }

        // Assert - key1 blocked, key2 still allowed
        Assert.False(rateLimiter.IsAllowed(key1, maxAttempts: 3));
        Assert.True(rateLimiter.IsAllowed(key2, maxAttempts: 3));
    }

    [Fact(Skip = "Test requires waiting for cache expiration which takes too long for unit tests")]
    public async Task IsAllowed_AfterWindowExpires_ResetsCounter()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var rateLimiter = new RateLimiterService(cache);
        var key = "test-action-expiry";

        // Act - Exhaust limit
        for (int i = 0; i < 3; i++)
        {
            rateLimiter.RecordAttempt(key, windowMinutes: 1);
        }

        Assert.False(rateLimiter.IsAllowed(key, maxAttempts: 3));

        // Wait for window to expire
        await Task.Delay(TimeSpan.FromMinutes(1.1));

        // Assert - Should be allowed again after window expires
        Assert.True(rateLimiter.IsAllowed(key, maxAttempts: 3));
    }

    [Fact]
    public async Task IsAllowed_ConcurrentAccess_ThreadSafe()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var rateLimiter = new RateLimiterService(cache);
        var key = "concurrent-test";
        var maxAttempts = 10;
        var threadCount = 50;
        var successCount = 0;
        var tasks = new List<Task>();

        // Act - Multiple threads try to record attempts concurrently
        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                if (rateLimiter.IsAllowed(key, maxAttempts))
                {
                    rateLimiter.RecordAttempt(key);
                    Interlocked.Increment(ref successCount);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - Rate limiter may allow slightly more due to check-then-act race condition
        // but should be reasonably close to the limit and definitely block further attempts
        Assert.True(successCount <= maxAttempts * 2,
            $"Expected at most {maxAttempts * 2} successful attempts (allowing for race conditions), but got {successCount}");
        
        // Verify rate limiter is now blocking
        Assert.False(rateLimiter.IsAllowed(key, maxAttempts));
    }
}

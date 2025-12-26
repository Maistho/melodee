using Melodee.Common.Services.Caching;
using Melodee.Common.Serialization;
using Serilog;

namespace Melodee.Tests.Common.Services.Caching;

/// <summary>
/// Tests to verify that genre data is properly cached to avoid expensive database queries.
/// The GetGenresAsync method queries ALL albums and songs to aggregate genre counts,
/// which is expensive and should be cached.
/// </summary>
public class GenresCachingTests
{
    private const string GenresCacheKey = "urn:album:genres";
    
    [Fact]
    public async Task GetGenresAsync_SecondCall_ShouldUseCachedData()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var serializer = new Serializer(logger);
        var cacheManager = new MemoryCacheManager(logger, TimeSpan.FromMinutes(5), serializer);
        
        var factoryCallCount = 0;
        var testData = new Dictionary<string, (int songCount, int albumCount)>
        {
            { "Rock", (100, 10) },
            { "Pop", (50, 5) },
            { "Jazz", (30, 3) }
        };
        
        // Simulate what GetGenresAsync does - cache the genre data
        async Task<Dictionary<string, (int songCount, int albumCount)>> GetGenresAsync()
        {
            return await cacheManager.GetAsync(GenresCacheKey, async () =>
            {
                Interlocked.Increment(ref factoryCallCount);
                await Task.Delay(50); // Simulate DB query time
                return testData;
            }, CancellationToken.None, TimeSpan.FromMinutes(5), "test-region");
        }
        
        // Act - First call should hit factory
        var result1 = await GetGenresAsync();
        
        // Second call should use cache
        var result2 = await GetGenresAsync();
        
        // Third call should use cache
        var result3 = await GetGenresAsync();
        
        // Assert
        Assert.Equal(1, factoryCallCount);
        Assert.Equal(testData.Count, result1.Count);
        Assert.Equal(testData.Count, result2.Count);
        Assert.Equal(testData.Count, result3.Count);
        Assert.Equal(100, result1["Rock"].songCount);
    }
    
    [Fact]
    public async Task GetGenresAsync_ConcurrentRequests_ShouldOnlyCallFactoryOnce()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var serializer = new Serializer(logger);
        var cacheManager = new MemoryCacheManager(logger, TimeSpan.FromMinutes(5), serializer);
        
        var factoryCallCount = 0;
        var testData = new Dictionary<string, (int songCount, int albumCount)>
        {
            { "Rock", (100, 10) },
            { "Pop", (50, 5) }
        };
        
        async Task<Dictionary<string, (int songCount, int albumCount)>> GetGenresAsync()
        {
            return await cacheManager.GetAsync(GenresCacheKey, async () =>
            {
                Interlocked.Increment(ref factoryCallCount);
                await Task.Delay(100); // Simulate slow DB query
                return testData;
            }, CancellationToken.None, TimeSpan.FromMinutes(5), "test-region");
        }
        
        // Act - Fire 10 concurrent requests
        var tasks = Enumerable.Range(0, 10).Select(_ => GetGenresAsync()).ToArray();
        var results = await Task.WhenAll(tasks);
        
        // Assert - Factory should only be called once due to request coalescing
        Assert.Equal(1, factoryCallCount);
        Assert.All(results, r => Assert.Equal(2, r.Count));
    }
    
    [Fact]
    public async Task GetGenresAsync_AfterCacheClear_ShouldCallFactoryAgain()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var serializer = new Serializer(logger);
        var cacheManager = new MemoryCacheManager(logger, TimeSpan.FromMinutes(5), serializer);
        
        var factoryCallCount = 0;
        var testData = new Dictionary<string, (int songCount, int albumCount)>
        {
            { "Rock", (100, 10) }
        };
        
        async Task<Dictionary<string, (int songCount, int albumCount)>> GetGenresAsync()
        {
            return await cacheManager.GetAsync(GenresCacheKey, async () =>
            {
                Interlocked.Increment(ref factoryCallCount);
                await Task.Delay(10);
                return testData;
            }, CancellationToken.None, TimeSpan.FromMinutes(5), "test-region");
        }
        
        // Act - First call
        await GetGenresAsync();
        Assert.Equal(1, factoryCallCount);
        
        // Clear cache (simulates album update clearing genre cache)
        cacheManager.Remove(GenresCacheKey, "test-region");
        
        // Second call after cache clear should hit factory again
        await GetGenresAsync();
        
        // Assert
        Assert.Equal(2, factoryCallCount);
    }
    
    [Fact]
    public async Task GetGenresAsync_PerformanceImprovement_CachedCallShouldBeFast()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var serializer = new Serializer(logger);
        var cacheManager = new MemoryCacheManager(logger, TimeSpan.FromMinutes(5), serializer);
        
        const int simulatedDbQueryTimeMs = 500;
        var testData = new Dictionary<string, (int songCount, int albumCount)>
        {
            { "Rock", (100, 10) },
            { "Pop", (50, 5) }
        };
        
        async Task<Dictionary<string, (int songCount, int albumCount)>> GetGenresAsync()
        {
            return await cacheManager.GetAsync(GenresCacheKey, async () =>
            {
                await Task.Delay(simulatedDbQueryTimeMs); // Simulate slow DB query
                return testData;
            }, CancellationToken.None, TimeSpan.FromMinutes(5), "test-region");
        }
        
        // Act - First call (cache miss, slow)
        var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
        await GetGenresAsync();
        stopwatch1.Stop();
        
        // Second call (cache hit, should be fast)
        var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
        await GetGenresAsync();
        stopwatch2.Stop();
        
        // Assert
        Assert.True(stopwatch1.ElapsedMilliseconds >= simulatedDbQueryTimeMs * 0.8, 
            $"First call should take at least {simulatedDbQueryTimeMs}ms, took {stopwatch1.ElapsedMilliseconds}ms");
        Assert.True(stopwatch2.ElapsedMilliseconds < 50, 
            $"Cached call should be fast (<50ms), took {stopwatch2.ElapsedMilliseconds}ms");
    }
}

using Melodee.Common.Services.Caching;

namespace Melodee.Tests.Common.Services.Caching;

public sealed class SingleFlightCacheTests
{
    [Fact]
    public async Task GetOrCreateAsync_WhenEmpty_ExecutesFactory()
    {
        var factoryCallCount = 0;
        var cache = new SingleFlightCache<string, string>(
            key => key.ToUpperInvariant(),
            maxSize: 100);

        var (result, wasHit, wasCoalesced) = await cache.GetOrCreateAsync(
            "test",
            async (key, ct) =>
            {
                Interlocked.Increment(ref factoryCallCount);
                await Task.Delay(10, ct);
                return "value";
            },
            CancellationToken.None);

        Assert.Equal("value", result);
        Assert.False(wasHit);
        Assert.False(wasCoalesced);
        Assert.Equal(1, factoryCallCount);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenCached_ReturnsCachedValue()
    {
        var factoryCallCount = 0;
        var cache = new SingleFlightCache<string, string>(
            key => key.ToUpperInvariant(),
            maxSize: 100);

        await cache.GetOrCreateAsync(
            "test",
            async (key, ct) =>
            {
                Interlocked.Increment(ref factoryCallCount);
                return "value";
            },
            CancellationToken.None);

        var (result, wasHit, wasCoalesced) = await cache.GetOrCreateAsync(
            "test",
            async (key, ct) =>
            {
                Interlocked.Increment(ref factoryCallCount);
                return "different";
            },
            CancellationToken.None);

        Assert.Equal("value", result);
        Assert.True(wasHit);
        Assert.Equal(1, factoryCallCount);
    }

    [Fact]
    public async Task GetOrCreateAsync_ConcurrentSameKey_OnlyOneFactoryCall()
    {
        var factoryCallCount = 0;
        var cache = new SingleFlightCache<string, string>(
            key => key.ToUpperInvariant(),
            maxSize: 100);

        var tasks = Enumerable.Range(0, 10).Select(_ =>
            cache.GetOrCreateAsync(
                "test",
                async (key, ct) =>
                {
                    Interlocked.Increment(ref factoryCallCount);
                    await Task.Delay(100, ct);
                    return "value";
                },
                CancellationToken.None)).ToList();

        var results = await Task.WhenAll(tasks);

        Assert.All(results, r => Assert.Equal("value", r.Value));
        Assert.Equal(1, factoryCallCount);
        Assert.True(results.Count(r => r.WasCoalesced) >= 1);
    }

    [Fact]
    public async Task GetOrCreateAsync_DifferentKeys_MultipleFactoryCalls()
    {
        var factoryCallCount = 0;
        var cache = new SingleFlightCache<string, string>(
            key => key.ToUpperInvariant(),
            maxSize: 100);

        var tasks = Enumerable.Range(0, 5).Select(i =>
            cache.GetOrCreateAsync(
                $"key{i}",
                async (key, ct) =>
                {
                    Interlocked.Increment(ref factoryCallCount);
                    await Task.Delay(10, ct);
                    return $"value{key}";
                },
                CancellationToken.None)).ToList();

        await Task.WhenAll(tasks);

        Assert.Equal(5, factoryCallCount);
    }

    [Fact]
    public async Task GetOrCreateAsync_FactoryThrows_CachesNegativeResult()
    {
        var factoryCallCount = 0;
        var cache = new SingleFlightCache<string, string>(
            key => key.ToUpperInvariant(),
            maxSize: 100,
            negativeTtl: TimeSpan.FromMinutes(5));

        var (result1, _, _) = await cache.GetOrCreateAsync(
            "test",
            (key, ct) =>
            {
                Interlocked.Increment(ref factoryCallCount);
                throw new InvalidOperationException("Test error");
            },
            CancellationToken.None);

        var (result2, wasHit, _) = await cache.GetOrCreateAsync(
            "test",
            (key, ct) =>
            {
                Interlocked.Increment(ref factoryCallCount);
                return Task.FromResult<string?>("value");
            },
            CancellationToken.None);

        Assert.Null(result1);
        Assert.Null(result2);
        Assert.True(wasHit);
        Assert.Equal(1, factoryCallCount);
    }

    [Fact]
    public async Task GetOrCreateAsync_NullResult_CachesAsNegative()
    {
        var factoryCallCount = 0;
        var cache = new SingleFlightCache<string, string>(
            key => key.ToUpperInvariant(),
            maxSize: 100);

        await cache.GetOrCreateAsync(
            "test",
            (key, ct) =>
            {
                Interlocked.Increment(ref factoryCallCount);
                return Task.FromResult<string?>(null);
            },
            CancellationToken.None);

        var (_, wasHit, _) = await cache.GetOrCreateAsync(
            "test",
            (key, ct) =>
            {
                Interlocked.Increment(ref factoryCallCount);
                return Task.FromResult<string?>("value");
            },
            CancellationToken.None);

        Assert.True(wasHit);
        Assert.Equal(1, factoryCallCount);
    }

    [Fact]
    public void GetStatistics_ReturnsCorrectCounts()
    {
        var cache = new SingleFlightCache<string, string>(
            key => key.ToUpperInvariant(),
            maxSize: 100);

        cache.Add("key1", "value1");
        cache.Add("key2", "value2");
        cache.AddNegative("key3");

        var stats = cache.GetStatistics();

        Assert.Equal(3, stats.TotalEntries);
        Assert.Equal(2, stats.PositiveEntries);
        Assert.Equal(1, stats.NegativeEntries);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var cache = new SingleFlightCache<string, string>(
            key => key.ToUpperInvariant(),
            maxSize: 100);

        cache.Add("key1", "value1");
        cache.Add("key2", "value2");
        cache.Clear();

        var stats = cache.GetStatistics();

        Assert.Equal(0, stats.TotalEntries);
    }

    [Fact]
    public void TryGet_WhenNotCached_ReturnsFalse()
    {
        var cache = new SingleFlightCache<string, string>(
            key => key.ToUpperInvariant(),
            maxSize: 100);

        var found = cache.TryGet("test", out var value, out var isNegative);

        Assert.False(found);
        Assert.Null(value);
        Assert.False(isNegative);
    }

    [Fact]
    public void TryGet_WhenCached_ReturnsTrue()
    {
        var cache = new SingleFlightCache<string, string>(
            key => key.ToUpperInvariant(),
            maxSize: 100);

        cache.Add("test", "value");
        var found = cache.TryGet("test", out var value, out var isNegative);

        Assert.True(found);
        Assert.Equal("value", value);
        Assert.False(isNegative);
    }

    [Fact]
    public void NormalizedKey_IsCaseInsensitive()
    {
        var cache = new SingleFlightCache<string, string>(
            key => key.ToUpperInvariant(),
            maxSize: 100);

        cache.Add("Test", "value1");
        var found = cache.TryGet("TEST", out var value, out _);

        Assert.True(found);
        Assert.Equal("value1", value);
    }
}

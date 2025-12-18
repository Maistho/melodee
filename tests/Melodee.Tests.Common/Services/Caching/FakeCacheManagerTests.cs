using Melodee.Common.Serialization;
using Melodee.Common.Services.Caching;
using Moq;
using Serilog;

namespace Melodee.Tests.Common.Services.Caching;

public class FakeCacheManagerTests
{
    private readonly FakeCacheManager _cacheManager;

    public FakeCacheManagerTests()
    {
        var logger = new Mock<ILogger>();
        var serializer = new Mock<ISerializer>();
        _cacheManager = new FakeCacheManager(logger.Object, TimeSpan.FromMinutes(5), serializer.Object);
    }

    [Fact]
    public void Clear_DoesNotThrow()
    {
        var exception = Record.Exception(() => _cacheManager.Clear());
        Assert.Null(exception);
    }

    [Fact]
    public void ClearRegion_DoesNotThrow()
    {
        var exception = Record.Exception(() => _cacheManager.ClearRegion("test-region"));
        Assert.Null(exception);
    }

    [Fact]
    public async Task GetAsync_AlwaysCallsGetItem()
    {
        var callCount = 0;
        Func<Task<string>> getItem = () =>
        {
            callCount++;
            return Task.FromResult("test-value");
        };

        var result1 = await _cacheManager.GetAsync("key1", getItem, CancellationToken.None);
        var result2 = await _cacheManager.GetAsync("key1", getItem, CancellationToken.None);

        // FakeCacheManager doesn't cache, so getItem should be called both times
        Assert.Equal(2, callCount);
        Assert.Equal("test-value", result1);
        Assert.Equal("test-value", result2);
    }

    [Fact]
    public async Task GetAsync_ReturnsValueFromGetItem()
    {
        var expected = 42;
        var result = await _cacheManager.GetAsync("key", () => Task.FromResult(expected), CancellationToken.None);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetAsync_WithDuration_StillReturnsValue()
    {
        var result = await _cacheManager.GetAsync(
            "key",
            () => Task.FromResult("value"),
            CancellationToken.None,
            TimeSpan.FromMinutes(10));
        Assert.Equal("value", result);
    }

    [Fact]
    public async Task GetAsync_WithRegion_StillReturnsValue()
    {
        var result = await _cacheManager.GetAsync(
            "key",
            () => Task.FromResult("value"),
            CancellationToken.None,
            region: "test-region");
        Assert.Equal("value", result);
    }

    [Fact]
    public void Remove_ReturnsTrue()
    {
        var result = _cacheManager.Remove("any-key");
        Assert.True(result);
    }

    [Fact]
    public void Remove_WithRegion_ReturnsTrue()
    {
        var result = _cacheManager.Remove("any-key", "any-region");
        Assert.True(result);
    }

    [Fact]
    public void CacheStatistics_ReturnsEmptyCollection()
    {
        var stats = _cacheManager.CacheStatistics();
        Assert.Empty(stats);
    }

    [Fact]
    public void Constructor_SetsSerializerProperty()
    {
        var logger = new Mock<ILogger>();
        var serializer = new Mock<ISerializer>();
        var cacheManager = new FakeCacheManager(logger.Object, TimeSpan.FromMinutes(5), serializer.Object);

        Assert.Same(serializer.Object, cacheManager.Serializer);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        var serializer = new Mock<ISerializer>();
        Assert.Throws<ArgumentNullException>(() =>
            new FakeCacheManager(null!, TimeSpan.FromMinutes(5), serializer.Object));
    }

    [Fact]
    public void Constructor_WithNullSerializer_ThrowsArgumentNullException()
    {
        var logger = new Mock<ILogger>();
        Assert.Throws<ArgumentNullException>(() =>
            new FakeCacheManager(logger.Object, TimeSpan.FromMinutes(5), null!));
    }
}

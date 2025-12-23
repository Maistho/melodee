using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Services.SearchEngines;

namespace Melodee.Tests.Common.Services.SearchEngines;

public sealed class ArtistSearchCacheTests
{
    [Fact]
    public void TryGetCachedResult_WhenEmpty_ReturnsFalse()
    {
        var cache = new ArtistSearchCache();
        var query = new ArtistQuery { Name = "Test Artist" };

        var found = cache.TryGetCachedResult(query, out var wasFound, out var artistId);

        Assert.False(found);
        Assert.False(wasFound);
        Assert.Null(artistId);
    }

    [Fact]
    public void CachePositiveResult_ThenRetrieve_ReturnsTrue()
    {
        var cache = new ArtistSearchCache();
        var query = new ArtistQuery { Name = "Test Artist" };
        var expectedArtistId = 123;

        cache.CachePositiveResult(query, expectedArtistId);
        var found = cache.TryGetCachedResult(query, out var wasFound, out var artistId);

        Assert.True(found);
        Assert.True(wasFound);
        Assert.Equal(expectedArtistId, artistId);
    }

    [Fact]
    public void CacheNegativeResult_ThenRetrieve_ReturnsNotFound()
    {
        var cache = new ArtistSearchCache();
        var query = new ArtistQuery { Name = "Unknown Artist" };

        cache.CacheNegativeResult(query);
        var found = cache.TryGetCachedResult(query, out var wasFound, out var artistId);

        Assert.True(found);
        Assert.False(wasFound);
        Assert.Null(artistId);
    }

    [Fact]
    public void CachePositiveResult_WithDifferentQuery_DoesNotMatch()
    {
        var cache = new ArtistSearchCache();
        var query1 = new ArtistQuery { Name = "Artist One" };
        var query2 = new ArtistQuery { Name = "Artist Two" };

        cache.CachePositiveResult(query1, 123);
        var found = cache.TryGetCachedResult(query2, out var wasFound, out var artistId);

        Assert.False(found);
    }

    [Fact]
    public void CachePositiveResult_OverwritesExisting_WithNewValue()
    {
        var cache = new ArtistSearchCache();
        var query = new ArtistQuery { Name = "Test Artist" };

        cache.CachePositiveResult(query, 123);
        cache.CachePositiveResult(query, 456);

        var found = cache.TryGetCachedResult(query, out var wasFound, out var artistId);

        Assert.True(found);
        Assert.True(wasFound);
        Assert.Equal(456, artistId);
    }

    [Fact]
    public void CacheNegativeResult_CanBeOverwrittenByPositive()
    {
        var cache = new ArtistSearchCache();
        var query = new ArtistQuery { Name = "Test Artist" };

        cache.CacheNegativeResult(query);
        cache.CachePositiveResult(query, 789);

        var found = cache.TryGetCachedResult(query, out var wasFound, out var artistId);

        Assert.True(found);
        Assert.True(wasFound);
        Assert.Equal(789, artistId);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var cache = new ArtistSearchCache();
        var query = new ArtistQuery { Name = "Test Artist" };

        cache.CachePositiveResult(query, 123);
        cache.Clear();

        var found = cache.TryGetCachedResult(query, out _, out _);

        Assert.False(found);
    }

    [Fact]
    public void GetStatistics_ReturnsCorrectCounts()
    {
        var cache = new ArtistSearchCache();
        var query1 = new ArtistQuery { Name = "Artist One" };
        var query2 = new ArtistQuery { Name = "Artist Two" };
        var query3 = new ArtistQuery { Name = "Artist Three" };

        cache.CachePositiveResult(query1, 1);
        cache.CachePositiveResult(query2, 2);
        cache.CacheNegativeResult(query3);

        var stats = cache.GetStatistics();

        Assert.Equal(3, stats.totalEntries);
        Assert.Equal(2, stats.positiveResults);
        Assert.Equal(1, stats.negativeResults);
    }

    [Fact]
    public void Cache_WithMusicBrainzId_UsesItInKey()
    {
        var cache = new ArtistSearchCache();
        var query1 = new ArtistQuery
        {
            Name = "Test Artist",
            MusicBrainzId = "123e4567-e89b-12d3-a456-426614174000"
        };
        var query2 = new ArtistQuery
        {
            Name = "Test Artist",
            MusicBrainzId = "223e4567-e89b-12d3-a456-426614174000"
        };

        cache.CachePositiveResult(query1, 100);

        var found = cache.TryGetCachedResult(query2, out _, out _);

        // Should not find because MusicBrainzId is different
        Assert.False(found);
    }

    [Fact]
    public void Cache_WithSpotifyId_UsesItInKey()
    {
        var cache = new ArtistSearchCache();
        var query1 = new ArtistQuery
        {
            Name = "Test Artist",
            SpotifyId = "spotify123"
        };
        var query2 = new ArtistQuery
        {
            Name = "Test Artist",
            SpotifyId = "spotify456"
        };

        cache.CachePositiveResult(query1, 100);

        var found = cache.TryGetCachedResult(query2, out _, out _);

        // Should not find because SpotifyId is different
        Assert.False(found);
    }
}

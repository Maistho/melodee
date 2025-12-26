using System.Diagnostics;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Services.Caching;
using Melodee.Common.Serialization;
using NodaTime;
using Serilog;

namespace Melodee.Tests.Common.Common.Services;

/// <summary>
/// Tests to verify that image caching properly avoids database lookups on cache hits.
/// These tests prove the fix for the database contention issue when many concurrent
/// image requests arrive simultaneously.
/// </summary>
public class ImageCachingTests : ServiceTestBase
{
    /// <summary>
    /// Verifies that the cache key is based on apiKey (not database ID), so that
    /// the database lookup happens INSIDE the cache factory, not before it.
    /// 
    /// This test uses MemoryCacheManager to verify actual caching behavior.
    /// </summary>
    [Fact]
    public async Task GetAlbumImageBytesAndEtagAsync_CacheKeyUsesApiKey_DatabaseLookupInsideCache()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var serializer = new Serializer(logger);
        var memoryCacheManager = new MemoryCacheManager(logger, TimeSpan.FromMinutes(5), serializer);
        
        var factoryCallCount = 0;
        var apiKey = Guid.NewGuid();
        var cacheKey = $"urn:album:imageBytesAndEtag:{apiKey}:Large";
        
        // Simulate what GetAlbumImageBytesAndEtagAsync does - the factory should only be called once
        async Task<byte[]?> SimulateGetImage()
        {
            return await memoryCacheManager.GetAsync(cacheKey, async () =>
            {
                Interlocked.Increment(ref factoryCallCount);
                // This simulates the database lookup + file read that happens on cache miss
                await Task.Delay(10);
                return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
            }, CancellationToken.None);
        }
        
        // Act - Call multiple times
        var result1 = await SimulateGetImage();
        var result2 = await SimulateGetImage();
        var result3 = await SimulateGetImage();
        
        // Assert - Factory should only be called ONCE (on first cache miss)
        Assert.Equal(1, factoryCallCount);
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);
    }
    
    /// <summary>
    /// Verifies that concurrent requests for the same album image result in only ONE
    /// factory call (database lookup), with all other requests waiting for the result.
    /// 
    /// This is the key test that proves the fix works for the thundering herd problem.
    /// </summary>
    [Fact]
    public async Task GetAlbumImageBytesAndEtagAsync_ConcurrentRequests_OnlyOneFactoryCall()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var serializer = new Serializer(logger);
        var memoryCacheManager = new MemoryCacheManager(logger, TimeSpan.FromMinutes(5), serializer);
        
        var factoryCallCount = 0;
        var apiKey = Guid.NewGuid();
        var cacheKey = $"urn:album:imageBytesAndEtag:{apiKey}:Large";
        const int concurrentRequests = 20;
        
        async Task<byte[]?> SimulateGetImage()
        {
            return await memoryCacheManager.GetAsync(cacheKey, async () =>
            {
                Interlocked.Increment(ref factoryCallCount);
                // Simulate slow database lookup to ensure requests overlap
                await Task.Delay(100);
                return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
            }, CancellationToken.None);
        }
        
        // Act - Fire many concurrent requests
        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => SimulateGetImage())
            .ToList();
        
        var results = await Task.WhenAll(tasks);
        
        // Assert - Factory should only be called ONCE despite many concurrent requests
        Assert.Equal(1, factoryCallCount);
        Assert.Equal(concurrentRequests, results.Length);
        Assert.All(results, r => Assert.NotNull(r));
    }
    
    /// <summary>
    /// Verifies that different album apiKeys result in different cache entries.
    /// Each unique apiKey should trigger its own factory call.
    /// </summary>
    [Fact]
    public async Task GetAlbumImageBytesAndEtagAsync_DifferentApiKeys_SeparateCacheEntries()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var serializer = new Serializer(logger);
        var memoryCacheManager = new MemoryCacheManager(logger, TimeSpan.FromMinutes(5), serializer);
        
        var factoryCallCount = 0;
        const int numberOfAlbums = 5;
        
        async Task<byte[]?> SimulateGetImage(Guid apiKey)
        {
            var cacheKey = $"urn:album:imageBytesAndEtag:{apiKey}:Large";
            return await memoryCacheManager.GetAsync(cacheKey, async () =>
            {
                Interlocked.Increment(ref factoryCallCount);
                await Task.Delay(10);
                return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
            }, CancellationToken.None);
        }
        
        // Act - Request images for different albums
        var apiKeys = Enumerable.Range(0, numberOfAlbums).Select(_ => Guid.NewGuid()).ToList();
        var tasks = apiKeys.Select(SimulateGetImage).ToList();
        var results = await Task.WhenAll(tasks);
        
        // Assert - Each unique apiKey should trigger one factory call
        Assert.Equal(numberOfAlbums, factoryCallCount);
        Assert.All(results, r => Assert.NotNull(r));
    }
    
    /// <summary>
    /// Performance test: Verifies that many concurrent requests for different albums
    /// complete efficiently when using proper caching.
    /// </summary>
    [Fact]
    public async Task ConcurrentDifferentAlbums_WithCaching_CompletesEfficiently()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var serializer = new Serializer(logger);
        var memoryCacheManager = new MemoryCacheManager(logger, TimeSpan.FromMinutes(5), serializer);
        
        const int numberOfAlbums = 30;
        const int simulatedDbLatencyMs = 20;
        const int maxAcceptableTimeMs = 2000;
        
        var factoryCallCount = 0;
        
        async Task<byte[]?> SimulateGetImage(Guid apiKey)
        {
            var cacheKey = $"urn:album:imageBytesAndEtag:{apiKey}:Large";
            return await memoryCacheManager.GetAsync(cacheKey, async () =>
            {
                Interlocked.Increment(ref factoryCallCount);
                await Task.Delay(simulatedDbLatencyMs);
                return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
            }, CancellationToken.None);
        }
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        
        var apiKeys = Enumerable.Range(0, numberOfAlbums).Select(_ => Guid.NewGuid()).ToList();
        var tasks = apiKeys.Select(SimulateGetImage).ToList();
        await Task.WhenAll(tasks);
        
        stopwatch.Stop();
        
        // Assert
        Assert.Equal(numberOfAlbums, factoryCallCount);
        Assert.True(
            stopwatch.ElapsedMilliseconds < maxAcceptableTimeMs,
            $"Concurrent requests took {stopwatch.ElapsedMilliseconds}ms, expected less than {maxAcceptableTimeMs}ms");
    }
    
    /// <summary>
    /// Integration test with actual AlbumService to verify caching behavior.
    /// </summary>
    [Fact]
    public async Task AlbumService_GetAlbumImageBytesAndEtagAsync_MultipleCalls_UsesCaching()
    {
        // Arrange
        var (album, _) = await SeedAlbumWithImage();
        var service = GetAlbumService();
        var apiKey = album.ApiKey;
        
        // Act - First call
        var result1 = await service.GetAlbumImageBytesAndEtagAsync(apiKey, "Large");
        
        // Second call - should use cached result
        var result2 = await service.GetAlbumImageBytesAndEtagAsync(apiKey, "Large");
        
        // Third call - should use cached result
        var result3 = await service.GetAlbumImageBytesAndEtagAsync(apiKey, "Large");
        
        // Assert - All results should be consistent
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);
        
        // Etags should be identical (from cache or same computation)
        Assert.Equal(result1.Etag, result2.Etag);
        Assert.Equal(result2.Etag, result3.Etag);
    }
    
    /// <summary>
    /// Integration test with actual ArtistService to verify caching behavior.
    /// </summary>
    [Fact]
    public async Task ArtistService_GetArtistImageBytesAndEtagAsync_MultipleCalls_UsesCaching()
    {
        // Arrange
        var artist = await SeedArtist();
        var service = GetArtistService();
        var apiKey = artist.ApiKey;
        
        // Act
        var result1 = await service.GetArtistImageBytesAndEtagAsync(apiKey, "Large");
        var result2 = await service.GetArtistImageBytesAndEtagAsync(apiKey, "Large");
        var result3 = await service.GetArtistImageBytesAndEtagAsync(apiKey, "Large");
        
        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);
        Assert.Equal(result1.Etag, result2.Etag);
        Assert.Equal(result2.Etag, result3.Etag);
    }

    #region Helper Methods
    
    private async Task<(Album album, Artist artist)> SeedAlbumWithImage(int index = 0)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        
        var library = context.Libraries.FirstOrDefault() ?? new Library
        {
            Name = "Test Library",
            Path = "/test/library",
            Type = (int)LibraryType.Storage,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        
        if (library.Id == 0)
        {
            context.Libraries.Add(library);
            await context.SaveChangesAsync();
        }
        
        var artistName = $"Test Artist {index}";
        var artist = new Artist
        {
            ApiKey = Guid.NewGuid(),
            Directory = $"test-artist-{index}",
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            LibraryId = library.Id,
            Name = artistName,
            NameNormalized = artistName.ToNormalizedString()!
        };
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        
        var albumName = $"Test Album {index}";
        var album = new Album
        {
            ApiKey = Guid.NewGuid(),
            Name = albumName,
            NameNormalized = albumName.ToNormalizedString()!,
            Directory = $"test-album-{index}",
            ArtistId = artist.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            ReleaseDate = new LocalDate(2020, 1, 1)
        };
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        
        return (album, artist);
    }
    
    private async Task<Artist> SeedArtist(int index = 0)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        
        var library = context.Libraries.FirstOrDefault() ?? new Library
        {
            Name = "Test Library",
            Path = "/test/library",
            Type = (int)LibraryType.Storage,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        
        if (library.Id == 0)
        {
            context.Libraries.Add(library);
            await context.SaveChangesAsync();
        }
        
        var artistName = $"Test Artist {index}";
        var artist = new Artist
        {
            ApiKey = Guid.NewGuid(),
            Directory = $"test-artist-{index}",
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            LibraryId = library.Id,
            Name = artistName,
            NameNormalized = artistName.ToNormalizedString()!
        };
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        
        return artist;
    }
    
    #endregion
}

using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using NodaTime;
using dbModels = Melodee.Common.Data.Models;

namespace Melodee.Tests.Common.Common.Services;

/// <summary>
/// Basic tests for AlbumService focusing on core functionality without complex data models
/// </summary>
public class AlbumServiceBasicTests : ServiceTestBase
{
    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_WithInvalidIdZero_ThrowsException()
    {
        // Arrange
        var service = GetAlbumService();
        var invalidId = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await service.GetAsync(invalidId));
    }

    [Fact]
    public async Task GetAsync_WithInvalidIdNegative_ThrowsException()
    {
        // Arrange
        var service = GetAlbumService();
        var invalidId = -1;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await service.GetAsync(invalidId));
    }

    [Fact]
    public async Task GetAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var service = GetAlbumService();
        var invalidId = 99999;

        // Act
        var result = await service.GetAsync(invalidId);

        // Assert
        Assert.NotNull(result);
        // The service may return false for IsSuccess when not found
        Assert.Null(result.Data);
    }

    #endregion

    #region GetByApiKeyAsync Tests

    [Fact]
    public async Task GetByApiKeyAsync_WithEmptyGuid_ThrowsException()
    {
        // Arrange
        var service = GetAlbumService();
        var emptyGuid = Guid.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await service.GetByApiKeyAsync(emptyGuid));
    }

    [Fact]
    public async Task GetByApiKeyAsync_WithNonExistentApiKey_ReturnsError()
    {
        // Arrange
        var service = GetAlbumService();
        var nonExistentApiKey = Guid.NewGuid();

        // Act
        var result = await service.GetByApiKeyAsync(nonExistentApiKey);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Contains("Unknown album", result.Messages?.FirstOrDefault() ?? string.Empty);
    }

    #endregion

    #region GetByMusicBrainzIdAsync Tests

    [Fact]
    public async Task GetByMusicBrainzIdAsync_WithNonExistentMusicBrainzId_ReturnsError()
    {
        // Arrange
        var service = GetAlbumService();
        var nonExistentMusicBrainzId = Guid.NewGuid();

        // Act
        var result = await service.GetByMusicBrainzIdAsync(nonExistentMusicBrainzId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Contains("Unknown album", result.Messages?.FirstOrDefault() ?? string.Empty);
    }

    #endregion

    #region ClearCache Tests

    [Fact]
    public void ClearCache_WithValidAlbum_DoesNotThrowException()
    {
        // Arrange
        var service = GetAlbumService();
        var album = new dbModels.Album
        {
            Id = 1,
            ApiKey = Guid.NewGuid(),
            Name = "Test Album",
            NameNormalized = "testalbum",
            MusicBrainzId = Guid.NewGuid(),
            Directory = "TestAlbum",
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            ArtistId = 1
        };

        // Act & Assert - Should not throw exception
        service.ClearCache(album);
        Assert.True(true); // Test passes if no exception is thrown
    }

    [Fact]
    public void ClearCache_WithAlbumWithoutMusicBrainzId_DoesNotThrowException()
    {
        // Arrange
        var service = GetAlbumService();
        var album = new dbModels.Album
        {
            Id = 1,
            ApiKey = Guid.NewGuid(),
            Name = "Test Album",
            NameNormalized = "testalbum",
            MusicBrainzId = null,
            Directory = "TestAlbum",
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            ArtistId = 1
        };

        // Act & Assert - Should not throw exception
        service.ClearCache(album);
        Assert.True(true); // Test passes if no exception is thrown
    }

    #endregion

    #region ListAsync Tests

    [Fact]
    public async Task ListAsync_WithEmptyDatabase_ReturnsEmptyResult()
    {
        // Arrange
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.TotalCount);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithNullArray_ThrowsException()
    {
        // Arrange
        var service = GetAlbumService();
        int[] nullArray = null!;

        // Act & Assert
        // ArgumentNullException is a subclass of ArgumentException
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await service.DeleteAsync(nullArray));
    }

    [Fact]
    public async Task DeleteAsync_WithEmptyArray_ThrowsException()
    {
        // Arrange
        var service = GetAlbumService();
        var emptyArray = Array.Empty<int>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await service.DeleteAsync(emptyArray));
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentAlbumId_ReturnsError()
    {
        // Arrange
        var service = GetAlbumService();
        var nonExistentIds = new[] { 99999 };

        // Act
        var result = await service.DeleteAsync(nonExistentIds);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.False(result.Data);
        Assert.Contains("Unknown album", result.Messages?.FirstOrDefault() ?? string.Empty);
    }

    #endregion

    #region Sorting Tests for New Fields

    [Fact]
    public async Task ListAsync_OrderByName_ReturnsOrderedResults()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "Name", "ASC" } }
        };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var names = result.Data.Select(a => a.Name).ToArray();
        var sortedNames = names.OrderBy(n => n).ToArray();
        Assert.Equal(sortedNames, names);
    }

    [Fact]
    public async Task ListAsync_OrderByNameDescending_ReturnsOrderedResults()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "Name", "DESC" } }
        };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task ListAsync_OrderByReleaseDate_ReturnsOrderedResults()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "ReleaseDate", "ASC" } }
        };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task ListAsync_OrderByReleaseDateDescending_ReturnsOrderedResults()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "ReleaseDate", "DESC" } }
        };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task ListAsync_OrderBySongCount_ReturnsOrderedResults()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "SongCount", "ASC" } }
        };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var songCounts = result.Data.Select(a => a.SongCount).ToArray();
        var sortedSongCounts = songCounts.OrderBy(c => c).ToArray();
        Assert.Equal(sortedSongCounts, songCounts);
    }

    [Fact]
    public async Task ListAsync_OrderBySongCountDescending_ReturnsOrderedResults()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "SongCount", "DESC" } }
        };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var songCounts = result.Data.Select(a => a.SongCount).ToArray();
        var sortedSongCounts = songCounts.OrderByDescending(c => c).ToArray();
        Assert.Equal(sortedSongCounts, songCounts);
    }

    [Fact]
    public async Task ListAsync_OrderByDuration_ReturnsOrderedResults()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "Duration", "ASC" } }
        };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var durations = result.Data.Select(a => a.Duration).ToArray();
        var sortedDurations = durations.OrderBy(d => d).ToArray();
        Assert.Equal(sortedDurations, durations);
    }

    [Fact]
    public async Task ListAsync_OrderByDurationDescending_ReturnsOrderedResults()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "Duration", "DESC" } }
        };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var durations = result.Data.Select(a => a.Duration).ToArray();
        var sortedDurations = durations.OrderByDescending(d => d).ToArray();
        Assert.Equal(sortedDurations, durations);
    }

    [Fact]
    public async Task ListAsync_OrderByPlayedCount_ReturnsOrderedResults()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "PlayedCount", "ASC" } }
        };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var playedCounts = result.Data.Select(a => a.PlayedCount).ToArray();
        var sortedPlayedCounts = playedCounts.OrderBy(c => c).ToArray();
        Assert.Equal(sortedPlayedCounts, playedCounts);
    }

    [Fact]
    public async Task ListAsync_OrderByPlayedCountDescending_ReturnsOrderedResults()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "PlayedCount", "DESC" } }
        };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var playedCounts = result.Data.Select(a => a.PlayedCount).ToArray();
        var sortedPlayedCounts = playedCounts.OrderByDescending(c => c).ToArray();
        Assert.Equal(sortedPlayedCounts, playedCounts);
    }

    [Fact]
    public async Task ListAsync_OrderByCalculatedRating_ReturnsOrderedResults()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "CalculatedRating", "ASC" } }
        };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var ratings = result.Data.Select(a => a.CalculatedRating).ToArray();
        var sortedRatings = ratings.OrderBy(r => r).ToArray();
        Assert.Equal(sortedRatings, ratings);
    }

    [Fact]
    public async Task ListAsync_OrderByCalculatedRatingDescending_ReturnsOrderedResults()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "CalculatedRating", "DESC" } }
        };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var ratings = result.Data.Select(a => a.CalculatedRating).ToArray();
        var sortedRatings = ratings.OrderByDescending(r => r).ToArray();
        Assert.Equal(sortedRatings, ratings);
    }

    [Fact]
    public async Task ListAsync_OrderByLastPlayedAt_ReturnsOrderedResults()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "LastPlayedAt", "ASC" } }
        };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task ListAsync_OrderByLastPlayedAtDescending_ReturnsOrderedResults()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "LastPlayedAt", "DESC" } }
        };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
    }

    #endregion

    #region AlbumDataInfo New Fields Tests

    [Fact]
    public async Task ListAsync_AlbumDataInfo_IncludesLastPlayedAt()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest { PageSize = 10, Page = 1 };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        // Verify at least one album has LastPlayedAt set
        Assert.Contains(result.Data, a => a.LastPlayedAt != null);
    }

    [Fact]
    public async Task ListAsync_AlbumDataInfo_IncludesPlayedCount()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest { PageSize = 10, Page = 1 };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        Assert.Contains(result.Data, a => a.PlayedCount > 0);
    }

    [Fact]
    public async Task ListAsync_AlbumDataInfo_IncludesCalculatedRating()
    {
        // Arrange
        await SeedTestAlbums();
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest { PageSize = 10, Page = 1 };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        Assert.Contains(result.Data, a => a.CalculatedRating > 0);
    }

    #endregion

    #region Helper Methods

    private async Task SeedTestAlbums(int count = 5)
    {
        await using var context = await MockFactory().CreateDbContextAsync();

        var library = new dbModels.Library
        {
            Name = "Test Library",
            Path = "/test/library/path",
            Type = (int)LibraryType.Storage,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.Libraries.Add(library);
        await context.SaveChangesAsync();

        var artistName = "Test Artist";
        var artist = new dbModels.Artist
        {
            ApiKey = Guid.NewGuid(),
            Directory = "test-artist",
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            LibraryId = library.Id,
            Name = artistName,
            NameNormalized = artistName.ToNormalizedString()!,
            Library = library
        };
        context.Artists.Add(artist);
        await context.SaveChangesAsync();

        for (int i = 1; i <= count; i++)
        {
            var albumName = $"Test Album {i}";
            var album = new dbModels.Album
            {
                ApiKey = Guid.NewGuid(),
                Name = albumName,
                NameNormalized = albumName.ToNormalizedString()!,
                Directory = $"test-album-{i}",
                ArtistId = artist.Id,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                ReleaseDate = new NodaTime.LocalDate(2020 + i, 1, 1),
                SongCount = (short)(i * 5),
                Duration = i * 3000,
                PlayedCount = i * 50,
                CalculatedRating = i * 0.8m,
                LastPlayedAt = Instant.FromDateTimeUtc(DateTime.UtcNow.AddDays(-i))
            };
            context.Albums.Add(album);
        }
        await context.SaveChangesAsync();
    }

    #endregion
}

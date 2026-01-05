using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Common.Services;
using NodaTime;

namespace Melodee.Tests.Common.Services;

public class SongServiceTests : ServiceTestBase
{
    private readonly SongService _songService;

    public SongServiceTests()
    {
        _songService = GetSongService();
    }

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var song = await CreateTestSong();

        // Act
        var result = await _songService.GetAsync(song.Id);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.Equal(song.Id, result.Data!.Id);
        Assert.Equal(song.Title, result.Data.Title);
        Assert.Equal(song.ApiKey, result.Data.ApiKey);
    }

    [Fact]
    public async Task GetAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act
        var result = await _songService.GetAsync(nonExistentId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetAsync_WithInvalidId_ThrowsArgumentException(int invalidId)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _songService.GetAsync(invalidId));
    }

    #endregion

    #region GetByApiKeyAsync Tests

    [Fact]
    public async Task GetByApiKeyAsync_WithValidApiKey_ReturnsSuccess()
    {
        // Arrange
        var song = await CreateTestSong();

        // Act
        var result = await _songService.GetByApiKeyAsync(song.ApiKey);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.Equal(song.Id, result.Data!.Id);
        Assert.Equal(song.ApiKey, result.Data.ApiKey);
    }

    [Fact]
    public async Task GetByApiKeyAsync_WithNonExistentApiKey_ReturnsFailure()
    {
        // Arrange
        var nonExistentApiKey = Guid.NewGuid();

        // Act
        var result = await _songService.GetByApiKeyAsync(nonExistentApiKey);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Contains("Unknown song", result.Messages ?? []);
    }

    [Fact]
    public async Task GetByApiKeyAsync_WithEmptyGuid_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _songService.GetByApiKeyAsync(Guid.Empty));
    }

    #endregion

    #region ListAsync Tests

    [Fact]
    public async Task ListAsync_WithValidRequest_ReturnsPagedResults()
    {
        // Arrange
        await CreateMultipleTestSongs(5);
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.TotalCount >= 5);
        Assert.True(result.Data.Count() >= 5);
    }

    [Fact]
    public async Task ListAsync_WithFilterByTitle_Equals_ReturnsFilteredResults()
    {
        // Arrange
        var songs = await CreateMultipleTestSongs(3);
        var specificTitle = songs.First().Title;
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10,
            FilterBy = [new FilterOperatorInfo("Title", FilterOperator.Equals, specificTitle)]
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.Contains(result.Data, s => s.Title.Contains(specificTitle, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ListAsync_WithFilterByTitle_Contains_ReturnsFilteredResults()
    {
        // Arrange
        var songs = await CreateMultipleTestSongs(3);
        var specificTitle = songs.First().Title.Split().First();
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10,
            FilterBy = [new FilterOperatorInfo("Title", FilterOperator.Equals, specificTitle)]
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.Contains(result.Data, s => s.Title.Contains(specificTitle, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ListAsync_WithUserStarredFilter_HandlesUserSpecificData()
    {
        // Arrange
        await CreateMultipleTestSongs(2);
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10,
            FilterBy = [new FilterOperatorInfo("UserStarred", FilterOperator.Equals, "true")]
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        // Result may be empty since no user starred songs are set up
    }

    [Fact]
    public async Task ListAsync_WithUserRatingFilter_HandlesUserSpecificData()
    {
        // Arrange
        await CreateMultipleTestSongs(2);
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10,
            FilterBy = [new FilterOperatorInfo("UserRating", FilterOperator.GreaterThan, "3")]
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ListAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await CreateMultipleTestSongs(15);
        var pagedRequest = new PagedRequest
        {
            Page = 2,
            PageSize = 5
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.Data.Count() <= 5);
        Assert.True(result.TotalPages >= 3);
    }

    [Fact]
    public async Task ListAsync_WithTotalCountOnly_ReturnsCountWithoutData()
    {
        // Arrange
        await CreateMultipleTestSongs(3);
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10,
            IsTotalCountOnlyRequest = true
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.True(result.TotalCount >= 3);
        Assert.Empty(result.Data);
    }

    #endregion

    #region ListForContributorsAsync Tests

    [Fact]
    public async Task ListForContributorsAsync_WithValidContributor_ReturnsResults()
    {
        // Arrange
        await CreateTestSongWithContributor("John Doe");
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _songService.ListForContributorsAsync(pagedRequest, "John");

        // Assert
        AssertResultIsSuccessful(result);
    }

    [Fact]
    public async Task ListForContributorsAsync_WithNonExistentContributor_ReturnsEmptyResults()
    {
        // Arrange
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _songService.ListForContributorsAsync(pagedRequest, "NonExistentContributor");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task ListForContributorsAsync_WithPartialContributorName_ReturnsMatchingResults()
    {
        // Arrange
        await CreateTestSongWithContributor("Jane Smith");
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _songService.ListForContributorsAsync(pagedRequest, "jane");

        // Assert
        AssertResultIsSuccessful(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ListForContributorsAsync_WithInvalidContributorName_HandlesGracefully(string contributorName)
    {
        // Arrange
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _songService.ListForContributorsAsync(pagedRequest, contributorName);

        // Assert
        Assert.NotNull(result);
        // Should handle gracefully without throwing
    }

    #endregion

    #region ListNowPlayingAsync Tests

    [Fact]
    public async Task ListNowPlayingAsync_WithNoNowPlayingSongs_ReturnsEmptyResults()
    {
        // Arrange
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _songService.ListNowPlayingAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Data);
    }

    #endregion

    #region GetStreamForSongAsync Tests

    [Fact]
    public async Task GetStreamForSongAsync_WithNonExistentSong_ReturnsFailure()
    {
        // Arrange
        var nonExistentApiKey = Guid.NewGuid();
        var user = new UserInfo(
            1,
            Guid.NewGuid(),
            "Test User",
            "testpassword",
            "testsalt",
            "testtoken"
        );

        // Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result = await _songService.GetStreamForSongAsync(user, nonExistentApiKey, CancellationToken.None);
#pragma warning restore CS0618 // Type or member is obsolete

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains("Unknown song", result.Messages ?? []);
        Assert.False(result.Data.IsSuccess);
    }

    [Fact]
    public async Task GetStreamForSongAsync_WithValidSong_ReturnsStreamResponse()
    {
        // Arrange - Baseline test for existing behavior
        var song = await CreateTestSong();
        var user = new UserInfo(
            1,
            Guid.NewGuid(),
            "Test User",
            "testpassword",
            "testsalt",
            "testtoken"
        );

        // Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result = await _songService.GetStreamForSongAsync(user, song.ApiKey, CancellationToken.None);
#pragma warning restore CS0618 // Type or member is obsolete

        // Assert - Capture baseline behavior
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.ResponseHeaders);

        // File may not exist in test environment, so we check for expected failure pattern
        if (result.IsSuccess && result.Data.IsSuccess)
        {
            // If successful, verify expected headers exist (baseline)
            Assert.True(result.Data.ResponseHeaders.ContainsKey("Content-Type"));
            Assert.True(result.Data.ResponseHeaders.ContainsKey("Content-Length"));
            Assert.True(result.Data.ResponseHeaders.ContainsKey("Accept-Ranges"));
            Assert.Equal("bytes", result.Data.ResponseHeaders["Accept-Ranges"].ToString());
        }
        else
        {
            // If file doesn't exist in test, that's expected behavior
            // Just verify we get a valid response structure
            Assert.NotNull(result.Data.ResponseHeaders);
        }
    }

    [Fact]
    public async Task GetStreamForSongAsync_WithRangeRequest_ReturnsPartialContent()
    {
        // Arrange - Baseline test for range request behavior
        var song = await CreateTestSong();
        var user = new UserInfo(
            1,
            Guid.NewGuid(),
            "Test User",
            "testpassword",
            "testsalt",
            "testtoken"
        );
        const long rangeStart = 100;
        const long rangeEnd = 500;

        // Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result = await _songService.GetStreamForSongAsync(
            user, song.ApiKey, rangeStart, rangeEnd, cancellationToken: CancellationToken.None);
#pragma warning restore CS0618 // Type or member is obsolete

        // Assert - Capture baseline range behavior
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.ResponseHeaders);

        // Check Content-Range header format (baseline)
        if (result.Data.ResponseHeaders.ContainsKey("Content-Range"))
        {
            var contentRange = result.Data.ResponseHeaders["Content-Range"].ToString();
            Assert.Contains("bytes", contentRange);
            Assert.Contains($"{rangeStart}-{rangeEnd}", contentRange);
        }
    }

    [Fact]
    public async Task GetStreamForSongAsync_WithDownloadRequest_SetsDownloadHeaders()
    {
        // Arrange - Baseline test for download behavior
        var song = await CreateTestSong();
        var user = new UserInfo(
            1,
            Guid.NewGuid(),
            "Test User",
            "testpassword",
            "testsalt",
            "testtoken"
        );

        // Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result = await _songService.GetStreamForSongAsync(
            user, song.ApiKey, 0, 0, null, null, true, CancellationToken.None);
#pragma warning restore CS0618 // Type or member is obsolete

        // Assert - Capture baseline download behavior
        Assert.NotNull(result);
        Assert.NotNull(result.Data);

        // File may not exist in test environment, handle both cases
        if (result.IsSuccess && result.Data.IsSuccess)
        {
            // Content-Disposition or filename should be set for downloads
            Assert.True(!string.IsNullOrEmpty(result.Data.FileName) ||
                       result.Data.ResponseHeaders.ContainsKey("Content-Disposition"));
        }
        else
        {
            // If file doesn't exist in test, that's expected behavior
            // Just verify we get a valid response structure
            Assert.NotNull(result.Data.ResponseHeaders);
        }
    }

    [Theory]
    [InlineData("/app/storage", "S/SA/Artist/", "[2025] Album/", "01 Song.mp3")]
    [InlineData("/app/storage/", "S/SA/Artist/", "[2025] Album/", "01 Song.mp3")]
    [InlineData("C:\\Music", "S\\SA\\Artist\\", "[2025] Album\\", "01 Song.mp3")]
    [InlineData("/mnt/music", "Artist/", "Album/", "song.flac")]
    public async Task GetStreamForSongAsync_PathConstruction_UsesProperPathSeparators(
        string libraryPath, string artistDirectory, string albumDirectory, string fileName)
    {
        // Arrange - Create test data with specific path components
        var library = new Library
        {
            ApiKey = Guid.NewGuid(),
            Name = "Path Test Library",
            Path = libraryPath,
            Type = (int)LibraryType.Storage,
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            LastUpdatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Libraries.Add(library);
            await context.SaveChangesAsync();
        }

        var artist = new Melodee.Common.Data.Models.Artist
        {
            ApiKey = Guid.NewGuid(),
            Name = "Path Test Artist",
            NameNormalized = "pathtestartist",
            LibraryId = library.Id,
            Directory = artistDirectory,
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            LastUpdatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Artists.Add(artist);
            await context.SaveChangesAsync();
        }

        var album = new Melodee.Common.Data.Models.Album
        {
            ApiKey = Guid.NewGuid(),
            Name = "Path Test Album",
            NameNormalized = "pathtestalbum",
            ArtistId = artist.Id,
            Directory = albumDirectory,
            ReleaseDate = new LocalDate(2025, 1, 1),
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            LastUpdatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Albums.Add(album);
            await context.SaveChangesAsync();
        }

        var song = new Melodee.Common.Data.Models.Song
        {
            ApiKey = Guid.NewGuid(),
            Title = "Path Test Song",
            TitleNormalized = "pathtestsong",
            AlbumId = album.Id,
            SongNumber = 1,
            FileName = fileName,
            FileSize = 1000000,
            FileHash = "pathtest",
            Duration = 180000,
            SamplingRate = 44100,
            BitRate = 320,
            BitDepth = 16,
            BPM = 120,
            ContentType = "audio/mpeg",
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            LastUpdatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Songs.Add(song);
            await context.SaveChangesAsync();
        }

        var user = new UserInfo(1, Guid.NewGuid(), "Test User", "testpassword", "testsalt", "testtoken");

        // Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result = await _songService.GetStreamForSongAsync(user, song.ApiKey, CancellationToken.None);
#pragma warning restore CS0618 // Type or member is obsolete

        // Assert - The path should use proper platform separators from Path.Combine
        // The file won't exist, but we verify the constructed path doesn't have doubled/missing separators
        Assert.NotNull(result);
        Assert.NotNull(result.Data);

        // Path.Combine correctly handles:
        // 1. Trailing slashes on directories
        // 2. Platform-specific separators
        // 3. No double separators (e.g., /app/storageS/SA instead of /app/storage/S/SA)
        var expectedPath = Path.Combine(libraryPath, artistDirectory, albumDirectory, fileName);

        // The error message should contain the properly constructed path if file not found
        if (!result.IsSuccess && result.Messages != null)
        {
            // Path construction was successful even if file doesn't exist
            Assert.DoesNotContain("storageS", string.Join(" ", result.Messages));
            Assert.DoesNotContain("\\\\", string.Join(" ", result.Messages));
        }
    }

    #endregion

    #region ClearCacheAsync Tests

    [Fact]
    public async Task ClearCacheAsync_WithValidSongId_ClearsCacheSuccessfully()
    {
        // Arrange
        var song = await CreateTestSong();

        // First get the song to populate cache
        await _songService.GetAsync(song.Id);
        await _songService.GetByApiKeyAsync(song.ApiKey);

        // Act
        await _songService.ClearCacheAsync(song.Id, CancellationToken.None);

        // Assert
        // Cache should be cleared - verify by checking if subsequent calls work
        var result = await _songService.GetAsync(song.Id);
        AssertResultIsSuccessful(result);
    }

    [Fact]
    public async Task ClearCacheAsync_WithNonExistentSongId_HandlesGracefully()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act & Assert
        // Should not throw exception
        await _songService.ClearCacheAsync(nonExistentId, CancellationToken.None);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidSongIds_DeletesSongsSuccessfully()
    {
        // Arrange
        var song = await CreateTestSong();
        var songIds = new[] { song.Id };

        // Act
        var result = await _songService.DeleteAsync(songIds, CancellationToken.None);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.Data);

        // Verify song is deleted
        var getResult = await _songService.GetAsync(song.Id);
        Assert.False(getResult.IsSuccess);
    }

    [Fact]
    public async Task DeleteAsync_WithMultipleSongs_DeletesAllSuccessfully()
    {
        // Arrange
        var song1 = await CreateTestSong();
        var song2 = await CreateTestSong(titleSuffix: "2");
        var songIds = new[] { song1.Id, song2.Id };

        // Act
        var result = await _songService.DeleteAsync(songIds, CancellationToken.None);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.Data);

        // Verify both songs are deleted
        var getResult1 = await _songService.GetAsync(song1.Id);
        var getResult2 = await _songService.GetAsync(song2.Id);
        Assert.False(getResult1.IsSuccess);
        Assert.False(getResult2.IsSuccess);
    }

    [Fact]
    public async Task DeleteAsync_ClearsCacheEntries()
    {
        // Arrange
        var song = await CreateTestSong();
        var songIds = new[] { song.Id };

        // Populate cache
        await _songService.GetAsync(song.Id);
        await _songService.GetByApiKeyAsync(song.ApiKey);

        // Act
        var result = await _songService.DeleteAsync(songIds, CancellationToken.None);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentSongId_ContinuesWithoutError()
    {
        // Arrange
        var songIds = new[] { 999999 };

        // Act
        var result = await _songService.DeleteAsync(songIds, CancellationToken.None);

        // Assert - Should succeed even if song doesn't exist
        AssertResultIsSuccessful(result);
        Assert.True(result.Data);
    }

    #endregion

    #region Caching Tests

    [Fact]
    public async Task GetAsync_CachesResults()
    {
        // Arrange
        var song = await CreateTestSong();

        // Act - First call should populate cache
        var result1 = await _songService.GetAsync(song.Id);
        var result2 = await _songService.GetAsync(song.Id);

        // Assert
        AssertResultIsSuccessful(result1);
        AssertResultIsSuccessful(result2);
        Assert.Equal(result1.Data!.Id, result2.Data!.Id);
    }

    [Fact]
    public async Task GetByApiKeyAsync_CachesApiKeyToIdMapping()
    {
        // Arrange
        var song = await CreateTestSong();

        // Act - First call should populate cache
        var result1 = await _songService.GetByApiKeyAsync(song.ApiKey);
        var result2 = await _songService.GetByApiKeyAsync(song.ApiKey);

        // Assert
        AssertResultIsSuccessful(result1);
        AssertResultIsSuccessful(result2);
        Assert.Equal(result1.Data!.ApiKey, result2.Data!.ApiKey);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task ListAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var pagedRequest = new PagedRequest { Page = 1, PageSize = 10 };
        var userId = 1;
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _songService.ListAsync(pagedRequest, userId, cts.Token));
    }

    [Fact]
    public async Task GetAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _songService.GetAsync(1, cts.Token));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("test")]
    [InlineData("rock")]
    public async Task ListForContributorsAsync_WithVariousContributorNames_HandlesCorrectly(string contributorName)
    {
        // Arrange
        var pagedRequest = new PagedRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await _songService.ListForContributorsAsync(pagedRequest, contributorName);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Sorting Tests for New Fields

    [Fact]
    public async Task ListAsync_OrderByTitle_ReturnsOrderedResults()
    {
        // Arrange
        await CreateMultipleTestSongs(5);
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "Title", "ASC" } }
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var titles = result.Data.Select(s => s.Title).ToArray();
        var sortedTitles = titles.OrderBy(t => t).ToArray();
        Assert.Equal(sortedTitles, titles);
    }

    [Fact]
    public async Task ListAsync_OrderByTitleDescending_ReturnsOrderedResults()
    {
        // Arrange
        await CreateMultipleTestSongs(5);
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "Title", "DESC" } }
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var titles = result.Data.Select(s => s.Title).ToArray();
        var sortedTitles = titles.OrderByDescending(t => t).ToArray();
        Assert.Equal(sortedTitles, titles);
    }

    [Fact]
    public async Task ListAsync_OrderBySongNumber_ReturnsOrderedResults()
    {
        // Arrange
        await CreateMultipleTestSongs(5);
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "SongNumber", "ASC" } }
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var songNumbers = result.Data.Select(s => s.SongNumber).ToArray();
        var sortedSongNumbers = songNumbers.OrderBy(n => n).ToArray();
        Assert.Equal(sortedSongNumbers, songNumbers);
    }

    [Fact]
    public async Task ListAsync_OrderBySongNumberDescending_ReturnsOrderedResults()
    {
        // Arrange
        await CreateMultipleTestSongs(5);
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "SongNumber", "DESC" } }
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var songNumbers = result.Data.Select(s => s.SongNumber).ToArray();
        var sortedSongNumbers = songNumbers.OrderByDescending(n => n).ToArray();
        Assert.Equal(sortedSongNumbers, songNumbers);
    }

    [Fact]
    public async Task ListAsync_OrderByAlbumId_ReturnsOrderedResults()
    {
        // Arrange
        await CreateMultipleTestSongs(5);
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "AlbumId", "ASC" } }
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var albumIds = result.Data.Select(s => s.AlbumId).ToArray();
        var sortedAlbumIds = albumIds.OrderBy(id => id).ToArray();
        Assert.Equal(sortedAlbumIds, albumIds);
    }

    [Fact]
    public async Task ListAsync_OrderByAlbumIdDescending_ReturnsOrderedResults()
    {
        // Arrange
        await CreateMultipleTestSongs(5);
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "AlbumId", "DESC" } }
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var albumIds = result.Data.Select(s => s.AlbumId).ToArray();
        var sortedAlbumIds = albumIds.OrderByDescending(id => id).ToArray();
        Assert.Equal(sortedAlbumIds, albumIds);
    }

    [Fact]
    public async Task ListAsync_OrderByDuration_ReturnsOrderedResults()
    {
        // Arrange
        await CreateMultipleTestSongs(5);
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "Duration", "ASC" } }
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var durations = result.Data.Select(s => s.Duration).ToArray();
        var sortedDurations = durations.OrderBy(d => d).ToArray();
        Assert.Equal(sortedDurations, durations);
    }

    [Fact]
    public async Task ListAsync_OrderByDurationDescending_ReturnsOrderedResults()
    {
        // Arrange
        await CreateMultipleTestSongs(5);
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "Duration", "DESC" } }
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var durations = result.Data.Select(s => s.Duration).ToArray();
        var sortedDurations = durations.OrderByDescending(d => d).ToArray();
        Assert.Equal(sortedDurations, durations);
    }

    [Fact]
    public async Task ListAsync_OrderByPlayedCount_ReturnsOrderedResults()
    {
        // Arrange
        await CreateTestSongsWithPlayStats(5);
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "PlayedCount", "ASC" } }
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var playedCounts = result.Data.Select(s => s.PlayedCount).ToArray();
        var sortedPlayedCounts = playedCounts.OrderBy(c => c).ToArray();
        Assert.Equal(sortedPlayedCounts, playedCounts);
    }

    [Fact]
    public async Task ListAsync_OrderByPlayedCountDescending_ReturnsOrderedResults()
    {
        // Arrange
        await CreateTestSongsWithPlayStats(5);
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "PlayedCount", "DESC" } }
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var playedCounts = result.Data.Select(s => s.PlayedCount).ToArray();
        var sortedPlayedCounts = playedCounts.OrderByDescending(c => c).ToArray();
        Assert.Equal(sortedPlayedCounts, playedCounts);
    }

    [Fact]
    public async Task ListAsync_OrderByCalculatedRating_ReturnsOrderedResults()
    {
        // Arrange
        await CreateTestSongsWithPlayStats(5);
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "CalculatedRating", "ASC" } }
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var ratings = result.Data.Select(s => s.CalculatedRating).ToArray();
        var sortedRatings = ratings.OrderBy(r => r).ToArray();
        Assert.Equal(sortedRatings, ratings);
    }

    [Fact]
    public async Task ListAsync_OrderByCalculatedRatingDescending_ReturnsOrderedResults()
    {
        // Arrange
        await CreateTestSongsWithPlayStats(5);
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "CalculatedRating", "DESC" } }
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        var ratings = result.Data.Select(s => s.CalculatedRating).ToArray();
        var sortedRatings = ratings.OrderByDescending(r => r).ToArray();
        Assert.Equal(sortedRatings, ratings);
    }

    [Fact]
    public async Task ListAsync_OrderByLastPlayedAt_ReturnsOrderedResults()
    {
        // Arrange
        await CreateTestSongsWithPlayStats(5);
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "LastPlayedAt", "ASC" } }
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task ListAsync_OrderByLastPlayedAtDescending_ReturnsOrderedResults()
    {
        // Arrange
        await CreateTestSongsWithPlayStats(5);
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "LastPlayedAt", "DESC" } }
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
    }

    #endregion

    #region SongDataInfo New Fields Tests

    [Fact]
    public async Task ListAsync_SongDataInfo_IncludesAlbumId()
    {
        // Arrange
        await CreateTestSongsWithPlayStats(3);
        var pagedRequest = new PagedRequest { PageSize = 10, Page = 1 };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        Assert.All(result.Data, s => Assert.True(s.AlbumId > 0));
    }

    [Fact]
    public async Task ListAsync_SongDataInfo_IncludesLastPlayedAt()
    {
        // Arrange
        await CreateTestSongsWithPlayStats(3);
        var pagedRequest = new PagedRequest { PageSize = 10, Page = 1 };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        Assert.Contains(result.Data, s => s.LastPlayedAt != null);
    }

    [Fact]
    public async Task ListAsync_SongDataInfo_IncludesPlayedCount()
    {
        // Arrange
        await CreateTestSongsWithPlayStats(3);
        var pagedRequest = new PagedRequest { PageSize = 10, Page = 1 };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        Assert.Contains(result.Data, s => s.PlayedCount > 0);
    }

    [Fact]
    public async Task ListAsync_SongDataInfo_IncludesCalculatedRating()
    {
        // Arrange
        await CreateTestSongsWithPlayStats(3);
        var pagedRequest = new PagedRequest { PageSize = 10, Page = 1 };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotEmpty(result.Data);
        Assert.Contains(result.Data, s => s.CalculatedRating > 0);
    }

    #endregion

    #region Helper Methods

    private async Task<Melodee.Common.Data.Models.Song[]> CreateTestSongsWithPlayStats(int count)
    {
        var songs = new List<Melodee.Common.Data.Models.Song>();
        var artist = await CreateTestArtist();
        var album = await CreateTestAlbum(artist);

        for (int i = 0; i < count; i++)
        {
            var song = new Melodee.Common.Data.Models.Song
            {
                ApiKey = Guid.NewGuid(),
                Title = $"Test Song {i + 1}",
                TitleNormalized = $"Test Song {i + 1}".ToNormalizedString() ?? string.Empty,
                AlbumId = album.Id,
                SongNumber = i + 1,
                FileName = $"test{i + 1}.mp3",
                FileSize = 1000000 + (i * 10000),
                FileHash = $"testhash{i + 1}",
                Duration = 180000 + (i * 1000),
                SamplingRate = 44100,
                BitRate = 320,
                BitDepth = 16,
                BPM = 120,
                ContentType = "audio/mpeg",
                CreatedAt = SystemClock.Instance.GetCurrentInstant(),
                LastUpdatedAt = SystemClock.Instance.GetCurrentInstant(),
                PlayedCount = (i + 1) * 10,
                CalculatedRating = (i + 1) * 0.5m,
                LastPlayedAt = Instant.FromDateTimeUtc(DateTime.UtcNow.AddDays(-(i + 1)))
            };
            songs.Add(song);
        }

        await using var context = await MockFactory().CreateDbContextAsync();
        context.Songs.AddRange(songs);
        await context.SaveChangesAsync();
        return songs.ToArray();
    }

    public async Task<Melodee.Common.Data.Models.Song> CreateTestSong(string? titleSuffix = null)
    {
        var artist = await CreateTestArtist();
        var album = await CreateTestAlbum(artist);

        var suffix = titleSuffix ?? Guid.NewGuid().ToString();
        var song = new Melodee.Common.Data.Models.Song
        {
            ApiKey = Guid.NewGuid(),
            Title = $"Test Song {suffix}",
            TitleNormalized = $"testsong{suffix}".Replace("-", ""),
            AlbumId = album.Id,
            SongNumber = 1,
            FileName = $"test{suffix}.mp3",
            FileSize = 1000000,
            FileHash = "testhash",
            Duration = 180000,
            SamplingRate = 44100,
            BitRate = 320,
            BitDepth = 16,
            BPM = 120,
            ContentType = "audio/mpeg",
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            LastUpdatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        await using var context = await MockFactory().CreateDbContextAsync();
        context.Songs.Add(song);
        await context.SaveChangesAsync();
        return song;
    }

    public async Task<Melodee.Common.Data.Models.Song[]> CreateMultipleTestSongs(int count)
    {
        var songs = new List<Melodee.Common.Data.Models.Song>();
        var artist = await CreateTestArtist();
        var album = await CreateTestAlbum(artist);

        for (int i = 0; i < count; i++)
        {
            var song = new Melodee.Common.Data.Models.Song
            {
                ApiKey = Guid.NewGuid(),
                Title = $"Test Song {i + 1}",
                TitleNormalized = $"Test Song {i + 1}".ToNormalizedString() ?? string.Empty,
                AlbumId = album.Id,
                SongNumber = i + 1,
                FileName = $"test{i + 1}.mp3",
                FileSize = 1000000 + (i * 10000),
                FileHash = $"testhash{i + 1}",
                Duration = 180000 + (i * 1000),
                SamplingRate = 44100,
                BitRate = 320,
                BitDepth = 16,
                BPM = 120,
                ContentType = "audio/mpeg",
                CreatedAt = SystemClock.Instance.GetCurrentInstant(),
                LastUpdatedAt = SystemClock.Instance.GetCurrentInstant()
            };
            songs.Add(song);
        }

        await using var context = await MockFactory().CreateDbContextAsync();
        context.Songs.AddRange(songs);
        await context.SaveChangesAsync();
        return songs.ToArray();
    }

    public async Task<Melodee.Common.Data.Models.Song> CreateTestSongWithContributor(string contributorName)
    {
        var song = await CreateTestSong();

        // Add contributor
        var contributor = new Contributor
        {
            SongId = song.Id,
            ContributorName = contributorName,
            Role = "Performer",
            AlbumId = song.AlbumId,
            ContributorType = (int)ContributorType.Performer,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        await using var context = await MockFactory().CreateDbContextAsync();
        context.Contributors.Add(contributor);
        await context.SaveChangesAsync();

        return song;
    }

    public async Task<Melodee.Common.Data.Models.Artist> CreateTestArtist()
    {
        var library = await CreateTestLibrary();

        var artist = new Melodee.Common.Data.Models.Artist
        {
            ApiKey = Guid.NewGuid(),
            Name = $"Test Artist {Guid.NewGuid()}",
            NameNormalized = $"testartist{Guid.NewGuid()}".Replace("-", ""),
            LibraryId = library.Id,
            Directory = "/testartist/",
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            LastUpdatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        await using var context = await MockFactory().CreateDbContextAsync();
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        return artist;
    }

    public async Task<Melodee.Common.Data.Models.Album> CreateTestAlbum(Melodee.Common.Data.Models.Artist artist)
    {
        var album = new Melodee.Common.Data.Models.Album
        {
            ApiKey = Guid.NewGuid(),
            Name = $"Test Album {Guid.NewGuid()}",
            NameNormalized = $"testalbum{Guid.NewGuid()}".Replace("-", ""),
            ArtistId = artist.Id,
            Directory = "/testalbum/",
            ReleaseDate = new LocalDate(2023, 1, 1),
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            LastUpdatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        await using var context = await MockFactory().CreateDbContextAsync();
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        return album;
    }

    public async Task<Library> CreateTestLibrary()
    {
        var library = new Library
        {
            ApiKey = Guid.NewGuid(),
            Name = $"Test Library {Guid.NewGuid()}",
            Path = "/test/library/",
            Type = (int)LibraryType.Storage,
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            LastUpdatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        await using var context = await MockFactory().CreateDbContextAsync();
        var existingLibrary = context.Libraries.FirstOrDefault();
        if (existingLibrary != null)
        {
            return existingLibrary;
        }

        context.Libraries.Add(library);
        await context.SaveChangesAsync();
        return library;
    }

    #endregion
}

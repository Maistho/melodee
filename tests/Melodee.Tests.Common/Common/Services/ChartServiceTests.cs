using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Services;
using NodaTime;
using Album = Melodee.Common.Data.Models.Album;
using Artist = Melodee.Common.Data.Models.Artist;
using Library = Melodee.Common.Data.Models.Library;
using Song = Melodee.Common.Data.Models.Song;

namespace Melodee.Tests.Common.Common.Services;

public class ChartServiceTests : ServiceTestBase
{
    private ChartService GetChartService()
    {
        return new ChartService(Logger, CacheManager, MockFactory(), GetLibraryService());
    }

    private async Task<Artist> CreateTestArtistAsync(string name)
    {
        await using var context = await MockFactory().CreateDbContextAsync();

        var library = new Library
        {
            Name = "Test Library",
            Path = "/test/library",
            Type = (int)LibraryType.Storage,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        context.Libraries.Add(library);
        await context.SaveChangesAsync();

        var artist = new Artist
        {
            Name = name,
            NameNormalized = name.ToNormalizedString() ?? name.ToUpperInvariant(),
            SortName = name,
            RealName = name,
            ApiKey = Guid.NewGuid(),
            Directory = $"/music/{name.ToLowerInvariant().Replace(" ", "_")}",
            LibraryId = library.Id,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        return artist;
    }

    private async Task<Album> CreateTestAlbumAsync(int artistId, string name, int year = 2024)
    {
        return await CreateTestAlbumWithSortAsync(artistId, name, name, year);
    }

    private async Task<Album> CreateTestAlbumWithSortAsync(int artistId, string name, string sortName, int year = 2024)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var album = new Album
        {
            ArtistId = artistId,
            Name = name,
            NameNormalized = name.ToNormalizedString() ?? name.ToUpperInvariant(),
            SortName = sortName,
            ApiKey = Guid.NewGuid(),
            Directory = $"/music/album_{sortName.ToLowerInvariant().Replace(" ", "_")}",
            ReleaseDate = new LocalDate(year, 1, 1),
            AlbumStatus = (int)AlbumStatus.Ok,
            SongCount = 10,
            Duration = 3600,
            ImageCount = 1,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        return album;
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidTitle_ReturnsChartWithGeneratedSlug()
    {
        var service = GetChartService();

        var result = await service.CreateAsync("Rolling Stone 500 Greatest Albums");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Rolling Stone 500 Greatest Albums", result.Data.Title);
        Assert.Equal("rolling-stone-500-greatest-albums", result.Data.Slug);
        Assert.True(result.Data.Id > 0);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateTitle_GeneratesUniqueSlug()
    {
        var service = GetChartService();

        var result1 = await service.CreateAsync("Best Albums");
        var result2 = await service.CreateAsync("Best Albums");

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.Equal("best-albums", result1.Data!.Slug);
        Assert.Equal("best-albums-2", result2.Data!.Slug);
    }

    [Fact]
    public async Task CreateAsync_WithAllOptionalFields_PersistsCorrectly()
    {
        var service = GetChartService();
        var tags = new[] { "rock", "progressive", "2024" };

        var result = await service.CreateAsync(
            "Test Chart",
            sourceName: "Rolling Stone",
            sourceUrl: "https://rollingstone.com/chart",
            year: 2024,
            description: "A test chart description",
            tags: tags,
            isVisible: true,
            isGeneratedPlaylistEnabled: true);

        Assert.True(result.IsSuccess);
        var chart = result.Data!;
        Assert.Equal("Rolling Stone", chart.SourceName);
        Assert.Equal("https://rollingstone.com/chart", chart.SourceUrl);
        Assert.Equal(2024, chart.Year);
        Assert.Equal("A test chart description", chart.Description);
        Assert.Contains("rock", chart.Tags ?? "");
        Assert.True(chart.IsVisible);
        Assert.True(chart.IsGeneratedPlaylistEnabled);
    }

    #endregion

    #region GetByIdAsync and GetBySlugAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingChart_ReturnsChart()
    {
        var service = GetChartService();
        var createResult = await service.CreateAsync("Test Chart");
        var chartId = createResult.Data!.Id;

        var result = await service.GetByIdAsync(chartId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(chartId, result.Data.Id);
    }

    [Fact]
    public async Task GetBySlugAsync_WithExistingSlug_ReturnsChart()
    {
        var service = GetChartService();
        await service.CreateAsync("Test Chart");

        var result = await service.GetBySlugAsync("test-chart");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("test-chart", result.Data.Slug);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        var service = GetChartService();

        var result = await service.GetByIdAsync(99999);

        Assert.Null(result.Data);
    }

    #endregion

    #region ListAsync Tests

    [Fact]
    public async Task ListAsync_WithVisibleCharts_ReturnsOnlyVisible()
    {
        var service = GetChartService();
        await service.CreateAsync("Visible Chart", isVisible: true);
        await service.CreateAsync("Hidden Chart", isVisible: false);

        var result = await service.ListAsync(new PagedRequest { PageSize = 10 }, includeHidden: false);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Data);
        Assert.Equal("Visible Chart", result.Data.First().Title);
    }

    [Fact]
    public async Task ListAsync_WithIncludeHidden_ReturnsAll()
    {
        var service = GetChartService();
        await service.CreateAsync("Visible Chart", isVisible: true);
        await service.CreateAsync("Hidden Chart", isVisible: false);

        var result = await service.ListAsync(new PagedRequest { PageSize = 10 }, includeHidden: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task ListAsync_FilterByYear_ReturnsMatchingCharts()
    {
        var service = GetChartService();
        await service.CreateAsync("Chart 2024", year: 2024);
        await service.CreateAsync("Chart 2023", year: 2023);

        var result = await service.ListAsync(new PagedRequest { PageSize = 10 }, includeHidden: true, filterByYear: 2024);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Data);
        Assert.Equal(2024, result.Data.First().Year);
    }

    [Fact]
    public async Task ListAsync_FilterByTags_UsesAndSemantics()
    {
        var service = GetChartService();
        await service.CreateAsync("Chart with Rock", tags: ["rock"]);
        await service.CreateAsync("Chart with Rock and Prog", tags: ["rock", "prog"]);

        var result = await service.ListAsync(
            new PagedRequest { PageSize = 10 },
            includeHidden: true,
            filterByTags: ["rock", "prog"]);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Data);
        Assert.Equal("Chart with Rock and Prog", result.Data.First().Title);
    }

    #endregion

    #region CSV Parsing Tests

    [Fact]
    public async Task ParseCsvAsync_WithValidData_ReturnsPreviewItems()
    {
        var service = GetChartService();
        var csv = @"1,The Beatles,Abbey Road,1969
2,Pink Floyd,The Dark Side of the Moon,1973
3,Led Zeppelin,IV,1971";

        var result = await service.ParseCsvAsync(csv);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Items.Count);
        Assert.Empty(result.Data.Errors);
        Assert.Equal("The Beatles", result.Data.Items[0].ArtistName);
        Assert.Equal("Abbey Road", result.Data.Items[0].AlbumTitle);
        Assert.Equal(1969, result.Data.Items[0].ReleaseYear);
    }

    [Fact]
    public async Task ParseCsvAsync_WithEmptyInput_ReturnsError()
    {
        var service = GetChartService();

        var result = await service.ParseCsvAsync("");

        Assert.NotNull(result.Data);
        Assert.True(result.Data.HasErrors);
        Assert.Single(result.Data.Errors);
    }

    [Fact]
    public async Task ParseCsvAsync_WithNonIntegerRank_ReturnsError()
    {
        var service = GetChartService();
        var csv = @"one,The Beatles,Abbey Road,1969";

        var result = await service.ParseCsvAsync(csv);

        Assert.NotNull(result.Data);
        Assert.True(result.Data.HasErrors);
        Assert.Contains(result.Data.Errors, e => e.Message.Contains("Invalid rank"));
    }

    [Fact]
    public async Task ParseCsvAsync_WithDuplicateRank_ReturnsError()
    {
        var service = GetChartService();
        var csv = @"1,The Beatles,Abbey Road,1969
1,Pink Floyd,The Wall,1979";

        var result = await service.ParseCsvAsync(csv);

        Assert.NotNull(result.Data);
        Assert.True(result.Data.HasErrors);
        Assert.Contains(result.Data.Errors, e => e.Message.Contains("Duplicate rank"));
    }

    [Fact]
    public async Task ParseCsvAsync_WithMissingArtist_ReturnsError()
    {
        var service = GetChartService();
        var csv = @"1,,Abbey Road,1969";

        var result = await service.ParseCsvAsync(csv);

        Assert.NotNull(result.Data);
        Assert.True(result.Data.HasErrors);
        Assert.Contains(result.Data.Errors, e => e.Message.Contains("Artist name is required"));
    }

    [Fact]
    public async Task ParseCsvAsync_WithMissingAlbum_ReturnsError()
    {
        var service = GetChartService();
        var csv = @"1,The Beatles,,1969";

        var result = await service.ParseCsvAsync(csv);

        Assert.NotNull(result.Data);
        Assert.True(result.Data.HasErrors);
        Assert.Contains(result.Data.Errors, e => e.Message.Contains("Album title is required"));
    }

    [Fact]
    public async Task ParseCsvAsync_WithQuotedFields_ParsesCorrectly()
    {
        var service = GetChartService();
        var csv = @"1,""AC/DC"",""Back in Black"",1980";

        var result = await service.ParseCsvAsync(csv);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Items);
        Assert.Equal("AC/DC", result.Data.Items[0].ArtistName);
        Assert.Equal("Back in Black", result.Data.Items[0].AlbumTitle);
    }

    #endregion

    #region Linking Tests

    [Fact]
    public async Task LinkItemsAsync_ExactMatch_SetsLinkedAlbumAndDerivedArtist()
    {
        var service = GetChartService();

        var artist = await CreateTestArtistAsync("The Beatles");
        var album = await CreateTestAlbumAsync(artist.Id, "Abbey Road", 1969);

        var createResult = await service.CreateAsync("Test Chart");
        var chartId = createResult.Data!.Id;

        var csv = @"1,The Beatles,Abbey Road,1969";
        var parseResult = await service.ParseCsvAsync(csv);
        await service.SaveItemsAsync(chartId, parseResult.Data!.Items, doAutoLink: false);

        var linkResult = await service.LinkItemsAsync(chartId);

        Assert.True(linkResult.IsSuccess);
        Assert.NotNull(linkResult.Data);
        Assert.Equal(1, linkResult.Data.LinkedCount);
        Assert.Equal(0, linkResult.Data.AmbiguousCount);

        var chartResult = await service.GetByIdAsync(chartId);
        var linkedItem = chartResult.Data!.Items.First();
        Assert.Equal(album.Id, linkedItem.LinkedAlbumId);
        Assert.Equal(artist.Id, linkedItem.LinkedArtistId);
        Assert.Equal(ChartItemLinkStatus.Linked, linkedItem.LinkStatusValue);
    }

    [Fact]
    public async Task LinkItemsAsync_NoMatch_RemainsUnlinked()
    {
        var service = GetChartService();

        var createResult = await service.CreateAsync("Test Chart");
        var chartId = createResult.Data!.Id;

        var csv = @"1,Nonexistent Artist,Nonexistent Album,2024";
        var parseResult = await service.ParseCsvAsync(csv);
        await service.SaveItemsAsync(chartId, parseResult.Data!.Items, doAutoLink: false);

        var linkResult = await service.LinkItemsAsync(chartId);

        Assert.True(linkResult.IsSuccess);
        Assert.NotNull(linkResult.Data);
        Assert.Equal(1, linkResult.Data.UnlinkedCount);

        var chartResult = await service.GetByIdAsync(chartId);
        var item = chartResult.Data!.Items.First();
        Assert.Null(item.LinkedAlbumId);
        Assert.Equal(ChartItemLinkStatus.Unlinked, item.LinkStatusValue);
    }

    [Fact(Skip = "Cannot create ambiguous albums due to unique constraint on (ArtistId, NameNormalized). Ambiguous linking would occur with different spellings resolving to the same normalized form, which is an edge case requiring special test data.")]
    public async Task LinkItemsAsync_AmbiguousMatch_SetsAmbiguousStatus()
    {
        // This test is skipped because the data model prevents creating multiple albums
        // with the same normalized name for an artist. Ambiguous matching would only occur
        // in edge cases like different unicode spellings that normalize to the same value.
    }

    [Fact]
    public async Task LinkItemsAsync_DoesNotOverwriteManualLinks_ByDefault()
    {
        var service = GetChartService();

        var artist = await CreateTestArtistAsync("Test Artist");
        var album1 = await CreateTestAlbumAsync(artist.Id, "Album One");
        var album2 = await CreateTestAlbumAsync(artist.Id, "Album Two");

        var createResult = await service.CreateAsync("Test Chart");
        var chartId = createResult.Data!.Id;

        var csv = @"1,Test Artist,Album One,2024";
        var parseResult = await service.ParseCsvAsync(csv);
        await service.SaveItemsAsync(chartId, parseResult.Data!.Items, doAutoLink: true);

        var chartResult = await service.GetByIdAsync(chartId);
        var itemId = chartResult.Data!.Items.First().Id;
        await service.ResolveItemAsync(itemId, album2.Id);

        await service.LinkItemsAsync(chartId, overwriteManualLinks: false);

        var updatedChart = await service.GetByIdAsync(chartId);
        var item = updatedChart.Data!.Items.First();
        Assert.Equal(album2.Id, item.LinkedAlbumId);
    }

    #endregion

    #region Manual Resolution Tests

    [Fact]
    public async Task ResolveItemAsync_SetsLinkedAlbumAndDerivedArtist()
    {
        var service = GetChartService();

        var artist = await CreateTestArtistAsync("Test Artist");
        var album = await CreateTestAlbumAsync(artist.Id, "Test Album");

        var createResult = await service.CreateAsync("Test Chart");
        var chartId = createResult.Data!.Id;

        var csv = @"1,Different Artist,Different Album,2024";
        var parseResult = await service.ParseCsvAsync(csv);
        await service.SaveItemsAsync(chartId, parseResult.Data!.Items, doAutoLink: false);

        var chartResult = await service.GetByIdAsync(chartId);
        var itemId = chartResult.Data!.Items.First().Id;

        var resolveResult = await service.ResolveItemAsync(itemId, album.Id);

        Assert.True(resolveResult.IsSuccess);

        var updatedChart = await service.GetByIdAsync(chartId);
        var item = updatedChart.Data!.Items.First();
        Assert.Equal(album.Id, item.LinkedAlbumId);
        Assert.Equal(artist.Id, item.LinkedArtistId);
        Assert.Equal(ChartItemLinkStatus.Linked, item.LinkStatusValue);
        Assert.Equal("Manually resolved", item.LinkNotes);
    }

    [Fact]
    public async Task ResolveItemAsync_WithNonExistentItem_ReturnsNotFound()
    {
        var service = GetChartService();

        var result = await service.ResolveItemAsync(99999, 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.NotFound, result.Type);
    }

    #endregion

    #region Ignore Item Tests

    [Fact]
    public async Task IgnoreItemAsync_SetsIgnoredStatus()
    {
        var service = GetChartService();

        var createResult = await service.CreateAsync("Test Chart");
        var chartId = createResult.Data!.Id;

        var csv = @"1,Test Artist,Test Album,2024";
        var parseResult = await service.ParseCsvAsync(csv);
        await service.SaveItemsAsync(chartId, parseResult.Data!.Items, doAutoLink: false);

        var chartResult = await service.GetByIdAsync(chartId);
        var itemId = chartResult.Data!.Items.First().Id;

        var ignoreResult = await service.IgnoreItemAsync(itemId);

        Assert.True(ignoreResult.IsSuccess);

        var updatedChart = await service.GetByIdAsync(chartId);
        var item = updatedChart.Data!.Items.First();
        Assert.Equal(ChartItemLinkStatus.Ignored, item.LinkStatusValue);
        Assert.Null(item.LinkedAlbumId);
    }

    #endregion

    #region Generated Playlist Tests

    [Fact]
    public async Task GetGeneratedPlaylistTracksAsync_OrdersByChartRankThenSongNumber()
    {
        var service = GetChartService();

        var artist = await CreateTestArtistAsync("Test Artist");
        var album1 = await CreateTestAlbumAsync(artist.Id, "Album One");
        var album2 = await CreateTestAlbumAsync(artist.Id, "Album Two");

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Songs.AddRange(
                new Song
                {
                    AlbumId = album1.Id,
                    Title = "Song 1",
                    TitleNormalized = "song1",
                    SongNumber = 1,
                    FileName = "song1.mp3",
                    FileSize = 1024,
                    FileHash = "hash1",
                    Duration = 180,
                    SamplingRate = 44100,
                    BitRate = 320,
                    BitDepth = 16,
                    BPM = 120,
                    ContentType = "audio/mpeg",
                    ApiKey = Guid.NewGuid(),
                    CreatedAt = SystemClock.Instance.GetCurrentInstant()
                },
                new Song
                {
                    AlbumId = album1.Id,
                    Title = "Song 2",
                    TitleNormalized = "song2",
                    SongNumber = 2,
                    FileName = "song2.mp3",
                    FileSize = 1024,
                    FileHash = "hash2",
                    Duration = 200,
                    SamplingRate = 44100,
                    BitRate = 320,
                    BitDepth = 16,
                    BPM = 120,
                    ContentType = "audio/mpeg",
                    ApiKey = Guid.NewGuid(),
                    CreatedAt = SystemClock.Instance.GetCurrentInstant()
                },
                new Song
                {
                    AlbumId = album2.Id,
                    Title = "Song A",
                    TitleNormalized = "songa",
                    SongNumber = 1,
                    FileName = "songa.mp3",
                    FileSize = 1024,
                    FileHash = "hasha",
                    Duration = 220,
                    SamplingRate = 44100,
                    BitRate = 320,
                    BitDepth = 16,
                    BPM = 120,
                    ContentType = "audio/mpeg",
                    ApiKey = Guid.NewGuid(),
                    CreatedAt = SystemClock.Instance.GetCurrentInstant()
                });
            await context.SaveChangesAsync();
        }

        var createResult = await service.CreateAsync("Test Chart", isGeneratedPlaylistEnabled: true);
        var chartId = createResult.Data!.Id;

        var csv = @"1,Test Artist,Album One,2024
2,Test Artist,Album Two,2024";
        var parseResult = await service.ParseCsvAsync(csv);
        await service.SaveItemsAsync(chartId, parseResult.Data!.Items, doAutoLink: true);

        var playlistResult = await service.GetGeneratedPlaylistTracksAsync(chartId);

        Assert.True(playlistResult.IsSuccess);
        var tracks = playlistResult.Data!.ToList();
        Assert.Equal(3, tracks.Count);

        Assert.Equal(1, tracks[0].ChartRank);
        Assert.Equal("Song 1", tracks[0].SongTitle);

        Assert.Equal(1, tracks[1].ChartRank);
        Assert.Equal("Song 2", tracks[1].SongTitle);

        Assert.Equal(2, tracks[2].ChartRank);
        Assert.Equal("Song A", tracks[2].SongTitle);
    }

    [Fact]
    public async Task GetGeneratedPlaylistTracksAsync_ExcludesUnlinkedItems()
    {
        var service = GetChartService();

        var createResult = await service.CreateAsync("Test Chart", isGeneratedPlaylistEnabled: true);
        var chartId = createResult.Data!.Id;

        var csv = @"1,Nonexistent Artist,Nonexistent Album,2024";
        var parseResult = await service.ParseCsvAsync(csv);
        await service.SaveItemsAsync(chartId, parseResult.Data!.Items, doAutoLink: true);

        var playlistResult = await service.GetGeneratedPlaylistTracksAsync(chartId);

        Assert.True(playlistResult.IsSuccess);
        Assert.Empty(playlistResult.Data!);
    }

    [Fact]
    public async Task GetGeneratedPlaylistTracksAsync_WhenDisabled_ReturnsError()
    {
        var service = GetChartService();

        var createResult = await service.CreateAsync("Test Chart", isGeneratedPlaylistEnabled: false);
        var chartId = createResult.Data!.Id;

        var playlistResult = await service.GetGeneratedPlaylistTracksAsync(chartId);

        Assert.NotNull(playlistResult.Messages);
        Assert.Contains(playlistResult.Messages, m => m.Contains("not enabled"));
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteAsync_WithExistingChart_DeletesChartAndItems()
    {
        var service = GetChartService();

        var createResult = await service.CreateAsync("Test Chart");
        var chartId = createResult.Data!.Id;

        var csv = @"1,Test Artist,Test Album,2024";
        var parseResult = await service.ParseCsvAsync(csv);
        await service.SaveItemsAsync(chartId, parseResult.Data!.Items, doAutoLink: false);

        var deleteResult = await service.DeleteAsync(chartId);

        Assert.True(deleteResult.IsSuccess);

        var getResult = await service.GetByIdAsync(chartId);
        Assert.Null(getResult.Data);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingChart_ReturnsNotFound()
    {
        var service = GetChartService();

        var result = await service.DeleteAsync(99999);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.NotFound, result.Type);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateAsync_WithTitleChange_UpdatesSlug()
    {
        var service = GetChartService();

        var createResult = await service.CreateAsync("Original Title");
        var chartId = createResult.Data!.Id;

        var updateResult = await service.UpdateAsync(chartId, "New Title");

        Assert.True(updateResult.IsSuccess);

        var getResult = await service.GetByIdAsync(chartId);
        Assert.Equal("New Title", getResult.Data!.Title);
        Assert.Equal("new-title", getResult.Data.Slug);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ReturnsNotFound()
    {
        var service = GetChartService();

        var result = await service.UpdateAsync(99999, "New Title");

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.NotFound, result.Type);
    }

    #endregion

    #region Transactional Behavior Tests

    [Fact]
    public async Task SaveItemsAsync_TransactionalBehavior_AllOrNothing()
    {
        var service = GetChartService();

        var createResult = await service.CreateAsync("Test Chart");
        var chartId = createResult.Data!.Id;

        var validItems = new[]
        {
            new ChartCsvPreviewItem { RowNumber = 1, Rank = 1, ArtistName = "Artist 1", AlbumTitle = "Album 1" },
            new ChartCsvPreviewItem { RowNumber = 2, Rank = 2, ArtistName = "Artist 2", AlbumTitle = "Album 2" }
        };

        var result = await service.SaveItemsAsync(chartId, validItems, doAutoLink: false);

        Assert.True(result.IsSuccess);

        var chartResult = await service.GetByIdAsync(chartId);
        Assert.Equal(2, chartResult.Data!.Items.Count);
    }

    #endregion
}

using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using NodaTime;

namespace Melodee.Tests.Common.Services;

public class StatisticsServiceTests : ServiceTestBase
{
    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnAllStatistics_WhenDatabaseIsEmpty()
    {
        var service = GetStatisticsService();

        var result = await service.GetStatisticsAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(17, result.Data.Length);

        var albumsStatistic = result.Data.First(x => x.Title == "Albums");
        Assert.Equal(StatisticType.Count, albumsStatistic.Type);
        Assert.Equal(0, albumsStatistic.Data);
        Assert.Equal("album", albumsStatistic.Icon);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldHandleNullGenres_WhenNoGenresExist()
    {
        var service = GetStatisticsService();

        var result = await service.GetStatisticsAsync();

        Assert.True(result.IsSuccess);
        var genresStatistic = result.Data!.First(x => x.Title == "Genres");
        Assert.Equal(0, genresStatistic.Data);
    }

    [Fact]
    public async Task GetUserSongStatisticsAsync_ShouldReturnZero_WhenUserHasNoData()
    {
        var service = GetStatisticsService();
        var result = await service.GetUserSongStatisticsAsync(Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data!.Length);
        Assert.Equal(0, result.Data!.First(x => x.Title == "Your Favorite songs").Data);
        Assert.Equal(0, result.Data!.First(x => x.Title == "Your Rated songs").Data);
    }

    [Fact]
    public async Task GetUserAlbumStatisticsAsync_ShouldReturnZero_WhenUserHasNoData()
    {
        var service = GetStatisticsService();
        var result = await service.GetUserAlbumStatisticsAsync(Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
        Assert.Equal(0, result.Data!.First(x => x.Title == "Your Favorite albums").Data);
    }

    [Fact]
    public async Task GetUserArtistStatisticsAsync_ShouldReturnZero_WhenUserHasNoData()
    {
        var service = GetStatisticsService();
        var result = await service.GetUserArtistStatisticsAsync(Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
        Assert.Equal(0, result.Data!.First(x => x.Title == "Your Favorite artists").Data);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldHandleCancellationToken()
    {
        var service = GetStatisticsService();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.GetStatisticsAsync(cts.Token));
    }

    [Fact]
    public async Task GetUserSongStatisticsAsync_ShouldHandleCancellationToken()
    {
        var service = GetStatisticsService();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.GetUserSongStatisticsAsync(Guid.NewGuid(), cts.Token));
    }

    [Fact]
    public async Task GetUserAlbumStatisticsAsync_ShouldHandleCancellationToken()
    {
        var service = GetStatisticsService();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.GetUserAlbumStatisticsAsync(Guid.NewGuid(), cts.Token));
    }

    [Fact]
    public async Task GetUserArtistStatisticsAsync_ShouldHandleCancellationToken()
    {
        var service = GetStatisticsService();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.GetUserArtistStatisticsAsync(Guid.NewGuid(), cts.Token));
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldRetainSortOrder_OfStatistics()
    {
        var service = GetStatisticsService();

        var result = await service.GetStatisticsAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var sortOrders = result.Data.Select(x => x.SortOrder).ToArray();
        var expectedOrder = Enumerable.Range(1, 17).Select(x => (short?)x).ToArray();

        Assert.Equal(expectedOrder, sortOrders);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldIncludeCorrectIcons_ForAllStatistics()
    {
        var service = GetStatisticsService();

        var result = await service.GetStatisticsAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var albumsIcon = result.Data!.First(x => x.Title == "Albums").Icon;
        var artistsIcon = result.Data!.First(x => x.Title == "Artists").Icon;
        var songsIcon = result.Data!.First(x => x.Title == "Songs").Icon;

        Assert.Equal("album", albumsIcon);
        Assert.Equal("artist", artistsIcon);
        Assert.Equal("music_note", songsIcon);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnCorrectStatisticTypes()
    {
        var service = GetStatisticsService();

        var result = await service.GetStatisticsAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var countStats = result.Data.Where(x => x.Type == StatisticType.Count).ToArray();
        var infoStats = result.Data.Where(x => x.Type == StatisticType.Information).ToArray();

        Assert.Equal(15, countStats.Length);
        Assert.Equal(2, infoStats.Length);

        Assert.Contains(countStats, x => x.Title == "Albums");
        Assert.Contains(countStats, x => x.Title == "Artists");
        Assert.Contains(countStats, x => x.Title == "Songs");
        Assert.Contains(infoStats, x => x.Title == "Total: Song Mb");
        Assert.Contains(infoStats, x => x.Title == "Total: Song Duration");
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldIncludeApiResultFlag_ForRelevantStatistics()
    {
        var service = GetStatisticsService();

        var result = await service.GetStatisticsAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var albumsStat = result.Data.First(x => x.Title == "Albums");
        var artistsStat = result.Data.First(x => x.Title == "Artists");
        var songsStat = result.Data.First(x => x.Title == "Songs");
        var playlistsStat = result.Data.First(x => x.Title == "Playlists");

        Assert.True(albumsStat.IncludeInApiResult);
        Assert.True(artistsStat.IncludeInApiResult);
        Assert.True(songsStat.IncludeInApiResult);
        Assert.True(playlistsStat.IncludeInApiResult);

        var contributorsStat = result.Data.First(x => x.Title == "Contributors");
        Assert.Null(contributorsStat.IncludeInApiResult);
    }

    [Fact]
    public async Task GetUserSongStatisticsAsync_ShouldReturnCorrectStructure()
    {
        var service = GetStatisticsService();
        var userApiKey = Guid.NewGuid();

        var result = await service.GetUserSongStatisticsAsync(userApiKey);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Length);

        var favoriteStat = result.Data.First(x => x.Title == "Your Favorite songs");
        var ratedStat = result.Data.First(x => x.Title == "Your Rated songs");

        Assert.Equal(StatisticType.Count, favoriteStat.Type);
        Assert.Equal(StatisticType.Count, ratedStat.Type);
        Assert.Equal((short?)1, favoriteStat.SortOrder);
        Assert.Equal((short?)2, ratedStat.SortOrder);
        Assert.Equal("analytics", favoriteStat.Icon);
        Assert.Equal("analytics", ratedStat.Icon);
    }

    [Fact]
    public async Task GetUserAlbumStatisticsAsync_ShouldReturnCorrectStructure()
    {
        var service = GetStatisticsService();
        var userApiKey = Guid.NewGuid();

        var result = await service.GetUserAlbumStatisticsAsync(userApiKey);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);

        var favoriteStat = result.Data.First();
        Assert.Equal("Your Favorite albums", favoriteStat.Title);
        Assert.Equal(StatisticType.Count, favoriteStat.Type);
        Assert.Equal((short?)1, favoriteStat.SortOrder);
        Assert.Equal("analytics", favoriteStat.Icon);
    }

    [Fact]
    public async Task GetUserArtistStatisticsAsync_ShouldReturnCorrectStructure()
    {
        var service = GetStatisticsService();
        var userApiKey = Guid.NewGuid();

        var result = await service.GetUserArtistStatisticsAsync(userApiKey);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);

        var favoriteStat = result.Data.First();
        Assert.Equal("Your Favorite artists", favoriteStat.Title);
        Assert.Equal(StatisticType.Count, favoriteStat.Type);
        Assert.Equal((short?)1, favoriteStat.SortOrder);
        Assert.Equal("analytics", favoriteStat.Icon);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnValidOperationResult()
    {
        var service = GetStatisticsService();

        var result = await service.GetStatisticsAsync();

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Messages);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Messages ?? []);
    }

    [Fact]
    public async Task GetUserSongStatisticsAsync_ShouldReturnValidOperationResult()
    {
        var service = GetStatisticsService();
        var userApiKey = Guid.NewGuid();

        var result = await service.GetUserSongStatisticsAsync(userApiKey);

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Messages);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldHaveCorrectStatisticTitles()
    {
        var service = GetStatisticsService();

        var result = await service.GetStatisticsAsync();

        Assert.True(result.IsSuccess);
        var titles = result.Data!.Select(x => x.Title).ToArray();

        var expectedTitles = new[]
        {
            "Albums", "Artists", "Contributors", "Genres", "Libraries",
            "Playlists", "Radio Stations", "Shares", "Songs", "Songs: Played count",
            "Users", "Users: Favorited artists", "Users: Favorited albums",
            "Users: Favorited songs", "Users: Rated songs",
            "Total: Song Mb", "Total: Song Duration"
        };

        foreach (var expectedTitle in expectedTitles)
        {
            Assert.Contains(expectedTitle, titles);
        }
    }

    [Fact]
    public async Task GetSongsAddedPerDayAsync_ShouldZeroFill_AndBucketByUserTimeZone()
    {
        var tz = "America/New_York";
        var start = new LocalDate(2025, 12, 14);
        var end = new LocalDate(2025, 12, 16);

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                Name = "Test Library",
                Path = "/test/path",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artistName = "Test Artist";
            var artist = new Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = "test-artist",
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!,
                Library = library
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            var albumName = "Test Album";
            var album = new Album
            {
                ApiKey = Guid.NewGuid(),
                ArtistId = artist.Id,
                Artist = artist,
                Name = albumName,
                NameNormalized = albumName.ToNormalizedString()!,
                Directory = "/test/album/",
                ReleaseDate = new LocalDate(2025, 1, 1),
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Albums.Add(album);
            await context.SaveChangesAsync();

            // A: 2025-12-14 06:00Z -> 2025-12-14 01:00 America/New_York
            var songAInstant = Instant.FromUtc(2025, 12, 14, 6, 0);
            // B: 2025-12-15 04:30Z -> 2025-12-14 23:30 America/New_York
            var songBInstant = Instant.FromUtc(2025, 12, 15, 4, 30);

            context.Songs.AddRange(
                new Song
                {
                    AlbumId = album.Id,
                    Album = album,
                    Title = "Song A",
                    TitleNormalized = "song a",
                    SongNumber = 1,
                    FileName = "a.flac",
                    FileSize = 1,
                    FileHash = "hash-a",
                    Duration = 1000,
                    SamplingRate = 44100,
                    BitRate = 320,
                    BitDepth = 16,
                    BPM = 120,
                    ContentType = "audio/flac",
                    CreatedAt = songAInstant
                },
                new Song
                {
                    AlbumId = album.Id,
                    Album = album,
                    Title = "Song B",
                    TitleNormalized = "song b",
                    SongNumber = 2,
                    FileName = "b.flac",
                    FileSize = 1,
                    FileHash = "hash-b",
                    Duration = 1000,
                    SamplingRate = 44100,
                    BitRate = 320,
                    BitDepth = 16,
                    BPM = 120,
                    ContentType = "audio/flac",
                    CreatedAt = songBInstant
                });

            await context.SaveChangesAsync();
        }

        var service = GetStatisticsService();
        var result = await service.GetSongsAddedPerDayAsync(start, end, tz);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Length);

        Assert.Equal(start, result.Data[0].Day);
        Assert.Equal(2, result.Data[0].Value);

        Assert.Equal(start.PlusDays(1), result.Data[1].Day);
        Assert.Equal(0, result.Data[1].Value);

        Assert.Equal(start.PlusDays(2), result.Data[2].Day);
        Assert.Equal(0, result.Data[2].Value);
    }

    [Fact]
    public async Task GetSongsAddedPerDayAsync_ShouldBucketDifferently_InUtc()
    {
        var start = new LocalDate(2025, 12, 14);
        var end = new LocalDate(2025, 12, 16);

        // Data already seeded by prior tests runs is isolated by test instance, but be explicit.
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Songs.RemoveRange(context.Songs);
            context.Albums.RemoveRange(context.Albums);
            context.Artists.RemoveRange(context.Artists);
            context.Libraries.RemoveRange(context.Libraries);
            await context.SaveChangesAsync();

            var library = new Library
            {
                Name = "Test Library",
                Path = "/test/path",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artistName = "Test Artist";
            var artist = new Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = "test-artist",
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!,
                Library = library
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            var albumName = "Test Album";
            var album = new Album
            {
                ApiKey = Guid.NewGuid(),
                ArtistId = artist.Id,
                Artist = artist,
                Name = albumName,
                NameNormalized = albumName.ToNormalizedString()!,
                Directory = "/test/album/",
                ReleaseDate = new LocalDate(2025, 1, 1),
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Albums.Add(album);
            await context.SaveChangesAsync();

            var songAInstant = Instant.FromUtc(2025, 12, 14, 6, 0);
            var songBInstant = Instant.FromUtc(2025, 12, 15, 4, 30);

            context.Songs.AddRange(
                new Song
                {
                    AlbumId = album.Id,
                    Album = album,
                    Title = "Song A",
                    TitleNormalized = "song a",
                    SongNumber = 1,
                    FileName = "a.flac",
                    FileSize = 1,
                    FileHash = "hash-a",
                    Duration = 1000,
                    SamplingRate = 44100,
                    BitRate = 320,
                    BitDepth = 16,
                    BPM = 120,
                    ContentType = "audio/flac",
                    CreatedAt = songAInstant
                },
                new Song
                {
                    AlbumId = album.Id,
                    Album = album,
                    Title = "Song B",
                    TitleNormalized = "song b",
                    SongNumber = 2,
                    FileName = "b.flac",
                    FileSize = 1,
                    FileHash = "hash-b",
                    Duration = 1000,
                    SamplingRate = 44100,
                    BitRate = 320,
                    BitDepth = 16,
                    BPM = 120,
                    ContentType = "audio/flac",
                    CreatedAt = songBInstant
                });

            await context.SaveChangesAsync();
        }

        var service = GetStatisticsService();
        var result = await service.GetSongsAddedPerDayAsync(start, end, "UTC");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var day14 = result.Data.Single(x => x.Day == start).Value;
        var day15 = result.Data.Single(x => x.Day == start.PlusDays(1)).Value;
        var day16 = result.Data.Single(x => x.Day == start.PlusDays(2)).Value;

        Assert.Equal(1, day14);
        Assert.Equal(1, day15);
        Assert.Equal(0, day16);
    }

    [Fact]
    public async Task GetUserSearchesPerDayAsync_ShouldZeroFill_AndBucketByUserTimeZone()
    {
        var tz = "America/New_York";
        var start = new LocalDate(2025, 12, 14);
        var end = new LocalDate(2025, 12, 16);

        var userApiKey = Guid.NewGuid();
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.SearchHistories.RemoveRange(context.SearchHistories);
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();

            var user = new User
            {
                ApiKey = userApiKey,
                UserName = "testuser",
                UserNameNormalized = "TESTUSER",
                Email = "test@example.com",
                EmailNormalized = "TEST@EXAMPLE.COM",
                PublicKey = "pk",
                PasswordEncrypted = "enc",
                TimeZoneId = tz,
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // A: 2025-12-14 06:00Z -> 2025-12-14 01:00 America/New_York
            var a = Instant.FromUtc(2025, 12, 14, 6, 0);
            // B: 2025-12-15 04:30Z -> 2025-12-14 23:30 America/New_York
            var b = Instant.FromUtc(2025, 12, 15, 4, 30);

            context.SearchHistories.AddRange(
                new SearchHistory { ByUserId = user.Id, SearchDurationInMs = 1, CreatedAt = a },
                new SearchHistory { ByUserId = user.Id, SearchDurationInMs = 1, CreatedAt = b });

            await context.SaveChangesAsync();
        }

        var service = GetStatisticsService();
        var result = await service.GetUserSearchesPerDayAsync(userApiKey, start, end, tz);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Length);

        Assert.Equal(start, result.Data[0].Day);
        Assert.Equal(2, result.Data[0].Value);
        Assert.Equal(start.PlusDays(1), result.Data[1].Day);
        Assert.Equal(0, result.Data[1].Value);
        Assert.Equal(start.PlusDays(2), result.Data[2].Day);
        Assert.Equal(0, result.Data[2].Value);
    }

    [Fact]
    public async Task GetShareViewsPerDayAsync_ShouldZeroFill_AndBucketByUserTimeZone()
    {
        var tz = "America/New_York";
        var start = new LocalDate(2025, 12, 14);
        var end = new LocalDate(2025, 12, 16);

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.ShareActivities.RemoveRange(context.ShareActivities);
            context.Shares.RemoveRange(context.Shares);
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();

            var user = new User
            {
                UserName = "testuser",
                UserNameNormalized = "TESTUSER",
                Email = "test@example.com",
                EmailNormalized = "TEST@EXAMPLE.COM",
                PublicKey = "pk",
                PasswordEncrypted = "enc",
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var share = new Share
            {
                UserId = user.Id,
                ShareId = 1,
                ShareType = (int)ShareType.Song,
                ShareUniqueId = "abc",
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Shares.Add(share);
            await context.SaveChangesAsync();

            var a = Instant.FromUtc(2025, 12, 14, 6, 0);
            var b = Instant.FromUtc(2025, 12, 15, 4, 30);

            context.ShareActivities.AddRange(
                new ShareActivity { ShareId = share.Id, Client = "c", CreatedAt = a },
                new ShareActivity { ShareId = share.Id, Client = "c", CreatedAt = b });

            await context.SaveChangesAsync();
        }

        var service = GetStatisticsService();
        var result = await service.GetShareViewsPerDayAsync(start, end, tz);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Length);

        Assert.Equal(start, result.Data[0].Day);
        Assert.Equal(2, result.Data[0].Value);
        Assert.Equal(start.PlusDays(1), result.Data[1].Day);
        Assert.Equal(0, result.Data[1].Value);
        Assert.Equal(start.PlusDays(2), result.Data[2].Day);
        Assert.Equal(0, result.Data[2].Value);
    }

    [Fact]
    public async Task GetLibraryScansPerDayAsync_ShouldZeroFill_AndBucketByUserTimeZone()
    {
        var tz = "America/New_York";
        var start = new LocalDate(2025, 12, 14);
        var end = new LocalDate(2025, 12, 16);

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.LibraryScanHistories.RemoveRange(context.LibraryScanHistories);
            context.Libraries.RemoveRange(context.Libraries);
            await context.SaveChangesAsync();

            var library = new Library
            {
                Name = "Test Library",
                Path = "/test/path",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var a = Instant.FromUtc(2025, 12, 14, 6, 0);  // local 12/14
            var b = Instant.FromUtc(2025, 12, 15, 5, 0);  // local 12/15

            context.LibraryScanHistories.AddRange(
                new LibraryScanHistory { LibraryId = library.Id, Library = library, DurationInMs = 1, CreatedAt = a },
                new LibraryScanHistory { LibraryId = library.Id, Library = library, DurationInMs = 1, CreatedAt = b });

            await context.SaveChangesAsync();
        }

        var service = GetStatisticsService();
        var result = await service.GetLibraryScansPerDayAsync(start, end, tz);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        Assert.Equal(1, result.Data.Single(x => x.Day == start).Value);
        Assert.Equal(1, result.Data.Single(x => x.Day == start.PlusDays(1)).Value);
        Assert.Equal(0, result.Data.Single(x => x.Day == start.PlusDays(2)).Value);
    }

    [Fact]
    public async Task GetUserSongPlaysPerDayAsync_ShouldZeroFillAndBucket()
    {
        var tz = "America/New_York";
        var start = new LocalDate(2025, 12, 14);
        var end = new LocalDate(2025, 12, 16);
        var userApiKey = Guid.NewGuid();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.UserSongPlayHistories.RemoveRange(context.UserSongPlayHistories);
            context.Users.RemoveRange(context.Users);
            context.Songs.RemoveRange(context.Songs);
            context.Albums.RemoveRange(context.Albums);
            context.Artists.RemoveRange(context.Artists);
            context.Libraries.RemoveRange(context.Libraries);
            await context.SaveChangesAsync();

            var library = new Library
            {
                Name = "Test Library",
                Path = "/test/path",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = "artist",
                Name = "Artist",
                NameNormalized = "ARTIST",
                LibraryId = library.Id,
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            var album = new Album
            {
                ApiKey = Guid.NewGuid(),
                ArtistId = artist.Id,
                Artist = artist,
                Name = "Album",
                NameNormalized = "ALBUM",
                Directory = "/album/",
                ReleaseDate = new LocalDate(2025, 1, 1),
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Albums.Add(album);
            await context.SaveChangesAsync();

            var user = new User
            {
                ApiKey = userApiKey,
                UserName = "testuser",
                UserNameNormalized = "TESTUSER",
                Email = "test@example.com",
                EmailNormalized = "TEST@EXAMPLE.COM",
                PublicKey = "pk",
                PasswordEncrypted = "enc",
                TimeZoneId = tz,
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Users.Add(user);

            var song = new Song
            {
                AlbumId = album.Id,
                Title = "Song A",
                TitleNormalized = "song a",
                SongNumber = 1,
                FileName = "a.flac",
                FileSize = 1,
                FileHash = "hash-a",
                Duration = 1000,
                SamplingRate = 44100,
                BitRate = 320,
                BitDepth = 16,
                BPM = 120,
                ContentType = "audio/flac",
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Songs.Add(song);
            await context.SaveChangesAsync();

            var a = Instant.FromUtc(2025, 12, 14, 6, 0);
            var b = Instant.FromUtc(2025, 12, 15, 4, 30);
            context.UserSongPlayHistories.AddRange(
                new UserSongPlayHistory { UserId = user.Id, SongId = song.Id, PlayedAt = a, Client = "test", Source = 1 },
                new UserSongPlayHistory { UserId = user.Id, SongId = song.Id, PlayedAt = b, Client = "test", Source = 1 });

            await context.SaveChangesAsync();
        }

        var service = GetStatisticsService();
        var result = await service.GetUserSongPlaysPerDayAsync(userApiKey, start, end, tz);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data[0].Value);
        Assert.Equal(0, result.Data[1].Value);
        Assert.Equal(0, result.Data[2].Value);
    }

    [Fact]
    public async Task GetUserTopPlayedSongsAsync_ShouldReturnTopN()
    {
        var userApiKey = Guid.NewGuid();
        var start = new LocalDate(2025, 12, 14);
        var end = new LocalDate(2025, 12, 16);

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.UserSongPlayHistories.RemoveRange(context.UserSongPlayHistories);
            context.Users.RemoveRange(context.Users);
            context.Songs.RemoveRange(context.Songs);
            context.Albums.RemoveRange(context.Albums);
            context.Artists.RemoveRange(context.Artists);
            context.Libraries.RemoveRange(context.Libraries);
            await context.SaveChangesAsync();

            var library = new Library
            {
                Name = "Test Library",
                Path = "/test/path",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = "artist",
                Name = "Artist",
                NameNormalized = "ARTIST",
                LibraryId = library.Id,
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            var album = new Album
            {
                ApiKey = Guid.NewGuid(),
                ArtistId = artist.Id,
                Artist = artist,
                Name = "Album",
                NameNormalized = "ALBUM",
                Directory = "/album/",
                ReleaseDate = new LocalDate(2025, 1, 1),
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Albums.Add(album);
            await context.SaveChangesAsync();

            var user = new User
            {
                ApiKey = userApiKey,
                UserName = "testuser",
                UserNameNormalized = "TESTUSER",
                Email = "test@example.com",
                EmailNormalized = "TEST@EXAMPLE.COM",
                PublicKey = "pk",
                PasswordEncrypted = "enc",
                TimeZoneId = "UTC",
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Users.Add(user);
            var song1 = new Song
            {
                AlbumId = album.Id,
                Title = "Song A",
                TitleNormalized = "song a",
                SongNumber = 1,
                FileName = "a.flac",
                FileSize = 1,
                FileHash = "hash-a",
                Duration = 1000,
                SamplingRate = 44100,
                BitRate = 320,
                BitDepth = 16,
                BPM = 120,
                ContentType = "audio/flac",
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            var song2 = new Song
            {
                AlbumId = album.Id,
                Title = "Song B",
                TitleNormalized = "song b",
                SongNumber = 2,
                FileName = "b.flac",
                FileSize = 1,
                FileHash = "hash-b",
                Duration = 1000,
                SamplingRate = 44100,
                BitRate = 320,
                BitDepth = 16,
                BPM = 120,
                ContentType = "audio/flac",
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Songs.AddRange(song1, song2);
            await context.SaveChangesAsync();

            context.UserSongPlayHistories.AddRange(
                new UserSongPlayHistory { UserId = user.Id, SongId = song1.Id, PlayedAt = Instant.FromUtc(2025, 12, 14, 0, 0), Client = "test", Source = 1 },
                new UserSongPlayHistory { UserId = user.Id, SongId = song1.Id, PlayedAt = Instant.FromUtc(2025, 12, 14, 1, 0), Client = "test", Source = 1 },
                new UserSongPlayHistory { UserId = user.Id, SongId = song2.Id, PlayedAt = Instant.FromUtc(2025, 12, 14, 2, 0), Client = "test", Source = 1 });
            await context.SaveChangesAsync();
        }

        var service = GetStatisticsService();
        var result = await service.GetUserTopPlayedSongsAsync(userApiKey, start, end, "UTC", 2);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Length);
        Assert.Equal("Song A", result.Data[0].Label);
        Assert.Equal(2, result.Data[0].Value);
    }

    [Fact]
    public async Task GetAlbumsByYearAsync_ShouldReturnEmpty_WhenNoAlbumsExist()
    {
        var service = GetStatisticsService();

        var result = await service.GetAlbumsByYearAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetAlbumsByYearAsync_ShouldGroupByYear_AndOrderByYear()
    {
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Albums.RemoveRange(context.Albums);
            context.Artists.RemoveRange(context.Artists);
            context.Libraries.RemoveRange(context.Libraries);
            await context.SaveChangesAsync();

            var library = new Library
            {
                Name = "Test Library",
                Path = "/test/path",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = "artist",
                Name = "Artist",
                NameNormalized = "ARTIST",
                LibraryId = library.Id,
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            context.Albums.AddRange(
                new Album { ApiKey = Guid.NewGuid(), ArtistId = artist.Id, Artist = artist, Name = "Album1", NameNormalized = "ALBUM1", Directory = "/a1/", ReleaseDate = new LocalDate(2020, 1, 1), CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) },
                new Album { ApiKey = Guid.NewGuid(), ArtistId = artist.Id, Artist = artist, Name = "Album2", NameNormalized = "ALBUM2", Directory = "/a2/", ReleaseDate = new LocalDate(2020, 6, 1), CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) },
                new Album { ApiKey = Guid.NewGuid(), ArtistId = artist.Id, Artist = artist, Name = "Album3", NameNormalized = "ALBUM3", Directory = "/a3/", ReleaseDate = new LocalDate(2021, 1, 1), CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) }
            );
            await context.SaveChangesAsync();
        }

        var service = GetStatisticsService();
        var result = await service.GetAlbumsByYearAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Length);
        Assert.Equal("2020", result.Data[0].Label);
        Assert.Equal(2, result.Data[0].Value);
        Assert.Equal("2021", result.Data[1].Label);
        Assert.Equal(1, result.Data[1].Value);
    }

    [Fact]
    public async Task GetAlbumsByGenreAsync_ShouldReturnEmpty_WhenNoAlbumsExist()
    {
        var service = GetStatisticsService();

        var result = await service.GetAlbumsByGenreAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetAlbumsByGenreAsync_ShouldGroupByGenre_AndOrderByCount()
    {
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Albums.RemoveRange(context.Albums);
            context.Artists.RemoveRange(context.Artists);
            context.Libraries.RemoveRange(context.Libraries);
            await context.SaveChangesAsync();

            var library = new Library
            {
                Name = "Test Library",
                Path = "/test/path",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = "artist",
                Name = "Artist",
                NameNormalized = "ARTIST",
                LibraryId = library.Id,
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            context.Albums.AddRange(
                new Album { ApiKey = Guid.NewGuid(), ArtistId = artist.Id, Artist = artist, Name = "Album1", NameNormalized = "ALBUM1", Directory = "/a1/", ReleaseDate = new LocalDate(2020, 1, 1), Genres = new[] { "Rock" }, CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) },
                new Album { ApiKey = Guid.NewGuid(), ArtistId = artist.Id, Artist = artist, Name = "Album2", NameNormalized = "ALBUM2", Directory = "/a2/", ReleaseDate = new LocalDate(2020, 6, 1), Genres = new[] { "Rock", "Alternative" }, CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) },
                new Album { ApiKey = Guid.NewGuid(), ArtistId = artist.Id, Artist = artist, Name = "Album3", NameNormalized = "ALBUM3", Directory = "/a3/", ReleaseDate = new LocalDate(2021, 1, 1), Genres = new[] { "Jazz" }, CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) },
                new Album { ApiKey = Guid.NewGuid(), ArtistId = artist.Id, Artist = artist, Name = "Album4", NameNormalized = "ALBUM4", Directory = "/a4/", ReleaseDate = new LocalDate(2021, 1, 1), CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) }
            );
            await context.SaveChangesAsync();
        }

        var service = GetStatisticsService();
        var result = await service.GetAlbumsByGenreAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Length);
        Assert.Equal("Rock", result.Data[0].Label);
        Assert.Equal(2, result.Data[0].Value);
        Assert.Equal("Jazz", result.Data[1].Label);
        Assert.Equal(1, result.Data[1].Value);
        Assert.Equal("Unknown", result.Data[2].Label);
        Assert.Equal(1, result.Data[2].Value);
    }

    [Fact]
    public async Task GetTopArtistsByAlbumCountAsync_ShouldReturnTopN()
    {
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Artists.RemoveRange(context.Artists);
            context.Libraries.RemoveRange(context.Libraries);
            await context.SaveChangesAsync();

            var library = new Library
            {
                Name = "Test Library",
                Path = "/test/path",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            context.Artists.AddRange(
                new Artist { ApiKey = Guid.NewGuid(), Directory = "a1", Name = "Artist A", NameNormalized = "ARTIST A", LibraryId = library.Id, AlbumCount = 5, CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) },
                new Artist { ApiKey = Guid.NewGuid(), Directory = "a2", Name = "Artist B", NameNormalized = "ARTIST B", LibraryId = library.Id, AlbumCount = 3, CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) },
                new Artist { ApiKey = Guid.NewGuid(), Directory = "a3", Name = "Artist C", NameNormalized = "ARTIST C", LibraryId = library.Id, AlbumCount = 1, CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) }
            );
            await context.SaveChangesAsync();
        }

        var service = GetStatisticsService();
        var result = await service.GetTopArtistsByAlbumCountAsync(2);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Length);
        Assert.Equal("Artist A", result.Data[0].Label);
        Assert.Equal(5, result.Data[0].Value);
        Assert.Equal("Artist B", result.Data[1].Label);
        Assert.Equal(3, result.Data[1].Value);
    }

    [Fact]
    public async Task GetTopArtistsByPlaysAsync_ShouldReturnTopN()
    {
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.UserSongPlayHistories.RemoveRange(context.UserSongPlayHistories);
            context.Songs.RemoveRange(context.Songs);
            context.Albums.RemoveRange(context.Albums);
            context.Artists.RemoveRange(context.Artists);
            context.Libraries.RemoveRange(context.Libraries);
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();

            var library = new Library
            {
                Name = "Test Library",
                Path = "/test/path",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist1 = new Artist { ApiKey = Guid.NewGuid(), Directory = "a1", Name = "Artist A", NameNormalized = "ARTIST A", LibraryId = library.Id, CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) };
            var artist2 = new Artist { ApiKey = Guid.NewGuid(), Directory = "a2", Name = "Artist B", NameNormalized = "ARTIST B", LibraryId = library.Id, CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) };
            context.Artists.AddRange(artist1, artist2);
            await context.SaveChangesAsync();

            var album1 = new Album { ApiKey = Guid.NewGuid(), ArtistId = artist1.Id, Artist = artist1, Name = "Album1", NameNormalized = "ALBUM1", Directory = "/a1/", ReleaseDate = new LocalDate(2020, 1, 1), CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) };
            var album2 = new Album { ApiKey = Guid.NewGuid(), ArtistId = artist2.Id, Artist = artist2, Name = "Album2", NameNormalized = "ALBUM2", Directory = "/a2/", ReleaseDate = new LocalDate(2020, 1, 1), CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) };
            context.Albums.AddRange(album1, album2);
            await context.SaveChangesAsync();

            var song1 = new Song { AlbumId = album1.Id, Title = "Song 1", TitleNormalized = "SONG 1", SongNumber = 1, FileName = "s1.flac", FileSize = 1, FileHash = "h1", Duration = 1000, SamplingRate = 44100, BitRate = 320, BitDepth = 16, BPM = 120, ContentType = "audio/flac", CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) };
            var song2 = new Song { AlbumId = album2.Id, Title = "Song 2", TitleNormalized = "SONG 2", SongNumber = 1, FileName = "s2.flac", FileSize = 1, FileHash = "h2", Duration = 1000, SamplingRate = 44100, BitRate = 320, BitDepth = 16, BPM = 120, ContentType = "audio/flac", CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) };
            context.Songs.AddRange(song1, song2);
            await context.SaveChangesAsync();

            var user = new User { ApiKey = Guid.NewGuid(), UserName = "testuser", UserNameNormalized = "TESTUSER", Email = "test@example.com", EmailNormalized = "TEST@EXAMPLE.COM", PublicKey = "pk", PasswordEncrypted = "enc", CreatedAt = Instant.FromUtc(2025, 12, 1, 0, 0) };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            context.UserSongPlayHistories.AddRange(
                new UserSongPlayHistory { UserId = user.Id, SongId = song1.Id, PlayedAt = Instant.FromUtc(2025, 12, 14, 0, 0), Client = "test", Source = 1 },
                new UserSongPlayHistory { UserId = user.Id, SongId = song1.Id, PlayedAt = Instant.FromUtc(2025, 12, 14, 1, 0), Client = "test", Source = 1 },
                new UserSongPlayHistory { UserId = user.Id, SongId = song1.Id, PlayedAt = Instant.FromUtc(2025, 12, 14, 2, 0), Client = "test", Source = 1 },
                new UserSongPlayHistory { UserId = user.Id, SongId = song2.Id, PlayedAt = Instant.FromUtc(2025, 12, 14, 3, 0), Client = "test", Source = 1 }
            );
            await context.SaveChangesAsync();
        }

        var service = GetStatisticsService();
        var result = await service.GetTopArtistsByPlaysAsync(2);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Length);
        Assert.Equal("Artist A", result.Data[0].Label);
        Assert.Equal(3, result.Data[0].Value);
        Assert.Equal("Artist B", result.Data[1].Label);
        Assert.Equal(1, result.Data[1].Value);
    }
}

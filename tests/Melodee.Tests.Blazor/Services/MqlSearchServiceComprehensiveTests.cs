using FluentAssertions;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Mql;
using Melodee.Mql.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;
using Album = Melodee.Common.Data.Models.Album;
using Artist = Melodee.Common.Data.Models.Artist;
using Song = Melodee.Common.Data.Models.Song;

namespace Melodee.Tests.Blazor.Services;

/// <summary>
/// Comprehensive MQL query tests covering songs, albums, artists, and complex queries.
/// </summary>
public class MqlSearchServiceComprehensiveTests
{
    private readonly IMqlValidator _validator;
    private readonly string _databaseName;
    private readonly DbContextOptions<MelodeeDbContext> _options;
    private readonly IMelodeeConfigurationFactory _configurationFactory;

    public MqlSearchServiceComprehensiveTests()
    {
        _databaseName = $"MqlComprehensiveTestDb_{Guid.NewGuid()}";
        _options = new DbContextOptionsBuilder<MelodeeDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        _validator = new MqlValidator();

        var configMock = new Mock<IMelodeeConfigurationFactory>();
        configMock.Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<IMelodeeConfiguration>().Object);
        _configurationFactory = configMock.Object;

        SeedComprehensiveTestData();
    }

    private MqlSearchService CreateService()
    {
        return new MqlSearchService(new TestDbContextFactory(_options), _validator, _configurationFactory);
    }

    private void SeedComprehensiveTestData()
    {
        using var context = new MelodeeDbContext(_options);

        var library = new Library
        {
            Id = 1,
            ApiKey = Guid.NewGuid(),
            Name = "Test Library",
            Path = "/test/library",
            Type = 0,
            CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0)
        };
        context.Libraries.Add(library);

        // Artists
        var eltonJohn = CreateArtist(1, "Elton John", library);
        var beachBoys = CreateArtist(2, "Beach Boys", library);
        var gunsNRoses = CreateArtist(3, "Guns N' Roses", library);
        var tatu = CreateArtist(4, "t.A.T.u.", library);
        var offspring = CreateArtist(5, "The Offspring", library);
        var donnaSummer = CreateArtist(6, "Donna Summer", library);
        var soundTracks = CreateArtist(7, "SoundTracks", library);
        var allyVenable = CreateArtist(8, "Ally Venable", library);
        var georgeShearing = CreateArtist(9, "George Shearing", library);
        var amazarashi = CreateArtist(10, "Amazarashi", library);
        var hawkwind = CreateArtist(11, "Hawkwind", library);
        var twoMinds = CreateArtist(12, "2Minds", library);
        var greenDay = CreateArtist(13, "Green Day", library);
        var sparks = CreateArtist(14, "Sparks", library);

        context.Artists.AddRange(eltonJohn, beachBoys, gunsNRoses, tatu, offspring,
            donnaSummer, soundTracks, allyVenable, georgeShearing, amazarashi,
            hawkwind, twoMinds, greenDay, sparks);

        // Albums
        var captainFantastic = CreateAlbum(1, "Captain Fantastic And The Brown Dirt Cowboy", eltonJohn, new LocalDate(1975, 5, 19));
        var smileySmile = CreateAlbum(2, "Smiley Smile/Wild Honey", beachBoys, new LocalDate(2001, 7, 10));
        var lostAcoustic = CreateAlbum(3, "The Lost Acoustic", gunsNRoses, new LocalDate(2025, 1, 1));
        var wrongLane = CreateAlbum(4, "200 km/h in the Wrong Lane", tatu, new LocalDate(2002, 5, 21));
        var tatuAlbum2 = CreateAlbum(5, "Dangerous and Moving", tatu, new LocalDate(2005, 10, 5));
        var tatuAlbum3 = CreateAlbum(6, "Waste Management", tatu, new LocalDate(2009, 12, 4));
        var smash = CreateAlbum(7, "Smash", offspring, new LocalDate(1994, 4, 8));
        var catsWithoutClaws = CreateAlbum(8, "Cats Without Claws", donnaSummer, new LocalDate(1984, 9, 28));
        var grease = CreateAlbum(9, "Grease", soundTracks, new LocalDate(1978, 5, 1));
        var mammaMia = CreateAlbum(10, "Mamma Mia!: The Movie Soundtrack", soundTracks, new LocalDate(2008, 7, 1));
        var moneyAndPower = CreateAlbum(11, "Money & Power", allyVenable, new LocalDate(2025, 2, 1));
        var shearingOnStage = CreateAlbum(12, "Shearing on Stage", georgeShearing, new LocalDate(1990, 1, 1));
        var anomaly = CreateAlbum(13, "Anomaly", twoMinds, new LocalDate(2010, 1, 1));
        var rockAlbum = CreateAlbum(14, "Rock Collection", amazarashi, new LocalDate(2025, 1, 1));
        var hawkwindAlbum = CreateAlbum(15, "Space Ritual", hawkwind, new LocalDate(2025, 1, 1));
        var greenDayAlbum = CreateAlbum(16, "Dookie", greenDay, new LocalDate(2025, 1, 1));
        var sparksAlbum = CreateAlbum(17, "Kimono My House", sparks, new LocalDate(2025, 1, 1));

        context.Albums.AddRange(captainFantastic, smileySmile, lostAcoustic, wrongLane,
            tatuAlbum2, tatuAlbum3, smash, catsWithoutClaws, grease, mammaMia,
            moneyAndPower, shearingOnStage, anomaly, rockAlbum, hawkwindAlbum, greenDayAlbum, sparksAlbum);

        var songs = new List<Song>();
        var songId = 1;

        // Elton John - Captain Fantastic (10 songs, 1975, Pop)
        for (var i = 1; i <= 10; i++)
        {
            songs.Add(CreateSong(songId++, $"Elton Track {i}", captainFantastic, i, genres: ["Pop"]));
        }

        // Beach Boys - Smiley Smile/Wild Honey (28 songs, including short interludes)
        for (var i = 1; i <= 26; i++)
        {
            songs.Add(CreateSong(songId++, $"Beach Boys Track {i}", smileySmile, i, genres: ["Pop"], durationMs: 180000));
        }
        songs.Add(CreateSong(songId++, "Short Interlude 1", smileySmile, 27, genres: ["Pop"], durationMs: 67000)); // 67s
        songs.Add(CreateSong(songId++, "Short Interlude 2", smileySmile, 28, genres: ["Pop"], durationMs: 70000)); // 70s

        // Guns N' Roses - The Lost Acoustic (13 songs)
        for (var i = 1; i <= 12; i++)
        {
            songs.Add(CreateSong(songId++, $"GNR Acoustic {i}", lostAcoustic, i, genres: ["Rock"]));
        }
        songs.Add(CreateSong(songId++, "November Rain", lostAcoustic, 13, genres: ["Rock"], durationMs: 530000));

        // t.A.T.u. - 200 km/h in the Wrong Lane (14 songs, 2002)
        for (var i = 1; i <= 14; i++)
        {
            songs.Add(CreateSong(songId++, $"Tatu Track {i}", wrongLane, i, genres: ["Pop"]));
        }

        // t.A.T.u. - Dangerous and Moving (12 songs, 2005)
        for (var i = 1; i <= 12; i++)
        {
            songs.Add(CreateSong(songId++, $"Tatu DM Track {i}", tatuAlbum2, i, genres: ["Pop"]));
        }

        // t.A.T.u. - Waste Management (10 songs, 2009)
        for (var i = 1; i <= 10; i++)
        {
            songs.Add(CreateSong(songId++, $"Tatu WM Track {i}", tatuAlbum3, i, genres: ["Pop"]));
        }

        // The Offspring - Smash (14 songs, 1994, Punk)
        songs.Add(CreateSong(songId++, "Time to Relax", smash, 1, genres: ["Punk"], durationMs: 25000)); // 25 seconds intro
        for (var i = 2; i <= 14; i++)
        {
            songs.Add(CreateSong(songId++, $"Offspring Punk {i}", smash, i, genres: ["Punk"]));
        }

        // Donna Summer - Cats Without Claws (10 songs, 1984, Pop)
        for (var i = 1; i <= 10; i++)
        {
            songs.Add(CreateSong(songId++, $"Donna Track {i}", catsWithoutClaws, i, genres: ["Pop"]));
        }

        // Grease Soundtrack (12 songs, 1978, Pop)
        for (var i = 1; i <= 12; i++)
        {
            songs.Add(CreateSong(songId++, $"Grease Track {i}", grease, i, genres: ["Pop"]));
        }

        // Mamma Mia Soundtrack (12 songs, 2008, Pop)
        for (var i = 1; i <= 12; i++)
        {
            songs.Add(CreateSong(songId++, $"Mamma Mia Track {i}", mammaMia, i, genres: ["Pop"]));
        }

        // Ally Venable - Money & Power (12 songs, 2025, Jazz)
        for (var i = 1; i <= 12; i++)
        {
            songs.Add(CreateSong(songId++, $"Ally Jazz Track {i}", moneyAndPower, i, genres: ["Jazz"]));
        }

        // George Shearing - Shearing on Stage (22 songs, 1990, Jazz)
        for (var i = 1; i <= 22; i++)
        {
            songs.Add(CreateSong(songId++, $"Shearing Jazz {i}", shearingOnStage, i, genres: ["Jazz"]));
        }

        // 2Minds - Anomaly (9 songs, 2010, Psychedelic, all long tracks 430-576 seconds)
        songs.Add(CreateSong(songId++, "Anomaly 1", anomaly, 1, genres: ["Psychedelic"], durationMs: 430000));
        songs.Add(CreateSong(songId++, "Anomaly 2", anomaly, 2, genres: ["Psychedelic"], durationMs: 450000));
        songs.Add(CreateSong(songId++, "Anomaly 3", anomaly, 3, genres: ["Psychedelic"], durationMs: 480000));
        songs.Add(CreateSong(songId++, "Anomaly 4", anomaly, 4, genres: ["Psychedelic"], durationMs: 510000));
        songs.Add(CreateSong(songId++, "Anomaly 5", anomaly, 5, genres: ["Psychedelic"], durationMs: 520000));
        songs.Add(CreateSong(songId++, "Anomaly 6", anomaly, 6, genres: ["Psychedelic"], durationMs: 540000));
        songs.Add(CreateSong(songId++, "Anomaly 7", anomaly, 7, genres: ["Psychedelic"], durationMs: 550000));
        songs.Add(CreateSong(songId++, "Anomaly 8", anomaly, 8, genres: ["Psychedelic"], durationMs: 560000));
        songs.Add(CreateSong(songId++, "Anomaly 9", anomaly, 9, genres: ["Psychedelic"], durationMs: 576000));

        // Amazarashi - Rock Collection (10 songs, 2025, Rock)
        for (var i = 1; i <= 10; i++)
        {
            songs.Add(CreateSong(songId++, $"Amazarashi Rock {i}", rockAlbum, i, genres: ["Rock"]));
        }

        // Hawkwind - Space Ritual (12 songs, 2025, Rock)
        songs.Add(CreateSong(songId++, "Marathon Machine", hawkwindAlbum, 1, genres: ["Rock"], durationMs: 785000)); // 785 seconds - longest
        for (var i = 2; i <= 12; i++)
        {
            songs.Add(CreateSong(songId++, $"Hawkwind Rock {i}", hawkwindAlbum, i, genres: ["Rock"]));
        }

        // Green Day - Dookie (10 songs, 2025, Punk)
        for (var i = 1; i <= 10; i++)
        {
            songs.Add(CreateSong(songId++, $"Green Day Track {i}", greenDayAlbum, i, genres: ["Punk"]));
        }

        // Sparks - Kimono My House (10 songs, 2025, Pop)
        for (var i = 1; i <= 10; i++)
        {
            songs.Add(CreateSong(songId++, $"Sparks Track {i}", sparksAlbum, i, genres: ["Pop"]));
        }

        context.Songs.AddRange(songs);
        context.SaveChanges();
    }

    private static Artist CreateArtist(int id, string name, Library library)
    {
        return new Artist
        {
            Id = id,
            ApiKey = Guid.NewGuid(),
            Directory = name.Replace(" ", "").ToLowerInvariant(),
            Name = name,
            NameNormalized = name.ToNormalizedString() ?? name.ToUpperInvariant(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0)
        };
    }

    private static Album CreateAlbum(int id, string name, Artist artist, LocalDate releaseDate)
    {
        return new Album
        {
            Id = id,
            ApiKey = Guid.NewGuid(),
            ArtistId = artist.Id,
            Artist = artist,
            Name = name,
            NameNormalized = name.ToNormalizedString() ?? name.ToUpperInvariant(),
            Directory = $"/{artist.Directory}/{name.Replace(" ", "").ToLowerInvariant()}/",
            ReleaseDate = releaseDate,
            CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0)
        };
    }

    private static Song CreateSong(int id, string title, Album album, int songNumber, string[]? genres = null, double durationMs = 180000)
    {
        return new Song
        {
            Id = id,
            AlbumId = album.Id,
            Album = album,
            Title = title,
            TitleNormalized = title.ToNormalizedString() ?? title.ToUpperInvariant(),
            SongNumber = songNumber,
            FileName = $"{title.Replace(" ", "_").ToLowerInvariant()}.flac",
            FileSize = 1000000,
            FileHash = $"hash_{id}",
            Duration = durationMs,
            SamplingRate = 44100,
            BitRate = 320,
            BitDepth = 16,
            BPM = 120,
            ContentType = "audio/flac",
            CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0),
            Genres = genres ?? []
        };
    }

    #region Trailing Space Tests

    [Fact]
    public async Task SearchSongsAsync_QueryWithTrailingSpace_WorksCorrectly()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        var resultWithoutSpace = await service.SearchSongsAsync("artist:\"Elton John\"", userId: 1, paging);
        var resultWithSpace = await service.SearchSongsAsync("artist:\"Elton John\" ", userId: 1, paging);

        resultWithSpace.IsValid.Should().BeTrue();
        resultWithSpace.Results.TotalCount.Should().Be(resultWithoutSpace.Results.TotalCount);
    }

    [Fact]
    public async Task SearchSongsAsync_QueryWithLeadingSpace_WorksCorrectly()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        var resultWithoutSpace = await service.SearchSongsAsync("artist:\"Elton John\"", userId: 1, paging);
        var resultWithSpace = await service.SearchSongsAsync(" artist:\"Elton John\"", userId: 1, paging);

        resultWithSpace.IsValid.Should().BeTrue();
        resultWithSpace.Results.TotalCount.Should().Be(resultWithoutSpace.Results.TotalCount);
    }

    #endregion

    #region Songs Queries Tests

    [Fact]
    public async Task SearchSongsAsync_ArtistEltonJohn_Returns10Songs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        var result = await service.SearchSongsAsync("artist:\"Elton John\"", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(10);
    }

    [Fact]
    public async Task SearchSongsAsync_ArtistBeatles_ReturnsNoResults()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // Beatles doesn't exist in test data, only Beach Boys
        var result = await service.SearchSongsAsync("artist:Beatles", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchSongsAsync_ArtistBeachBoys_Returns28Songs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        var result = await service.SearchSongsAsync("artist:\"Beach Boys\"", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(28);
    }

    [Fact(Skip = "InMemory provider cannot translate string[].Contains() for array fields")]
    public async Task SearchSongsAsync_GenreJazz_Returns34Songs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // Ally Venable (12) + George Shearing (22) = 34 Jazz songs
        var result = await service.SearchSongsAsync("genre:Jazz", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(34);
    }

    [Fact(Skip = "InMemory provider cannot translate string[].Contains() for array fields")]
    public async Task SearchSongsAsync_GenreRock_ReturnsRockSongs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // GNR (13) + Amazarashi (10) + Hawkwind (12) = 35 Rock songs
        var result = await service.SearchSongsAsync("genre:Rock", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().BeGreaterThan(0);
        result.Results.Data.Should().OnlyContain(s => s.Genres != null && s.Genres.Contains("Rock"));
    }

    [Fact(Skip = "InMemory provider cannot translate string[].Contains() for array fields")]
    public async Task SearchSongsAsync_GenrePunk_ReturnsPunkSongs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // The Offspring (14) + Green Day (10) = 24 Punk songs
        var result = await service.SearchSongsAsync("genre:Punk", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(24);
    }

    [Fact(Skip = "InMemory provider cannot translate string[].Contains() for array fields")]
    public async Task SearchSongsAsync_GenrePsychedelic_Returns9Songs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // 2Minds - Anomaly (9 songs)
        var result = await service.SearchSongsAsync("genre:Psychedelic", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(9);
    }

    [Fact]
    public async Task SearchSongsAsync_DurationLessThan60_ReturnsShortSongs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // Time to Relax (25s)
        var result = await service.SearchSongsAsync("duration:<60", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().BeGreaterThan(0);
        result.Results.Data.Should().OnlyContain(s => s.Duration < 60000);
    }

    [Fact]
    public async Task SearchSongsAsync_DurationGreaterThan500_ReturnsLongSongs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // Marathon Machine (785s), November Rain (530s), several Anomaly tracks
        var result = await service.SearchSongsAsync("duration:>500", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().BeGreaterThan(0);
        result.Results.Data.Should().OnlyContain(s => s.Duration > 500000);
    }

    [Fact]
    public async Task SearchSongsAsync_Year1975_Returns10Songs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // Elton John - Captain Fantastic (1975)
        var result = await service.SearchSongsAsync("year:1975", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(10);
    }

    [Fact]
    public async Task SearchSongsAsync_YearLessThan2000_ReturnsOlderSongs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // Elton John 1975, Grease 1978, Donna Summer 1984, George Shearing 1990, Offspring 1994
        var result = await service.SearchSongsAsync("year:<2000", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SearchSongsAsync_Year2025_ReturnsMajorityOfSongs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 500 };

        var result = await service.SearchSongsAsync("year:2025", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().BeGreaterThan(50);
    }

    [Fact]
    public async Task SearchSongsAsync_YearRange1970To1990_ReturnsOldAlbumsSongs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // Elton John 1975, Grease 1978, Donna Summer 1984, George Shearing 1990
        var result = await service.SearchSongsAsync("year:1970-1990", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SearchSongsAsync_ArtistGunsNRoses_Returns13Songs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        var result = await service.SearchSongsAsync("artist:\"Guns N' Roses\"", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(13);
    }

    [Fact]
    public async Task SearchSongsAsync_FreeTextNovember_ReturnsNovemberRain()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // Free-text search uses Contains (substring match)
        var result = await service.SearchSongsAsync("November", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(1);
        result.Results.Data.First().Title.Should().Contain("November");
    }

    [Fact]
    public async Task SearchSongsAsync_ArtistTatu_Returns36Songs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // 3 albums: 14 + 12 + 10 = 36 songs
        var result = await service.SearchSongsAsync("artist:t.A.T.u.", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(36);
    }

    #endregion

    #region Albums Queries Tests

    [Fact]
    public async Task SearchAlbumsAsync_ArtistDonnaSummer_Returns1Album()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        var result = await service.SearchAlbumsAsync("artist:\"Donna Summer\"", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(1);
        result.Results.Data.First().Name.Should().Contain("Cats Without Claws");
    }

    [Fact]
    public async Task SearchAlbumsAsync_Year1978_ReturnsGrease()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        var result = await service.SearchAlbumsAsync("year:1978", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(1);
        result.Results.Data.First().Name.Should().Be("Grease");
    }

    [Fact]
    public async Task SearchAlbumsAsync_YearLessThan1990_Returns3Albums()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // Elton John 1975, Grease 1978, Donna Summer 1984
        var result = await service.SearchAlbumsAsync("year:<1990", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task SearchAlbumsAsync_YearRange2000To2015_ReturnsAlbumsInRange()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // Beach Boys 2001, t.A.T.u. 2002/2005/2009, Mamma Mia 2008, 2Minds 2010
        var result = await service.SearchAlbumsAsync("year:2000-2015", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public async Task SearchAlbumsAsync_AlbumGrease_ReturnsGrease()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        var result = await service.SearchAlbumsAsync("album:Grease", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(1);
        result.Results.Data.First().Name.Should().Be("Grease");
    }

    [Fact(Skip = "Regex queries not reliably translatable with InMemory provider")]
    public async Task SearchAlbumsAsync_AlbumRegexMammaMia_ReturnsMammaMia()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        var result = await service.SearchAlbumsAsync("album:/.*Mamma.*/i", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(1);
        result.Results.Data.First().Name.Should().Contain("Mamma Mia");
    }

    [Fact]
    public async Task SearchAlbumsAsync_ArtistSoundTracks_Returns2Albums()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // Grease and Mamma Mia soundtracks
        var result = await service.SearchAlbumsAsync("artist:SoundTracks", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(2);
    }

    #endregion

    #region Artists Queries Tests

    [Fact]
    public async Task SearchArtistsAsync_FreeTextGreen_ReturnsGreenDay()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // Free-text search uses Contains (substring match)
        var result = await service.SearchArtistsAsync("Green", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(1);
        result.Results.Data.First().Name.Should().Be("Green Day");
    }

    [Fact(Skip = "Regex queries not reliably translatable with InMemory provider")]
    public async Task SearchArtistsAsync_ArtistRegexStartsWithThe_ReturnsTheOffspring()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        var result = await service.SearchArtistsAsync("artist:/^The.*/", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(1);
        result.Results.Data.First().Name.Should().Be("The Offspring");
    }

    [Fact(Skip = "Regex queries not reliably translatable with InMemory provider")]
    public async Task SearchArtistsAsync_ArtistRegexSummer_ReturnsDonnaSummer()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        var result = await service.SearchArtistsAsync("artist:/.*Summer.*/i", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(1);
        result.Results.Data.First().Name.Should().Be("Donna Summer");
    }

    [Fact]
    public async Task SearchArtistsAsync_NameSparks_ReturnsSparks()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        var result = await service.SearchArtistsAsync("name:Sparks", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(1);
        result.Results.Data.First().Name.Should().Be("Sparks");
    }

    #endregion

    #region Complex Queries Tests

    [Fact(Skip = "InMemory provider cannot translate string[].Contains() for array fields")]
    public async Task SearchSongsAsync_GenreJazzAndYear2025_ReturnsAllyVenableSongs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // Ally Venable - Money & Power (12 songs, 2025, Jazz)
        var result = await service.SearchSongsAsync("genre:Jazz AND year:2025", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(12);
    }

    [Fact(Skip = "InMemory provider cannot translate string[].Contains() for array fields")]
    public async Task SearchSongsAsync_GenreRockOrPunk_ReturnsCombinedResults()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 200 };

        var result = await service.SearchSongsAsync("genre:Rock OR genre:Punk", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        // Rock (35) + Punk (24) but no overlap expected
        result.Results.TotalCount.Should().BeGreaterThan(30);
    }

    [Fact(Skip = "InMemory provider cannot translate string[].Contains() for array fields")]
    public async Task SearchSongsAsync_PopOrJazzAndYearLessThan2010_ReturnsOlderPopJazz()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 200 };

        var result = await service.SearchSongsAsync("(genre:Pop OR genre:Jazz) AND year:<2010", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SearchSongsAsync_TatuAndYear2002_ReturnsWrongLaneAlbum()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // 200 km/h in the Wrong Lane has 14 songs
        var result = await service.SearchSongsAsync("artist:\"t.A.T.u.\" AND year:2002", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(14);
    }

    [Fact(Skip = "InMemory provider cannot translate string[].Contains() for array fields")]
    public async Task SearchSongsAsync_DurationGreaterThan300AndPsychedelic_ReturnsAnomalySongs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // 2Minds - Anomaly has 9 songs all 430-576 seconds
        var result = await service.SearchSongsAsync("duration:>300 AND genre:Psychedelic", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(9);
    }

    [Fact]
    public async Task SearchSongsAsync_BeachBoysAndDurationLessThan120_ReturnsShortTracks()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        // Beach Boys has 2 short interludes (67s and 70s)
        var result = await service.SearchSongsAsync("artist:\"Beach Boys\" AND duration:<120", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(2);
    }

    [Fact(Skip = "InMemory provider cannot translate string[].Contains() for array fields")]
    public async Task SearchSongsAsync_NotGenrePop_ExcludesPopSongs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 500 };

        var allSongsResult = await service.SearchSongsAsync("*", userId: 1, paging);
        var nonPopResult = await service.SearchSongsAsync("NOT genre:Pop", userId: 1, paging);

        nonPopResult.IsValid.Should().BeTrue();
        nonPopResult.Results.TotalCount.Should().BeLessThan(allSongsResult.Results.TotalCount);
    }

    [Fact(Skip = "Regex queries not reliably translatable with InMemory provider")]
    public async Task SearchAlbumsAsync_AlbumRegexAcoustic_ReturnsLostAcoustic()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        var result = await service.SearchAlbumsAsync("album:/.*Acoustic.*/i", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.TotalCount.Should().Be(1);
        result.Results.Data.First().Name.Should().Contain("Acoustic");
    }

    #endregion

    private sealed class TestDbContextFactory(DbContextOptions<MelodeeDbContext> options)
        : IDbContextFactory<MelodeeDbContext>
    {
        public MelodeeDbContext CreateDbContext() => new(options);

        public Task<MelodeeDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new MelodeeDbContext(options));
    }
}

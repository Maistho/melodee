using FluentAssertions;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Mql;
using Melodee.Mql.Interfaces;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Album = Melodee.Common.Data.Models.Album;
using Artist = Melodee.Common.Data.Models.Artist;
using Song = Melodee.Common.Data.Models.Song;

namespace Melodee.Tests.Blazor.Services;

/// <summary>
/// Unit tests for MqlSearchService.
/// Tests validation and query execution functionality.
/// </summary>
public class MqlSearchServiceTests
{
    private readonly IMqlValidator _validator;
    private readonly string _databaseName;
    private readonly DbContextOptions<MelodeeDbContext> _options;

    public MqlSearchServiceTests()
    {
        _databaseName = $"MqlSearchTestDb_{Guid.NewGuid()}";
        _options = new DbContextOptionsBuilder<MelodeeDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        _validator = new MqlValidator();

        SeedTestData();
    }

    private MqlSearchService CreateService()
    {
        return new MqlSearchService(new TestDbContextFactory(_options), _validator);
    }

    private void SeedTestData()
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

        var beatles = CreateArtist(1, "The Beatles", library);
        var pinkFloyd = CreateArtist(2, "Pink Floyd", library);
        var queen = CreateArtist(3, "Queen", library);
        context.Artists.AddRange(beatles, pinkFloyd, queen);

        var abbeyRoad = CreateAlbum(1, "Abbey Road", beatles, new LocalDate(1969, 9, 26));
        var darkSide = CreateAlbum(2, "The Dark Side of the Moon", pinkFloyd, new LocalDate(1973, 3, 1));
        var nightAtOpera = CreateAlbum(3, "A Night at the Opera", queen, new LocalDate(1975, 11, 21));
        context.Albums.AddRange(abbeyRoad, darkSide, nightAtOpera);

        var songs = new[]
        {
            CreateSong(1, "Come Together", abbeyRoad, 1, bpm: 82),
            CreateSong(2, "Something", abbeyRoad, 2, bpm: 66),
            CreateSong(3, "Here Comes the Sun", abbeyRoad, 7, bpm: 129),
            CreateSong(4, "Breathe", darkSide, 1, bpm: 63),
            CreateSong(5, "Time", darkSide, 2, bpm: 68),
            CreateSong(6, "Money", darkSide, 3, bpm: 126),
            CreateSong(7, "Bohemian Rhapsody", nightAtOpera, 1, bpm: 72),
            CreateSong(8, "You're My Best Friend", nightAtOpera, 2, bpm: 118),
            CreateSong(9, "Her Majesty", abbeyRoad, 17, bpm: 120, durationMs: 23000), // 23 seconds - short hidden track
        };
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

    private static Song CreateSong(int id, string title, Album album, int songNumber, int bpm = 120, double durationMs = 180000)
    {
        return new Song
        {
            Id = id,
            AlbumId = album.Id,
            Album = album,
            Title = title,
            TitleNormalized = title.ToUpperInvariant(),
            SongNumber = songNumber,
            FileName = $"{title.Replace(" ", "_").ToLowerInvariant()}.flac",
            FileSize = 1000000,
            FileHash = $"hash_{id}",
            Duration = durationMs,
            SamplingRate = 44100,
            BitRate = 320,
            BitDepth = 16,
            BPM = bpm,
            ContentType = "audio/flac",
            CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0),
            Genres = []
        };
    }

    #region ValidateQuery Tests

    [Fact]
    public void ValidateQuery_ValidSongQuery_ReturnsValid()
    {
        var service = CreateService();
        var result = service.ValidateQuery("artist:Beatles", "songs");

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateQuery_ValidAlbumQuery_ReturnsValid()
    {
        var service = CreateService();
        var result = service.ValidateQuery("name:Abbey", "albums");

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateQuery_ValidArtistQuery_ReturnsValid()
    {
        var service = CreateService();
        var result = service.ValidateQuery("name:Beatles", "artists");

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateQuery_EmptyQuery_ReturnsInvalid()
    {
        var service = CreateService();
        var result = service.ValidateQuery("", "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateQuery_UnknownField_ReturnsInvalid()
    {
        var service = CreateService();
        var result = service.ValidateQuery("unknownfield:value", "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("unknownfield", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateQuery_UnbalancedParentheses_ReturnsInvalid()
    {
        var service = CreateService();
        var result = service.ValidateQuery("(artist:Beatles", "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateQuery_ComplexBooleanQuery_ReturnsValid()
    {
        var service = CreateService();
        var result = service.ValidateQuery("(artist:Beatles OR artist:Queen) AND year:>=1970", "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateQuery_RangeExpression_ReturnsValid()
    {
        var service = CreateService();
        var result = service.ValidateQuery("year:1970-1980", "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateQuery_ComparisonOperators_ReturnsValid()
    {
        var service = CreateService();

        service.ValidateQuery("bpm:>100", "songs").IsValid.Should().BeTrue();
        service.ValidateQuery("bpm:>=100", "songs").IsValid.Should().BeTrue();
        service.ValidateQuery("bpm:<100", "songs").IsValid.Should().BeTrue();
        service.ValidateQuery("bpm:<=100", "songs").IsValid.Should().BeTrue();
        service.ValidateQuery("bpm:=100", "songs").IsValid.Should().BeTrue();
        service.ValidateQuery("bpm:!=100", "songs").IsValid.Should().BeTrue();
    }

    #endregion

    #region SearchSongsAsync Tests

    [Fact]
    public async Task SearchSongsAsync_EmptyQuery_ReturnsError()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.SearchSongsAsync("", userId: 1, paging);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Results.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchSongsAsync_InvalidSyntax_ReturnsErrors()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.SearchSongsAsync("(artist:Beatles", userId: 1, paging);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Results.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchSongsAsync_UnknownField_ReturnsError()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.SearchSongsAsync("unknownfield:value", userId: 1, paging);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchSongsAsync_ValidQuery_ReturnsResults()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        // Free-text search uses Contains for substring matching across title, artist, album
        var result = await service.SearchSongsAsync("Beatles", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Results.Data.Should().NotBeEmpty();
        result.Results.Data.Should().OnlyContain(s => s.Album.Artist.Name == "The Beatles");
    }

    [Fact]
    public async Task SearchSongsAsync_BpmFilter_ReturnsMatchingSongs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.SearchSongsAsync("bpm:>100", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.Data.Should().NotBeEmpty();
        result.Results.Data.Should().OnlyContain(s => s.BPM > 100);
    }

    [Fact]
    public async Task SearchSongsAsync_TitleSearch_ReturnsMatchingSongs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        // Free-text search matches against title using Contains
        var result = await service.SearchSongsAsync("Sun", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.Data.Should().NotBeEmpty();
        result.Results.Data.Should().OnlyContain(s => s.Title.Contains("Sun", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchSongsAsync_NoMatches_ReturnsEmptyData()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.SearchSongsAsync("artist:Nonexistent", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Results.Data.Should().BeEmpty();
        result.Results.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchSongsAsync_BooleanOr_ReturnsMatchingSongs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        // Use free-text search with OR to match multiple artists
        var result = await service.SearchSongsAsync("Beatles OR Queen", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.Data.Should().NotBeEmpty();
        result.Results.Data.Should().OnlyContain(s =>
            s.Album.Artist.Name == "The Beatles" || s.Album.Artist.Name == "Queen");
    }

    [Fact]
    public async Task SearchSongsAsync_DurationLessThan60Seconds_ReturnsShortSongs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        // duration:<60 means songs shorter than 60 seconds
        // "Her Majesty" is 23 seconds (23000ms)
        var result = await service.SearchSongsAsync("duration:<60", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.Data.Should().HaveCount(1);
        result.Results.Data.First().Title.Should().Be("Her Majesty");
    }

    [Fact]
    public async Task SearchSongsAsync_DurationGreaterThan60Seconds_ReturnsLongSongs()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        // duration:>60 means songs longer than 60 seconds
        // All songs except "Her Majesty" are 180 seconds
        var result = await service.SearchSongsAsync("duration:>60", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.Data.Should().HaveCount(8);
        result.Results.Data.Should().NotContain(s => s.Title == "Her Majesty");
    }

    [Fact]
    public async Task SearchSongsAsync_DurationRange_ReturnsSongsInRange()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        // duration:20-30 means songs between 20 and 30 seconds (MQL uses hyphen for ranges)
        // "Her Majesty" is 23 seconds
        var result = await service.SearchSongsAsync("duration:20-30", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.Data.Should().HaveCount(1);
        result.Results.Data.First().Title.Should().Be("Her Majesty");
    }

    #endregion

    #region SearchAlbumsAsync Tests

    [Fact]
    public async Task SearchAlbumsAsync_EmptyQuery_ReturnsError()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.SearchAlbumsAsync("", userId: 1, paging);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Results.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAlbumsAsync_InvalidSyntax_ReturnsErrors()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.SearchAlbumsAsync("(name:Abbey", userId: 1, paging);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Results.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAlbumsAsync_ValidQuery_ReturnsResults()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        // Free-text search matches against album name and artist name using Contains
        var result = await service.SearchAlbumsAsync("Abbey", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Results.Data.Should().NotBeEmpty();
        result.Results.Data.Should().OnlyContain(a => a.Name.Contains("Abbey", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchAlbumsAsync_NameSearch_ReturnsMatchingAlbums()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        // Free-text search matches against album name using Contains
        var result = await service.SearchAlbumsAsync("Dark", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.Data.Should().NotBeEmpty();
        result.Results.Data.Should().OnlyContain(a => a.Name.Contains("Dark", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchAlbumsAsync_YearFilter_ReturnsMatchingAlbums()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.SearchAlbumsAsync("year:>=1970", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.Data.Should().NotBeEmpty();
        result.Results.Data.Should().OnlyContain(a => a.ReleaseDate.Year >= 1970);
    }

    [Fact]
    public async Task SearchAlbumsAsync_NoMatches_ReturnsEmptyData()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.SearchAlbumsAsync("artist:Nonexistent", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Results.Data.Should().BeEmpty();
        result.Results.TotalCount.Should().Be(0);
    }

    #endregion

    #region SearchArtistsAsync Tests

    [Fact]
    public async Task SearchArtistsAsync_EmptyQuery_ReturnsError()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.SearchArtistsAsync("", userId: 1, paging);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Results.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchArtistsAsync_InvalidSyntax_ReturnsErrors()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.SearchArtistsAsync("(name:Beatles", userId: 1, paging);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Results.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchArtistsAsync_ValidQuery_ReturnsResults()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        // Free-text search matches against artist name using Contains
        var result = await service.SearchArtistsAsync("Beatles", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Results.Data.Should().NotBeEmpty();
        result.Results.Data.Should().OnlyContain(a => a.Name.Contains("Beatles", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchArtistsAsync_PartialName_ReturnsMatchingArtists()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        // Free-text search with partial name matches using Contains
        var result = await service.SearchArtistsAsync("Pink", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Results.Data.Should().NotBeEmpty();
        result.Results.Data.Should().OnlyContain(a => a.Name.Contains("Pink", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchArtistsAsync_NoMatches_ReturnsEmptyData()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.SearchArtistsAsync("name:Nonexistent", userId: 1, paging);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Results.Data.Should().BeEmpty();
        result.Results.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchArtistsAsync_UnknownField_ReturnsError()
    {
        var service = CreateService();
        var paging = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.SearchArtistsAsync("unknownfield:value", userId: 1, paging);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
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

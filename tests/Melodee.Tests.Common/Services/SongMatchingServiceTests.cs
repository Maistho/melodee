using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Services;
using Melodee.Common.Services.Parsing;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Core;

namespace Melodee.Tests.Common.Services;

public class SongMatchingServiceTests : IDisposable
{
    private readonly MelodeeDbContext _context;
    private readonly IDbContextFactory<MelodeeDbContext> _contextFactory;
    private readonly SongMatchingService _service;

    public SongMatchingServiceTests()
    {
        var options = new DbContextOptionsBuilder<MelodeeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MelodeeDbContext(options);
        _contextFactory = new TestDbContextFactory(_context);
        
        var cacheManager = new Caching.FakeCacheManager(Logger.None, TimeSpan.FromMinutes(5), new Serialization.Serializer());
        _service = new SongMatchingService(Logger.None, cacheManager, _contextFactory);

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Create test library
        var library = new Library
        {
            Name = "Test Library",
            Path = "/music",
            Type = (int)LibraryType.Storage,
            CreatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant()
        };
        _context.Libraries.Add(library);

        // Create test artists
        var artist1 = new Artist
        {
            Name = "Pink Floyd",
            NameNormalized = "pinkfloyd",
            CreatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant()
        };
        var artist2 = new Artist
        {
            Name = "The Beatles",
            NameNormalized = "thebeatles",
            CreatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant()
        };
        _context.Artists.AddRange(artist1, artist2);

        // Create test albums
        var album1 = new Album
        {
            ArtistId = 1,
            Title = "The Wall",
            TitleNormalized = "thewall",
            CreatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant()
        };
        var album2 = new Album
        {
            ArtistId = 2,
            Title = "Abbey Road",
            TitleNormalized = "abbeyroad",
            CreatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant()
        };
        _context.Albums.AddRange(album1, album2);

        // Create test songs
        var song1 = new Song
        {
            AlbumId = 1,
            Title = "Comfortably Numb",
            TitleNormalized = "comfortablynumb",
            FilePath = "/music/Pink Floyd/The Wall/01 - Comfortably Numb.flac",
            Duration = 380000,
            CreatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant()
        };
        var song2 = new Song
        {
            AlbumId = 1,
            Title = "Another Brick in the Wall",
            TitleNormalized = "anotherbrickinthewall",
            FilePath = "/music/Pink Floyd/The Wall/02 - Another Brick in the Wall.flac",
            Duration = 238000,
            CreatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant()
        };
        var song3 = new Song
        {
            AlbumId = 2,
            Title = "Come Together",
            TitleNormalized = "cometogether",
            FilePath = "/music/The Beatles/Abbey Road/01 - Come Together.mp3",
            Duration = 259000,
            CreatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant()
        };
        _context.Songs.AddRange(song1, song2, song3);

        _context.SaveChanges();
    }

    [Fact]
    public async Task MatchEntryAsync_ExactPathMatch_ReturnsHighConfidence()
    {
        var entry = new M3UEntry
        {
            RawReference = "/music/Pink Floyd/The Wall/01 - Comfortably Numb.flac",
            NormalizedReference = "/music/Pink Floyd/The Wall/01 - Comfortably Numb.flac",
            FileName = "01 - Comfortably Numb.flac",
            SortOrder = 0
        };

        var result = await _service.MatchEntryAsync(entry, "/music", CancellationToken.None);

        Assert.NotNull(result.Song);
        Assert.Equal("Comfortably Numb", result.Song.Title);
        Assert.Equal(1.0, result.Confidence);
        Assert.Equal(SongMatchStrategy.ExactPath, result.MatchStrategy);
    }

    [Fact]
    public async Task MatchEntryAsync_RelativePathMatch_ReturnsHighConfidence()
    {
        var entry = new M3UEntry
        {
            RawReference = "Pink Floyd/The Wall/01 - Comfortably Numb.flac",
            NormalizedReference = "Pink Floyd/The Wall/01 - Comfortably Numb.flac",
            FileName = "01 - Comfortably Numb.flac",
            ArtistFolder = "Pink Floyd",
            AlbumFolder = "The Wall",
            SortOrder = 0
        };

        var result = await _service.MatchEntryAsync(entry, "/music", CancellationToken.None);

        Assert.NotNull(result.Song);
        Assert.Equal("Comfortably Numb", result.Song.Title);
        Assert.True(result.Confidence >= 0.8);
    }

    [Fact]
    public async Task MatchEntryAsync_FilenameWithHints_ReturnsMatch()
    {
        var entry = new M3UEntry
        {
            RawReference = "01 - Comfortably Numb.flac",
            NormalizedReference = "01 - Comfortably Numb.flac",
            FileName = "01 - Comfortably Numb.flac",
            ArtistFolder = "Pink Floyd",
            AlbumFolder = "The Wall",
            SortOrder = 0
        };

        var result = await _service.MatchEntryAsync(entry, "/music", CancellationToken.None);

        Assert.NotNull(result.Song);
        Assert.Equal("Comfortably Numb", result.Song.Title);
        Assert.True(result.Confidence > 0.0);
    }

    [Fact]
    public async Task MatchEntryAsync_NoMatch_ReturnsNull()
    {
        var entry = new M3UEntry
        {
            RawReference = "NonExistent/Album/Song.mp3",
            NormalizedReference = "NonExistent/Album/Song.mp3",
            FileName = "Song.mp3",
            ArtistFolder = "NonExistent",
            AlbumFolder = "Album",
            SortOrder = 0
        };

        var result = await _service.MatchEntryAsync(entry, "/music", CancellationToken.None);

        Assert.Null(result.Song);
        Assert.Equal(0.0, result.Confidence);
        Assert.Equal(SongMatchStrategy.None, result.MatchStrategy);
    }

    [Fact]
    public async Task MatchEntryAsync_WindowsPathWithBackslashes_Normalizes()
    {
        var entry = new M3UEntry
        {
            RawReference = "D:\\Music\\Pink Floyd\\The Wall\\01 - Comfortably Numb.flac",
            NormalizedReference = "D:/Music/Pink Floyd/The Wall/01 - Comfortably Numb.flac",
            FileName = "01 - Comfortably Numb.flac",
            ArtistFolder = "Pink Floyd",
            AlbumFolder = "The Wall",
            SortOrder = 0
        };

        var result = await _service.MatchEntryAsync(entry, "/music", CancellationToken.None);

        // Should still find the song even though the drive letter doesn't match
        Assert.NotNull(result.Song);
        Assert.Equal("Comfortably Numb", result.Song.Title);
    }

    [Fact]
    public async Task MatchEntryAsync_URLEncodedPath_Decodes()
    {
        var entry = new M3UEntry
        {
            RawReference = "/music/The%20Beatles/Abbey%20Road/01%20-%20Come%20Together.mp3",
            NormalizedReference = "/music/The Beatles/Abbey Road/01 - Come Together.mp3",
            FileName = "01 - Come Together.mp3",
            ArtistFolder = "The Beatles",
            AlbumFolder = "Abbey Road",
            SortOrder = 0
        };

        var result = await _service.MatchEntryAsync(entry, "/music", CancellationToken.None);

        Assert.NotNull(result.Song);
        Assert.Equal("Come Together", result.Song.Title);
    }

    [Fact]
    public async Task MatchEntryAsync_MultipleMatches_ReturnsBestMatch()
    {
        // Add a duplicate song with slightly different path
        var duplicateSong = new Song
        {
            AlbumId = 1,
            Title = "Comfortably Numb",
            TitleNormalized = "comfortablynumb",
            FilePath = "/music/Pink Floyd/The Wall/Comfortably Numb.flac",
            Duration = 380000,
            CreatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant()
        };
        _context.Songs.Add(duplicateSong);
        await _context.SaveChangesAsync();

        var entry = new M3UEntry
        {
            RawReference = "Pink Floyd/The Wall/Comfortably Numb.flac",
            NormalizedReference = "Pink Floyd/The Wall/Comfortably Numb.flac",
            FileName = "Comfortably Numb.flac",
            ArtistFolder = "Pink Floyd",
            AlbumFolder = "The Wall",
            SortOrder = 0
        };

        var result = await _service.MatchEntryAsync(entry, "/music", CancellationToken.None);

        Assert.NotNull(result.Song);
        Assert.Equal("Comfortably Numb", result.Song.Title);
        // Should return a match even when multiple exist
        Assert.True(result.Confidence > 0.0);
    }

    [Fact]
    public async Task MatchEntryAsync_NullLibraryPath_StillMatches()
    {
        var entry = new M3UEntry
        {
            RawReference = "01 - Comfortably Numb.flac",
            NormalizedReference = "01 - Comfortably Numb.flac",
            FileName = "01 - Comfortably Numb.flac",
            ArtistFolder = "Pink Floyd",
            AlbumFolder = "The Wall",
            SortOrder = 0
        };

        var result = await _service.MatchEntryAsync(entry, null, CancellationToken.None);

        // Should still attempt filename and metadata matching
        Assert.NotNull(result.Song);
        Assert.Equal("Comfortably Numb", result.Song.Title);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    private class TestDbContextFactory : IDbContextFactory<MelodeeDbContext>
    {
        private readonly MelodeeDbContext _context;

        public TestDbContextFactory(MelodeeDbContext context)
        {
            _context = context;
        }

        public MelodeeDbContext CreateDbContext()
        {
            return _context;
        }

        public async Task<MelodeeDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_context);
        }
    }
}

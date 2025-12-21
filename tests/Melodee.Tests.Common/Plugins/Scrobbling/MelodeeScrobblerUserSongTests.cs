using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;
using Melodee.Common.Plugins.Scrobbling;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using DbAlbum = Melodee.Common.Data.Models.Album;
using DbArtist = Melodee.Common.Data.Models.Artist;
using DbSong = Melodee.Common.Data.Models.Song;
using DbUser = Melodee.Common.Data.Models.User;

namespace Melodee.Tests.Common.Plugins.Scrobbling;

public class MelodeeScrobblerUserSongTests : IDisposable
{
    private readonly MelodeeDbContext _dbContext;
    private readonly MelodeeScrobbler _scrobbler;
    private readonly UserInfo _testUser;
    private readonly DbContextOptions<MelodeeDbContext> _dbOptions;

    public MelodeeScrobblerUserSongTests()
    {
        _dbOptions = new DbContextOptionsBuilder<MelodeeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new MelodeeDbContext(_dbOptions);
        _dbContext.Database.EnsureCreated();

        var testArtist = new DbArtist
        {
            Id = 1,
            Name = "Test Artist",
            NameNormalized = "test artist",
            LibraryId = 1,
            ApiKey = Guid.NewGuid(),
            Directory = "/test/artist",
            SortOrder = 1,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        var testAlbum = new DbAlbum
        {
            Id = 1,
            ArtistId = 1,
            Name = "Test Album",
            NameNormalized = "test album",
            ApiKey = Guid.NewGuid(),
            Directory = "/test/album",
            SortOrder = 1,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        var testSong = new DbSong
        {
            Id = 1,
            AlbumId = 1,
            Title = "Test Song",
            TitleNormalized = "test song",
            ApiKey = Guid.NewGuid(),
            Duration = 180000,
            SongNumber = 1,
            FileName = "test.mp3",
            FileSize = 5000000,
            FileHash = "testhash",
            SamplingRate = 44100,
            BitRate = 320,
            BitDepth = 16,
            BPM = 120,
            ContentType = "audio/mpeg",
            SortOrder = 1,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        var testUserEntity = new DbUser
        {
            Id = 1,
            UserName = "testuser",
            UserNameNormalized = "testuser",
            Email = "test@test.com",
            EmailNormalized = "test@test.com",
            PublicKey = "testkey",
            PasswordEncrypted = "encrypted",
            ApiKey = Guid.NewGuid(),
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        _dbContext.Artists.Add(testArtist);
        _dbContext.Albums.Add(testAlbum);
        _dbContext.Songs.Add(testSong);
        _dbContext.Users.Add(testUserEntity);
        _dbContext.SaveChanges();

        _testUser = new UserInfo(
            Id: 1,
            ApiKey: testUserEntity.ApiKey,
            UserName: "testuser",
            Email: "test@test.com",
            PublicKey: "testkey",
            PasswordEncrypted: "encrypted",
            TimeZoneId: "UTC"
        );

        var contextFactory = new TestDbContextFactory(_dbOptions);
        AlbumService albumService = null!;
        var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        _scrobbler = new MelodeeScrobbler(albumService, contextFactory, logger);
    }

    [Fact]
    public async Task NowPlaying_CreatesUserSong_WhenDoesNotExist()
    {
        var scrobbleInfo = CreateScrobbleInfo(secondsPlayed: 10);

        var userSongBefore = await _dbContext.UserSongs
            .FirstOrDefaultAsync(us => us.UserId == _testUser.Id && us.SongId == 1);
        Assert.Null(userSongBefore);

        var result = await _scrobbler.NowPlaying(_testUser, scrobbleInfo, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var userSongAfter = await _dbContext.UserSongs
            .FirstOrDefaultAsync(us => us.UserId == _testUser.Id && us.SongId == 1);
        Assert.NotNull(userSongAfter);
        Assert.Equal(_testUser.Id, userSongAfter.UserId);
        Assert.Equal(1, userSongAfter.SongId);
    }

    [Fact]
    public async Task NowPlaying_DoesNotDuplicateUserSong_WhenAlreadyExists()
    {
        var existingUserSong = new Melodee.Common.Data.Models.UserSong
        {
            UserId = _testUser.Id,
            SongId = 1,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        _dbContext.UserSongs.Add(existingUserSong);
        await _dbContext.SaveChangesAsync();

        var scrobbleInfo = CreateScrobbleInfo(secondsPlayed: 10);

        var result = await _scrobbler.NowPlaying(_testUser, scrobbleInfo, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var userSongCount = await _dbContext.UserSongs
            .CountAsync(us => us.UserId == _testUser.Id && us.SongId == 1);
        Assert.Equal(1, userSongCount);
    }

    [Fact]
    public async Task NowPlaying_CreatesUserSongPlayHistory_WithIsNowPlayingTrue()
    {
        var scrobbleInfo = CreateScrobbleInfo(secondsPlayed: 10);

        var result = await _scrobbler.NowPlaying(_testUser, scrobbleInfo, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var userSong = await _dbContext.UserSongs
            .FirstOrDefaultAsync(us => us.UserId == _testUser.Id && us.SongId == 1);
        Assert.NotNull(userSong);

        var playHistory = await _dbContext.UserSongPlayHistories
            .FirstOrDefaultAsync(h => h.UserId == _testUser.Id && h.SongId == 1);
        Assert.NotNull(playHistory);
        Assert.Equal(10, playHistory.SecondsPlayed);
        Assert.True(playHistory.IsNowPlaying);
        Assert.NotNull(playHistory.LastHeartbeatAt);
    }

    [Fact]
    public async Task NowPlaying_CreatesUserSongForMultipleUsers_Independently()
    {
        var user2 = new DbUser
        {
            Id = 2,
            UserName = "user2",
            UserNameNormalized = "user2",
            Email = "user2@test.com",
            EmailNormalized = "user2@test.com",
            PublicKey = "key2",
            PasswordEncrypted = "encrypted",
            ApiKey = Guid.NewGuid(),
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        _dbContext.Users.Add(user2);
        await _dbContext.SaveChangesAsync();

        var user2Info = new UserInfo(
            Id: 2,
            ApiKey: user2.ApiKey,
            UserName: "user2",
            Email: "user2@test.com",
            PublicKey: "key2",
            PasswordEncrypted: "encrypted",
            TimeZoneId: "UTC"
        );

        var scrobbleUser1 = CreateScrobbleInfo(secondsPlayed: 10);
        var scrobbleUser2 = CreateScrobbleInfo(secondsPlayed: 20);

        await _scrobbler.NowPlaying(_testUser, scrobbleUser1, CancellationToken.None);
        await _scrobbler.NowPlaying(user2Info, scrobbleUser2, CancellationToken.None);

        var user1Song = await _dbContext.UserSongs
            .FirstOrDefaultAsync(us => us.UserId == _testUser.Id && us.SongId == 1);
        var user2Song = await _dbContext.UserSongs
            .FirstOrDefaultAsync(us => us.UserId == 2 && us.SongId == 1);

        Assert.NotNull(user1Song);
        Assert.NotNull(user2Song);
        Assert.NotEqual(user1Song.Id, user2Song.Id);
    }

    [Fact]
    public async Task NowPlaying_ClearsPreviousNowPlaying_ForSameUser()
    {
        // Add a second song for this test
        var secondSong = new DbSong
        {
            Id = 2,
            AlbumId = 1,
            Title = "Second Test Song",
            TitleNormalized = "second test song",
            ApiKey = Guid.NewGuid(),
            Duration = 200000,
            SongNumber = 2,
            FileName = "test2.mp3",
            FileSize = 6000000,
            FileHash = "testhash2",
            SamplingRate = 44100,
            BitRate = 320,
            BitDepth = 16,
            BPM = 125,
            ContentType = "audio/mpeg",
            SortOrder = 2,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        _dbContext.Songs.Add(secondSong);
        await _dbContext.SaveChangesAsync();

        var scrobbleInfo1 = CreateScrobbleInfo(songId: 1, secondsPlayed: 10);
        await _scrobbler.NowPlaying(_testUser, scrobbleInfo1, CancellationToken.None);

        var firstPlayHistory = await _dbContext.UserSongPlayHistories
            .FirstOrDefaultAsync(h => h.UserId == _testUser.Id && h.SongId == 1);
        Assert.NotNull(firstPlayHistory);
        Assert.True(firstPlayHistory.IsNowPlaying);

        var scrobbleInfo2 = CreateScrobbleInfo(songId: 2, secondsPlayed: 20);
        await _scrobbler.NowPlaying(_testUser, scrobbleInfo2, CancellationToken.None);

        await _dbContext.Entry(firstPlayHistory).ReloadAsync();
        Assert.False(firstPlayHistory.IsNowPlaying);

        var nowPlayingCount = await _dbContext.UserSongPlayHistories
            .CountAsync(h => h.UserId == _testUser.Id && h.IsNowPlaying);
        Assert.Equal(1, nowPlayingCount);
    }

    private ScrobbleInfo CreateScrobbleInfo(int secondsPlayed, int songId = 1)
    {
        return new ScrobbleInfo(
            SongApiKey: Guid.NewGuid(),
            ArtistId: 1,
            AlbumId: 1,
            SongId: songId,
            SongTitle: $"Test Song {songId}",
            ArtistName: "Test Artist",
            IsRandomizedScrobble: false,
            AlbumTitle: "Test Album",
            SongDuration: 180,
            SongMusicBrainzId: null,
            SongNumber: songId,
            SongArtist: null,
            CreatedAt: SystemClock.Instance.GetCurrentInstant(),
            PlayerName: "Melodee",
            UserAgent: "Mozilla/5.0",
            IpAddress: "192.168.1.1",
            SecondsPlayed: secondsPlayed
        );
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private class TestDbContextFactory : IDbContextFactory<MelodeeDbContext>
    {
        private readonly DbContextOptions<MelodeeDbContext> _options;

        public TestDbContextFactory(DbContextOptions<MelodeeDbContext> options)
        {
            _options = options;
        }

        public MelodeeDbContext CreateDbContext()
        {
            return new MelodeeDbContext(_options);
        }

        public Task<MelodeeDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new MelodeeDbContext(_options));
        }
    }
}

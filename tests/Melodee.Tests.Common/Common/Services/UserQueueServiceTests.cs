using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Services;
using NodaTime;
using dbModels = Melodee.Common.Data.Models;

namespace Melodee.Tests.Common.Common.Services;

public class UserQueueServiceTests : ServiceTestBase
{
    private UserQueueService CreateUserQueueService()
    {
        return new UserQueueService(Logger, CacheManager, MockFactory(), GetUserService());
    }

    private async Task<(dbModels.User user, dbModels.Song song1, dbModels.Song song2)> CreateTestUserAndSongs()
    {
        await using var context = await MockFactory().CreateDbContextAsync();

        var user = new dbModels.User
        {
            UserName = "testuser",
            UserNameNormalized = "TESTUSER",
            Email = "test@example.com",
            EmailNormalized = "TEST@EXAMPLE.COM",
            PublicKey = "testkey",
            PasswordEncrypted = "encrypted",
            ApiKey = Guid.NewGuid(),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            IsAdmin = false,
            IsLocked = false
        };
        context.Users.Add(user);

        var library = await context.Libraries.FirstOrDefaultAsync(x => x.Type == (int)LibraryType.Storage);
        if (library == null)
        {
            library = new dbModels.Library
            {
                Name = "Test Library",
                Path = "/test/path",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
        }

        var artist = new dbModels.Artist
        {
            Name = "Test Artist",
            NameNormalized = "TEST ARTIST",
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.Artists.Add(artist);

        var album = new dbModels.Album
        {
            Name = "Test Album",
            NameNormalized = "TEST ALBUM",
            ApiKey = Guid.NewGuid(),
            ArtistId = 0,
            LibraryId = library.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.Albums.Add(album);

        await context.SaveChangesAsync();

        artist.Id = artist.Id;
        album.ArtistId = artist.Id;
        await context.SaveChangesAsync();

        var song1 = new dbModels.Song
        {
            Title = "Test Song 1",
            TitleNormalized = "TEST SONG 1",
            ApiKey = Guid.NewGuid(),
            AlbumId = album.Id,
            FileHash = "hash1",
            Duration = 180000,
            FileSize = 1024,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        var song2 = new dbModels.Song
        {
            Title = "Test Song 2",
            TitleNormalized = "TEST SONG 2",
            ApiKey = Guid.NewGuid(),
            AlbumId = album.Id,
            FileHash = "hash2",
            Duration = 200000,
            FileSize = 2048,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.Songs.AddRange(song1, song2);
        await context.SaveChangesAsync();

        return (user, song1, song2);
    }

    [Fact]
    public async Task GetPlayQueueForUserAsync_WithNoQueue_ReturnsNull()
    {
        var service = CreateUserQueueService();

        await using var context = await MockFactory().CreateDbContextAsync();
        var user = new dbModels.User
        {
            UserName = "emptyuser",
            UserNameNormalized = "EMPTYUSER",
            Email = "empty@example.com",
            EmailNormalized = "EMPTY@EXAMPLE.COM",
            PublicKey = "testkey",
            PasswordEncrypted = "encrypted",
            ApiKey = Guid.NewGuid(),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            IsAdmin = false,
            IsLocked = false
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = await service.GetPlayQueueForUserAsync("emptyuser");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPlayQueueForUserAsync_WithNonexistentUser_ReturnsNull()
    {
        var service = CreateUserQueueService();

        var result = await service.GetPlayQueueForUserAsync("nonexistentuser");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPlayQueueByUserIdAsync_WithValidUserId_ReturnsQueue()
    {
        var service = CreateUserQueueService();
        var (user, song1, song2) = await CreateTestUserAndSongs();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var queue1 = new dbModels.PlayQueue
            {
                PlayQueId = 1,
                UserId = user.Id,
                SongId = song1.Id,
                SongApiKey = song1.ApiKey,
                IsCurrentSong = true,
                Position = 30.0,
                ChangedBy = user.UserName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            var queue2 = new dbModels.PlayQueue
            {
                PlayQueId = 2,
                UserId = user.Id,
                SongId = song2.Id,
                SongApiKey = song2.ApiKey,
                IsCurrentSong = false,
                Position = 0,
                ChangedBy = user.UserName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.PlayQues.AddRange(queue1, queue2);
            await context.SaveChangesAsync();
        }

        var result = await service.GetPlayQueueByUserIdAsync(user.Id);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Songs.Length);
        Assert.Equal(song1.ApiKey, result.Data.CurrentSongApiKey);
        Assert.Equal(30.0, result.Data.Position);
    }

    [Fact]
    public async Task GetPlayQueueByUserIdAsync_WithInvalidUserId_ReturnsNotFound()
    {
        var service = CreateUserQueueService();

        var result = await service.GetPlayQueueByUserIdAsync(99999);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.NotFound, result.Type);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetPlayQueueByUserIdAsync_WithEmptyQueue_ReturnsEmptyQueue()
    {
        var service = CreateUserQueueService();

        await using var context = await MockFactory().CreateDbContextAsync();
        var user = new dbModels.User
        {
            UserName = "emptyqueueuser",
            UserNameNormalized = "EMPTYQUEUEUSER",
            Email = "emptyqueue@example.com",
            EmailNormalized = "EMPTYQUEUE@EXAMPLE.COM",
            PublicKey = "testkey",
            PasswordEncrypted = "encrypted",
            ApiKey = Guid.NewGuid(),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            IsAdmin = false,
            IsLocked = false
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = await service.GetPlayQueueByUserIdAsync(user.Id);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Songs);
        Assert.Null(result.Data.CurrentSongApiKey);
    }

    [Fact]
    public async Task SavePlayQueueByUserIdAsync_WithValidData_SavesQueue()
    {
        var service = CreateUserQueueService();
        var (user, song1, song2) = await CreateTestUserAndSongs();

        var songApiKeys = new[] { song1.ApiKey, song2.ApiKey };
        var result = await service.SavePlayQueueByUserIdAsync(user.Id, songApiKeys, song1.ApiKey, 15.0, user.UserName);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        await using var context = await MockFactory().CreateDbContextAsync();
        var savedQueue = await context.PlayQues.Where(pq => pq.UserId == user.Id).ToArrayAsync();
        Assert.Equal(2, savedQueue.Length);
        Assert.True(savedQueue.Any(pq => pq.SongApiKey == song1.ApiKey && pq.IsCurrentSong));
    }

    [Fact]
    public async Task SavePlayQueueByUserIdAsync_WithInvalidUserId_ReturnsNotFound()
    {
        var service = CreateUserQueueService();

        var result = await service.SavePlayQueueByUserIdAsync(99999, [Guid.NewGuid()], null, null, "test");

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.NotFound, result.Type);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task SavePlayQueueByUserIdAsync_ClearsExistingQueue_BeforeSaving()
    {
        var service = CreateUserQueueService();
        var (user, song1, song2) = await CreateTestUserAndSongs();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var oldQueue = new dbModels.PlayQueue
            {
                PlayQueId = 1,
                UserId = user.Id,
                SongId = song1.Id,
                SongApiKey = song1.ApiKey,
                IsCurrentSong = true,
                Position = 0,
                ChangedBy = user.UserName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.PlayQues.Add(oldQueue);
            await context.SaveChangesAsync();
        }

        var newSongApiKeys = new[] { song2.ApiKey };
        await service.SavePlayQueueByUserIdAsync(user.Id, newSongApiKeys, song2.ApiKey, 0, user.UserName);

        await using var context2 = await MockFactory().CreateDbContextAsync();
        var queue = await context2.PlayQues.Where(pq => pq.UserId == user.Id).ToArrayAsync();
        Assert.Single(queue);
        Assert.Equal(song2.ApiKey, queue[0].SongApiKey);
    }

    [Fact]
    public async Task SavePlayQueueByUserIdAsync_WithEmptySongList_ClearsQueue()
    {
        var service = CreateUserQueueService();
        var (user, song1, _) = await CreateTestUserAndSongs();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var queue = new dbModels.PlayQueue
            {
                PlayQueId = 1,
                UserId = user.Id,
                SongId = song1.Id,
                SongApiKey = song1.ApiKey,
                IsCurrentSong = true,
                Position = 0,
                ChangedBy = user.UserName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.PlayQues.Add(queue);
            await context.SaveChangesAsync();
        }

        var result = await service.SavePlayQueueByUserIdAsync(user.Id, [], null, null, user.UserName);

        Assert.True(result.IsSuccess);

        await using var context2 = await MockFactory().CreateDbContextAsync();
        var savedQueue = await context2.PlayQues.Where(pq => pq.UserId == user.Id).ToArrayAsync();
        Assert.Empty(savedQueue);
    }

    [Fact]
    public async Task ClearPlayQueueByUserIdAsync_WithExistingQueue_ClearsQueue()
    {
        var service = CreateUserQueueService();
        var (user, song1, _) = await CreateTestUserAndSongs();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var queue = new dbModels.PlayQueue
            {
                PlayQueId = 1,
                UserId = user.Id,
                SongId = song1.Id,
                SongApiKey = song1.ApiKey,
                IsCurrentSong = true,
                Position = 0,
                ChangedBy = user.UserName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.PlayQues.Add(queue);
            await context.SaveChangesAsync();
        }

        var result = await service.ClearPlayQueueByUserIdAsync(user.Id);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        await using var context2 = await MockFactory().CreateDbContextAsync();
        var cleared = await context2.PlayQues.Where(pq => pq.UserId == user.Id).ToArrayAsync();
        Assert.Empty(cleared);
    }

    [Fact]
    public async Task ClearPlayQueueByUserIdAsync_WithNoQueue_ReturnsSuccess()
    {
        var service = CreateUserQueueService();

        await using var context = await MockFactory().CreateDbContextAsync();
        var user = new dbModels.User
        {
            UserName = "noclearuser",
            UserNameNormalized = "NOCLEARUSER",
            Email = "noclear@example.com",
            EmailNormalized = "NOCLEAR@EXAMPLE.COM",
            PublicKey = "testkey",
            PasswordEncrypted = "encrypted",
            ApiKey = Guid.NewGuid(),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            IsAdmin = false,
            IsLocked = false
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = await service.ClearPlayQueueByUserIdAsync(user.Id);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task SavePlayQueueByUserIdAsync_WithPosition_SetsCurrentPosition()
    {
        var service = CreateUserQueueService();
        var (user, song1, song2) = await CreateTestUserAndSongs();

        var songApiKeys = new[] { song1.ApiKey, song2.ApiKey };
        await service.SavePlayQueueByUserIdAsync(user.Id, songApiKeys, song1.ApiKey, 45.5, user.UserName);

        await using var context = await MockFactory().CreateDbContextAsync();
        var currentSong = await context.PlayQues
            .FirstOrDefaultAsync(pq => pq.UserId == user.Id && pq.IsCurrentSong);

        Assert.NotNull(currentSong);
        Assert.Equal(45.5, currentSong!.Position);
    }

    [Fact]
    public async Task SavePlayQueueByUserIdAsync_SkipsInvalidSongApiKeys()
    {
        var service = CreateUserQueueService();
        var (user, song1, _) = await CreateTestUserAndSongs();

        var invalidApiKey = Guid.NewGuid();
        var songApiKeys = new[] { song1.ApiKey, invalidApiKey };
        await service.SavePlayQueueByUserIdAsync(user.Id, songApiKeys, song1.ApiKey, 0, user.UserName);

        await using var context = await MockFactory().CreateDbContextAsync();
        var queue = await context.PlayQues.Where(pq => pq.UserId == user.Id).ToArrayAsync();
        Assert.Single(queue);
        Assert.Equal(song1.ApiKey, queue[0].SongApiKey);
    }

    [Fact]
    public async Task SavePlayQueueForUserAsync_WithNullApiIds_ClearsQueue()
    {
        var service = CreateUserQueueService();
        var (user, song1, _) = await CreateTestUserAndSongs();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var queue = new dbModels.PlayQueue
            {
                PlayQueId = 1,
                UserId = user.Id,
                SongId = song1.Id,
                SongApiKey = song1.ApiKey,
                IsCurrentSong = true,
                Position = 0,
                ChangedBy = user.UserName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.PlayQues.Add(queue);
            await context.SaveChangesAsync();
        }

        var result = await service.SavePlayQueueForUserAsync(user.UserName, null, null, null, null);

        Assert.True(result);

        await using var context2 = await MockFactory().CreateDbContextAsync();
        var cleared = await context2.PlayQues.Where(pq => pq.UserId == user.Id).ToArrayAsync();
        Assert.Empty(cleared);
    }

    [Fact]
    public async Task SavePlayQueueForUserAsync_WithInvalidUser_ReturnsFalse()
    {
        var service = CreateUserQueueService();

        var result = await service.SavePlayQueueForUserAsync("nonexistentuser", ["song:123"], null, null, null);

        Assert.False(result);
    }

    [Fact]
    public async Task SavePlayQueueByUserIdAsync_WithMultipleUsers_IsolatesQueues()
    {
        var service = CreateUserQueueService();
        var (user1, song1, song2) = await CreateTestUserAndSongs();

        await using var context = await MockFactory().CreateDbContextAsync();
        var user2 = new dbModels.User
        {
            UserName = "user2",
            UserNameNormalized = "USER2",
            Email = "user2@example.com",
            EmailNormalized = "USER2@EXAMPLE.COM",
            PublicKey = "testkey2",
            PasswordEncrypted = "encrypted2",
            ApiKey = Guid.NewGuid(),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            IsAdmin = false,
            IsLocked = false
        };
        context.Users.Add(user2);
        await context.SaveChangesAsync();

        await service.SavePlayQueueByUserIdAsync(user1.Id, [song1.ApiKey], null, null, user1.UserName);
        await service.SavePlayQueueByUserIdAsync(user2.Id, [song2.ApiKey], null, null, user2.UserName);

        var queue1 = await service.GetPlayQueueByUserIdAsync(user1.Id);
        var queue2 = await service.GetPlayQueueByUserIdAsync(user2.Id);

        Assert.Single(queue1.Data!.Songs);
        Assert.Single(queue2.Data!.Songs);
        Assert.Equal(song1.ApiKey, queue1.Data.Songs[0].ApiKey);
        Assert.Equal(song2.ApiKey, queue2.Data.Songs[0].ApiKey);
    }
}

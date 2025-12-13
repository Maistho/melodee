using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;

namespace Melodee.Tests.Common.Data.Models.Extensions;

public class BookmarkExtensionTests
{
    [Fact]
    public void ToCoverArtId_ShouldReturnBookmarkApiKeyWithPrefix()
    {
        // Arrange
        var bookmark = new Bookmark
        {
            ApiKey = Guid.NewGuid(),
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            UserId = 1,
            SongId = 1
        };

        // Act
        var result = bookmark.ToCoverArtId();

        // Assert
        Assert.StartsWith("bookmark_", result);
        Assert.Contains(bookmark.ApiKey.ToString(), result);
    }

    [Fact]
    public void ToApiKey_ShouldReturnBookmarkApiKeyWithPrefix()
    {
        // Arrange
        var bookmark = new Bookmark
        {
            ApiKey = Guid.NewGuid(),
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            UserId = 1,
            SongId = 1
        };

        // Act
        var result = bookmark.ToApiKey();

        // Assert
        Assert.StartsWith("bookmark_", result);
        Assert.Contains(bookmark.ApiKey.ToString(), result);
    }

    [Fact]
    public void ToApiBookmark_ShouldReturnValidApiBookmark()
    {
        // Arrange
        var artist = new Melodee.Common.Data.Models.Artist
        {
            Name = "Test Artist",
            NameNormalized = "testartist",
            Directory = "testartist",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            LibraryId = 1
        };

        var user = new Melodee.Common.Data.Models.User
        {
            UserName = "TestUser",
            UserNameNormalized = "testuser",
            Email = "test@example.com",
            EmailNormalized = "test@example.com",
            PublicKey = "testkey",
            PasswordEncrypted = "encrypted",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow)
        };

        var album = new Melodee.Common.Data.Models.Album
        {
            Name = "Test Album",
            NameNormalized = "testalbum",
            Directory = "testalbum",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            Artist = artist
        };

        var song = new Melodee.Common.Data.Models.Song
        {
            Album = album,
            Title = "Test Song",
            TitleNormalized = "testsong",
            SongNumber = 1,
            FileName = "test.mp3",
            FileSize = 1000,
            FileHash = "hash",
            Duration = 100,
            SamplingRate = 44100,
            BitRate = 320,
            BitDepth = 16,
            BPM = 120,
            ContentType = "audio/mpeg",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow)
        };

        var userSong = new Melodee.Common.Data.Models.UserSong
        {
            UserId = 1,
            SongId = 1,
            Rating = 5,
            PlayedCount = 1,
            LastPlayedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            IsStarred = true,
            IsHated = false
        };

        var bookmark = new Bookmark
        {
            Position = 100,
            User = user,
            Comment = "Test Comment",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            LastUpdatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            Song = song,
            UserId = 1,
            SongId = 1
        };

        // Add the user song to the song's collection
        song.UserSongs = [userSong];

        // Act
        var result = bookmark.ToApiBookmark();

        // Assert
        Assert.Equal(100, result.Position);
        Assert.Equal("TestUser", result.Username);
        Assert.Equal("Test Comment", result.Comment);
    }

    [Fact]
    public void ToApiBookmark_WithNullLastUpdatedAt_ShouldUseCreatedAt()
    {
        // Arrange
        var artist = new Melodee.Common.Data.Models.Artist
        {
            Name = "Test Artist",
            NameNormalized = "testartist",
            Directory = "testartist",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            LibraryId = 1
        };

        var user = new Melodee.Common.Data.Models.User
        {
            UserName = "TestUser",
            UserNameNormalized = "testuser",
            Email = "test@example.com",
            EmailNormalized = "test@example.com",
            PublicKey = "testkey",
            PasswordEncrypted = "encrypted",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow)
        };

        var album = new Melodee.Common.Data.Models.Album
        {
            Name = "Test Album",
            NameNormalized = "testalbum",
            Directory = "testalbum",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            Artist = artist
        };

        var song = new Melodee.Common.Data.Models.Song
        {
            Album = album,
            Title = "Test Song",
            TitleNormalized = "testsong",
            SongNumber = 1,
            FileName = "test.mp3",
            FileSize = 1000,
            FileHash = "hash",
            Duration = 100,
            SamplingRate = 44100,
            BitRate = 320,
            BitDepth = 16,
            BPM = 120,
            ContentType = "audio/mpeg",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow)
        };

        var userSong = new Melodee.Common.Data.Models.UserSong
        {
            UserId = 1,
            SongId = 1,
            Rating = 5,
            PlayedCount = 1,
            LastPlayedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            IsStarred = true,
            IsHated = false
        };

        var bookmark = new Bookmark
        {
            Position = 100,
            User = user,
            Comment = "Test Comment",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            LastUpdatedAt = null,
            Song = song,
            UserId = 1,
            SongId = 1
        };

        // Add the user song to the song's collection
        song.UserSongs = [userSong];

        // Act
        var result = bookmark.ToApiBookmark();

        // Assert
        Assert.Equal(100, result.Position);
        Assert.Equal("TestUser", result.Username);
        Assert.Equal("Test Comment", result.Comment);
    }
}
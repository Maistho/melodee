using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models.AlbumMerge;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Tests.Common.Services;

/// <summary>
/// Tests for ArtistService.MergeAlbumsAsync functionality, focusing on song user data merging.
/// </summary>
public class AlbumMergeServiceTests : ServiceTestBase
{
    #region Happy Path Tests

    [Fact]
    public async Task MergeAlbumsAsync_BasicMerge_MovesUniqueSongsToTarget()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        var targetSong = await CreateSongForAlbum(targetAlbum, "Target Song", 1);
        var sourceSong = await CreateSongForAlbum(sourceAlbum, "Source Song", 2);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");
        Assert.Equal(1, result.Data.SongsMoved);

        await using var context = await MockFactory().CreateDbContextAsync();
        var movedSong = await context.Songs.FindAsync(sourceSong.Id);
        Assert.NotNull(movedSong);
        Assert.Equal(targetAlbum.Id, movedSong.AlbumId);
    }

    [Fact]
    public async Task MergeAlbumsAsync_DuplicateSongs_SkipsAndMergesUserData()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        var targetSong = await CreateSongForAlbum(targetAlbum, "Same Song", 1, duration: 180000);
        var sourceSong = await CreateSongForAlbum(sourceAlbum, "Same Song", 1, duration: 180000);

        var user = await CreateTestUser();
        await CreateUserSong(user, sourceSong, playedCount: 10, rating: 5, isStarred: true);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");
        Assert.True(result.Data.SongsSkipped >= 1, "Expected at least 1 song to be skipped");

        await using var context = await MockFactory().CreateDbContextAsync();
        var targetUserSong = await context.UserSongs
            .FirstOrDefaultAsync(us => us.SongId == targetSong.Id && us.UserId == user.Id);
        Assert.NotNull(targetUserSong);
        Assert.Equal(10, targetUserSong.PlayedCount);
        Assert.Equal(5, targetUserSong.Rating);
        Assert.True(targetUserSong.IsStarred);
    }

    [Fact]
    public async Task MergeAlbumsAsync_UpdatesAlbumMetadata()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        await CreateSongForAlbum(targetAlbum, "Target Song 1", 1, duration: 180000);
        await CreateSongForAlbum(sourceAlbum, "Source Song 1", 2, duration: 200000);
        await CreateSongForAlbum(sourceAlbum, "Source Song 2", 3, duration: 220000);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");

        await using var context = await MockFactory().CreateDbContextAsync();
        var updatedAlbum = await context.Albums.FindAsync(targetAlbum.Id);
        Assert.NotNull(updatedAlbum);
        Assert.Equal((short)3, updatedAlbum.SongCount);
        Assert.Equal(600000d, updatedAlbum.Duration);
    }

    [Fact]
    public async Task MergeAlbumsAsync_DeletesSourceAlbum()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        await CreateSongForAlbum(sourceAlbum, "Source Song", 1);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");

        await using var context = await MockFactory().CreateDbContextAsync();
        var deletedAlbum = await context.Albums.FindAsync(sourceAlbum.Id);
        Assert.Null(deletedAlbum);
    }

    #endregion

    #region User Data Merge Tests

    [Fact]
    public async Task MergeAlbumsAsync_MergesUserSongPlayCounts()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        var targetSong = await CreateSongForAlbum(targetAlbum, "Same Song", 1, duration: 180000);
        var sourceSong = await CreateSongForAlbum(sourceAlbum, "Same Song", 1, duration: 180000);

        var user = await CreateTestUser();
        await CreateUserSong(user, targetSong, playedCount: 5, rating: 3);
        await CreateUserSong(user, sourceSong, playedCount: 10, rating: 5);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");

        await using var context = await MockFactory().CreateDbContextAsync();
        var mergedUserSong = await context.UserSongs
            .FirstOrDefaultAsync(us => us.SongId == targetSong.Id && us.UserId == user.Id);

        Assert.NotNull(mergedUserSong);
        Assert.Equal(15, mergedUserSong.PlayedCount);
        Assert.Equal(5, mergedUserSong.Rating);
    }

    [Fact]
    public async Task MergeAlbumsAsync_KeepsHighestRating()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        var targetSong = await CreateSongForAlbum(targetAlbum, "Same Song", 1, duration: 180000);
        var sourceSong = await CreateSongForAlbum(sourceAlbum, "Same Song", 1, duration: 180000);

        var user = await CreateTestUser();
        await CreateUserSong(user, targetSong, rating: 2);
        await CreateUserSong(user, sourceSong, rating: 4);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");

        await using var context = await MockFactory().CreateDbContextAsync();
        var mergedUserSong = await context.UserSongs
            .FirstOrDefaultAsync(us => us.SongId == targetSong.Id && us.UserId == user.Id);

        Assert.NotNull(mergedUserSong);
        Assert.Equal(4, mergedUserSong.Rating);
    }

    [Fact]
    public async Task MergeAlbumsAsync_PreservesStarIfEitherIsStarred()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        var targetSong = await CreateSongForAlbum(targetAlbum, "Same Song", 1, duration: 180000);
        var sourceSong = await CreateSongForAlbum(sourceAlbum, "Same Song", 1, duration: 180000);

        var user = await CreateTestUser();
        await CreateUserSong(user, targetSong, isStarred: false);
        await CreateUserSong(user, sourceSong, isStarred: true);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");

        await using var context = await MockFactory().CreateDbContextAsync();
        var mergedUserSong = await context.UserSongs
            .FirstOrDefaultAsync(us => us.SongId == targetSong.Id && us.UserId == user.Id);

        Assert.NotNull(mergedUserSong);
        Assert.True(mergedUserSong.IsStarred);
    }

    [Fact]
    public async Task MergeAlbumsAsync_OnlyHatedIfBothAreHated()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        var targetSong = await CreateSongForAlbum(targetAlbum, "Same Song", 1, duration: 180000);
        var sourceSong = await CreateSongForAlbum(sourceAlbum, "Same Song", 1, duration: 180000);

        var user = await CreateTestUser();
        await CreateUserSong(user, targetSong, isHated: true);
        await CreateUserSong(user, sourceSong, isHated: false);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");

        await using var context = await MockFactory().CreateDbContextAsync();
        var mergedUserSong = await context.UserSongs
            .FirstOrDefaultAsync(us => us.SongId == targetSong.Id && us.UserId == user.Id);

        Assert.NotNull(mergedUserSong);
        Assert.False(mergedUserSong.IsHated);
    }

    [Fact]
    public async Task MergeAlbumsAsync_KeepsMostRecentLastPlayedAt()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        var targetSong = await CreateSongForAlbum(targetAlbum, "Same Song", 1, duration: 180000);
        var sourceSong = await CreateSongForAlbum(sourceAlbum, "Same Song", 1, duration: 180000);

        var user = await CreateTestUser();
        var olderDate = Instant.FromDateTimeUtc(DateTime.UtcNow.AddDays(-10));
        var newerDate = Instant.FromDateTimeUtc(DateTime.UtcNow.AddDays(-1));

        await CreateUserSong(user, targetSong, lastPlayedAt: olderDate);
        await CreateUserSong(user, sourceSong, lastPlayedAt: newerDate);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");

        await using var context = await MockFactory().CreateDbContextAsync();
        var mergedUserSong = await context.UserSongs
            .FirstOrDefaultAsync(us => us.SongId == targetSong.Id && us.UserId == user.Id);

        Assert.NotNull(mergedUserSong);
        Assert.Equal(newerDate, mergedUserSong.LastPlayedAt);
    }

    [Fact]
    public async Task MergeAlbumsAsync_MovesUserSongWhenNoTargetUserSong()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        var targetSong = await CreateSongForAlbum(targetAlbum, "Same Song", 1, duration: 180000);
        var sourceSong = await CreateSongForAlbum(sourceAlbum, "Same Song", 1, duration: 180000);

        var user = await CreateTestUser();
        var sourceUserSong = await CreateUserSong(user, sourceSong, playedCount: 10, rating: 5, isStarred: true);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");

        await using var context = await MockFactory().CreateDbContextAsync();

        // The user song should now be associated with the target song
        var targetUserSong = await context.UserSongs
            .FirstOrDefaultAsync(us => us.SongId == targetSong.Id && us.UserId == user.Id);

        Assert.NotNull(targetUserSong);
        Assert.Equal(10, targetUserSong.PlayedCount);
        Assert.Equal(5, targetUserSong.Rating);
        Assert.True(targetUserSong.IsStarred);
    }

    #endregion

    #region Playlist Song Tests

    [Fact]
    public async Task MergeAlbumsAsync_MovesPlaylistSongsToTarget()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        var targetSong = await CreateSongForAlbum(targetAlbum, "Same Song", 1, duration: 180000);
        var sourceSong = await CreateSongForAlbum(sourceAlbum, "Same Song", 1, duration: 180000);

        var user = await CreateTestUser();
        var playlist = await CreatePlaylist(user, "Test Playlist");
        await AddSongToPlaylist(playlist, sourceSong, order: 1);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");

        await using var context = await MockFactory().CreateDbContextAsync();
        var playlistSongs = await context.PlaylistSong
            .Where(ps => ps.PlaylistId == playlist.Id)
            .ToListAsync();

        Assert.Single(playlistSongs);
        Assert.Equal(targetSong.Id, playlistSongs[0].SongId);
    }

    [Fact]
    public async Task MergeAlbumsAsync_RemovesDuplicatePlaylistEntries()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        var targetSong = await CreateSongForAlbum(targetAlbum, "Same Song", 1, duration: 180000);
        var sourceSong = await CreateSongForAlbum(sourceAlbum, "Same Song", 1, duration: 180000);

        var user = await CreateTestUser();
        var playlist = await CreatePlaylist(user, "Test Playlist");
        await AddSongToPlaylist(playlist, targetSong, order: 1);
        await AddSongToPlaylist(playlist, sourceSong, order: 2);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");

        await using var context = await MockFactory().CreateDbContextAsync();
        var playlistSongs = await context.PlaylistSong
            .Where(ps => ps.PlaylistId == playlist.Id)
            .ToListAsync();

        Assert.Single(playlistSongs);
        Assert.Equal(targetSong.Id, playlistSongs[0].SongId);
    }

    #endregion

    #region Play History Tests

    [Fact]
    public async Task MergeAlbumsAsync_MovesAllPlayHistoryToTarget()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        var targetSong = await CreateSongForAlbum(targetAlbum, "Same Song", 1, duration: 180000);
        var sourceSong = await CreateSongForAlbum(sourceAlbum, "Same Song", 1, duration: 180000);

        var user = await CreateTestUser();
        await CreatePlayHistory(user, sourceSong, DateTime.UtcNow.AddDays(-5));
        await CreatePlayHistory(user, sourceSong, DateTime.UtcNow.AddDays(-3));
        await CreatePlayHistory(user, sourceSong, DateTime.UtcNow.AddDays(-1));

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");

        await using var context = await MockFactory().CreateDbContextAsync();
        var historyCount = await context.UserSongPlayHistories
            .CountAsync(h => h.SongId == targetSong.Id);

        Assert.Equal(3, historyCount);
    }

    #endregion

    #region Bookmark Tests

    [Fact]
    public async Task MergeAlbumsAsync_MovesBookmarksToTarget()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        var targetSong = await CreateSongForAlbum(targetAlbum, "Same Song", 1, duration: 180000);
        var sourceSong = await CreateSongForAlbum(sourceAlbum, "Same Song", 1, duration: 180000);

        var user = await CreateTestUser();
        var bookmark = await CreateBookmark(user, sourceSong, position: 60000, comment: "Good part");

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");

        await using var context = await MockFactory().CreateDbContextAsync();
        var movedBookmark = await context.Bookmarks.FindAsync(bookmark.Id);

        Assert.NotNull(movedBookmark);
        Assert.Equal(targetSong.Id, movedBookmark.SongId);
        Assert.Equal(60000, movedBookmark.Position);
        Assert.Equal("Good part", movedBookmark.Comment);
    }

    [Fact]
    public async Task MergeAlbumsAsync_RemovesDuplicateBookmarks()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        var targetSong = await CreateSongForAlbum(targetAlbum, "Same Song", 1, duration: 180000);
        var sourceSong = await CreateSongForAlbum(sourceAlbum, "Same Song", 1, duration: 180000);

        var user = await CreateTestUser();
        await CreateBookmark(user, targetSong, position: 30000);
        var sourceBookmark = await CreateBookmark(user, sourceSong, position: 60000);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");

        await using var context = await MockFactory().CreateDbContextAsync();
        var bookmarks = await context.Bookmarks
            .Where(b => b.UserId == user.Id)
            .ToListAsync();

        Assert.Single(bookmarks);
        Assert.Equal(targetSong.Id, bookmarks[0].SongId);

        var removedBookmark = await context.Bookmarks.FindAsync(sourceBookmark.Id);
        Assert.Null(removedBookmark);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task MergeAlbumsAsync_MultipleSourceAlbums_MergesAllUserData()
    {
        var library = await CreateTestLibrary();
        var artist = await CreateArtistInLibrary(library, "Test Artist");
        var targetAlbum = await CreateAlbumForArtist(artist, "Target Album");
        var sourceAlbum1 = await CreateAlbumForArtist(artist, "Source Album 1");
        var sourceAlbum2 = await CreateAlbumForArtist(artist, "Source Album 2");

        var targetSong = await CreateSongForAlbum(targetAlbum, "Same Song", 1, duration: 180000);
        var sourceSong1 = await CreateSongForAlbum(sourceAlbum1, "Same Song", 1, duration: 180000);
        var sourceSong2 = await CreateSongForAlbum(sourceAlbum2, "Same Song", 1, duration: 180000);

        var user = await CreateTestUser();
        await CreateUserSong(user, sourceSong1, playedCount: 5, rating: 3);
        await CreateUserSong(user, sourceSong2, playedCount: 10, rating: 5);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum1.Id, sourceAlbum2.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");

        await using var context = await MockFactory().CreateDbContextAsync();
        var mergedUserSong = await context.UserSongs
            .FirstOrDefaultAsync(us => us.SongId == targetSong.Id && us.UserId == user.Id);

        Assert.NotNull(mergedUserSong);
        Assert.Equal(15, mergedUserSong.PlayedCount);
        Assert.Equal(5, mergedUserSong.Rating);
    }

    [Fact]
    public async Task MergeAlbumsAsync_MultipleUsers_MergesUserDataIndependently()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        var targetSong = await CreateSongForAlbum(targetAlbum, "Same Song", 1, duration: 180000);
        var sourceSong = await CreateSongForAlbum(sourceAlbum, "Same Song", 1, duration: 180000);

        var user1 = await CreateTestUser("user1@test.com");
        var user2 = await CreateTestUser("user2@test.com");

        await CreateUserSong(user1, targetSong, playedCount: 5, rating: 3);
        await CreateUserSong(user1, sourceSong, playedCount: 10, rating: 4);
        await CreateUserSong(user2, sourceSong, playedCount: 20, rating: 5, isStarred: true);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");

        await using var context = await MockFactory().CreateDbContextAsync();

        var user1Song = await context.UserSongs
            .FirstOrDefaultAsync(us => us.SongId == targetSong.Id && us.UserId == user1.Id);
        Assert.NotNull(user1Song);
        Assert.Equal(15, user1Song.PlayedCount);
        Assert.Equal(4, user1Song.Rating);

        var user2Song = await context.UserSongs
            .FirstOrDefaultAsync(us => us.SongId == targetSong.Id && us.UserId == user2.Id);
        Assert.NotNull(user2Song);
        Assert.Equal(20, user2Song.PlayedCount);
        Assert.Equal(5, user2Song.Rating);
        Assert.True(user2Song.IsStarred);
    }

    [Fact]
    public async Task MergeAlbumsAsync_NoUserData_CompletesSuccessfully()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();
        await CreateSongForAlbum(targetAlbum, "Target Song", 1);
        await CreateSongForAlbum(sourceAlbum, "Source Song", 2);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");
        Assert.Equal(0, result.Data.SongsUserDataMerged);
    }

    [Fact]
    public async Task MergeAlbumsAsync_BonusTracks_MovedToTarget()
    {
        var (targetAlbum, sourceAlbum, artist, library) = await CreateMergeTestAlbums();

        await CreateSongForAlbum(targetAlbum, "Track 1", 1, duration: 180000);
        await CreateSongForAlbum(targetAlbum, "Track 2", 2, duration: 200000);

        await CreateSongForAlbum(sourceAlbum, "Track 1", 1, duration: 180000);
        await CreateSongForAlbum(sourceAlbum, "Track 2", 2, duration: 200000);
        await CreateSongForAlbum(sourceAlbum, "Bonus Track 1", 3, duration: 150000);
        await CreateSongForAlbum(sourceAlbum, "Bonus Track 2", 4, duration: 160000);

        var result = await MergeAlbumsWithAutoResolveAsync(artist.Id, targetAlbum.Id, [sourceAlbum.Id]);

        Assert.True(result.IsSuccess, $"Merge failed: {string.Join(", ", result.Messages ?? [])}");
        Assert.Equal(2, result.Data.SongsMoved);
        Assert.True(result.Data.SongsSkipped >= 2, "Expected at least 2 songs to be skipped");

        await using var context = await MockFactory().CreateDbContextAsync();
        var targetSongs = await context.Songs
            .Where(s => s.AlbumId == targetAlbum.Id)
            .ToListAsync();

        Assert.Equal(4, targetSongs.Count);
        Assert.Contains(targetSongs, s => s.Title == "Bonus Track 1");
        Assert.Contains(targetSongs, s => s.Title == "Bonus Track 2");
    }

    #endregion

    #region Error Cases

    [Fact]
    public async Task MergeAlbumsAsync_InvalidArtistId_ReturnsError()
    {
        var library = await CreateTestLibrary();
        var artist = await CreateArtistInLibrary(library, "Test Artist");
        var targetAlbum = await CreateAlbumForArtist(artist, "Target Album");
        var sourceAlbum = await CreateAlbumForArtist(artist, "Source Album");

        var request = new AlbumMergeRequest
        {
            ArtistId = 999999,
            TargetAlbumId = targetAlbum.Id,
            SourceAlbumIds = [sourceAlbum.Id]
        };

        var result = await GetArtistService().MergeAlbumsAsync(request);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task MergeAlbumsAsync_InvalidTargetAlbumId_ReturnsError()
    {
        var library = await CreateTestLibrary();
        var artist = await CreateArtistInLibrary(library, "Test Artist");
        var sourceAlbum = await CreateAlbumForArtist(artist, "Source Album");

        var request = new AlbumMergeRequest
        {
            ArtistId = artist.Id,
            TargetAlbumId = 999999,
            SourceAlbumIds = [sourceAlbum.Id]
        };

        var result = await GetArtistService().MergeAlbumsAsync(request);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task MergeAlbumsAsync_EmptySourceAlbumIds_ThrowsException()
    {
        var library = await CreateTestLibrary();
        var artist = await CreateArtistInLibrary(library, "Test Artist");
        var targetAlbum = await CreateAlbumForArtist(artist, "Target Album");

        var request = new AlbumMergeRequest
        {
            ArtistId = artist.Id,
            TargetAlbumId = targetAlbum.Id,
            SourceAlbumIds = []
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            GetArtistService().MergeAlbumsAsync(request));
    }

    #endregion

    #region Helper Methods

    private async Task<(Album target, Album source, Artist artist, Library library)> CreateMergeTestAlbums()
    {
        var library = await CreateTestLibrary();
        var artist = await CreateArtistInLibrary(library, "Test Artist");
        var targetAlbum = await CreateAlbumForArtist(artist, "Target Album");
        var sourceAlbum = await CreateAlbumForArtist(artist, "Source Album");
        return (targetAlbum, sourceAlbum, artist, library);
    }

    /// <summary>
    /// Helper to execute merge with auto-generated resolutions for detected conflicts.
    /// This allows tests to focus on user data merging rather than conflict resolution workflow.
    /// </summary>
    private async Task<Melodee.Common.Models.OperationResult<AlbumMergeReport>> MergeAlbumsWithAutoResolveAsync(
        int artistId, int targetAlbumId, int[] sourceAlbumIds)
    {
        var artistService = GetArtistService();

        // Detect conflicts first
        var conflictResult = await artistService.DetectAlbumMergeConflictsAsync(
            artistId, targetAlbumId, sourceAlbumIds);

        if (!conflictResult.IsSuccess)
        {
            return new Melodee.Common.Models.OperationResult<AlbumMergeReport>(conflictResult.Messages)
            {
                Data = null!
            };
        }

        // Auto-resolve any conflicts - use KeepTarget for all required conflicts
        var resolutions = new List<AlbumMergeResolution>();
        if (conflictResult.Data?.Conflicts != null)
        {
            foreach (var conflict in conflictResult.Data.Conflicts.Where(c => c.IsRequired))
            {
                resolutions.Add(new AlbumMergeResolution
                {
                    ConflictId = conflict.ConflictId,
                    Action = AlbumMergeResolutionAction.KeepTarget,
                    TrackIds = conflict.TrackIds
                });
            }
        }

        var request = new AlbumMergeRequest
        {
            ArtistId = artistId,
            TargetAlbumId = targetAlbumId,
            SourceAlbumIds = sourceAlbumIds,
            Resolutions = resolutions.ToArray()
        };

        return await artistService.MergeAlbumsAsync(request);
    }

    private async Task<Library> CreateTestLibrary()
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var library = new Library
        {
            Name = $"Test Library {Guid.NewGuid():N}",
            Path = $"/test/library/{Guid.NewGuid():N}",
            Type = (int)LibraryType.Storage,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.Libraries.Add(library);
        await context.SaveChangesAsync();
        return library;
    }

    private async Task<Artist> CreateArtistInLibrary(Library library, string artistName)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var artist = new Artist
        {
            ApiKey = Guid.NewGuid(),
            Directory = $"{artistName.ToNormalizedString()}-{Guid.NewGuid():N}",
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            LibraryId = library.Id,
            Name = artistName,
            NameNormalized = artistName.ToNormalizedString()!
        };
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        return artist;
    }

    private async Task<Album> CreateAlbumForArtist(Artist artist, string albumName)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var album = new Album
        {
            ApiKey = Guid.NewGuid(),
            ArtistId = artist.Id,
            Directory = $"{albumName.ToNormalizedString()}-{Guid.NewGuid():N}",
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            Name = albumName,
            NameNormalized = albumName.ToNormalizedString()!,
            SongCount = 0,
            Duration = 0,
            ReleaseDate = LocalDate.FromDateTime(DateTime.Today),
            AlbumStatus = (short)AlbumStatus.Ok
        };
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        return album;
    }

    private async Task<Song> CreateSongForAlbum(Album album, string songTitle, int songNumber, int duration = 180000)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var song = new Song
        {
            ApiKey = Guid.NewGuid(),
            AlbumId = album.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            Title = songTitle,
            TitleNormalized = songTitle.ToNormalizedString()!,
            SongNumber = songNumber,
            Duration = duration,
            FileSize = 1024000,
            FileName = $"{songTitle.ToNormalizedString()}.mp3",
            FileHash = Guid.NewGuid().ToString("N"),
            BitRate = 320,
            BitDepth = 16,
            SamplingRate = 44100,
            BPM = 120,
            ContentType = "audio/mpeg"
        };
        context.Songs.Add(song);
        await context.SaveChangesAsync();
        return song;
    }

    private async Task<User> CreateTestUser(string email = "test@test.com")
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var user = new User
        {
            ApiKey = Guid.NewGuid(),
            UserName = email.Split('@')[0] + uniqueId,
            UserNameNormalized = (email.Split('@')[0] + uniqueId).ToUpperInvariant(),
            Email = $"{uniqueId}_{email}",
            EmailNormalized = $"{uniqueId}_{email}".ToUpperInvariant(),
            PublicKey = Guid.NewGuid().ToString(),
            PasswordEncrypted = "encryptedpassword",
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private async Task<UserSong> CreateUserSong(
        User user,
        Song song,
        int playedCount = 0,
        int rating = 0,
        bool isStarred = false,
        bool isHated = false,
        Instant? lastPlayedAt = null)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var userSong = new UserSong
        {
            UserId = user.Id,
            SongId = song.Id,
            PlayedCount = playedCount,
            Rating = rating,
            IsStarred = isStarred,
            IsHated = isHated,
            LastPlayedAt = lastPlayedAt,
            StarredAt = isStarred ? Instant.FromDateTimeUtc(DateTime.UtcNow) : null,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.UserSongs.Add(userSong);
        await context.SaveChangesAsync();
        return userSong;
    }

    private async Task<Playlist> CreatePlaylist(User user, string name)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var playlist = new Playlist
        {
            ApiKey = Guid.NewGuid(),
            UserId = user.Id,
            Name = name,
            IsPublic = false,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();
        return playlist;
    }

    private async Task<PlaylistSong> AddSongToPlaylist(Playlist playlist, Song song, int order)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var playlistSong = new PlaylistSong
        {
            PlaylistId = playlist.Id,
            SongId = song.Id,
            SongApiKey = song.ApiKey,
            PlaylistOrder = order
        };
        context.PlaylistSong.Add(playlistSong);
        await context.SaveChangesAsync();
        return playlistSong;
    }

    private async Task<UserSongPlayHistory> CreatePlayHistory(User user, Song song, DateTime playedAt)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var history = new UserSongPlayHistory
        {
            UserId = user.Id,
            SongId = song.Id,
            PlayedAt = Instant.FromDateTimeUtc(playedAt),
            Client = "TestClient"
        };
        context.UserSongPlayHistories.Add(history);
        await context.SaveChangesAsync();
        return history;
    }

    private async Task<Bookmark> CreateBookmark(User user, Song song, int position, string? comment = null)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var bookmark = new Bookmark
        {
            ApiKey = Guid.NewGuid(),
            UserId = user.Id,
            SongId = song.Id,
            Position = position,
            Comment = comment,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.Bookmarks.Add(bookmark);
        await context.SaveChangesAsync();
        return bookmark;
    }

    #endregion
}

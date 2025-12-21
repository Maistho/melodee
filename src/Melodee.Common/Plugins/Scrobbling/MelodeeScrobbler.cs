using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Plugins.Scrobbling;

public class MelodeeScrobbler(
    AlbumService albumService,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    Serilog.ILogger? logger = null) : IScrobbler
{
    public bool StopProcessing { get; } = false;
    public string Id => "D8A07387-87DF-4136-8D3E-C59EABEB501F";

    public string DisplayName => nameof(MelodeeScrobbler);

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 0;

    public async Task<OperationResult<bool>> NowPlaying(UserInfo user, ScrobbleInfo scrobble,
        CancellationToken cancellationToken = default)
    {
        logger?.Information("[{ScrobblerName}] NowPlaying: User [{User}], Song [{Song}] ([{SongId}]), SecondsPlayed [{Seconds}]",
            DisplayName,
            user.UserName,
            scrobble.SongTitle,
            scrobble.SongId,
            scrobble.SecondsPlayed);

        await using var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var now = SystemClock.Instance.GetCurrentInstant();

        // Clear any previous "now playing" for this user (only one song can be playing at a time)
        var previousNowPlaying = await scopedContext.UserSongPlayHistories
            .Where(h => h.UserId == user.Id && h.IsNowPlaying)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var previous in previousNowPlaying)
        {
            previous.IsNowPlaying = false;
        }

        // Ensure UserSong exists (create if doesn't exist)
        var userSong = await scopedContext.UserSongs
            .FirstOrDefaultAsync(us => us.UserId == user.Id && us.SongId == scrobble.SongId, cancellationToken)
            .ConfigureAwait(false);

        if (userSong == null)
        {
            logger?.Information("[{ScrobblerName}] NowPlaying: Creating new UserSong for User [{UserId}], Song [{SongId}]",
                DisplayName, user.Id, scrobble.SongId);

            userSong = new UserSong
            {
                UserId = user.Id,
                SongId = scrobble.SongId,
                CreatedAt = now
            };
            scopedContext.UserSongs.Add(userSong);
            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger?.Information("[{ScrobblerName}] Created new UserSong", DisplayName);
        }

        // Look for an existing "now playing" record for this user/song combination
        var existingPlayHistory = await scopedContext.UserSongPlayHistories
            .Where(h => h.UserId == user.Id && h.SongId == scrobble.SongId && h.IsNowPlaying)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existingPlayHistory != null)
        {
            // Update existing record with progress (heartbeat)
            logger?.Information("[{ScrobblerName}] Updating existing UserSongPlayHistory Id [{Id}] with SecondsPlayed [{Seconds}]",
                DisplayName, existingPlayHistory.Id, scrobble.SecondsPlayed);

            existingPlayHistory.SecondsPlayed = scrobble.SecondsPlayed;
            existingPlayHistory.LastHeartbeatAt = now;
            if (!string.IsNullOrWhiteSpace(scrobble.UserAgent))
            {
                existingPlayHistory.ByUserAgent = scrobble.UserAgent;
            }
            if (!string.IsNullOrWhiteSpace(scrobble.IpAddress))
            {
                existingPlayHistory.IpAddress = scrobble.IpAddress;
            }

            var updated = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger?.Information("[{ScrobblerName}] Updated play history, SaveChanges resulted in [{Count}] records saved", DisplayName, updated);
        }
        else
        {
            // Create new play history record with IsNowPlaying = true
            logger?.Information("[{ScrobblerName}] Creating new UserSongPlayHistory for NowPlaying: User [{UserId}], Song [{SongId}], SecondsPlayed [{Seconds}]",
                DisplayName, user.Id, scrobble.SongId, scrobble.SecondsPlayed);

            var playHistory = new UserSongPlayHistory
            {
                UserId = user.Id,
                SongId = scrobble.SongId,
                PlayedAt = now,
                Client = string.IsNullOrWhiteSpace(scrobble.PlayerName) ? nameof(MelodeeScrobbler) : scrobble.PlayerName,
                Source = 1,
                ByUserAgent = scrobble.UserAgent,
                IpAddress = scrobble.IpAddress,
                SecondsPlayed = scrobble.SecondsPlayed,
                IsNowPlaying = true,
                LastHeartbeatAt = now
            };

            scopedContext.UserSongPlayHistories.Add(playHistory);
            var saved = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger?.Information("[{ScrobblerName}] Created new play history, SaveChanges resulted in [{Count}] records saved", DisplayName, saved);
        }

        return new OperationResult<bool>
        {
            Data = true
        };
    }

    public async Task<OperationResult<bool>> Scrobble(UserInfo user, ScrobbleInfo scrobble,
        CancellationToken cancellationToken = default)
    {
        logger?.Information("[{ScrobblerName}] Scrobble: User [{User}] (Id: {UserId}), Song [{Song}] (Id: {SongId}), Artist (Id: {ArtistId}), Album (Id: {AlbumId})",
            DisplayName,
            user.UserName,
            user.Id,
            scrobble.SongTitle,
            scrobble.SongId,
            scrobble.ArtistId,
            scrobble.AlbumId);

        await using var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var now = SystemClock.Instance.GetCurrentInstant();

        // Update Artist played count and last played time
        var artist = await scopedContext.Artists
            .FirstOrDefaultAsync(a => a.Id == scrobble.ArtistId, cancellationToken)
            .ConfigureAwait(false);
        if (artist != null)
        {
            artist.PlayedCount++;
            artist.LastPlayedAt = now;
            logger?.Information("[{ScrobblerName}] Updated Artist record", DisplayName);
        }

        // Update Album played count and last played time
        var album = await scopedContext.Albums
            .FirstOrDefaultAsync(a => a.Id == scrobble.AlbumId, cancellationToken)
            .ConfigureAwait(false);
        if (album != null)
        {
            album.PlayedCount++;
            album.LastPlayedAt = now;
            logger?.Information("[{ScrobblerName}] Updated Album record", DisplayName);
        }

        // Update Song played count and last played time
        var song = await scopedContext.Songs
            .FirstOrDefaultAsync(s => s.Id == scrobble.SongId, cancellationToken)
            .ConfigureAwait(false);
        if (song != null)
        {
            song.PlayedCount++;
            song.LastPlayedAt = now;
            logger?.Information("[{ScrobblerName}] Updated Song record", DisplayName);
        }

        // Handle UserSong upsert logic
        var existingUserSong = await scopedContext.UserSongs
            .FirstOrDefaultAsync(us => us.UserId == user.Id && us.SongId == scrobble.SongId, cancellationToken)
            .ConfigureAwait(false);

        if (existingUserSong != null)
        {
            existingUserSong.PlayedCount++;
            existingUserSong.LastPlayedAt = now;
            logger?.Information("[{ScrobblerName}] Updated existing UserSong for User [{UserId}], Song [{SongId}]",
                DisplayName, user.Id, scrobble.SongId);
        }
        else
        {
            logger?.Information("[{ScrobblerName}] Creating new UserSong for User [{UserId}], Song [{SongId}]",
                DisplayName, user.Id, scrobble.SongId);

            var newUserSong = new UserSong
            {
                UserId = user.Id,
                SongId = scrobble.SongId,
                PlayedCount = 1,
                LastPlayedAt = now,
                IsStarred = false,
                IsHated = false,
                Rating = 0,
                IsLocked = false,
                SortOrder = 0,
                ApiKey = Guid.NewGuid(),
                CreatedAt = now
            };

            scopedContext.UserSongs.Add(newUserSong);
        }

        // Find the "now playing" record for this song and mark it as completed
        var nowPlayingRecord = await scopedContext.UserSongPlayHistories
            .Where(h => h.UserId == user.Id && h.SongId == scrobble.SongId && h.IsNowPlaying)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (nowPlayingRecord != null)
        {
            // Update existing record with final play time and mark as no longer playing
            logger?.Information("[{ScrobblerName}] Scrobble: Updating existing UserSongPlayHistory Id [{Id}] with final SecondsPlayed [{Seconds}]",
                DisplayName, nowPlayingRecord.Id, scrobble.SecondsPlayed);

            nowPlayingRecord.SecondsPlayed = scrobble.SecondsPlayed;
            nowPlayingRecord.IsNowPlaying = false;
            nowPlayingRecord.LastHeartbeatAt = now;
            if (!string.IsNullOrWhiteSpace(scrobble.UserAgent))
            {
                nowPlayingRecord.ByUserAgent = scrobble.UserAgent;
            }
            if (!string.IsNullOrWhiteSpace(scrobble.IpAddress))
            {
                nowPlayingRecord.IpAddress = scrobble.IpAddress;
            }
        }
        else
        {
            // No "now playing" record found, create a new completed play history
            logger?.Information("[{ScrobblerName}] Scrobble: Creating new UserSongPlayHistory for User [{UserId}], Song [{SongId}], SecondsPlayed [{Seconds}]",
                DisplayName, user.Id, scrobble.SongId, scrobble.SecondsPlayed);

            var playHistory = new UserSongPlayHistory
            {
                UserId = user.Id,
                SongId = scrobble.SongId,
                PlayedAt = now,
                Client = string.IsNullOrWhiteSpace(scrobble.PlayerName) ? nameof(MelodeeScrobbler) : scrobble.PlayerName,
                Source = 1,
                ByUserAgent = scrobble.UserAgent,
                IpAddress = scrobble.IpAddress,
                SecondsPlayed = scrobble.SecondsPlayed,
                IsNowPlaying = false,
                LastHeartbeatAt = now
            };
            scopedContext.UserSongPlayHistories.Add(playHistory);
        }

        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var albumFromService = await albumService.GetAsync(scrobble.AlbumId, cancellationToken).ConfigureAwait(false);
        if (albumFromService.IsSuccess)
        {
            albumService.ClearCache(albumFromService.Data!);
        }

        logger?.Information("[{ScrobblerName}] Scrobble completed successfully", DisplayName);
        return new OperationResult<bool>
        {
            Data = true
        };
    }
}

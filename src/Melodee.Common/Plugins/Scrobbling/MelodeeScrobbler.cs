using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Plugins.Scrobbling;

public class MelodeeScrobbler(
    AlbumService albumService,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    INowPlayingRepository nowPlayingRepository,
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

        await nowPlayingRepository.AddOrUpdateNowPlayingAsync(new NowPlayingInfo(user, scrobble), cancellationToken)
            .ConfigureAwait(false);

        // Create or update UserSong and UserSongPlayHistory records
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var now = SystemClock.Instance.GetCurrentInstant();

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

            // Look for a recent play history record (within last 10 minutes)
            // This simple time-based approach works for all clients
            var recentPlayHistory = await scopedContext.UserSongPlayHistories
                .Where(h => h.UserId == user.Id
                    && h.SongId == scrobble.SongId
                    && h.PlayedAt >= now.Minus(Duration.FromMinutes(10)))
                .OrderByDescending(h => h.PlayedAt)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (recentPlayHistory != null)
            {
                // Update existing record with progress
                logger?.Information("[{ScrobblerName}] Updating existing UserSongPlayHistory Id [{Id}] with SecondsPlayed [{Seconds}]",
                    DisplayName, recentPlayHistory.Id, scrobble.SecondsPlayed);

                recentPlayHistory.SecondsPlayed = scrobble.SecondsPlayed;
                recentPlayHistory.PlayedAt = now; // Keep the record fresh in the 10-minute window
                if (!string.IsNullOrWhiteSpace(scrobble.UserAgent))
                {
                    recentPlayHistory.ByUserAgent = scrobble.UserAgent;
                }
                if (!string.IsNullOrWhiteSpace(scrobble.IpAddress))
                {
                    recentPlayHistory.IpAddress = scrobble.IpAddress;
                }

                var updated = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                logger?.Information("[{ScrobblerName}] Updated play history, SaveChanges resulted in [{Count}] records saved", DisplayName, updated);
            }
            else
            {
                // Create new play history record
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
                    SecondsPlayed = scrobble.SecondsPlayed
                };

                scopedContext.UserSongPlayHistories.Add(playHistory);
                var saved = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                logger?.Information("[{ScrobblerName}] Created new play history, SaveChanges resulted in [{Count}] records saved", DisplayName, saved);
            }
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

        await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            // Update Artist played count and last played time using ExecuteUpdateAsync for performance
            var artistUpdated = await scopedContext.Artists
                .Where(a => a.Id == scrobble.ArtistId)
                .ExecuteUpdateAsync(a => a
                    .SetProperty(p => p.PlayedCount, p => p.PlayedCount + 1)
                    .SetProperty(p => p.LastPlayedAt, now), cancellationToken)
                .ConfigureAwait(false);
            logger?.Information("[{ScrobblerName}] Updated [{Count}] Artist records", DisplayName, artistUpdated);

            // Update Album played count and last played time using ExecuteUpdateAsync for performance
            var albumUpdated = await scopedContext.Albums
                .Where(a => a.Id == scrobble.AlbumId)
                .ExecuteUpdateAsync(a => a
                    .SetProperty(p => p.PlayedCount, p => p.PlayedCount + 1)
                    .SetProperty(p => p.LastPlayedAt, now), cancellationToken)
                .ConfigureAwait(false);
            logger?.Information("[{ScrobblerName}] Updated [{Count}] Album records", DisplayName, albumUpdated);

            // Update Song played count and last played time using ExecuteUpdateAsync for performance
            var songUpdated = await scopedContext.Songs
                .Where(s => s.Id == scrobble.SongId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.PlayedCount, p => p.PlayedCount + 1)
                    .SetProperty(p => p.LastPlayedAt, now), cancellationToken)
                .ConfigureAwait(false);
            logger?.Information("[{ScrobblerName}] Updated [{Count}] Song records", DisplayName, songUpdated);

            // Handle UserSong upsert logic - first try to update, if no rows affected then insert
            var updatedRows = await scopedContext.UserSongs
                .Where(us => us.UserId == user.Id && us.SongId == scrobble.SongId)
                .ExecuteUpdateAsync(us => us
                    .SetProperty(p => p.PlayedCount, p => p.PlayedCount + 1)
                    .SetProperty(p => p.LastPlayedAt, now), cancellationToken)
                .ConfigureAwait(false);

            if (updatedRows == 0)
            {
                logger?.Information("[{ScrobblerName}] Creating new UserSong for User [{UserId}], Song [{SongId}]",
                    DisplayName, user.Id, scrobble.SongId);

                // No existing UserSong found, create new one
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
                await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                logger?.Information("[{ScrobblerName}] Updated existing UserSong for User [{UserId}], Song [{SongId}]",
                    DisplayName, user.Id, scrobble.SongId);
            }

            // Look for a recent play history record (within last 10 minutes)
            // This simple time-based approach works for all clients
            var recentPlayHistory = await scopedContext.UserSongPlayHistories
                .Where(h => h.UserId == user.Id
                    && h.SongId == scrobble.SongId
                    && h.PlayedAt >= now.Minus(Duration.FromMinutes(10)))
                .OrderByDescending(h => h.PlayedAt)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (recentPlayHistory != null)
            {
                // Update existing record with final play time
                logger?.Information("[{ScrobblerName}] Scrobble: Updating existing UserSongPlayHistory Id [{Id}] with final SecondsPlayed [{Seconds}]",
                    DisplayName, recentPlayHistory.Id, scrobble.SecondsPlayed);

                recentPlayHistory.SecondsPlayed = scrobble.SecondsPlayed;
                recentPlayHistory.PlayedAt = now; // Keep the record fresh
                if (!string.IsNullOrWhiteSpace(scrobble.UserAgent))
                {
                    recentPlayHistory.ByUserAgent = scrobble.UserAgent;
                }
                if (!string.IsNullOrWhiteSpace(scrobble.IpAddress))
                {
                    recentPlayHistory.IpAddress = scrobble.IpAddress;
                }

                var updated = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                logger?.Information("[{ScrobblerName}] Updated existing play history, SaveChanges resulted in [{Count}] records saved", DisplayName, updated);
            }
            else
            {
                // No recent play history found, create new one
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
                    SecondsPlayed = scrobble.SecondsPlayed
                };
                scopedContext.UserSongPlayHistories.Add(playHistory);

                var saved = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                logger?.Information("[{ScrobblerName}] Created new play history, SaveChanges resulted in [{Count}] records saved", DisplayName, saved);
            }

            await nowPlayingRepository
                .RemoveNowPlayingAsync(SafeParser.Hash(user.ApiKey.ToString(), scrobble.SongApiKey.ToString()),
                    cancellationToken)
                .ConfigureAwait(false);

            var album = await albumService.GetAsync(scrobble.AlbumId, cancellationToken).ConfigureAwait(false);
            if (album.IsSuccess)
            {
                albumService.ClearCache(album.Data!);
            }
        }

        logger?.Information("[{ScrobblerName}] Scrobble completed successfully", DisplayName);
        return new OperationResult<bool>
        {
            Data = true
        };
    }
}

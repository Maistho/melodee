using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services;

/// <summary>
/// Service for tracking podcast episode playback and managing bookmarks.
/// Mirrors the music scrobbling infrastructure but specifically for podcast episodes.
/// </summary>
public class PodcastPlaybackService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    /// <summary>
    /// Update "now playing" status for a podcast episode (heartbeat mechanism).
    /// Creates or updates a play history record with IsNowPlaying = true.
    /// </summary>
    public async Task<OperationResult<bool>> NowPlayingAsync(
        int userId,
        int episodeId,
        int? secondsPlayed = null,
        string? client = null,
        string? userAgent = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        Logger.Information("[{ServiceName}] NowPlaying: User [{UserId}], Episode [{EpisodeId}], SecondsPlayed [{Seconds}]",
            nameof(PodcastPlaybackService), userId, episodeId, secondsPlayed);

        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var now = SystemClock.Instance.GetCurrentInstant();

        // Clear any previous "now playing" for this user (only one episode can be playing at a time)
        var previousNowPlaying = await context.UserPodcastEpisodePlayHistories
            .Where(h => h.UserId == userId && h.IsNowPlaying)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var previous in previousNowPlaying)
        {
            previous.IsNowPlaying = false;
        }

        // Look for an existing "now playing" record for this user/episode combination
        var existingPlayHistory = await context.UserPodcastEpisodePlayHistories
            .Where(h => h.UserId == userId && h.PodcastEpisodeId == episodeId && h.IsNowPlaying)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existingPlayHistory != null)
        {
            // Update existing record with progress (heartbeat)
            Logger.Information("[{ServiceName}] Updating existing play history Id [{Id}] with SecondsPlayed [{Seconds}]",
                nameof(PodcastPlaybackService), existingPlayHistory.Id, secondsPlayed);

            existingPlayHistory.SecondsPlayed = secondsPlayed;
            existingPlayHistory.LastHeartbeatAt = now;
            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                existingPlayHistory.ByUserAgent = userAgent;
            }
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                existingPlayHistory.IpAddress = ipAddress;
            }

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            // Create new play history record with IsNowPlaying = true
            Logger.Information("[{ServiceName}] Creating new play history for NowPlaying: User [{UserId}], Episode [{EpisodeId}], SecondsPlayed [{Seconds}]",
                nameof(PodcastPlaybackService), userId, episodeId, secondsPlayed);

            var playHistory = new UserPodcastEpisodePlayHistory
            {
                UserId = userId,
                PodcastEpisodeId = episodeId,
                PlayedAt = now,
                Client = string.IsNullOrWhiteSpace(client) ? "Melodee" : client,
                Source = 4, // Podcast source
                ByUserAgent = userAgent,
                IpAddress = ipAddress,
                SecondsPlayed = secondsPlayed,
                IsNowPlaying = true,
                LastHeartbeatAt = now
            };

            context.UserPodcastEpisodePlayHistories.Add(playHistory);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return new OperationResult<bool> { Data = true };
    }

    /// <summary>
    /// Record a completed play (scrobble) of a podcast episode.
    /// Updates episode play count and marks the "now playing" record as completed.
    /// </summary>
    public async Task<OperationResult<bool>> ScrobbleAsync(
        int userId,
        int episodeId,
        int? secondsPlayed = null,
        string? client = null,
        string? userAgent = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        Logger.Information("[{ServiceName}] Scrobble: User [{UserId}], Episode [{EpisodeId}], SecondsPlayed [{Seconds}]",
            nameof(PodcastPlaybackService), userId, episodeId, secondsPlayed);

        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var now = SystemClock.Instance.GetCurrentInstant();

        // Update Episode played count and last played time
        var episode = await context.PodcastEpisodes
            .FirstOrDefaultAsync(e => e.Id == episodeId, cancellationToken)
            .ConfigureAwait(false);

        if (episode == null)
        {
            Logger.Warning("[{ServiceName}] Episode [{EpisodeId}] not found for scrobble", nameof(PodcastPlaybackService), episodeId);
            return new OperationResult<bool>("Episode not found") { Data = false };
        }

        // Find the "now playing" record for this episode and mark it as completed
        var nowPlayingRecord = await context.UserPodcastEpisodePlayHistories
            .Where(h => h.UserId == userId && h.PodcastEpisodeId == episodeId && h.IsNowPlaying)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (nowPlayingRecord != null)
        {
            // Update existing record with final play time and mark as no longer playing
            Logger.Information("[{ServiceName}] Scrobble: Updating existing play history Id [{Id}] with final SecondsPlayed [{Seconds}]",
                nameof(PodcastPlaybackService), nowPlayingRecord.Id, secondsPlayed);

            nowPlayingRecord.SecondsPlayed = secondsPlayed;
            nowPlayingRecord.IsNowPlaying = false;
            nowPlayingRecord.LastHeartbeatAt = now;
            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                nowPlayingRecord.ByUserAgent = userAgent;
            }
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                nowPlayingRecord.IpAddress = ipAddress;
            }
        }
        else
        {
            // No "now playing" record found, create a new completed play history
            Logger.Information("[{ServiceName}] Scrobble: Creating new play history for User [{UserId}], Episode [{EpisodeId}], SecondsPlayed [{Seconds}]",
                nameof(PodcastPlaybackService), userId, episodeId, secondsPlayed);

            var playHistory = new UserPodcastEpisodePlayHistory
            {
                UserId = userId,
                PodcastEpisodeId = episodeId,
                PlayedAt = now,
                Client = string.IsNullOrWhiteSpace(client) ? "Melodee" : client,
                Source = 4, // Podcast source
                ByUserAgent = userAgent,
                IpAddress = ipAddress,
                SecondsPlayed = secondsPlayed,
                IsNowPlaying = false,
                LastHeartbeatAt = now
            };
            context.UserPodcastEpisodePlayHistories.Add(playHistory);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Logger.Information("[{ServiceName}] Scrobble completed successfully", nameof(PodcastPlaybackService));
        return new OperationResult<bool> { Data = true };
    }

    /// <summary>
    /// Save or update a bookmark (resume position) for a podcast episode.
    /// </summary>
    public async Task<OperationResult<bool>> SaveBookmarkAsync(
        int userId,
        int episodeId,
        int positionSeconds,
        string? comment = null,
        CancellationToken cancellationToken = default)
    {
        Logger.Information("[{ServiceName}] SaveBookmark: User [{UserId}], Episode [{EpisodeId}], Position [{Position}]",
            nameof(PodcastPlaybackService), userId, episodeId, positionSeconds);

        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var now = SystemClock.Instance.GetCurrentInstant();

        // Try to find existing bookmark
        var existing = await context.PodcastEpisodeBookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.PodcastEpisodeId == episodeId, cancellationToken)
            .ConfigureAwait(false);

        if (existing != null)
        {
            // Update existing bookmark
            existing.PositionSeconds = positionSeconds;
            existing.UpdatedAt = now;
            if (comment != null)
            {
                existing.Comment = comment;
            }
            Logger.Information("[{ServiceName}] Updated existing bookmark Id [{Id}]", nameof(PodcastPlaybackService), existing.Id);
        }
        else
        {
            // Create new bookmark
            var bookmark = new PodcastEpisodeBookmark
            {
                UserId = userId,
                PodcastEpisodeId = episodeId,
                PositionSeconds = positionSeconds,
                Comment = comment,
                CreatedAt = now,
                UpdatedAt = now
            };
            context.PodcastEpisodeBookmarks.Add(bookmark);
            Logger.Information("[{ServiceName}] Created new bookmark", nameof(PodcastPlaybackService));
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new OperationResult<bool> { Data = true };
    }

    /// <summary>
    /// Get the bookmark (resume position) for a podcast episode.
    /// </summary>
    public async Task<OperationResult<PodcastEpisodeBookmark?>> GetBookmarkAsync(
        int userId,
        int episodeId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var bookmark = await context.PodcastEpisodeBookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.PodcastEpisodeId == episodeId, cancellationToken)
            .ConfigureAwait(false);

        return new OperationResult<PodcastEpisodeBookmark?> { Data = bookmark };
    }

    /// <summary>
    /// Delete a bookmark for a podcast episode.
    /// </summary>
    public async Task<OperationResult<bool>> DeleteBookmarkAsync(
        int userId,
        int episodeId,
        CancellationToken cancellationToken = default)
    {
        Logger.Information("[{ServiceName}] DeleteBookmark: User [{UserId}], Episode [{EpisodeId}]",
            nameof(PodcastPlaybackService), userId, episodeId);

        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var bookmark = await context.PodcastEpisodeBookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.PodcastEpisodeId == episodeId, cancellationToken)
            .ConfigureAwait(false);

        if (bookmark != null)
        {
            context.PodcastEpisodeBookmarks.Remove(bookmark);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            Logger.Information("[{ServiceName}] Deleted bookmark Id [{Id}]", nameof(PodcastPlaybackService), bookmark.Id);
            return new OperationResult<bool> { Data = true };
        }

        Logger.Warning("[{ServiceName}] No bookmark found to delete", nameof(PodcastPlaybackService));
        return new OperationResult<bool>("Bookmark not found") { Data = false };
    }

    /// <summary>
    /// Get play history for a user's podcast episodes.
    /// </summary>
    public async Task<OperationResult<IEnumerable<UserPodcastEpisodePlayHistory>>> GetPlayHistoryAsync(
        int userId,
        int? episodeId = null,
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var query = context.UserPodcastEpisodePlayHistories
            .Where(h => h.UserId == userId && !h.IsNowPlaying)
            .OrderByDescending(h => h.PlayedAt)
            .AsQueryable();

        if (episodeId.HasValue)
        {
            query = query.Where(h => h.PodcastEpisodeId == episodeId.Value);
        }

        var history = await query
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new OperationResult<IEnumerable<UserPodcastEpisodePlayHistory>> { Data = history };
    }

    /// <summary>
    /// Get currently playing podcast episodes for a user.
    /// </summary>
    public async Task<OperationResult<IEnumerable<UserPodcastEpisodePlayHistory>>> GetNowPlayingAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var nowPlaying = await context.UserPodcastEpisodePlayHistories
            .Where(h => h.UserId == userId && h.IsNowPlaying)
            .Include(h => h.PodcastEpisode)
            .OrderByDescending(h => h.LastHeartbeatAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new OperationResult<IEnumerable<UserPodcastEpisodePlayHistory>> { Data = nowPlaying };
    }
}

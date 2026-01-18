using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Quartz;
using Serilog;

namespace Melodee.Common.Jobs;

/// <summary>
/// Re-attempts matching for missing playlist items when new music is added to the library.
/// Runs periodically or can be triggered after library scans.
/// </summary>
[DisallowConcurrentExecution]
public sealed class PlaylistReconciliationJob(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    LibraryService libraryService,
    Services.Caching.ICacheManager cacheManager) : JobBase(logger, configurationFactory)
{
    public override bool DoCreateJobHistory => true;

    public override async Task Execute(IJobExecutionContext context)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(context.CancellationToken).ConfigureAwait(false);

        // Find all missing playlist items that haven't been attempted recently
        var retryThreshold = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromHours(1));

        var missingItems = await dbContext.PlaylistUploadedFileItems
            .Where(x => x.Status == PlaylistItemStatus.Missing)
            .Where(x => x.LastAttemptUtc == null || x.LastAttemptUtc < retryThreshold)
            .Include(x => x.PlaylistUploadedFile)
                .ThenInclude(f => f.Items)
            .OrderBy(x => x.LastAttemptUtc)
            .Take(500) // Process in batches to avoid overwhelming the database
            .ToListAsync(context.CancellationToken)
            .ConfigureAwait(false);

        if (missingItems.Count == 0)
        {
            Logger.Debug("[{JobName}] No missing playlist items to reconcile.", nameof(PlaylistReconciliationJob));
            return;
        }

        Logger.Information("[{JobName}] Attempting to reconcile {ItemCount} missing playlist items.",
            nameof(PlaylistReconciliationJob), missingItems.Count);

        // Get library path for matching
        var librariesResult = await libraryService.GetStorageLibrariesAsync(context.CancellationToken).ConfigureAwait(false);
        var libraryPath = librariesResult.Data?.FirstOrDefault()?.Path;

        var songMatcher = new SongMatchingService(Logger, cacheManager, contextFactory);

        var resolvedCount = 0;
        var now = SystemClock.Instance.GetCurrentInstant();

        foreach (var item in missingItems)
        {
            try
            {
                // Reconstruct M3UEntry from stored data
                var entry = new Services.Parsing.M3UEntry
                {
                    RawReference = item.RawReference,
                    NormalizedReference = item.NormalizedReference,
                    SortOrder = item.SortOrder,
                    // Hints are stored in JSON, but for now we'll extract them from normalized reference
                    FileName = item.NormalizedReference.Split('/').LastOrDefault(),
                    ArtistFolder = null,
                    AlbumFolder = null
                };

                var matchResult = await songMatcher.MatchEntryAsync(entry, libraryPath, context.CancellationToken)
                    .ConfigureAwait(false);

                // Update the item
                item.LastAttemptUtc = now;

                if (matchResult.Song != null)
                {
                    item.SongId = matchResult.Song.Id;
                    item.Status = PlaylistItemStatus.Resolved;
                    resolvedCount++;

                    // Find the associated playlist and add the song
                    var playlist = await dbContext.Playlists
                        .Include(p => p.Songs)
                        .FirstOrDefaultAsync(p => p.PlaylistUploadedFileId == item.PlaylistUploadedFileId,
                            context.CancellationToken)
                        .ConfigureAwait(false);

                    if (playlist != null)
                    {
                        // Check if this song is already in the playlist to maintain idempotency
                        var existingPlaylistSong = playlist.Songs
                            .FirstOrDefault(ps => ps.SongId == matchResult.Song.Id);

                        if (existingPlaylistSong == null)
                        {
                            // Add song to playlist maintaining sort order
                            var maxOrder = playlist.Songs.Any() ? playlist.Songs.Max(ps => ps.PlaylistOrder) : -1;

                            var playlistSong = new Data.Models.PlaylistSong
                            {
                                PlaylistId = playlist.Id,
                                SongId = matchResult.Song.Id,
                                SongApiKey = matchResult.Song.ApiKey,
                                PlaylistOrder = maxOrder + 1
                            };

                            playlist.Songs.Add(playlistSong);
                            playlist.SongCount = (short)playlist.Songs.Count;

                            // Efficiently calculate duration by loading song from matchResult instead of querying
                            var currentDuration = playlist.Songs
                                .Where(ps => ps.SongId != matchResult.Song.Id)
                                .Sum(ps => dbContext.Songs.First(s => s.Id == ps.SongId).Duration);
                            playlist.Duration = currentDuration + matchResult.Song.Duration;

                            Logger.Debug("[{JobName}] Resolved missing item for playlist [{PlaylistId}]: {Reference}",
                                nameof(PlaylistReconciliationJob), playlist.Id, item.NormalizedReference);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "[{JobName}] Error reconciling item: {Reference}",
                    nameof(PlaylistReconciliationJob), item.NormalizedReference);
                item.LastAttemptUtc = now;
            }
        }

        await dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);

        Logger.Information("[{JobName}] Reconciliation complete. Resolved {ResolvedCount}/{TotalCount} items.",
            nameof(PlaylistReconciliationJob), resolvedCount, missingItems.Count);
    }
}

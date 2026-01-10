using System.Diagnostics;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Quartz;
using Serilog;

namespace Melodee.Common.Jobs;

/// <summary>
///     Cleans up downloaded podcast episodes based on retention policies.
///     Supports three retention modes:
///     1. Keep for X days (PodcastRetentionDownloadedEpisodesInDays)
///     2. Keep last N episodes per channel (PodcastRetentionKeepLastNEpisodes)
///     3. Keep unplayed only (PodcastRetentionKeepUnplayedOnly)
/// </summary>
[DisallowConcurrentExecution]
public sealed class PodcastCleanupJob(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IFileSystemService fileSystemService)
    : JobBase(logger, configurationFactory)
{
    public override async Task Execute(IJobExecutionContext context)
    {
        var jobId = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();
        Logger.Information("[{JobId}] PodcastCleanupJob started", jobId);

        try
        {
            var configuration = await ConfigurationFactory.GetConfigurationAsync(context.CancellationToken);
            if (!configuration.GetValue<bool>(SettingRegistry.PodcastEnabled))
            {
                Logger.Information("[{JobId}] Podcast support disabled, skipping cleanup", jobId);
                return;
            }

            await using var scopedContext = await contextFactory.CreateDbContextAsync(context.CancellationToken);

            var library = await scopedContext.Libraries.FirstOrDefaultAsync(x => x.Type == (int)LibraryType.Podcast, context.CancellationToken);
            if (library == null)
            {
                Logger.Warning("[{JobId}] Podcast library not found, skipping cleanup", jobId);
                return;
            }

            var episodesToDelete = new List<PodcastEpisode>();

            // Policy 1: Keep for X days
            var retentionDays = configuration.GetValue<int>(SettingRegistry.PodcastRetentionDownloadedEpisodesInDays);
            if (retentionDays > 0)
            {
                var cutoffDate = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(retentionDays));
                var oldEpisodes = await scopedContext.PodcastEpisodes
                    .Where(x => x.DownloadStatus == PodcastEpisodeDownloadStatus.Downloaded &&
                                x.LocalPath != null &&
                                x.LastUpdatedAt != null &&
                                x.LastUpdatedAt < cutoffDate)
                    .ToListAsync(context.CancellationToken);

                episodesToDelete.AddRange(oldEpisodes);
                Logger.Debug("[{JobId}] Policy 'keep for X days': Found {Count} episodes older than {Days} days", jobId, oldEpisodes.Count, retentionDays);
            }

            // Policy 2: Keep last N episodes per channel
            var keepLastN = configuration.GetValue<int>(SettingRegistry.PodcastRetentionKeepLastNEpisodes);
            if (keepLastN > 0)
            {
                var channels = await scopedContext.PodcastChannels
                    .Where(x => !x.IsDeleted)
                    .Select(x => x.Id)
                    .ToListAsync(context.CancellationToken);

                foreach (var channelId in channels)
                {
                    var downloadedEpisodesInChannel = await scopedContext.PodcastEpisodes
                        .Where(x => x.PodcastChannelId == channelId &&
                                    x.DownloadStatus == PodcastEpisodeDownloadStatus.Downloaded &&
                                    x.LocalPath != null)
                        .OrderByDescending(x => x.PublishDate ?? x.CreatedAt)
                        .ToListAsync(context.CancellationToken);

                    if (downloadedEpisodesInChannel.Count > keepLastN)
                    {
                        var excess = downloadedEpisodesInChannel.Skip(keepLastN);
                        episodesToDelete.AddRange(excess);
                        Logger.Debug("[{JobId}] Policy 'keep last N': Channel {ChannelId} has {Count} excess episodes to delete", jobId, channelId, excess.Count());
                    }
                }
            }

            // Policy 3: Keep unplayed only
            var keepUnplayedOnly = configuration.GetValue<bool>(SettingRegistry.PodcastRetentionKeepUnplayedOnly);
            if (keepUnplayedOnly)
            {
                var playedEpisodes = await scopedContext.PodcastEpisodes
                    .Where(x => x.DownloadStatus == PodcastEpisodeDownloadStatus.Downloaded &&
                                x.LocalPath != null &&
                                scopedContext.UserPodcastEpisodePlayHistories.Any(h => h.PodcastEpisodeId == x.Id && !h.IsNowPlaying))
                    .ToListAsync(context.CancellationToken);

                episodesToDelete.AddRange(playedEpisodes);
                Logger.Debug("[{JobId}] Policy 'keep unplayed only': Found {Count} played episodes to delete", jobId, playedEpisodes.Count);
            }

            // Deduplicate
            episodesToDelete = episodesToDelete.DistinctBy(x => x.Id).ToList();

            Logger.Information("[{JobId}] Total episodes to clean up: {Count}", jobId, episodesToDelete.Count);

            foreach (var episode in episodesToDelete)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(episode.LocalPath))
                    {
                        var fullPath = fileSystemService.CombinePath(library.Path, episode.LocalPath);
                        if (fileSystemService.FileExists(fullPath))
                        {
                            fileSystemService.DeleteFile(fullPath);
                            Logger.Debug("[{JobId}] Deleted file {Path}", jobId, fullPath);
                        }
                    }

                    episode.DownloadStatus = PodcastEpisodeDownloadStatus.None;
                    episode.LocalPath = null;
                    episode.LocalFileSize = null;
                    episode.DownloadError = null;
                    episode.LastUpdatedAt = SystemClock.Instance.GetCurrentInstant();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "[{JobId}] Error cleaning up episode {EpisodeId}", jobId, episode.Id);
                }
            }

            if (episodesToDelete.Count > 0)
            {
                await scopedContext.SaveChangesAsync(context.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{JobId}] PodcastCleanupJob failed", jobId);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            Logger.Information("[{JobId}] PodcastCleanupJob finished in {ElapsedMs}ms", jobId, stopwatch.ElapsedMilliseconds);
        }
    }
}

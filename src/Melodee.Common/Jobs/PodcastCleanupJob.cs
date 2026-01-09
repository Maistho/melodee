using System.Diagnostics;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;

namespace Melodee.Common.Jobs;

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
            var configuration = await configurationFactory.GetConfigurationAsync(context.CancellationToken);
            if (!configuration.GetValue<bool>(SettingRegistry.PodcastEnabled))
            {
                Logger.Information("[{JobId}] Podcast support disabled, skipping cleanup", jobId);
                return;
            }

            var retentionDays = configuration.GetValue<int>(SettingRegistry.PodcastRetentionDownloadedEpisodesInDays);
            if (retentionDays <= 0)
            {
                Logger.Information("[{JobId}] Podcast retention disabled (days <= 0), skipping cleanup", jobId);
                return;
            }

            var cutoffDate = NodaTime.SystemClock.Instance.GetCurrentInstant().Minus(NodaTime.Duration.FromDays(retentionDays));

            await using var scopedContext = await contextFactory.CreateDbContextAsync(context.CancellationToken);
            
            var library = await scopedContext.Libraries.FirstOrDefaultAsync(x => x.Type == (int)LibraryType.Podcast, context.CancellationToken);
            if (library == null)
            {
                 Logger.Warning("[{JobId}] Podcast library not found, skipping cleanup", jobId);
                 return;
            }

            var episodesToDelete = await scopedContext.PodcastEpisodes
                .Where(x => x.DownloadStatus == PodcastEpisodeDownloadStatus.Downloaded && 
                            x.LocalPath != null &&
                            x.LastUpdatedAt != null && 
                            x.LastUpdatedAt < cutoffDate)
                .ToListAsync(context.CancellationToken);

            Logger.Information("[{JobId}] Found {Count} episodes to clean up (older than {Days} days)", jobId, episodesToDelete.Count, retentionDays);

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
                    episode.LastUpdatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant();
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

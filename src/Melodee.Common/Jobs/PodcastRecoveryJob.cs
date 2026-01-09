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
public sealed class PodcastRecoveryJob(
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
        Logger.Information("[{JobId}] PodcastRecoveryJob started", jobId);

        try
        {
            var configuration = await configurationFactory.GetConfigurationAsync(context.CancellationToken);
            if (!configuration.GetValue<bool>(SettingRegistry.PodcastEnabled))
            {
                Logger.Information("[{JobId}] Podcast support disabled, skipping recovery", jobId);
                return;
            }

            var stuckThresholdMinutes = configuration.GetValue<int>(SettingRegistry.PodcastRecoveryStuckDownloadThresholdMinutes);
            var orphanedThresholdHours = configuration.GetValue<int>(SettingRegistry.PodcastRecoveryOrphanedUsageThresholdHours);

            var stuckCutoff = NodaTime.SystemClock.Instance.GetCurrentInstant().Minus(NodaTime.Duration.FromMinutes(stuckThresholdMinutes));
            
            await using var scopedContext = await contextFactory.CreateDbContextAsync(context.CancellationToken);
            
            // 1. Reset Stuck Downloads
            var stuckEpisodes = await scopedContext.PodcastEpisodes
                .Where(x => x.DownloadStatus == PodcastEpisodeDownloadStatus.Downloading && 
                            x.LastUpdatedAt != null && 
                            x.LastUpdatedAt < stuckCutoff)
                .ToListAsync(context.CancellationToken);

            if (stuckEpisodes.Count > 0)
            {
                Logger.Information("[{JobId}] Found {Count} stuck downloading episodes", jobId, stuckEpisodes.Count);
                foreach (var episode in stuckEpisodes)
                {
                    episode.DownloadStatus = PodcastEpisodeDownloadStatus.Failed;
                    episode.DownloadError = "Recovery job: download stuck/timed out";
                    episode.LastUpdatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant();
                }
                await scopedContext.SaveChangesAsync(context.CancellationToken);
            }

            // 2. Clean Orphaned Temp Files
            var library = await scopedContext.Libraries.FirstOrDefaultAsync(x => x.Type == (int)LibraryType.Podcast, context.CancellationToken);
            if (library != null && fileSystemService.DirectoryExists(library.Path))
            {
                var orphanedCutoff = DateTime.UtcNow.AddHours(-orphanedThresholdHours);
                
                var tempFiles = fileSystemService.GetFiles(library.Path, "*.tmp", SearchOption.AllDirectories);
                var deletedCount = 0;
                
                foreach (var file in tempFiles)
                {
                    if (!Path.GetFileName(file).EndsWith(".tmp")) continue;

                    if (File.GetLastWriteTimeUtc(file) < orphanedCutoff)
                    {
                        try
                        {
                            fileSystemService.DeleteFile(file);
                            deletedCount++;
                        }
                        catch (Exception ex)
                        {
                            // Logger.Warning(ex, "[{JobId}] Failed to delete orphaned file {Path}", jobId, file);
                        }
                    }
                }
                
                if (deletedCount > 0)
                {
                    Logger.Information("[{JobId}] Deleted {Count} orphaned temp files", jobId, deletedCount);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{JobId}] PodcastRecoveryJob failed", jobId);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            Logger.Information("[{JobId}] PodcastRecoveryJob finished in {ElapsedMs}ms", jobId, stopwatch.ElapsedMilliseconds);
        }
    }
}

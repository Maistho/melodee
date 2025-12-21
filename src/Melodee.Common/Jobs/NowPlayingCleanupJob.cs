using Melodee.Common.Configuration;
using Melodee.Common.Plugins.Scrobbling;
using Quartz;
using Serilog;

namespace Melodee.Common.Jobs;

/// <summary>
///     Background job to clean up stale "now playing" entries in the database.
///     Runs periodically to mark entries as no longer playing if they haven't
///     received a heartbeat within the configured threshold.
/// </summary>
public class NowPlayingCleanupJob(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    NowPlayingDatabaseRepository nowPlayingRepository) : JobBase(logger, configurationFactory)
{
    public override async Task Execute(IJobExecutionContext context)
    {
        Logger.Debug("[{JobName}] Starting now playing cleanup", nameof(NowPlayingCleanupJob));

        var cleanedCount = await nowPlayingRepository.CleanupStaleEntriesAsync(context.CancellationToken)
            .ConfigureAwait(false);

        if (cleanedCount > 0)
        {
            Logger.Information("[{JobName}] Cleaned up [{Count}] stale now playing entries",
                nameof(NowPlayingCleanupJob), cleanedCount);
        }
    }
}

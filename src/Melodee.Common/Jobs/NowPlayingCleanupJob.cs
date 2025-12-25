using Melodee.Common.Configuration;
using Melodee.Common.Plugins.Scrobbling;
using Quartz;
using Serilog;

namespace Melodee.Common.Jobs;

/// <summary>
///     Removes stale "now playing" entries from the database when users stop listening without explicit notification.
/// </summary>
/// <remarks>
///     <para>
///         The OpenSubsonic/Subsonic API uses a "now playing" mechanism where clients periodically report
///         what the user is currently listening to. However, clients don't always send a "stopped playing"
///         notification (e.g., app crash, network disconnect, user closes app).
///     </para>
///     <para>
///         This job cleans up entries that haven't received a heartbeat update within the configured
///         threshold, ensuring the "now playing" list accurately reflects active listeners.
///     </para>
///     <para>
///         Processing flow:
///         <list type="number">
///             <item>Queries the NowPlaying table for entries older than the stale threshold</item>
///             <item>Deletes entries that haven't been updated recently</item>
///             <item>Logs the count of cleaned entries for monitoring</item>
///         </list>
///     </para>
///     <para>
///         This is a lightweight job that runs frequently to keep the now playing display current.
///         The stale threshold is configured in the NowPlayingDatabaseRepository.
///     </para>
/// </remarks>
public class NowPlayingCleanupJob(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    NowPlayingDatabaseRepository nowPlayingRepository) : JobBase(logger, configurationFactory)
{
    /// <summary>
    ///     Disabled for this high-frequency job to avoid cluttering JobHistory.
    /// </summary>
    public override bool DoCreateJobHistory => false;

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

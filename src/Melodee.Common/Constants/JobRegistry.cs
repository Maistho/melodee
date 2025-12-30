using Quartz;

namespace Melodee.Common.Constants;

/// <summary>
///     Central registry of all background jobs in the system.
///     This provides a single source of truth for job metadata used by both CLI and Admin UI.
/// </summary>
public static class JobRegistry
{
    /// <summary>
    ///     Information about a background job.
    /// </summary>
    /// <param name="Description">Human-readable description of what the job does.</param>
    /// <param name="CronSettingKey">The setting key that controls this job's cron schedule. Null for jobs without configurable schedules.</param>
    public record JobInfo(string Description, string? CronSettingKey);

    /// <summary>
    ///     All known jobs in the system with their metadata.
    /// </summary>
    public static readonly IReadOnlyDictionary<JobKey, JobInfo> Jobs = new Dictionary<JobKey, JobInfo>
    {
        [JobKeyRegistry.LibraryInboundProcessJobKey] = new(
            "Processes new files in the inbound library",
            SettingRegistry.JobsLibraryProcessCronExpression),

        [JobKeyRegistry.LibraryProcessJobJobKey] = new(
            "Processes staged albums and moves to storage",
            SettingRegistry.JobsLibraryInsertCronExpression),

        [JobKeyRegistry.MusicBrainzUpdateDatabaseJobKey] = new(
            "Updates local MusicBrainz database from data dumps",
            SettingRegistry.JobsMusicBrainzUpdateDatabaseCronExpression),

        [JobKeyRegistry.ArtistHousekeepingJobJobKey] = new(
            "Performs artist data cleanup and maintenance",
            SettingRegistry.JobsArtistHousekeepingCronExpression),

        [JobKeyRegistry.NowPlayingCleanupJobKey] = new(
            "Cleans up stale now playing records",
            SettingRegistry.JobsNowPlayingCleanupCronExpression),

        [JobKeyRegistry.ArtistSearchEngineHousekeepingJobKey] = new(
            "Refreshes artist data from search engines",
            SettingRegistry.JobsArtistSearchEngineHousekeepingCronExpression),

        [JobKeyRegistry.ChartUpdateJobKey] = new(
            "Updates chart data and links chart items to albums",
            SettingRegistry.JobsChartUpdateCronExpression),

        [JobKeyRegistry.StagingAutoMoveJobKey] = new(
            "Automatically moves approved albums from staging to storage",
            SettingRegistry.JobsStagingAutoMoveCronExpression),

        [JobKeyRegistry.StagingAlbumRevalidationJobKey] = new(
            "Re-validates albums with invalid artists in staging",
            SettingRegistry.JobsStagingAlbumRevalidationCronExpression)
    };

    /// <summary>
    ///     Gets the description for a job by its key name.
    /// </summary>
    public static string GetDescription(string jobName)
    {
        var job = Jobs.FirstOrDefault(j => j.Key.Name == jobName);
        return job.Value?.Description ?? "Unknown job";
    }

    /// <summary>
    ///     Gets the description for a job by its JobKey.
    /// </summary>
    public static string GetDescription(JobKey jobKey)
    {
        return Jobs.TryGetValue(jobKey, out var info) ? info.Description : "Unknown job";
    }

    /// <summary>
    ///     Gets all known job names.
    /// </summary>
    public static IEnumerable<string> AllJobNames => Jobs.Keys.Select(k => k.Name);
}

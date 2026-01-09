using Quartz;

namespace Melodee.Common.Constants;

public static class JobKeyRegistry
{
    public static readonly JobKey LibraryInboundProcessJobKey = new("LibraryInboundProcessJob");
    public static readonly JobKey LibraryProcessJobJobKey = new("LibraryProcessJob");
    public static readonly JobKey MusicBrainzUpdateDatabaseJobKey = new("MusicBrainzUpdateDatabaseJob");
    public static readonly JobKey ArtistHousekeepingJobJobKey = new("ArtistHousekeepingJob");
    public static readonly JobKey NowPlayingCleanupJobKey = new("NowPlayingCleanupJob");

    public static readonly JobKey ArtistSearchEngineHousekeepingJobKey =
        new("ArtistSearchEngineHousekeepingJobJobKey");

    public static readonly JobKey ChartUpdateJobKey = new("ChartUpdateJob");
    public static readonly JobKey StagingAutoMoveJobKey = new("StagingAutoMoveJob");
    public static readonly JobKey StagingAlbumRevalidationJobKey = new("StagingAlbumRevalidationJob");
    public static readonly JobKey PodcastRefreshJobKey = new("PodcastRefreshJob");
    public static readonly JobKey PodcastDownloadJobKey = new("PodcastDownloadJob");
}

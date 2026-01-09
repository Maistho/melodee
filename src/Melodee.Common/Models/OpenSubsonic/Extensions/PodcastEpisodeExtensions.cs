using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Models.OpenSubsonic.Responses;
using System.Globalization;

namespace Melodee.Common.Models.OpenSubsonic.Extensions;

public static class PodcastEpisodeExtensions
{
    public static PodcastEpisodeResponse ToPodcastEpisodeResponse(this PodcastEpisode episode)
    {
        var status = episode.DownloadStatus switch
        {
            PodcastEpisodeDownloadStatus.None => "pending",
            PodcastEpisodeDownloadStatus.Queued => "pending",
            PodcastEpisodeDownloadStatus.Downloading => "loading",
            PodcastEpisodeDownloadStatus.Downloaded => "completed",
            PodcastEpisodeDownloadStatus.Failed => "error",
            _ => "pending"
        };

        return new PodcastEpisodeResponse
        {
            Id = $"podcast:episode:{episode.Id}",
            ChannelId = $"podcast:channel:{episode.PodcastChannelId}",
            Title = episode.Title,
            Description = episode.Description,
            Url = episode.EnclosureUrl,
            PublishDate = episode.PublishDate?.ToString("ddd, dd MMM yyyy HH:mm:ss K", CultureInfo.InvariantCulture) ?? episode.CreatedAt.ToString("ddd, dd MMM yyyy HH:mm:ss K", CultureInfo.InvariantCulture),
            Duration = episode.Duration?.ToString(@"hh\:mm\:ss"),
            Status = status,
            FileSize = episode.LocalFileSize ?? episode.EnclosureLength,
            CoverArt = $"podcast:channel:{episode.PodcastChannelId}",
            StreamId = $"podcast:episode:{episode.Id}"
        };
    }
}

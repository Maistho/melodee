using Melodee.Common.Enums;
using NodaTime;

namespace Melodee.Common.Models.Collection;

public sealed record PodcastEpisodeDataInfo(
    int Id,
    Guid ApiKey,
    string Title,
    string TitleNormalized,
    string Description,
    DateTimeOffset? PublishDate,
    TimeSpan? Duration,
    string ChannelTitle,
    Guid ChannelApiKey,
    bool IsDownloaded,
    Instant CreatedAt,
    string Tags,
    bool UserStarred,
    int UserRating,
    PodcastEpisodeDownloadStatus DownloadStatus = PodcastEpisodeDownloadStatus.None,
    string? DownloadError = null,
    string? EnclosureUrl = null,
    Instant? LastPlayedAt = null,
    int PlayedCount = 0)
{
    public static string InfoLineTitle => "Published | Duration";

    public string InfoLineData => $"{PublishDate} | {Duration}";
}

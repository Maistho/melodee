using NodaTime;

namespace Melodee.Common.Models.Collection;

public sealed record PodcastChannelDataInfo(
    int Id,
    Guid ApiKey,
    string Title,
    string TitleNormalized,
    string Description,
    string ImageUrl,
    string FeedUrl,
    string Website,
    Instant? LastSyncAt,
    Instant CreatedAt,
    string Tags,
    bool UserStarred,
    int UserRating,
    Instant? LastPlayedAt = null,
    int PlayedCount = 0)
{
    public static string InfoLineTitle => "Last Sync";

    public string InfoLineData => LastSyncAt?.ToString() ?? "Never";
}

using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
public class PodcastChannel : DataModelBase
{
    public required int UserId { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    [Required]
    public required string FeedUrl { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public required string Title { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? TitleNormalized { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxTextLength)]
    public new string? Description { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? SiteUrl { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? ImageUrl { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? CoverArtLocalPath { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? Etag { get; set; }

    public DateTimeOffset? LastModified { get; set; }

    public Instant? LastSyncAt { get; set; }

    public Instant? LastSyncAttemptAt { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? LastSyncError { get; set; }

    public int ConsecutiveFailureCount { get; set; }

    public Instant? NextSyncAt { get; set; }

    public bool IsDeleted { get; set; }

    /// <summary>
    ///     Maximum number of downloaded episodes to keep for this channel. 0 or null means unlimited (uses global setting).
    /// </summary>
    public int? MaxDownloadedEpisodes { get; set; }

    /// <summary>
    ///     Maximum storage bytes for downloaded episodes in this channel. 0 or null means unlimited (uses global/user quota).
    /// </summary>
    public long? MaxStorageBytes { get; set; }

    /// <summary>
    ///     When true, new episodes discovered during refresh are automatically queued for download.
    ///     Default is false (manual download only).
    /// </summary>
    public bool AutoDownloadEnabled { get; set; }

    /// <summary>
    ///     Per-channel refresh interval in hours. Null means use the global job schedule.
    ///     Allows high-frequency podcasts (e.g., news) to refresh more often than others.
    /// </summary>
    public int? RefreshIntervalHours { get; set; }

    public ICollection<PodcastEpisode> Episodes { get; set; } = new List<PodcastEpisode>();
}

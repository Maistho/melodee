using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Constants;
using Melodee.Common.Enums;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
public class PodcastEpisode : DataModelBase
{
    public required int PodcastChannelId { get; set; }

    [ForeignKey(nameof(PodcastChannelId))]
    public PodcastChannel? PodcastChannel { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? Guid { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public required string Title { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxTextLength)]
    public string? Description { get; set; }

    public DateTimeOffset? PublishDate { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public required string EnclosureUrl { get; set; }

    public long? EnclosureLength { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? MimeType { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    [Required]
    public required string EpisodeKey { get; set; }

    public PodcastEpisodeDownloadStatus DownloadStatus { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? DownloadError { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? LocalPath { get; set; }

    public long? LocalFileSize { get; set; }

    public TimeSpan? Duration { get; set; }
}

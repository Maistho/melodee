using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

/// <summary>
/// Stores resume position for podcast episodes per user.
/// Allows users to resume playback where they left off.
/// Mirrors the Bookmark model structure but for podcast episodes.
/// </summary>
[Serializable]
[Index(nameof(UserId), nameof(PodcastEpisodeId), IsUnique = true)]
public class PodcastEpisodeBookmark
{
    public int Id { get; set; }

    [RequiredGreaterThanZero]
    public required int UserId { get; set; }

    public User User { get; set; } = null!;

    [RequiredGreaterThanZero]
    public required int PodcastEpisodeId { get; set; }

    public PodcastEpisode PodcastEpisode { get; set; } = null!;

    /// <summary>
    /// Resume position in seconds.
    /// </summary>
    [Required]
    public required int PositionSeconds { get; set; }

    /// <summary>
    /// Optional comment/note about this bookmark.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public string? Comment { get; set; }

    /// <summary>
    /// When this bookmark was created.
    /// </summary>
    [Required]
    public Instant CreatedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();

    /// <summary>
    /// Last time this bookmark was updated.
    /// </summary>
    [Required]
    public Instant UpdatedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();
}

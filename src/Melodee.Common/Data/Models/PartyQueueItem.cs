using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using NodaTime;

namespace Melodee.Common.Data.Models;

/// <summary>
/// Represents an item in a party queue.
/// </summary>
[Serializable]
public class PartyQueueItem : DataModelBase
{
    [RequiredGreaterThanZero]
    public int PartySessionId { get; set; }

    public PartySession PartySession { get; set; } = null!;

    /// <summary>
    /// The API key of the song being queued.
    /// </summary>
    [Required]
    public Guid SongApiKey { get; set; }

    /// <summary>
    /// The user who enqueued this item.
    /// </summary>
    [RequiredGreaterThanZero]
    public int EnqueuedByUserId { get; set; }

    public User EnqueuedByUser { get; set; } = null!;

    [Required]
    public Instant EnqueuedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();

    /// <summary>
    /// Sort order for the queue item.
    /// </summary>
    [Required]
    public new int SortOrder { get; set; }

    /// <summary>
    /// Source of the queued item (e.g., "album", "playlist", "search").
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? Source { get; set; }

    /// <summary>
    /// Optional note for the queued item.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    public string? Note { get; set; }
}

using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Validators;
using NodaTime;

namespace Melodee.Common.Data.Models;

/// <summary>
/// Represents the playback state for a party session.
/// </summary>
[Serializable]
public class PartyPlaybackState : DataModelBase
{
    [RequiredGreaterThanZero]
    public int PartySessionId { get; set; }

    public PartySession PartySession { get; set; } = null!;

    /// <summary>
    /// The current queue item being played.
    /// </summary>
    public Guid? CurrentQueueItemApiKey { get; set; }

    public PartyQueueItem? CurrentQueueItem { get; set; }

    /// <summary>
    /// Current playback position in seconds.
    /// </summary>
    [Required]
    public double PositionSeconds { get; set; }

    /// <summary>
    /// Whether playback is currently active.
    /// </summary>
    [Required]
    public bool IsPlaying { get; set; }

    /// <summary>
    /// Volume level from 0.0 to 1.0.
    /// </summary>
    public double? Volume { get; set; }

    /// <summary>
    /// Last heartbeat timestamp from the endpoint.
    /// </summary>
    public Instant? LastHeartbeatAt { get; set; }

    /// <summary>
    /// User who last updated the playback state.
    /// </summary>
    public int? UpdatedByUserId { get; set; }

    public User? UpdatedByUser { get; set; }
}

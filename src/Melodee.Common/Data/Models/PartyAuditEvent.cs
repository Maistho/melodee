using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;

namespace Melodee.Common.Data.Models;

/// <summary>
/// Represents an audit event for a party session.
/// </summary>
[Serializable]
public class PartyAuditEvent : DataModelBase
{
    [RequiredGreaterThanZero]
    public int PartySessionId { get; set; }

    public PartySession PartySession { get; set; } = null!;

    [RequiredGreaterThanZero]
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    [Required]
    public PartyAuditEventType EventType { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxTextLength)]
    public string? PayloadJson { get; set; }
}

/// <summary>
/// Types of audit events that can occur in a party session.
/// </summary>
public enum PartyAuditEventType
{
    QueueItemAdded = 1,
    QueueItemRemoved = 2,
    QueueItemReordered = 3,
    QueueCleared = 4,
    QueueLocked = 5,
    QueueUnlocked = 6,
    PlaybackPlayed = 7,
    PlaybackPaused = 8,
    PlaybackSkipped = 9,
    PlaybackSeeked = 10,
    PlaybackVolumeChanged = 11,
    ParticipantJoined = 12,
    ParticipantLeft = 13,
    RoleChanged = 14,
    ParticipantKicked = 15,
    ParticipantBanned = 16,
    ParticipantUnbanned = 17,
    SessionEnded = 18,
    EndpointHeartbeat = 19
}

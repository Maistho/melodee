using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums.PartyMode;

namespace Melodee.Common.Data.Models;

/// <summary>
/// Represents a party session for shared music playback.
/// </summary>
[Serializable]
public class PartySession : DataModelBase
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Name { get; set; }

    [RequiredGreaterThanZero]
    public int OwnerUserId { get; set; }

    public User OwnerUser { get; set; } = null!;

    [Required]
    public PartySessionStatus Status { get; set; } = PartySessionStatus.Active;

    /// <summary>
    /// Hashed join code for the session. Null if no join code is required.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.HashOrGuidLength)]
    public string? JoinCodeHash { get; set; }

    /// <summary>
    /// The currently active endpoint for this session.
    /// </summary>
    public Guid? ActiveEndpointId { get; set; }

    public PartySessionEndpoint? ActiveEndpoint { get; set; }

    /// <summary>
    /// Revision number for optimistic concurrency on queue operations.
    /// </summary>
    [Required]
    public long QueueRevision { get; set; }

    /// <summary>
    /// Revision number for optimistic concurrency on playback operations.
    /// </summary>
    [Required]
    public long PlaybackRevision { get; set; }

    public ICollection<PartySessionParticipant> Participants { get; set; } = new List<PartySessionParticipant>();

    public ICollection<PartyQueueItem> QueueItems { get; set; } = new List<PartyQueueItem>();

    public PartyPlaybackState? PlaybackState { get; set; }
}

using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Melodee.Common.Enums.PartyMode;
using NodaTime;

namespace Melodee.Common.Data.Models;

/// <summary>
/// Represents an endpoint that can be attached to a party session for playback.
/// </summary>
[Serializable]
public class PartySessionEndpoint : DataModelBase
{
    /// <summary>
    /// The user who owns this endpoint (if any).
    /// </summary>
    public int? OwnerUserId { get; set; }

    public User? OwnerUser { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Name { get; set; }

    [Required]
    public PartySessionEndpointType Type { get; set; }

    /// <summary>
    /// JSON-encoded capabilities of this endpoint.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxTextLength)]
    public string? CapabilitiesJson { get; set; }

    /// <summary>
    /// Last time this endpoint was seen/heartbeat.
    /// </summary>
    public Instant? LastSeenAt { get; set; }

    /// <summary>
    /// Whether this endpoint is shared among multiple users.
    /// </summary>
    [Required]
    public bool IsShared { get; set; }

    /// <summary>
    /// Optional room identifier for multi-room setups.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? Room { get; set; }
}

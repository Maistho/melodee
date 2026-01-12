using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums.PartyMode;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

/// <summary>
/// Represents a participant in a party session.
/// </summary>
[Serializable]
[PrimaryKey(nameof(PartySessionId), nameof(UserId))]
public class PartySessionParticipant
{
    [RequiredGreaterThanZero]
    public int PartySessionId { get; set; }

    public PartySession PartySession { get; set; } = null!;

    [RequiredGreaterThanZero]
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    [Required]
    public PartyRole Role { get; set; } = PartyRole.Listener;

    [Required]
    public Instant JoinedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();

    public Instant? LastSeenAt { get; set; }

    /// <summary>
    /// Whether this participant is banned from the session.
    /// </summary>
    [Required]
    public bool IsBanned { get; set; }
}

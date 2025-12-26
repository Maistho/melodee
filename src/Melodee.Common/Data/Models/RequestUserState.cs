using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

/// <summary>
/// Tracks per-user per-request last_seen_at for O(1) unread checks.
/// </summary>
[Serializable]
[PrimaryKey(nameof(RequestId), nameof(UserId))]
public class RequestUserState
{
    [RequiredGreaterThanZero]
    public int RequestId { get; set; }

    public Request Request { get; set; } = null!;

    [RequiredGreaterThanZero]
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public Instant LastSeenAt { get; set; }

    public Instant CreatedAt { get; set; }

    public Instant UpdatedAt { get; set; }
}

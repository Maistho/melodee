using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

/// <summary>
/// Tracks "in-scope" requests for navbar unread indicator: creator OR commenter.
/// This denormalized table makes hasUnread a cheap indexed join.
/// </summary>
[Serializable]
[PrimaryKey(nameof(RequestId), nameof(UserId))]
public class RequestParticipant
{
    [RequiredGreaterThanZero]
    public int RequestId { get; set; }

    public Request Request { get; set; } = null!;

    [RequiredGreaterThanZero]
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public bool IsCreator { get; set; }

    public bool IsCommenter { get; set; }

    public Instant CreatedAt { get; set; }
}

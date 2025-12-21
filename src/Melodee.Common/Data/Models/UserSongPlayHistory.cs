using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using NodaTime;

namespace Melodee.Common.Data.Models;

public class UserSongPlayHistory
{
    public int Id { get; set; }

    [RequiredGreaterThanZero] public int UserId { get; set; }

    public User User { get; set; } = null!;

    [RequiredGreaterThanZero] public int SongId { get; set; }

    public Song Song { get; set; } = null!;

    [Required] public Instant PlayedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public string Client { get; set; } = "Melodee";

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? ByUserAgent { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? IpAddress { get; set; }

    public int? SecondsPlayed { get; set; }

    /// <summary>
    ///     Source of the play (0 = Unknown, 1 = Stream, 2 = Share, 3 = Radio)
    /// </summary>
    public short Source { get; set; }

    /// <summary>
    ///     True if this song is currently being played (not yet scrobbled/completed).
    /// </summary>
    public bool IsNowPlaying { get; set; }

    /// <summary>
    ///     Last time the client sent a "now playing" heartbeat.
    ///     Used to detect stale entries (client disconnected without scrobbling).
    /// </summary>
    public Instant? LastHeartbeatAt { get; set; }
}

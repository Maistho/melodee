using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

/// <summary>
/// Tracks each time a user plays a podcast episode.
/// Mirrors UserSongPlayHistory but for podcast episodes.
/// </summary>
[Index(nameof(UserId), nameof(PodcastEpisodeId), nameof(PlayedAt))]
[Index(nameof(PodcastEpisodeId), nameof(PlayedAt))]
public class UserPodcastEpisodePlayHistory
{
    public int Id { get; set; }

    [RequiredGreaterThanZero]
    public required int UserId { get; set; }

    public User User { get; set; } = null!;

    [RequiredGreaterThanZero]
    public required int PodcastEpisodeId { get; set; }

    public PodcastEpisode PodcastEpisode { get; set; } = null!;

    [Required]
    public Instant PlayedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public string Client { get; set; } = "Melodee";

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? ByUserAgent { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Number of seconds played in this session.
    /// Used for scrobbling logic (typically needs 50%+ or 240+ seconds).
    /// </summary>
    public int? SecondsPlayed { get; set; }

    /// <summary>
    /// Source of the play (0 = Unknown, 1 = Stream, 2 = Share, 3 = Radio, 4 = Podcast)
    /// </summary>
    public short Source { get; set; } = 4; // Default to Podcast

    /// <summary>
    /// True if this episode is currently being played (not yet scrobbled/completed).
    /// </summary>
    public bool IsNowPlaying { get; set; }

    /// <summary>
    /// Last time the client sent a "now playing" heartbeat.
    /// Used to detect stale entries (client disconnected without scrobbling).
    /// </summary>
    public Instant? LastHeartbeatAt { get; set; }
}

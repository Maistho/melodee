using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

/// <summary>
/// User playback preferences.
/// </summary>
[Serializable]
[Index(nameof(UserId), IsUnique = true)]
public class UserPlaybackSettings : DataModelBase
{
    public User User { get; set; } = null!;

    public int UserId { get; set; }

    /// <summary>
    /// Crossfade duration in seconds. Must be >= 0.
    /// </summary>
    public double CrossfadeDuration { get; set; }

    /// <summary>
    /// Enable gapless playback.
    /// </summary>
    public bool GaplessPlayback { get; set; } = true;

    /// <summary>
    /// Enable volume normalization.
    /// </summary>
    public bool VolumeNormalization { get; set; }

    /// <summary>
    /// Replay gain mode: "none", "track", "album".
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string ReplayGain { get; set; } = "none";

    /// <summary>
    /// Audio quality: "low", "medium", "high", "lossless".
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string AudioQuality { get; set; } = "high";

    /// <summary>
    /// Name of the equalizer preset to use.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? EqualizerPreset { get; set; }

    /// <summary>
    /// Last device used for playback. Set automatically by the system.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? LastUsedDevice { get; set; }
}

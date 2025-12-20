namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// User playback preferences.
/// </summary>
public record PlaybackSettings(
    double CrossfadeDuration,
    bool GaplessPlayback,
    bool VolumeNormalization,
    string ReplayGain,
    string AudioQuality,
    string? EqualizerPreset,
    string? LastUsedDevice);

/// <summary>
/// Request to update playback settings. All fields are optional for partial updates.
/// </summary>
public record UpdatePlaybackSettingsRequest(
    double? CrossfadeDuration,
    bool? GaplessPlayback,
    bool? VolumeNormalization,
    string? ReplayGain,
    string? AudioQuality,
    string? EqualizerPreset);

using System.Text.Json.Serialization;

namespace Melodee.Common.Models.PartyMode;

/// <summary>
/// Represents the capabilities of a party session endpoint.
/// </summary>
public class PartySessionEndpointCapabilities
{
    /// <summary>
    /// Whether the endpoint can play audio.
    /// </summary>
    [JsonPropertyName("canPlay")]
    public bool CanPlay { get; set; } = true;

    /// <summary>
    /// Whether the endpoint can pause playback.
    /// </summary>
    [JsonPropertyName("canPause")]
    public bool CanPause { get; set; } = true;

    /// <summary>
    /// Whether the endpoint can skip tracks.
    /// </summary>
    [JsonPropertyName("canSkip")]
    public bool CanSkip { get; set; } = true;

    /// <summary>
    /// Whether the endpoint can seek to a specific position.
    /// </summary>
    [JsonPropertyName("canSeek")]
    public bool CanSeek { get; set; } = true;

    /// <summary>
    /// Whether the endpoint volume can be controlled.
    /// </summary>
    [JsonPropertyName("canSetVolume")]
    public bool CanSetVolume { get; set; } = true;

    /// <summary>
    /// Whether the endpoint can report its current playback position.
    /// </summary>
    [JsonPropertyName("canReportPosition")]
    public bool CanReportPosition { get; set; } = true;

    /// <summary>
    /// Preferred audio output device (if applicable).
    /// </summary>
    [JsonPropertyName("audioDevice")]
    public string? AudioDevice { get; set; }

    /// <summary>
    /// Maximum volume level (0.0-1.0).
    /// </summary>
    [JsonPropertyName("maxVolume")]
    public double MaxVolume { get; set; } = 1.0;

    /// <summary>
    /// Minimum volume level (0.0-1.0).
    /// </summary>
    [JsonPropertyName("minVolume")]
    public double MinVolume { get; set; } = 0.0;

    /// <summary>
    /// Supported audio formats.
    /// </summary>
    [JsonPropertyName("supportedFormats")]
    public string[] SupportedFormats { get; set; } = { "mp3", "flac", "ogg", "wav" };

    /// <summary>
    /// Endpoint display name or friendly name.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Creates default capabilities for a web player.
    /// </summary>
    public static PartySessionEndpointCapabilities WebPlayerDefault()
    {
        return new PartySessionEndpointCapabilities
        {
            CanPlay = true,
            CanPause = true,
            CanSkip = true,
            CanSeek = true,
            CanSetVolume = true,
            CanReportPosition = true,
            DisplayName = "Web Player"
        };
    }

    /// <summary>
    /// Creates default capabilities for a system backend (MPV, etc.).
    /// </summary>
    public static PartySessionEndpointCapabilities SystemBackendDefault(string backendType)
    {
        return new PartySessionEndpointCapabilities
        {
            CanPlay = true,
            CanPause = true,
            CanSkip = true,
            CanSeek = true,
            CanSetVolume = true,
            CanReportPosition = true,
            DisplayName = $"{backendType} Backend",
            SupportedFormats = backendType switch
            {
                "mpv" => new[] { "mp3", "flac", "ogg", "wav", "m4a", "aac", "opus" },
                "mpd" => new[] { "mp3", "flac", "ogg", "wav", "m4a", "aac" },
                _ => new[] { "mp3", "flac", "ogg", "wav" }
            }
        };
    }

    /// <summary>
    /// Checks if a specific playback control is available.
    /// </summary>
    public bool CanControl(PlaybackControlType control)
    {
        return control switch
        {
            PlaybackControlType.Play => CanPlay,
            PlaybackControlType.Pause => CanPause,
            PlaybackControlType.Skip => CanSkip,
            PlaybackControlType.Seek => CanSeek,
            PlaybackControlType.Volume => CanSetVolume,
            _ => false
        };
    }
}

/// <summary>
/// Types of playback controls that can be enabled/disabled based on capabilities.
/// </summary>
public enum PlaybackControlType
{
    Play = 1,
    Pause = 2,
    Skip = 3,
    Seek = 4,
    Volume = 5
}

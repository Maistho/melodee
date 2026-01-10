namespace Melodee.Common.Configuration;

/// <summary>
/// Configuration options for Jukebox feature.
/// </summary>
public class JukeboxOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Jukebox";

    /// <summary>
    /// Whether Jukebox feature is enabled. Default is false.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// The type of backend to use for jukebox playback (e.g., "mpv"). Optional.
    /// </summary>
    public string? BackendType { get; set; }
}

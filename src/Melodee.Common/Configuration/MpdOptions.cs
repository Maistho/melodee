namespace Melodee.Common.Configuration;

/// <summary>
/// Configuration options for MPD (Music Player Daemon) playback backend.
/// </summary>
public class MpdOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Mpd";

    /// <summary>
    /// Unique name/identifier for this MPD instance (used for multi-instance support).
    /// </summary>
    public string? InstanceName { get; set; }

    /// <summary>
    /// Hostname or IP address of the MPD server. Default is "localhost".
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Port number for MPD connection. Default is 6600.
    /// </summary>
    public int Port { get; set; } = 6600;

    /// <summary>
    /// Password for MPD authentication. If null, no password is sent.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Timeout for TCP connection and operations in milliseconds. Default is 10000 (10 seconds).
    /// </summary>
    public int TimeoutMs { get; set; } = 10000;

    /// <summary>
    /// Volume level on startup (0.0 to 1.0). Default is 0.8.
    /// </summary>
    public double InitialVolume { get; set; } = 0.8;

    /// <summary>
    /// Whether to enable debug logging for MPD commands. Default is false.
    /// </summary>
    public bool EnableDebugOutput { get; set; } = false;
}

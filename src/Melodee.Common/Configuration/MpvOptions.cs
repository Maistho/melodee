namespace Melodee.Common.Configuration;

/// <summary>
/// Configuration options for MPV playback backend.
/// </summary>
public class MpvOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Mpv";

    /// <summary>
    /// Path to the MPV executable. If null, uses system PATH.
    /// </summary>
    public string? MpvPath { get; set; }

    /// <summary>
    /// Audio device to use. If null, uses default device.
    /// </summary>
    public string? AudioDevice { get; set; }

    /// <summary>
    /// Extra command-line arguments to pass to MPV.
    /// </summary>
    public string? ExtraArgs { get; set; }

    /// <summary>
    /// Command template for starting MPV. Supports placeholders:
    /// {socket_path} - IPC socket path
    /// {mpv_path} - MPV executable path
    /// {audio_device} - Audio device name
    /// {extra_args} - Extra arguments
    /// </summary>
    public string? CmdTemplate { get; set; }

    /// <summary>
    /// Path for the IPC socket. If null, uses temp directory.
    /// </summary>
    public string? SocketPath { get; set; }

    /// <summary>
    /// Volume level on startup (0.0 to 1.0). Default is 0.8.
    /// </summary>
    public double InitialVolume { get; set; } = 0.8;

    /// <summary>
    /// Whether to enable audio output debugging. Default is false.
    /// </summary>
    public bool EnableDebugOutput { get; set; } = false;
}

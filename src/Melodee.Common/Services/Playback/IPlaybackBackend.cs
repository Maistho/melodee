using Melodee.Common.Data.Models;
using Melodee.Common.Models;

namespace Melodee.Common.Services.Playback;

/// <summary>
/// Interface for playback backend implementations.
/// </summary>
public interface IPlaybackBackend
{
    /// <summary>
    /// Gets the capabilities of this backend.
    /// </summary>
    Task<BackendCapabilities> GetCapabilitiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts playback of a queue item.
    /// </summary>
    Task PlayAsync(PartyQueueItem item, double startPositionSeconds = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses playback.
    /// </summary>
    Task PauseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes playback.
    /// </summary>
    Task ResumeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops playback.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeks to a specific position.
    /// </summary>
    Task SeekAsync(double positionSeconds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the playback volume.
    /// </summary>
    Task SetVolumeAsync(double volume01, CancellationToken cancellationToken = default);

    /// <summary>
    /// Skips to the next track.
    /// </summary>
    Task SkipNextAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Skips to the previous track.
    /// </summary>
    Task SkipPreviousAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current status of the backend.
    /// </summary>
    Task<BackendStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the backend. Called when the backend is first configured.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Shuts down the backend. Called when the backend is being disposed.
    /// </summary>
    Task ShutdownAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the capabilities of a playback backend.
/// </summary>
public class BackendCapabilities
{
    /// <summary>
    /// Whether the backend can play audio.
    /// </summary>
    public bool CanPlay { get; init; } = true;

    /// <summary>
    /// Whether the backend can pause playback.
    /// </summary>
    public bool CanPause { get; init; } = true;

    /// <summary>
    /// Whether the backend can stop playback.
    /// </summary>
    public bool CanStop { get; init; } = true;

    /// <summary>
    /// Whether the backend can seek to a position.
    /// </summary>
    public bool CanSeek { get; init; } = true;

    /// <summary>
    /// Whether the backend can skip to next/previous tracks.
    /// </summary>
    public bool CanSkip { get; init; } = true;

    /// <summary>
    /// Whether the backend can set volume.
    /// </summary>
    public bool CanSetVolume { get; init; } = true;

    /// <summary>
    /// Whether the backend reports position updates.
    /// </summary>
    public bool CanReportPosition { get; init; } = true;

    /// <summary>
    /// Whether the backend is currently available.
    /// </summary>
    public bool IsAvailable { get; init; } = true;

    /// <summary>
    /// Backend-specific information (e.g., MPV version).
    /// </summary>
    public string? BackendInfo { get; init; }
}

/// <summary>
/// Represents the current status of a playback backend.
/// </summary>
public class BackendStatus
{
    /// <summary>
    /// Whether playback is currently active.
    /// </summary>
    public bool IsPlaying { get; init; }

    /// <summary>
    /// Current playback position in seconds.
    /// </summary>
    public double PositionSeconds { get; init; }

    /// <summary>
    /// Current volume level (0.0 to 1.0).
    /// </summary>
    public double? Volume { get; init; }

    /// <summary>
    /// Currently playing item API key, if any.
    /// </summary>
    public Guid? CurrentItemApiKey { get; init; }

    /// <summary>
    /// Whether the backend is connected and responsive.
    /// </summary>
    public bool IsConnected { get; init; }

    /// <summary>
    /// Backend-specific status message.
    /// </summary>
    public string? StatusMessage { get; init; }

    /// <summary>
    /// Error message if the backend is in an error state.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

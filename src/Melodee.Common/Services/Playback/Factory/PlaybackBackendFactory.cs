using Melodee.Common.Configuration;
using Melodee.Common.Services.Playback.Backends;
using Microsoft.Extensions.Options;
using Serilog;

namespace Melodee.Common.Services.Playback.Factory;

/// <summary>
/// Factory for creating playback backend instances based on configuration.
/// </summary>
public sealed class PlaybackBackendFactory
{
    private readonly ILogger _logger;
    private readonly IOptions<MpvOptions> _mpvOptions;
    private readonly IOptions<MpdOptions> _mpdOptions;
    private IPlaybackBackend? _cachedBackend;

    public PlaybackBackendFactory(
        ILogger logger,
        IOptions<MpvOptions> mpvOptions,
        IOptions<MpdOptions> mpdOptions)
    {
        _logger = logger;
        _mpvOptions = mpvOptions;
        _mpdOptions = mpdOptions;
    }

    /// <summary>
    /// Creates a playback backend instance based on the configured backend type.
    /// </summary>
    /// <param name="backendType">The type of backend to create (e.g., "mpv", "mpd").</param>
    /// <returns>A new IPlaybackBackend instance, or null if the backend type is not supported.</returns>
    public IPlaybackBackend? CreateBackend(string? backendType)
    {
        if (string.IsNullOrEmpty(backendType))
        {
            _logger.Debug("[PlaybackBackendFactory] No backend type specified, returning null");
            return null;
        }

        return backendType.ToLowerInvariant() switch
        {
            "mpv" => CreateMpvBackend(),
            "mpd" => CreateMpdBackend(),
            _ => null
        };
    }

    /// <summary>
    /// Gets or creates the singleton MPV backend instance.
    /// </summary>
    public IPlaybackBackend? GetOrCreateMpvBackend()
    {
        if (_cachedBackend != null)
        {
            return _cachedBackend;
        }

        _cachedBackend = CreateMpvBackend();
        return _cachedBackend;
    }

    /// <summary>
    /// Gets or creates the singleton MPD backend instance.
    /// </summary>
    public IPlaybackBackend? GetOrCreateMpdBackend()
    {
        if (_cachedBackend != null)
        {
            return _cachedBackend;
        }

        _cachedBackend = CreateMpdBackend();
        return _cachedBackend;
    }

    private IPlaybackBackend? CreateMpvBackend()
    {
        try
        {
            var backend = new MpvPlaybackBackend(_logger, _mpvOptions.Value);
            _logger.Information("[PlaybackBackendFactory] Created MPV playback backend");
            return backend;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[PlaybackBackendFactory] Failed to create MPV playback backend");
            return null;
        }
    }

    private IPlaybackBackend? CreateMpdBackend()
    {
        try
        {
            var backend = new MpdPlaybackBackend(_logger, _mpdOptions.Value);
            _logger.Information("[PlaybackBackendFactory] Created MPD playback backend for {InstanceName}",
                _mpdOptions.Value.InstanceName ?? "default");
            return backend;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[PlaybackBackendFactory] Failed to create MPD playback backend");
            return null;
        }
    }

    /// <summary>
    /// Disposes the cached backend instance if one exists.
    /// </summary>
    public void DisposeBackend()
    {
        if (_cachedBackend is IDisposable disposable)
        {
            disposable.Dispose();
            _cachedBackend = null;
        }
    }
}

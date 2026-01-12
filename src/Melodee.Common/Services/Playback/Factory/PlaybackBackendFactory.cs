using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Services.Playback.Backends;
using Serilog;

namespace Melodee.Common.Services.Playback.Factory;

/// <summary>
/// Factory for creating playback backend instances based on configuration.
/// </summary>
public sealed class PlaybackBackendFactory
{
    private readonly ILogger _logger;
    private readonly IMelodeeConfigurationFactory _configurationFactory;
    private IPlaybackBackend? _cachedBackend;

    public PlaybackBackendFactory(
        ILogger logger,
        IMelodeeConfigurationFactory configurationFactory)
    {
        _logger = logger;
        _configurationFactory = configurationFactory;
    }

    /// <summary>
    /// Creates a playback backend instance based on the configured backend type.
    /// </summary>
    /// <param name="backendType">The type of backend to create (e.g., "mpv", "mpd").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new IPlaybackBackend instance, or null if the backend type is not supported.</returns>
    public async Task<IPlaybackBackend?> CreateBackendAsync(string? backendType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(backendType))
        {
            _logger.Debug("[PlaybackBackendFactory] No backend type specified, returning null");
            return null;
        }

        return backendType.ToLowerInvariant() switch
        {
            "mpv" => await CreateMpvBackendAsync(cancellationToken).ConfigureAwait(false),
            "mpd" => await CreateMpdBackendAsync(cancellationToken).ConfigureAwait(false),
            _ => null
        };
    }

    /// <summary>
    /// Gets or creates the singleton MPV backend instance.
    /// </summary>
    public async Task<IPlaybackBackend?> GetOrCreateMpvBackendAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedBackend != null)
        {
            return _cachedBackend;
        }

        _cachedBackend = await CreateMpvBackendAsync(cancellationToken).ConfigureAwait(false);
        return _cachedBackend;
    }

    /// <summary>
    /// Gets or creates the singleton MPD backend instance.
    /// </summary>
    public async Task<IPlaybackBackend?> GetOrCreateMpdBackendAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedBackend != null)
        {
            return _cachedBackend;
        }

        _cachedBackend = await CreateMpdBackendAsync(cancellationToken).ConfigureAwait(false);
        return _cachedBackend;
    }

    private async Task<IPlaybackBackend?> CreateMpvBackendAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var configuration = await _configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);

            var mpvPath = configuration.GetValue<string>(SettingRegistry.MpvPath);
            var audioDevice = configuration.GetValue<string>(SettingRegistry.MpvAudioDevice);
            var extraArgs = configuration.GetValue<string>(SettingRegistry.MpvExtraArgs);
            var socketPath = configuration.GetValue<string>(SettingRegistry.MpvSocketPath);
            var initialVolume = configuration.GetValue<double>(SettingRegistry.MpvInitialVolume);
            var enableDebugOutput = configuration.GetValue<bool>(SettingRegistry.MpvEnableDebugOutput);

            var backend = new MpvPlaybackBackend(
                _logger,
                string.IsNullOrEmpty(mpvPath) ? null : mpvPath,
                string.IsNullOrEmpty(audioDevice) ? null : audioDevice,
                string.IsNullOrEmpty(extraArgs) ? null : extraArgs,
                string.IsNullOrEmpty(socketPath) ? null : socketPath,
                initialVolume > 0 ? initialVolume : 0.8,
                enableDebugOutput);

            _logger.Information("[PlaybackBackendFactory] Created MPV playback backend");
            return backend;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[PlaybackBackendFactory] Failed to create MPV playback backend");
            return null;
        }
    }

    private async Task<IPlaybackBackend?> CreateMpdBackendAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var configuration = await _configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);

            var instanceName = configuration.GetValue<string>(SettingRegistry.MpdInstanceName);
            var host = configuration.GetValue<string>(SettingRegistry.MpdHost);
            var port = configuration.GetValue<int>(SettingRegistry.MpdPort);
            var password = configuration.GetValue<string>(SettingRegistry.MpdPassword);
            var timeoutMs = configuration.GetValue<int>(SettingRegistry.MpdTimeoutMs);
            var initialVolume = configuration.GetValue<double>(SettingRegistry.MpdInitialVolume);
            var enableDebugOutput = configuration.GetValue<bool>(SettingRegistry.MpdEnableDebugOutput);

            var backend = new MpdPlaybackBackend(
                _logger,
                string.IsNullOrEmpty(instanceName) ? null : instanceName,
                string.IsNullOrEmpty(host) ? "localhost" : host,
                port > 0 ? port : 6600,
                string.IsNullOrEmpty(password) ? null : password,
                timeoutMs > 0 ? timeoutMs : 10000,
                initialVolume > 0 ? initialVolume : 0.8,
                enableDebugOutput);

            _logger.Information("[PlaybackBackendFactory] Created MPD playback backend for {InstanceName}",
                instanceName ?? "default");
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

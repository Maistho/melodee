using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Melodee.Common.Data.Models;
using Serilog;

namespace Melodee.Common.Services.Playback.Backends;

/// <summary>
/// MPV playback backend implementation using IPC socket communication.
/// </summary>
public sealed class MpvPlaybackBackend : IPlaybackBackend, IDisposable
{
    private readonly ILogger _logger;
    private readonly string? _mpvPath;
    private readonly string? _audioDevice;
    private readonly string? _extraArgs;
    private readonly string? _socketPath;
    private readonly double _initialVolume;
    private readonly bool _enableDebugOutput;

    private Process? _mpvProcess;
    private bool _disposed;
    private string? _actualSocketPath;
    private readonly object _lock = new();
    private bool _isInitialized;
    private string? _mpvVersion;
    private Socket? _ipcSocket;
    private NetworkStream? _ipcStream;
    private StreamReader? _ipcReader;
    private StreamWriter? _ipcWriter;
    private int _requestId;

    public MpvPlaybackBackend(
        ILogger logger,
        string? mpvPath,
        string? audioDevice,
        string? extraArgs,
        string? socketPath,
        double initialVolume,
        bool enableDebugOutput)
    {
        _logger = logger;
        _mpvPath = mpvPath;
        _audioDevice = audioDevice;
        _extraArgs = extraArgs;
        _socketPath = socketPath;
        _initialVolume = initialVolume > 0 ? initialVolume : 0.8;
        _enableDebugOutput = enableDebugOutput;
    }

    /// <inheritdoc />
    public Task<BackendCapabilities> GetCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new BackendCapabilities
        {
            CanPlay = true,
            CanPause = true,
            CanStop = true,
            CanSeek = true,
            CanSkip = true,
            CanSetVolume = true,
            CanReportPosition = true,
            IsAvailable = _mpvProcess != null && !_mpvProcess.HasExited && _ipcSocket?.Connected == true,
            BackendInfo = _mpvVersion
        });
    }

    /// <inheritdoc />
    public Task PlayAsync(PartyQueueItem item, double startPositionSeconds = 0, CancellationToken cancellationToken = default)
    {
        // MPV backend cannot resolve file paths from SongApiKey. The orchestrating service
        // must call PlayFileAsync with the resolved file path instead.
        _logger.Warning("[MpvPlaybackBackend] PlayAsync(PartyQueueItem) is not supported for MPV. " +
                       "Use PlayFileAsync with the resolved file path. SongApiKey: {SongApiKey}", item.SongApiKey);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task PlayFileAsync(string filePath, Guid songApiKey, double startPositionSeconds = 0, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            _logger.Warning("[MpvPlaybackBackend] Cannot play empty file path");
            return;
        }

        _logger.Information("[MpvPlaybackBackend] Playing file: {FilePath} (SongApiKey: {SongApiKey})", filePath, songApiKey);

        await SendCommandAsync("loadfile", [filePath, "replace"], cancellationToken).ConfigureAwait(false);

        if (startPositionSeconds > 0)
        {
            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            await SeekAsync(startPositionSeconds, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task PauseAsync(CancellationToken cancellationToken = default)
    {
        await SetPropertyAsync("pause", true, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ResumeAsync(CancellationToken cancellationToken = default)
    {
        await SetPropertyAsync("pause", false, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await SendCommandAsync("stop", [], cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SeekAsync(double positionSeconds, CancellationToken cancellationToken = default)
    {
        await SendCommandAsync("seek", [positionSeconds, "absolute"], cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SetVolumeAsync(double volume01, CancellationToken cancellationToken = default)
    {
        var volumePercent = Math.Clamp(volume01 * 100, 0, 100);
        await SetPropertyAsync("volume", volumePercent, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SkipNextAsync(CancellationToken cancellationToken = default)
    {
        await SendCommandAsync("playlist-next", [], cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SkipPreviousAsync(CancellationToken cancellationToken = default)
    {
        await SendCommandAsync("playlist-prev", [], cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BackendStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var isConnected = _mpvProcess != null && !_mpvProcess.HasExited && _ipcSocket?.Connected == true;

        if (!isConnected)
        {
            return new BackendStatus
            {
                IsPlaying = false,
                PositionSeconds = 0,
                Volume = _initialVolume,
                CurrentItemApiKey = null,
                IsConnected = false,
                StatusMessage = "MPV process not running or IPC not connected",
                ErrorMessage = null
            };
        }

        try
        {
            var pausedResult = await GetPropertyAsync<bool?>("pause", cancellationToken).ConfigureAwait(false);
            var positionResult = await GetPropertyAsync<double?>("time-pos", cancellationToken).ConfigureAwait(false);
            var volumeResult = await GetPropertyAsync<double?>("volume", cancellationToken).ConfigureAwait(false);
            var idleResult = await GetPropertyAsync<bool?>("idle-active", cancellationToken).ConfigureAwait(false);

            var isPaused = pausedResult ?? true;
            var isIdle = idleResult ?? true;
            var isPlaying = !isPaused && !isIdle;
            var positionSeconds = positionResult ?? 0;
            var volume = (volumeResult ?? _initialVolume * 100) / 100.0;

            return new BackendStatus
            {
                IsPlaying = isPlaying,
                PositionSeconds = positionSeconds,
                Volume = volume,
                CurrentItemApiKey = null,
                IsConnected = true,
                StatusMessage = isPlaying ? "Playing" : (isIdle ? "Idle" : "Paused"),
                ErrorMessage = null
            };
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "[MpvPlaybackBackend] Error getting MPV status");
            return new BackendStatus
            {
                IsPlaying = false,
                PositionSeconds = 0,
                Volume = _initialVolume,
                CurrentItemApiKey = null,
                IsConnected = isConnected,
                StatusMessage = "Error getting status",
                ErrorMessage = ex.Message
            };
        }
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            return;
        }

        lock (_lock)
        {
            if (_isInitialized)
            {
                return;
            }

            try
            {
                _actualSocketPath = _socketPath ?? Path.Combine(Path.GetTempPath(), $"melodee-mpv-{Guid.NewGuid():N}.sock");

                var mpvPath = _mpvPath ?? FindMpvExecutable();
                if (mpvPath == null)
                {
                    _logger.Error("[MpvPlaybackBackend] MPV executable not found");
                    return;
                }

                var audioDeviceArg = string.IsNullOrEmpty(_audioDevice)
                    ? ""
                    : $"--audio-device={_audioDevice}";

                var extraArgs = _extraArgs ?? "";

                var arguments = $"--idle=yes --input-ipc-server={_actualSocketPath} --volume={_initialVolume * 100:F0} {audioDeviceArg} {extraArgs}";

                if (_enableDebugOutput)
                {
                    arguments += " --msg-level=all=v";
                }

                _logger.Information("[MpvPlaybackBackend] Starting MPV with command: {MpvPath} {Arguments}",
                    mpvPath, arguments);

                var startInfo = new ProcessStartInfo
                {
                    FileName = mpvPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _mpvProcess = new Process { StartInfo = startInfo };

                _mpvProcess.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data) && _enableDebugOutput)
                    {
                        _logger.Debug("[MpvPlaybackBackend] MPV output: {Output}", args.Data);
                    }
                };

                _mpvProcess.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data) && _enableDebugOutput)
                    {
                        _logger.Debug("[MpvPlaybackBackend] MPV error: {Error}", args.Data);
                    }
                };

                _mpvProcess.Start();

                _mpvProcess.BeginOutputReadLine();
                _mpvProcess.BeginErrorReadLine();

                _logger.Information("[MpvPlaybackBackend] MPV process started with PID {Pid}", _mpvProcess.Id);

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[MpvPlaybackBackend] Failed to initialize MPV backend");
                return;
            }
        }

        await ConnectToIpcSocketAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task ConnectToIpcSocketAsync(CancellationToken cancellationToken)
    {
        const int maxRetries = 10;
        const int retryDelayMs = 200;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    _ipcSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    await _ipcSocket.ConnectAsync("127.0.0.1", 0, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    _ipcSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                    var endpoint = new UnixDomainSocketEndPoint(_actualSocketPath!);
                    await _ipcSocket.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);
                }

                _ipcStream = new NetworkStream(_ipcSocket, ownsSocket: false);
                _ipcReader = new StreamReader(_ipcStream, Encoding.UTF8);
                _ipcWriter = new StreamWriter(_ipcStream, Encoding.UTF8) { AutoFlush = true };

                _logger.Information("[MpvPlaybackBackend] Connected to MPV IPC socket at {SocketPath}", _actualSocketPath);

                var versionResult = await GetPropertyAsync<string>("mpv-version", cancellationToken).ConfigureAwait(false);
                _mpvVersion = versionResult;

                return;
            }
            catch (Exception ex)
            {
                if (i == maxRetries - 1)
                {
                    _logger.Error(ex, "[MpvPlaybackBackend] Failed to connect to MPV IPC socket after {MaxRetries} retries", maxRetries);
                    throw;
                }

                _logger.Debug("[MpvPlaybackBackend] Waiting for MPV IPC socket (attempt {Attempt}/{MaxRetries})", i + 1, maxRetries);
                await Task.Delay(retryDelayMs, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task<T?> GetPropertyAsync<T>(string propertyName, CancellationToken cancellationToken)
    {
        var response = await SendCommandInternalAsync("get_property", [propertyName], cancellationToken).ConfigureAwait(false);

        if (response == null)
        {
            return default;
        }

        var responseElement = response.Value;
        if (responseElement.TryGetProperty("error", out var errorProp) && errorProp.GetString() != "success")
        {
            return default;
        }

        if (responseElement.TryGetProperty("data", out var dataProp))
        {
            return ConvertJsonElement<T>(dataProp);
        }

        return default;
    }

    private async Task SetPropertyAsync<T>(string propertyName, T value, CancellationToken cancellationToken)
    {
        await SendCommandInternalAsync("set_property", [propertyName, value!], cancellationToken).ConfigureAwait(false);
    }

    private async Task SendCommandAsync(string command, object[] args, CancellationToken cancellationToken)
    {
        await SendCommandInternalAsync(command, args, cancellationToken).ConfigureAwait(false);
    }

    private async Task<JsonElement?> SendCommandInternalAsync(string command, object[] args, CancellationToken cancellationToken)
    {
        if (_ipcWriter == null || _ipcReader == null)
        {
            _logger.Warning("[MpvPlaybackBackend] IPC not connected, cannot send command: {Command}", command);
            return null;
        }

        var requestId = Interlocked.Increment(ref _requestId);

        var commandArray = new object[args.Length + 1];
        commandArray[0] = command;
        Array.Copy(args, 0, commandArray, 1, args.Length);

        var request = new
        {
            command = commandArray,
            request_id = requestId
        };

        var json = JsonSerializer.Serialize(request);

        if (_enableDebugOutput)
        {
            _logger.Debug("[MpvPlaybackBackend] Sending IPC command: {Command}", json);
        }

        try
        {
            await _ipcWriter.WriteLineAsync(json.AsMemory(), cancellationToken).ConfigureAwait(false);

            var responseLine = await _ipcReader.ReadLineAsync(cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrEmpty(responseLine))
            {
                return null;
            }

            if (_enableDebugOutput)
            {
                _logger.Debug("[MpvPlaybackBackend] Received IPC response: {Response}", responseLine);
            }

            var responseDoc = JsonDocument.Parse(responseLine);
            return responseDoc.RootElement;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "[MpvPlaybackBackend] Error sending IPC command: {Command}", command);
            return null;
        }
    }

    private static T? ConvertJsonElement<T>(JsonElement element)
    {
        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        if (targetType == typeof(bool))
        {
            if (element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False)
            {
                return (T)(object)element.GetBoolean();
            }
            return default;
        }
        if (targetType == typeof(int))
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return (T)(object)element.GetInt32();
            }
            return default;
        }
        if (targetType == typeof(double))
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return (T)(object)element.GetDouble();
            }
            return default;
        }
        if (targetType == typeof(string))
        {
            return (T)(object?)element.GetString()!;
        }

        return default;
    }

    /// <inheritdoc />
    public Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            try
            {
                _ipcWriter?.Dispose();
                _ipcReader?.Dispose();
                _ipcStream?.Dispose();
                _ipcSocket?.Dispose();

                _ipcWriter = null;
                _ipcReader = null;
                _ipcStream = null;
                _ipcSocket = null;

                if (_mpvProcess != null && !_mpvProcess.HasExited)
                {
                    _logger.Information("[MpvPlaybackBackend] Shutting down MPV process");

                    try
                    {
                        _mpvProcess.Kill();
                        _mpvProcess.WaitForExit(5000);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "[MpvPlaybackBackend] Error during MPV shutdown");
                    }

                    _mpvProcess.Dispose();
                    _mpvProcess = null;
                }

                if (!string.IsNullOrEmpty(_actualSocketPath) && File.Exists(_actualSocketPath))
                {
                    try
                    {
                        File.Delete(_actualSocketPath);
                    }
                    catch
                    {
                        // Ignore socket cleanup errors
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[MpvPlaybackBackend] Error during shutdown");
            }

            _isInitialized = false;
            return Task.CompletedTask;
        }
    }

    private static string? FindMpvExecutable()
    {
        var isWindows = OperatingSystem.IsWindows();
        var name = isWindows ? "mpv" : "mpv";

        var pathVar = Environment.GetEnvironmentVariable("PATH") ?? "";
        var pathSeparator = isWindows ? ';' : ':';
        var extensions = isWindows ? new[] { ".exe", ".com", "" } : new[] { "" };

        foreach (var dir in pathVar.Split(pathSeparator))
        {
            if (string.IsNullOrEmpty(dir))
            {
                continue;
            }

            foreach (var ext in extensions)
            {
                var fullPath = Path.Combine(dir, name + ext);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
        }

        return null;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            _ = ShutdownAsync().ConfigureAwait(false);
        }
        catch
        {
            // Ignore dispose errors
        }

        _disposed = true;
    }
}

using System.Diagnostics;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models;
using Serilog;

namespace Melodee.Common.Services.Playback.Backends;

/// <summary>
/// MPV playback backend implementation using IPC socket communication.
/// </summary>
public sealed class MpvPlaybackBackend : IPlaybackBackend, IDisposable
{
    private readonly ILogger _logger;
    private readonly MpvOptions _options;
    private Process? _mpvProcess;
    private bool _disposed;
    private string? _socketPath;
    private readonly object _lock = new();
    private bool _isInitialized;
#pragma warning disable CS0649
    private string? _mpvVersion;
#pragma warning restore CS0649

    public MpvPlaybackBackend(ILogger logger, MpvOptions options)
    {
        _logger = logger;
        _options = options;
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
            IsAvailable = _mpvProcess != null && !_mpvProcess.HasExited,
            BackendInfo = _mpvVersion
        });
    }

    /// <inheritdoc />
    public Task PlayAsync(PartyQueueItem item, double startPositionSeconds = 0, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("MpvPlaybackBackend requires full implementation with IPC communication.");
    }

    /// <inheritdoc />
    public Task PauseAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("MpvPlaybackBackend requires full implementation with IPC communication.");
    }

    /// <inheritdoc />
    public Task ResumeAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("MpvPlaybackBackend requires full implementation with IPC communication.");
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("MpvPlaybackBackend requires full implementation with IPC communication.");
    }

    /// <inheritdoc />
    public Task SeekAsync(double positionSeconds, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("MpvPlaybackBackend requires full implementation with IPC communication.");
    }

    /// <inheritdoc />
    public Task SetVolumeAsync(double volume01, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("MpvPlaybackBackend requires full implementation with IPC communication.");
    }

    /// <inheritdoc />
    public Task SkipNextAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("MpvPlaybackBackend requires full implementation with IPC communication.");
    }

    /// <inheritdoc />
    public Task SkipPreviousAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("MpvPlaybackBackend requires full implementation with IPC communication.");
    }

    /// <inheritdoc />
    public Task<BackendStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var isConnected = _mpvProcess != null && !_mpvProcess.HasExited;
        return Task.FromResult(new BackendStatus
        {
            IsPlaying = false,
            PositionSeconds = 0,
            Volume = _options.InitialVolume,
            CurrentItemApiKey = null,
            IsConnected = isConnected,
            StatusMessage = isConnected ? "MPV process running" : "MPV process not running",
            ErrorMessage = null
        });
    }

    /// <inheritdoc />
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            return Task.CompletedTask;
        }

        lock (_lock)
        {
            if (_isInitialized)
            {
                return Task.CompletedTask;
            }

            try
            {
                _socketPath = _options.SocketPath ?? Path.Combine(Path.GetTempPath(), $"melodee-mpv-{Guid.NewGuid():N}.sock");

                var mpvPath = _options.MpvPath ?? FindMpvExecutable();
                if (mpvPath == null)
                {
                    _logger.Error("[MpvPlaybackBackend] MPV executable not found");
                    return Task.CompletedTask;
                }

                var audioDeviceArg = string.IsNullOrEmpty(_options.AudioDevice)
                    ? ""
                    : $"--audio-device={_options.AudioDevice}";

                var extraArgs = _options.ExtraArgs ?? "";

                var arguments = $"--idle=yes --input-ipc-server={_socketPath} {audioDeviceArg} {extraArgs}";

                if (_options.EnableDebugOutput)
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
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        _logger.Debug("[MpvPlaybackBackend] MPV output: {Output}", args.Data);
                    }
                };

                _mpvProcess.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
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
            }

            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            try
            {
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

                if (!string.IsNullOrEmpty(_socketPath) && File.Exists(_socketPath))
                {
                    try
                    {
                        File.Delete(_socketPath);
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
        var name = isWindows ? "mpv.com" : "mpv";

        var pathVar = Environment.GetEnvironmentVariable("PATH") ?? "";
        var pathSeparator = isWindows ? ';' : ':';
        var extensions = isWindows ? new[] { ".exe", ".com", ".bat", "" } : new[] { "" };

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

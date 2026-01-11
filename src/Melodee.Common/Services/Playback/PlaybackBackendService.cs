using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums.PartyMode;
using Melodee.Common.Models;
using Melodee.Common.Models.PartyMode;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.Playback.Factory;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services.Playback;

/// <summary>
/// Interface for managing playback backend lifecycle and health.
/// </summary>
public interface IPlaybackBackendService
{
    /// <summary>
    /// Gets the current backend status.
    /// </summary>
    Task<OperationResult<BackendStatus>> GetBackendStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the backend capabilities.
    /// </summary>
    Task<OperationResult<BackendCapabilities>> GetBackendCapabilitiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the backend if not already initialized.
    /// </summary>
    Task<OperationResult<bool>> InitializeBackendAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Shuts down the backend.
    /// </summary>
    Task<OperationResult<bool>> ShutdownBackendAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers the MPV backend as a system endpoint for party sessions.
    /// </summary>
    Task<OperationResult<PartySessionEndpoint?>> RegisterBackendEndpointAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the backend endpoint heartbeat.
    /// </summary>
    Task<OperationResult<bool>> UpdateBackendHeartbeatAsync(Guid sessionApiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Plays a song by its API key. Resolves the file path from the database and plays it.
    /// </summary>
    /// <param name="songApiKey">The API key of the song to play.</param>
    /// <param name="startPositionSeconds">Optional start position in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result indicating success or failure.</returns>
    Task<OperationResult<bool>> PlaySongAsync(Guid songApiKey, double startPositionSeconds = 0, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for managing playback backend lifecycle and health reporting.
/// </summary>
public sealed class PlaybackBackendService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory,
    PlaybackBackendFactory backendFactory)
    : ServiceBase(logger, cacheManager, contextFactory), IPlaybackBackendService
{
    private const string BackendStatusCacheKey = "urn:playback:backend:status";
    private PartySessionEndpoint? _backendEndpoint;

    public async Task<OperationResult<BackendStatus>> GetBackendStatusAsync(CancellationToken cancellationToken = default)
    {
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var jukeboxEnabled = configuration.GetValue<bool>(SettingRegistry.JukeboxEnabled);
        var backendType = configuration.GetValue<string>(SettingRegistry.JukeboxBackendType);

        if (!jukeboxEnabled || string.IsNullOrEmpty(backendType))
        {
            return new OperationResult<BackendStatus>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = new BackendStatus { IsConnected = false }
            };
        }

        var backend = await GetBackendAsync(backendType, cancellationToken).ConfigureAwait(false);
        if (backend == null)
        {
            return new OperationResult<BackendStatus>($"Failed to create {backendType} backend")
            {
                Type = OperationResponseType.Error,
                Data = new BackendStatus { IsConnected = false }
            };
        }

        var status = await backend.GetStatusAsync(cancellationToken).ConfigureAwait(false);

        return new OperationResult<BackendStatus> { Data = status };
    }

    public async Task<OperationResult<BackendCapabilities>> GetBackendCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var jukeboxEnabled = configuration.GetValue<bool>(SettingRegistry.JukeboxEnabled);
        var backendType = configuration.GetValue<string>(SettingRegistry.JukeboxBackendType);

        if (!jukeboxEnabled || string.IsNullOrEmpty(backendType))
        {
            return new OperationResult<BackendCapabilities>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = new BackendCapabilities { IsAvailable = false }
            };
        }

        var backend = await GetBackendAsync(backendType, cancellationToken).ConfigureAwait(false);
        if (backend == null)
        {
            return new OperationResult<BackendCapabilities>($"Failed to create {backendType} backend")
            {
                Type = OperationResponseType.Error,
                Data = new BackendCapabilities { IsAvailable = false }
            };
        }

        var capabilities = await backend.GetCapabilitiesAsync(cancellationToken).ConfigureAwait(false);

        return new OperationResult<BackendCapabilities> { Data = capabilities };
    }

    public async Task<OperationResult<bool>> InitializeBackendAsync(CancellationToken cancellationToken = default)
    {
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        if (!configuration.GetValue<bool>(SettingRegistry.JukeboxEnabled))
        {
            return new OperationResult<bool>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = false
            };
        }

        var backendType = configuration.GetValue<string>(SettingRegistry.JukeboxBackendType);
        var backend = await GetBackendAsync(backendType, cancellationToken).ConfigureAwait(false);
        if (backend == null)
        {
            return new OperationResult<bool>($"Failed to create {backendType} backend")
            {
                Type = OperationResponseType.Error,
                Data = false
            };
        }

        await backend.InitializeAsync(cancellationToken).ConfigureAwait(false);
        var capabilities = await backend.GetCapabilitiesAsync(cancellationToken).ConfigureAwait(false);

        Logger.Information("[PlaybackBackendService] {BackendType} backend initialized. Available: {IsAvailable}, Info: {BackendInfo}",
            backendType, capabilities.IsAvailable, capabilities.BackendInfo);

        return new OperationResult<bool> { Data = capabilities.IsAvailable };
    }

    public Task<OperationResult<bool>> ShutdownBackendAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            backendFactory.DisposeBackend();
            Logger.Information("[PlaybackBackendService] Playback backend shutdown complete");
            return Task.FromResult(new OperationResult<bool> { Data = true });
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[PlaybackBackendService] Error during backend shutdown");
            return Task.FromResult(new OperationResult<bool>("Error shutting down backend")
            {
                Type = OperationResponseType.Error,
                Data = false
            });
        }
    }

    public async Task<OperationResult<PartySessionEndpoint?>> RegisterBackendEndpointAsync(CancellationToken cancellationToken = default)
    {
        if (_backendEndpoint != null)
        {
            return new OperationResult<PartySessionEndpoint?> { Data = _backendEndpoint };
        }

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var existingEndpoint = await scopedContext.PartySessionEndpoints
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Type == PartySessionEndpointType.MpvBackend, cancellationToken)
            .ConfigureAwait(false);

        if (existingEndpoint != null)
        {
            _backendEndpoint = existingEndpoint;
            return new OperationResult<PartySessionEndpoint?> { Data = existingEndpoint };
        }

        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var backendType = configuration.GetValue<string>(SettingRegistry.JukeboxBackendType);

        var backend = await GetBackendAsync(backendType, cancellationToken).ConfigureAwait(false);
        if (backend == null)
        {
            return new OperationResult<PartySessionEndpoint?>($"Failed to create {backendType} backend")
            {
                Type = OperationResponseType.Error,
                Data = null
            };
        }

        var capabilities = await backend.GetCapabilitiesAsync(cancellationToken).ConfigureAwait(false);
        var capabilitiesJson = System.Text.Json.JsonSerializer.Serialize(new PartySessionEndpointCapabilities
        {
            CanPlay = capabilities.CanPlay,
            CanPause = capabilities.CanPause,
            CanSkip = capabilities.CanSkip,
            CanSeek = capabilities.CanSeek,
            CanSetVolume = capabilities.CanSetVolume,
            CanReportPosition = capabilities.CanReportPosition
        });

        var endpoint = new PartySessionEndpoint
        {
            Name = $"{backendType?.ToUpperInvariant() ?? "Unknown"} Backend",
            Type = PartySessionEndpointType.MpvBackend,
            OwnerUserId = null,
            CapabilitiesJson = capabilitiesJson,
            IsShared = true,
            LastSeenAt = SystemClock.Instance.GetCurrentInstant(),
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        scopedContext.PartySessionEndpoints.Add(endpoint);
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _backendEndpoint = endpoint;
        Logger.Information("[PlaybackBackendService] Registered {BackendType} backend endpoint {EndpointId}", backendType, endpoint.Id);

        return new OperationResult<PartySessionEndpoint?> { Data = endpoint };
    }

    public async Task<OperationResult<bool>> UpdateBackendHeartbeatAsync(Guid sessionApiKey, CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var endpoint = await scopedContext.PartySessionEndpoints
            .FirstOrDefaultAsync(x => x.Type == PartySessionEndpointType.MpvBackend, cancellationToken)
            .ConfigureAwait(false);

        if (endpoint == null)
        {
            return new OperationResult<bool>("MPV backend endpoint not found")
            {
                Type = OperationResponseType.NotFound,
                Data = false
            };
        }

        var session = await scopedContext.PartySessions
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<bool>("Session not found")
            {
                Type = OperationResponseType.NotFound,
                Data = false
            };
        }

        if (session.ActiveEndpointId != endpoint.ApiKey)
        {
            return new OperationResult<bool>("MPV backend is not the active endpoint for this session")
            {
                Type = OperationResponseType.ValidationFailure,
                Data = false
            };
        }

        endpoint.LastSeenAt = SystemClock.Instance.GetCurrentInstant();
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new OperationResult<bool> { Data = true };
    }

    public async Task<OperationResult<bool>> PlaySongAsync(Guid songApiKey, double startPositionSeconds = 0, CancellationToken cancellationToken = default)
    {
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        if (!configuration.GetValue<bool>(SettingRegistry.JukeboxEnabled))
        {
            return new OperationResult<bool>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = false
            };
        }

        var backendType = configuration.GetValue<string>(SettingRegistry.JukeboxBackendType);
        var backend = await GetBackendAsync(backendType, cancellationToken).ConfigureAwait(false);
        if (backend == null)
        {
            return new OperationResult<bool>($"Failed to create {backendType} backend")
            {
                Type = OperationResponseType.Error,
                Data = false
            };
        }

        // Resolve the song from the database to get the file path
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var song = await scopedContext.Songs
            .AsNoTracking()
            .Include(s => s.Album)
                .ThenInclude(a => a.Artist)
                    .ThenInclude(ar => ar.Library)
            .FirstOrDefaultAsync(s => s.ApiKey == songApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (song == null)
        {
            return new OperationResult<bool>($"Song not found: {songApiKey}")
            {
                Type = OperationResponseType.NotFound,
                Data = false
            };
        }

        // Build the full file path
        var filePath = Path.Combine(
            song.Album.Artist.Library.Path,
            song.Album.Artist.Directory,
            song.Album.Directory,
            song.FileName);

        if (!File.Exists(filePath))
        {
            Logger.Warning("[PlaybackBackendService] Song file not found: {FilePath}", filePath);
            return new OperationResult<bool>($"Song file not found: {filePath}")
            {
                Type = OperationResponseType.NotFound,
                Data = false
            };
        }

        Logger.Information("[PlaybackBackendService] Playing song {SongApiKey}: {FilePath}", songApiKey, filePath);

        await backend.PlayFileAsync(filePath, songApiKey, startPositionSeconds, cancellationToken).ConfigureAwait(false);

        return new OperationResult<bool> { Data = true };
    }

    private async Task<IPlaybackBackend?> GetBackendAsync(string? backendType, CancellationToken cancellationToken)
    {
        return backendType?.ToLowerInvariant() switch
        {
            "mpv" => await backendFactory.GetOrCreateMpvBackendAsync(cancellationToken).ConfigureAwait(false),
            "mpd" => await backendFactory.GetOrCreateMpdBackendAsync(cancellationToken).ConfigureAwait(false),
            _ => null
        };
    }
}

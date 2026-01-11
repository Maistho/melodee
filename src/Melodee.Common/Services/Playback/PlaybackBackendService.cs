using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums.PartyMode;
using Melodee.Common.Models;
using Melodee.Common.Models.PartyMode;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.Playback.Factory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
}

/// <summary>
/// Service for managing playback backend lifecycle and health reporting.
/// </summary>
public sealed class PlaybackBackendService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IOptions<JukeboxOptions> jukeboxOptions,
    PlaybackBackendFactory backendFactory)
    : ServiceBase(logger, cacheManager, contextFactory), IPlaybackBackendService
{
    private const string BackendStatusCacheKey = "urn:playback:backend:status";
    private PartySessionEndpoint? _backendEndpoint;

    public async Task<OperationResult<BackendStatus>> GetBackendStatusAsync(CancellationToken cancellationToken = default)
    {
        if (!jukeboxOptions.Value.Enabled || string.IsNullOrEmpty(jukeboxOptions.Value.BackendType))
        {
            return new OperationResult<BackendStatus>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = new BackendStatus { IsConnected = false }
            };
        }

        var backend = backendFactory.GetOrCreateMpvBackend();
        if (backend == null)
        {
            return new OperationResult<BackendStatus>("Failed to create MPV backend")
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
        if (!jukeboxOptions.Value.Enabled || string.IsNullOrEmpty(jukeboxOptions.Value.BackendType))
        {
            return new OperationResult<BackendCapabilities>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = new BackendCapabilities { IsAvailable = false }
            };
        }

        var backend = backendFactory.GetOrCreateMpvBackend();
        if (backend == null)
        {
            return new OperationResult<BackendCapabilities>("Failed to create MPV backend")
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
        if (!jukeboxOptions.Value.Enabled)
        {
            return new OperationResult<bool>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = false
            };
        }

        var backend = backendFactory.GetOrCreateMpvBackend();
        if (backend == null)
        {
            return new OperationResult<bool>("Failed to create MPV backend")
            {
                Type = OperationResponseType.Error,
                Data = false
            };
        }

        await backend.InitializeAsync(cancellationToken).ConfigureAwait(false);
        var capabilities = await backend.GetCapabilitiesAsync(cancellationToken).ConfigureAwait(false);

        Logger.Information("[PlaybackBackendService] MPV backend initialized. Available: {IsAvailable}, Info: {BackendInfo}",
            capabilities.IsAvailable, capabilities.BackendInfo);

        return new OperationResult<bool> { Data = capabilities.IsAvailable };
    }

    public async Task<OperationResult<bool>> ShutdownBackendAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            backendFactory.DisposeBackend();
            Logger.Information("[PlaybackBackendService] MPV backend shutdown complete");
            return new OperationResult<bool> { Data = true };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[PlaybackBackendService] Error during backend shutdown");
            return new OperationResult<bool>("Error shutting down backend")
            {
                Type = OperationResponseType.Error,
                Data = false
            };
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

        var backend = backendFactory.GetOrCreateMpvBackend();
        if (backend == null)
        {
            return new OperationResult<PartySessionEndpoint?>("Failed to create MPV backend")
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
            Name = "MPV Backend",
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
        Logger.Information("[PlaybackBackendService] Registered MPV backend endpoint {EndpointId}", endpoint.Id);

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
}

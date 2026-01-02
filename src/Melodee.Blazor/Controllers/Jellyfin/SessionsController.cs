using System.Text.Json.Serialization;
using Melodee.Blazor.Controllers.Jellyfin.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Blazor.Controllers.Jellyfin;

/// <summary>
/// Jellyfin-compatible session and playback reporting endpoints.
/// Integrates with Melodee's existing ScrobbleService for now playing and scrobble functionality.
/// </summary>
[ApiController]
[Route("api/jf/Sessions")]
[ApiExplorerSettings(GroupName = "jellyfin")]
[EnableRateLimiting("jellyfin-api")]
public class SessionsController(
    EtagRepository etagRepository,
    ISerializer serializer,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> dbContextFactory,
    IClock clock,
    ILoggerFactory loggerFactory,
    ScrobbleService scrobbleService,
    ILogger<SessionsController> logger) : JellyfinControllerBase(etagRepository, serializer, configuration, configurationFactory, dbContextFactory, clock, loggerFactory)
{
    /// <summary>
    /// Reports playback has started within a session.
    /// </summary>
    [HttpPost("Playing")]
    public async Task<IActionResult> ReportPlaybackStartAsync(
        [FromBody] JellyfinPlaybackStartInfo? request,
        CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            return JellyfinUnauthorized();
        }

        if (request == null || string.IsNullOrWhiteSpace(request.ItemId))
        {
            return NoContent();
        }

        if (!TryParseJellyfinGuid(request.ItemId, out var apiKey))
        {
            logger.LogWarning("JellyfinPlaybackStart InvalidItemId={ItemId} UserId={UserId}", request.ItemId, user.Id);
            return NoContent();
        }

        var positionSeconds = request.PositionTicks.HasValue
            ? request.PositionTicks.Value / 10_000_000.0
            : 0;

        var userInfo = user.ToUserInfo();
        var clientName = request.PlaySessionId ?? "Jellyfin";
        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        try
        {
            var config = await GetConfigurationAsync(cancellationToken);
            await scrobbleService.InitializeAsync(config, cancellationToken);
            await scrobbleService.NowPlaying(userInfo, apiKey, positionSeconds, clientName, userAgent, ipAddress, cancellationToken);
            logger.LogInformation("JellyfinPlaybackStart UserId={UserId} ItemId={ItemId} Position={Position}",
                user.Id, request.ItemId, positionSeconds);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "JellyfinPlaybackStart failed UserId={UserId} ItemId={ItemId}", user.Id, request.ItemId);
        }

        return NoContent();
    }

    /// <summary>
    /// Reports playback progress within a session.
    /// </summary>
    [HttpPost("Playing/Progress")]
    public async Task<IActionResult> ReportPlaybackProgressAsync(
        [FromBody] JellyfinPlaybackProgressInfo? request,
        CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            return JellyfinUnauthorized();
        }

        if (request == null || string.IsNullOrWhiteSpace(request.ItemId))
        {
            return NoContent();
        }

        if (!TryParseJellyfinGuid(request.ItemId, out var apiKey))
        {
            return NoContent();
        }

        double? positionSeconds = request.PositionTicks.HasValue
            ? request.PositionTicks.Value / 10_000_000.0
            : null;

        var userInfo = user.ToUserInfo();
        var clientName = request.PlaySessionId ?? "Jellyfin";
        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        try
        {
            var config = await GetConfigurationAsync(cancellationToken);
            await scrobbleService.InitializeAsync(config, cancellationToken);
            await scrobbleService.NowPlaying(userInfo, apiKey, positionSeconds, clientName, userAgent, ipAddress, cancellationToken);
            logger.LogDebug("JellyfinPlaybackProgress UserId={UserId} ItemId={ItemId} Position={Position} IsPaused={IsPaused}",
                user.Id, request.ItemId, positionSeconds, request.IsPaused);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "JellyfinPlaybackProgress failed UserId={UserId} ItemId={ItemId}", user.Id, request.ItemId);
        }

        return NoContent();
    }

    /// <summary>
    /// Reports playback has stopped within a session.
    /// </summary>
    [HttpPost("Playing/Stopped")]
    public async Task<IActionResult> ReportPlaybackStoppedAsync(
        [FromBody] JellyfinPlaybackStopInfo? request,
        CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            return JellyfinUnauthorized();
        }

        if (request == null || string.IsNullOrWhiteSpace(request.ItemId))
        {
            return NoContent();
        }

        if (!TryParseJellyfinGuid(request.ItemId, out var apiKey))
        {
            logger.LogWarning("JellyfinPlaybackStopped InvalidItemId={ItemId} UserId={UserId}", request.ItemId, user.Id);
            return NoContent();
        }

        var userInfo = user.ToUserInfo();
        var clientName = request.PlaySessionId ?? "Jellyfin";
        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        try
        {
            // Only scrobble if playback was not failed
            if (request.Failed != true)
            {
                var config = await GetConfigurationAsync(cancellationToken);
                await scrobbleService.InitializeAsync(config, cancellationToken);
                await scrobbleService.Scrobble(userInfo, apiKey, false, clientName, userAgent, ipAddress, cancellationToken);
                logger.LogInformation("JellyfinPlaybackStopped UserId={UserId} ItemId={ItemId} PositionTicks={PositionTicks}",
                    user.Id, request.ItemId, request.PositionTicks);
            }
            else
            {
                logger.LogDebug("JellyfinPlaybackStopped skipped scrobble (failed) UserId={UserId} ItemId={ItemId}",
                    user.Id, request.ItemId);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "JellyfinPlaybackStopped failed UserId={UserId} ItemId={ItemId}", user.Id, request.ItemId);
        }

        return NoContent();
    }

    /// <summary>
    /// Pings a playback session.
    /// </summary>
    [HttpPost("Playing/Ping")]
    public async Task<IActionResult> PingPlaybackAsync(
        [FromQuery] string? playSessionId,
        CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            return JellyfinUnauthorized();
        }

        logger.LogDebug("JellyfinPlaybackPing UserId={UserId} PlaySessionId={PlaySessionId}", user.Id, playSessionId);
        return NoContent();
    }

    /// <summary>
    /// Gets a list of sessions.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSessionsAsync(
        [FromQuery] string? controllableByUserId,
        [FromQuery] string? deviceId,
        [FromQuery] int? activeWithinSeconds,
        CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            return JellyfinUnauthorized();
        }

        // Get now playing information from the scrobble service
        var nowPlayingResult = await scrobbleService.GetNowPlaying(cancellationToken);

        var sessions = new List<JellyfinSessionInfo>();

        if (nowPlayingResult.IsSuccess && nowPlayingResult.Data != null)
        {
            foreach (var np in nowPlayingResult.Data)
            {
                sessions.Add(new JellyfinSessionInfo
                {
                    Id = np.UniqueId.ToString(),
                    UserId = np.User.Id.ToString(),
                    UserName = np.User.UserName,
                    Client = np.Scrobble.PlayerName,
                    DeviceId = np.Scrobble.IpAddress ?? string.Empty,
                    DeviceName = np.Scrobble.UserAgent ?? "Unknown Device",
                    ApplicationVersion = "1.0.0",
                    IsActive = true,
                    SupportsMediaControl = false,
                    SupportsRemoteControl = false,
                    NowPlayingItem = new JellyfinNowPlayingItem
                    {
                        Id = ToJellyfinId(np.Scrobble.SongApiKey),
                        Name = np.Scrobble.SongTitle,
                        Type = "Audio",
                        MediaType = "Audio",
                        RunTimeTicks = np.Scrobble.SongDuration.HasValue ? np.Scrobble.SongDuration.Value * 10_000_000L : null
                    },
                    PlayState = new JellyfinPlayState
                    {
                        PositionTicks = np.Scrobble.SecondsPlayed.HasValue ? np.Scrobble.SecondsPlayed.Value * 10_000_000L : 0,
                        IsPaused = false,
                        CanSeek = true,
                        PlayMethod = "DirectPlay"
                    },
                    LastActivityDate = FormatInstantForJellyfin(np.Scrobble.LastScrobbledAt)
                });
            }
        }

        return Ok(sessions);
    }

    /// <summary>
    /// Reports that a session has ended / logout. Revokes the current access token.
    /// </summary>
    [HttpPost("Logout")]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            // Already logged out or invalid token - return success anyway
            return NoContent();
        }

        // Revoke the current token
        var tokenInfo = JellyfinTokenParser.ParseFromRequest(Request);
        if (!string.IsNullOrEmpty(tokenInfo.Token))
        {
            await using var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);
            var tokenPrefix = JellyfinTokenParser.GetTokenPrefix(tokenInfo.Token);

            var accessToken = await dbContext.JellyfinAccessTokens
                .Where(t => t.UserId == user.Id && t.TokenPrefixHash == tokenPrefix && t.RevokedAt == null)
                .FirstOrDefaultAsync(cancellationToken);

            if (accessToken != null)
            {
                accessToken.RevokedAt = Clock.GetCurrentInstant();
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return NoContent();
    }

    /// <summary>
    /// Reports full client capabilities for a session. Used by Streamyfin/Finamp to register device capabilities.
    /// </summary>
    [HttpPost("Capabilities/Full")]
    public async Task<IActionResult> PostCapabilitiesFullAsync(
        [FromBody] JellyfinClientCapabilitiesRequest? request,
        CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            return JellyfinUnauthorized();
        }

        // For now, just acknowledge the capabilities - we don't need to store them
        // since Melodee doesn't support remote control features yet
        logger.LogDebug("JellyfinCapabilities UserId={UserId} PlayableMediaTypes={MediaTypes} SupportsMediaControl={SupportsMediaControl}",
            user.Id,
            request?.PlayableMediaTypes != null ? string.Join(",", request.PlayableMediaTypes) : "none",
            request?.SupportsMediaControl);

        return NoContent();
    }

    /// <summary>
    /// Reports client capabilities for a session (simplified version).
    /// </summary>
    [HttpPost("Capabilities")]
    public async Task<IActionResult> PostCapabilitiesAsync(
        [FromQuery] string? playableMediaTypes,
        [FromQuery] string? supportedCommands,
        [FromQuery] bool? supportsMediaControl,
        [FromQuery] bool? supportsPersistentIdentifier,
        CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            return JellyfinUnauthorized();
        }

        logger.LogDebug("JellyfinCapabilities UserId={UserId} PlayableMediaTypes={MediaTypes} SupportsMediaControl={SupportsMediaControl}",
            user.Id, playableMediaTypes ?? "none", supportsMediaControl);

        return NoContent();
    }
}

// Models for playback reporting
public record JellyfinPlaybackStartInfo
{
    [JsonPropertyName("ItemId")]
    public string? ItemId { get; init; }

    [JsonPropertyName("PlaySessionId")]
    public string? PlaySessionId { get; init; }

    [JsonPropertyName("MediaSourceId")]
    public string? MediaSourceId { get; init; }

    [JsonPropertyName("AudioStreamIndex")]
    public int? AudioStreamIndex { get; init; }

    [JsonPropertyName("SubtitleStreamIndex")]
    public int? SubtitleStreamIndex { get; init; }

    [JsonPropertyName("PositionTicks")]
    public long? PositionTicks { get; init; }

    [JsonPropertyName("PlaybackStartTimeTicks")]
    public long? PlaybackStartTimeTicks { get; init; }

    [JsonPropertyName("IsPaused")]
    public bool? IsPaused { get; init; }

    [JsonPropertyName("IsMuted")]
    public bool? IsMuted { get; init; }

    [JsonPropertyName("VolumeLevel")]
    public int? VolumeLevel { get; init; }

    [JsonPropertyName("PlayMethod")]
    public string? PlayMethod { get; init; }

    [JsonPropertyName("LiveStreamId")]
    public string? LiveStreamId { get; init; }

    [JsonPropertyName("CanSeek")]
    public bool? CanSeek { get; init; }
}

public record JellyfinPlaybackProgressInfo
{
    [JsonPropertyName("ItemId")]
    public string? ItemId { get; init; }

    [JsonPropertyName("PlaySessionId")]
    public string? PlaySessionId { get; init; }

    [JsonPropertyName("MediaSourceId")]
    public string? MediaSourceId { get; init; }

    [JsonPropertyName("PositionTicks")]
    public long? PositionTicks { get; init; }

    [JsonPropertyName("IsPaused")]
    public bool? IsPaused { get; init; }

    [JsonPropertyName("IsMuted")]
    public bool? IsMuted { get; init; }

    [JsonPropertyName("VolumeLevel")]
    public int? VolumeLevel { get; init; }

    [JsonPropertyName("AudioStreamIndex")]
    public int? AudioStreamIndex { get; init; }

    [JsonPropertyName("SubtitleStreamIndex")]
    public int? SubtitleStreamIndex { get; init; }

    [JsonPropertyName("PlayMethod")]
    public string? PlayMethod { get; init; }

    [JsonPropertyName("LiveStreamId")]
    public string? LiveStreamId { get; init; }

    [JsonPropertyName("RepeatMode")]
    public string? RepeatMode { get; init; }
}

public record JellyfinPlaybackStopInfo
{
    [JsonPropertyName("ItemId")]
    public string? ItemId { get; init; }

    [JsonPropertyName("PlaySessionId")]
    public string? PlaySessionId { get; init; }

    [JsonPropertyName("MediaSourceId")]
    public string? MediaSourceId { get; init; }

    [JsonPropertyName("PositionTicks")]
    public long? PositionTicks { get; init; }

    [JsonPropertyName("LiveStreamId")]
    public string? LiveStreamId { get; init; }

    [JsonPropertyName("Failed")]
    public bool? Failed { get; init; }

    [JsonPropertyName("NextMediaType")]
    public string? NextMediaType { get; init; }
}

public record JellyfinClientCapabilitiesRequest
{
    [JsonPropertyName("PlayableMediaTypes")]
    public string[]? PlayableMediaTypes { get; init; }

    [JsonPropertyName("SupportedCommands")]
    public string[]? SupportedCommands { get; init; }

    [JsonPropertyName("SupportsMediaControl")]
    public bool? SupportsMediaControl { get; init; }

    [JsonPropertyName("SupportsPersistentIdentifier")]
    public bool? SupportsPersistentIdentifier { get; init; }

    [JsonPropertyName("Id")]
    public string? Id { get; init; }

    [JsonPropertyName("DeviceProfile")]
    public object? DeviceProfile { get; init; }

    [JsonPropertyName("AppStoreUrl")]
    public string? AppStoreUrl { get; init; }

    [JsonPropertyName("IconUrl")]
    public string? IconUrl { get; init; }
}


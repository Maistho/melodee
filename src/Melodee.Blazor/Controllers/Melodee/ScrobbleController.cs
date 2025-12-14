using System.Security.Cryptography;
using System.Text;
using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Blazor.Services;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class ScrobbleController(
    ILogger logger,
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    SongService songService,
    ScrobbleService scrobbleService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    [HttpPost]
    public async Task<IActionResult> ScrobbleSong([FromBody] ScrobbleRequest scrobbleRequest, CancellationToken cancellationToken = default)
    {
        if (!ApiRequest.IsAuthorized)
        {
            return Unauthorized(new { error = "Authorization token is invalid" });
        }

        var userResult = await userService.GetByApiKeyAsync(SafeParser.ToGuid(ApiRequest.ApiKey) ?? Guid.Empty, cancellationToken).ConfigureAwait(false);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return Unauthorized(new { error = "Authorization token is invalid" });
        }

        if (userResult.Data.IsLocked)
        {
            return Forbid("User is locked");
        }

        var songRequest = await songService.GetByApiKeyAsync(scrobbleRequest.SongId, cancellationToken).ConfigureAwait(false);
        if (!songRequest.IsSuccess || songRequest.Data == null)
        {
            logger.Warning("[{ControllerName}] [{MethodName}] Scrobble request for unknown song [{Request}]",
                nameof(ScrobbleController),
                nameof(ScrobbleSong),
                scrobbleRequest);
            return BadRequest(new { error = "Unknown song" });
        }

        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        await scrobbleService.InitializeAsync(configuration, cancellationToken).ConfigureAwait(false);

        OperationResult<bool>? result = null;

        if (scrobbleRequest.ScrobbleTypeValue == ScrobbleRequestType.NowPlaying)
        {
            result = await scrobbleService.NowPlaying(
                    userResult.Data.ToUserInfo(),
                    scrobbleRequest.SongId,
                    scrobbleRequest.PlayedDuration,
                    scrobbleRequest.PlayerName,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        else if (scrobbleRequest.ScrobbleTypeValue == ScrobbleRequestType.Played)
        {
            result = await scrobbleService.Scrobble(
                    userResult.Data.ToUserInfo(),
                    scrobbleRequest.SongId,
                    false,
                    scrobbleRequest.PlayerName,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (result != null)
        {
            if (result.IsSuccess)
            {
                return Ok();
            }

            logger.Warning("[{ControllerName}] [{MethodName}] Scrobble request for unknown song [{Request}] Message [{Message}",
                nameof(ScrobbleController),
                nameof(ScrobbleSong),
                scrobbleRequest,
                result.Messages?.First() ?? "Unknown error");
            return BadRequest(result.Messages?.First() ?? "Unknown error");
        }

        logger.Warning("[{ControllerName}] [{MethodName}] Scrobble request for unknown song [{Request}] Message [{Message}",
            nameof(ScrobbleController),
            nameof(ScrobbleSong),
            scrobbleRequest,
            $"Unknown scrobble type: {scrobbleRequest.ScrobbleType}");
        return BadRequest($"Unknown scrobble type: {scrobbleRequest.ScrobbleType}");
    }

    [HttpGet]
    [Route("lastfm/auth-url")]
    public async Task<IActionResult> GetLastFmAuthUrl([FromQuery] string callback, CancellationToken cancellationToken = default)
    {
        if (!ApiRequest.IsAuthorized)
        {
            return Unauthorized(new { error = "Authorization token is invalid" });
        }

        var config = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var apiKey = config.GetValue<string>(SettingRegistry.ScrobblingLastFmApiKey);
        if (apiKey.Nullify() == null)
        {
            return BadRequest(new { error = "Last.fm not configured" });
        }

        var url = $"https://www.last.fm/api/auth/?api_key={apiKey}&cb={callback}";
        return Ok(new { url });
    }

    [HttpPost]
    [Route("lastfm/session")]
    public async Task<IActionResult> ExchangeLastFmSession([FromBody] LastFmSessionRequest request, CancellationToken cancellationToken = default)
    {
        if (!ApiRequest.IsAuthorized)
        {
            return Unauthorized(new { error = "Authorization token is invalid" });
        }

        var userResult = await userService.GetByApiKeyAsync(SafeParser.ToGuid(ApiRequest.ApiKey) ?? Guid.Empty, cancellationToken).ConfigureAwait(false);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return Unauthorized(new { error = "Authorization token is invalid" });
        }

        var sessionKey = await GetLastFmSessionKeyAsync(request.Token, cancellationToken).ConfigureAwait(false);
        if (sessionKey.Nullify() == null)
        {
            return BadRequest(new { error = "Unable to exchange Last.fm session key" });
        }

        var updateResult = await userService.SetLastFmSessionKeyAsync(userResult.Data.Id, sessionKey, cancellationToken)
            .ConfigureAwait(false);
        if (!updateResult.IsSuccess)
        {
            return BadRequest(updateResult.Messages?.FirstOrDefault() ?? "Unable to save Last.fm session key");
        }

        return Ok(new { message = "Last.fm linked" });
    }

    [HttpPost]
    [Route("lastfm/disconnect")]
    public async Task<IActionResult> DisconnectLastFm(CancellationToken cancellationToken = default)
    {
        if (!ApiRequest.IsAuthorized)
        {
            return Unauthorized(new { error = "Authorization token is invalid" });
        }

        var userResult = await userService.GetByApiKeyAsync(SafeParser.ToGuid(ApiRequest.ApiKey) ?? Guid.Empty, cancellationToken).ConfigureAwait(false);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return Unauthorized(new { error = "Authorization token is invalid" });
        }

        await userService.SetLastFmSessionKeyAsync(userResult.Data.Id, null, cancellationToken).ConfigureAwait(false);
        return Ok(new { message = "Last.fm disconnected" });
    }

    private async Task<string?> GetLastFmSessionKeyAsync(string token, CancellationToken cancellationToken)
    {
        var config = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var apiKey = config.GetValue<string>(SettingRegistry.ScrobblingLastFmApiKey);
        var secret = config.GetValue<string>(SettingRegistry.ScrobblingLastFmSharedSecret);
        if (apiKey.Nullify() == null || secret.Nullify() == null)
        {
            return null;
        }

        var signature = BuildApiSignature(apiKey!, secret!, token);
        var requestUri =
            $"https://ws.audioscrobbler.com/2.0/?method=auth.getSession&api_key={apiKey}&token={token}&api_sig={signature}&format=json";

        using var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var sessionResponse = Serializer.Deserialize<LastFmSessionResponse>(content);
            return sessionResponse?.Session?.Key;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed exchanging Last.fm session key");
            return null;
        }
    }

    private static string BuildApiSignature(string apiKey, string secret, string token)
    {
        var sigString = $"api_key{apiKey}methodauth.getSessiontoken{token}{secret}";
        using var md5 = MD5.Create();
        var bytes = Encoding.UTF8.GetBytes(sigString);
        var hash = md5.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

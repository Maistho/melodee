using System.Net;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Models;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Models.OpenSubsonic.Responses.Jukebox;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Jukebox;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

/// <summary>
/// Jukebox control endpoints for Subsonic/OpenSubsonic clients.
/// Only works when Jukebox is enabled with a backend configured.
/// Returns 410 Gone when disabled.
/// </summary>
public class JukeboxController(
    ISerializer serializer,
    EtagRepository etagRepository,
    IMelodeeConfigurationFactory configurationFactory,
    ISubsonicJukeboxService subsonicJukeboxService,
    global::Melodee.Common.Services.OpenSubsonicApiService openSubsonicApiService)
    : ControllerBase(etagRepository, serializer, configurationFactory)
{
    /// <summary>
    /// Jukebox control - main entry point for all jukebox operations.
    /// Returns 410 Gone when jukebox is not enabled.
    /// </summary>
    [HttpGet]
    [HttpPost]
    [Route("/rest/jukeboxControl.view")]
    [Route("/rest/jukeboxControl")]
    public async Task<IActionResult> JukeboxControl(
        [FromQuery] string? action = null,
        [FromQuery] int? index = null,
        [FromQuery] int? offset = null,
        [FromQuery] double? gain = null,
        [FromQuery] string? id = null,
        [FromQuery] string? ids = null,
        CancellationToken cancellationToken = default)
    {
        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var jukeboxEnabled = configuration.GetValue<bool>(SettingRegistry.JukeboxEnabled);
        var backendType = configuration.GetValue<string>(SettingRegistry.JukeboxBackendType);

        if (!jukeboxEnabled || string.IsNullOrEmpty(backendType))
        {
            HttpContext.Response.Headers.Append("Cache-Control", "no-cache");
            return StatusCode((int)HttpStatusCode.Gone);
        }

        // Auth check
        var authResponse = await openSubsonicApiService.AuthenticateSubsonicApiAsync(ApiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return await MakeResult(Task.FromResult(authResponse)).ConfigureAwait(false);
        }

        var userId = authResponse.UserInfo.Id;

        try
        {
            return action?.ToLowerInvariant() switch
            {
                "get" => await GetJukeboxAsync(userId, cancellationToken).ConfigureAwait(false),
                "status" => await GetStatusAsync(userId, cancellationToken).ConfigureAwait(false),
                "set" => await SetGainAsync(userId, gain ?? SettingDefaults.JukeboxDefaultGain, cancellationToken).ConfigureAwait(false),
                "start" => await StartAsync(userId, cancellationToken).ConfigureAwait(false),
                "stop" => await StopAsync(userId, cancellationToken).ConfigureAwait(false),
                "skip" => await SkipAsync(userId, index, offset, cancellationToken).ConfigureAwait(false),
                "add" => await AddAsync(userId, id, ids, cancellationToken).ConfigureAwait(false),
                "clear" => await ClearAsync(userId, cancellationToken).ConfigureAwait(false),
                "remove" => await RemoveAsync(userId, index ?? 0, cancellationToken).ConfigureAwait(false),
                "shuffle" => await ShuffleAsync(userId, cancellationToken).ConfigureAwait(false),
                _ => await GetStatusAsync(userId, cancellationToken).ConfigureAwait(false)
            };
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error in jukeboxControl");
            var errorResponse = await CreateErrorResponseAsync(Error.DataNotFoundError.Code, "Jukebox operation failed: " + ex.Message).ConfigureAwait(false);
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }
    }

    private async Task<IActionResult> GetStatusAsync(int userId, CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.GetStatusAsync(userId, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess || result.Data == null)
        {
            var errorResponse = await CreateErrorResponseAsync(Error.DataNotFoundError.Code, result.Errors?.FirstOrDefault()?.Message ?? "Failed to get jukebox status").ConfigureAwait(false);
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        var response = await CreateJukeboxStatusResponseAsync(result.Data).ConfigureAwait(false);
        return await MakeResult(Task.FromResult(response)).ConfigureAwait(false);
    }

    private async Task<IActionResult> GetJukeboxAsync(int userId, CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.GetPlaylistAsync(userId, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess || result.Data == null)
        {
            var errorResponse = await CreateErrorResponseAsync(Error.DataNotFoundError.Code, result.Errors?.FirstOrDefault()?.Message ?? "Failed to get jukebox playlist").ConfigureAwait(false);
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        var response = new ResponseModel
        {
            IsSuccess = true,
            UserInfo = UserInfo.BlankUserInfo,
            ResponseData = await openSubsonicApiService.NewApiResponse(true, "jukeboxPlaylist", string.Empty, null, new JukeboxGetResponse(result.Data.Playlist)).ConfigureAwait(false)
        };

        return await MakeResult(Task.FromResult(response)).ConfigureAwait(false);
    }

    private async Task<IActionResult> SetGainAsync(int userId, double gainValue, CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.SetGainAsync(userId, gainValue, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResponse = await CreateErrorResponseAsync(Error.DataNotFoundError.Code, result.Errors?.FirstOrDefault()?.Message ?? "Failed to set gain").ConfigureAwait(false);
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        return await GetStatusAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> StartAsync(int userId, CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.StartAsync(userId, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResponse = await CreateErrorResponseAsync(Error.DataNotFoundError.Code, result.Errors?.FirstOrDefault()?.Message ?? "Failed to start playback").ConfigureAwait(false);
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        return await GetStatusAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> StopAsync(int userId, CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.StopAsync(userId, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResponse = await CreateErrorResponseAsync(Error.DataNotFoundError.Code, result.Errors?.FirstOrDefault()?.Message ?? "Failed to stop playback").ConfigureAwait(false);
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        return await GetStatusAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> SkipAsync(int userId, int? index, int? offset, CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.SkipAsync(userId, index, offset, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResponse = await CreateErrorResponseAsync(Error.DataNotFoundError.Code, result.Errors?.FirstOrDefault()?.Message ?? "Failed to skip").ConfigureAwait(false);
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        return await GetStatusAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> AddAsync(int userId, string? id, string? ids, CancellationToken cancellationToken)
    {
        var songIds = new List<string>();

        if (!string.IsNullOrEmpty(id))
        {
            songIds.Add(id);
        }

        if (!string.IsNullOrEmpty(ids))
        {
            songIds.AddRange(ids.Split(',').Where(x => !string.IsNullOrEmpty(x)));
        }

        if (!songIds.Any())
        {
            var errorResponse = await CreateErrorResponseAsync(Error.DataNotFoundError.Code, "No song IDs provided").ConfigureAwait(false);
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        var result = await subsonicJukeboxService.AddAsync(userId, songIds, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResponse = await CreateErrorResponseAsync(Error.DataNotFoundError.Code, result.Errors?.FirstOrDefault()?.Message ?? "Failed to add songs").ConfigureAwait(false);
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        return await GetStatusAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> ClearAsync(int userId, CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.ClearAsync(userId, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResponse = await CreateErrorResponseAsync(Error.DataNotFoundError.Code, result.Errors?.FirstOrDefault()?.Message ?? "Failed to clear playlist").ConfigureAwait(false);
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        return await GetStatusAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> RemoveAsync(int userId, int index, CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.RemoveAsync(userId, index, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResponse = await CreateErrorResponseAsync(Error.DataNotFoundError.Code, result.Errors?.FirstOrDefault()?.Message ?? "Failed to remove song").ConfigureAwait(false);
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        return await GetStatusAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> ShuffleAsync(int userId, CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.ShuffleAsync(userId, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResponse = await CreateErrorResponseAsync(Error.DataNotFoundError.Code, result.Errors?.FirstOrDefault()?.Message ?? "Failed to shuffle playlist").ConfigureAwait(false);
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        return await GetStatusAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ResponseModel> CreateJukeboxStatusResponseAsync(JukeboxStatusResponse status)
    {
        return new ResponseModel
        {
            IsSuccess = true,
            UserInfo = UserInfo.BlankUserInfo,
            ResponseData = await openSubsonicApiService.NewApiResponse(true, "jukeboxStatus", string.Empty, null, status).ConfigureAwait(false)
        };
    }

    private async Task<ResponseModel> CreateErrorResponseAsync(short code, string message)
    {
        return new ResponseModel
        {
            IsSuccess = false,
            UserInfo = UserInfo.BlankUserInfo,
            ResponseData = await openSubsonicApiService.NewApiResponse(false, "error", string.Empty, new Error(code, message)).ConfigureAwait(false)
        };
    }
}

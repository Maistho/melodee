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
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
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

        try
        {
            return action?.ToLowerInvariant() switch
            {
                "get" => await GetJukeboxAsync(cancellationToken).ConfigureAwait(false),
                "status" => await GetStatusAsync(cancellationToken).ConfigureAwait(false),
                "set" => await SetGainAsync(gain ?? 0.8, cancellationToken).ConfigureAwait(false),
                "start" => await StartAsync(cancellationToken).ConfigureAwait(false),
                "stop" => await StopAsync(cancellationToken).ConfigureAwait(false),
                "skip" => await SkipAsync(index, offset, cancellationToken).ConfigureAwait(false),
                "add" => await AddAsync(id, ids, cancellationToken).ConfigureAwait(false),
                "clear" => await ClearAsync(cancellationToken).ConfigureAwait(false),
                "remove" => await RemoveAsync(index ?? 0, cancellationToken).ConfigureAwait(false),
                "shuffle" => await ShuffleAsync(cancellationToken).ConfigureAwait(false),
                _ => await GetStatusAsync(cancellationToken).ConfigureAwait(false)
            };
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error in jukeboxControl");
            var errorResponse = CreateErrorResponse(70, "Jukebox operation failed: " + ex.Message);
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }
    }

    private async Task<IActionResult> GetStatusAsync(CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.GetStatusAsync(cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess || result.Data == null)
        {
            var errorResponse = CreateErrorResponse(70, result.Errors?.FirstOrDefault()?.Message ?? "Failed to get jukebox status");
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        var response = CreateJukeboxStatusResponse(result.Data);
        return await MakeResult(Task.FromResult(response)).ConfigureAwait(false);
    }

    private async Task<IActionResult> GetJukeboxAsync(CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.GetPlaylistAsync(cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess || result.Data == null)
        {
            var errorResponse = CreateErrorResponse(70, result.Errors?.FirstOrDefault()?.Message ?? "Failed to get jukebox playlist");
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        var response = new ResponseModel
        {
            IsSuccess = true,
            UserInfo = UserInfo.BlankUserInfo,
            ResponseData = new ApiResponse
            {
                IsSuccess = true,
                Version = "1.16.1",
                Type = "melodee",
                ServerVersion = "1.0.0",
                DataPropertyName = "jukeboxPlaylist",
                Data = new JukeboxGetResponse(result.Data.Playlist)
            }
        };

        return await MakeResult(Task.FromResult(response)).ConfigureAwait(false);
    }

    private async Task<IActionResult> SetGainAsync(double gainValue, CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.SetGainAsync(gainValue, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResponse = CreateErrorResponse(70, result.Errors?.FirstOrDefault()?.Message ?? "Failed to set gain");
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        return await GetStatusAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> StartAsync(CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.StartAsync(cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResponse = CreateErrorResponse(70, result.Errors?.FirstOrDefault()?.Message ?? "Failed to start playback");
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        return await GetStatusAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> StopAsync(CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.StopAsync(cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResponse = CreateErrorResponse(70, result.Errors?.FirstOrDefault()?.Message ?? "Failed to stop playback");
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        return await GetStatusAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> SkipAsync(int? index, int? offset, CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.SkipAsync(index, offset, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResponse = CreateErrorResponse(70, result.Errors?.FirstOrDefault()?.Message ?? "Failed to skip");
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        return await GetStatusAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> AddAsync(string? id, string? ids, CancellationToken cancellationToken)
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
            var errorResponse = CreateErrorResponse(70, "No song IDs provided");
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        var result = await subsonicJukeboxService.AddAsync(songIds, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResponse = CreateErrorResponse(70, result.Errors?.FirstOrDefault()?.Message ?? "Failed to add songs");
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        return await GetStatusAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> ClearAsync(CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.ClearAsync(cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResponse = CreateErrorResponse(70, result.Errors?.FirstOrDefault()?.Message ?? "Failed to clear playlist");
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        return await GetStatusAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> RemoveAsync(int index, CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.RemoveAsync(index, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResponse = CreateErrorResponse(70, result.Errors?.FirstOrDefault()?.Message ?? "Failed to remove song");
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        return await GetStatusAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> ShuffleAsync(CancellationToken cancellationToken)
    {
        var result = await subsonicJukeboxService.ShuffleAsync(cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResponse = CreateErrorResponse(70, result.Errors?.FirstOrDefault()?.Message ?? "Failed to shuffle playlist");
            return await MakeResult(Task.FromResult(errorResponse)).ConfigureAwait(false);
        }

        return await GetStatusAsync(cancellationToken).ConfigureAwait(false);
    }

    private ResponseModel CreateJukeboxStatusResponse(JukeboxStatusResponse status)
    {
        return new ResponseModel
        {
            IsSuccess = true,
            UserInfo = UserInfo.BlankUserInfo,
            ResponseData = new ApiResponse
            {
                IsSuccess = true,
                Version = "1.16.1",
                Type = "melodee",
                ServerVersion = "1.0.0",
                DataPropertyName = "jukeboxStatus",
                Data = status
            }
        };
    }

    private static ResponseModel CreateErrorResponse(short code, string message)
    {
        return new ResponseModel
        {
            IsSuccess = false,
            UserInfo = UserInfo.BlankUserInfo,
            ResponseData = new ApiResponse
            {
                IsSuccess = false,
                Version = "1.16.1",
                Type = "melodee",
                ServerVersion = "1.0.0",
                DataPropertyName = "error",
                Error = new Error(code, message)
            }
        };
    }
}

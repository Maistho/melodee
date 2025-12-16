using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ServiceFilter(typeof(MelodeeApiAuthFilter))]
[RequireCapability(UserCapability.Playlist)]
[EnableRateLimiting("melodee-api")]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class PlaylistsController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    PlaylistService playlistService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    [HttpGet]
    [Route("{id:guid}")]
    public async Task<IActionResult> PlaylistById(Guid id, CancellationToken cancellationToken = default)
    {
        if (!ApiRequest.IsAuthorized)
        {
            return ApiUnauthorized();
        }

        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (user.IsLocked)
        {
            return ApiUserLocked();
        }

        var playlistResult = await playlistService.GetByApiKeyAsync(user.ToUserInfo(), id, cancellationToken).ConfigureAwait(false);
        if (!playlistResult.IsSuccess || playlistResult.Data == null)
        {
            return ApiNotFound("Playlist");
        }

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);
        return Ok(playlistResult.Data.ToPlaylistModel(
            baseUrl,
            user.ToUserModel(baseUrl)));
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync(short page, short pageSize, CancellationToken cancellationToken = default)
    {
        if (!ApiRequest.IsAuthorized)
        {
            return ApiUnauthorized();
        }

        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (user.IsLocked)
        {
            return ApiUserLocked();
        }

        if (!TryValidatePaging(page, pageSize, out var validatedPage, out var validatedPageSize, out var pagingError))
        {
            return pagingError!;
        }

        var playlists = await playlistService.ListAsync(user.ToUserInfo(), new PagedRequest { Page = validatedPage, PageSize = validatedPageSize }, cancellationToken).ConfigureAwait(false);
        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);
        return Ok(new
        {
            meta = new PaginationMetadata(
                playlists.TotalCount,
                validatedPageSize,
                validatedPage,
                playlists.TotalPages
            ),
            data = playlists.Data.Select(x => x.ToPlaylistModel(baseUrl, user.ToUserModel(baseUrl))).ToArray()
        });
    }

    [HttpGet]
    [Route("{apiKey:guid}/songs")]
    public async Task<IActionResult> SongsForPlaylist(Guid apiKey, int? page, short? pageSize, CancellationToken cancellationToken = default)
    {
        if (!ApiRequest.IsAuthorized)
        {
            return ApiUnauthorized();
        }

        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (user.IsLocked)
        {
            return ApiUserLocked();
        }

        var normalizedPage = page ?? 1;
        var normalizedPageSize = pageSize ?? 50;
        if (!TryValidatePaging((short)normalizedPage, (short)normalizedPageSize, out var validatedPage, out var validatedPageSize, out var pagingError))
        {
            return pagingError!;
        }

        var userInfo = user.ToUserInfo();
        var playlistResult = await playlistService.GetByApiKeyAsync(userInfo, apiKey, cancellationToken).ConfigureAwait(false);
        if (!playlistResult.IsSuccess || playlistResult.Data == null)
        {
            return ApiNotFound("Playlist");
        }

        var songsForPlaylistResult = await playlistService.SongsForPlaylistAsync(apiKey,
            userInfo,
            new PagedRequest
            {
                PageSize = validatedPageSize,
                Page = validatedPage
            },
            cancellationToken).ConfigureAwait(false);
        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);
        return Ok(new
        {
            meta = new PaginationMetadata(
                songsForPlaylistResult.TotalCount,
                validatedPageSize,
                validatedPage,
                songsForPlaylistResult.TotalPages
            ),
            data = songsForPlaylistResult.Data.Select(x => x.ToSongModel(baseUrl, user.ToUserModel(baseUrl), user.PublicKey, GetClientBinding()))
        });
    }
}

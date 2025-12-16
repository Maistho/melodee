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
using Melodee.Common.Models.Collection;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ServiceFilter(typeof(MelodeeApiAuthFilter))]
[EnableRateLimiting("melodee-api")]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AlbumsController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    AlbumService albumService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    private static readonly HashSet<string> AlbumOrderFields =
    [
        nameof(AlbumDataInfo.CreatedAt),
        nameof(AlbumDataInfo.ReleaseDate),
        nameof(AlbumDataInfo.Name)
    ];

    [HttpGet]
    [Route("{id:guid}")]
    public async Task<IActionResult> AlbumById(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        var albumResult = await albumService.GetByApiKeyAsync(id, cancellationToken).ConfigureAwait(false);
        if (!albumResult.IsSuccess || albumResult.Data == null)
        {
            return ApiNotFound("Album");
        }

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);
        return Ok(albumResult.Data.ToAlbumDataInfo().ToAlbumModel(
            baseUrl,
            user.ToUserModel(baseUrl)));
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync(short page, short pageSize, string? orderBy, string? orderDirection, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, pageSize, out var validatedPage, out var validatedPageSize, out var pagingError))
        {
            return pagingError!;
        }

        if (!TryValidateOrdering(orderBy, orderDirection, AlbumOrderFields, out var validatedOrder, out var orderError))
        {
            return orderError!;
        }

        var listResult = await albumService.ListAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedPageSize,
            OrderBy = new Dictionary<string, string> { { validatedOrder.field, validatedOrder.direction } }
        }, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            meta = new PaginationMetadata(
                listResult.TotalCount,
                validatedPageSize,
                validatedPage,
                listResult.TotalPages
            ),
            data = listResult.Data.Select(x => x.ToAlbumModel(baseUrl, user.ToUserModel(baseUrl))).ToArray()
        });
    }

    [HttpGet]
    [Route("recent")]
    public async Task<IActionResult> RecentlyAddedAsync(short limit, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidateLimit(limit, out var validatedLimit, out var limitError))
        {
            return limitError!;
        }

        var albumRecentResult = await albumService.ListAsync(new PagedRequest
        {
            Page = 1,
            PageSize = validatedLimit,
            OrderBy = new Dictionary<string, string> { { nameof(AlbumDataInfo.CreatedAt), PagedRequest.OrderDescDirection } }
        }, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            meta = new PaginationMetadata(
                albumRecentResult.TotalCount,
                validatedLimit,
                1,
                albumRecentResult.TotalPages
            ),
            data = albumRecentResult.Data.Select(x => x.ToAlbumModel(baseUrl, user.ToUserModel(baseUrl))).ToArray()
        });
    }

    [HttpGet]
    [Route("{id:guid}/songs")]
    public async Task<IActionResult> AlbumSongsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        var albumResult = await albumService.GetByApiKeyAsync(id, cancellationToken).ConfigureAwait(false);
        if (!albumResult.IsSuccess || albumResult.Data == null)
        {
            return ApiNotFound("Album");
        }

        var userSongsForAlbum = await userService.UserSongsForAlbumAsync(user.Id, albumResult.Data!.ApiKey, cancellationToken);
        if (userSongsForAlbum != null)
        {
            // Now set the userrating on songs for the album 
            foreach (var song in albumResult.Data.Songs)
            {
                var userSong = userSongsForAlbum.FirstOrDefault(x => x.Song.ApiKey == song.ApiKey);
                if (userSong != null)
                {
                    song.UserSongs.Add(userSong);
                }
            }
        }

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            meta = new PaginationMetadata(
                albumResult.Data.Songs.Count,
                albumResult.Data.SongCount ?? 0,
                1,
                1
            ),
            data = albumResult.Data.Songs.Select(x => x.ToSongDataInfo(x.UserSongs.FirstOrDefault()).ToSongModel(baseUrl, user.ToUserModel(baseUrl), user.PublicKey, GetClientBinding())).ToArray()
        });
    }
}

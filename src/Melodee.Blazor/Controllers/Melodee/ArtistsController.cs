using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Common.Models.Collection;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Mvc;
using Nextended.Core.Extensions;
using Album = Melodee.Blazor.Controllers.Melodee.Models.Album;
using Artist = Melodee.Blazor.Controllers.Melodee.Models.Artist;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ServiceFilter(typeof(MelodeeApiAuthFilter))]
[EnableRateLimiting("melodee-api")]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ArtistsController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    ArtistService artistService,
    AlbumService albumService,
    SongService songService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    private static readonly HashSet<string> ArtistOrderFields =
    [
        nameof(AlbumDataInfo.CreatedAt),
        nameof(ArtistDataInfo.Name)
    ];

    [HttpGet]
    [Route("{id:guid}")]
    public async Task<IActionResult> ArtistById(Guid id, CancellationToken cancellationToken = default)
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

        var artistResult = await artistService.GetByApiKeyAsync(id, cancellationToken).ConfigureAwait(false);
        if (!artistResult.IsSuccess || artistResult.Data == null)
        {
            return ApiNotFound("Artist");
        }

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);
        return Ok(artistResult.Data.ToArtistDataInfo().ToArtistModel(
            baseUrl,
            user.ToUserModel(baseUrl)));
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync(string? q, short page, short pageSize, string? orderBy, string? orderDirection, CancellationToken cancellationToken = default)
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

        if (!TryValidateOrdering(orderBy, orderDirection, ArtistOrderFields, out var validatedOrder, out var orderError))
        {
            return orderError!;
        }

        var filterBy = new List<FilterOperatorInfo>();
        if (q.Nullify() != null)
        {
            filterBy.Add(new FilterOperatorInfo(nameof(ArtistDataInfo.Name), FilterOperator.Contains, q!));
        }
        var listResult = await artistService.ListAsync(new PagedRequest
        {
            Page = page,
            PageSize = validatedPageSize,
            FilterBy = filterBy.ToArray(),
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
            data = listResult.Data.Select(x => x.ToArtistModel(baseUrl, user.ToUserModel(baseUrl))).ToArray()
        });
    }

    [HttpGet]
    [Route("recent")]
    public async Task<IActionResult> RecentlyAddedAsync(short limit, CancellationToken cancellationToken = default)
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


        if (!TryValidateLimit(limit, out var validatedLimit, out var limitError))
        {
            return limitError!;
        }

        var artistRecentResult = await artistService.ListAsync(new PagedRequest
        {
            Page = 1,
            PageSize = validatedLimit,
            OrderBy = new Dictionary<string, string> { { nameof(AlbumDataInfo.CreatedAt), PagedRequest.OrderDescDirection } }
        }, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            meta = new PaginationMetadata(
                artistRecentResult.TotalCount,
                validatedLimit,
                1,
                artistRecentResult.TotalPages
            ),
            data = artistRecentResult.Data.Select(x => x.ToArtistModel(baseUrl, user.ToUserModel(baseUrl))).ToArray()
        });
    }

    [HttpGet]
    [Route("{id:guid}/albums")]
    public async Task<IActionResult> ArtistAlbumsAsync(Guid id, short page, short pageSize, CancellationToken cancellationToken = default)
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

        var artistResult = await artistService.GetByApiKeyAsync(id, cancellationToken).ConfigureAwait(false);
        if (!artistResult.IsSuccess || artistResult.Data == null)
        {
            return ApiNotFound("Artist");
        }

        var artistAlbumsResult = await albumService.ListAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedPageSize,
            FilterBy =
            [
                new FilterOperatorInfo("ArtistId", FilterOperator.Equals, artistResult.Data.Id)
            ],
            OrderBy = new Dictionary<string, string> { { nameof(AlbumDataInfo.CreatedAt), PagedRequest.OrderDescDirection } }
        }, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            meta = new PaginationMetadata(
                artistAlbumsResult.TotalCount,
                validatedPageSize,
                validatedPage,
                artistAlbumsResult.TotalPages
            ),
            data = artistAlbumsResult.Data.Select(x => x.ToAlbumModel(baseUrl, user.ToUserModel(baseUrl))).ToArray()
        });
    }

    [HttpGet]
    [Route("{id:guid}/songs")]
    public async Task<IActionResult> ArtistSongsAsync(Guid id, string? q, short page, short pageSize, CancellationToken cancellationToken = default)
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

        var artistResult = await artistService.GetByApiKeyAsync(id, cancellationToken).ConfigureAwait(false);
        if (!artistResult.IsSuccess || artistResult.Data == null)
        {
            return ApiNotFound("Artist");
        }

        var searchTermNormalized = q?.ToNormalizedString() ?? q ?? string.Empty;
        var songsResult = await songService.ListAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedPageSize,
            FilterBy =
            [
                new FilterOperatorInfo(nameof(SongDataInfo.ArtistApiKey), FilterOperator.Equals, artistResult.Data.ApiKey),
                new FilterOperatorInfo(nameof(SongDataInfo.TitleNormalized), FilterOperator.Contains, searchTermNormalized)
            ],
            OrderBy = new Dictionary<string, string> { { nameof(SongDataInfo.CreatedAt), PagedRequest.OrderDescDirection } }
        }, user.Id, cancellationToken).ConfigureAwait(false);
        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            meta = new PaginationMetadata(
                songsResult.TotalCount,
                validatedPageSize,
                validatedPage,
                songsResult.TotalPages
            ),
            data = songsResult.Data.Select(x => x.ToSongModel(baseUrl, user.ToUserModel(baseUrl), user.PublicKey, GetClientBinding())).ToArray()
        });
    }
}

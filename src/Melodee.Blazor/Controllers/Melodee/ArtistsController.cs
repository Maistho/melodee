using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Blazor.Services;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Common.Models.Collection;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    IBlacklistService blacklistService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    private static readonly HashSet<string> ArtistOrderFields =
    [
        nameof(ArtistDataInfo.Name),
        nameof(ArtistDataInfo.AlbumCount),
        nameof(ArtistDataInfo.SongCount),
        nameof(ArtistDataInfo.LastPlayedAt),
        nameof(ArtistDataInfo.PlayedCount),
        nameof(ArtistDataInfo.CalculatedRating)
    ];

    private static readonly HashSet<string> AlbumOrderFields =
    [
        nameof(AlbumDataInfo.Name),
        nameof(AlbumDataInfo.ReleaseDate),
        nameof(AlbumDataInfo.SongCount),
        nameof(AlbumDataInfo.Duration),
        nameof(AlbumDataInfo.LastPlayedAt),
        nameof(AlbumDataInfo.PlayedCount),
        nameof(AlbumDataInfo.CalculatedRating)
    ];

    private static readonly HashSet<string> SongOrderFields =
    [
        nameof(SongDataInfo.Title),
        nameof(SongDataInfo.SongNumber),
        nameof(SongDataInfo.AlbumId),
        nameof(SongDataInfo.PlayedCount),
        nameof(SongDataInfo.Duration),
        nameof(SongDataInfo.LastPlayedAt),
        nameof(SongDataInfo.CalculatedRating)
    ];

    /// <summary>
    /// Get an artist by ID.
    /// </summary>
    [HttpGet]
    [Route("{id:guid}")]
    [ProducesResponseType(typeof(Artist), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// List all artists with pagination and ordering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ArtistPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Get recently added artists.
    /// </summary>
    [HttpGet]
    [Route("recent")]
    [ProducesResponseType(typeof(ArtistPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Get albums for an artist with pagination.
    /// </summary>
    [HttpGet]
    [Route("{id:guid}/albums")]
    [ProducesResponseType(typeof(AlbumPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ArtistAlbumsAsync(Guid id, short page, short pageSize, string? orderBy, string? orderDirection, CancellationToken cancellationToken = default)
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

        if (!TryValidateOrdering(orderBy, orderDirection, AlbumOrderFields, out var validatedOrder, out var orderError))
        {
            return orderError!;
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
            OrderBy = new Dictionary<string, string> { { validatedOrder.field, validatedOrder.direction } }
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

    /// <summary>
    /// Get songs for an artist with pagination and search.
    /// </summary>
    [HttpGet]
    [Route("{id:guid}/songs")]
    [ProducesResponseType(typeof(SongPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ArtistSongsAsync(Guid id, string? q, short page, short pageSize, string? orderBy, string? orderDirection, CancellationToken cancellationToken = default)
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

        if (!TryValidateOrdering(orderBy, orderDirection, SongOrderFields, out var validatedOrder, out var orderError))
        {
            return orderError!;
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
            OrderBy = new Dictionary<string, string> { { validatedOrder.field, validatedOrder.direction } }
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

    /// <summary>
    /// Toggle starred status for an artist.
    /// </summary>
    [HttpPost]
    [Route("starred/{apiKey:guid}/{isStarred:bool}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ToggleArtistStarred(Guid apiKey, bool isStarred, CancellationToken cancellationToken = default)
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

        if (await blacklistService.IsEmailBlacklistedAsync(user.Email).ConfigureAwait(false) ||
            await blacklistService.IsIpBlacklistedAsync(GetRequestIp(HttpContext)).ConfigureAwait(false))
        {
            return ApiBlacklisted();
        }

        var toggleStarredResult = await userService.ToggleArtistStarAsync(user.Id, apiKey, isStarred, cancellationToken).ConfigureAwait(false);
        if (toggleStarredResult.IsSuccess)
        {
            return Ok();
        }

        return ApiBadRequest("Unable to toggle star for artist for user.");
    }

    /// <summary>
    /// Set rating for an artist.
    /// </summary>
    [HttpPost]
    [Route("setrating/{apiKey:guid}/{rating:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetArtistRating(Guid apiKey, int rating, CancellationToken cancellationToken = default)
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

        if (await blacklistService.IsEmailBlacklistedAsync(user.Email).ConfigureAwait(false) ||
            await blacklistService.IsIpBlacklistedAsync(GetRequestIp(HttpContext)).ConfigureAwait(false))
        {
            return ApiBlacklisted();
        }

        var setRatingResult = await userService.SetArtistRatingAsync(user.Id, apiKey, rating, cancellationToken).ConfigureAwait(false);
        if (setRatingResult.IsSuccess)
        {
            return Ok();
        }

        return ApiBadRequest("Unable to set rating for artist for user.");
    }

    /// <summary>
    /// Toggle hated status for an artist.
    /// </summary>
    [HttpPost]
    [Route("hated/{apiKey:guid}/{isHated:bool}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ToggleArtistHated(Guid apiKey, bool isHated, CancellationToken cancellationToken = default)
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

        if (await blacklistService.IsEmailBlacklistedAsync(user.Email).ConfigureAwait(false) ||
            await blacklistService.IsIpBlacklistedAsync(GetRequestIp(HttpContext)).ConfigureAwait(false))
        {
            return ApiBlacklisted();
        }

        var toggleHatedResult = await userService.ToggleArtistHatedAsync(user.Id, apiKey, isHated, cancellationToken).ConfigureAwait(false);
        if (toggleHatedResult.IsSuccess)
        {
            return Ok();
        }

        return ApiBadRequest("Unable to toggle hated for artist for user.");
    }
}

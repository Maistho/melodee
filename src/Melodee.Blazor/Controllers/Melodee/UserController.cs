using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Blazor.Services;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Melodee.Blazor.Controllers.Melodee;

/// <summary>
/// User-specific endpoints for the authenticated user's library data.
/// Uses singular /user to distinguish from admin /users endpoints.
/// For authentication, use /api/v1/auth.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ServiceFilter(typeof(MelodeeApiAuthFilter))]
[EnableRateLimiting("melodee-api")]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/user")]
public class UserController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    SongService songService,
    AlbumService albumService,
    ArtistService artistService,
    PlaylistService playlistService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    #region User Profile

    /// <summary>
    /// Return information about the current user making the request.
    /// </summary>
    [HttpGet]
    [Route("me")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AboutMeAsync(CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        return Ok(user.ToUserModel(await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false)));
    }

    /// <summary>
    /// Return the last played songs by the user.
    /// </summary>
    [HttpGet]
    [Route("last-played")]
    [ProducesResponseType(typeof(SongPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LastPlayedSongsAsync(short page = 1, short pageSize = 3, CancellationToken cancellationToken = default)
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

        var userLastPlayedResult = await userService.UserLastPlayedSongsAsync(user.Id, validatedPageSize, cancellationToken).ConfigureAwait(false);
        return Ok(new
        {
            data = userLastPlayedResult.Data.Where(x => x?.Song != null).Select(x => x!.Song.ToSongDataInfo()).ToArray(),
            meta = new
            {
                totalCount = userLastPlayedResult.Data.Length,
                pageSize = (int)validatedPageSize,
                currentPage = (int)validatedPage,
                totalPages = 1,
                hasNext = false,
                hasPrevious = false
            }
        });
    }

    /// <summary>
    /// Get playlists for the current user.
    /// </summary>
    [HttpGet]
    [Route("playlists")]
    [RequireCapability(UserCapability.Playlist)]
    [ProducesResponseType(typeof(PlaylistPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlaylistsAsync(int page = 1, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, limit, out var validatedPage, out var validatedLimit, out var pagingError))
        {
            return pagingError!;
        }

        var playlists = await playlistService.ListAsync(user.ToUserInfo(), new PagedRequest { Page = validatedPage, PageSize = validatedLimit }, cancellationToken).ConfigureAwait(false);
        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = playlists.Data.Select(x => x.ToPlaylistModel(baseUrl, user.ToUserModel(baseUrl))).ToArray(),
            meta = new
            {
                totalCount = playlists.TotalCount,
                pageSize = (int)validatedLimit,
                currentPage = (int)validatedPage,
                totalPages = playlists.TotalPages,
                hasNext = validatedPage < playlists.TotalPages,
                hasPrevious = validatedPage > 1
            }
        });
    }

    #endregion

    #region User Songs

    /// <summary>
    /// Get songs the current user has liked (starred).
    /// </summary>
    [HttpGet]
    [Route("songs/liked")]
    [ProducesResponseType(typeof(SongPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LikedSongsAsync(int page = 1, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, limit, out var validatedPage, out var validatedLimit, out var pagingError))
        {
            return pagingError!;
        }

        var result = await songService.ListStarredAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedLimit
        }, user.Id, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = result.Data.Select(x => x.ToSongModel(baseUrl, user.ToUserModel(baseUrl), user.PublicKey, GetClientBinding())).ToArray(),
            meta = new
            {
                totalCount = result.TotalCount,
                pageSize = (int)validatedLimit,
                currentPage = (int)validatedPage,
                totalPages = result.TotalPages,
                hasNext = validatedPage < result.TotalPages,
                hasPrevious = validatedPage > 1
            }
        });
    }

    /// <summary>
    /// Get songs the current user has disliked.
    /// </summary>
    [HttpGet]
    [Route("songs/disliked")]
    [ProducesResponseType(typeof(SongPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DislikedSongsAsync(int page = 1, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, limit, out var validatedPage, out var validatedLimit, out var pagingError))
        {
            return pagingError!;
        }

        var result = await songService.ListHatedAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedLimit
        }, user.Id, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = result.Data.Select(x => x.ToSongModel(baseUrl, user.ToUserModel(baseUrl), user.PublicKey, GetClientBinding())).ToArray(),
            meta = new
            {
                totalCount = result.TotalCount,
                pageSize = (int)validatedLimit,
                currentPage = (int)validatedPage,
                totalPages = result.TotalPages,
                hasNext = validatedPage < result.TotalPages,
                hasPrevious = validatedPage > 1
            }
        });
    }

    /// <summary>
    /// Get all songs the current user has rated, sorted by rating descending.
    /// </summary>
    [HttpGet]
    [Route("songs/rated")]
    [ProducesResponseType(typeof(SongPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RatedSongsAsync(int page = 1, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, limit, out var validatedPage, out var validatedLimit, out var pagingError))
        {
            return pagingError!;
        }

        var result = await songService.ListRatedAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedLimit
        }, user.Id, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = result.Data.Select(x => x.ToSongModel(baseUrl, user.ToUserModel(baseUrl), user.PublicKey, GetClientBinding())).ToArray(),
            meta = new
            {
                totalCount = result.TotalCount,
                pageSize = (int)validatedLimit,
                currentPage = (int)validatedPage,
                totalPages = result.TotalPages,
                hasNext = validatedPage < result.TotalPages,
                hasPrevious = validatedPage > 1
            }
        });
    }

    /// <summary>
    /// Get songs the current user has rated 4+ stars.
    /// </summary>
    [HttpGet]
    [Route("songs/top-rated")]
    [ProducesResponseType(typeof(SongPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TopRatedSongsAsync(int page = 1, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, limit, out var validatedPage, out var validatedLimit, out var pagingError))
        {
            return pagingError!;
        }

        var result = await songService.ListTopRatedAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedLimit
        }, user.Id, 4, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = result.Data.Select(x => x.ToSongModel(baseUrl, user.ToUserModel(baseUrl), user.PublicKey, GetClientBinding())).ToArray(),
            meta = new
            {
                totalCount = result.TotalCount,
                pageSize = (int)validatedLimit,
                currentPage = (int)validatedPage,
                totalPages = result.TotalPages,
                hasNext = validatedPage < result.TotalPages,
                hasPrevious = validatedPage > 1
            }
        });
    }

    /// <summary>
    /// Get songs the current user has recently played, sorted by most recent first.
    /// </summary>
    [HttpGet]
    [Route("songs/recently-played")]
    [ProducesResponseType(typeof(SongPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecentlyPlayedSongsAsync(int page = 1, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, limit, out var validatedPage, out var validatedLimit, out var pagingError))
        {
            return pagingError!;
        }

        var result = await songService.ListRecentlyPlayedAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedLimit
        }, user.Id, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = result.Data.Select(x => x.ToSongModel(baseUrl, user.ToUserModel(baseUrl), user.PublicKey, GetClientBinding())).ToArray(),
            meta = new
            {
                totalCount = result.TotalCount,
                pageSize = (int)validatedLimit,
                currentPage = (int)validatedPage,
                totalPages = result.TotalPages,
                hasNext = validatedPage < result.TotalPages,
                hasPrevious = validatedPage > 1
            }
        });
    }

    #endregion

    #region User Albums

    /// <summary>
    /// Get albums the current user has liked (starred).
    /// </summary>
    [HttpGet]
    [Route("albums/liked")]
    [ProducesResponseType(typeof(AlbumPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LikedAlbumsAsync(int page = 1, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, limit, out var validatedPage, out var validatedLimit, out var pagingError))
        {
            return pagingError!;
        }

        var result = await albumService.ListStarredAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedLimit
        }, user.Id, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = result.Data.Select(x => x.ToAlbumModel(baseUrl, user.ToUserModel(baseUrl))).ToArray(),
            meta = new
            {
                totalCount = result.TotalCount,
                pageSize = (int)validatedLimit,
                currentPage = (int)validatedPage,
                totalPages = result.TotalPages,
                hasNext = validatedPage < result.TotalPages,
                hasPrevious = validatedPage > 1
            }
        });
    }

    /// <summary>
    /// Get albums the current user has disliked.
    /// </summary>
    [HttpGet]
    [Route("albums/disliked")]
    [ProducesResponseType(typeof(AlbumPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DislikedAlbumsAsync(int page = 1, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, limit, out var validatedPage, out var validatedLimit, out var pagingError))
        {
            return pagingError!;
        }

        var result = await albumService.ListHatedAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedLimit
        }, user.Id, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = result.Data.Select(x => x.ToAlbumModel(baseUrl, user.ToUserModel(baseUrl))).ToArray(),
            meta = new
            {
                totalCount = result.TotalCount,
                pageSize = (int)validatedLimit,
                currentPage = (int)validatedPage,
                totalPages = result.TotalPages,
                hasNext = validatedPage < result.TotalPages,
                hasPrevious = validatedPage > 1
            }
        });
    }

    /// <summary>
    /// Get all albums the current user has rated, sorted by rating descending.
    /// </summary>
    [HttpGet]
    [Route("albums/rated")]
    [ProducesResponseType(typeof(AlbumPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RatedAlbumsAsync(int page = 1, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, limit, out var validatedPage, out var validatedLimit, out var pagingError))
        {
            return pagingError!;
        }

        var result = await albumService.ListRatedAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedLimit
        }, user.Id, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = result.Data.Select(x => x.ToAlbumModel(baseUrl, user.ToUserModel(baseUrl))).ToArray(),
            meta = new
            {
                totalCount = result.TotalCount,
                pageSize = (int)validatedLimit,
                currentPage = (int)validatedPage,
                totalPages = result.TotalPages,
                hasNext = validatedPage < result.TotalPages,
                hasPrevious = validatedPage > 1
            }
        });
    }

    /// <summary>
    /// Get albums the current user has rated 4+ stars.
    /// </summary>
    [HttpGet]
    [Route("albums/top-rated")]
    [ProducesResponseType(typeof(AlbumPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TopRatedAlbumsAsync(int page = 1, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, limit, out var validatedPage, out var validatedLimit, out var pagingError))
        {
            return pagingError!;
        }

        var result = await albumService.ListTopRatedAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedLimit
        }, user.Id, 4, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = result.Data.Select(x => x.ToAlbumModel(baseUrl, user.ToUserModel(baseUrl))).ToArray(),
            meta = new
            {
                totalCount = result.TotalCount,
                pageSize = (int)validatedLimit,
                currentPage = (int)validatedPage,
                totalPages = result.TotalPages,
                hasNext = validatedPage < result.TotalPages,
                hasPrevious = validatedPage > 1
            }
        });
    }

    /// <summary>
    /// Get albums the current user has recently played, sorted by most recent first.
    /// </summary>
    [HttpGet]
    [Route("albums/recently-played")]
    [ProducesResponseType(typeof(AlbumPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecentlyPlayedAlbumsAsync(int page = 1, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, limit, out var validatedPage, out var validatedLimit, out var pagingError))
        {
            return pagingError!;
        }

        var result = await albumService.ListRecentlyPlayedAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedLimit
        }, user.Id, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = result.Data.Select(x => x.ToAlbumModel(baseUrl, user.ToUserModel(baseUrl))).ToArray(),
            meta = new
            {
                totalCount = result.TotalCount,
                pageSize = (int)validatedLimit,
                currentPage = (int)validatedPage,
                totalPages = result.TotalPages,
                hasNext = validatedPage < result.TotalPages,
                hasPrevious = validatedPage > 1
            }
        });
    }

    #endregion

    #region User Artists

    /// <summary>
    /// Get artists the current user has liked (starred).
    /// </summary>
    [HttpGet]
    [Route("artists/liked")]
    [ProducesResponseType(typeof(ArtistPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LikedArtistsAsync(int page = 1, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, limit, out var validatedPage, out var validatedLimit, out var pagingError))
        {
            return pagingError!;
        }

        var result = await artistService.ListStarredAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedLimit
        }, user.Id, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = result.Data.Select(x => x.ToArtistModel(baseUrl, user.ToUserModel(baseUrl))).ToArray(),
            meta = new
            {
                totalCount = result.TotalCount,
                pageSize = (int)validatedLimit,
                currentPage = (int)validatedPage,
                totalPages = result.TotalPages,
                hasNext = validatedPage < result.TotalPages,
                hasPrevious = validatedPage > 1
            }
        });
    }

    /// <summary>
    /// Get artists the current user has disliked.
    /// </summary>
    [HttpGet]
    [Route("artists/disliked")]
    [ProducesResponseType(typeof(ArtistPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DislikedArtistsAsync(int page = 1, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, limit, out var validatedPage, out var validatedLimit, out var pagingError))
        {
            return pagingError!;
        }

        var result = await artistService.ListHatedAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedLimit
        }, user.Id, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = result.Data.Select(x => x.ToArtistModel(baseUrl, user.ToUserModel(baseUrl))).ToArray(),
            meta = new
            {
                totalCount = result.TotalCount,
                pageSize = (int)validatedLimit,
                currentPage = (int)validatedPage,
                totalPages = result.TotalPages,
                hasNext = validatedPage < result.TotalPages,
                hasPrevious = validatedPage > 1
            }
        });
    }

    /// <summary>
    /// Get all artists the current user has rated, sorted by rating descending.
    /// </summary>
    [HttpGet]
    [Route("artists/rated")]
    [ProducesResponseType(typeof(ArtistPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RatedArtistsAsync(int page = 1, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, limit, out var validatedPage, out var validatedLimit, out var pagingError))
        {
            return pagingError!;
        }

        var result = await artistService.ListRatedAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedLimit
        }, user.Id, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = result.Data.Select(x => x.ToArtistModel(baseUrl, user.ToUserModel(baseUrl))).ToArray(),
            meta = new
            {
                totalCount = result.TotalCount,
                pageSize = (int)validatedLimit,
                currentPage = (int)validatedPage,
                totalPages = result.TotalPages,
                hasNext = validatedPage < result.TotalPages,
                hasPrevious = validatedPage > 1
            }
        });
    }

    /// <summary>
    /// Get artists the current user has rated 4+ stars.
    /// </summary>
    [HttpGet]
    [Route("artists/top-rated")]
    [ProducesResponseType(typeof(ArtistPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TopRatedArtistsAsync(int page = 1, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, limit, out var validatedPage, out var validatedLimit, out var pagingError))
        {
            return pagingError!;
        }

        var result = await artistService.ListTopRatedAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedLimit
        }, user.Id, 4, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = result.Data.Select(x => x.ToArtistModel(baseUrl, user.ToUserModel(baseUrl))).ToArray(),
            meta = new
            {
                totalCount = result.TotalCount,
                pageSize = (int)validatedLimit,
                currentPage = (int)validatedPage,
                totalPages = result.TotalPages,
                hasNext = validatedPage < result.TotalPages,
                hasPrevious = validatedPage > 1
            }
        });
    }

    /// <summary>
    /// Get artists the current user has recently played, sorted by most recent first.
    /// Artists are considered "recently played" based on songs the user has played from that artist.
    /// </summary>
    [HttpGet]
    [Route("artists/recently-played")]
    [ProducesResponseType(typeof(ArtistPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecentlyPlayedArtistsAsync(int page = 1, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, limit, out var validatedPage, out var validatedLimit, out var pagingError))
        {
            return pagingError!;
        }

        var result = await artistService.ListRecentlyPlayedAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedLimit
        }, user.Id, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = result.Data.Select(x => x.ToArtistModel(baseUrl, user.ToUserModel(baseUrl))).ToArray(),
            meta = new
            {
                totalCount = result.TotalCount,
                pageSize = (int)validatedLimit,
                currentPage = (int)validatedPage,
                totalPages = result.TotalPages,
                hasNext = validatedPage < result.TotalPages,
                hasPrevious = validatedPage > 1
            }
        });
    }

    #endregion
}

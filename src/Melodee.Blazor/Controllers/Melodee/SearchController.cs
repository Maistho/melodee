using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Models.Collection.Extensions;
using Melodee.Common.Models.Search;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ServiceFilter(typeof(MelodeeApiAuthFilter))]
[EnableRateLimiting("melodee-api")]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/search")]
public class SearchController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    SearchService searchService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    /// <summary>
    /// Search for artists, albums, songs, and playlists.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SearchResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchAsync([FromBody] SearchRequest searchRequest, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        var requestedPageSize = searchRequest.PageSize ?? 50;
        if (!TryValidateLimit(requestedPageSize, out var pageSizeValue, out var pagingError))
        {
            return pagingError!;
        }

        // Short-circuit for empty or too-short queries to reduce unnecessary DB load
        var query = searchRequest.Query?.Trim();
        if (string.IsNullOrEmpty(query) || query.Length < 2)
        {
            return Ok(new
            {
                meta = new PaginationMetadata(0, pageSizeValue, 1, 0),
                data = new Models.SearchResult(0, [], 0, [], 0, [], 0, [], 0)
            });
        }

        var includedTyped = SearchInclude.Data;
        var typeValue = searchRequest.Type?.Split(',');
        if (typeValue is { Length: > 0 })
        {
            includedTyped = SearchInclude.Data;
            foreach (var t in typeValue)
            {
                if (Enum.TryParse<SearchInclude>(t, out var include))
                {
                    includedTyped |= include;
                }
            }
        }

        var searchResult = await searchService.DoSearchAsync(user.ApiKey,
                ApiRequest.ApiRequestPlayer.UserAgent,
                searchRequest.Query,
                searchRequest.AlbumPageValue,
                searchRequest.ArtistPageValue,
                searchRequest.SongPageValue,
                pageSizeValue,
                includedTyped,
                searchRequest.FilterByArtistId,
                cancellationToken)
            .ConfigureAwait(false);
        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);
        return Ok(new
        {
            meta = new PaginationMetadata(
                searchResult.Data.TotalCount,
                pageSizeValue,
                1,
                searchResult.Data.TotalCount < 1 ? 0 : (searchResult.Data.TotalCount + pageSizeValue - 1) / pageSizeValue
            ),
            data = searchResult.Data.ToSearchResultModel(baseUrl, user.ToUserModel(baseUrl), user.PublicKey, GetClientBinding())
        });
    }

    /// <summary>
    /// Search for songs with pagination.
    /// </summary>
    [HttpGet]
    [Route("songs")]
    [ProducesResponseType(typeof(SongPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchSongsAsync(string q, short? page, short? pageSize, Guid? filterByArtistApiKey, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        var pageValue = page ?? 1;
        var requestedPageSize = pageSize ?? 50;
        if (!TryValidatePaging(pageValue, requestedPageSize, out var validatedPage, out var validatedPageSize, out var pageError))
        {
            return pageError!;
        }

        // Short-circuit for empty or too-short queries to reduce unnecessary DB load
        var query = q?.Trim();
        if (string.IsNullOrEmpty(query) || query.Length < 2)
        {
            return Ok(new
            {
                meta = new PaginationMetadata(0, validatedPageSize, validatedPage, 0),
                data = Array.Empty<Models.Song>()
            });
        }

        var searchResult = await searchService.DoSearchAsync(user.ApiKey,
                ApiRequest.ApiRequestPlayer.UserAgent,
                q,
                0,
                0,
                validatedPage,
                validatedPageSize,
                SearchInclude.Songs,
                filterByArtistApiKey,
                cancellationToken)
            .ConfigureAwait(false);
        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);
        return Ok(new
        {
            meta = new PaginationMetadata(
                searchResult.Data.TotalCount,
                validatedPageSize,
                validatedPage,
                searchResult.Data.TotalCount < 1 ? 0 : (searchResult.Data.TotalCount + validatedPageSize - 1) / validatedPageSize
            ),
            data = searchResult.Data.Songs.Select(x => x.ToSongModel(baseUrl, user.ToUserModel(baseUrl), user.PublicKey, GetClientBinding()))
        });
    }

    /// <summary>
    /// Get search suggestions/autocomplete for a query. Returns lightweight results for quick display.
    /// </summary>
    [HttpGet]
    [Route("suggest")]
    [ProducesResponseType(typeof(SearchSuggestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SuggestAsync(string q, short? limit, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        var limitValue = limit ?? 10;
        if (limitValue < 1 || limitValue > 50)
        {
            limitValue = 10;
        }

        // Short-circuit for empty or too-short queries
        var query = q?.Trim();
        if (string.IsNullOrEmpty(query) || query.Length < 2)
        {
            return Ok(new SearchSuggestResponse([], [], [], []));
        }

        // Search with a small page size for quick suggestions
        var searchResult = await searchService.DoSearchAsync(user.ApiKey,
                ApiRequest.ApiRequestPlayer.UserAgent,
                query,
                1, // albumPage
                1, // artistPage
                1, // songPage
                limitValue,
                SearchInclude.Artists | SearchInclude.Albums | SearchInclude.Songs | SearchInclude.Playlists,
                null,
                cancellationToken)
            .ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        // Return lightweight suggestion objects
        var artistSuggestions = searchResult.Data.Artists
            .Take(limitValue)
            .Select(a => new SearchSuggestion(a.ApiKey, a.Name, "artist", $"{baseUrl}/images/{a.ToApiKey()}/{MelodeeConfiguration.DefaultThumbNailSize}"))
            .ToArray();

        var albumSuggestions = searchResult.Data.Albums
            .Take(limitValue)
            .Select(a => new SearchSuggestion(a.ApiKey, $"{a.Name} - {a.ArtistName}", "album", $"{baseUrl}/images/{a.ToApiKey()}/{MelodeeConfiguration.DefaultThumbNailSize}"))
            .ToArray();

        var songSuggestions = searchResult.Data.Songs
            .Take(limitValue)
            .Select(s => new SearchSuggestion(s.ApiKey, $"{s.Title} - {s.ArtistName}", "song", $"{baseUrl}/images/{s.ToApiKey()}/{MelodeeConfiguration.DefaultThumbNailSize}"))
            .ToArray();

        var playlistSuggestions = searchResult.Data.Playlists
            .Take(limitValue)
            .Select(p => new SearchSuggestion(p.ApiKey, p.Name, "playlist", $"{baseUrl}/images/{p.ToApiKey()}/{MelodeeConfiguration.DefaultThumbNailSize}"))
            .ToArray();

        return Ok(new SearchSuggestResponse(artistSuggestions, albumSuggestions, songSuggestions, playlistSuggestions));
    }
}

/// <summary>
/// Lightweight suggestion item for autocomplete.
/// </summary>
public record SearchSuggestion(Guid Id, string Name, string Type, string ThumbnailUrl);

/// <summary>
/// Response for search suggestions.
/// </summary>
public record SearchSuggestResponse(
    SearchSuggestion[] Artists,
    SearchSuggestion[] Albums,
    SearchSuggestion[] Songs,
    SearchSuggestion[] Playlists);

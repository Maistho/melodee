using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Models.Search;
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
    [HttpPost]
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

    [HttpGet]
    [Route("songs")]
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
}

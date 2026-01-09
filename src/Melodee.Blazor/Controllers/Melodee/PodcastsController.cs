using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Common.Models.Collection;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ServiceFilter(typeof(MelodeeApiAuthFilter))]
[EnableRateLimiting("melodee-api")]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/podcasts")]
public sealed class PodcastsController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    PodcastService podcastService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory,
    ILogger<PodcastsController> logger) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    private ILogger<PodcastsController> Logger { get; } = logger;

    /// <summary>
    /// List podcast channels.
    /// </summary>
    [HttpGet]
    [Route("channels")]
    [ProducesResponseType(typeof(PagedResult<PodcastChannelDataInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListChannelsAsync(
        short page = 1,
        short pageSize = 50,
        string? orderBy = null,
        string? orderDirection = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null) return ApiUnauthorized();
        if (user.IsLocked) return ApiUserLocked();

        if (!TryValidatePaging(page, pageSize, out var validatedPage, out var validatedPageSize, out var pagingError))
            return pagingError!;

        var pagedRequest = new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedPageSize,
            OrderBy = new Dictionary<string, string> { { orderBy ?? "Title", orderDirection ?? "ASC" } }
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            pagedRequest.FilterBy = new[] { new FilterOperatorInfo(nameof(PodcastChannelDataInfo.TitleNormalized), FilterOperator.Contains, search) };
        }

        var result = await podcastService.ListChannelsAsync(pagedRequest, user.Id, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Create a new podcast channel.
    /// </summary>
    [HttpPost]
    [Route("channels")]
    [ProducesResponseType(typeof(PodcastChannel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateChannelAsync([FromBody] CreateChannelRequest request, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null) return ApiUnauthorized();

        if (string.IsNullOrWhiteSpace(request?.Url))
            return ApiBadRequest("Feed URL is required");

        var result = await podcastService.CreateChannelAsync(user.Id, request.Url, cancellationToken);
        if (!result.IsSuccess)
            return ApiBadRequest(result.Messages?.FirstOrDefault() ?? "Failed to create channel");

        return Ok(result.Data);
    }

    /// <summary>
    /// Refresh a podcast channel.
    /// </summary>
    [HttpPost]
    [Route("channels/{id:int}/refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshChannelAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null) return ApiUnauthorized();

        var channel = await podcastService.GetChannelAsync(id, user.Id, cancellationToken);
        if (channel.Data == null) return ApiNotFound("Channel");

        var result = await podcastService.RefreshChannelAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return ApiBadRequest(result.Messages?.FirstOrDefault() ?? "Failed to refresh channel");

        return Ok();
    }

    /// <summary>
    /// Delete a podcast channel.
    /// </summary>
    [HttpDelete]
    [Route("channels/{id:int}")]
    public async Task<IActionResult> DeleteChannelAsync(int id, bool softDelete = true, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null) return ApiUnauthorized();

        var result = await podcastService.DeleteChannelAsync(id, user.Id, softDelete, cancellationToken);
        if (!result.IsSuccess) return ApiBadRequest(result.Messages?.FirstOrDefault() ?? "Error deleting channel");

        return Ok();
    }

    /// <summary>
    /// List episodes for a channel.
    /// </summary>
    [HttpGet]
    [Route("channels/{id:int}/episodes")]
    public async Task<IActionResult> ListEpisodesAsync(
        int id,
        short page = 1,
        short pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null) return ApiUnauthorized();

        var channel = await podcastService.GetChannelAsync(id, user.Id, cancellationToken);
        if (channel.Data == null) return ApiNotFound("Channel");

        if (!TryValidatePaging(page, pageSize, out var validatedPage, out var validatedPageSize, out var pagingError))
            return pagingError!;

        var offset = (validatedPage - 1) * validatedPageSize;
        var limit = validatedPageSize;

        var result = await podcastService.ListEpisodesAsync(id, user.Id, limit, offset, cancellationToken);

        if (!result.IsSuccess) return ApiBadRequest(result.Messages?.FirstOrDefault() ?? "Error listing episodes");

        return Ok(new
        {
            Data = result.Data.Select(x => new
            {
                x.Id,
                x.Title,
                x.PublishDate,
                x.DownloadStatus,
                x.Duration,
                x.EnclosureLength,
                x.Guid
            }),
            TotalCount = result.AdditionalData.TryGetValue("TotalCount", out var count) ? count : 0
        });
    }

    /// <summary>
    /// Download an episode.
    /// </summary>
    [HttpPost]
    [Route("episodes/{id:int}/download")]
    public async Task<IActionResult> DownloadEpisodeAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null) return ApiUnauthorized();

        var result = await podcastService.QueueDownloadAsync(id, user.Id, cancellationToken);
        if (!result.IsSuccess) return ApiBadRequest(result.Messages?.FirstOrDefault() ?? "Error queuing download");

        return Ok();
    }

    /// <summary>
    /// Delete an episode.
    /// </summary>
    [HttpDelete]
    [Route("episodes/{id:int}")]
    public async Task<IActionResult> DeleteEpisodeAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null) return ApiUnauthorized();

        var result = await podcastService.DeleteEpisodeAsync(id, user.Id, cancellationToken);
        if (!result.IsSuccess) return ApiBadRequest(result.Messages?.FirstOrDefault() ?? "Error deleting episode");

        return Ok();
    }
}

public record CreateChannelRequest(string Url);

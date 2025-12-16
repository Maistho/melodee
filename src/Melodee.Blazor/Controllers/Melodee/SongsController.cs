using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Blazor.Services;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Collection;
using Melodee.Common.Models.Streaming;
using Melodee.Common.Security;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ServiceFilter(typeof(MelodeeApiAuthFilter))]
[RequireCapability(UserCapability.Stream)]
[EnableRateLimiting("melodee-api")]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class SongsController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    SongService songService,
    StreamingLimiter streamingLimiter,
    IConfiguration configuration,
    IBlacklistService blacklistService,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    private static readonly HashSet<string> SongOrderFields =
    [
        nameof(AlbumDataInfo.CreatedAt),
        nameof(SongDataInfo.Title),
        nameof(SongDataInfo.Duration),
        nameof(SongDataInfo.PlayedCount)
    ];

    [HttpGet]
    [Route("{id:guid}")]
    public async Task<IActionResult> SongById(Guid id, CancellationToken cancellationToken = default)
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

        var songResult = await songService.GetByApiKeyAsync(id, cancellationToken).ConfigureAwait(false);
        if (!songResult.IsSuccess || songResult.Data == null)
        {
            return ApiNotFound("Song");
        }

        // Try to enrich with user-specific data (rating/starred) via album scope for this song
        UserSong? userSong = null;
        try
        {
            userSong = await userService.GetUserSongAsync(user.Id, songResult.Data.ApiKey, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            // best-effort; ignore enrichment failures
        }

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(songResult.Data
            .ToSongDataInfo(userSong)
            .ToSongModel(baseUrl, user.ToUserModel(baseUrl), user.PublicKey, GetClientBinding()));
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync(short page, short pageSize, string? orderBy, string? orderDirection, CancellationToken cancellationToken = default)
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

        var listResult = await songService.ListAsync(new PagedRequest
        {
            Page = validatedPage,
            PageSize = validatedPageSize,
            OrderBy = new Dictionary<string, string> { { validatedOrder.field, validatedOrder.direction } }
        }, user.Id, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            meta = new PaginationMetadata(
                listResult.TotalCount,
                validatedPageSize,
                validatedPage,
                listResult.TotalPages
            ),
            data = listResult.Data.Select(x => x.ToSongModel(baseUrl, user.ToUserModel(baseUrl), user.PublicKey, GetClientBinding())).ToArray()
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

        var songRecentResult = await songService.ListAsync(new PagedRequest
        {
            Page = 1,
            PageSize = validatedLimit,
            OrderBy = new Dictionary<string, string> { { nameof(AlbumDataInfo.CreatedAt), PagedRequest.OrderDescDirection } }
        }, user.Id, cancellationToken).ConfigureAwait(false);

        var baseUrl = await GetBaseUrlAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            meta = new PaginationMetadata(
                songRecentResult.TotalCount,
                validatedLimit,
                1,
                songRecentResult.TotalPages
            ),
            data = songRecentResult.Data.Select(x => x.ToSongModel(baseUrl, user.ToUserModel(baseUrl), user.PublicKey, GetClientBinding())).ToArray()
        });
    }

    [HttpPost]
    [Route("starred/{apiKey:guid}/{isStarred:bool}")]
    public async Task<IActionResult>? ToggleSongStarred(Guid apiKey, bool isStarred, CancellationToken cancellationToken = default)
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

        var toggleStarredResult = await userService.ToggleSongStarAsync(user.Id, apiKey, isStarred, cancellationToken).ConfigureAwait(false);
        if (toggleStarredResult.IsSuccess)
        {
            return Ok();
        }

        return ApiBadRequest("Unable to toggle star for song for user.");
    }

    [HttpPost]
    [Route("setrating/{apiKey:guid}/{rating:int}")]
    public async Task<IActionResult>? ToggleSongStarred(Guid apiKey, int rating, CancellationToken cancellationToken = default)
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
        var setRatingResult = await userService.SetSongRatingAsync(user.Id, apiKey, rating, cancellationToken).ConfigureAwait(false);
        if (setRatingResult.IsSuccess)
        {
            return Ok();
        }

        return ApiBadRequest("Unable to set rating for song for user.");
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("/song/stream/{apiKey:guid}/{userApiKey:guid}/{authToken}")]
    public async Task<IActionResult> StreamSong(Guid apiKey, Guid userApiKey, string authToken, CancellationToken cancellationToken = default)
    {
        var userResult = await userService.GetByApiKeyAsync(userApiKey, cancellationToken).ConfigureAwait(false);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return ApiUnauthorized();
        }

        if (userResult.Data.IsLocked)
        {
            return ApiUserLocked();
        }

        if (await blacklistService.IsEmailBlacklistedAsync(userResult.Data.Email).ConfigureAwait(false) ||
            await blacklistService.IsIpBlacklistedAsync(GetRequestIp(HttpContext)).ConfigureAwait(false))
        {
            return ApiBlacklisted();
        }

        var clientBinding = GetClientBinding();
        var hmacService = new HmacTokenService(userResult.Data.PublicKey);
        if (!hmacService.TryValidateTimedToken(authToken.FromBase64(), out var tokenData, out _))
        {
            return ApiUnauthorized("Invalid Auth Token");
        }

        var tokenParts = tokenData.Split(':', 3);
        if (tokenParts.Length < 3 ||
            !Guid.TryParse(tokenParts[0], out var tokenUserId) ||
            !Guid.TryParse(tokenParts[1], out var tokenSongId) ||
            !string.Equals(tokenParts[2], clientBinding, StringComparison.OrdinalIgnoreCase) ||
            tokenUserId != userResult.Data.ApiKey ||
            tokenSongId != apiKey)
        {
            return ApiUnauthorized("Invalid Auth Token");
        }

        // Optional: enforce streaming concurrency limits
        var userKey = $"web:{userResult.Data.Id}";
        if (!await streamingLimiter.TryEnterAsync(userKey, cancellationToken).ConfigureAwait(false))
        {
            return ApiTooManyRequests("Too many concurrent streams");
        }

        // Ensure exit on request completion
        HttpContext.Response.OnCompleted(() => { streamingLimiter.Exit(userKey); return Task.CompletedTask; });

        // Parse range header if present
        var rangeHeader = Request.Headers["Range"].ToString();

        var melodeeConfig = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var useBuffered = melodeeConfig.GetValue<bool?>(SettingRegistry.StreamingUseBufferedResponses) ?? false;

        if (useBuffered)
        {
            // Buffered fallback using descriptor
            var bufferedDescriptorResult = await songService.GetStreamingDescriptorAsync(
                userResult.Data.ToUserInfo(),
                apiKey,
                rangeHeader,
                false,
                cancellationToken).ConfigureAwait(false);

            if (!bufferedDescriptorResult.IsSuccess || bufferedDescriptorResult.Data == null)
            {
                return ApiBadRequest("Unable to load song");
            }
            var bufferedDescriptor = bufferedDescriptorResult.Data;

            foreach (var header in RangeParser.CreateResponseHeaders(bufferedDescriptor, bufferedDescriptor.Range != null ? 206 : 200))
            {
                Response.Headers[header.Key] = header.Value;
            }
            Response.StatusCode = bufferedDescriptor.Range != null ? 206 : 200;

            byte[] bytes;
            if (bufferedDescriptor.Range != null)
            {
                await using var fs = new FileStream(bufferedDescriptor.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                fs.Seek(bufferedDescriptor.Range.Start, SeekOrigin.Begin);
                var len = (int)bufferedDescriptor.Range.GetContentLength(bufferedDescriptor.FileSize);
                bytes = new byte[len];
                var read = await fs.ReadAsync(bytes, 0, len, cancellationToken);
                if (read != len) Array.Resize(ref bytes, read);
            }
            else
            {
                bytes = await System.IO.File.ReadAllBytesAsync(bufferedDescriptor.FilePath, cancellationToken);
            }

            return new FileContentResult(bytes, bufferedDescriptor.ContentType ?? "application/octet-stream")
            {
                FileDownloadName = bufferedDescriptor.FileName
            };
        }

        var descriptorResult = await songService.GetStreamingDescriptorAsync(
            userResult.Data.ToUserInfo(),
            apiKey,
            rangeHeader,
            false, // not a download
            cancellationToken).ConfigureAwait(false);

        if (!descriptorResult.IsSuccess || descriptorResult.Data == null)
        {
            return ApiBadRequest("Unable to load song");
        }

        var descriptor = descriptorResult.Data;

        // Return FileStreamResult for efficient streaming
        if (descriptor.Range != null)
        {
            // For range requests, manually handle the range and disable ASP.NET range processing
            // to avoid double range header issues
            var fileStream = new FileStream(descriptor.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 65536, FileOptions.Asynchronous | FileOptions.SequentialScan);

            // Seek to start position
            fileStream.Seek(descriptor.Range.Start, SeekOrigin.Begin);

            // Create a bounded stream for the range
            var rangeStream = new BoundedStream(fileStream, descriptor.Range.GetContentLength(descriptor.FileSize));

            // Set range response headers manually
            Response.StatusCode = 206;
            Response.Headers["Accept-Ranges"] = "bytes";
            Response.Headers["Content-Range"] = $"bytes {descriptor.Range.Start}-{descriptor.Range.End}/{descriptor.FileSize}";

            return new FileStreamResult(rangeStream, descriptor.ContentType)
            {
                EnableRangeProcessing = false,
                FileDownloadName = descriptor.IsDownload ? descriptor.FileName : null
            };
        }
        else
        {
            // For full file requests, let ASP.NET handle range processing for future requests
            return new PhysicalFileResult(descriptor.FilePath, descriptor.ContentType)
            {
                EnableRangeProcessing = true,
                FileDownloadName = descriptor.IsDownload ? descriptor.FileName : null
            };
        }
    }
}

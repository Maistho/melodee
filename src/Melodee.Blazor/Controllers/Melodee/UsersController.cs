using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Blazor.Services;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Data = Melodee.Common.Data.Models;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ServiceFilter(typeof(MelodeeApiAuthFilter))]
[EnableRateLimiting("melodee-api")]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    PlaylistService playlistService,
    IConfiguration configuration,
    IBlacklistService blacklistService,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    [HttpPost]
    [Route("authenticate")]
    [Route("auth")]
    [AllowAnonymous]
    [EnableRateLimiting("melodee-auth")]
    public async Task<IActionResult> AuthenticateUserAsync([FromBody] LoginModel model, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.Email) && string.IsNullOrWhiteSpace(model.Password))
        {
            return ApiValidationError("Email or password are required");
        }
        OperationResult<Data.User?> authResult;
        if (model.UserName.Nullify() != null)
        {
            authResult = await userService.LoginUserByUsernameAsync(model.UserName ?? string.Empty, model.Password, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            authResult = await userService.LoginUserAsync(model.Email ?? string.Empty, model.Password, cancellationToken).ConfigureAwait(false);
        }
        if (!authResult.IsSuccess || authResult.Data == null)
        {
            return ApiUnauthorized("Invalid credentials");
        }

        if (authResult.Data.IsLocked)
        {
            return ApiUserLocked();
        }

        if (await blacklistService.IsEmailBlacklistedAsync(authResult.Data.Email).ConfigureAwait(false) ||
            await blacklistService.IsIpBlacklistedAsync(GetRequestIp(HttpContext)).ConfigureAwait(false))
        {
            return ApiBlacklisted();
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = Configuration.GetValue<string>("Jwt:Key") ?? throw new InvalidOperationException("JWT signing key is not configured");
        var issuer = Configuration.GetValue<string>("Jwt:Issuer") ?? throw new InvalidOperationException("JWT issuer is not configured");
        var audience = Configuration.GetValue<string>("Jwt:Audience") ?? throw new InvalidOperationException("JWT audience is not configured");
        var key = Encoding.UTF8.GetBytes(jwtKey);
        var tokenHoursString = Configuration.GetSection("MelodeeAuthSettings:TokenHours").Value;
        var tokenHours = SafeParser.ToNumber<int>(tokenHoursString);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Email, authResult.Data.Email),
                new Claim(ClaimTypes.Name, authResult.Data.UserName),
                new Claim(ClaimTypes.Sid, authResult.Data.ApiKey.ToString())
            ]),
            Expires = DateTime.UtcNow.AddHours(tokenHours),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var serverVersion = configuration.ApiVersion();
        return Ok(new
        {
            user = authResult.Data.ToUserModel(GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false))),
            serverVersion = configuration.ApiVersion(),
            token = tokenHandler.WriteToken(token)
        });
    }

    /// <summary>
    ///     Return information about the current user making the request.
    /// </summary>
    [HttpGet]
    [Route("me")]
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
    ///     Return the last three songs played by the user
    /// </summary>
    [HttpGet]
    [Route("lastPlayed")]
    public async Task<IActionResult> Last3PlayedSongsForUserAsync(short page, short pageSize, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        if (!TryValidatePaging(page, pageSize, out _, out _, out var pagingError))
        {
            return pagingError!;
        }

        short numberOfLastPlayedSongs = 3;
        var userLastPlayedResult = await userService.UserLastPlayedSongsAsync(user.Id, numberOfLastPlayedSongs, cancellationToken).ConfigureAwait(false);
        return Ok(new
        {
            meta = new PaginationMetadata(
                numberOfLastPlayedSongs,
                numberOfLastPlayedSongs,
                1,
                1
            ),
            data = userLastPlayedResult.Data.Where(x => x?.Song != null).Select(x => x!.Song.ToSongDataInfo()).ToArray()
        });
    }

    [HttpGet]
    [Route("playlists")]
    [RequireCapability(UserCapability.Playlist)]
    public async Task<IActionResult> UsersPlaylistsAsync(int? page, short? pageSize, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        var pageValue = page ?? 1;
        var pageSizeValue = pageSize ?? 50;
        if (!TryValidatePaging(pageValue, pageSizeValue, out var validatedPage, out var validatedPageSize, out var pagingError))
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
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Data = Melodee.Common.Data.Models;

namespace Melodee.Blazor.Controllers.Melodee;

/// <summary>
/// Authentication endpoints for login, logout, and token refresh.
/// </summary>
[ApiController]
[EnableRateLimiting("melodee-api")]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    IConfiguration configuration,
    IBlacklistService blacklistService,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    /// <summary>
    /// Authenticate a user and return a JWT token.
    /// </summary>
    [HttpPost]
    [Route("authenticate")]
    [AllowAnonymous]
    [EnableRateLimiting("melodee-auth")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AuthenticateAsync([FromBody] LoginModel model, CancellationToken cancellationToken = default)
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

        var (token, expiresAt) = GenerateJwtToken(authResult.Data);
        var melodeeConfig = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            user = authResult.Data.ToUserModel(GetBaseUrl(melodeeConfig)),
            serverVersion = melodeeConfig.ApiVersion(),
            token,
            expiresAt
        });
    }

    /// <summary>
    /// Refresh an existing JWT token. Requires a valid (but potentially expired) token.
    /// </summary>
    [HttpPost]
    [Route("refresh")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ServiceFilter(typeof(MelodeeApiAuthFilter))]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
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

        var (token, expiresAt) = GenerateJwtToken(user);
        var melodeeConfig = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            user = user.ToUserModel(GetBaseUrl(melodeeConfig)),
            serverVersion = melodeeConfig.ApiVersion(),
            token,
            expiresAt
        });
    }

    /// <summary>
    /// Logout the current user. This is a client-side operation - the token should be discarded.
    /// For server-side token invalidation, implement a token blacklist.
    /// </summary>
    [HttpPost]
    [Route("logout")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ServiceFilter(typeof(MelodeeApiAuthFilter))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        // JWT tokens are stateless - logout is handled client-side by discarding the token.
        // For enhanced security, implement a token blacklist in the future.
        return Ok(new { message = "Logged out successfully" });
    }

    private (string token, DateTime expiresAt) GenerateJwtToken(Data.User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = Configuration.GetValue<string>("Jwt:Key") ?? throw new InvalidOperationException("JWT signing key is not configured");
        var issuer = Configuration.GetValue<string>("Jwt:Issuer") ?? throw new InvalidOperationException("JWT issuer is not configured");
        var audience = Configuration.GetValue<string>("Jwt:Audience") ?? throw new InvalidOperationException("JWT audience is not configured");
        var key = Encoding.UTF8.GetBytes(jwtKey);
        var tokenHoursString = Configuration.GetSection("MelodeeAuthSettings:TokenHours").Value;
        var tokenHours = SafeParser.ToNumber<int>(tokenHoursString);
        var expiresAt = DateTime.UtcNow.AddHours(tokenHours);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Sid, user.ApiKey.ToString())
            ]),
            Expires = expiresAt,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiresAt);
    }
}

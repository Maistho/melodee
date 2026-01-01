using Melodee.Blazor.Controllers.Jellyfin.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Blazor.Controllers.Jellyfin;

[ApiController]
[Route("api/jf/[controller]")]
[ApiExplorerSettings(GroupName = "jellyfin")]
[EnableRateLimiting("jellyfin-api")]
public class UsersController(
    EtagRepository etagRepository,
    ISerializer serializer,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> dbContextFactory,
    IClock clock,
    UserService userService,
    ILogger<UsersController> logger) : JellyfinControllerBase(etagRepository, serializer, configuration, configurationFactory, dbContextFactory, clock)
{
    [HttpPost("AuthenticateByName")]
    [AllowAnonymous]
    [EnableRateLimiting("jellyfin-auth")]
    public async Task<IActionResult> AuthenticateByNameAsync(
        [FromBody] JellyfinAuthenticationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Pw))
        {
            logger.LogWarning("JellyfinAuthFailed UserName={UserName} RemoteIp={RemoteIp} Reason={Reason}",
                request.Username ?? "[empty]", GetClientBinding(), "Missing credentials");
            return JellyfinBadRequest("Username and password are required.");
        }

        var authenticateResult = await userService.LoginUserByUsernameAsync(request.Username, request.Pw, cancellationToken);
        if (!authenticateResult.IsSuccess || authenticateResult.Data == null)
        {
            logger.LogWarning("JellyfinAuthFailed UserName={UserName} RemoteIp={RemoteIp} Reason={Reason}",
                request.Username, GetClientBinding(), "Invalid credentials");
            return JellyfinUnauthorized("Invalid username or password.");
        }

        var user = authenticateResult.Data;
        if (user.IsLocked)
        {
            logger.LogWarning("JellyfinAuthFailed UserName={UserName} RemoteIp={RemoteIp} Reason={Reason}",
                request.Username, GetClientBinding(), "User locked");
            return JellyfinForbidden("User account is locked.");
        }

        var tokenInfo = JellyfinTokenParser.ParseFromRequest(Request);
        var token = JellyfinTokenParser.GenerateToken();
        var salt = JellyfinTokenParser.GenerateSalt();
        var pepper = await GetTokenPepperAsync(cancellationToken);
        var tokenHash = JellyfinTokenParser.HashToken(token, salt, pepper);

        var config = await GetConfigurationAsync(cancellationToken);
        var expiresHours = config.GetValue<int>(SettingRegistry.JellyfinTokenExpiresAfterHours);
        if (expiresHours <= 0) expiresHours = 168;

        var maxTokens = config.GetValue<int>(SettingRegistry.JellyfinTokenMaxActivePerUser);
        if (maxTokens <= 0) maxTokens = 10;

        var now = Clock.GetCurrentInstant();
        var expiresAt = now.Plus(Duration.FromHours(expiresHours));

        await using var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        var activeTokenCount = await dbContext.JellyfinAccessTokens
            .Where(t => t.UserId == user.Id && t.RevokedAt == null && (t.ExpiresAt == null || t.ExpiresAt > now))
            .CountAsync(cancellationToken);

        if (activeTokenCount >= maxTokens)
        {
            var oldestToken = await dbContext.JellyfinAccessTokens
                .Where(t => t.UserId == user.Id && t.RevokedAt == null)
                .OrderBy(t => t.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (oldestToken != null)
            {
                oldestToken.RevokedAt = now;
            }
        }

        var jellyfinToken = new JellyfinAccessToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            TokenSalt = salt,
            CreatedAt = now,
            ExpiresAt = expiresAt,
            Client = tokenInfo.Client?.Length > 255 ? tokenInfo.Client[..255] : tokenInfo.Client,
            Device = tokenInfo.Device?.Length > 255 ? tokenInfo.Device[..255] : tokenInfo.Device,
            DeviceId = tokenInfo.DeviceId?.Length > 255 ? tokenInfo.DeviceId[..255] : tokenInfo.DeviceId,
            Version = tokenInfo.Version?.Length > 255 ? tokenInfo.Version[..255] : tokenInfo.Version
        };

        dbContext.JellyfinAccessTokens.Add(jellyfinToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("JellyfinTokenIssued UserId={UserId} TokenId={TokenId} Client={Client} DeviceId={DeviceId}",
            user.Id, jellyfinToken.Id, tokenInfo.Client ?? "unknown", tokenInfo.DeviceId ?? "unknown");

        var sessionId = Guid.NewGuid().ToString("N");
        var result = new JellyfinAuthenticationResult
        {
            User = MapUserToJellyfin(user),
            AccessToken = token,
            ServerId = GetServerId(),
            SessionInfo = new JellyfinSessionInfo
            {
                Id = sessionId,
                UserId = ToJellyfinId(user.ApiKey),
                UserName = user.UserName,
                Client = tokenInfo.Client,
                DeviceName = tokenInfo.Device,
                DeviceId = tokenInfo.DeviceId,
                ApplicationVersion = tokenInfo.Version,
                RemoteEndPoint = GetClientBinding(),
                LastActivityDate = FormatInstantForJellyfin(now),
                IsActive = true,
                PlayableMediaTypes = ["Audio"],
                ServerId = GetServerId(),
                SupportedCommands = []
            }
        };

        return Ok(result);
    }

    [HttpGet("Me")]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            return JellyfinUnauthorized();
        }

        return Ok(MapUserToJellyfin(user));
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            return JellyfinUnauthorized();
        }

        if (!TryParseJellyfinGuid(userId, out var apiKey))
        {
            return JellyfinBadRequest("Invalid user ID format.");
        }

        if (user.ApiKey != apiKey && !user.IsAdmin)
        {
            return JellyfinForbidden("Cannot access other users.");
        }

        await using var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        var targetUser = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.ApiKey == apiKey, cancellationToken);

        if (targetUser == null)
        {
            return JellyfinNotFound("User not found.");
        }

        return Ok(MapUserToJellyfin(targetUser));
    }

    [HttpGet]
    public async Task<IActionResult> GetUsersAsync(CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            return JellyfinUnauthorized();
        }

        await using var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        var query = dbContext.Users.AsNoTracking();

        if (!user.IsAdmin)
        {
            query = query.Where(u => u.Id == user.Id);
        }

        var users = await query
            .OrderBy(u => u.UserName)
            .Take(100)
            .ToListAsync(cancellationToken);

        return Ok(users.Select(MapUserToJellyfin).ToArray());
    }
}

using System.Reflection;
using Melodee.Blazor.Controllers.Jellyfin.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Serialization;
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
public class SystemController(
    EtagRepository etagRepository,
    ISerializer serializer,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> dbContextFactory,
    IClock clock,
    ILogger<SystemController> logger) : JellyfinControllerBase(etagRepository, serializer, configuration, configurationFactory, dbContextFactory, clock)
{
    private static readonly string ServerVersion = Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0";

    [HttpGet("Info/Public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicSystemInfoAsync(CancellationToken cancellationToken)
    {
        var config = await GetConfigurationAsync(cancellationToken);
        var siteName = config.GetValue<string>(SettingRegistry.SystemSiteName);
        if (string.IsNullOrWhiteSpace(siteName))
        {
            siteName = "Melodee";
        }

        logger.LogDebug("Returning public system info for server {ServerName}", siteName);

        return Ok(new JellyfinPublicSystemInfo
        {
            ServerName = siteName,
            Version = ServerVersion,
            ProductName = "Melodee",
            Id = GetServerId(),
            StartupWizardCompleted = true,
            LocalAddress = $"{Request.Scheme}://{Request.Host}",
            OperatingSystem = Environment.OSVersion.Platform.ToString()
        });
    }

    [HttpGet("Ping")]
    [HttpPost("Ping")]
    [AllowAnonymous]
    public IActionResult Ping()
    {
        return NoContent();
    }

    [HttpGet("Info")]
    public async Task<IActionResult> GetSystemInfoAsync(CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            logger.LogDebug("Unauthenticated request for system info, returning minimal info");
        }

        var config = await GetConfigurationAsync(cancellationToken);
        var siteName = config.GetValue<string>(SettingRegistry.SystemSiteName);
        if (string.IsNullOrWhiteSpace(siteName))
        {
            siteName = "Melodee";
        }

        return Ok(new JellyfinSystemInfo
        {
            ServerName = siteName,
            Version = ServerVersion,
            ProductName = "Melodee",
            Id = GetServerId(),
            StartupWizardCompleted = true,
            LocalAddress = $"{Request.Scheme}://{Request.Host}",
            OperatingSystem = Environment.OSVersion.Platform.ToString(),
            OperatingSystemDisplayName = Environment.OSVersion.VersionString,
            HasPendingRestart = false,
            IsShuttingDown = false,
            SupportsLibraryMonitor = true,
            WebSocketPortNumber = Request.Host.Port ?? (Request.IsHttps ? 443 : 80),
            CanSelfRestart = false,
            CanLaunchWebBrowser = false,
            HasUpdateAvailable = false
        });
    }
}

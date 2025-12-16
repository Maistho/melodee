using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.Melodee;

/// <summary>
///     This controller is used to get meta-information about the API.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ServiceFilter(typeof(MelodeeApiAuthFilter))]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class SystemController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    StatisticsService statisticsService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    [HttpGet]
    [Route("info")]
    [AllowAnonymous]
    public async Task<IActionResult> GetServerInfo(CancellationToken cancellationToken = default)
    {
        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var versionInfo = configuration.ApiVersion();

        var versionParts = versionInfo.Split('.');
        if (versionParts.Length < 3)
        {
            return ApiBadRequest("Invalid version format");
        }

        var majorVersion = SafeParser.ToNumber<int?>(versionParts[0]) ?? 0;
        var minorVersion = SafeParser.ToNumber<int?>(versionParts[1]) ?? 0;
        var patchVersion = SafeParser.ToNumber<int?>(versionParts[2]) ?? 0;

        return Ok(new ServerInfo(configuration.GetValue<string>(SettingRegistry.OpenSubsonicServerType) ?? "Melodee",
            "Melodee API",
            majorVersion,
            minorVersion,
            patchVersion));
    }

    /// <summary>
    ///     Return some statistics about the system.
    /// </summary>
    [HttpGet]
    [Route("stats")]
    [RequireCapability(UserCapability.Admin)]
    public async Task<IActionResult> GetSystemStatsAsync(CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        var statsResult = await statisticsService.GetStatisticsAsync(cancellationToken).ConfigureAwait(false);

        return Ok(statsResult.Data.Where(x => x.IncludeInApiResult ?? false).Select(x => x.ToStatisticModel()).ToArray());
    }
}

using System.Diagnostics;
using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Controllers.Melodee.Models.ArtistLookup;
using Melodee.Blazor.Filters;
using Melodee.Blazor.Services;
using Melodee.Common.Configuration;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.SearchEngines;
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
[Route("api/v{version:apiVersion}/artist-lookup")]
public sealed class ArtistLookupController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    ArtistSearchEngineService artistSearchEngineService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory,
    ILogger<ArtistLookupController> logger)
    : ControllerBase(
        etagRepository,
        serializer,
        configuration,
        configurationFactory)
{
    private ILogger<ArtistLookupController> Logger { get; } = logger;
    [HttpPost]
    [ProducesResponseType(typeof(ArtistLookupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Lookup(
        [FromBody] ArtistLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.Validate(out var validationError))
        {
            return BadRequest(new ApiError(ApiError.Codes.ValidationError, validationError!, GetCorrelationId()));
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

        var stopwatch = Stopwatch.StartNew();

        var result = await artistSearchEngineService.LookupAsync(
            request.ArtistName.Trim(),
            request.Limit,
            request.ProviderIds,
            cancellationToken).ConfigureAwait(false);

        stopwatch.Stop();
        Logger.LogDebug("[ArtistLookupController] Lookup completed in {ElapsedMs}ms for [{ArtistName}]",
            stopwatch.ElapsedMilliseconds,
            request.ArtistName);

        var plugins = artistSearchEngineService.GetRegisteredPlugins();
        var providers = plugins.Select(p => new ProviderInfo
        {
            Id = p.Id,
            DisplayName = p.DisplayName,
            IsEnabled = p.IsEnabled
        }).ToArray();

        var candidates = result.Candidates.Select(c => new ArtistLookupCandidate
        {
            ProviderDisplayName = c.FromPlugin,
            ProviderId = plugins.FirstOrDefault(p => p.DisplayName == c.FromPlugin)?.Id,
            Name = c.Name,
            SortName = c.SortName,
            ImageUrl = c.ImageUrl,
            ThumbnailUrl = c.ThumbnailUrl,
            MusicBrainzId = c.MusicBrainzId,
            SpotifyId = c.SpotifyId,
            DiscogsId = c.DiscogsId,
            AmgId = c.AmgId,
            WikiDataId = c.WikiDataId,
            ItunesId = c.ItunesId,
            LastFmId = c.LastFmId
        }).ToArray();

        var response = new ArtistLookupResponse
        {
            Candidates = candidates,
            HasPartialFailures = result.HasPartialFailures,
            Providers = providers
        };

        return Ok(response);
    }

    [HttpGet]
    [Route("providers")]
    [ProducesResponseType(typeof(ProviderInfo[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProviders(CancellationToken cancellationToken = default)
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

        var plugins = artistSearchEngineService.GetRegisteredPlugins();
        var providers = plugins.Select(p => new ProviderInfo
        {
            Id = p.Id,
            DisplayName = p.DisplayName,
            IsEnabled = p.IsEnabled
        }).ToArray();

        return Ok(providers);
    }
}

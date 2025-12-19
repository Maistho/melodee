using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Melodee.Blazor.Controllers.Melodee;

/// <summary>
/// Playback settings endpoints.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ServiceFilter(typeof(MelodeeApiAuthFilter))]
[EnableRateLimiting("melodee-api")]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/playback/settings")]
public class PlaybackSettingsController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    PlaybackSettingsService playbackSettingsService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    /// <summary>
    /// Get user playback preferences.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PlaybackSettings), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        var result = await playbackSettingsService.GetByUserIdAsync(user.Id, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data == null)
        {
            return Ok(new PlaybackSettings(
                0,
                true,
                false,
                "none",
                "high",
                null,
                null));
        }

        return Ok(new PlaybackSettings(
            result.Data.CrossfadeDuration,
            result.Data.GaplessPlayback,
            result.Data.VolumeNormalization,
            result.Data.ReplayGain,
            result.Data.AudioQuality,
            result.Data.EqualizerPreset,
            result.Data.LastUsedDevice));
    }

    /// <summary>
    /// Save user playback preferences. Omitted fields are left unchanged.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateSettingsAsync([FromBody] UpdatePlaybackSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(userService, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return ApiUnauthorized();
        }

        var result = await playbackSettingsService.UpdateAsync(
            user.Id,
            request.CrossfadeDuration,
            request.GaplessPlayback,
            request.VolumeNormalization,
            request.ReplayGain,
            request.AudioQuality,
            request.EqualizerPreset,
            cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            return ApiValidationError(result.Messages?.FirstOrDefault() ?? "Validation failed");
        }

        return Ok(new { success = true });
    }
}

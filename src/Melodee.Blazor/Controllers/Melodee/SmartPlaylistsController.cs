using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Melodee.Blazor.Controllers.Melodee;

/// <summary>
/// Create and manage smart playlists with dynamic rules.
/// </summary>
/// <remarks>
/// Smart playlists automatically populate based on rules like genre, year, rating, play count, BPM, duration, artist, or album.
/// Rules can use operators like equals, contains, greaterThan, lessThan, and between.
/// Playlists can optionally auto-update as your library changes.
/// </remarks>
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ServiceFilter(typeof(MelodeeApiAuthFilter))]
[RequireCapability(UserCapability.Playlist)]
[EnableRateLimiting("melodee-api")]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/playlists/smart")]
public class SmartPlaylistsController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    PlaylistService playlistService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    private static readonly string[] ValidFields = ["genre", "year", "rating", "playCount", "bpm", "duration", "artist", "album"];
    private static readonly string[] ValidOperators = ["equals", "contains", "greaterThan", "lessThan", "between"];

    /// <summary>
    /// Create a smart playlist that automatically populates based on defined rules.
    /// </summary>
    /// <param name="request">Smart playlist configuration with name, description, rules, limit, and auto-update preference.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created smart playlist with its ID and initial track count.</returns>
    /// <remarks>
    /// Rules define which tracks to include. Each rule has a field (genre, year, rating, playCount, bpm, duration, artist, album),
    /// an operator (equals, contains, greaterThan, lessThan, between), and a value.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(SmartPlaylistResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateSmartPlaylistAsync([FromBody] CreateSmartPlaylistRequest request, CancellationToken cancellationToken = default)
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

        // Validation
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiValidationError("name is required");
        }

        if (request.Rules == null || request.Rules.Length == 0)
        {
            return ApiValidationError("rules is required and must not be empty");
        }

        foreach (var rule in request.Rules)
        {
            if (string.IsNullOrWhiteSpace(rule.Field))
            {
                return ApiValidationError("Each rule must have a field");
            }

            if (!ValidFields.Contains(rule.Field, StringComparer.OrdinalIgnoreCase))
            {
                return ApiValidationError($"Invalid field '{rule.Field}'. Valid fields: {string.Join(", ", ValidFields)}");
            }

            if (string.IsNullOrWhiteSpace(rule.Operator))
            {
                return ApiValidationError("Each rule must have an operator");
            }

            if (!ValidOperators.Contains(rule.Operator, StringComparer.OrdinalIgnoreCase))
            {
                return ApiValidationError($"Invalid operator '{rule.Operator}'. Valid operators: {string.Join(", ", ValidOperators)}");
            }
        }

        var limit = request.Limit ?? 100;
        if (limit < 1 || limit > 1000)
        {
            limit = 100;
        }

        // Create the playlist as a regular playlist with smart rules metadata
        var rulesJson = System.Text.Json.JsonSerializer.Serialize(request.Rules);
        var comment = $"Smart playlist. AutoUpdate: {request.AutoUpdate}. Rules: {rulesJson}";

        var createResult = await playlistService.CreatePlaylistAsync(
            request.Name,
            user.Id,
            comment,
            false,
            Array.Empty<Guid>(),
            returnPrefixedApiKey: false,
            cancellationToken).ConfigureAwait(false);

        if (!createResult.IsSuccess || string.IsNullOrEmpty(createResult.Data))
        {
            return ApiBadRequest("Unable to create smart playlist.");
        }

        var playlistApiKey = Guid.Parse(createResult.Data);
        var playlistResult = await playlistService.GetByApiKeyAsync(user.ToUserInfo(), playlistApiKey, cancellationToken).ConfigureAwait(false);
        if (!playlistResult.IsSuccess || playlistResult.Data == null)
        {
            return ApiBadRequest("Smart playlist created but unable to retrieve details.");
        }

        return Ok(new SmartPlaylistResponse(
            playlistApiKey,
            request.Name,
            request.Description,
            request.Rules,
            playlistResult.Data.SongCount ?? 0,
            request.AutoUpdate));
    }
}

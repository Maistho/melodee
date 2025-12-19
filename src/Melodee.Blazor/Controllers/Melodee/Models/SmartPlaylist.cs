using System.Text.Json.Serialization;

namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Smart playlist rule.
/// </summary>
public record SmartPlaylistRule(
    string Field,
    string Operator,
    [property: JsonPropertyName("value")] object? Value);

/// <summary>
/// Request to create a smart playlist.
/// </summary>
public record CreateSmartPlaylistRequest(
    string Name,
    string? Description,
    SmartPlaylistRule[] Rules,
    int? Limit,
    bool AutoUpdate);

/// <summary>
/// Response for smart playlist creation.
/// </summary>
public record SmartPlaylistResponse(
    Guid Id,
    string Name,
    string? Description,
    SmartPlaylistRule[] Rules,
    int TrackCount,
    bool AutoUpdate);

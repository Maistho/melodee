namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Request model for updating an existing playlist.
/// </summary>
public record UpdatePlaylistRequest(
    string? Name,
    string? Comment,
    bool? IsPublic);

namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Request model for creating a new playlist.
/// </summary>
public record CreatePlaylistRequest(
    string Name,
    string? Comment,
    bool IsPublic = false,
    Guid[]? SongIds = null);

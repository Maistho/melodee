using Melodee.Common.Enums;

namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Request model for creating a new share.
/// </summary>
/// <param name="ShareType">The type of resource to share (Song, Album, Playlist, Artist).</param>
/// <param name="ResourceId">The API key of the resource to share.</param>
/// <param name="Description">Optional description for the share.</param>
/// <param name="IsDownloadable">Whether the shared content can be downloaded.</param>
/// <param name="ExpiresAt">Optional expiration timestamp in ISO 8601 format.</param>
public record CreateShareRequest(
    ShareType ShareType,
    Guid ResourceId,
    string? Description = null,
    bool IsDownloadable = false,
    string? ExpiresAt = null);

namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Represents a share that can be used to share content publicly.
/// </summary>
/// <param name="Id">The unique API key for the share.</param>
/// <param name="ShareUrl">The public URL for accessing the shared content.</param>
/// <param name="ShareType">The type of content being shared (Song, Album, Playlist, Artist).</param>
/// <param name="ResourceId">The API key of the shared resource.</param>
/// <param name="ResourceName">The name of the shared resource.</param>
/// <param name="ResourceThumbnailUrl">The thumbnail URL of the shared resource.</param>
/// <param name="ResourceImageUrl">The image URL of the shared resource.</param>
/// <param name="Description">Optional description for the share.</param>
/// <param name="IsDownloadable">Whether the shared content can be downloaded.</param>
/// <param name="VisitCount">Number of times the share has been accessed.</param>
/// <param name="Owner">The user who created the share.</param>
/// <param name="CreatedAt">When the share was created.</param>
/// <param name="ExpiresAt">When the share expires (null if no expiration).</param>
/// <param name="LastVisitedAt">When the share was last accessed.</param>
public record Share(
    Guid Id,
    string ShareUrl,
    string ShareType,
    Guid ResourceId,
    string ResourceName,
    string ResourceThumbnailUrl,
    string ResourceImageUrl,
    string? Description,
    bool IsDownloadable,
    int VisitCount,
    User Owner,
    string CreatedAt,
    string? ExpiresAt,
    string? LastVisitedAt);

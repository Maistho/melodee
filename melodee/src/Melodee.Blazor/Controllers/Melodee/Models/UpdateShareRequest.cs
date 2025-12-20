namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Request model for updating an existing share.
/// </summary>
/// <param name="Description">Optional new description for the share.</param>
/// <param name="IsDownloadable">Whether the shared content can be downloaded.</param>
/// <param name="ExpiresAt">Optional new expiration timestamp in ISO 8601 format (null to remove expiration).</param>
public record UpdateShareRequest(
    string? Description = null,
    bool? IsDownloadable = null,
    string? ExpiresAt = null);

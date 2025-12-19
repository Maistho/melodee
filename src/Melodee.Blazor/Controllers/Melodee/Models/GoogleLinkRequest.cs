using System.ComponentModel.DataAnnotations;

namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Request model for linking a Google account to an existing user.
/// </summary>
public record GoogleLinkRequest
{
    /// <summary>
    /// The Google ID token to link.
    /// </summary>
    [Required]
    public required string IdToken { get; init; }
}

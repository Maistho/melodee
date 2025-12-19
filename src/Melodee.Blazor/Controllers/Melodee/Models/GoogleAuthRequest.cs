using System.ComponentModel.DataAnnotations;

namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Request model for Google authentication exchange.
/// </summary>
public record GoogleAuthRequest
{
    /// <summary>
    /// The Google ID token from the client.
    /// </summary>
    [Required]
    public required string IdToken { get; init; }

    /// <summary>
    /// Optional device identifier for token tracking.
    /// </summary>
    public string? DeviceId { get; init; }
}

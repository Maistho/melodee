using System.ComponentModel.DataAnnotations;

namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Request model for refresh token rotation.
/// </summary>
public record RefreshTokenRequest
{
    /// <summary>
    /// The refresh token to exchange for a new token pair.
    /// </summary>
    [Required]
    public required string RefreshToken { get; init; }

    /// <summary>
    /// Optional device identifier for token tracking.
    /// </summary>
    public string? DeviceId { get; init; }
}

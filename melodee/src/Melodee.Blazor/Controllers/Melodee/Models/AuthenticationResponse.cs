namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Response model for authentication operations.
/// </summary>
public record AuthenticationResponse
{
    /// <summary>
    /// The authenticated user information.
    /// </summary>
    public required User User { get; init; }

    /// <summary>
    /// Server version information.
    /// </summary>
    public required string ServerVersion { get; init; }

    /// <summary>
    /// The JWT access token.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// When the access token expires.
    /// </summary>
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// The refresh token for obtaining new access tokens.
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// When the refresh token expires.
    /// </summary>
    public DateTime? RefreshTokenExpiresAt { get; init; }
}

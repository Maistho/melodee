namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Admin-visible information about a refresh token.
/// </summary>
public record AdminRefreshTokenInfo
{
    /// <summary>
    /// Token family identifier (session ID).
    /// </summary>
    public required string TokenFamily { get; init; }

    /// <summary>
    /// User ID this token belongs to.
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// Username of the token owner.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// When the token was issued.
    /// </summary>
    public DateTime IssuedAt { get; init; }

    /// <summary>
    /// When the token expires.
    /// </summary>
    public DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Whether the token has been revoked.
    /// </summary>
    public bool IsRevoked { get; init; }

    /// <summary>
    /// Reason for revocation, if revoked.
    /// </summary>
    public string? RevokedReason { get; init; }

    /// <summary>
    /// Device identifier, if available.
    /// </summary>
    public string? DeviceId { get; init; }

    /// <summary>
    /// IP address from which the token was issued.
    /// </summary>
    public string? IpAddress { get; init; }
}

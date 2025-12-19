namespace Melodee.Common.Configuration;

/// <summary>
/// Configuration options for token lifetimes and refresh token policies.
/// </summary>
public class TokenOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Auth:Tokens";

    /// <summary>
    /// Access token (JWT) lifetime in minutes. Default is 15 minutes.
    /// </summary>
    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token lifetime in days. Default is 30 days.
    /// </summary>
    public int RefreshTokenLifetimeDays { get; set; } = 30;

    /// <summary>
    /// Absolute maximum session lifetime in days per device/session. Default is 90 days.
    /// After this time, the user must re-authenticate even with a valid refresh token.
    /// </summary>
    public int MaxSessionDays { get; set; } = 90;

    /// <summary>
    /// Whether to rotate refresh tokens on each use. Default is true (recommended for security).
    /// </summary>
    public bool RotateRefreshTokens { get; set; } = true;

    /// <summary>
    /// Whether to revoke the entire token family when a refresh token reuse is detected.
    /// Default is true (recommended for security).
    /// </summary>
    public bool RevokeOnReplay { get; set; } = true;
}

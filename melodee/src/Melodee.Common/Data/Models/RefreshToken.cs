using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

/// <summary>
/// Represents a refresh token for JWT authentication with rotation support.
/// Tokens are stored hashed - never store the raw token value.
/// </summary>
[Serializable]
[Index(nameof(HashedToken), IsUnique = true)]
[Index(nameof(UserId))]
[Index(nameof(TokenFamily))]
[Index(nameof(ExpiresAt))]
public class RefreshToken : DataModelBase
{
    /// <summary>
    /// The user this refresh token belongs to.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// SHA-256 hash of the refresh token. Never store raw tokens.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.HashOrGuidLength)]
    [Required]
    public required string HashedToken { get; set; }

    /// <summary>
    /// Token family identifier for rotation tracking.
    /// All tokens in a rotation chain share the same family ID.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.HashOrGuidLength)]
    [Required]
    public required string TokenFamily { get; set; }

    /// <summary>
    /// When this token was issued.
    /// </summary>
    public Instant IssuedAt { get; set; }

    /// <summary>
    /// When this token expires.
    /// </summary>
    public Instant ExpiresAt { get; set; }

    /// <summary>
    /// When this token was revoked (null if still valid).
    /// </summary>
    public Instant? RevokedAt { get; set; }

    /// <summary>
    /// Reason for revocation (e.g., "rotated", "logout", "admin_revoked", "replay_detected").
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? RevokedReason { get; set; }

    /// <summary>
    /// The hashed token that replaced this one during rotation (null if not rotated).
    /// </summary>
    [MaxLength(MaxLengthDefinitions.HashOrGuidLength)]
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// When the session (token family) was originally created.
    /// Used to enforce absolute max session lifetime.
    /// </summary>
    public Instant SessionStartedAt { get; set; }

    /// <summary>
    /// Device/client identifier for the session.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? DeviceId { get; set; }

    /// <summary>
    /// User agent string from the client.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// IP address from which the token was issued.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Whether this token is currently valid (not expired and not revoked).
    /// </summary>
    public bool IsValid(Instant now) => RevokedAt == null && ExpiresAt > now;

    /// <summary>
    /// Whether this token has been revoked.
    /// </summary>
    public bool IsRevoked => RevokedAt != null;
}

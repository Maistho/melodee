using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(UserId))]
[Index(nameof(TokenHash), IsUnique = true)]
[Index(nameof(TokenPrefixHash))]
[Index(nameof(UserId), nameof(ExpiresAt), nameof(RevokedAt))]
public class JellyfinAccessToken
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    [Required]
    [MaxLength(MaxLengthDefinitions.HashOrGuidLength)]
    public required string TokenHash { get; set; }

    /// <summary>
    /// First 8 characters of the raw token, used for fast prefix-based lookup
    /// before performing full HMAC verification.
    /// </summary>
    [Required]
    [MaxLength(8)]
    public required string TokenPrefixHash { get; set; }

    [Required]
    [MaxLength(32)]
    public required string TokenSalt { get; set; }

    [Required]
    public required Instant CreatedAt { get; set; }

    public Instant? LastUsedAt { get; set; }

    public Instant? ExpiresAt { get; set; }

    public Instant? RevokedAt { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? Client { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? Device { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? DeviceId { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? Version { get; set; }

    public bool IsValid(Instant currentTime) =>
        RevokedAt == null && (ExpiresAt == null || ExpiresAt > currentTime);
}

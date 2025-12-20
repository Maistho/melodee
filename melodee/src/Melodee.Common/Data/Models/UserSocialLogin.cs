using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

/// <summary>
/// Represents a social login provider linked to a user account.
/// </summary>
[Serializable]
[Index(nameof(Provider), nameof(Subject), IsUnique = true)]
[Index(nameof(UserId))]
public class UserSocialLogin : DataModelBase
{
    /// <summary>
    /// The user this social login is linked to.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// The social login provider (e.g., "Google", "Microsoft", "GitHub").
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Provider { get; set; }

    /// <summary>
    /// The unique subject identifier from the provider (e.g., Google's 'sub' claim).
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Subject { get; set; }

    /// <summary>
    /// The email associated with the social login (may differ from User.Email).
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    /// <summary>
    /// The display name from the social provider.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// The hosted domain (hd claim) for Google Workspace accounts.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? HostedDomain { get; set; }

    /// <summary>
    /// When the user last logged in using this social provider.
    /// </summary>
    public Instant? LastLoginAt { get; set; }
}

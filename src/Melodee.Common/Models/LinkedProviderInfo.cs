namespace Melodee.Common.Models;

/// <summary>
/// Information about a linked social login provider.
/// </summary>
public record LinkedProviderInfo
{
    /// <summary>
    /// The provider name (e.g., "Google").
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// The email associated with this provider.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// When the provider was linked.
    /// </summary>
    public DateTime? LinkedAt { get; init; }

    /// <summary>
    /// When the user last logged in via this provider.
    /// </summary>
    public DateTime? LastLoginAt { get; init; }
}

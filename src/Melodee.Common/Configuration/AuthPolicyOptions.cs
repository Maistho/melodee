namespace Melodee.Common.Configuration;

/// <summary>
/// Configuration options for authentication policies.
/// </summary>
public class AuthPolicyOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Auth";

    /// <summary>
    /// Whether self-registration is enabled (new accounts can be created on first Google sign-in).
    /// Default is true for consumer-style deployments.
    /// </summary>
    public bool SelfRegistrationEnabled { get; set; } = true;
}

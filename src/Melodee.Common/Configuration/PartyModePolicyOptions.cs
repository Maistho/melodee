namespace Melodee.Common.Configuration;

/// <summary>
/// Configuration options for Party Mode authorization policies.
/// </summary>
public class PartyModePolicyOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "PartyMode:Policy";

    /// <summary>
    /// Policy name for requiring Party Mode to be enabled.
    /// </summary>
    public const string PartyModeEnabled = "PartyModeEnabled";

    /// <summary>
    /// Policy name for requiring session membership.
    /// </summary>
    public const string PartySessionMember = "PartySessionMember";

    /// <summary>
    /// Policy name for requiring Owner or DJ role.
    /// </summary>
    public const string PartySessionController = "PartySessionController";

    /// <summary>
    /// Policy name for requiring Owner role.
    /// </summary>
    public const string PartySessionOwner = "PartySessionOwner";
}

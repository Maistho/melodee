using Melodee.Mql.Security;

namespace Melodee.Mql;

/// <summary>
/// Configuration options for MQL processing.
/// </summary>
public sealed class MqlOptions
{
    public bool EnableRegex { get; set; } = false;

    public int MaxResultSetForRegex { get; set; } = Constants.MqlConstants.MaxResultSetForRegex;

    public int RegexTimeoutMs { get; set; } = Constants.MqlConstants.RegexTimeoutMs;

    public IMqlRegexGuard RegexGuard { get; set; } = new MqlRegexGuard();
}

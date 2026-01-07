namespace Melodee.Mql.Constants;

/// <summary>
/// Constants defining limits and thresholds for MQL processing.
/// </summary>
public static class MqlConstants
{
    public const int MaxQueryLength = 500;
    public const int MaxFieldCount = 20;
    public const int MaxRecursionDepth = 10;
    public const int MaxRegexPatternLength = 100;
    public const int MaxResultSetForRegex = 1000;
    public const int RegexTimeoutMs = 500;
    public const int ParseRateLimitPerMinute = 10;
    public const int ParseTimeoutMs = 200;
    public const int DefaultTopLimit = 100;
    public const int MaxTopLimit = 1000;
}

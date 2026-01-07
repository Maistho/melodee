namespace Melodee.Mql.Security;

/// <summary>
/// Interface for regex pattern validation with ReDoS protection.
/// </summary>
public interface IMqlRegexGuard
{
    /// <summary>
    /// Validates a regex pattern for safety.
    /// </summary>
    RegexValidationResult ValidatePattern(string pattern);

    /// <summary>
    /// Safely evaluates a regex pattern against a test string.
    /// </summary>
    RegexValidationResult SafeMatch(string pattern, string testString, TimeSpan? timeout = null);
}

/// <summary>
/// Represents the result of a regex validation check.
/// </summary>
public record RegexValidationResult
{
    public bool IsValid { get; init; }
    public bool IsBlocked { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public string? SafePattern { get; init; }
    public long EvaluationTimeMs { get; init; }
}

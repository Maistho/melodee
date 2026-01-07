namespace Melodee.Mql.Models;

/// <summary>
/// Represents the result of validating an MQL query.
/// </summary>
/// <param name="IsValid">Whether the query is valid.</param>
/// <param name="Errors">List of validation errors.</param>
/// <param name="Warnings">List of warnings (non-blocking issues).</param>
/// <param name="ComplexityScore">The calculated complexity score of the query.</param>
public record MqlValidationResult(
    bool IsValid,
    List<MqlError> Errors,
    List<string> Warnings,
    int ComplexityScore)
{
    public static MqlValidationResult Valid(List<string>? warnings = null, int complexityScore = 0) => new(
        true,
        new List<MqlError>(),
        warnings ?? new List<string>(),
        complexityScore);

    public static MqlValidationResult Invalid(List<MqlError> errors) => new(
        false,
        errors,
        new List<string>(),
        0);
}

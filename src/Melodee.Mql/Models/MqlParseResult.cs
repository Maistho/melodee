namespace Melodee.Mql.Models;

/// <summary>
/// Represents the result of parsing an MQL query.
/// </summary>
/// <param name="IsValid">Whether parsing was successful.</param>
/// <param name="Ast">The resulting AST (null if parsing failed).</param>
/// <param name="NormalizedQuery">The normalized form of the query.</param>
/// <param name="Errors">List of parse errors.</param>
/// <param name="Warnings">List of warnings.</param>
public record MqlParseResult(
    bool IsValid,
    MqlAstNode? Ast,
    string NormalizedQuery,
    List<MqlError> Errors,
    List<string> Warnings)
{
    public static MqlParseResult Success(MqlAstNode ast, string normalizedQuery, List<string>? warnings = null) => new(
        true,
        ast,
        normalizedQuery,
        new List<MqlError>(),
        warnings ?? new List<string>());

    public static MqlParseResult Failed(List<MqlError> errors, string normalizedQuery) => new(
        false,
        null,
        normalizedQuery,
        errors,
        new List<string>());
}

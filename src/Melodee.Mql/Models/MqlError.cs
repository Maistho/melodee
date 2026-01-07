namespace Melodee.Mql.Models;

/// <summary>
/// Represents an error in MQL processing.
/// </summary>
/// <param name="ErrorCode">The error code identifying the type of error.</param>
/// <param name="Message">User-friendly error message.</param>
/// <param name="Position">Optional position information for the error.</param>
/// <param name="Suggestions">Optional suggestions for fixing the error.</param>
public record MqlError(
    string ErrorCode,
    string Message,
    MqlErrorPosition? Position,
    string[]? Suggestions = null)
{
}

/// <summary>
/// Represents the position of an error in the query string.
/// </summary>
/// <param name="Start">The zero-based start position.</param>
/// <param name="End">The zero-based end position.</param>
/// <param name="Line">The line number (1-based).</param>
/// <param name="Column">The column number (1-based).</param>
public record MqlErrorPosition(
    int Start,
    int End,
    int Line,
    int Column);

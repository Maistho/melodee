namespace Melodee.Mql.Models;

/// <summary>
/// Represents a token in the MQL language with position tracking for error reporting.
/// </summary>
/// <param name="Type">The type of token.</param>
/// <param name="Value">The raw value of the token.</param>
/// <param name="StartPosition">The zero-based start position in the input string.</param>
/// <param name="EndPosition">The zero-based end position in the input string.</param>
/// <param name="Line">The line number (1-based).</param>
/// <param name="Column">The column number (1-based).</param>
public record MqlToken(
    MqlTokenType Type,
    string Value,
    int StartPosition,
    int EndPosition,
    int Line,
    int Column)
{
    /// <summary>
    /// Gets the length of the token value.
    /// </summary>
    public int Length => EndPosition - StartPosition;

    /// <summary>
    /// Creates an EndOfInput token at the end of the input.
    /// </summary>
    public static MqlToken EndOfInput(int line, int column) => new(
        MqlTokenType.EndOfInput,
        string.Empty,
        line,
        column,
        line,
        column);
}

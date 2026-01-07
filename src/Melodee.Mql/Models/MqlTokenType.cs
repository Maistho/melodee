namespace Melodee.Mql.Models;

/// <summary>
/// Represents the type of a token in the MQL language.
/// </summary>
public enum MqlTokenType
{
    FreeText,
    FieldName,
    Colon,
    Operator,
    StringLiteral,
    NumberLiteral,
    DateLiteral,
    BooleanLiteral,
    And,
    Or,
    Not,
    LeftParen,
    RightParen,
    Range,
    Regex,
    Wildcard,
    EndOfInput,
    Unknown
}

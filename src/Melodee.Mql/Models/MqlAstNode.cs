namespace Melodee.Mql.Models;

/// <summary>
/// Base class for all AST nodes in MQL.
/// </summary>
public abstract record MqlAstNode;

/// <summary>
/// Represents a free-text search term (unqualified search).
/// </summary>
/// <param name="Text">The search text.</param>
/// <param name="Token">The original token.</param>
public record FreeTextNode(
    string Text,
    MqlToken Token) : MqlAstNode;

/// <summary>
/// Represents a field expression: field:operator:value.
/// </summary>
/// <param name="Field">The field name.</param>
/// <param name="Operator">The operator (Equals, Contains, etc.).</param>
/// <param name="Value">The value to compare against.</param>
/// <param name="Token">The original token.</param>
public record FieldExpressionNode(
    string Field,
    string Operator,
    object Value,
    MqlToken Token) : MqlAstNode;

/// <summary>
/// Represents a binary expression (AND/OR).
/// </summary>
/// <param name="Operator">The binary operator.</param>
/// <param name="Left">The left operand.</param>
/// <param name="Right">The right operand.</param>
public record BinaryExpressionNode(
    string Operator,
    MqlAstNode Left,
    MqlAstNode Right) : MqlAstNode;

/// <summary>
/// Represents a unary expression (NOT).
/// </summary>
/// <param name="Operator">The unary operator.</param>
/// <param name="Operand">The operand.</param>
public record UnaryExpressionNode(
    string Operator,
    MqlAstNode Operand) : MqlAstNode;

/// <summary>
/// Represents a grouped expression (parenthesized).
/// </summary>
/// <param name="Inner">The inner expression.</param>
public record GroupNode(
    MqlAstNode Inner) : MqlAstNode;

/// <summary>
/// Represents a range expression: field:min-max.
/// </summary>
/// <param name="Field">The field name.</param>
/// <param name="Min">The minimum value.</param>
/// <param name="Max">The maximum value.</param>
/// <param name="Token">The original token.</param>
public record RangeNode(
    string Field,
    object Min,
    object Max,
    MqlToken Token) : MqlAstNode;

/// <summary>
/// Represents a regex expression: field:/pattern/flags.
/// </summary>
/// <param name="Field">The field name.</param>
/// <param name="Pattern">The regex pattern (without delimiters).</param>
/// <param name="Flags">Regex flags (i, g, ig).</param>
/// <param name="Token">The original token.</param>
public record RegexExpressionNode(
    string Field,
    string Pattern,
    string Flags,
    MqlToken Token) : MqlAstNode;

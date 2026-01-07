using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Melodee.Mql.Api.Dto;

/// <summary>
/// Request DTO for MQL parse endpoint.
/// </summary>
public sealed record MqlParseRequest
{
    /// <summary>
    /// The entity type to search (songs, albums, or artists).
    /// </summary>
    /// <example>songs</example>
    [Required]
    [JsonPropertyName("entity")]
    public string Entity { get; init; } = string.Empty;

    /// <summary>
    /// The MQL query string to parse.
    /// </summary>
    /// <example>artist:Beatles AND year:>=1970</example>
    [Required]
    [JsonPropertyName("query")]
    public string Query { get; init; } = string.Empty;
}

/// <summary>
/// Response DTO for successful MQL parse.
/// </summary>
public sealed record MqlParseResponse
{
    /// <summary>
    /// Normalized query with consistent formatting.
    /// </summary>
    /// <example>artist:Beatles AND year:>=1970</example>
    [JsonPropertyName("normalizedQuery")]
    public string NormalizedQuery { get; init; } = string.Empty;

    /// <summary>
    /// Abstract Syntax Tree representation of the query.
    /// </summary>
    [JsonPropertyName("ast")]
    public MqlAstDto? Ast { get; init; }

    /// <summary>
    /// Any warnings generated during parsing.
    /// </summary>
    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// Estimated query complexity score (higher = more complex).
    /// </summary>
    /// <example>5</example>
    [JsonPropertyName("estimatedComplexity")]
    public int EstimatedComplexity { get; init; }

    /// <summary>
    /// Whether the query is valid.
    /// </summary>
    /// <example>true</example>
    [JsonPropertyName("valid")]
    public bool Valid { get; init; }

    /// <summary>
    /// Processing time in milliseconds.
    /// </summary>
    /// <example>15</example>
    [JsonPropertyName("processingTimeMs")]
    public long ProcessingTimeMs { get; init; }
}

/// <summary>
/// Request DTO for autocomplete suggestions.
/// </summary>
public sealed record MqlSuggestionRequestDto
{
    /// <summary>
    /// The entity type for suggestions.
    /// </summary>
    /// <example>songs</example>
    [Required]
    [JsonPropertyName("entity")]
    public string Entity { get; init; } = string.Empty;

    /// <summary>
    /// The current query text.
    /// </summary>
    /// <example>artist:Beat</example>
    [JsonPropertyName("query")]
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// Cursor position in the query string.
    /// </summary>
    /// <example>12</example>
    [JsonPropertyName("cursorPosition")]
    public int CursorPosition { get; init; }
}

/// <summary>
/// Response DTO for autocomplete suggestions.
/// </summary>
public sealed record MqlSuggestionResponseDto
{
    /// <summary>
    /// List of suggestions.
    /// </summary>
    [JsonPropertyName("suggestions")]
    public List<MqlSuggestionDto> Suggestions { get; init; } = new();

    /// <summary>
    /// The original query.
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// Cursor position in the query.
    /// </summary>
    [JsonPropertyName("cursorPosition")]
    public int CursorPosition { get; init; }

    /// <summary>
    /// Detected context for suggestions (e.g., "inFieldName", "afterColon").
    /// </summary>
    /// <example>inFieldName</example>
    [JsonPropertyName("detectedContext")]
    public string DetectedContext { get; init; } = string.Empty;

    /// <summary>
    /// Processing time in milliseconds.
    /// </summary>
    [JsonPropertyName("processingTimeMs")]
    public long ProcessingTimeMs { get; init; }
}

/// <summary>
/// Error response DTO for MQL parse failures.
/// </summary>
public sealed record MqlErrorResponse
{
    /// <summary>
    /// Error code identifier.
    /// </summary>
    /// <example>MQL_UNKNOWN_FIELD</example>
    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    /// <example>Unknown field 'artust'</example>
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Position information for the error.
    /// </summary>
    [JsonPropertyName("position")]
    public MqlPositionDto? Position { get; init; }

    /// <summary>
    /// Suggestions to fix the error.
    /// </summary>
    [JsonPropertyName("suggestions")]
    public List<MqlSuggestionDto> Suggestions { get; init; } = new();

    /// <summary>
    /// Timestamp of the error.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Position information for error locations.
/// </summary>
public sealed record MqlPositionDto
{
    /// <summary>
    /// Character position in the query string.
    /// </summary>
    /// <example>5</example>
    [JsonPropertyName("position")]
    public int Position { get; init; }

    /// <summary>
    /// Line number (1-based).
    /// </summary>
    /// <example>1</example>
    [JsonPropertyName("line")]
    public int Line { get; init; }

    /// <summary>
    /// Column number (1-based).
    /// </summary>
    /// <example>6</example>
    [JsonPropertyName("column")]
    public int Column { get; init; }

    /// <summary>
    /// Length of the problematic token.
    /// </summary>
    /// <example>5</example>
    [JsonPropertyName("length")]
    public int Length { get; init; }
}

/// <summary>
/// Suggestion for fixing an error or completing input.
/// </summary>
public sealed record MqlSuggestionDto
{
    /// <summary>
    /// The suggestion text to insert.
    /// </summary>
    /// <example>artist:</example>
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Type of suggestion (field, operator, value, keyword).
    /// </summary>
    /// <example>field</example>
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Description of the suggestion.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Offset for inserting the suggestion.
    /// </summary>
    [JsonPropertyName("offset")]
    public int? Offset { get; init; }

    /// <summary>
    /// Confidence score (0-1) for the suggestion.
    /// </summary>
    /// <example>0.95</example>
    [JsonPropertyName("confidence")]
    public double Confidence { get; init; }
}

/// <summary>
/// Serialized AST node representation.
/// </summary>
public abstract class MqlAstDto
{
    /// <summary>
    /// Type of AST node (e.g., "FreeText", "FieldExpression", "BinaryExpression").
    /// </summary>
    [JsonPropertyName("nodeType")]
    public string NodeType { get; init; } = string.Empty;
}

/// <summary>
/// Free text search node.
/// </summary>
public sealed class MqlFreeTextDto : MqlAstDto
{
    /// <summary>
    /// The free text content.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}

/// <summary>
/// Field expression node.
/// </summary>
public sealed class MqlFieldExpressionDto : MqlAstDto
{
    /// <summary>
    /// Field name.
    /// </summary>
    [JsonPropertyName("field")]
    public string Field { get; init; } = string.Empty;

    /// <summary>
    /// Operator (e.g., ":=", ":>", "contains").
    /// </summary>
    [JsonPropertyName("operator")]
    public string Operator { get; init; } = string.Empty;

    /// <summary>
    /// Value to compare against.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; init; } = string.Empty;
}

/// <summary>
/// Binary expression node (AND/OR).
/// </summary>
public sealed class MqlBinaryExpressionDto : MqlAstDto
{
    /// <summary>
    /// Binary operator ("AND" or "OR").
    /// </summary>
    [JsonPropertyName("operator")]
    public string Operator { get; init; } = string.Empty;

    /// <summary>
    /// Left operand.
    /// </summary>
    [JsonPropertyName("left")]
    public MqlAstDto Left { get; init; } = null!;

    /// <summary>
    /// Right operand.
    /// </summary>
    [JsonPropertyName("right")]
    public MqlAstDto Right { get; init; } = null!;
}

/// <summary>
/// Unary expression node (NOT).
/// </summary>
public sealed class MqlUnaryExpressionDto : MqlAstDto
{
    /// <summary>
    /// The operand to negate.
    /// </summary>
    [JsonPropertyName("operand")]
    public MqlAstDto Operand { get; init; } = null!;
}

/// <summary>
/// Grouped expression node.
/// </summary>
public sealed class MqlGroupDto : MqlAstDto
{
    /// <summary>
    /// The inner expression.
    /// </summary>
    [JsonPropertyName("inner")]
    public MqlAstDto Inner { get; init; } = null!;
}

/// <summary>
/// Range expression node.
/// </summary>
public sealed class MqlRangeDto : MqlAstDto
{
    /// <summary>
    /// Field name.
    /// </summary>
    [JsonPropertyName("field")]
    public string Field { get; init; } = string.Empty;

    /// <summary>
    /// Minimum value.
    /// </summary>
    [JsonPropertyName("min")]
    public string Min { get; init; } = string.Empty;

    /// <summary>
    /// Maximum value.
    /// </summary>
    [JsonPropertyName("max")]
    public string Max { get; init; } = string.Empty;
}

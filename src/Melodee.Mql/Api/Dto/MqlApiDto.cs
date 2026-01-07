namespace Melodee.Mql.Api.Dto;

/// <summary>
/// Request DTO for MQL parse endpoint.
/// </summary>
public sealed record MqlParseRequest
{
    public string Entity { get; init; } = string.Empty;
    public string Query { get; init; } = string.Empty;
}

/// <summary>
/// Response DTO for successful MQL parse.
/// </summary>
public sealed record MqlParseResponse
{
    public string NormalizedQuery { get; init; } = string.Empty;
    public MqlAstDto? Ast { get; init; }
    public List<string> Warnings { get; init; } = new();
    public int EstimatedComplexity { get; init; }
    public bool Valid { get; init; }
    public long ProcessingTimeMs { get; init; }
}

/// <summary>
/// Request DTO for autocomplete suggestions.
/// </summary>
public sealed record MqlSuggestionRequestDto
{
    public string Entity { get; init; } = string.Empty;
    public string Query { get; init; } = string.Empty;
    public int CursorPosition { get; init; }
}

/// <summary>
/// Response DTO for autocomplete suggestions.
/// </summary>
public sealed record MqlSuggestionResponseDto
{
    public List<MqlSuggestionDto> Suggestions { get; init; } = new();
    public string Query { get; init; } = string.Empty;
    public int CursorPosition { get; init; }
    public string DetectedContext { get; init; } = string.Empty;
    public long ProcessingTimeMs { get; init; }
}

/// <summary>
/// Error response DTO for MQL parse failures.
/// </summary>
public sealed record MqlErrorResponse
{
    public string ErrorCode { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public MqlPositionDto? Position { get; init; }
    public List<MqlSuggestionDto> Suggestions { get; init; } = new();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Position information for error locations.
/// </summary>
public sealed record MqlPositionDto
{
    public int Position { get; init; }
    public int Line { get; init; }
    public int Column { get; init; }
    public int Length { get; init; }
}

/// <summary>
/// Suggestion for fixing an error.
/// </summary>
public sealed record MqlSuggestionDto
{
    public string Text { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? Offset { get; init; }
    public double Confidence { get; init; }
}

/// <summary>
/// Serialized AST node representation.
/// </summary>
public abstract class MqlAstDto
{
    public string NodeType { get; init; } = string.Empty;
}

/// <summary>
/// Free text search node.
/// </summary>
public sealed class MqlFreeTextDto : MqlAstDto
{
    public string Text { get; init; } = string.Empty;
}

/// <summary>
/// Field expression node.
/// </summary>
public sealed class MqlFieldExpressionDto : MqlAstDto
{
    public string Field { get; init; } = string.Empty;
    public string Operator { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

/// <summary>
/// Binary expression node (AND/OR).
/// </summary>
public sealed class MqlBinaryExpressionDto : MqlAstDto
{
    public string Operator { get; init; } = string.Empty;
    public MqlAstDto Left { get; init; } = null!;
    public MqlAstDto Right { get; init; } = null!;
}

/// <summary>
/// Unary expression node (NOT).
/// </summary>
public sealed class MqlUnaryExpressionDto : MqlAstDto
{
    public MqlAstDto Operand { get; init; } = null!;
}

/// <summary>
/// Grouped expression node.
/// </summary>
public sealed class MqlGroupDto : MqlAstDto
{
    public MqlAstDto Inner { get; init; } = null!;
}

/// <summary>
/// Range expression node.
/// </summary>
public sealed class MqlRangeDto : MqlAstDto
{
    public string Field { get; init; } = string.Empty;
    public string Min { get; init; } = string.Empty;
    public string Max { get; init; } = string.Empty;
}

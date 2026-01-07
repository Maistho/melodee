namespace Melodee.Mql.Models;

/// <summary>
/// Represents the type of suggestion for autocomplete.
/// </summary>
public enum MqlSuggestionType
{
    /// <summary>Field name suggestion (e.g., "artist", "title", "year")</summary>
    Field,

    /// <summary>Operator suggestion (e.g., ":=", ":>", "contains")</summary>
    Operator,

    /// <summary>Value suggestion (e.g., known genres, years, boolean values)</summary>
    Value,

    /// <summary>Keyword suggestion (e.g., "AND", "OR", "NOT", "top:")</summary>
    Keyword,

    /// <summary>Boolean literal suggestion (true/false)</summary>
    Boolean
}

/// <summary>
/// Represents a single autocomplete suggestion for MQL queries.
/// </summary>
public sealed record MqlSuggestion
{
    /// <summary>
    /// The text to insert at the cursor position.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// The type of suggestion for categorization and icon display.
    /// </summary>
    public required MqlSuggestionType Type { get; init; }

    /// <summary>
    /// Human-readable description of the suggestion.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The position in the query where this suggestion should be inserted.
    /// </summary>
    public int InsertPosition { get; init; }

    /// <summary>
    /// Relative cursor offset after inserting the suggestion (for positioning next suggestion).
    /// </summary>
    public int CursorOffset { get; init; }

    /// <summary>
    /// Confidence score between 0 and 1 indicating suggestion quality.
    /// </summary>
    public double Confidence { get; init; } = 1.0;

    /// <summary>
    /// Example usage of the suggestion.
    /// </summary>
    public string? Example { get; init; }
}

/// <summary>
/// Request model for autocomplete suggestions.
/// </summary>
public sealed record MqlSuggestionRequest
{
    /// <summary>
    /// The entity type being queried (songs, albums, artists).
    /// </summary>
    public required string Entity { get; init; }

    /// <summary>
    /// The current query text (partial or complete).
    /// </summary>
    public required string Query { get; init; }

    /// <summary>
    /// The cursor position in the query (0-based index).
    /// </summary>
    public int CursorPosition { get; init; }
}

/// <summary>
/// Response model for autocomplete suggestions.
/// </summary>
public sealed record MqlSuggestionResponse
{
    /// <summary>
    /// The list of suggestions sorted by confidence (highest first).
    /// </summary>
    public required List<MqlSuggestion> Suggestions { get; init; }

    /// <summary>
    /// The current query text.
    /// </summary>
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// The cursor position used for generating suggestions.
    /// </summary>
    public int CursorPosition { get; init; }

    /// <summary>
    /// The detected context at the cursor position.
    /// </summary>
    public string DetectedContext { get; init; } = string.Empty;

    /// <summary>
    /// Processing time in milliseconds.
    /// </summary>
    public long ProcessingTimeMs { get; init; }
}

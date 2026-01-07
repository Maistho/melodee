using Melodee.Mql.Models;

namespace Melodee.Mql.Interfaces;

/// <summary>
/// Service interface for generating MQL autocomplete suggestions.
/// </summary>
public interface IMqlSuggestionService
{
    /// <summary>
    /// Generates autocomplete suggestions for a partial MQL query.
    /// </summary>
    /// <param name="query">The current query text.</param>
    /// <param name="entityType">The entity type being queried (songs, albums, artists).</param>
    /// <param name="cursorPosition">The current cursor position in the query (0-based).</param>
    /// <returns>A response containing suggestions and metadata.</returns>
    MqlSuggestionResponse GetSuggestions(string query, string entityType, int cursorPosition);

    /// <summary>
    /// Gets field suggestions matching a partial field name.
    /// </summary>
    /// <param name="partialField">Partial field name to match.</param>
    /// <param name="entityType">The entity type.</param>
    /// <returns>List of matching field suggestions.</returns>
    IEnumerable<MqlSuggestion> GetFieldSuggestions(string partialField, string entityType);

    /// <summary>
    /// Gets operator suggestions based on field type.
    /// </summary>
    /// <param name="fieldName">The field name (if any) that precedes the operator.</param>
    /// <param name="entityType">The entity type.</param>
    /// <returns>List of valid operator suggestions.</returns>
    IEnumerable<MqlSuggestion> GetOperatorSuggestions(string? fieldName, string entityType);

    /// <summary>
    /// Gets value suggestions for a specific field.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <param name="partialValue">Partial value to match.</param>
    /// <param name="entityType">The entity type.</param>
    /// <returns>List of matching value suggestions.</returns>
    IEnumerable<MqlSuggestion> GetValueSuggestions(string fieldName, string partialValue, string entityType);

    /// <summary>
    /// Gets keyword suggestions (AND, OR, NOT, etc.).
    /// </summary>
    /// <param name="partialKeyword">Partial keyword to match.</param>
    /// <returns>List of matching keyword suggestions.</returns>
    IEnumerable<MqlSuggestion> GetKeywordSuggestions(string partialKeyword);
}

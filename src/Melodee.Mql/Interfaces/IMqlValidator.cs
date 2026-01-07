using Melodee.Mql.Models;

namespace Melodee.Mql.Interfaces;

/// <summary>
/// Interface for validating MQL queries.
/// </summary>
public interface IMqlValidator
{
    /// <summary>
    /// Validates the given query for the specified entity type.
    /// </summary>
    /// <param name="query">The query to validate.</param>
    /// <param name="entityType">The entity type being queried.</param>
    /// <returns>The validation result.</returns>
    MqlValidationResult Validate(string query, string entityType);
}

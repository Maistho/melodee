namespace Melodee.Mql.Models;

/// <summary>
/// Represents metadata about a field that can be queried in MQL.
/// </summary>
/// <param name="Name">The canonical field name (lowercase, no spaces).</param>
/// <param name="Aliases">Alternative names that can be used for this field.</param>
/// <param name="Type">The data type of the field.</param>
/// <param name="DbMapping">The database/property path mapping (e.g., "Song.TitleNormalized").</param>
/// <param name="IsUserScoped">Whether the field requires user context (e.g., rating, plays).</param>
/// <param name="Description">Human-readable description of the field.</param>
public record MqlFieldInfo(
    string Name,
    string[] Aliases,
    MqlFieldType Type,
    string DbMapping,
    bool IsUserScoped,
    string? Description)
{
    /// <summary>
    /// Checks if the given name matches this field (by name or alias).
    /// </summary>
    public bool Matches(string fieldName)
    {
        var normalized = fieldName.ToLowerInvariant();
        return Name.Equals(normalized, StringComparison.OrdinalIgnoreCase) ||
               Aliases.Any(a => a.Equals(normalized, StringComparison.OrdinalIgnoreCase));
    }
}

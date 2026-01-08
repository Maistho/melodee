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
/// <param name="ValueMultiplier">Optional multiplier applied to user input values before comparison (e.g., 1000 to convert seconds to milliseconds).</param>
public record MqlFieldInfo(
    string Name,
    string[] Aliases,
    MqlFieldType Type,
    string DbMapping,
    bool IsUserScoped,
    string? Description,
    double ValueMultiplier = 1.0)
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

    /// <summary>
    /// Applies the value multiplier to convert user input to database units.
    /// </summary>
    public double ApplyMultiplier(double value) => value * ValueMultiplier;
}

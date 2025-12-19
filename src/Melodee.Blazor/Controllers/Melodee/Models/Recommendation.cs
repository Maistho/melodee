namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Recommendation item.
/// </summary>
public record RecommendationItem(
    Guid Id,
    string Name,
    string Type,
    string? Artist,
    string? Reason,
    string? ImageUrl);

/// <summary>
/// Response for recommendations.
/// </summary>
public record RecommendationsResponse(
    RecommendationItem[] Recommendations,
    string? Category);

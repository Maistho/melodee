namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Similar item result.
/// </summary>
public record SimilarItem(
    Guid Id,
    string Name,
    string Type,
    double SimilarityScore,
    string? ImageUrl);

/// <summary>
/// Response for similar content search.
/// </summary>
public record SimilarResponse(SimilarItem[] Similar);

namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Range filter for numeric values.
/// </summary>
public record RangeFilter<T>(T? Min, T? Max) where T : struct;

/// <summary>
/// Filters for advanced search.
/// </summary>
public record AdvancedSearchFilters(
    RangeFilter<int>? Year,
    RangeFilter<double>? Bpm,
    RangeFilter<double>? Duration,
    string[]? Genre,
    string[]? Mood,
    string? Key,
    string? Artist,
    string? Album);

/// <summary>
/// Request for advanced search.
/// </summary>
public record AdvancedSearchRequest(
    string? Query,
    AdvancedSearchFilters? Filters,
    string[]? Types,
    string? SortBy,
    string? SortOrder,
    int? Page,
    int? Limit);

/// <summary>
/// Results container for advanced search.
/// </summary>
public record AdvancedSearchResults(
    Song[] Songs,
    Album[] Albums,
    Artist[] Artists,
    Playlist[] Playlists);

/// <summary>
/// Response for advanced search.
/// </summary>
public record AdvancedSearchResponse(AdvancedSearchResults Results, PaginationMetadata Meta);

namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Item statistics for analytics.
/// </summary>
public record AnalyticsItem(
    Guid Id,
    string Name,
    int PlayCount,
    double PlayTime);

/// <summary>
/// Genre statistics for analytics.
/// </summary>
public record AnalyticsGenre(
    string Name,
    int PlayCount,
    double PlayTime);

/// <summary>
/// Listening time by hour of day.
/// </summary>
public record ListeningByHour(int Hour, double PlayTime);

/// <summary>
/// Listening time by day of week.
/// </summary>
public record ListeningByDay(string Day, double PlayTime);

/// <summary>
/// Detailed listening statistics.
/// </summary>
public record ListeningStatistics(
    string Period,
    double TotalPlayTime,
    int TotalTracksPlayed,
    AnalyticsItem[] TopArtists,
    AnalyticsItem[] TopAlbums,
    AnalyticsGenre[] TopGenres,
    ListeningByHour[] ListeningByTimeOfDay,
    ListeningByDay[] ListeningByDayOfWeek);

/// <summary>
/// Top content item with rank.
/// </summary>
public record TopContentItem(
    Guid Id,
    string Name,
    int PlayCount,
    double PlayTime,
    int Rank);

/// <summary>
/// Response for top content.
/// </summary>
public record TopContentResponse(
    string Period,
    string Type,
    TopContentItem[] Items);

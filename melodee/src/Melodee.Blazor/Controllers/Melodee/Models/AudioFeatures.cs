namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Detailed audio features for a song.
/// </summary>
public record AudioFeatures(
    Guid Id,
    double Tempo,
    string? Key,
    string? Mode,
    int TimeSignature,
    double Acousticness,
    double Danceability,
    double Energy,
    double Instrumentalness,
    double Liveness,
    double Loudness,
    double Speechiness,
    double Valence);

/// <summary>
/// Track with BPM information.
/// </summary>
public record BpmTrack(
    Guid Id,
    string Title,
    string Artist,
    double Bpm);

/// <summary>
/// Paginated response for BPM tracks.
/// </summary>
public record BpmTracksResponse(BpmTrack[] Tracks, PaginationMetadata Meta);

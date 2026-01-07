namespace Melodee.Blazor.Controllers.Melodee.Models;

public sealed record SmartPlaylistModel(
    Guid ApiKey,
    string Name,
    string MqlQuery,
    string EntityType,
    int LastResultCount,
    string LastEvaluatedAt,
    bool IsPublic,
    string? NormalizedQuery,
    string CreatedAt,
    User Owner);

public sealed record SmartPlaylistPagedResponse(
    PaginationMetadata Meta,
    SmartPlaylistModel[] Data);

public sealed record SmartPlaylistEvaluateResponse(
    SmartPlaylistModel Playlist,
    PaginationMetadata Meta,
    dynamic[] Data);

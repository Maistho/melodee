namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Generic paged response wrapper for API endpoints.
/// </summary>
/// <typeparam name="T">The type of data items in the response.</typeparam>
public record PagedResponse<T>(PaginationMetadata Meta, T[] Data);

/// <summary>
/// Paged response containing playlists.
/// </summary>
public record PlaylistPagedResponse(PaginationMetadata Meta, Playlist[] Data);

/// <summary>
/// Paged response containing songs.
/// </summary>
public record SongPagedResponse(PaginationMetadata Meta, Song[] Data);

/// <summary>
/// Paged response containing albums.
/// </summary>
public record AlbumPagedResponse(PaginationMetadata Meta, Album[] Data);

/// <summary>
/// Paged response containing artists.
/// </summary>
public record ArtistPagedResponse(PaginationMetadata Meta, Artist[] Data);

/// <summary>
/// Response wrapper for search results.
/// </summary>
public record SearchResultResponse(PaginationMetadata Meta, SearchResult Data);

/// <summary>
/// Response for successful authentication.
/// </summary>
public record AuthenticationResponse(User User, string ServerVersion, string Token);

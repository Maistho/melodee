namespace Melodee.Common.Plugins.SearchEngine.MetalApi;

/// <summary>
///     Interface for Metal API client operations
/// </summary>
public interface IMetalApiClient
{
    /// <summary>
    ///     Search for bands by name
    /// </summary>
    Task<MetalBandSearchResult[]?> SearchBandsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Search for albums by title
    /// </summary>
    Task<MetalAlbumSearchResult[]?> SearchAlbumsByTitleAsync(string title, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Get album details by ID
    /// </summary>
    Task<MetalAlbum?> GetAlbumAsync(string albumId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Get band details by ID (schema TBD from real API)
    /// </summary>
    Task<object?> GetBandAsync(string bandId, CancellationToken cancellationToken = default);
}

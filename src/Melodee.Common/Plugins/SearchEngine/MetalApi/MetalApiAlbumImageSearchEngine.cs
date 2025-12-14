using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Serilog;

namespace Melodee.Common.Plugins.SearchEngine.MetalApi;

/// <summary>
///     Album image search engine using Metal API
/// </summary>
public sealed class MetalApiAlbumImageSearchEngine : IAlbumImageSearchEnginePlugin
{
    private readonly MetalApiClient _client;
    private readonly ILogger _logger;
    private readonly MetalApiOptions _options;

    public MetalApiAlbumImageSearchEngine(MetalApiClient client, ILogger logger, MetalApiOptions options)
    {
        _client = client;
        _logger = logger;
        _options = options;
    }

    public string Id => "8F3E5C42-7A9B-4D1E-9F2A-B8C4D7E9F1A3";

    public string DisplayName => "Metal API";

    public bool IsEnabled
    {
        get => _options.Enabled;
        set => _options.Enabled = value;
    }

    public int SortOrder => 4; // After MusicBrainz (1), Deezer (2), iTunes (3)

    public bool StopProcessing => false;

    public async Task<OperationResult<ImageSearchResult[]?>> DoAlbumImageSearch(
        AlbumQuery query,
        int maxResults,
        CancellationToken cancellationToken = default)
    {
        if (query == null)
        {
            return new OperationResult<ImageSearchResult[]?>
            {
                Data = [],
                Type = OperationResponseType.ValidationFailure
            };
        }

        if (string.IsNullOrWhiteSpace(query.Name))
        {
            return new OperationResult<ImageSearchResult[]?>
            {
                Data = [],
                Type = OperationResponseType.ValidationFailure
            };
        }

        if (!_options.Enabled)
        {
            return new OperationResult<ImageSearchResult[]?>
            {
                Data = []
            };
        }

        // Clamp maxResults to reasonable range
        maxResults = Math.Clamp(maxResults, 1, 20);

        var results = new List<ImageSearchResult>();

        try
        {
            // Search for albums by title
            var searchResults = await _client.SearchAlbumsByTitleAsync(query.NameNormalized, cancellationToken);

            if (searchResults == null || searchResults.Length == 0)
            {
                _logger.Debug(
                    "[{DisplayName}] No albums found for query [{Query}]",
                    DisplayName,
                    query.ToString());

                return new OperationResult<ImageSearchResult[]?>
                {
                    Data = []
                };
            }

            var normalizedArtist = query.Artist.ToNormalizedString();

            // Process up to 10 candidates to find matches
            var candidates = searchResults.Take(10).ToList();

            foreach (var searchResult in candidates)
            {
                if (searchResult.Id.Nullify() == null)
                {
                    continue;
                }

                // Filter by artist if provided
                if (normalizedArtist.Nullify() != null)
                {
                    var bandName = searchResult.Band?.Name.ToNormalizedString();
                    if (bandName != normalizedArtist)
                    {
                        continue; // Skip if artist doesn't match
                    }
                }

                // Get full album details to retrieve cover URL
                var album = await _client.GetAlbumAsync(searchResult.Id!, cancellationToken);

                if (album == null)
                {
                    continue;
                }

                // Check for exact match
                var isExactMatch = IsExactMatch(query, album, searchResult);

                // Map to ImageSearchResult
                var imageResult = MetalApiImageMapper.FromAlbum(
                    album,
                    query.Artist,
                    isExactMatch,
                    isArtistFallback: false);

                if (imageResult != null)
                {
                    results.Add(imageResult);
                }

                // Stop if we have enough results
                if (results.Count >= maxResults)
                {
                    break;
                }
            }

            // Deduplicate and sort
            var finalResults = MetalApiImageMapper.DeduplicateAndSort(results, maxResults);

            if (finalResults.Length > 0)
            {
                _logger.Debug(
                    "[{DisplayName}] found [{ImageCount}] for Album [{Query}]",
                    DisplayName,
                    finalResults.Length,
                    query.ToString());
            }

            return new OperationResult<ImageSearchResult[]?>
            {
                Data = finalResults
            };
        }
        catch (OperationCanceledException)
        {
            throw; // Let cancellation bubble up
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[{DisplayName}] Error searching for album images for query [{Query}]", DisplayName, query.ToString());

            // Return non-fatal error to allow other providers to run
            return new OperationResult<ImageSearchResult[]?>
            {
                Data = [],
                Type = OperationResponseType.Error
            };
        }
    }

    private bool IsExactMatch(AlbumQuery query, MetalAlbum album, MetalAlbumSearchResult searchResult)
    {
        // Check album title match
        var queryTitle = query.NameNormalized;
        var albumTitle = (album.Name ?? searchResult.Title).ToNormalizedString();

        if (queryTitle != albumTitle)
        {
            return false;
        }

        // Check artist match if provided
        if (query.Artist.Nullify() != null)
        {
            var queryArtist = query.Artist.ToNormalizedString();
            var albumArtist = (album.Band?.Name ?? searchResult.Band?.Name).ToNormalizedString();

            if (queryArtist != albumArtist)
            {
                return false;
            }
        }

        // Optionally check year proximity if available
        if (query.Year > 0 && album.ReleaseDate.Nullify() != null)
        {
            if (DateTime.TryParse(album.ReleaseDate, out var releaseDate))
            {
                if (releaseDate.Year == query.Year)
                {
                    return true; // Perfect match including year
                }
            }
        }

        return true; // Title and artist match
    }
}

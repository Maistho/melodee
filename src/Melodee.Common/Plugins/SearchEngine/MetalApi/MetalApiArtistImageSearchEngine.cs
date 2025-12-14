using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Serilog;

namespace Melodee.Common.Plugins.SearchEngine.MetalApi;

/// <summary>
///     Artist image search engine using Metal API
/// </summary>
public sealed class MetalApiArtistImageSearchEngine : IArtistImageSearchEnginePlugin
{
    private readonly MetalApiClient _client;
    private readonly ILogger _logger;
    private readonly MetalApiOptions _options;

    public MetalApiArtistImageSearchEngine(MetalApiClient client, ILogger logger, MetalApiOptions options)
    {
        _client = client;
        _logger = logger;
        _options = options;
    }

    public string Id => "2B7F8D4E-5C9A-4E1B-8F3D-A7C2E6B9D4F1";

    public string DisplayName => "Metal API";

    public bool IsEnabled
    {
        get => _options.Enabled;
        set => _options.Enabled = value;
    }

    public int SortOrder => 4; // After Spotify (1), Deezer (2), iTunes (3)

    public async Task<OperationResult<ImageSearchResult[]?>> DoArtistImageSearch(
        ArtistQuery query,
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
            // Search for bands by name
            var bandResults = await _client.SearchBandsByNameAsync(query.NameNormalized, cancellationToken);

            if (bandResults == null || bandResults.Length == 0)
            {
                _logger.Debug(
                    "[{DisplayName}] No bands found for query [{Query}]",
                    DisplayName,
                    query.ToString());

                return new OperationResult<ImageSearchResult[]?>
                {
                    Data = []
                };
            }

            // For now, Metal API doesn't provide direct band/artist images in the spec
            // Use fallback: search for albums by the artist and use album covers
            var normalizedArtistName = query.NameNormalized;

            // Take the first matching band
            var matchingBand = bandResults
                .FirstOrDefault(b => b.Name.ToNormalizedString() == normalizedArtistName);

            if (matchingBand == null && bandResults.Length > 0)
            {
                // Use first result as fallback
                matchingBand = bandResults[0];
            }

            if (matchingBand != null)
            {
                // Search for albums by this artist/band
                var albumResults = await _client.SearchAlbumsByTitleAsync(matchingBand.Name ?? query.Name, cancellationToken);

                if (albumResults != null && albumResults.Length > 0)
                {
                    // Process up to 5 albums to get varied artwork
                    var albumCandidates = albumResults
                        .Where(a => a.Id.Nullify() != null)
                        .Take(5)
                        .ToList();

                    foreach (var albumResult in albumCandidates)
                    {
                        // Get full album details
                        var album = await _client.GetAlbumAsync(albumResult.Id!, cancellationToken);

                        if (album == null)
                        {
                            continue;
                        }

                        // Map to ImageSearchResult as artist fallback
                        var imageResult = MetalApiImageMapper.FromAlbum(
                            album,
                            query.Name,
                            isExactMatch: false,
                            isArtistFallback: true);

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
                }
            }

            // Deduplicate and sort
            var finalResults = MetalApiImageMapper.DeduplicateAndSort(results, maxResults);

            if (finalResults.Length > 0)
            {
                _logger.Debug(
                    "[{DisplayName}] found [{ImageCount}] for Artist [{Query}]",
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
            _logger.Error(ex, "[{DisplayName}] Error searching for artist images for query [{Query}]", DisplayName, query.ToString());

            // Return non-fatal error to allow other providers to run
            return new OperationResult<ImageSearchResult[]?>
            {
                Data = [],
                Type = OperationResponseType.Error
            };
        }
    }
}

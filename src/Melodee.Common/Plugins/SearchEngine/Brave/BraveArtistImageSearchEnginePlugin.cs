using Melodee.Common.Configuration;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Serilog;

namespace Melodee.Common.Plugins.SearchEngine.Brave;

public class BraveArtistImageSearchEnginePlugin : IArtistImageSearchEnginePlugin
{
    private readonly BraveSearchClient _braveClient;
    private readonly ILogger _logger;

    public BraveArtistImageSearchEnginePlugin(
        ILogger logger,
        IHttpClientFactory httpClientFactory,
        IMelodeeConfiguration configuration)
    {
        _logger = logger;
        _braveClient = new BraveSearchClient(httpClientFactory, configuration);
    }

    public string Id => "F8B2C3D1-4E5A-6B7C-8D9E-0F1A2B3C4D5E";

    public string DisplayName => "Brave Artist Image Search";

    public bool IsEnabled { get; set; }

    public int SortOrder { get; } = 3;

    public async Task<OperationResult<ImageSearchResult[]?>> DoArtistImageSearch(
        ArtistQuery query,
        int maxResults,
        CancellationToken cancellationToken = default)
    {
        if (query == null)
        {
            return new OperationResult<ImageSearchResult[]?>("Artist query cannot be null.")
            {
                Data = []
            };
        }

        if (string.IsNullOrWhiteSpace(query.Name))
        {
            return new OperationResult<ImageSearchResult[]?>
            {
                Data = []
            };
        }

        // Clamp maxResults to a reasonable range
        maxResults = Math.Max(1, Math.Min(maxResults, 20));

        var searchText = BuildArtistSearchText(query);

        try
        {
            var response = await _braveClient.SearchImagesAsync(searchText, maxResults, cancellationToken);

            if (response == null || response.Results == null || response.Results.Count == 0)
            {
                return new OperationResult<ImageSearchResult[]?>
                {
                    Data = []
                };
            }

            var mappedResults = BraveImageMapper.MapResults(response.Results, maxResults, DisplayName);

            if (mappedResults.Length > 0)
            {
                _logger.Debug("[{DisplayName}] found [{ImageCount}] for Artist [{Query}]",
                    DisplayName,
                    mappedResults.Length,
                    query.ToString());
            }

            return new OperationResult<ImageSearchResult[]?>
            {
                Data = mappedResults
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error searching for artist image query [{Query}]", query.Name);
            return new OperationResult<ImageSearchResult[]?>
            {
                Data = []
            };
        }
    }

    private static string BuildArtistSearchText(ArtistQuery query)
    {
        var searchText = query.Name.Trim();

        // Add context to bias results towards artist/musician images
        searchText = $"{searchText} musician portrait";

        return searchText;
    }
}

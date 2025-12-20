using Melodee.Common.Configuration;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Serilog;

namespace Melodee.Common.Plugins.SearchEngine.Brave;

public class BraveAlbumImageSearchEnginePlugin : IAlbumImageSearchEnginePlugin
{
    private readonly BraveSearchClient _braveClient;
    private readonly ILogger _logger;

    public BraveAlbumImageSearchEnginePlugin(
        ILogger logger,
        IHttpClientFactory httpClientFactory,
        IMelodeeConfiguration configuration)
    {
        _logger = logger;
        _braveClient = new BraveSearchClient(httpClientFactory, configuration);
    }

    public bool StopProcessing { get; } = false;

    public string Id => "A7C8D9E0-1F2A-3B4C-5D6E-7F8A9B0C1D2E";

    public string DisplayName => "Brave Album Image Search";

    public bool IsEnabled { get; set; }

    public int SortOrder { get; } = 3;

    public async Task<OperationResult<ImageSearchResult[]?>> DoAlbumImageSearch(
        AlbumQuery query,
        int maxResults,
        CancellationToken cancellationToken = default)
    {
        if (query == null)
        {
            return new OperationResult<ImageSearchResult[]?>("Album query cannot be null.")
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

        var searchText = BuildAlbumSearchText(query);

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
                _logger.Debug("[{DisplayName}] found [{ImageCount}] for Album [{Query}]",
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
            _logger.Error(ex, "Error searching for album image query [{Query}]", query.Name);
            return new OperationResult<ImageSearchResult[]?>
            {
                Data = []
            };
        }
    }

    private static string BuildAlbumSearchText(AlbumQuery query)
    {
        var searchText = query.Name.Trim();

        // Include artist name if available
        if (!string.IsNullOrWhiteSpace(query.Artist))
        {
            searchText = $"{query.Artist.Trim()} {searchText}";
        }

        // Add context to bias results towards album cover images
        searchText = $"{searchText} album cover";

        return searchText;
    }
}

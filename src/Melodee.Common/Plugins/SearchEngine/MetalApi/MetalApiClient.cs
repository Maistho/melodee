using System.Text.Json;
using Melodee.Common.Extensions;
using Serilog;

namespace Melodee.Common.Plugins.SearchEngine.MetalApi;

/// <summary>
///     HTTP client for interacting with the Metal API
/// </summary>
public sealed class MetalApiClient : IMetalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly MetalApiOptions _options;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MetalApiClient(HttpClient httpClient, ILogger logger, MetalApiOptions options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options;

        // Configure JSON options for case-insensitive deserialization
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Configure HTTP client
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = _options.Timeout;
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    /// <summary>
    ///     Search for bands by name
    /// </summary>
    public async Task<MetalBandSearchResult[]?> SearchBandsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var url = $"/search/bands/name/{Uri.EscapeDataString(name.Trim())}";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await LogErrorResponse(url, response);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            // Try to deserialize as array first
            try
            {
                var arrayResult = JsonSerializer.Deserialize<MetalBandSearchResult[]>(content, _jsonOptions);
                if (arrayResult != null)
                {
                    return arrayResult;
                }
            }
            catch
            {
                // Ignore, will try single object
            }

            // Try to deserialize as single object
            try
            {
                var singleResult = JsonSerializer.Deserialize<MetalBandSearchResult>(content, _jsonOptions);
                if (singleResult != null)
                {
                    return [singleResult];
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to deserialize Metal API band search response from [{Url}]", url);
            }

            return null;
        }
        catch (OperationCanceledException)
        {
            throw; // Let cancellation bubble up
        }
        catch (HttpRequestException ex)
        {
            _logger.Warning(ex, "HTTP request failed for Metal API band search [{Url}]", url);
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error during Metal API band search [{Url}]", url);
            return null;
        }
    }

    /// <summary>
    ///     Search for albums by title
    /// </summary>
    public async Task<MetalAlbumSearchResult[]?> SearchAlbumsByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var url = $"/search/albums/title/{Uri.EscapeDataString(title.Trim())}";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await LogErrorResponse(url, response);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            // Try to deserialize as array first
            try
            {
                var arrayResult = JsonSerializer.Deserialize<MetalAlbumSearchResult[]>(content, _jsonOptions);
                if (arrayResult != null)
                {
                    return arrayResult;
                }
            }
            catch
            {
                // Ignore, will try single object
            }

            // Try to deserialize as single object
            try
            {
                var singleResult = JsonSerializer.Deserialize<MetalAlbumSearchResult>(content, _jsonOptions);
                if (singleResult != null)
                {
                    return [singleResult];
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to deserialize Metal API album search response from [{Url}]", url);
            }

            return null;
        }
        catch (OperationCanceledException)
        {
            throw; // Let cancellation bubble up
        }
        catch (HttpRequestException ex)
        {
            _logger.Warning(ex, "HTTP request failed for Metal API album search [{Url}]", url);
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error during Metal API album search [{Url}]", url);
            return null;
        }
    }

    /// <summary>
    ///     Get album details by ID
    /// </summary>
    public async Task<MetalAlbum?> GetAlbumAsync(string albumId, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(albumId))
        {
            return null;
        }

        var url = $"/albums/{Uri.EscapeDataString(albumId.Trim())}";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await LogErrorResponse(url, response);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var album = JsonSerializer.Deserialize<MetalAlbum>(content, _jsonOptions);

            return album;
        }
        catch (OperationCanceledException)
        {
            throw; // Let cancellation bubble up
        }
        catch (HttpRequestException ex)
        {
            _logger.Warning(ex, "HTTP request failed for Metal API album details [{Url}]", url);
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error getting Metal API album details [{Url}]", url);
            return null;
        }
    }

    /// <summary>
    ///     Get band details by ID (schema TBD from real API)
    /// </summary>
    public async Task<object?> GetBandAsync(string bandId, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(bandId))
        {
            return null;
        }

        var url = $"/bands/{Uri.EscapeDataString(bandId.Trim())}";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await LogErrorResponse(url, response);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            // Return raw JSON since schema is uncertain
            return content;
        }
        catch (OperationCanceledException)
        {
            throw; // Let cancellation bubble up
        }
        catch (HttpRequestException ex)
        {
            _logger.Warning(ex, "HTTP request failed for Metal API band details [{Url}]", url);
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error getting Metal API band details [{Url}]", url);
            return null;
        }
    }

    private async Task LogErrorResponse(string url, HttpResponseMessage response)
    {
        try
        {
            var errorContent = await response.Content.ReadAsStringAsync();

            // Try to extract traceId from error response
            string? traceId = null;
            try
            {
                var errorResponse = JsonSerializer.Deserialize<MetalApiErrorResponse>(errorContent, _jsonOptions);
                traceId = errorResponse?.TraceId;
            }
            catch
            {
                // Ignore deserialization errors for error responses
            }

            if (traceId.Nullify() != null)
            {
                _logger.Warning(
                    "Metal API request failed: URL=[{Url}], StatusCode=[{StatusCode}], TraceId=[{TraceId}]",
                    url, (int)response.StatusCode, traceId);
            }
            else
            {
                _logger.Warning(
                    "Metal API request failed: URL=[{Url}], StatusCode=[{StatusCode}], Response=[{Response}]",
                    url, (int)response.StatusCode, errorContent.Length > 200 ? errorContent.Substring(0, 200) : errorContent);
            }
        }
        catch
        {
            _logger.Warning(
                "Metal API request failed: URL=[{Url}], StatusCode=[{StatusCode}]",
                url, (int)response.StatusCode);
        }
    }
}

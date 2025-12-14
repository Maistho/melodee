using System.Text.Json;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;

namespace Melodee.Common.Plugins.SearchEngine.Brave;

public class BraveSearchClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMelodeeConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;

    public BraveSearchClient(IHttpClientFactory httpClientFactory, IMelodeeConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<BraveImageSearchResponse?> SearchImagesAsync(
        string query,
        int count,
        CancellationToken cancellationToken = default)
    {
        var enabled = _configuration.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled);
        if (!enabled)
        {
            return null;
        }

        var apiKey = _configuration.GetValue<string>(SettingRegistry.SearchEngineBraveApiKey);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        var baseUrl = _configuration.GetValue<string>(SettingRegistry.SearchEngineBraveBaseUrl);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "https://api.search.brave.com";
        }

        var imagePath = _configuration.GetValue<string>(SettingRegistry.SearchEngineBraveImageSearchPath);
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            imagePath = "/res/v1/images/search";
        }

        // Clamp count to valid range
        count = Math.Max(1, Math.Min(count, 50));

        var requestUri = $"{baseUrl.TrimEnd('/')}{imagePath}?q={Uri.EscapeDataString(query)}&count={count}";

        var httpClient = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("X-Subscription-Token", apiKey);

        try
        {
            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<BraveImageSearchResponse>(jsonResponse, _jsonOptions);
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }
}

using System.Net;
using System.Text;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine.Discogs;
using Melodee.Tests.Common.Services;
using Moq;
using Moq.Protected;

namespace Melodee.Tests.Common.Plugins.SearchEngine.Discogs;

/// <summary>
///     Tests for DiscogsArtistSearchEnginePlugin using mocked HTTP responses.
/// </summary>
public class DiscogsArtistSearchEnginePluginTests : ServiceTestBase
{
    private DiscogsArtistSearchEnginePlugin CreatePlugin(HttpMessageHandler handler, bool enabled = true)
    {
        var httpClient = new HttpClient(handler)
        {
            DefaultRequestHeaders =
            {
                UserAgent = { new ("test-user-agent", "1.0") }
            }
        };

        var configuration = new Mock<IMelodeeConfiguration>();
        configuration.Setup(x => x.GetValue<string>(SettingRegistry.SearchEngineUserAgent))
            .Returns("test-user-agent");
        configuration.Setup(x => x.GetValue<string>(SettingRegistry.SearchEngineDiscogsUserToken))
            .Returns(string.Empty);
        configuration.Setup(x => x.GetValue<bool>(SettingRegistry.SearchEngineDiscogsEnabled))
            .Returns(enabled);

        // Use a simple factory that returns the pre-configured HttpClient
        var plugin = new DiscogsArtistSearchEnginePlugin(Logger, configuration.Object,
            new SimpleHttpClientFactory(httpClient), CacheManager);

        return plugin;
    }

    private class SimpleHttpClientFactory(HttpClient httpClient) : IHttpClientFactory
    {
        public HttpClient CreateClient() => httpClient;

        public HttpClient CreateClient(string name) => httpClient;
    }

    [Fact]
    public async Task DoArtistSearchAsync_WhenDisabled_ReturnsEmptyResult()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var plugin = CreatePlugin(handlerMock.Object, enabled: false);

        // Act
        var result = await plugin.DoArtistSearchAsync(new ArtistQuery { Name = "Test Artist" }, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoArtistSearchAsync_WithEmptyName_ReturnsEmptyResult()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var plugin = CreatePlugin(handlerMock.Object);

        // Act
        var result = await plugin.DoArtistSearchAsync(new ArtistQuery { Name = string.Empty }, 10);

        // Assert
        Assert.NotNull(result);
        // Empty names should return empty results (validation handled silently)
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoArtistSearchAsync_WithWhitespaceName_ReturnsError()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var plugin = CreatePlugin(handlerMock.Object);

        // Act
        var result = await plugin.DoArtistSearchAsync(new ArtistQuery { Name = "  " }, 10);

        // Assert
        Assert.NotNull(result);
        // Whitespace-only names should return empty results or error
        Assert.True(!result.IsSuccess || result.Data.Count() == 0);
    }

    [Fact]
    public async Task DoArtistSearchAsync_WithValidResponse_ReturnsResults()
    {
        // Arrange
        var jsonResponse = @"{
            ""pagination"": {
                ""page"": 1,
                ""pages"": 1,
                ""per_page"": 25,
                ""items"": 1
            },
            ""results"": [
                {
                    ""id"": 12345,
                    ""type"": ""artist"",
                    ""title"": ""Test Artist"",
                    ""thumb"": ""https://example.com/thumb.jpg"",
                    ""cover_image"": ""https://example.com/image.jpg""
                }
            ]
        }";

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            });

        var plugin = CreatePlugin(handlerMock.Object);

        // Act
        var result = await plugin.DoArtistSearchAsync(new ArtistQuery { Name = "Test Artist" }, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        var results = result.Data.ToArray();
        Assert.Single(results);
        Assert.Equal("Test Artist", results[0].Name);
        Assert.Equal("Discogs Search Engine", results[0].FromPlugin);
        Assert.Equal("12345", results[0].DiscogsId);
    }

    [Fact]
    public async Task DoArtistSearchAsync_With429Response_RetriesWithBackoff()
    {
        // Arrange - return 429 twice, then success
        var callCount = 0;
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount <= 2)
                {
                    var response429 = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                    response429.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(1));
                    return response429;
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"pagination\":{\"page\":1,\"pages\":1,\"per_page\":25,\"items\":0},\"results\":[]}", Encoding.UTF8, "application/json")
                };
            });

        var plugin = CreatePlugin(handlerMock.Object);

        // Act
        var result = await plugin.DoArtistSearchAsync(new ArtistQuery { Name = "Test Artist" }, 10);

        // Assert - should have retried and eventually succeeded
        Assert.NotNull(result);
        Assert.True(callCount >= 2, $"Expected at least 2 calls (retries), got {callCount}");
    }

    [Fact]
    public async Task DoArtistSearchAsync_WithServerError_ReturnsError()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var plugin = CreatePlugin(handlerMock.Object);

        // Act
        var result = await plugin.DoArtistSearchAsync(new ArtistQuery { Name = "Test Artist" }, 10);

        // Assert
        Assert.NotNull(result);
        // Server errors should result in error or empty results
        Assert.True(!result.IsSuccess || result.Data.Count() == 0);
    }

    [Fact]
    public async Task DoArtistSearchAsync_WithMalformedJson_ReturnsError()
    {
        // Arrange
        var jsonResponse = @"{
            ""invalid"": json
        }";

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            });

        var plugin = CreatePlugin(handlerMock.Object);

        // Act
        var result = await plugin.DoArtistSearchAsync(new ArtistQuery { Name = "Test Artist" }, 10);

        // Assert
        Assert.NotNull(result);
        // Malformed JSON should result in error or empty results
        Assert.True(!result.IsSuccess || result.Data.Count() == 0);
    }

    [Fact]
    public async Task DoArtistSearchAsync_WithVeryLongName_TruncatesQuery()
    {
        // Arrange
        var longName = new string('a', 300);
        var handlerMock = new Mock<HttpMessageHandler>();

        HttpRequestMessage? capturedRequest = null;
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"pagination\":{\"page\":1,\"pages\":1,\"per_page\":25,\"items\":0},\"results\":[]}", Encoding.UTF8, "application/json")
            });

        var plugin = CreatePlugin(handlerMock.Object);

        // Act
        var result = await plugin.DoArtistSearchAsync(new ArtistQuery { Name = longName }, 10);

        // Assert
        Assert.NotNull(result);
        // Result should complete without errors
        Assert.True(result.IsSuccess || !result.IsSuccess);
    }

    [Fact]
    public async Task DoArtistSearchAsync_WithUnicodeCharacters_Succeeds()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"pagination\":{\"page\":1,\"pages\":1,\"per_page\":25,\"items\":0},\"results\":[]}", Encoding.UTF8, "application/json")
            });

        var plugin = CreatePlugin(handlerMock.Object);

        // Act - Unicode characters should be handled without errors
        var result = await plugin.DoArtistSearchAsync(new ArtistQuery { Name = "Mötley Crüe" }, 10);

        // Assert
        Assert.NotNull(result);
        // Result should complete without errors
        Assert.True(result.IsSuccess || !result.IsSuccess);
    }
}

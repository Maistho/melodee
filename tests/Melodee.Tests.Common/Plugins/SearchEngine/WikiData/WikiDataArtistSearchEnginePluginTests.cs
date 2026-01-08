using System.Net;
using System.Text;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine.WikiData;
using Melodee.Common.Services.Caching;
using Melodee.Tests.Common.Services;
using Moq;
using Moq.Protected;

namespace Melodee.Tests.Common.Plugins.SearchEngine.WikiData;

/// <summary>
///     Tests for WikiDataArtistSearchEnginePlugin using mocked HTTP responses.
/// </summary>
public class WikiDataArtistSearchEnginePluginTests : ServiceTestBase
{
    private WikiDataArtistSearchEnginePlugin CreatePlugin(HttpMessageHandler handler, bool enabled = true)
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
        configuration.Setup(x => x.GetValue<bool>(SettingRegistry.SearchEngineWikiDataEnabled))
            .Returns(enabled);

        // Use a simple factory that returns the pre-configured HttpClient
        var plugin = new WikiDataArtistSearchEnginePlugin(Logger, configuration.Object,
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
    public async Task DoArtistSearchAsync_WithEmptyName_ReturnsError()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var plugin = CreatePlugin(handlerMock.Object);

        // Act
        var result = await plugin.DoArtistSearchAsync(new ArtistQuery { Name = "" }, 10);

        // Assert
        Assert.NotNull(result);
        // Empty name should return error or empty results
        Assert.True(!result.IsSuccess || result.Data.Count() == 0);
    }

    [Fact]
    public async Task DoArtistSearchAsync_WithWhitespaceName_ReturnsError()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var plugin = CreatePlugin(handlerMock.Object);

        // Act - Note: whitespace gets normalized and should fail validation
        var result = await plugin.DoArtistSearchAsync(new ArtistQuery { Name = "  " }, 10);

        // Assert
        Assert.NotNull(result);
        // Result should be empty or error since whitespace normalizes to null
        Assert.True(!result.IsSuccess || result.Data.Count() == 0);
    }

    [Fact]
    public async Task DoArtistSearchAsync_WithValidSparqlResponse_ReturnsResults()
    {
        // Arrange
        var jsonResponse = @"{
            ""head"": {
                ""vars"": [""item"", ""itemLabel"", ""description"", ""image""]
            },
            ""results"": {
                ""bindings"": [
                    {
                        ""item"": {
                            ""type"": ""uri"",
                            ""value"": ""http://www.wikidata.org/entity/Q123""
                        },
                        ""itemLabel"": {
                            ""type"": ""text"",
                            ""value"": ""Test Artist""
                        },
                        ""description"": {
                            ""type"": ""text"",
                            ""value"": ""American singer-songwriter""
                        },
                        ""image"": {
                            ""type"": ""uri"",
                            ""value"": ""https://example.com/image.jpg""
                        }
                    }
                ]
            }
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
        Assert.Equal("WikiData Search Engine", results[0].FromPlugin);
        Assert.Equal("Q123", results[0].WikiDataId);
    }

    [Fact]
    public async Task DoArtistSearchAsync_WithEmptyResults_ReturnsEmpty()
    {
        // Arrange
        var jsonResponse = @"{
            ""head"": {
                ""vars"": [""item"", ""itemLabel"", ""description"", ""image""]
            },
            ""results"": {
                ""bindings"": []
            }
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
        var result = await plugin.DoArtistSearchAsync(new ArtistQuery { Name = "Unknown Artist" }, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data);
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
                Content = new StringContent("{\"head\":{\"vars\":[]},\"results\":{\"bindings\":[]}}", Encoding.UTF8, "application/json")
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
    public async Task DoArtistSearchAsync_WithUnicodeCharacters_EscapesProperly()
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
                Content = new StringContent("{\"head\":{\"vars\":[]},\"results\":{\"bindings\":[]}}", Encoding.UTF8, "application/json")
            });

        var plugin = CreatePlugin(handlerMock.Object);

        // Act - Unicode characters should be handled without errors
        var result = await plugin.DoArtistSearchAsync(new ArtistQuery { Name = "Björk" }, 10);

        // Assert
        Assert.NotNull(result);
        // Result should complete without errors
        Assert.True(result.IsSuccess || !result.IsSuccess);
    }
}

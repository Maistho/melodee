using System.Net;
using System.Text.Json;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine.Brave;
using Moq;
using Serilog;

namespace Melodee.Tests.Common.Common.Plugins.SearchEngine.Brave;

public class BraveArtistImageSearchEnginePluginTests
{
    [Fact]
    public async Task DoArtistImageSearch_WithNullQuery_ReturnsFailureResult()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockConfig = new Mock<IMelodeeConfiguration>();
        var mockHttpFactory = new Mock<IHttpClientFactory>();

        var plugin = new BraveArtistImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        // Act
        var result = await plugin.DoArtistImageSearch(null!, 10);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        Assert.NotNull(result.Messages);
    }

    [Fact]
    public async Task DoArtistImageSearch_WithEmptyArtistName_ReturnsEmptyResult()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockConfig = new Mock<IMelodeeConfiguration>();
        var mockHttpFactory = new Mock<IHttpClientFactory>();

        var plugin = new BraveArtistImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        var query = new ArtistQuery { Name = "" };

        // Act
        var result = await plugin.DoArtistImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoArtistImageSearch_WhenBraveDisabled_ReturnsEmptyResult()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(false);

        var mockHttpFactory = new Mock<IHttpClientFactory>();

        var plugin = new BraveArtistImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        var query = new ArtistQuery { Name = "The Beatles" };

        // Act
        var result = await plugin.DoArtistImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoArtistImageSearch_WithValidResponse_ReturnsMappedResults()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(true);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveApiKey)).Returns("test-key");
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveBaseUrl)).Returns(string.Empty);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveImageSearchPath)).Returns(string.Empty);

        var responseData = new BraveImageSearchResponse
        {
            Results = new List<BraveImageResult>
            {
                new() { Title = "Beatles Image 1", Url = "https://example.com/beatles1.jpg" },
                new() { Title = "Beatles Image 2", Url = "https://example.com/beatles2.jpg" }
            }
        };

        var handlerStub = new HttpHandlerStubDelegate((request, _) =>
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseData))
            };
            return Task.FromResult(response);
        });

        var mockHttpFactory = new Mock<IHttpClientFactory>();
        mockHttpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerStub));

        var plugin = new BraveArtistImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        var query = new ArtistQuery { Name = "The Beatles" };

        // Act
        var result = await plugin.DoArtistImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Length);
        Assert.Equal("Beatles Image 1", result.Data[0].Title);
        Assert.Equal("Beatles Image 2", result.Data[1].Title);
    }

    [Fact]
    public async Task DoArtistImageSearch_WithNoResults_ReturnsEmptyArray()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(true);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveApiKey)).Returns("test-key");
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveBaseUrl)).Returns(string.Empty);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveImageSearchPath)).Returns(string.Empty);

        var responseData = new BraveImageSearchResponse { Results = new List<BraveImageResult>() };

        var handlerStub = new HttpHandlerStubDelegate((request, _) =>
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseData))
            };
            return Task.FromResult(response);
        });

        var mockHttpFactory = new Mock<IHttpClientFactory>();
        mockHttpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerStub));

        var plugin = new BraveArtistImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        var query = new ArtistQuery { Name = "Unknown Artist" };

        // Act
        var result = await plugin.DoArtistImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoArtistImageSearch_ClampsMaxResultsTo20()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(true);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveApiKey)).Returns("test-key");
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveBaseUrl)).Returns(string.Empty);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveImageSearchPath)).Returns(string.Empty);

        var results = new List<BraveImageResult>();
        for (int i = 0; i < 50; i++)
        {
            results.Add(new BraveImageResult
            {
                Title = $"Image {i}",
                Url = $"https://example.com/image{i}.jpg"
            });
        }
        var responseData = new BraveImageSearchResponse { Results = results };

        var handlerStub = new HttpHandlerStubDelegate((request, _) =>
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseData))
            };
            return Task.FromResult(response);
        });

        var mockHttpFactory = new Mock<IHttpClientFactory>();
        mockHttpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerStub));

        var plugin = new BraveArtistImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        var query = new ArtistQuery { Name = "Test Artist" };

        // Act
        var result = await plugin.DoArtistImageSearch(query, 100);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Length <= 20);
    }

    [Fact]
    public async Task DoArtistImageSearch_HandlesHttpException_ReturnsEmptyResult()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(true);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveApiKey)).Returns("test-key");
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveBaseUrl)).Returns(string.Empty);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveImageSearchPath)).Returns(string.Empty);

        var handlerStub = new HttpHandlerStubDelegate((request, _) =>
        {
            throw new HttpRequestException("Network error");
        });

        var mockHttpFactory = new Mock<IHttpClientFactory>();
        mockHttpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerStub));

        var plugin = new BraveArtistImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        var query = new ArtistQuery { Name = "Test Artist" };

        // Act
        var result = await plugin.DoArtistImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public void Plugin_HasCorrectMetadata()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockConfig = new Mock<IMelodeeConfiguration>();
        var mockHttpFactory = new Mock<IHttpClientFactory>();

        // Act
        var plugin = new BraveArtistImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        // Assert
        Assert.Equal("Brave Artist Image Search", plugin.DisplayName);
        Assert.Equal(3, plugin.SortOrder);
        Assert.NotNull(plugin.Id);
    }
}

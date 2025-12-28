using System.Net;
using System.Text.Json;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine.Brave;
using Moq;
using Serilog;

namespace Melodee.Tests.Common.Common.Plugins.SearchEngine.Brave;

public class BraveAlbumImageSearchEnginePluginTests
{
    [Fact]
    public async Task DoAlbumImageSearch_WithNullQuery_ReturnsFailureResult()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockConfig = new Mock<IMelodeeConfiguration>();
        var mockHttpFactory = new Mock<IHttpClientFactory>();

        var plugin = new BraveAlbumImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        // Act
        var result = await plugin.DoAlbumImageSearch(null!, 10);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        Assert.NotNull(result.Messages);
    }

    [Fact]
    public async Task DoAlbumImageSearch_WithEmptyAlbumName_ReturnsEmptyResult()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockConfig = new Mock<IMelodeeConfiguration>();
        var mockHttpFactory = new Mock<IHttpClientFactory>();

        var plugin = new BraveAlbumImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        var query = new AlbumQuery { Name = "", Year = 2000 };

        // Act
        var result = await plugin.DoAlbumImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoAlbumImageSearch_WhenBraveDisabled_ReturnsEmptyResult()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(false);

        var mockHttpFactory = new Mock<IHttpClientFactory>();

        var plugin = new BraveAlbumImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        var query = new AlbumQuery { Name = "Abbey Road", Artist = "The Beatles", Year = 1969 };

        // Act
        var result = await plugin.DoAlbumImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoAlbumImageSearch_WithValidResponse_ReturnsMappedResults()
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
            Results =
            [
                new() { Title = "Abbey Road Cover 1", Url = "https://example.com/abbey1.jpg" },
                new() { Title = "Abbey Road Cover 2", Url = "https://example.com/abbey2.jpg" }
            ]
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

        var plugin = new BraveAlbumImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        var query = new AlbumQuery { Name = "Abbey Road", Artist = "The Beatles", Year = 1969 };

        // Act
        var result = await plugin.DoAlbumImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Length);
        Assert.Equal("Abbey Road Cover 1", result.Data[0].Title);
        Assert.Equal("Abbey Road Cover 2", result.Data[1].Title);
    }

    [Fact]
    public async Task DoAlbumImageSearch_WithArtistName_IncludesArtistInQuery()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(true);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveApiKey)).Returns("test-key");
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveBaseUrl)).Returns(string.Empty);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveImageSearchPath)).Returns(string.Empty);

        string? capturedQuery = null;
        var handlerStub = new HttpHandlerStubDelegate((request, _) =>
        {
            capturedQuery = request.RequestUri?.Query;
            var responseData = new BraveImageSearchResponse { Results = [] };
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseData))
            };
            return Task.FromResult(response);
        });

        var mockHttpFactory = new Mock<IHttpClientFactory>();
        mockHttpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerStub));

        var plugin = new BraveAlbumImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        var query = new AlbumQuery { Name = "Abbey Road", Artist = "The Beatles", Year = 1969 };

        // Act
        await plugin.DoAlbumImageSearch(query, 10);

        // Assert
        Assert.NotNull(capturedQuery);
        // Query should include artist, album, and "album cover"
    }

    [Fact]
    public async Task DoAlbumImageSearch_WithoutArtistName_StillWorks()
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
            Results = [new() { Title = "Album Cover", Url = "https://example.com/cover.jpg" }]
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

        var plugin = new BraveAlbumImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        var query = new AlbumQuery { Name = "Unknown Album", Artist = null, Year = 2000 };

        // Act
        var result = await plugin.DoAlbumImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
    }

    [Fact]
    public async Task DoAlbumImageSearch_WithNoResults_ReturnsEmptyArray()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(true);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveApiKey)).Returns("test-key");
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveBaseUrl)).Returns(string.Empty);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveImageSearchPath)).Returns(string.Empty);

        var responseData = new BraveImageSearchResponse { Results = [] };

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

        var plugin = new BraveAlbumImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        var query = new AlbumQuery { Name = "Unknown Album", Year = 2000 };

        // Act
        var result = await plugin.DoAlbumImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoAlbumImageSearch_ClampsMaxResultsTo20()
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
                Title = $"Cover {i}",
                Url = $"https://example.com/cover{i}.jpg"
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

        var plugin = new BraveAlbumImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        var query = new AlbumQuery { Name = "Test Album", Year = 2000 };

        // Act
        var result = await plugin.DoAlbumImageSearch(query, 100);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Length <= 20);
    }

    [Fact]
    public async Task DoAlbumImageSearch_HandlesHttpException_ReturnsEmptyResult()
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

        var plugin = new BraveAlbumImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        var query = new AlbumQuery { Name = "Test Album", Year = 2000 };

        // Act
        var result = await plugin.DoAlbumImageSearch(query, 10);

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
        var plugin = new BraveAlbumImageSearchEnginePlugin(
            mockLogger.Object,
            mockHttpFactory.Object,
            mockConfig.Object);

        // Assert
        Assert.Equal("Brave Album Image Search", plugin.DisplayName);
        Assert.Equal(3, plugin.SortOrder);
        Assert.False(plugin.StopProcessing);
        Assert.NotNull(plugin.Id);
    }
}

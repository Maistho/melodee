using System.Net;
using System.Text.Json;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Plugins.SearchEngine.Brave;
using Moq;

namespace Melodee.Tests.Common.Common.Plugins.SearchEngine.Brave;

public class BraveSearchClientTests
{
    [Fact]
    public async Task SearchImagesAsync_WhenDisabled_ReturnsNull()
    {
        // Arrange
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(false);

        var mockHttpFactory = new Mock<IHttpClientFactory>();

        var client = new BraveSearchClient(mockHttpFactory.Object, mockConfig.Object);

        // Act
        var result = await client.SearchImagesAsync("test query", 10);

        // Assert
        Assert.Null(result);
        mockHttpFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SearchImagesAsync_WithEmptyApiKey_ReturnsNull()
    {
        // Arrange
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(true);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveApiKey)).Returns(string.Empty);

        var mockHttpFactory = new Mock<IHttpClientFactory>();

        var client = new BraveSearchClient(mockHttpFactory.Object, mockConfig.Object);

        // Act
        var result = await client.SearchImagesAsync("test query", 10);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchImagesAsync_WithValidResponse_DeserializesCorrectly()
    {
        // Arrange
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(true);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveApiKey)).Returns("test-api-key");
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveBaseUrl)).Returns("https://api.test.brave.com");
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveImageSearchPath)).Returns("/res/v1/images/search");

        var responseData = new BraveImageSearchResponse
        {
            Results = [new() { Title = "Test Image", Url = "https://example.com/image.jpg" }]
        };
        var jsonResponse = JsonSerializer.Serialize(responseData);

        var handlerStub = new HttpHandlerStubDelegate((request, _) =>
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };
            return Task.FromResult(response);
        });

        var mockHttpFactory = new Mock<IHttpClientFactory>();
        mockHttpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerStub));

        var client = new BraveSearchClient(mockHttpFactory.Object, mockConfig.Object);

        // Act
        var result = await client.SearchImagesAsync("test query", 10);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Results);
        Assert.Single(result.Results);
        Assert.Equal("Test Image", result.Results[0].Title);
    }

    [Fact]
    public async Task SearchImagesAsync_WithNonSuccessStatusCode_ReturnsNull()
    {
        // Arrange
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(true);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveApiKey)).Returns("test-api-key");
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveBaseUrl)).Returns(string.Empty);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveImageSearchPath)).Returns(string.Empty);

        var handlerStub = new HttpHandlerStubDelegate((request, _) =>
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };
            return Task.FromResult(response);
        });

        var mockHttpFactory = new Mock<IHttpClientFactory>();
        mockHttpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerStub));

        var client = new BraveSearchClient(mockHttpFactory.Object, mockConfig.Object);

        // Act
        var result = await client.SearchImagesAsync("test query", 10);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchImagesAsync_ClampsCountTo50()
    {
        // Arrange
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(true);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveApiKey)).Returns("test-api-key");
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveBaseUrl)).Returns(string.Empty);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveImageSearchPath)).Returns(string.Empty);

        string? capturedUrl = null;
        var handlerStub = new HttpHandlerStubDelegate((request, _) =>
        {
            capturedUrl = request.RequestUri?.ToString();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new BraveImageSearchResponse()))
            };
            return Task.FromResult(response);
        });

        var mockHttpFactory = new Mock<IHttpClientFactory>();
        mockHttpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerStub));

        var client = new BraveSearchClient(mockHttpFactory.Object, mockConfig.Object);

        // Act
        await client.SearchImagesAsync("test query", 100);

        // Assert
        Assert.NotNull(capturedUrl);
        Assert.Contains("count=50", capturedUrl);
    }

    [Fact]
    public async Task SearchImagesAsync_ClampsCountToMinimum1()
    {
        // Arrange
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(true);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveApiKey)).Returns("test-api-key");
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveBaseUrl)).Returns(string.Empty);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveImageSearchPath)).Returns(string.Empty);

        string? capturedUrl = null;
        var handlerStub = new HttpHandlerStubDelegate((request, _) =>
        {
            capturedUrl = request.RequestUri?.ToString();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new BraveImageSearchResponse()))
            };
            return Task.FromResult(response);
        });

        var mockHttpFactory = new Mock<IHttpClientFactory>();
        mockHttpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerStub));

        var client = new BraveSearchClient(mockHttpFactory.Object, mockConfig.Object);

        // Act
        await client.SearchImagesAsync("test query", 0);

        // Assert
        Assert.NotNull(capturedUrl);
        Assert.Contains("count=1", capturedUrl);
    }

    [Fact]
    public async Task SearchImagesAsync_SetsCorrectHeaders()
    {
        // Arrange
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(true);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveApiKey)).Returns("test-api-key");
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveBaseUrl)).Returns(string.Empty);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveImageSearchPath)).Returns(string.Empty);

        HttpRequestMessage? capturedRequest = null;
        var handlerStub = new HttpHandlerStubDelegate((request, _) =>
        {
            capturedRequest = request;
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new BraveImageSearchResponse()))
            };
            return Task.FromResult(response);
        });

        var mockHttpFactory = new Mock<IHttpClientFactory>();
        mockHttpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerStub));

        var client = new BraveSearchClient(mockHttpFactory.Object, mockConfig.Object);

        // Act
        await client.SearchImagesAsync("test query", 10);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("Accept"));
        Assert.True(capturedRequest.Headers.Contains("X-Subscription-Token"));
        Assert.Equal("test-api-key", capturedRequest.Headers.GetValues("X-Subscription-Token").First());
    }

    [Fact]
    public async Task SearchImagesAsync_EscapesQueryString()
    {
        // Arrange
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(true);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveApiKey)).Returns("test-api-key");
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveBaseUrl)).Returns(string.Empty);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveImageSearchPath)).Returns(string.Empty);

        string? capturedUrl = null;
        var handlerStub = new HttpHandlerStubDelegate((request, _) =>
        {
            capturedUrl = request.RequestUri?.ToString();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new BraveImageSearchResponse()))
            };
            return Task.FromResult(response);
        });

        var mockHttpFactory = new Mock<IHttpClientFactory>();
        mockHttpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerStub));

        var client = new BraveSearchClient(mockHttpFactory.Object, mockConfig.Object);

        // Act
        await client.SearchImagesAsync("test query with spaces & special chars", 10);

        // Assert
        Assert.NotNull(capturedUrl);
        Assert.Contains("q=", capturedUrl);
        // Should be URL encoded (spaces and special characters encoded)
        // Uri.EscapeDataString encodes spaces as %20, but HttpClient may convert them to +
        Assert.DoesNotContain("spaces & special", capturedUrl); // Should be encoded, not raw
    }

    [Fact]
    public async Task SearchImagesAsync_PropagatesCancellation()
    {
        // Arrange
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)).Returns(true);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveApiKey)).Returns("test-api-key");
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveBaseUrl)).Returns(string.Empty);
        mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SearchEngineBraveImageSearchPath)).Returns(string.Empty);

        var handlerStub = new HttpHandlerStubDelegate((request, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            return Task.FromResult(response);
        });

        var mockHttpFactory = new Mock<IHttpClientFactory>();
        mockHttpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerStub));

        var client = new BraveSearchClient(mockHttpFactory.Object, mockConfig.Object);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        // TaskCanceledException is a subclass of OperationCanceledException
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await client.SearchImagesAsync("test query", 10, cts.Token);
        });
    }
}

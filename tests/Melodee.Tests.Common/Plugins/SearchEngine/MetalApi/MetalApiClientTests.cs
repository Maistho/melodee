using System.Net;
using System.Text;
using Melodee.Common.Plugins.SearchEngine.MetalApi;
using Melodee.Tests.Common.Common.Services;
using Moq;
using Moq.Protected;

namespace Melodee.Tests.Common.Plugins.SearchEngine.MetalApi;

public class MetalApiClientTests : ServiceTestBase
{
    private MetalApiClient CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var options = new MetalApiOptions
        {
            BaseUrl = "https://www.metal-api.dev",
            Enabled = true,
            Timeout = TimeSpan.FromSeconds(30)
        };

        return new MetalApiClient(httpClient, Logger, options);
    }

    [Fact]
    public async Task SearchBandsByNameAsync_WhenDisabled_ReturnsNull()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(handlerMock.Object);
        var options = new MetalApiOptions { Enabled = false };
        var client = new MetalApiClient(httpClient, Logger, options);

        // Act
        var result = await client.SearchBandsByNameAsync("Metallica");

        // Assert
        Assert.Null(result);
        
        // Verify no HTTP calls were made
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SearchBandsByNameAsync_WithEmptyName_ReturnsNull()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var client = CreateClient(handlerMock.Object);

        // Act
        var result = await client.SearchBandsByNameAsync("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchBandsByNameAsync_WithArrayResponse_ReturnsArray()
    {
        // Arrange
        var jsonResponse = @"[
            {
                ""id"": ""1"",
                ""name"": ""Metallica"",
                ""genre"": ""Thrash Metal"",
                ""country"": ""USA"",
                ""link"": ""https://metal-api.dev/bands/1""
            }
        ]";

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

        var client = CreateClient(handlerMock.Object);

        // Act
        var result = await client.SearchBandsByNameAsync("Metallica");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("1", result[0].Id);
        Assert.Equal("Metallica", result[0].Name);
        Assert.Equal("Thrash Metal", result[0].Genre);
    }

    [Fact]
    public async Task SearchBandsByNameAsync_WithSingleObjectResponse_WrapsInArray()
    {
        // Arrange
        var jsonResponse = @"{
            ""id"": ""1"",
            ""name"": ""Metallica"",
            ""genre"": ""Thrash Metal"",
            ""country"": ""USA"",
            ""link"": ""https://metal-api.dev/bands/1""
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

        var client = CreateClient(handlerMock.Object);

        // Act
        var result = await client.SearchBandsByNameAsync("Metallica");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("1", result[0].Id);
        Assert.Equal("Metallica", result[0].Name);
    }

    [Fact]
    public async Task SearchBandsByNameAsync_With500Error_ReturnsNull()
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
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent(@"{""traceId"": ""123""}", Encoding.UTF8, "application/json")
            });

        var client = CreateClient(handlerMock.Object);

        // Act
        var result = await client.SearchBandsByNameAsync("Metallica");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAlbumsByTitleAsync_WithValidResponse_ReturnsResults()
    {
        // Arrange
        var jsonResponse = @"[
            {
                ""id"": ""100"",
                ""title"": ""Master of Puppets"",
                ""band"": {
                    ""id"": ""1"",
                    ""name"": ""Metallica"",
                    ""link"": ""https://metal-api.dev/bands/1""
                },
                ""type"": ""Full-length"",
                ""date"": ""1986-03-03"",
                ""link"": ""https://metal-api.dev/albums/100""
            }
        ]";

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

        var client = CreateClient(handlerMock.Object);

        // Act
        var result = await client.SearchAlbumsByTitleAsync("Master of Puppets");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("100", result[0].Id);
        Assert.Equal("Master of Puppets", result[0].Title);
        Assert.Equal("Metallica", result[0].Band?.Name);
    }

    [Fact]
    public async Task GetAlbumAsync_WithValidId_ReturnsAlbum()
    {
        // Arrange
        var jsonResponse = @"{
            ""id"": ""100"",
            ""name"": ""Master of Puppets"",
            ""type"": ""Full-length"",
            ""releaseDate"": ""1986-03-03"",
            ""coverUrl"": ""https://metal-api.dev/images/100.jpg"",
            ""band"": {
                ""id"": ""1"",
                ""name"": ""Metallica""
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

        var client = CreateClient(handlerMock.Object);

        // Act
        var result = await client.GetAlbumAsync("100");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("100", result.Id);
        Assert.Equal("Master of Puppets", result.Name);
        Assert.Equal("https://metal-api.dev/images/100.jpg", result.CoverUrl);
        Assert.Equal("Metallica", result.Band?.Name);
    }

    [Fact]
    public async Task GetAlbumAsync_WithEmptyId_ReturnsNull()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var client = CreateClient(handlerMock.Object);

        // Act
        var result = await client.GetAlbumAsync("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchBandsByNameAsync_WithCancellationToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        var client = CreateClient(handlerMock.Object);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert - TaskCanceledException inherits from OperationCanceledException
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.SearchBandsByNameAsync("Metallica", cts.Token));
    }

    [Fact]
    public async Task SearchBandsByNameAsync_WithHttpRequestException_ReturnsNull()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var client = CreateClient(handlerMock.Object);

        // Act
        var result = await client.SearchBandsByNameAsync("Metallica");

        // Assert
        Assert.Null(result);
    }
}

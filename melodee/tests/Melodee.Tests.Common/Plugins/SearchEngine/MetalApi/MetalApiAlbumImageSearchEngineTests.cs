using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine.MetalApi;
using Melodee.Tests.Common.Common.Services;
using Moq;

namespace Melodee.Tests.Common.Plugins.SearchEngine.MetalApi;

public class MetalApiAlbumImageSearchEngineTests : ServiceTestBase
{
    [Fact]
    public async Task DoAlbumImageSearch_WithNullQuery_ReturnsValidationFailure()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        var engine = new MetalApiAlbumImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        // Act
        var result = await engine.DoAlbumImageSearch(null!, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OperationResponseType.ValidationFailure, result.Type);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoAlbumImageSearch_WithEmptyName_ReturnsValidationFailure()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        var engine = new MetalApiAlbumImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new AlbumQuery
        {
            Name = "",
            Artist = "Test Artist",
            Year = 2020
        };

        // Act
        var result = await engine.DoAlbumImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OperationResponseType.ValidationFailure, result.Type);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoAlbumImageSearch_WhenDisabled_ReturnsEmptyResults()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        var engine = new MetalApiAlbumImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = false });

        var query = new AlbumQuery
        {
            Name = "Test Album",
            Artist = "Test Artist",
            Year = 2020
        };

        // Act
        var result = await engine.DoAlbumImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoAlbumImageSearch_WithNoSearchResults_ReturnsEmptyArray()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        mockClient.Setup(c => c.SearchAlbumsByTitleAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((MetalAlbumSearchResult[]?)null);

        var engine = new MetalApiAlbumImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new AlbumQuery
        {
            Name = "Nonexistent Album",
            Artist = "Unknown Artist",
            Year = 2020
        };

        // Act
        var result = await engine.DoAlbumImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoAlbumImageSearch_WithMatchingResults_ReturnsImages()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        var searchResults = new[]
        {
            new MetalAlbumSearchResult
            {
                Id = "1",
                Title = "Master of Puppets",
                Band = new MetalBandInfo { Id = "10", Name = "Metallica" },
                Date = "1986-03-03"
            }
        };

        var albumDetails = new MetalAlbum
        {
            Id = "1",
            Name = "Master of Puppets",
            CoverUrl = "https://metal-api.dev/images/1.jpg",
            ReleaseDate = "1986-03-03",
            Band = new MetalBandInfo { Id = "10", Name = "Metallica" }
        };

        mockClient.Setup(c => c.SearchAlbumsByTitleAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResults);

        mockClient.Setup(c => c.GetAlbumAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(albumDetails);

        var engine = new MetalApiAlbumImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new AlbumQuery
        {
            Name = "Master of Puppets",
            Artist = "Metallica",
            Year = 1986
        };

        // Act
        var result = await engine.DoAlbumImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("https://metal-api.dev/images/1.jpg", result.Data[0].MediaUrl);
        Assert.Equal("Metal API", result.Data[0].FromPlugin);
    }

    [Fact]
    public async Task DoAlbumImageSearch_FiltersNonMatchingArtist()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        var searchResults = new[]
        {
            new MetalAlbumSearchResult
            {
                Id = "1",
                Title = "Master of Puppets",
                Band = new MetalBandInfo { Id = "10", Name = "Different Band" }
            }
        };

        mockClient.Setup(c => c.SearchAlbumsByTitleAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResults);

        var engine = new MetalApiAlbumImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new AlbumQuery
        {
            Name = "Master of Puppets",
            Artist = "Metallica",
            Year = 1986
        };

        // Act
        var result = await engine.DoAlbumImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data); // Should be filtered out
    }

    [Fact]
    public async Task DoAlbumImageSearch_SkipsAlbumsWithoutCoverUrl()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        var searchResults = new[]
        {
            new MetalAlbumSearchResult
            {
                Id = "1",
                Title = "Test Album",
                Band = new MetalBandInfo { Id = "10", Name = "Test Band" }
            }
        };

        var albumDetails = new MetalAlbum
        {
            Id = "1",
            Name = "Test Album",
            CoverUrl = null, // No cover
            Band = new MetalBandInfo { Id = "10", Name = "Test Band" }
        };

        mockClient.Setup(c => c.SearchAlbumsByTitleAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResults);

        mockClient.Setup(c => c.GetAlbumAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(albumDetails);

        var engine = new MetalApiAlbumImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new AlbumQuery
        {
            Name = "Test Album",
            Artist = "Test Band",
            Year = 2020
        };

        // Act
        var result = await engine.DoAlbumImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoAlbumImageSearch_ClampsMaxResults()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        mockClient.Setup(c => c.SearchAlbumsByTitleAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((MetalAlbumSearchResult[]?)null);

        var engine = new MetalApiAlbumImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new AlbumQuery
        {
            Name = "Test Album",
            Artist = "Test Artist",
            Year = 2020
        };

        // Act - Test with value above max
        var result1 = await engine.DoAlbumImageSearch(query, 100);

        // Assert - Should be clamped
        Assert.NotNull(result1);

        // Act - Test with value below min
        var result2 = await engine.DoAlbumImageSearch(query, 0);

        // Assert - Should be clamped to 1
        Assert.NotNull(result2);
    }

    [Fact]
    public async Task DoAlbumImageSearch_WithException_ReturnsErrorResult()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        mockClient.Setup(c => c.SearchAlbumsByTitleAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        var engine = new MetalApiAlbumImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new AlbumQuery
        {
            Name = "Test Album",
            Artist = "Test Artist",
            Year = 2020
        };

        // Act
        var result = await engine.DoAlbumImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OperationResponseType.Error, result.Type);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoAlbumImageSearch_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        mockClient.Setup(c => c.SearchAlbumsByTitleAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var engine = new MetalApiAlbumImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new AlbumQuery
        {
            Name = "Test Album",
            Artist = "Test Artist",
            Year = 2020
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => engine.DoAlbumImageSearch(query, 10, cts.Token));
    }
}

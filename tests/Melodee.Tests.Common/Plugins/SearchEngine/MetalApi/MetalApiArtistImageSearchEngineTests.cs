using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine.MetalApi;
using Melodee.Tests.Common.Services;
using Moq;

namespace Melodee.Tests.Common.Plugins.SearchEngine.MetalApi;

public class MetalApiArtistImageSearchEngineTests : ServiceTestBase
{
    [Fact]
    public async Task DoArtistImageSearch_WithNullQuery_ReturnsValidationFailure()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        var engine = new MetalApiArtistImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        // Act
        var result = await engine.DoArtistImageSearch(null!, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OperationResponseType.ValidationFailure, result.Type);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoArtistImageSearch_WithEmptyName_ReturnsValidationFailure()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        var engine = new MetalApiArtistImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new ArtistQuery
        {
            Name = ""
        };

        // Act
        var result = await engine.DoArtistImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OperationResponseType.ValidationFailure, result.Type);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoArtistImageSearch_WhenDisabled_ReturnsEmptyResults()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        var engine = new MetalApiArtistImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = false });

        var query = new ArtistQuery
        {
            Name = "Test Artist"
        };

        // Act
        var result = await engine.DoArtistImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoArtistImageSearch_WithNoBandResults_ReturnsEmptyArray()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        mockClient.Setup(c => c.SearchBandsByNameAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((MetalBandSearchResult[]?)null);

        var engine = new MetalApiArtistImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new ArtistQuery
        {
            Name = "Unknown Artist"
        };

        // Act
        var result = await engine.DoArtistImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoArtistImageSearch_WithMatchingBand_ReturnsAlbumArtAsFallback()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        var bandResults = new[]
        {
            new MetalBandSearchResult
            {
                Id = "1",
                Name = "Metallica",
                Genre = "Thrash Metal"
            }
        };

        var albumResults = new[]
        {
            new MetalAlbumSearchResult
            {
                Id = "100",
                Title = "Master of Puppets",
                Band = new MetalBandInfo { Id = "1", Name = "Metallica" }
            }
        };

        var albumDetails = new MetalAlbum
        {
            Id = "100",
            Name = "Master of Puppets",
            CoverUrl = "https://metal-api.dev/images/100.jpg",
            Band = new MetalBandInfo { Id = "1", Name = "Metallica" }
        };

        mockClient.Setup(c => c.SearchBandsByNameAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(bandResults);

        mockClient.Setup(c => c.SearchAlbumsByTitleAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(albumResults);

        mockClient.Setup(c => c.GetAlbumAsync("100", It.IsAny<CancellationToken>()))
            .ReturnsAsync(albumDetails);

        var engine = new MetalApiArtistImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new ArtistQuery
        {
            Name = "Metallica"
        };

        // Act
        var result = await engine.DoArtistImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("https://metal-api.dev/images/100.jpg", result.Data[0].MediaUrl);
        Assert.Equal("Metal API", result.Data[0].FromPlugin);
        Assert.Contains("album art", result.Data[0].Title);
    }

    [Fact]
    public async Task DoArtistImageSearch_WithNoAlbums_ReturnsEmptyArray()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        var bandResults = new[]
        {
            new MetalBandSearchResult
            {
                Id = "1",
                Name = "Test Band",
                Genre = "Test Genre"
            }
        };

        mockClient.Setup(c => c.SearchBandsByNameAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(bandResults);

        mockClient.Setup(c => c.SearchAlbumsByTitleAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((MetalAlbumSearchResult[]?)null);

        var engine = new MetalApiArtistImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new ArtistQuery
        {
            Name = "Test Band"
        };

        // Act
        var result = await engine.DoArtistImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoArtistImageSearch_SkipsAlbumsWithoutCoverUrl()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        var bandResults = new[]
        {
            new MetalBandSearchResult
            {
                Id = "1",
                Name = "Test Band"
            }
        };

        var albumResults = new[]
        {
            new MetalAlbumSearchResult
            {
                Id = "100",
                Title = "Test Album",
                Band = new MetalBandInfo { Id = "1", Name = "Test Band" }
            }
        };

        var albumDetails = new MetalAlbum
        {
            Id = "100",
            Name = "Test Album",
            CoverUrl = null, // No cover
            Band = new MetalBandInfo { Id = "1", Name = "Test Band" }
        };

        mockClient.Setup(c => c.SearchBandsByNameAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(bandResults);

        mockClient.Setup(c => c.SearchAlbumsByTitleAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(albumResults);

        mockClient.Setup(c => c.GetAlbumAsync("100", It.IsAny<CancellationToken>()))
            .ReturnsAsync(albumDetails);

        var engine = new MetalApiArtistImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new ArtistQuery
        {
            Name = "Test Band"
        };

        // Act
        var result = await engine.DoArtistImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoArtistImageSearch_LimitsAlbumProcessing()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        var bandResults = new[]
        {
            new MetalBandSearchResult { Id = "1", Name = "Test Band" }
        };

        // Create 10 albums (should only process 5)
        var albumResults = Enumerable.Range(1, 10)
            .Select(i => new MetalAlbumSearchResult
            {
                Id = i.ToString(),
                Title = $"Album {i}",
                Band = new MetalBandInfo { Id = "1", Name = "Test Band" }
            })
            .ToArray();

        mockClient.Setup(c => c.SearchBandsByNameAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(bandResults);

        mockClient.Setup(c => c.SearchAlbumsByTitleAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(albumResults);

        // Setup to return albums with covers
        foreach (var album in albumResults.Take(5))
        {
            mockClient.Setup(c => c.GetAlbumAsync(album.Id!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MetalAlbum
                {
                    Id = album.Id,
                    Name = album.Title,
                    CoverUrl = $"https://test.com/{album.Id}.jpg",
                    Band = new MetalBandInfo { Id = "1", Name = "Test Band" }
                });
        }

        var engine = new MetalApiArtistImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new ArtistQuery
        {
            Name = "Test Band"
        };

        // Act
        var result = await engine.DoArtistImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(5, result.Data.Length); // Should be limited to 5
    }

    [Fact]
    public async Task DoArtistImageSearch_RespectsMaxResults()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        var bandResults = new[]
        {
            new MetalBandSearchResult { Id = "1", Name = "Test Band" }
        };

        var albumResults = Enumerable.Range(1, 5)
            .Select(i => new MetalAlbumSearchResult
            {
                Id = i.ToString(),
                Title = $"Album {i}",
                Band = new MetalBandInfo { Id = "1", Name = "Test Band" }
            })
            .ToArray();

        mockClient.Setup(c => c.SearchBandsByNameAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(bandResults);

        mockClient.Setup(c => c.SearchAlbumsByTitleAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(albumResults);

        foreach (var album in albumResults)
        {
            mockClient.Setup(c => c.GetAlbumAsync(album.Id!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MetalAlbum
                {
                    Id = album.Id,
                    Name = album.Title,
                    CoverUrl = $"https://test.com/{album.Id}.jpg",
                    Band = new MetalBandInfo { Id = "1", Name = "Test Band" }
                });
        }

        var engine = new MetalApiArtistImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new ArtistQuery
        {
            Name = "Test Band"
        };

        // Act
        var result = await engine.DoArtistImageSearch(query, 3);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Length); // Should respect maxResults
    }

    [Fact]
    public async Task DoArtistImageSearch_WithException_ReturnsErrorResult()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        mockClient.Setup(c => c.SearchBandsByNameAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        var engine = new MetalApiArtistImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new ArtistQuery
        {
            Name = "Test Artist"
        };

        // Act
        var result = await engine.DoArtistImageSearch(query, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OperationResponseType.Error, result.Type);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task DoArtistImageSearch_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockClient = new Mock<IMetalApiClient>();

        mockClient.Setup(c => c.SearchBandsByNameAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var engine = new MetalApiArtistImageSearchEngine(
            mockClient.Object,
            Logger,
            new MetalApiOptions { Enabled = true });

        var query = new ArtistQuery
        {
            Name = "Test Artist"
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => engine.DoArtistImageSearch(query, 10, cts.Token));
    }
}

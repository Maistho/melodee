using Melodee.Common.Plugins.SearchEngine.MetalApi;
using Melodee.Common.Models.SearchEngines;

namespace Melodee.Tests.Common.Plugins.SearchEngine.MetalApi;

public class MetalApiImageMapperTests
{
    [Fact]
    public void FromAlbum_WithNullCoverUrl_ReturnsNull()
    {
        // Arrange
        var album = new MetalAlbum
        {
            Id = "1",
            Name = "Test Album",
            CoverUrl = null
        };

        // Act
        var result = MetalApiImageMapper.FromAlbum(album, "Test Artist", isExactMatch: true, isArtistFallback: false);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromAlbum_WithEmptyCoverUrl_ReturnsNull()
    {
        // Arrange
        var album = new MetalAlbum
        {
            Id = "1",
            Name = "Test Album",
            CoverUrl = ""
        };

        // Act
        var result = MetalApiImageMapper.FromAlbum(album, "Test Artist", isExactMatch: true, isArtistFallback: false);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromAlbum_WithValidAlbum_MapsCorrectly()
    {
        // Arrange
        var album = new MetalAlbum
        {
            Id = "1",
            Name = "Master of Puppets",
            CoverUrl = "https://metal-api.dev/images/1.jpg",
            ReleaseDate = "1986-03-03",
            Band = new MetalBandInfo
            {
                Id = "10",
                Name = "Metallica"
            }
        };

        // Act
        var result = MetalApiImageMapper.FromAlbum(album, "Metallica", isExactMatch: true, isArtistFallback: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Metal API", result.FromPlugin);
        Assert.Equal("https://metal-api.dev/images/1.jpg", result.MediaUrl);
        Assert.Equal("https://metal-api.dev/images/1.jpg", result.ThumbnailUrl);
        Assert.Equal("Master of Puppets", result.Title);
        Assert.Equal(15, result.Rank); // Exact match rank
        Assert.NotNull(result.ReleaseDate);
        Assert.Equal(1986, result.ReleaseDate.Value.Year);
    }

    [Fact]
    public void FromAlbum_WithExactMatch_HasHighRank()
    {
        // Arrange
        var album = new MetalAlbum
        {
            Id = "1",
            Name = "Test Album",
            CoverUrl = "https://test.com/image.jpg"
        };

        // Act
        var result = MetalApiImageMapper.FromAlbum(album, "Test Artist", isExactMatch: true, isArtistFallback: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(15, result.Rank);
    }

    [Fact]
    public void FromAlbum_WithPartialMatch_HasMediumRank()
    {
        // Arrange
        var album = new MetalAlbum
        {
            Id = "1",
            Name = "Test Album",
            CoverUrl = "https://test.com/image.jpg"
        };

        // Act
        var result = MetalApiImageMapper.FromAlbum(album, "Test Artist", isExactMatch: false, isArtistFallback: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(8, result.Rank);
    }

    [Fact]
    public void FromAlbum_AsArtistFallback_HasLowRank()
    {
        // Arrange
        var album = new MetalAlbum
        {
            Id = "1",
            Name = "Test Album",
            CoverUrl = "https://test.com/image.jpg",
            Band = new MetalBandInfo { Name = "Test Band" }
        };

        // Act
        var result = MetalApiImageMapper.FromAlbum(album, "Test Band", isExactMatch: false, isArtistFallback: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Rank);
        Assert.Equal("Test Band album art", result.Title);
    }

    [Fact]
    public void FromAlbum_WithInvalidReleaseDate_SetsNullReleaseDate()
    {
        // Arrange
        var album = new MetalAlbum
        {
            Id = "1",
            Name = "Test Album",
            CoverUrl = "https://test.com/image.jpg",
            ReleaseDate = "invalid-date"
        };

        // Act
        var result = MetalApiImageMapper.FromAlbum(album, "Test Artist", isExactMatch: true, isArtistFallback: false);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.ReleaseDate);
    }

    [Fact]
    public void DeduplicateAndSort_WithDuplicateUrls_RemovesDuplicates()
    {
        // Arrange
        var results = new[]
        {
            new ImageSearchResult
            {
                FromPlugin = "Test",
                MediaUrl = "https://test.com/image.jpg",
                ThumbnailUrl = "https://test.com/image.jpg",
                Rank = 10,
                UniqueId = 1
            },
            new ImageSearchResult
            {
                FromPlugin = "Test",
                MediaUrl = "https://test.com/image.jpg", // Duplicate
                ThumbnailUrl = "https://test.com/image.jpg",
                Rank = 5,
                UniqueId = 2
            },
            new ImageSearchResult
            {
                FromPlugin = "Test",
                MediaUrl = "https://test.com/other.jpg",
                ThumbnailUrl = "https://test.com/other.jpg",
                Rank = 8,
                UniqueId = 3
            }
        };

        // Act
        var deduplicated = MetalApiImageMapper.DeduplicateAndSort(results, 10);

        // Assert
        Assert.Equal(2, deduplicated.Length);
        Assert.Contains(deduplicated, r => r.MediaUrl == "https://test.com/image.jpg" && r.Rank == 10);
        Assert.Contains(deduplicated, r => r.MediaUrl == "https://test.com/other.jpg");
    }

    [Fact]
    public void DeduplicateAndSort_WithCaseInsensitiveUrls_RemovesDuplicates()
    {
        // Arrange
        var results = new[]
        {
            new ImageSearchResult
            {
                FromPlugin = "Test",
                MediaUrl = "https://test.com/IMAGE.jpg",
                ThumbnailUrl = "https://test.com/IMAGE.jpg",
                Rank = 10,
                UniqueId = 1
            },
            new ImageSearchResult
            {
                FromPlugin = "Test",
                MediaUrl = "https://test.com/image.jpg", // Case-insensitive duplicate
                ThumbnailUrl = "https://test.com/image.jpg",
                Rank = 5,
                UniqueId = 2
            }
        };

        // Act
        var deduplicated = MetalApiImageMapper.DeduplicateAndSort(results, 10);

        // Assert
        Assert.Single(deduplicated);
        Assert.Equal(10, deduplicated[0].Rank); // Should prefer higher rank
    }

    [Fact]
    public void DeduplicateAndSort_SortsByRankDescending()
    {
        // Arrange
        var results = new[]
        {
            new ImageSearchResult
            {
                FromPlugin = "Test",
                MediaUrl = "https://test.com/a.jpg",
                ThumbnailUrl = "https://test.com/a.jpg",
                Rank = 5,
                UniqueId = 1
            },
            new ImageSearchResult
            {
                FromPlugin = "Test",
                MediaUrl = "https://test.com/b.jpg",
                ThumbnailUrl = "https://test.com/b.jpg",
                Rank = 15,
                UniqueId = 2
            },
            new ImageSearchResult
            {
                FromPlugin = "Test",
                MediaUrl = "https://test.com/c.jpg",
                ThumbnailUrl = "https://test.com/c.jpg",
                Rank = 10,
                UniqueId = 3
            }
        };

        // Act
        var sorted = MetalApiImageMapper.DeduplicateAndSort(results, 10);

        // Assert
        Assert.Equal(3, sorted.Length);
        Assert.Equal(15, sorted[0].Rank);
        Assert.Equal(10, sorted[1].Rank);
        Assert.Equal(5, sorted[2].Rank);
    }

    [Fact]
    public void DeduplicateAndSort_RespectsMaxResults()
    {
        // Arrange
        var results = new[]
        {
            new ImageSearchResult
            {
                FromPlugin = "Test",
                MediaUrl = "https://test.com/a.jpg",
                ThumbnailUrl = "https://test.com/a.jpg",
                Rank = 15,
                UniqueId = 1
            },
            new ImageSearchResult
            {
                FromPlugin = "Test",
                MediaUrl = "https://test.com/b.jpg",
                ThumbnailUrl = "https://test.com/b.jpg",
                Rank = 10,
                UniqueId = 2
            },
            new ImageSearchResult
            {
                FromPlugin = "Test",
                MediaUrl = "https://test.com/c.jpg",
                ThumbnailUrl = "https://test.com/c.jpg",
                Rank = 5,
                UniqueId = 3
            }
        };

        // Act
        var limited = MetalApiImageMapper.DeduplicateAndSort(results, 2);

        // Assert
        Assert.Equal(2, limited.Length);
        Assert.Equal(15, limited[0].Rank);
        Assert.Equal(10, limited[1].Rank);
    }

    [Fact]
    public void DeduplicateAndSort_WithNullResults_ReturnsEmptyArray()
    {
        // Act
        var result = MetalApiImageMapper.DeduplicateAndSort(null!, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void DeduplicateAndSort_WithEmptyResults_ReturnsEmptyArray()
    {
        // Arrange
        var results = Array.Empty<ImageSearchResult>();

        // Act
        var result = MetalApiImageMapper.DeduplicateAndSort(results, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}

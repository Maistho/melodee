using Melodee.Common.Plugins.SearchEngine.Brave;
using Melodee.Common.Models.SearchEngines;

namespace Melodee.Tests.Common.Common.Plugins.SearchEngine.Brave;

public class BraveImageMapperTests
{
    [Fact]
    public void ToImageSearchResult_WithNullSource_ReturnsNull()
    {
        // Act
        var result = BraveImageMapper.ToImageSearchResult(null!, "TestPlugin");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ToImageSearchResult_WithNullUrl_ReturnsNull()
    {
        // Arrange
        var braveResult = new BraveImageResult
        {
            Title = "Test Image",
            Url = null
        };

        // Act
        var result = BraveImageMapper.ToImageSearchResult(braveResult, "TestPlugin");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ToImageSearchResult_WithEmptyUrl_ReturnsNull()
    {
        // Arrange
        var braveResult = new BraveImageResult
        {
            Title = "Test Image",
            Url = ""
        };

        // Act
        var result = BraveImageMapper.ToImageSearchResult(braveResult, "TestPlugin");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ToImageSearchResult_WithValidData_MapsPropertiesCorrectly()
    {
        // Arrange
        var braveResult = new BraveImageResult
        {
            Title = "Test Image",
            Url = "https://example.com/image.jpg",
            Thumbnail = new BraveThumbnail { Src = "https://example.com/thumb.jpg" },
            Source = "example.com",
            PageUrl = "https://example.com/page",
            Properties = new BraveImageProperties
            {
                Width = 800,
                Height = 600
            }
        };

        // Act
        var result = BraveImageMapper.ToImageSearchResult(braveResult, "TestPlugin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Image", result.Title);
        Assert.Equal("https://example.com/image.jpg", result.MediaUrl);
        Assert.Equal("https://example.com/thumb.jpg", result.ThumbnailUrl);
        Assert.Equal(800, result.Width);
        Assert.Equal(600, result.Height);
        Assert.Equal("TestPlugin", result.FromPlugin);
        Assert.Equal(1, result.Rank);
    }

    [Fact]
    public void ToImageSearchResult_WithoutThumbnail_UsesPrimaryUrl()
    {
        // Arrange
        var braveResult = new BraveImageResult
        {
            Title = "Test Image",
            Url = "https://example.com/image.jpg",
            Thumbnail = null
        };

        // Act
        var result = BraveImageMapper.ToImageSearchResult(braveResult, "TestPlugin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://example.com/image.jpg", result.ThumbnailUrl);
    }

    [Fact]
    public void MapResults_WithNullResults_ReturnsEmptyArray()
    {
        // Act
        var result = BraveImageMapper.MapResults(null!, 10, "TestPlugin");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void MapResults_WithEmptyResults_ReturnsEmptyArray()
    {
        // Arrange
        var results = new List<BraveImageResult>();

        // Act
        var mappedResults = BraveImageMapper.MapResults(results, 10, "TestPlugin");

        // Assert
        Assert.NotNull(mappedResults);
        Assert.Empty(mappedResults);
    }

    [Fact]
    public void MapResults_RemovesEntriesWithNullUrls()
    {
        // Arrange
        var results = new List<BraveImageResult>
        {
            new() { Title = "Valid", Url = "https://example.com/1.jpg" },
            new() { Title = "Invalid", Url = null },
            new() { Title = "Also Valid", Url = "https://example.com/2.jpg" }
        };

        // Act
        var mappedResults = BraveImageMapper.MapResults(results, 10, "TestPlugin");

        // Assert
        Assert.NotNull(mappedResults);
        Assert.Equal(2, mappedResults.Length);
    }

    [Fact]
    public void MapResults_DeduplicatesBySameUrl()
    {
        // Arrange
        var results = new List<BraveImageResult>
        {
            new() { Title = "First", Url = "https://example.com/image.jpg" },
            new() { Title = "Duplicate", Url = "https://example.com/image.jpg" },
            new() { Title = "Different", Url = "https://example.com/other.jpg" }
        };

        // Act
        var mappedResults = BraveImageMapper.MapResults(results, 10, "TestPlugin");

        // Assert
        Assert.NotNull(mappedResults);
        Assert.Equal(2, mappedResults.Length);
    }

    [Fact]
    public void MapResults_DeduplicatesCaseInsensitive()
    {
        // Arrange
        var results = new List<BraveImageResult>
        {
            new() { Title = "First", Url = "https://example.com/Image.jpg" },
            new() { Title = "Duplicate", Url = "https://example.com/image.JPG" },
            new() { Title = "Different", Url = "https://example.com/other.jpg" }
        };

        // Act
        var mappedResults = BraveImageMapper.MapResults(results, 10, "TestPlugin");

        // Assert
        Assert.NotNull(mappedResults);
        Assert.Equal(2, mappedResults.Length);
    }

    [Fact]
    public void MapResults_RespectsMaxResults()
    {
        // Arrange
        var results = new List<BraveImageResult>();
        for (int i = 0; i < 20; i++)
        {
            results.Add(new BraveImageResult
            {
                Title = $"Image {i}",
                Url = $"https://example.com/image{i}.jpg"
            });
        }

        // Act
        var mappedResults = BraveImageMapper.MapResults(results, 5, "TestPlugin");

        // Assert
        Assert.NotNull(mappedResults);
        Assert.Equal(5, mappedResults.Length);
    }

    [Fact]
    public void MapResults_PreservesOriginalOrder()
    {
        // Arrange
        var results = new List<BraveImageResult>
        {
            new() { Title = "First", Url = "https://example.com/1.jpg" },
            new() { Title = "Second", Url = "https://example.com/2.jpg" },
            new() { Title = "Third", Url = "https://example.com/3.jpg" }
        };

        // Act
        var mappedResults = BraveImageMapper.MapResults(results, 10, "TestPlugin");

        // Assert
        Assert.NotNull(mappedResults);
        Assert.Equal(3, mappedResults.Length);
        Assert.Equal("First", mappedResults[0].Title);
        Assert.Equal("Second", mappedResults[1].Title);
        Assert.Equal("Third", mappedResults[2].Title);
    }
}

using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee.Models;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

public class SearchRequestTests
{
    #region Constructor Tests

    [Fact]
    public void SearchRequest_WithAllParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        var artistId = Guid.NewGuid();

        // Act
        var request = new SearchRequest(
            "test query",
            "Artists,Albums",
            2, // AlbumPage
            3, // ArtistPage
            4, // SongPage
            25, // PageSize
            "Name",
            "ASC",
            artistId);

        // Assert
        request.Query.Should().Be("test query");
        request.Type.Should().Be("Artists,Albums");
        request.AlbumPage.Should().Be(2);
        request.ArtistPage.Should().Be(3);
        request.SongPage.Should().Be(4);
        request.PageSize.Should().Be(25);
        request.SortBy.Should().Be("Name");
        request.SortOrder.Should().Be("ASC");
        request.FilterByArtistId.Should().Be(artistId);
    }

    [Fact]
    public void SearchRequest_WithNullOptionalParameters_HasNullValues()
    {
        // Arrange & Act
        var request = new SearchRequest("query", null, null, null, null, null, null, null, null);

        // Assert
        request.Query.Should().Be("query");
        request.Type.Should().BeNull();
        request.AlbumPage.Should().BeNull();
        request.ArtistPage.Should().BeNull();
        request.SongPage.Should().BeNull();
        request.PageSize.Should().BeNull();
        request.SortBy.Should().BeNull();
        request.SortOrder.Should().BeNull();
        request.FilterByArtistId.Should().BeNull();
    }

    #endregion

    #region Default Page Value Tests

    [Fact]
    public void AlbumPageValue_WhenNull_ReturnsOne()
    {
        // Arrange
        var request = new SearchRequest("query", null, null, null, null, null, null, null, null);

        // Act & Assert
        request.AlbumPageValue.Should().Be(1);
    }

    [Fact]
    public void AlbumPageValue_WhenSet_ReturnsValue()
    {
        // Arrange
        var request = new SearchRequest("query", null, 5, null, null, null, null, null, null);

        // Act & Assert
        request.AlbumPageValue.Should().Be(5);
    }

    [Fact]
    public void ArtistPageValue_WhenNull_ReturnsOne()
    {
        // Arrange
        var request = new SearchRequest("query", null, null, null, null, null, null, null, null);

        // Act & Assert
        request.ArtistPageValue.Should().Be(1);
    }

    [Fact]
    public void ArtistPageValue_WhenSet_ReturnsValue()
    {
        // Arrange
        var request = new SearchRequest("query", null, null, 7, null, null, null, null, null);

        // Act & Assert
        request.ArtistPageValue.Should().Be(7);
    }

    [Fact]
    public void SongPageValue_WhenNull_ReturnsOne()
    {
        // Arrange
        var request = new SearchRequest("query", null, null, null, null, null, null, null, null);

        // Act & Assert
        request.SongPageValue.Should().Be(1);
    }

    [Fact]
    public void SongPageValue_WhenSet_ReturnsValue()
    {
        // Arrange
        var request = new SearchRequest("query", null, null, null, 10, null, null, null, null);

        // Act & Assert
        request.SongPageValue.Should().Be(10);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void SearchRequest_Equality_WorksCorrectly()
    {
        // Arrange
        var request1 = new SearchRequest("query", "Artists", 1, 1, 1, 10, "Name", "ASC", null);
        var request2 = new SearchRequest("query", "Artists", 1, 1, 1, 10, "Name", "ASC", null);
        var request3 = new SearchRequest("different", "Artists", 1, 1, 1, 10, "Name", "ASC", null);

        // Assert
        request1.Should().Be(request2);
        request1.Should().NotBe(request3);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void SearchRequest_EmptyQuery_IsAccepted()
    {
        // Arrange & Act
        var request = new SearchRequest("", null, null, null, null, null, null, null, null);

        // Assert
        request.Query.Should().BeEmpty();
    }

    [Fact]
    public void SearchRequest_WhitespaceQuery_IsAccepted()
    {
        // Arrange & Act
        var request = new SearchRequest("   ", null, null, null, null, null, null, null, null);

        // Assert
        request.Query.Should().Be("   ");
    }

    [Fact]
    public void SearchRequest_ZeroPageValue_IsAccepted()
    {
        // Arrange & Act
        var request = new SearchRequest("query", null, 0, 0, 0, null, null, null, null);

        // Assert
        request.AlbumPageValue.Should().Be(0);
        request.ArtistPageValue.Should().Be(0);
        request.SongPageValue.Should().Be(0);
    }

    [Fact]
    public void SearchRequest_NegativePageValue_IsAccepted()
    {
        // Arrange & Act - The model accepts negative values; validation should be done elsewhere
        var request = new SearchRequest("query", null, -1, -1, -1, null, null, null, null);

        // Assert
        request.AlbumPageValue.Should().Be(-1);
        request.ArtistPageValue.Should().Be(-1);
        request.SongPageValue.Should().Be(-1);
    }

    #endregion
}

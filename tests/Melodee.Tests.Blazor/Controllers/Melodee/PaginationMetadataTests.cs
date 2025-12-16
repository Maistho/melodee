using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Xunit;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

public class PaginationMetadataTests
{
    #region Constructor Tests

    [Fact]
    public void PaginationMetadata_SetsAllPropertiesCorrectly()
    {
        // Arrange & Act
        var metadata = new PaginationMetadata(100, 10, 2, 10);

        // Assert
        metadata.TotalCount.Should().Be(100);
        metadata.PageSize.Should().Be(10);
        metadata.CurrentPage.Should().Be(2);
        metadata.TotalPages.Should().Be(10);
    }

    #endregion

    #region HasPrevious Tests

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(5, true)]
    [InlineData(10, true)]
    public void HasPrevious_ReturnsCorrectValue(int currentPage, bool expectedHasPrevious)
    {
        // Arrange
        var metadata = new PaginationMetadata(100, 10, currentPage, 10);

        // Assert
        metadata.HasPrevious.Should().Be(expectedHasPrevious);
    }

    [Fact]
    public void HasPrevious_OnFirstPage_ReturnsFalse()
    {
        // Arrange
        var metadata = new PaginationMetadata(50, 10, 1, 5);

        // Assert
        metadata.HasPrevious.Should().BeFalse();
    }

    #endregion

    #region HasNext Tests

    [Theory]
    [InlineData(1, 10, true)]
    [InlineData(5, 10, true)]
    [InlineData(9, 10, true)]
    [InlineData(10, 10, false)]
    public void HasNext_ReturnsCorrectValue(int currentPage, int totalPages, bool expectedHasNext)
    {
        // Arrange
        var metadata = new PaginationMetadata(100, 10, currentPage, totalPages);

        // Assert
        metadata.HasNext.Should().Be(expectedHasNext);
    }

    [Fact]
    public void HasNext_OnLastPage_ReturnsFalse()
    {
        // Arrange
        var metadata = new PaginationMetadata(50, 10, 5, 5);

        // Assert
        metadata.HasNext.Should().BeFalse();
    }

    [Fact]
    public void HasNext_OnSinglePageResult_ReturnsFalse()
    {
        // Arrange
        var metadata = new PaginationMetadata(5, 10, 1, 1);

        // Assert
        metadata.HasNext.Should().BeFalse();
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void EmptyResult_HasNoNavigation()
    {
        // Arrange
        var metadata = new PaginationMetadata(0, 10, 1, 0);

        // Assert
        metadata.TotalCount.Should().Be(0);
        metadata.TotalPages.Should().Be(0);
        metadata.HasPrevious.Should().BeFalse();
        metadata.HasNext.Should().BeFalse();
    }

    [Fact]
    public void SingleItemResult_HasNoNavigation()
    {
        // Arrange
        var metadata = new PaginationMetadata(1, 10, 1, 1);

        // Assert
        metadata.TotalCount.Should().Be(1);
        metadata.HasPrevious.Should().BeFalse();
        metadata.HasNext.Should().BeFalse();
    }

    [Fact]
    public void MiddlePage_HasBothNavigation()
    {
        // Arrange
        var metadata = new PaginationMetadata(100, 10, 5, 10);

        // Assert
        metadata.HasPrevious.Should().BeTrue();
        metadata.HasNext.Should().BeTrue();
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void PaginationMetadata_Equality_WorksCorrectly()
    {
        // Arrange
        var metadata1 = new PaginationMetadata(100, 10, 2, 10);
        var metadata2 = new PaginationMetadata(100, 10, 2, 10);
        var metadata3 = new PaginationMetadata(100, 10, 3, 10);

        // Assert
        metadata1.Should().Be(metadata2);
        metadata1.Should().NotBe(metadata3);
    }

    #endregion

    #region Serialization Tests

    [Fact]
    public void PaginationMetadata_CanBeSerializedToJson()
    {
        // Arrange
        var metadata = new PaginationMetadata(100, 10, 2, 10);

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(metadata);

        // Assert
        json.Should().Contain("\"TotalCount\":100");
        json.Should().Contain("\"PageSize\":10");
        json.Should().Contain("\"CurrentPage\":2");
        json.Should().Contain("\"TotalPages\":10");
        json.Should().Contain("\"HasPrevious\":true");
        json.Should().Contain("\"HasNext\":true");
    }

    #endregion
}

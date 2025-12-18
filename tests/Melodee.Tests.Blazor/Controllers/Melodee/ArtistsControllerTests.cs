using System.Reflection;
using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Melodee.Common.Models.Collection;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for ArtistsController starred, setrating, and hated endpoints.
/// </summary>
public class ArtistsControllerTests
{
    #region Artist Order Fields Tests

    [Fact]
    public void ArtistsController_HasArtistOrderFields_WithExpectedSortOptions()
    {
        // Arrange
        var expectedFields = new[]
        {
            nameof(ArtistDataInfo.Name),
            nameof(ArtistDataInfo.AlbumCount),
            nameof(ArtistDataInfo.SongCount),
            nameof(ArtistDataInfo.LastPlayedAt),
            nameof(ArtistDataInfo.PlayedCount),
            nameof(ArtistDataInfo.CalculatedRating)
        };

        // Act
        var field = typeof(ArtistsController).GetField("ArtistOrderFields", BindingFlags.NonPublic | BindingFlags.Static);

        // Assert
        field.Should().NotBeNull();
        var orderFields = field!.GetValue(null) as HashSet<string>;
        orderFields.Should().NotBeNull();
        orderFields.Should().BeEquivalentTo(expectedFields);
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("AlbumCount")]
    [InlineData("SongCount")]
    [InlineData("LastPlayedAt")]
    [InlineData("PlayedCount")]
    [InlineData("CalculatedRating")]
    public void ArtistsController_ArtistOrderFields_ContainsExpectedField(string fieldName)
    {
        // Arrange
        var field = typeof(ArtistsController).GetField("ArtistOrderFields", BindingFlags.NonPublic | BindingFlags.Static);
        var orderFields = field!.GetValue(null) as HashSet<string>;

        // Assert
        orderFields.Should().Contain(fieldName);
    }

    [Fact]
    public void ArtistsController_HasAlbumOrderFields_WithExpectedSortOptions()
    {
        // Arrange
        var expectedFields = new[]
        {
            nameof(AlbumDataInfo.Name),
            nameof(AlbumDataInfo.ReleaseDate),
            nameof(AlbumDataInfo.SongCount),
            nameof(AlbumDataInfo.Duration),
            nameof(AlbumDataInfo.LastPlayedAt),
            nameof(AlbumDataInfo.PlayedCount),
            nameof(AlbumDataInfo.CalculatedRating)
        };

        // Act
        var field = typeof(ArtistsController).GetField("AlbumOrderFields", BindingFlags.NonPublic | BindingFlags.Static);

        // Assert
        field.Should().NotBeNull();
        var orderFields = field!.GetValue(null) as HashSet<string>;
        orderFields.Should().NotBeNull();
        orderFields.Should().BeEquivalentTo(expectedFields);
    }

    [Fact]
    public void ListAsync_HasOrderByAndOrderDirectionParameters()
    {
        // Arrange
        var method = typeof(ArtistsController).GetMethod(nameof(ArtistsController.ListAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().Contain(p => p.Name == "orderBy");
        parameters.Should().Contain(p => p.Name == "orderDirection");
    }

    [Fact]
    public void ArtistAlbumsAsync_HasOrderByAndOrderDirectionParameters()
    {
        // Arrange
        var method = typeof(ArtistsController).GetMethod(nameof(ArtistsController.ArtistAlbumsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().Contain(p => p.Name == "orderBy");
        parameters.Should().Contain(p => p.Name == "orderDirection");
    }

    [Fact]
    public void ArtistsController_HasSongOrderFields_WithExpectedSortOptions()
    {
        // Arrange
        var expectedFields = new[]
        {
            nameof(SongDataInfo.Title),
            nameof(SongDataInfo.SongNumber),
            nameof(SongDataInfo.AlbumId),
            nameof(SongDataInfo.PlayedCount),
            nameof(SongDataInfo.Duration),
            nameof(SongDataInfo.LastPlayedAt),
            nameof(SongDataInfo.CalculatedRating)
        };

        // Act
        var field = typeof(ArtistsController).GetField("SongOrderFields", BindingFlags.NonPublic | BindingFlags.Static);

        // Assert
        field.Should().NotBeNull();
        var orderFields = field!.GetValue(null) as HashSet<string>;
        orderFields.Should().NotBeNull();
        orderFields.Should().BeEquivalentTo(expectedFields);
    }

    [Fact]
    public void ArtistSongsAsync_HasOrderByAndOrderDirectionParameters()
    {
        // Arrange
        var method = typeof(ArtistsController).GetMethod(nameof(ArtistsController.ArtistSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().Contain(p => p.Name == "orderBy");
        parameters.Should().Contain(p => p.Name == "orderDirection");
    }

    #endregion

    #region Route Tests

    [Fact]
    public void ToggleArtistStarred_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(ArtistsController).GetMethod(nameof(ArtistsController.ToggleArtistStarred));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("starred/{apiKey:guid}/{isStarred:bool}");
    }

    [Fact]
    public void ToggleArtistStarred_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(ArtistsController).GetMethod(nameof(ArtistsController.ToggleArtistStarred));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void SetArtistRating_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(ArtistsController).GetMethod(nameof(ArtistsController.SetArtistRating));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("setrating/{apiKey:guid}/{rating:int}");
    }

    [Fact]
    public void SetArtistRating_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(ArtistsController).GetMethod(nameof(ArtistsController.SetArtistRating));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void ToggleArtistHated_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(ArtistsController).GetMethod(nameof(ArtistsController.ToggleArtistHated));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("hated/{apiKey:guid}/{isHated:bool}");
    }

    [Fact]
    public void ToggleArtistHated_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(ArtistsController).GetMethod(nameof(ArtistsController.ToggleArtistHated));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    #endregion

    #region Method Signature Tests

    [Fact]
    public void ToggleArtistStarred_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(ArtistsController).GetMethod(nameof(ArtistsController.ToggleArtistStarred));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("apiKey");
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].Name.Should().Be("isStarred");
        parameters[1].ParameterType.Should().Be(typeof(bool));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void SetArtistRating_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(ArtistsController).GetMethod(nameof(ArtistsController.SetArtistRating));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("apiKey");
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].Name.Should().Be("rating");
        parameters[1].ParameterType.Should().Be(typeof(int));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void ToggleArtistHated_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(ArtistsController).GetMethod(nameof(ArtistsController.ToggleArtistHated));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("apiKey");
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].Name.Should().Be("isHated");
        parameters[1].ParameterType.Should().Be(typeof(bool));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    #endregion

    #region Return Type Tests

    [Fact]
    public void ToggleArtistStarred_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(ArtistsController).GetMethod(nameof(ArtistsController.ToggleArtistStarred));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    [Fact]
    public void SetArtistRating_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(ArtistsController).GetMethod(nameof(ArtistsController.SetArtistRating));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    [Fact]
    public void ToggleArtistHated_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(ArtistsController).GetMethod(nameof(ArtistsController.ToggleArtistHated));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Controller Attribute Tests

    [Fact]
    public void ArtistsController_HasApiControllerAttribute()
    {
        // Arrange & Assert
        var attribute = typeof(ArtistsController).GetCustomAttributes(typeof(ApiControllerAttribute), false).FirstOrDefault();
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void ArtistsController_HasCorrectRoutePrefix()
    {
        // Arrange
        var routeAttribute = typeof(ArtistsController).GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;

        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/[controller]");
    }

    #endregion
}

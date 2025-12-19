using System.Reflection;
using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Melodee.Common.Models.Collection;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for SongsController starred, setrating, and hated endpoints.
/// </summary>
public class SongsControllerTests
{
    #region Song Order Fields Tests

    [Fact]
    public void SongsController_HasSongOrderFields_WithExpectedSortOptions()
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
        var field = typeof(SongsController).GetField("SongOrderFields", BindingFlags.NonPublic | BindingFlags.Static);

        // Assert
        field.Should().NotBeNull();
        var orderFields = field!.GetValue(null) as HashSet<string>;
        orderFields.Should().NotBeNull();
        orderFields.Should().BeEquivalentTo(expectedFields);
    }

    [Theory]
    [InlineData("Title")]
    [InlineData("SongNumber")]
    [InlineData("AlbumId")]
    [InlineData("PlayedCount")]
    [InlineData("Duration")]
    [InlineData("LastPlayedAt")]
    [InlineData("CalculatedRating")]
    public void SongsController_SongOrderFields_ContainsExpectedField(string fieldName)
    {
        // Arrange
        var field = typeof(SongsController).GetField("SongOrderFields", BindingFlags.NonPublic | BindingFlags.Static);
        var orderFields = field!.GetValue(null) as HashSet<string>;

        // Assert
        orderFields.Should().Contain(fieldName);
    }

    [Fact]
    public void ListAsync_HasOrderByAndOrderDirectionParameters()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.ListAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().Contain(p => p.Name == "orderBy");
        parameters.Should().Contain(p => p.Name == "orderDirection");
    }

    #endregion

    #region Starred Route Tests

    [Fact]
    public void ToggleSongStarred_HasCorrectRouteAttribute()
    {
        // Arrange
        var methods = typeof(SongsController).GetMethods()
            .Where(m => m.Name == "ToggleSongStarred" && m.GetParameters().Any(p => p.Name == "isStarred"))
            .ToList();

        // Assert
        methods.Should().NotBeEmpty();
        var method = methods.First();
        var routeAttribute = method.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("starred/{apiKey:guid}/{isStarred:bool}");
    }

    [Fact]
    public void ToggleSongStarred_HasHttpPostAttribute()
    {
        // Arrange
        var methods = typeof(SongsController).GetMethods()
            .Where(m => m.Name == "ToggleSongStarred" && m.GetParameters().Any(p => p.Name == "isStarred"))
            .ToList();

        // Assert
        methods.Should().NotBeEmpty();
        var method = methods.First();
        var httpPostAttribute = method.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    #endregion

    #region SetRating Route Tests

    [Fact]
    public void SetSongRating_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.SetSongRating));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("setrating/{apiKey:guid}/{rating:int}");
    }

    [Fact]
    public void SetSongRating_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.SetSongRating));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    #endregion

    #region Hated Route Tests

    [Fact]
    public void ToggleSongHated_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.ToggleSongHated));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("hated/{apiKey:guid}/{isHated:bool}");
    }

    [Fact]
    public void ToggleSongHated_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.ToggleSongHated));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    #endregion

    #region Method Signature Tests

    [Fact]
    public void ToggleSongStarred_HasCorrectParameters()
    {
        // Arrange
        var methods = typeof(SongsController).GetMethods()
            .Where(m => m.Name == "ToggleSongStarred" && m.GetParameters().Any(p => p.Name == "isStarred"))
            .ToList();

        // Assert
        methods.Should().NotBeEmpty();
        var method = methods.First();
        var parameters = method.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("apiKey");
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].Name.Should().Be("isStarred");
        parameters[1].ParameterType.Should().Be(typeof(bool));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void SetSongRating_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.SetSongRating));

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
    public void ToggleSongHated_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.ToggleSongHated));

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
    public void ToggleSongStarred_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var methods = typeof(SongsController).GetMethods()
            .Where(m => m.Name == "ToggleSongStarred" && m.GetParameters().Any(p => p.Name == "isStarred"))
            .ToList();

        // Assert
        methods.Should().NotBeEmpty();
        var method = methods.First();
        // The method returns Task<IActionResult>? (nullable)
        method.ReturnType.Should().Match(t =>
            t == typeof(Task<IActionResult>) ||
            (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)));
    }

    [Fact]
    public void SetSongRating_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.SetSongRating));

        // Assert
        method.Should().NotBeNull();
        // The method returns Task<IActionResult>? (nullable)
        method!.ReturnType.Should().Match(t =>
            t == typeof(Task<IActionResult>) ||
            (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)));
    }

    [Fact]
    public void ToggleSongHated_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.ToggleSongHated));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Controller Attribute Tests

    [Fact]
    public void SongsController_HasApiControllerAttribute()
    {
        // Arrange & Assert
        var attribute = typeof(SongsController).GetCustomAttributes(typeof(ApiControllerAttribute), false).FirstOrDefault();
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void SongsController_HasCorrectRoutePrefix()
    {
        // Arrange
        var routeAttribute = typeof(SongsController).GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;

        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/[controller]");
    }

    #endregion

    #region Starred Songs Endpoint Tests

    [Fact]
    public void StarredSongsAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.StarredSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("starred");
    }

    [Fact]
    public void StarredSongsAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.StarredSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void StarredSongsAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.StarredSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("page");
        parameters[0].ParameterType.Should().Be(typeof(short));
        parameters[1].Name.Should().Be("pageSize");
        parameters[1].ParameterType.Should().Be(typeof(short));
        parameters[2].Name.Should().Be("cancellationToken");
    }

    [Fact]
    public void StarredSongsAsync_HasCorrectDefaultValues()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.StarredSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        
        var pageParam = parameters.First(p => p.Name == "page");
        pageParam.HasDefaultValue.Should().BeTrue();
        pageParam.DefaultValue.Should().Be((short)1);
        
        var pageSizeParam = parameters.First(p => p.Name == "pageSize");
        pageSizeParam.HasDefaultValue.Should().BeTrue();
        pageSizeParam.DefaultValue.Should().Be((short)20);
    }

    [Fact]
    public void StarredSongsAsync_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.StarredSongsAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Hated Songs Endpoint Tests

    [Fact]
    public void HatedSongsAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.HatedSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("hated");
    }

    [Fact]
    public void HatedSongsAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.HatedSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void HatedSongsAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.HatedSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("page");
        parameters[0].ParameterType.Should().Be(typeof(short));
        parameters[1].Name.Should().Be("pageSize");
        parameters[1].ParameterType.Should().Be(typeof(short));
        parameters[2].Name.Should().Be("cancellationToken");
    }

    [Fact]
    public void HatedSongsAsync_HasCorrectDefaultValues()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.HatedSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        
        var pageParam = parameters.First(p => p.Name == "page");
        pageParam.HasDefaultValue.Should().BeTrue();
        pageParam.DefaultValue.Should().Be((short)1);
        
        var pageSizeParam = parameters.First(p => p.Name == "pageSize");
        pageSizeParam.HasDefaultValue.Should().BeTrue();
        pageSizeParam.DefaultValue.Should().Be((short)20);
    }

    [Fact]
    public void HatedSongsAsync_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.HatedSongsAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Top Rated Songs Endpoint Tests

    [Fact]
    public void TopRatedSongsAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.TopRatedSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("top-rated");
    }

    [Fact]
    public void TopRatedSongsAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.TopRatedSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void TopRatedSongsAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.TopRatedSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("page");
        parameters[0].ParameterType.Should().Be(typeof(short));
        parameters[1].Name.Should().Be("pageSize");
        parameters[1].ParameterType.Should().Be(typeof(short));
        parameters[2].Name.Should().Be("cancellationToken");
    }

    [Fact]
    public void TopRatedSongsAsync_HasCorrectDefaultValues()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.TopRatedSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        
        var pageParam = parameters.First(p => p.Name == "page");
        pageParam.HasDefaultValue.Should().BeTrue();
        pageParam.DefaultValue.Should().Be((short)1);
        
        var pageSizeParam = parameters.First(p => p.Name == "pageSize");
        pageSizeParam.HasDefaultValue.Should().BeTrue();
        pageSizeParam.DefaultValue.Should().Be((short)20);
    }

    [Fact]
    public void TopRatedSongsAsync_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.TopRatedSongsAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion
}

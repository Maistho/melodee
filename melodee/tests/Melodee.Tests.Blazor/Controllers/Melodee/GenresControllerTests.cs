using System.Reflection;
using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Common.Models.Collection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for GenresController endpoints.
/// </summary>
public class GenresControllerTests
{
    #region Genre Order Fields Tests

    [Fact]
    public void GenresController_HasGenreOrderFields_WithExpectedSortOptions()
    {
        // Arrange
        var expectedFields = new[]
        {
            nameof(Genre.Name),
            nameof(Genre.SongCount),
            nameof(Genre.AlbumCount)
        };

        // Act
        var field = typeof(GenresController).GetField("GenreOrderFields", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);

        // Assert
        field.Should().NotBeNull();
        var orderFields = field!.GetValue(null) as HashSet<string>;
        orderFields.Should().NotBeNull();
        orderFields.Should().BeEquivalentTo(expectedFields);
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("SongCount")]
    [InlineData("AlbumCount")]
    public void GenresController_GenreOrderFields_ContainsExpectedField(string fieldName)
    {
        // Arrange
        var field = typeof(GenresController).GetField("GenreOrderFields", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
        var orderFields = field!.GetValue(null) as HashSet<string>;

        // Assert
        orderFields.Should().Contain(fieldName);
    }

    [Fact]
    public void GenresController_HasSongOrderFields_WithExpectedSortOptions()
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
        var field = typeof(GenresController).GetField("SongOrderFields", BindingFlags.NonPublic | BindingFlags.Static);

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
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ListAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().Contain(p => p.Name == "orderBy");
        parameters.Should().Contain(p => p.Name == "orderDirection");
    }

    [Fact]
    public void ListAsync_HasPageAndLimitParameters()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ListAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().Contain(p => p.Name == "page");
        parameters.Should().Contain(p => p.Name == "limit");
    }

    [Fact]
    public void ListAsync_HasCorrectDefaultValues()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ListAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();

        var pageParam = parameters.First(p => p.Name == "page");
        pageParam.HasDefaultValue.Should().BeTrue();
        pageParam.DefaultValue.Should().Be(1);

        var limitParam = parameters.First(p => p.Name == "limit");
        limitParam.HasDefaultValue.Should().BeTrue();
        limitParam.DefaultValue.Should().Be(50);
    }

    #endregion

    #region Route Tests

    [Fact]
    public void ListAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ListAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GenreSongsAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GenreSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{id}/songs");
    }

    [Fact]
    public void GenreSongsAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GenreSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    #endregion

    #region Method Signature Tests

    [Fact]
    public void ListAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ListAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(5);
        parameters[0].Name.Should().Be("page");
        parameters[0].ParameterType.Should().Be(typeof(int));
        parameters[1].Name.Should().Be("limit");
        parameters[1].ParameterType.Should().Be(typeof(int));
        parameters[2].Name.Should().Be("orderBy");
        parameters[2].ParameterType.Should().Be(typeof(string));
        parameters[3].Name.Should().Be("orderDirection");
        parameters[3].ParameterType.Should().Be(typeof(string));
        parameters[4].Name.Should().Be("cancellationToken");
        parameters[4].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void GenreSongsAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GenreSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(4);
        parameters[0].Name.Should().Be("id");
        parameters[0].ParameterType.Should().Be(typeof(string));
        parameters[1].Name.Should().Be("page");
        parameters[1].ParameterType.Should().Be(typeof(int));
        parameters[2].Name.Should().Be("limit");
        parameters[2].ParameterType.Should().Be(typeof(int));
        parameters[3].Name.Should().Be("cancellationToken");
        parameters[3].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void GenreSongsAsync_HasCorrectDefaultValues()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GenreSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();

        var pageParam = parameters.First(p => p.Name == "page");
        pageParam.HasDefaultValue.Should().BeTrue();
        pageParam.DefaultValue.Should().Be(1);

        var limitParam = parameters.First(p => p.Name == "limit");
        limitParam.HasDefaultValue.Should().BeTrue();
        limitParam.DefaultValue.Should().Be(50);
    }

    #endregion

    #region Return Type Tests

    [Fact]
    public void ListAsync_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ListAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    [Fact]
    public void GenreSongsAsync_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GenreSongsAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Controller Attribute Tests

    [Fact]
    public void GenresController_HasApiControllerAttribute()
    {
        // Arrange & Assert
        var attribute = typeof(GenresController).GetCustomAttributes(typeof(ApiControllerAttribute), false).FirstOrDefault();
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void GenresController_HasCorrectRoutePrefix()
    {
        // Arrange
        var routeAttribute = typeof(GenresController).GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;

        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/genres");
    }

    [Fact]
    public void GenresController_HasAuthorizeAttribute()
    {
        // Arrange
        var authorizeAttribute = typeof(GenresController).GetCustomAttributes(typeof(AuthorizeAttribute), false).FirstOrDefault() as AuthorizeAttribute;

        // Assert
        authorizeAttribute.Should().NotBeNull();
        authorizeAttribute!.AuthenticationSchemes.Should().Be(JwtBearerDefaults.AuthenticationScheme);
    }

    [Fact]
    public void GenresController_HasRateLimitingAttribute()
    {
        // Arrange
        var rateLimitAttribute = typeof(GenresController).GetCustomAttributes(typeof(EnableRateLimitingAttribute), false).FirstOrDefault() as EnableRateLimitingAttribute;

        // Assert
        rateLimitAttribute.Should().NotBeNull();
        rateLimitAttribute!.PolicyName.Should().Be("melodee-api");
    }

    #endregion

    #region ProducesResponseType Tests

    [Fact]
    public void ListAsync_HasProducesResponseTypeForOk()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ListAsync));

        // Assert
        method.Should().NotBeNull();
        var attributes = method!.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false).Cast<ProducesResponseTypeAttribute>();
        attributes.Should().Contain(a => a.StatusCode == 200);
    }

    [Fact]
    public void ListAsync_HasProducesResponseTypeForUnauthorized()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ListAsync));

        // Assert
        method.Should().NotBeNull();
        var attributes = method!.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false).Cast<ProducesResponseTypeAttribute>();
        attributes.Should().Contain(a => a.StatusCode == 401);
    }

    [Fact]
    public void ListAsync_HasProducesResponseTypeForBadRequest()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ListAsync));

        // Assert
        method.Should().NotBeNull();
        var attributes = method!.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false).Cast<ProducesResponseTypeAttribute>();
        attributes.Should().Contain(a => a.StatusCode == 400);
    }

    [Fact]
    public void GenreSongsAsync_HasProducesResponseTypeForOk()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GenreSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var attributes = method!.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false).Cast<ProducesResponseTypeAttribute>();
        attributes.Should().Contain(a => a.StatusCode == 200);
    }

    [Fact]
    public void GenreSongsAsync_HasProducesResponseTypeForUnauthorized()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GenreSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var attributes = method!.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false).Cast<ProducesResponseTypeAttribute>();
        attributes.Should().Contain(a => a.StatusCode == 401);
    }

    [Fact]
    public void GenreSongsAsync_HasProducesResponseTypeForNotFound()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GenreSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var attributes = method!.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false).Cast<ProducesResponseTypeAttribute>();
        attributes.Should().Contain(a => a.StatusCode == 404);
    }

    [Fact]
    public void GenreSongsAsync_HasProducesResponseTypeForBadRequest()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GenreSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var attributes = method!.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false).Cast<ProducesResponseTypeAttribute>();
        attributes.Should().Contain(a => a.StatusCode == 400);
    }

    #endregion

    #region Genre Model Tests

    [Fact]
    public void GenreModel_HasExpectedProperties()
    {
        // Arrange
        var genreType = typeof(Genre);

        // Assert
        genreType.GetProperty(nameof(Genre.Id)).Should().NotBeNull();
        genreType.GetProperty(nameof(Genre.Name)).Should().NotBeNull();
        genreType.GetProperty(nameof(Genre.SongCount)).Should().NotBeNull();
        genreType.GetProperty(nameof(Genre.AlbumCount)).Should().NotBeNull();
    }

    [Fact]
    public void GenreModel_PropertiesHaveCorrectTypes()
    {
        // Arrange
        var genreType = typeof(Genre);

        // Assert
        genreType.GetProperty(nameof(Genre.Id))!.PropertyType.Should().Be(typeof(string));
        genreType.GetProperty(nameof(Genre.Name))!.PropertyType.Should().Be(typeof(string));
        genreType.GetProperty(nameof(Genre.SongCount))!.PropertyType.Should().Be(typeof(int));
        genreType.GetProperty(nameof(Genre.AlbumCount))!.PropertyType.Should().Be(typeof(int));
    }

    [Fact]
    public void GenreModel_CanBeInstantiated()
    {
        // Arrange & Act
        var genre = new Genre("dGVzdA==", "Rock", 100, 10);

        // Assert
        genre.Id.Should().Be("dGVzdA==");
        genre.Name.Should().Be("Rock");
        genre.SongCount.Should().Be(100);
        genre.AlbumCount.Should().Be(10);
    }

    #endregion
}

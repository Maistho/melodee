using System.Reflection;
using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for UserController user-specific library endpoints.
/// </summary>
public class UserControllerTests
{
    #region Controller Attribute Tests

    [Fact]
    public void UserController_HasApiControllerAttribute()
    {
        var attribute = typeof(UserController).GetCustomAttributes(typeof(ApiControllerAttribute), false).FirstOrDefault();
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void UserController_HasCorrectRoutePrefix()
    {
        var routeAttribute = typeof(UserController).GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/user");
    }

    [Fact]
    public void UserController_HasAuthorizeAttribute()
    {
        var authorizeAttribute = typeof(UserController).GetCustomAttributes(typeof(AuthorizeAttribute), false).FirstOrDefault() as AuthorizeAttribute;
        authorizeAttribute.Should().NotBeNull();
        authorizeAttribute!.AuthenticationSchemes.Should().Be(JwtBearerDefaults.AuthenticationScheme);
    }

    [Fact]
    public void UserController_HasRateLimitingAttribute()
    {
        var rateLimitAttribute = typeof(UserController).GetCustomAttributes(typeof(EnableRateLimitingAttribute), false).FirstOrDefault() as EnableRateLimitingAttribute;
        rateLimitAttribute.Should().NotBeNull();
        rateLimitAttribute!.PolicyName.Should().Be("melodee-api");
    }

    #endregion

    #region User Profile Endpoints

    [Fact]
    public void AboutMeAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.AboutMeAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("me");
    }

    [Fact]
    public void LastPlayedSongsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.LastPlayedSongsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("last-played");
    }

    [Fact]
    public void PlaylistsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.PlaylistsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("playlists");
    }

    #endregion

    #region User Songs Endpoints

    [Fact]
    public void LikedSongsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.LikedSongsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("songs/liked");
    }

    [Fact]
    public void LikedSongsAsync_HasHttpGetAttribute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.LikedSongsAsync));
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void LikedSongsAsync_HasCorrectDefaultValues()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.LikedSongsAsync));
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        
        var pageParam = parameters.First(p => p.Name == "page");
        pageParam.HasDefaultValue.Should().BeTrue();
        pageParam.DefaultValue.Should().Be(1);
        
        var limitParam = parameters.First(p => p.Name == "limit");
        limitParam.HasDefaultValue.Should().BeTrue();
        limitParam.DefaultValue.Should().Be(50);
    }

    [Fact]
    public void DislikedSongsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.DislikedSongsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("songs/disliked");
    }

    [Fact]
    public void RatedSongsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.RatedSongsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("songs/rated");
    }

    [Fact]
    public void TopRatedSongsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.TopRatedSongsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("songs/top-rated");
    }

    #endregion

    #region User Albums Endpoints

    [Fact]
    public void LikedAlbumsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.LikedAlbumsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("albums/liked");
    }

    [Fact]
    public void LikedAlbumsAsync_HasHttpGetAttribute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.LikedAlbumsAsync));
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void LikedAlbumsAsync_HasCorrectDefaultValues()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.LikedAlbumsAsync));
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        
        var pageParam = parameters.First(p => p.Name == "page");
        pageParam.HasDefaultValue.Should().BeTrue();
        pageParam.DefaultValue.Should().Be(1);
        
        var limitParam = parameters.First(p => p.Name == "limit");
        limitParam.HasDefaultValue.Should().BeTrue();
        limitParam.DefaultValue.Should().Be(50);
    }

    [Fact]
    public void DislikedAlbumsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.DislikedAlbumsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("albums/disliked");
    }

    [Fact]
    public void RatedAlbumsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.RatedAlbumsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("albums/rated");
    }

    [Fact]
    public void TopRatedAlbumsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.TopRatedAlbumsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("albums/top-rated");
    }

    #endregion

    #region User Artists Endpoints

    [Fact]
    public void LikedArtistsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.LikedArtistsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("artists/liked");
    }

    [Fact]
    public void LikedArtistsAsync_HasHttpGetAttribute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.LikedArtistsAsync));
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void LikedArtistsAsync_HasCorrectDefaultValues()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.LikedArtistsAsync));
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        
        var pageParam = parameters.First(p => p.Name == "page");
        pageParam.HasDefaultValue.Should().BeTrue();
        pageParam.DefaultValue.Should().Be(1);
        
        var limitParam = parameters.First(p => p.Name == "limit");
        limitParam.HasDefaultValue.Should().BeTrue();
        limitParam.DefaultValue.Should().Be(50);
    }

    [Fact]
    public void DislikedArtistsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.DislikedArtistsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("artists/disliked");
    }

    [Fact]
    public void RatedArtistsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.RatedArtistsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("artists/rated");
    }

    [Fact]
    public void TopRatedArtistsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.TopRatedArtistsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("artists/top-rated");
    }

    #endregion

    #region Recently Played Endpoints

    [Fact]
    public void RecentlyPlayedSongsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.RecentlyPlayedSongsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("songs/recently-played");
    }

    [Fact]
    public void RecentlyPlayedSongsAsync_HasHttpGetAttribute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.RecentlyPlayedSongsAsync));
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void RecentlyPlayedSongsAsync_HasCorrectDefaultValues()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.RecentlyPlayedSongsAsync));
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        
        var pageParam = parameters.First(p => p.Name == "page");
        pageParam.HasDefaultValue.Should().BeTrue();
        pageParam.DefaultValue.Should().Be(1);
        
        var limitParam = parameters.First(p => p.Name == "limit");
        limitParam.HasDefaultValue.Should().BeTrue();
        limitParam.DefaultValue.Should().Be(50);
    }

    [Fact]
    public void RecentlyPlayedAlbumsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.RecentlyPlayedAlbumsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("albums/recently-played");
    }

    [Fact]
    public void RecentlyPlayedAlbumsAsync_HasHttpGetAttribute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.RecentlyPlayedAlbumsAsync));
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void RecentlyPlayedArtistsAsync_HasCorrectRoute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.RecentlyPlayedArtistsAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("artists/recently-played");
    }

    [Fact]
    public void RecentlyPlayedArtistsAsync_HasHttpGetAttribute()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.RecentlyPlayedArtistsAsync));
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    #endregion

    #region Return Type Tests

    [Theory]
    [InlineData(nameof(UserController.LikedSongsAsync))]
    [InlineData(nameof(UserController.DislikedSongsAsync))]
    [InlineData(nameof(UserController.RatedSongsAsync))]
    [InlineData(nameof(UserController.TopRatedSongsAsync))]
    [InlineData(nameof(UserController.RecentlyPlayedSongsAsync))]
    [InlineData(nameof(UserController.LikedAlbumsAsync))]
    [InlineData(nameof(UserController.DislikedAlbumsAsync))]
    [InlineData(nameof(UserController.RatedAlbumsAsync))]
    [InlineData(nameof(UserController.TopRatedAlbumsAsync))]
    [InlineData(nameof(UserController.RecentlyPlayedAlbumsAsync))]
    [InlineData(nameof(UserController.LikedArtistsAsync))]
    [InlineData(nameof(UserController.DislikedArtistsAsync))]
    [InlineData(nameof(UserController.RatedArtistsAsync))]
    [InlineData(nameof(UserController.TopRatedArtistsAsync))]
    [InlineData(nameof(UserController.RecentlyPlayedArtistsAsync))]
    public void LibraryEndpoint_ReturnsTaskOfIActionResult(string methodName)
    {
        var method = typeof(UserController).GetMethod(methodName);
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion
}

using System.Reflection;
using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for AuthController authentication endpoints.
/// </summary>
public class AuthControllerTests
{
    #region Controller Attribute Tests

    [Fact]
    public void AuthController_HasApiControllerAttribute()
    {
        var attribute = typeof(AuthController).GetCustomAttributes(typeof(ApiControllerAttribute), false).FirstOrDefault();
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void AuthController_HasCorrectRoutePrefix()
    {
        var routeAttribute = typeof(AuthController).GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/auth");
    }

    [Fact]
    public void AuthController_HasRateLimitingAttribute()
    {
        var rateLimitAttribute = typeof(AuthController).GetCustomAttributes(typeof(EnableRateLimitingAttribute), false).FirstOrDefault() as EnableRateLimitingAttribute;
        rateLimitAttribute.Should().NotBeNull();
        rateLimitAttribute!.PolicyName.Should().Be("melodee-api");
    }

    #endregion

    #region Authenticate Endpoint Tests

    [Fact]
    public void AuthenticateAsync_HasCorrectRoute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.AuthenticateAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("authenticate");
    }

    [Fact]
    public void AuthenticateAsync_HasHttpPostAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.AuthenticateAsync));
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void AuthenticateAsync_HasAllowAnonymousAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.AuthenticateAsync));
        method.Should().NotBeNull();
        var allowAnonymousAttribute = method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), false).FirstOrDefault();
        allowAnonymousAttribute.Should().NotBeNull();
    }

    [Fact]
    public void AuthenticateAsync_HasAuthRateLimiting()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.AuthenticateAsync));
        method.Should().NotBeNull();
        var rateLimitAttribute = method!.GetCustomAttributes(typeof(EnableRateLimitingAttribute), false).FirstOrDefault() as EnableRateLimitingAttribute;
        rateLimitAttribute.Should().NotBeNull();
        rateLimitAttribute!.PolicyName.Should().Be("melodee-auth");
    }

    [Fact]
    public void AuthenticateAsync_ReturnsTaskOfIActionResult()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.AuthenticateAsync));
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Google Auth Endpoint Tests

    [Fact]
    public void GoogleAuthAsync_HasCorrectRoute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.GoogleAuthAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("google");
    }

    [Fact]
    public void GoogleAuthAsync_HasHttpPostAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.GoogleAuthAsync));
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GoogleAuthAsync_HasAllowAnonymousAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.GoogleAuthAsync));
        method.Should().NotBeNull();
        var allowAnonymousAttribute = method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), false).FirstOrDefault();
        allowAnonymousAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GoogleAuthAsync_HasAuthRateLimiting()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.GoogleAuthAsync));
        method.Should().NotBeNull();
        var rateLimitAttribute = method!.GetCustomAttributes(typeof(EnableRateLimitingAttribute), false).FirstOrDefault() as EnableRateLimitingAttribute;
        rateLimitAttribute.Should().NotBeNull();
        rateLimitAttribute!.PolicyName.Should().Be("melodee-auth");
    }

    [Fact]
    public void GoogleAuthAsync_ReturnsTaskOfIActionResult()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.GoogleAuthAsync));
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Refresh Token With Rotation Endpoint Tests

    [Fact]
    public void RefreshTokenWithRotationAsync_HasCorrectRoute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RefreshTokenWithRotationAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("refresh-token");
    }

    [Fact]
    public void RefreshTokenWithRotationAsync_HasHttpPostAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RefreshTokenWithRotationAsync));
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void RefreshTokenWithRotationAsync_HasAllowAnonymousAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RefreshTokenWithRotationAsync));
        method.Should().NotBeNull();
        var allowAnonymousAttribute = method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), false).FirstOrDefault();
        allowAnonymousAttribute.Should().NotBeNull();
    }

    [Fact]
    public void RefreshTokenWithRotationAsync_HasAuthRateLimiting()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RefreshTokenWithRotationAsync));
        method.Should().NotBeNull();
        var rateLimitAttribute = method!.GetCustomAttributes(typeof(EnableRateLimitingAttribute), false).FirstOrDefault() as EnableRateLimitingAttribute;
        rateLimitAttribute.Should().NotBeNull();
        rateLimitAttribute!.PolicyName.Should().Be("melodee-auth");
    }

    [Fact]
    public void RefreshTokenWithRotationAsync_ReturnsTaskOfIActionResult()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RefreshTokenWithRotationAsync));
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Legacy Refresh Endpoint Tests

    [Fact]
    public void RefreshTokenAsync_HasCorrectRoute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RefreshTokenAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("refresh");
    }

    [Fact]
    public void RefreshTokenAsync_HasHttpPostAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RefreshTokenAsync));
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void RefreshTokenAsync_HasAuthorizeAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RefreshTokenAsync));
        method.Should().NotBeNull();
        var authorizeAttribute = method!.GetCustomAttributes(typeof(AuthorizeAttribute), false).FirstOrDefault();
        authorizeAttribute.Should().NotBeNull();
    }

    [Fact]
    public void RefreshTokenAsync_ReturnsTaskOfIActionResult()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RefreshTokenAsync));
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Logout Endpoint Tests

    [Fact]
    public void LogoutAsync_HasCorrectRoute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.LogoutAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("logout");
    }

    [Fact]
    public void LogoutAsync_HasHttpPostAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.LogoutAsync));
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void LogoutAsync_HasAuthorizeAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.LogoutAsync));
        method.Should().NotBeNull();
        var authorizeAttribute = method!.GetCustomAttributes(typeof(AuthorizeAttribute), false).FirstOrDefault();
        authorizeAttribute.Should().NotBeNull();
    }

    [Fact]
    public void LogoutAsync_ReturnsTaskOfIActionResult()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.LogoutAsync));
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Revoke Token Endpoint Tests

    [Fact]
    public void RevokeTokenAsync_HasCorrectRoute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RevokeTokenAsync));
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("revoke");
    }

    [Fact]
    public void RevokeTokenAsync_HasHttpPostAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RevokeTokenAsync));
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void RevokeTokenAsync_HasAuthorizeAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RevokeTokenAsync));
        method.Should().NotBeNull();
        var authorizeAttribute = method!.GetCustomAttributes(typeof(AuthorizeAttribute), false).FirstOrDefault();
        authorizeAttribute.Should().NotBeNull();
    }

    [Fact]
    public void RevokeTokenAsync_ReturnsTaskOfIActionResult()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RevokeTokenAsync));
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion
}

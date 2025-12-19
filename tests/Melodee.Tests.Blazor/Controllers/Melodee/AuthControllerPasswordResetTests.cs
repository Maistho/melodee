using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Melodee.Blazor.Controllers.Melodee.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for AuthController password reset endpoints.
/// </summary>
public class AuthControllerPasswordResetTests
{
    #region Request Password Reset Tests

    [Fact]
    public void RequestPasswordResetAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RequestPasswordResetAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("password-reset/request");
    }

    [Fact]
    public void RequestPasswordResetAsync_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RequestPasswordResetAsync));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void RequestPasswordResetAsync_HasAllowAnonymousAttribute()
    {
        // Arrange
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RequestPasswordResetAsync));

        // Assert
        method.Should().NotBeNull();
        var allowAnonymousAttribute = method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), false).FirstOrDefault();
        allowAnonymousAttribute.Should().NotBeNull();
    }

    [Fact]
    public void RequestPasswordResetAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RequestPasswordResetAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("request");
        parameters[0].ParameterType.Should().Be(typeof(PasswordResetRequest));
        parameters[1].Name.Should().Be("cancellationToken");
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void RequestPasswordResetAsync_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RequestPasswordResetAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Validate Password Reset Token Tests

    [Fact]
    public void ValidatePasswordResetTokenAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(AuthController).GetMethod(nameof(AuthController.ValidatePasswordResetTokenAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("password-reset/validate/{token}");
    }

    [Fact]
    public void ValidatePasswordResetTokenAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(AuthController).GetMethod(nameof(AuthController.ValidatePasswordResetTokenAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void ValidatePasswordResetTokenAsync_HasAllowAnonymousAttribute()
    {
        // Arrange
        var method = typeof(AuthController).GetMethod(nameof(AuthController.ValidatePasswordResetTokenAsync));

        // Assert
        method.Should().NotBeNull();
        var allowAnonymousAttribute = method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), false).FirstOrDefault();
        allowAnonymousAttribute.Should().NotBeNull();
    }

    [Fact]
    public void ValidatePasswordResetTokenAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(AuthController).GetMethod(nameof(AuthController.ValidatePasswordResetTokenAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("token");
        parameters[0].ParameterType.Should().Be(typeof(string));
        parameters[1].Name.Should().Be("cancellationToken");
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void ValidatePasswordResetTokenAsync_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(AuthController).GetMethod(nameof(AuthController.ValidatePasswordResetTokenAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Confirm Password Reset Tests

    [Fact]
    public void ConfirmPasswordResetAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(AuthController).GetMethod(nameof(AuthController.ConfirmPasswordResetAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("password-reset/confirm");
    }

    [Fact]
    public void ConfirmPasswordResetAsync_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(AuthController).GetMethod(nameof(AuthController.ConfirmPasswordResetAsync));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void ConfirmPasswordResetAsync_HasAllowAnonymousAttribute()
    {
        // Arrange
        var method = typeof(AuthController).GetMethod(nameof(AuthController.ConfirmPasswordResetAsync));

        // Assert
        method.Should().NotBeNull();
        var allowAnonymousAttribute = method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), false).FirstOrDefault();
        allowAnonymousAttribute.Should().NotBeNull();
    }

    [Fact]
    public void ConfirmPasswordResetAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(AuthController).GetMethod(nameof(AuthController.ConfirmPasswordResetAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("request");
        parameters[0].ParameterType.Should().Be(typeof(PasswordResetConfirmRequest));
        parameters[1].Name.Should().Be("cancellationToken");
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void ConfirmPasswordResetAsync_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(AuthController).GetMethod(nameof(AuthController.ConfirmPasswordResetAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Request Model Tests

    [Fact]
    public void PasswordResetRequest_HasEmailProperty()
    {
        // Arrange & Act
        var request = new PasswordResetRequest("test@example.com");

        // Assert
        request.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void PasswordResetConfirmRequest_HasRequiredProperties()
    {
        // Arrange & Act
        var request = new PasswordResetConfirmRequest("some-token", "newPassword123");

        // Assert
        request.Token.Should().Be("some-token");
        request.NewPassword.Should().Be("newPassword123");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void PasswordResetRequest_WithNullEmail_CanBeCreated()
    {
        // Arrange & Act - null can be passed but controller should validate
        var request = new PasswordResetRequest(null!);

        // Assert
        request.Email.Should().BeNull();
    }

    [Fact]
    public void PasswordResetRequest_WithEmptyEmail_CanBeCreated()
    {
        // Arrange & Act - empty string can be passed but controller should validate
        var request = new PasswordResetRequest(string.Empty);

        // Assert
        request.Email.Should().BeEmpty();
    }

    [Fact]
    public void PasswordResetConfirmRequest_WithShortPassword_CanBeCreated()
    {
        // Arrange & Act - short password can be passed but controller should validate
        var request = new PasswordResetConfirmRequest("token", "short");

        // Assert - Model can be created, validation happens in controller
        request.NewPassword.Should().Be("short");
    }

    #endregion
}

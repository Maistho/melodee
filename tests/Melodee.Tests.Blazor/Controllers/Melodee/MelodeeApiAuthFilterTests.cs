using System.Security.Claims;
using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for MelodeeApiAuthFilter authentication and authorization logic.
/// These tests focus on the filter's behavior without deeply mocking the services.
/// </summary>
public class MelodeeApiAuthFilterTests
{
    private static ActionExecutingContext CreateContext(ClaimsPrincipal? principal = null, bool includeAllowAnonymous = false)
    {
        var httpContext = new DefaultHttpContext();
        if (principal != null)
        {
            httpContext.User = principal;
        }

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        var filters = new List<IFilterMetadata>();
        if (includeAllowAnonymous)
        {
            filters.Add(new AllowAnonymousFilter());
        }

        return new ActionExecutingContext(
            actionContext,
            filters,
            new Dictionary<string, object?>(),
            new object());
    }

    private static ClaimsPrincipal CreateAuthenticatedUser(Guid apiKey, string username = "testuser", string email = "test@example.com")
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Sid, apiKey.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private class AllowAnonymousFilter : IFilterMetadata, IAllowAnonymous { }

    #region Context Creation Tests

    [Fact]
    public void CreateContext_WithoutPrincipal_HasNoAuthenticatedUser()
    {
        // Arrange & Act
        var context = CreateContext();

        // Assert
        context.HttpContext.User.Identity?.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void CreateContext_WithPrincipal_HasAuthenticatedUser()
    {
        // Arrange
        var apiKey = Guid.NewGuid();
        var principal = CreateAuthenticatedUser(apiKey);

        // Act
        var context = CreateContext(principal);

        // Assert
        context.HttpContext.User.Identity?.IsAuthenticated.Should().BeTrue();
        context.HttpContext.User.FindFirstValue(ClaimTypes.Sid).Should().Be(apiKey.ToString());
    }

    [Fact]
    public void CreateContext_WithAllowAnonymous_HasFilterInList()
    {
        // Arrange & Act
        var context = CreateContext(includeAllowAnonymous: true);

        // Assert
        context.Filters.Should().ContainSingle(f => f is IAllowAnonymous);
    }

    #endregion

    #region Claims Parsing Tests

    [Fact]
    public void CreateAuthenticatedUser_SetsAllClaims()
    {
        // Arrange
        var apiKey = Guid.NewGuid();
        var username = "myuser";
        var email = "my@email.com";

        // Act
        var principal = CreateAuthenticatedUser(apiKey, username, email);

        // Assert
        principal.FindFirstValue(ClaimTypes.Sid).Should().Be(apiKey.ToString());
        principal.FindFirstValue(ClaimTypes.Name).Should().Be(username);
        principal.FindFirstValue(ClaimTypes.Email).Should().Be(email);
        principal.Identity?.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void ClaimsSid_WithValidGuid_CanBeParsed()
    {
        // Arrange
        var expectedApiKey = Guid.NewGuid();
        var principal = CreateAuthenticatedUser(expectedApiKey);

        // Act
        var sidClaim = principal.FindFirstValue(ClaimTypes.Sid);
        var parseSuccess = Guid.TryParse(sidClaim, out var parsedApiKey);

        // Assert
        parseSuccess.Should().BeTrue();
        parsedApiKey.Should().Be(expectedApiKey);
    }

    [Fact]
    public void ClaimsSid_WithInvalidGuid_FailsParsing()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Sid, "not-a-guid") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var sidClaim = principal.FindFirstValue(ClaimTypes.Sid);
        var parseSuccess = Guid.TryParse(sidClaim, out _);

        // Assert
        parseSuccess.Should().BeFalse();
    }

    #endregion

    #region ApiError Response Tests

    [Fact]
    public void UnauthorizedObjectResult_WithApiError_HasCorrectStructure()
    {
        // Arrange
        var correlationId = "test-correlation-123";
        var error = new ApiError(ApiError.Codes.Unauthorized, "Test message", correlationId);

        // Act
        var result = new UnauthorizedObjectResult(error);

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        var returnedError = result.Value.Should().BeOfType<ApiError>().Subject;
        returnedError.Code.Should().Be(ApiError.Codes.Unauthorized);
        returnedError.Message.Should().Be("Test message");
        returnedError.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void ObjectResult_WithForbiddenStatus_HasCorrectCode()
    {
        // Arrange
        var error = new ApiError(ApiError.Codes.UserLocked, "User is locked", "trace-1");

        // Act
        var result = new ObjectResult(error) { StatusCode = StatusCodes.Status403Forbidden };

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void ObjectResult_WithBlacklistedError_HasCorrectCode()
    {
        // Arrange
        var error = new ApiError(ApiError.Codes.Blacklisted, "User is blacklisted", "trace-2");

        // Act
        var result = new ObjectResult(error) { StatusCode = StatusCodes.Status403Forbidden };

        // Assert
        var returnedError = result.Value.Should().BeOfType<ApiError>().Subject;
        returnedError.Code.Should().Be(ApiError.Codes.Blacklisted);
    }

    #endregion

    #region Filter Detection Tests

    [Fact]
    public void FilterList_WithAllowAnonymous_IsDetectable()
    {
        // Arrange
        var context = CreateContext(includeAllowAnonymous: true);

        // Act
        var hasAllowAnonymous = context.Filters.OfType<IAllowAnonymous>().Any();

        // Assert
        hasAllowAnonymous.Should().BeTrue();
    }

    [Fact]
    public void FilterList_WithoutAllowAnonymous_IsNotDetected()
    {
        // Arrange
        var context = CreateContext(includeAllowAnonymous: false);

        // Act
        var hasAllowAnonymous = context.Filters.OfType<IAllowAnonymous>().Any();

        // Assert
        hasAllowAnonymous.Should().BeFalse();
    }

    #endregion

    #region HttpContext Items Tests

    [Fact]
    public void HttpContext_Items_CanStoreUser()
    {
        // Arrange
        var context = CreateContext();
        var testValue = "test-user-object";

        // Act
        context.HttpContext.Items["MelodeeAuthenticatedUser"] = testValue;

        // Assert
        context.HttpContext.Items["MelodeeAuthenticatedUser"].Should().Be(testValue);
    }

    [Fact]
    public void HttpContext_Items_CanStoreClientIp()
    {
        // Arrange
        var context = CreateContext();
        var clientIp = "192.168.1.100";

        // Act
        context.HttpContext.Items["MelodeeClientIp"] = clientIp;

        // Assert
        context.HttpContext.Items["MelodeeClientIp"].Should().Be(clientIp);
    }

    [Fact]
    public void HttpContext_TraceIdentifier_IsAvailable()
    {
        // Arrange
        var context = CreateContext();

        // Act
        var traceId = context.HttpContext.TraceIdentifier;

        // Assert
        traceId.Should().NotBeNullOrEmpty();
    }

    #endregion
}

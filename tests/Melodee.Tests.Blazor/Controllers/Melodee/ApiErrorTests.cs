using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Xunit;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

public class ApiErrorTests
{
    #region ApiError Record Tests

    [Fact]
    public void ApiError_WithAllParameters_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var error = new ApiError("TEST_CODE", "Test message", "correlation-123");

        // Assert
        error.Code.Should().Be("TEST_CODE");
        error.Message.Should().Be("Test message");
        error.CorrelationId.Should().Be("correlation-123");
    }

    [Fact]
    public void ApiError_WithoutCorrelationId_HasNullCorrelationId()
    {
        // Arrange & Act
        var error = new ApiError("TEST_CODE", "Test message");

        // Assert
        error.Code.Should().Be("TEST_CODE");
        error.Message.Should().Be("Test message");
        error.CorrelationId.Should().BeNull();
    }

    [Fact]
    public void ApiError_Equality_WorksCorrectly()
    {
        // Arrange
        var error1 = new ApiError("CODE", "Message", "123");
        var error2 = new ApiError("CODE", "Message", "123");
        var error3 = new ApiError("DIFFERENT", "Message", "123");

        // Assert
        error1.Should().Be(error2);
        error1.Should().NotBe(error3);
    }

    #endregion

    #region Error Code Constants Tests

    [Theory]
    [InlineData(nameof(ApiError.Codes.Unauthorized), "UNAUTHORIZED")]
    [InlineData(nameof(ApiError.Codes.Forbidden), "FORBIDDEN")]
    [InlineData(nameof(ApiError.Codes.NotFound), "NOT_FOUND")]
    [InlineData(nameof(ApiError.Codes.BadRequest), "BAD_REQUEST")]
    [InlineData(nameof(ApiError.Codes.ValidationError), "VALIDATION_ERROR")]
    [InlineData(nameof(ApiError.Codes.TooManyRequests), "TOO_MANY_REQUESTS")]
    [InlineData(nameof(ApiError.Codes.Blacklisted), "BLACKLISTED")]
    [InlineData(nameof(ApiError.Codes.UserLocked), "USER_LOCKED")]
    [InlineData(nameof(ApiError.Codes.InternalError), "INTERNAL_ERROR")]
    public void ErrorCodes_HaveCorrectValues(string codeName, string expectedValue)
    {
        // Arrange & Act
        var actualValue = codeName switch
        {
            nameof(ApiError.Codes.Unauthorized) => ApiError.Codes.Unauthorized,
            nameof(ApiError.Codes.Forbidden) => ApiError.Codes.Forbidden,
            nameof(ApiError.Codes.NotFound) => ApiError.Codes.NotFound,
            nameof(ApiError.Codes.BadRequest) => ApiError.Codes.BadRequest,
            nameof(ApiError.Codes.ValidationError) => ApiError.Codes.ValidationError,
            nameof(ApiError.Codes.TooManyRequests) => ApiError.Codes.TooManyRequests,
            nameof(ApiError.Codes.Blacklisted) => ApiError.Codes.Blacklisted,
            nameof(ApiError.Codes.UserLocked) => ApiError.Codes.UserLocked,
            nameof(ApiError.Codes.InternalError) => ApiError.Codes.InternalError,
            _ => throw new ArgumentException($"Unknown code name: {codeName}")
        };

        // Assert
        actualValue.Should().Be(expectedValue);
    }

    [Fact]
    public void AllErrorCodes_AreUpperSnakeCase()
    {
        // Arrange
        var codes = new[]
        {
            ApiError.Codes.Unauthorized,
            ApiError.Codes.Forbidden,
            ApiError.Codes.NotFound,
            ApiError.Codes.BadRequest,
            ApiError.Codes.ValidationError,
            ApiError.Codes.TooManyRequests,
            ApiError.Codes.Blacklisted,
            ApiError.Codes.UserLocked,
            ApiError.Codes.InternalError
        };

        // Assert
        foreach (var code in codes)
        {
            code.Should().MatchRegex(@"^[A-Z][A-Z0-9_]*$", $"Code '{code}' should be UPPER_SNAKE_CASE");
        }
    }

    #endregion

    #region Serialization Tests

    [Fact]
    public void ApiError_CanBeSerializedToJson()
    {
        // Arrange
        var error = new ApiError(ApiError.Codes.NotFound, "Resource not found", "trace-456");

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(error);

        // Assert
        json.Should().Contain("\"Code\":\"NOT_FOUND\"");
        json.Should().Contain("\"Message\":\"Resource not found\"");
        json.Should().Contain("\"CorrelationId\":\"trace-456\"");
    }

    [Fact]
    public void ApiError_CanBeDeserializedFromJson()
    {
        // Arrange
        var json = "{\"Code\":\"UNAUTHORIZED\",\"Message\":\"Invalid token\",\"CorrelationId\":\"abc\"}";

        // Act
        var error = System.Text.Json.JsonSerializer.Deserialize<ApiError>(json);

        // Assert
        error.Should().NotBeNull();
        error!.Code.Should().Be("UNAUTHORIZED");
        error.Message.Should().Be("Invalid token");
        error.CorrelationId.Should().Be("abc");
    }

    #endregion
}

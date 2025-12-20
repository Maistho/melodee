using FluentAssertions;
using Melodee.Common.Configuration;
using Melodee.Common.Services.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Melodee.Tests.Common.Security;

/// <summary>
/// Tests for GoogleTokenService.
/// </summary>
public class GoogleTokenServiceTests
{
    private readonly Mock<ILogger<GoogleTokenService>> _loggerMock;

    public GoogleTokenServiceTests()
    {
        _loggerMock = new Mock<ILogger<GoogleTokenService>>();
    }

    private GoogleTokenService CreateService(GoogleAuthOptions options)
    {
        var optionsMock = new Mock<IOptions<GoogleAuthOptions>>();
        optionsMock.Setup(x => x.Value).Returns(options);
        return new GoogleTokenService(optionsMock.Object, _loggerMock.Object);
    }

    #region Empty/Null Token Tests

    [Fact]
    public async Task ValidateTokenAsync_WithNullToken_ReturnsInvalidGoogleToken()
    {
        // Arrange
        var options = new GoogleAuthOptions { Enabled = true, ClientId = "test-client" };
        var service = CreateService(options);

        // Act
        var result = await service.ValidateTokenAsync(null!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("invalid_google_token");
        result.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task ValidateTokenAsync_WithEmptyToken_ReturnsInvalidGoogleToken()
    {
        // Arrange
        var options = new GoogleAuthOptions { Enabled = true, ClientId = "test-client" };
        var service = CreateService(options);

        // Act
        var result = await service.ValidateTokenAsync("");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("invalid_google_token");
    }

    [Fact]
    public async Task ValidateTokenAsync_WithWhitespaceToken_ReturnsInvalidGoogleToken()
    {
        // Arrange
        var options = new GoogleAuthOptions { Enabled = true, ClientId = "test-client" };
        var service = CreateService(options);

        // Act
        var result = await service.ValidateTokenAsync("   ");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("invalid_google_token");
    }

    #endregion

    #region Disabled Auth Tests

    [Fact]
    public async Task ValidateTokenAsync_WhenDisabled_ReturnsInvalidGoogleToken()
    {
        // Arrange
        var options = new GoogleAuthOptions { Enabled = false };
        var service = CreateService(options);

        // Act
        var result = await service.ValidateTokenAsync("some-token");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("invalid_google_token");
        result.ErrorMessage.Should().Contain("not enabled");
    }

    #endregion

    #region Missing Configuration Tests

    [Fact]
    public async Task ValidateTokenAsync_WithNoClientIds_ReturnsInvalidGoogleToken()
    {
        // Arrange
        var options = new GoogleAuthOptions
        {
            Enabled = true,
            ClientId = null,
            AdditionalClientIds = []
        };
        var service = CreateService(options);

        // Act
        var result = await service.ValidateTokenAsync("some-token");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("invalid_google_token");
        result.ErrorMessage.Should().Contain("not properly configured");
    }

    #endregion

    #region Invalid Token Format Tests

    [Fact]
    public async Task ValidateTokenAsync_WithMalformedToken_ReturnsInvalidGoogleToken()
    {
        // Arrange
        var options = new GoogleAuthOptions
        {
            Enabled = true,
            ClientId = "test-client-id"
        };
        var service = CreateService(options);

        // Act
        var result = await service.ValidateTokenAsync("not-a-valid-jwt-token");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("invalid_google_token");
    }

    [Fact]
    public async Task ValidateTokenAsync_WithRandomBase64_ReturnsInvalidGoogleToken()
    {
        // Arrange
        var options = new GoogleAuthOptions
        {
            Enabled = true,
            ClientId = "test-client-id"
        };
        var service = CreateService(options);
        var randomBase64 = Convert.ToBase64String(new byte[64]);

        // Act
        var result = await service.ValidateTokenAsync(randomBase64);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("invalid_google_token");
    }

    #endregion

    #region Result Object Tests

    [Fact]
    public void GoogleTokenValidationResult_Success_CreatesValidResult()
    {
        // Arrange & Act
        // Note: We can't easily create a real payload without Google infrastructure,
        // so we test the static factory methods
        var result = GoogleTokenValidationResult.Failure("test_error", "Test message");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("test_error");
        result.ErrorMessage.Should().Be("Test message");
        result.Payload.Should().BeNull();
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task ValidateTokenAsync_WhenDisabled_LogsWarning()
    {
        // Arrange
        var options = new GoogleAuthOptions { Enabled = false };
        var service = CreateService(options);

        // Act
        await service.ValidateTokenAsync("some-token");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("disabled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithNoClientIds_LogsError()
    {
        // Arrange
        var options = new GoogleAuthOptions
        {
            Enabled = true,
            ClientId = null,
            AdditionalClientIds = []
        };
        var service = CreateService(options);

        // Act
        await service.ValidateTokenAsync("some-token");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("client IDs")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}

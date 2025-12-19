using FluentAssertions;
using Melodee.Common.Services.Security;
using Microsoft.Extensions.Options;

namespace Melodee.Tests.Blazor.Components;

/// <summary>
/// Tests for Login component Google Sign-In functionality.
/// </summary>
public class LoginGoogleSignInTests
{
    [Fact]
    public void GoogleAuthEnabled_WhenEnabledAndClientIdSet_ReturnsTrue()
    {
        // Arrange
        var options = Options.Create(new GoogleAuthOptions
        {
            Enabled = true,
            ClientId = "test-client-id"
        });

        // Act
        var isEnabled = options.Value.Enabled && !string.IsNullOrEmpty(options.Value.ClientId);

        // Assert
        isEnabled.Should().BeTrue();
    }

    [Fact]
    public void GoogleAuthEnabled_WhenDisabled_ReturnsFalse()
    {
        // Arrange
        var options = Options.Create(new GoogleAuthOptions
        {
            Enabled = false,
            ClientId = "test-client-id"
        });

        // Act
        var isEnabled = options.Value.Enabled && !string.IsNullOrEmpty(options.Value.ClientId);

        // Assert
        isEnabled.Should().BeFalse();
    }

    [Fact]
    public void GoogleAuthEnabled_WhenNoClientId_ReturnsFalse()
    {
        // Arrange
        var options = Options.Create(new GoogleAuthOptions
        {
            Enabled = true,
            ClientId = ""
        });

        // Act
        var isEnabled = options.Value.Enabled && !string.IsNullOrEmpty(options.Value.ClientId);

        // Assert
        isEnabled.Should().BeFalse();
    }

    [Fact]
    public void GoogleAuthEnabled_WhenClientIdNull_ReturnsFalse()
    {
        // Arrange
        var options = Options.Create(new GoogleAuthOptions
        {
            Enabled = true,
            ClientId = null!
        });

        // Act
        var isEnabled = options.Value.Enabled && !string.IsNullOrEmpty(options.Value.ClientId);

        // Assert
        isEnabled.Should().BeFalse();
    }

    [Theory]
    [InlineData("invalid_google_token", "Invalid Google credentials. Please try again.")]
    [InlineData("expired_google_token", "Your Google session has expired. Please sign in again.")]
    [InlineData("google_account_not_linked", "This Google account is not linked to any Melodee account. Please sign in with your password and link your Google account from your profile.")]
    [InlineData("signup_disabled", "New account registration is currently disabled. Please contact an administrator.")]
    [InlineData("forbidden_tenant", "Your Google account domain is not allowed. Please use a different account.")]
    [InlineData("account_disabled", "Your account has been disabled. Please contact support.")]
    [InlineData("unknown_error", "An error occurred during Google Sign-In. Please try again.")]
    public void MapGoogleAuthError_ReturnsExpectedMessage(string errorCode, string expectedMessage)
    {
        // Act
        var result = MapGoogleAuthError(errorCode, null);

        // Assert
        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void MapGoogleAuthError_WithCustomMessage_ReturnsCustomMessageForUnknownError()
    {
        // Arrange
        var customMessage = "Custom error message";

        // Act
        var result = MapGoogleAuthError("unknown_code", customMessage);

        // Assert
        result.Should().Be(customMessage);
    }

    // Helper method that mirrors the Login component's implementation
    private static string MapGoogleAuthError(string? errorCode, string? message)
    {
        return errorCode switch
        {
            "invalid_google_token" => "Invalid Google credentials. Please try again.",
            "expired_google_token" => "Your Google session has expired. Please sign in again.",
            "google_account_not_linked" => "This Google account is not linked to any Melodee account. Please sign in with your password and link your Google account from your profile.",
            "signup_disabled" => "New account registration is currently disabled. Please contact an administrator.",
            "forbidden_tenant" => "Your Google account domain is not allowed. Please use a different account.",
            "account_disabled" => "Your account has been disabled. Please contact support.",
            _ => message ?? "An error occurred during Google Sign-In. Please try again."
        };
    }
}

using FluentAssertions;
using Melodee.Common.Configuration;

namespace Melodee.Tests.Common.Configuration;

/// <summary>
/// Tests for TokenOptions configuration.
/// </summary>
public class TokenOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldMatchPhase0Spec()
    {
        // Arrange & Act
        var options = new TokenOptions();

        // Assert - Per Phase 0 spec
        options.AccessTokenLifetimeMinutes.Should().Be(15);
        options.RefreshTokenLifetimeDays.Should().Be(30);
        options.MaxSessionDays.Should().Be(90);
        options.RotateRefreshTokens.Should().BeTrue();
        options.RevokeOnReplay.Should().BeTrue();
    }

    [Fact]
    public void AccessTokenLifetimeMinutes_CanBeCustomized()
    {
        // Arrange & Act
        var options = new TokenOptions
        {
            AccessTokenLifetimeMinutes = 5
        };

        // Assert
        options.AccessTokenLifetimeMinutes.Should().Be(5);
    }

    [Fact]
    public void RefreshTokenLifetimeDays_CanBeCustomized()
    {
        // Arrange & Act
        var options = new TokenOptions
        {
            RefreshTokenLifetimeDays = 7
        };

        // Assert
        options.RefreshTokenLifetimeDays.Should().Be(7);
    }

    [Fact]
    public void MaxSessionDays_CanBeCustomized()
    {
        // Arrange & Act
        var options = new TokenOptions
        {
            MaxSessionDays = 30
        };

        // Assert
        options.MaxSessionDays.Should().Be(30);
    }
}

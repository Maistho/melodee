using FluentAssertions;
using Melodee.Common.Configuration;

namespace Melodee.Tests.Common.Configuration;

/// <summary>
/// Tests for GoogleAuthOptions configuration binding and validation.
/// </summary>
public class GoogleAuthOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new GoogleAuthOptions();

        // Assert
        options.Enabled.Should().BeFalse();
        options.ClientId.Should().BeNull();
        options.AdditionalClientIds.Should().BeEmpty();
        options.AllowedHostedDomains.Should().BeEmpty();
        options.AutoLinkEnabled.Should().BeFalse();
        options.ClockSkewSeconds.Should().Be(300);
    }

    [Fact]
    public void GetAllClientIds_WithPrimaryClientIdOnly_ReturnsOne()
    {
        // Arrange
        var options = new GoogleAuthOptions
        {
            ClientId = "primary-client-id"
        };

        // Act
        var clientIds = options.GetAllClientIds().ToList();

        // Assert
        clientIds.Should().HaveCount(1);
        clientIds.Should().Contain("primary-client-id");
    }

    [Fact]
    public void GetAllClientIds_WithPrimaryAndAdditional_ReturnsAll()
    {
        // Arrange
        var options = new GoogleAuthOptions
        {
            ClientId = "primary-client-id",
            AdditionalClientIds = ["android-client-id", "ios-client-id"]
        };

        // Act
        var clientIds = options.GetAllClientIds().ToList();

        // Assert
        clientIds.Should().HaveCount(3);
        clientIds.Should().Contain("primary-client-id");
        clientIds.Should().Contain("android-client-id");
        clientIds.Should().Contain("ios-client-id");
    }

    [Fact]
    public void GetAllClientIds_WithOnlyAdditional_ReturnsAdditional()
    {
        // Arrange
        var options = new GoogleAuthOptions
        {
            AdditionalClientIds = ["android-client-id"]
        };

        // Act
        var clientIds = options.GetAllClientIds().ToList();

        // Assert
        clientIds.Should().HaveCount(1);
        clientIds.Should().Contain("android-client-id");
    }

    [Fact]
    public void GetAllClientIds_WithEmptyStrings_FiltersEmptyValues()
    {
        // Arrange
        var options = new GoogleAuthOptions
        {
            ClientId = "primary",
            AdditionalClientIds = ["valid", "", null!]
        };

        // Act
        var clientIds = options.GetAllClientIds().ToList();

        // Assert
        clientIds.Should().HaveCount(2);
        clientIds.Should().Contain("primary");
        clientIds.Should().Contain("valid");
    }

    [Fact]
    public void Validate_WhenEnabledWithNoClientIds_ReturnsError()
    {
        // Arrange
        var options = new GoogleAuthOptions
        {
            Enabled = true,
            ClientId = null,
            AdditionalClientIds = []
        };

        // Act
        var results = options.Validate().ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Contain("ClientId");
    }

    [Fact]
    public void Validate_WhenEnabledWithClientId_ReturnsNoErrors()
    {
        // Arrange
        var options = new GoogleAuthOptions
        {
            Enabled = true,
            ClientId = "valid-client-id"
        };

        // Act
        var results = options.Validate().ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenEnabledWithAdditionalClientId_ReturnsNoErrors()
    {
        // Arrange
        var options = new GoogleAuthOptions
        {
            Enabled = true,
            AdditionalClientIds = ["valid-client-id"]
        };

        // Act
        var results = options.Validate().ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenDisabled_ReturnsNoErrors()
    {
        // Arrange
        var options = new GoogleAuthOptions
        {
            Enabled = false
        };

        // Act
        var results = options.Validate().ToList();

        // Assert
        results.Should().BeEmpty();
    }
}

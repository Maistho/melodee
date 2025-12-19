using FluentAssertions;
using Melodee.Common.Configuration;

namespace Melodee.Tests.Common.Configuration;

/// <summary>
/// Tests for AuthPolicyOptions configuration.
/// </summary>
public class AuthPolicyOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldMatchPhase0Spec()
    {
        // Arrange & Act
        var options = new AuthPolicyOptions();

        // Assert - Per Phase 0 spec: SelfRegistrationEnabled defaults to true
        options.SelfRegistrationEnabled.Should().BeTrue();
    }

    [Fact]
    public void SelfRegistrationEnabled_CanBeDisabled()
    {
        // Arrange & Act
        var options = new AuthPolicyOptions
        {
            SelfRegistrationEnabled = false
        };

        // Assert
        options.SelfRegistrationEnabled.Should().BeFalse();
    }
}

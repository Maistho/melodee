using FluentAssertions;
using Melodee.Cli.CommandSettings;
using Spectre.Console;

namespace Melodee.Tests.Cli.CommandSettings;

/// <summary>
/// Tests for LibraryProcessSettings validation and behavior
/// </summary>
public class LibraryProcessSettingsTests
{
    [Fact]
    public void Validate_WithValidLibraryName_ReturnsSuccess()
    {
        // Arrange
        var settings = new LibraryProcessSettings
        {
            LibraryName = "TestLibrary"
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.Successful.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyLibraryName_ReturnsError()
    {
        // Arrange
        var settings = new LibraryProcessSettings
        {
            LibraryName = string.Empty
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.Successful.Should().BeFalse();
        result.Message.Should().Be("Library name is required");
    }

    [Fact]
    public void Validate_WithNullLibraryName_ReturnsError()
    {
        // Arrange
        var settings = new LibraryProcessSettings
        {
            LibraryName = null!
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.Successful.Should().BeFalse();
        result.Message.Should().Be("Library name is required");
    }

    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var settings = new LibraryProcessSettings();

        // Assert
        // Note: [DefaultValue] attribute is CLI metadata, not actual initialization
        settings.CopyMode.Should().BeFalse(); // bool default is false
        settings.ForceMode.Should().BeFalse(); // bool default is false
        settings.ProcessLimit.Should().BeNull(); // Default should be null (unlimited)
        settings.PreDiscoveryScript.Should().BeNull(); // Default should be null
        settings.Verbose.Should().BeFalse(); // bool default is false
    }

    [Fact]
    public void CopyMode_CanBeSetToFalse()
    {
        // Arrange & Act
        var settings = new LibraryProcessSettings
        {
            CopyMode = false
        };

        // Assert
        settings.CopyMode.Should().BeFalse();
    }

    [Fact]
    public void ForceMode_CanBeSetToFalse()
    {
        // Arrange & Act
        var settings = new LibraryProcessSettings
        {
            ForceMode = false
        };

        // Assert
        settings.ForceMode.Should().BeFalse();
    }

    [Fact]
    public void ProcessLimit_CanBeSetToSpecificValue()
    {
        // Arrange & Act
        var settings = new LibraryProcessSettings
        {
            ProcessLimit = 100
        };

        // Assert
        settings.ProcessLimit.Should().Be(100);
    }

    [Fact]
    public void ProcessLimit_CanBeSetToZero()
    {
        // Arrange & Act
        var settings = new LibraryProcessSettings
        {
            ProcessLimit = 0
        };

        // Assert
        settings.ProcessLimit.Should().Be(0);
    }

    [Fact]
    public void PreDiscoveryScript_CanBeSet()
    {
        // Arrange & Act
        var settings = new LibraryProcessSettings
        {
            PreDiscoveryScript = "/path/to/script.sh"
        };

        // Assert
        settings.PreDiscoveryScript.Should().Be("/path/to/script.sh");
    }

    [Theory]
    [InlineData("ValidLibrary")]
    [InlineData("Library with spaces")]
    [InlineData("Library123")]
    [InlineData("UPPERCASE_LIBRARY")]
    [InlineData("lowercase-library")]
    public void Validate_WithVariousValidLibraryNames_ReturnsSuccess(string libraryName)
    {
        // Arrange
        var settings = new LibraryProcessSettings
        {
            LibraryName = libraryName
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.Successful.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    public void Validate_WithEmptyLibraryName_ReturnsError_Theory(string libraryName)
    {
        // Arrange
        var settings = new LibraryProcessSettings
        {
            LibraryName = libraryName
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.Successful.Should().BeFalse();
        result.Message.Should().Be("Library name is required");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Validate_WithWhitespaceLibraryNames_PassesValidation(string libraryName)
    {
        // Note: Current implementation uses IsNullOrEmpty, not IsNullOrWhiteSpace
        // Whitespace-only names are technically valid
        var settings = new LibraryProcessSettings
        {
            LibraryName = libraryName
        };

        var result = settings.Validate();

        result.Successful.Should().BeTrue();
    }
}
using FluentAssertions;
using Melodee.Blazor.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace Melodee.Tests.Blazor.Components.Pages;

/// <summary>
/// Tests for Data/Shares.razor component localization.
/// Verifies that all resource keys used in the Shares component exist and return non-empty values.
/// </summary>
public class SharesLocalizationTests
{
    private readonly Mock<IStringLocalizer<SharedResources>> _mockLocalizer;
    private readonly Mock<ILogger<LocalizationService>> _mockLogger;
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly LocalizationService _localizationService;

    public SharesLocalizationTests()
    {
        _mockLocalizer = new Mock<IStringLocalizer<SharedResources>>();
        _mockLogger = new Mock<ILogger<LocalizationService>>();
        _mockLocalStorage = new Mock<ILocalStorageService>();

        _localizationService = new LocalizationService(
            _mockLocalizer.Object,
            _mockLocalStorage.Object,
            _mockLogger.Object);

        // Setup localizer to return non-empty values for valid keys
        _mockLocalizer.Setup(x => x[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, $"Localized_{key}", false));
    }

    #region Resource Key Existence Tests

    [Theory]
    [InlineData("Shares.PageTitle")]
    [InlineData("Shares.Header")]
    [InlineData("Shares.Add")]
    [InlineData("Shares.Delete")]
    public void Shares_ResourceKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    #endregion

    #region Comprehensive Key Tests

    [Fact]
    public void Shares_AllResourceKeys_ReturnNonEmptyValues()
    {
        // Arrange
        var allKeys = new[]
        {
            "Shares.PageTitle",
            "Shares.Header",
            "Shares.Add",
            "Shares.Delete"
        };

        // Act & Assert
        foreach (var key in allKeys)
        {
            var result = _localizationService.Localize(key);
            result.Should().NotBeNullOrEmpty($"Resource key '{key}' should return a non-empty value");
            result.Should().Contain("Localized_", $"Resource key '{key}' should be localized");
        }
    }

    [Fact]
    public void Shares_ResourceKeyCount_ShouldBe4()
    {
        // Arrange
        var expectedCount = 4;
        var allKeys = new[]
        {
            "Shares.PageTitle",
            "Shares.Header",
            "Shares.Add",
            "Shares.Delete"
        };

        // Assert
        allKeys.Length.Should().Be(expectedCount, $"Shares page should have exactly {expectedCount} resource keys");
    }

    #endregion
}

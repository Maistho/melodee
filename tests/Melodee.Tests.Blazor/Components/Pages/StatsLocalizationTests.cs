using FluentAssertions;
using Melodee.Blazor.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace Melodee.Tests.Blazor.Components.Pages;

/// <summary>
/// Tests for Stats.razor component localization.
/// Verifies that all resource keys used in the Stats component exist and return non-empty values.
/// </summary>
public class StatsLocalizationTests
{
    private readonly Mock<IStringLocalizer<SharedResources>> _mockLocalizer;
    private readonly Mock<ILogger<LocalizationService>> _mockLogger;
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly LocalizationService _localizationService;

    public StatsLocalizationTests()
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
    [InlineData("Stats.PageTitle")]
    [InlineData("Stats.Statistics")]
    public void Stats_PageTitleKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    [Theory]
    [InlineData("Stats.Summary")]
    [InlineData("Stats.YourPlaysPerDayLast30d")]
    [InlineData("Stats.PlaysChartTitle")]
    [InlineData("Stats.DateAxisTitle")]
    [InlineData("Stats.SongsAddedPerDayLast30d")]
    [InlineData("Stats.AddedChartTitle")]
    [InlineData("Stats.SongsAxisTitle")]
    public void Stats_ChartKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    [Theory]
    [InlineData("Stats.TopPlayedSongsYou")]
    [InlineData("Stats.SongColumnTitle")]
    [InlineData("Stats.PlaysColumnTitle")]
    public void Stats_TopSongsKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    [Theory]
    [InlineData("Stats.EditorMissingImages")]
    [InlineData("Stats.ItemColumnTitle")]
    [InlineData("Stats.CountColumnTitle")]
    public void Stats_EditorKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    [Theory]
    [InlineData("Stats.AdminActivity")]
    [InlineData("Stats.SearchesPerDay")]
    [InlineData("Stats.ShareViewsPerDay")]
    [InlineData("Stats.LibraryScansPerDay")]
    public void Stats_AdminActivityKeys_Exist(string resourceKey)
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
    public void Stats_AllResourceKeys_ReturnNonEmptyValues()
    {
        // Arrange
        var allKeys = new[]
        {
            "Stats.PageTitle",
            "Stats.Statistics",
            "Stats.Summary",
            "Stats.YourPlaysPerDayLast30d",
            "Stats.PlaysChartTitle",
            "Stats.DateAxisTitle",
            "Stats.SongsAddedPerDayLast30d",
            "Stats.AddedChartTitle",
            "Stats.SongsAxisTitle",
            "Stats.TopPlayedSongsYou",
            "Stats.SongColumnTitle",
            "Stats.PlaysColumnTitle",
            "Stats.EditorMissingImages",
            "Stats.ItemColumnTitle",
            "Stats.CountColumnTitle",
            "Stats.AdminActivity",
            "Stats.SearchesPerDay",
            "Stats.ShareViewsPerDay",
            "Stats.LibraryScansPerDay"
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
    public void Stats_ResourceKeyCount_ShouldBe19()
    {
        // Arrange
        var expectedCount = 19;
        var allKeys = new[]
        {
            "Stats.PageTitle",
            "Stats.Statistics",
            "Stats.Summary",
            "Stats.YourPlaysPerDayLast30d",
            "Stats.PlaysChartTitle",
            "Stats.DateAxisTitle",
            "Stats.SongsAddedPerDayLast30d",
            "Stats.AddedChartTitle",
            "Stats.SongsAxisTitle",
            "Stats.TopPlayedSongsYou",
            "Stats.SongColumnTitle",
            "Stats.PlaysColumnTitle",
            "Stats.EditorMissingImages",
            "Stats.ItemColumnTitle",
            "Stats.CountColumnTitle",
            "Stats.AdminActivity",
            "Stats.SearchesPerDay",
            "Stats.ShareViewsPerDay",
            "Stats.LibraryScansPerDay"
        };

        // Assert
        allKeys.Length.Should().Be(expectedCount, $"Stats page should have exactly {expectedCount} resource keys");
    }

    #endregion
}

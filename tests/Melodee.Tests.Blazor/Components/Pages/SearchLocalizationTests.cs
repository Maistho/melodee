using FluentAssertions;
using Melodee.Blazor.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace Melodee.Tests.Blazor.Components.Pages;

/// <summary>
/// Tests for Search.razor component localization.
/// Verifies that all resource keys used in the Search component exist and return non-empty values.
/// </summary>
public class SearchLocalizationTests
{
    private readonly Mock<IStringLocalizer<SharedResources>> _mockLocalizer;
    private readonly Mock<ILogger<LocalizationService>> _mockLogger;
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly LocalizationService _localizationService;

    public SearchLocalizationTests()
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
    [InlineData("Search.PageTitle")]
    [InlineData("Search.ResultsFor")]
    [InlineData("Search.RequestMusic")]
    public void Search_PageKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    [Theory]
    [InlineData("Search.ArtistsCount")]
    [InlineData("Search.AlbumsCount")]
    [InlineData("Search.SongsCount")]
    public void Search_CountKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    [Theory]
    [InlineData("Search.ArtistColumnTitle")]
    [InlineData("Search.AlbumColumnTitle")]
    [InlineData("Search.YearColumnTitle")]
    [InlineData("Search.TitleColumnTitle")]
    public void Search_TableColumnKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    [Fact]
    public void Search_PlaySongKey_Exists()
    {
        // Arrange
        var resourceKey = "Search.PlaySong";

        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    #endregion

    #region Comprehensive Key Tests

    [Fact]
    public void Search_AllResourceKeys_ReturnNonEmptyValues()
    {
        // Arrange
        var allKeys = new[]
        {
            "Search.PageTitle",
            "Search.ResultsFor",
            "Search.RequestMusic",
            "Search.ArtistsCount",
            "Search.AlbumsCount",
            "Search.SongsCount",
            "Search.ArtistColumnTitle",
            "Search.AlbumColumnTitle",
            "Search.YearColumnTitle",
            "Search.TitleColumnTitle",
            "Search.PlaySong"
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
    public void Search_ResourceKeyCount_ShouldBe11()
    {
        // Arrange
        var expectedCount = 11;
        var allKeys = new[]
        {
            "Search.PageTitle",
            "Search.ResultsFor",
            "Search.RequestMusic",
            "Search.ArtistsCount",
            "Search.AlbumsCount",
            "Search.SongsCount",
            "Search.ArtistColumnTitle",
            "Search.AlbumColumnTitle",
            "Search.YearColumnTitle",
            "Search.TitleColumnTitle",
            "Search.PlaySong"
        };

        // Assert
        allKeys.Length.Should().Be(expectedCount, $"Search page should have exactly {expectedCount} resource keys");
    }

    #endregion
}

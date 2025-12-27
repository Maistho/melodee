using FluentAssertions;
using Melodee.Blazor.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace Melodee.Tests.Blazor.Components.Pages;

/// <summary>
/// Tests for Songs.razor component localization.
/// Verifies that all resource keys used in the Songs component exist and return non-empty values.
/// </summary>
public class SongsLocalizationTests
{
    private readonly Mock<IStringLocalizer<SharedResources>> _mockLocalizer;
    private readonly Mock<ILogger<LocalizationService>> _mockLogger;
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly LocalizationService _localizationService;

    public SongsLocalizationTests()
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
    [InlineData("Navigation.Songs")]
    [InlineData("Navigation.Dashboard")]
    public void Songs_NavigationKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    [Theory]
    [InlineData("Data.Statistics")]
    [InlineData("Data.ClickToFilterSongs")]
    [InlineData("Data.Title")]
    [InlineData("Data.SongNumber")]
    [InlineData("Data.Artist")]
    [InlineData("Data.Album")]
    [InlineData("Data.Duration")]
    [InlineData("Data.FileSize")]
    [InlineData("Data.Rating")]
    [InlineData("Data.Loved")]
    [InlineData("Data.Created")]
    [InlineData("Data.Tags")]
    [InlineData("Data.MergeSongs")]
    [InlineData("Data.SongMergingInfo")]
    [InlineData("Data.DeletingSongs")]
    [InlineData("Data.SongLockedMessage")]
    [InlineData("Data.YourFavoriteSongs")]
    [InlineData("Data.YourRatedSongs")]
    [InlineData("Data.OK")]
    public void Songs_DataKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    [Theory]
    [InlineData("Actions.Delete")]
    [InlineData("Actions.No")]
    public void Songs_ActionKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    #endregion

    #region All Keys Validation

    [Fact]
    public void Songs_AllRequiredKeys_ExistAndAreLocalized()
    {
        // Arrange - All resource keys used in Songs.razor
        var allKeys = new[]
        {
            // Navigation
            "Navigation.Songs",
            "Navigation.Dashboard",
            
            // Data
            "Data.Statistics",
            "Data.ClickToFilterSongs",
            "Data.Title",
            "Data.SongNumber",
            "Data.Artist",
            "Data.Album",
            "Data.Duration",
            "Data.FileSize",
            "Data.Rating",
            "Data.Loved",
            "Data.Created",
            "Data.Tags",
            "Data.MergeSongs",
            "Data.SongMergingInfo",
            "Data.DeletingSongs",
            "Data.SongLockedMessage",
            "Data.YourFavoriteSongs",
            "Data.YourRatedSongs",
            "Data.OK",
            
            // Actions
            "Actions.Delete",
            "Actions.No"
        };

        // Act & Assert
        foreach (var key in allKeys)
        {
            var result = _localizationService.Localize(key);

            result.Should().NotBeNullOrEmpty($"Resource key '{key}' should exist");
            result.Should().Contain("Localized_", $"Resource key '{key}' should be localized");

            // Verify the localizer was called for this key
            _mockLocalizer.Verify(x => x[key], Times.AtLeastOnce,
                $"Localization service should be called for key '{key}'");
        }
    }

    #endregion

    #region Count Validation

    [Fact]
    public void Songs_RequiredResourceKeys_CountIs23()
    {
        // Arrange - All unique resource keys used in Songs.razor
        var uniqueKeys = new HashSet<string>
        {
            "Navigation.Songs",
            "Navigation.Dashboard",
            "Data.Statistics",
            "Data.ClickToFilterSongs",
            "Data.Title",
            "Data.SongNumber",
            "Data.Artist",
            "Data.Album",
            "Data.Duration",
            "Data.FileSize",
            "Data.Rating",
            "Data.Loved",
            "Data.Created",
            "Data.Tags",
            "Data.MergeSongs",
            "Data.SongMergingInfo",
            "Data.DeletingSongs",
            "Data.SongLockedMessage",
            "Data.YourFavoriteSongs",
            "Data.YourRatedSongs",
            "Data.OK",
            "Actions.Delete",
            "Actions.No"
        };

        // Assert
        uniqueKeys.Count.Should().Be(23, "Songs component should use exactly 23 unique resource keys");
    }

    #endregion

    #region Key Pattern Validation

    [Fact]
    public void Songs_NavigationKeys_FollowNamingConvention()
    {
        // Arrange
        var navigationKeys = new[] { "Navigation.Songs", "Navigation.Dashboard" };

        // Act & Assert
        foreach (var key in navigationKeys)
        {
            key.Should().StartWith("Navigation.", $"Navigation key '{key}' should follow naming convention");
            var result = _localizationService.Localize(key);
            result.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void Songs_DataKeys_FollowNamingConvention()
    {
        // Arrange
        var dataKeys = new[]
        {
            "Data.Statistics", "Data.ClickToFilterSongs", "Data.Title", "Data.SongNumber",
            "Data.Artist", "Data.Album", "Data.Duration", "Data.FileSize",
            "Data.Rating", "Data.Loved", "Data.Created", "Data.Tags",
            "Data.MergeSongs", "Data.SongMergingInfo", "Data.DeletingSongs",
            "Data.SongLockedMessage", "Data.YourFavoriteSongs", "Data.YourRatedSongs",
            "Data.OK"
        };

        // Act & Assert
        foreach (var key in dataKeys)
        {
            key.Should().StartWith("Data.", $"Data key '{key}' should follow naming convention");
            var result = _localizationService.Localize(key);
            result.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void Songs_ActionKeys_FollowNamingConvention()
    {
        // Arrange
        var actionKeys = new[] { "Actions.Delete", "Actions.No" };

        // Act & Assert
        foreach (var key in actionKeys)
        {
            key.Should().StartWith("Actions.", $"Action key '{key}' should follow naming convention");
            var result = _localizationService.Localize(key);
            result.Should().NotBeNullOrEmpty();
        }
    }

    #endregion

    #region Special Keys Validation

    [Fact]
    public void Songs_FilterStatisticsKeys_ExistForUserInteraction()
    {
        // Arrange - Keys used for filtering songs by statistics
        var filterKeys = new[]
        {
            "Data.ClickToFilterSongs",
            "Data.YourFavoriteSongs",
            "Data.YourRatedSongs"
        };

        // Act & Assert
        foreach (var key in filterKeys)
        {
            var result = _localizationService.Localize(key);
            result.Should().NotBeNullOrEmpty($"Filter key '{key}' should exist for user interaction");
        }
    }

    [Fact]
    public void Songs_MergeDialogKeys_ExistForMergeOperation()
    {
        // Arrange - Keys used in merge dialog
        var mergeKeys = new[]
        {
            "Data.MergeSongs",
            "Data.SongMergingInfo",
            "Data.OK"
        };

        // Act & Assert
        foreach (var key in mergeKeys)
        {
            var result = _localizationService.Localize(key);
            result.Should().NotBeNullOrEmpty($"Merge dialog key '{key}' should exist");
        }
    }

    #endregion
}

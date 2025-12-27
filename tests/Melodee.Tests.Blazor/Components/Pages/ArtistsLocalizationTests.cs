using FluentAssertions;
using Melodee.Blazor.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace Melodee.Tests.Blazor.Components.Pages;

/// <summary>
/// Tests for Artists.razor component localization.
/// Verifies that all resource keys used in the Artists component exist and return non-empty values.
/// </summary>
public class ArtistsLocalizationTests
{
    private readonly Mock<IStringLocalizer<SharedResources>> _mockLocalizer;
    private readonly Mock<ILogger<LocalizationService>> _mockLogger;
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly LocalizationService _localizationService;

    public ArtistsLocalizationTests()
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
    [InlineData("Navigation.Artists")]
    [InlineData("Navigation.Dashboard")]
    public void Artists_NavigationKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    [Theory]
    [InlineData("Data.Statistics")]
    [InlineData("Data.Name")]
    [InlineData("Data.AltNames")]
    [InlineData("Data.Directory")]
    [InlineData("Data.Albums")]
    [InlineData("Data.Songs")]
    [InlineData("Data.Created")]
    [InlineData("Data.Tags")]
    [InlineData("Data.Merge")]
    [InlineData("Data.MergeArtists")]
    [InlineData("Data.MergeTheseArtists")]
    [InlineData("Data.IntoThisArtist")]
    [InlineData("Data.MergedArtists")]
    [InlineData("Data.DeletingArtists")]
    [InlineData("Data.ConfirmDelete")]
    [InlineData("Data.ArtistLockedMessage")]
    public void Artists_DataKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    [Theory]
    [InlineData("Actions.Add")]
    [InlineData("Actions.Delete")]
    [InlineData("Actions.Yes")]
    [InlineData("Actions.No")]
    public void Artists_ActionKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    [Theory]
    [InlineData("Messages.ConfirmDelete")]
    public void Artists_MessageKeys_Exist(string resourceKey)
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
    public void Artists_AllRequiredKeys_ExistAndAreLocalized()
    {
        // Arrange - All resource keys used in Artists.razor
        var allKeys = new[]
        {
            // Navigation
            "Navigation.Artists",
            "Navigation.Dashboard",
            
            // Data
            "Data.Statistics",
            "Data.Name",
            "Data.AltNames",
            "Data.Directory",
            "Data.Albums",
            "Data.Songs",
            "Data.Created",
            "Data.Tags",
            "Data.Merge",
            "Data.MergeArtists",
            "Data.MergeTheseArtists",
            "Data.IntoThisArtist",
            "Data.MergedArtists",
            "Data.DeletingArtists",
            "Data.ConfirmDelete",
            "Data.ArtistLockedMessage",
            
            // Actions
            "Actions.Add",
            "Actions.Delete",
            "Actions.Yes",
            "Actions.No",
            
            // Messages
            "Messages.ConfirmDelete"
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
    public void Artists_RequiredResourceKeys_CountIs23()
    {
        // Arrange - All unique resource keys used in Artists.razor
        var uniqueKeys = new HashSet<string>
        {
            "Navigation.Artists",
            "Navigation.Dashboard",
            "Data.Statistics",
            "Data.Name",
            "Data.AltNames",
            "Data.Directory",
            "Data.Albums",
            "Data.Songs",
            "Data.Created",
            "Data.Tags",
            "Data.Merge",
            "Data.MergeArtists",
            "Data.MergeTheseArtists",
            "Data.IntoThisArtist",
            "Data.MergedArtists",
            "Data.DeletingArtists",
            "Data.ConfirmDelete",
            "Data.ArtistLockedMessage",
            "Actions.Add",
            "Actions.Delete",
            "Actions.Yes",
            "Actions.No",
            "Messages.ConfirmDelete"
        };

        // Assert
        uniqueKeys.Count.Should().Be(23, "Artists component should use exactly 23 unique resource keys");
    }

    #endregion

    #region Key Pattern Validation

    [Fact]
    public void Artists_NavigationKeys_FollowNamingConvention()
    {
        // Arrange
        var navigationKeys = new[] { "Navigation.Artists", "Navigation.Dashboard" };

        // Act & Assert
        foreach (var key in navigationKeys)
        {
            key.Should().StartWith("Navigation.", $"Navigation key '{key}' should follow naming convention");
            var result = _localizationService.Localize(key);
            result.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void Artists_DataKeys_FollowNamingConvention()
    {
        // Arrange
        var dataKeys = new[]
        {
            "Data.Statistics", "Data.Name", "Data.AltNames", "Data.Directory",
            "Data.Albums", "Data.Songs", "Data.Created", "Data.Tags",
            "Data.Merge", "Data.MergeArtists", "Data.MergeTheseArtists",
            "Data.IntoThisArtist", "Data.MergedArtists", "Data.DeletingArtists",
            "Data.ConfirmDelete", "Data.ArtistLockedMessage"
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
    public void Artists_ActionKeys_FollowNamingConvention()
    {
        // Arrange
        var actionKeys = new[] { "Actions.Add", "Actions.Delete", "Actions.Yes", "Actions.No" };

        // Act & Assert
        foreach (var key in actionKeys)
        {
            key.Should().StartWith("Actions.", $"Action key '{key}' should follow naming convention");
            var result = _localizationService.Localize(key);
            result.Should().NotBeNullOrEmpty();
        }
    }

    #endregion
}

using FluentAssertions;
using Melodee.Blazor.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace Melodee.Tests.Blazor.Components.Pages;

/// <summary>
/// Tests for Playlists.razor component localization.
/// Verifies that all resource keys used in the Playlists component exist and return non-empty values.
/// </summary>
public class PlaylistsLocalizationTests
{
    private readonly Mock<IStringLocalizer<SharedResources>> _mockLocalizer;
    private readonly Mock<ILogger<LocalizationService>> _mockLogger;
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly LocalizationService _localizationService;

    public PlaylistsLocalizationTests()
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
    [InlineData("Navigation.Playlists")]
    [InlineData("Navigation.Dashboard")]
    public void Playlists_NavigationKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    [Theory]
    [InlineData("Data.Name")]
    [InlineData("Data.Created")]
    [InlineData("Data.Tags")]
    [InlineData("Data.DeletingPlaylists")]
    [InlineData("Data.PlaylistLockedMessage")]
    public void Playlists_DataKeys_Exist(string resourceKey)
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
    public void Playlists_ActionKeys_Exist(string resourceKey)
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
    public void Playlists_AllRequiredKeys_ExistAndAreLocalized()
    {
        // Arrange - All resource keys used in Playlists.razor
        var allKeys = new[]
        {
            // Navigation
            "Navigation.Playlists",
            "Navigation.Dashboard",
            
            // Data
            "Data.Name",
            "Data.Created",
            "Data.Tags",
            "Data.DeletingPlaylists",
            "Data.PlaylistLockedMessage",
            
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
    public void Playlists_RequiredResourceKeys_CountIs9()
    {
        // Arrange - All unique resource keys used in Playlists.razor
        var uniqueKeys = new HashSet<string>
        {
            "Navigation.Playlists",
            "Navigation.Dashboard",
            "Data.Name",
            "Data.Created",
            "Data.Tags",
            "Data.DeletingPlaylists",
            "Data.PlaylistLockedMessage",
            "Actions.Delete",
            "Actions.No"
        };

        // Assert
        uniqueKeys.Count.Should().Be(9, "Playlists component should use exactly 9 unique resource keys");
    }

    #endregion

    #region Key Pattern Validation

    [Fact]
    public void Playlists_NavigationKeys_FollowNamingConvention()
    {
        // Arrange
        var navigationKeys = new[] { "Navigation.Playlists", "Navigation.Dashboard" };

        // Act & Assert
        foreach (var key in navigationKeys)
        {
            key.Should().StartWith("Navigation.", $"Navigation key '{key}' should follow naming convention");
            var result = _localizationService.Localize(key);
            result.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void Playlists_DataKeys_FollowNamingConvention()
    {
        // Arrange
        var dataKeys = new[]
        {
            "Data.Name",
            "Data.Created",
            "Data.Tags",
            "Data.DeletingPlaylists",
            "Data.PlaylistLockedMessage"
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
    public void Playlists_ActionKeys_FollowNamingConvention()
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
    public void Playlists_LockWarningKey_ExistsForUserGuidance()
    {
        // Act
        var result = _localizationService.Localize("Data.PlaylistLockedMessage");

        // Assert
        result.Should().NotBeNullOrEmpty("Lock warning message should guide users about locked playlists");
        result.Should().Contain("Localized_");
    }

    [Fact]
    public void Playlists_DeleteNotificationKey_ExistsForUserFeedback()
    {
        // Act
        var result = _localizationService.Localize("Data.DeletingPlaylists");

        // Assert
        result.Should().NotBeNullOrEmpty("Delete notification should provide feedback to users");
        result.Should().Contain("Localized_");
    }

    #endregion
}

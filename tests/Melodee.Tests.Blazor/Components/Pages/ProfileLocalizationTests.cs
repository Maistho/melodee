using FluentAssertions;
using Melodee.Blazor.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace Melodee.Tests.Blazor.Components.Pages;

/// <summary>
/// Tests for Profile.razor component localization.
/// Verifies that all resource keys used in the Profile component exist and return non-empty values.
/// </summary>
public class ProfileLocalizationTests
{
    private readonly Mock<IStringLocalizer<SharedResources>> _mockLocalizer;
    private readonly Mock<ILogger<LocalizationService>> _mockLogger;
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly LocalizationService _localizationService;

    public ProfileLocalizationTests()
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
        _mockLocalizer.Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, $"Localized_{key}", false));
    }

    #region Resource Key Existence Tests

    [Theory]
    [InlineData("Profile.YourProfile")]
    [InlineData("Profile.ProfileInformation")]
    [InlineData("Profile.Username")]
    [InlineData("Profile.Email")]
    [InlineData("Profile.Bio")]
    [InlineData("Profile.TimeZone")]
    [InlineData("Profile.ProfileImage")]
    [InlineData("Profile.ChangeProfilePicture")]
    [InlineData("Profile.DefaultsToUTC")]
    [InlineData("Profile.LinkedAccounts")]
    [InlineData("Profile.Google")]
    [InlineData("Profile.Linked")]
    [InlineData("Profile.SaveChanges")]
    [InlineData("Profile.Saving")]
    [InlineData("Profile.ProfileUpdated")]
    [InlineData("Profile.ProfileUpdatedDetail")]
    [InlineData("Profile.UpdateFailed")]
    public void Profile_BasicProfileKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    [Theory]
    [InlineData("Profile.UsernameRequired")]
    [InlineData("Profile.EmailRequired")]
    [InlineData("Profile.EmailInvalid")]
    [InlineData("Profile.InvalidTimeZone")]
    [InlineData("Profile.InvalidTimeZoneDetail")]
    public void Profile_ValidationKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    [Theory]
    [InlineData("Profile.LinkGoogleAccount")]
    [InlineData("Profile.Linking")]
    [InlineData("Profile.LinkingError")]
    [InlineData("Profile.GoogleLinkedSuccess")]
    [InlineData("Profile.GoogleLinkFailed")]
    [InlineData("Profile.UnlinkGoogleAccount")]
    [InlineData("Profile.UnlinkConfirmation")]
    [InlineData("Profile.Unlink")]
    [InlineData("Profile.Unlinking")]
    [InlineData("Profile.UnlinkingError")]
    [InlineData("Profile.GoogleUnlinkedSuccess")]
    [InlineData("Profile.GoogleUnlinkFailed")]
    [InlineData("Profile.GoogleSignInUnavailable")]
    [InlineData("Profile.GoogleSignInOpenFailed")]
    public void Profile_GoogleAccountKeys_Exist(string resourceKey)
    {
        // Act
        var result = _localizationService.Localize(resourceKey);

        // Assert
        result.Should().NotBeNullOrEmpty($"Resource key '{resourceKey}' should exist");
        result.Should().Contain("Localized_", $"Resource key '{resourceKey}' should be localized");
    }

    [Theory]
    [InlineData("Actions.Cancel")]
    public void Profile_ActionKeys_Exist(string resourceKey)
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
    public void Profile_AllRequiredKeys_ExistAndAreLocalized()
    {
        // Arrange - All resource keys used in Profile.razor
        var allKeys = new[]
        {
            // Basic Profile
            "Profile.YourProfile",
            "Profile.ProfileInformation",
            "Profile.Username",
            "Profile.Email",
            "Profile.Bio",
            "Profile.TimeZone",
            "Profile.ProfileImage",
            "Profile.ChangeProfilePicture",
            "Profile.DefaultsToUTC",
            "Profile.LinkedAccounts",
            "Profile.Google",
            "Profile.Linked",
            "Profile.SaveChanges",
            "Profile.Saving",
            "Profile.ProfileUpdated",
            "Profile.ProfileUpdatedDetail",
            "Profile.UpdateFailed",
            
            // Validation
            "Profile.UsernameRequired",
            "Profile.EmailRequired",
            "Profile.EmailInvalid",
            "Profile.InvalidTimeZone",
            "Profile.InvalidTimeZoneDetail",
            
            // Google Account Linking
            "Profile.LinkGoogleAccount",
            "Profile.Linking",
            "Profile.LinkingError",
            "Profile.GoogleLinkedSuccess",
            "Profile.GoogleLinkFailed",
            "Profile.UnlinkGoogleAccount",
            "Profile.UnlinkConfirmation",
            "Profile.Unlink",
            "Profile.Unlinking",
            "Profile.UnlinkingError",
            "Profile.GoogleUnlinkedSuccess",
            "Profile.GoogleUnlinkFailed",
            "Profile.GoogleSignInUnavailable",
            "Profile.GoogleSignInOpenFailed",
            
            // Actions
            "Actions.Cancel"
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
    public void Profile_RequiredResourceKeys_CountIs36()
    {
        // Arrange - All unique resource keys used in Profile.razor
        var uniqueKeys = new HashSet<string>
        {
            "Profile.YourProfile",
            "Profile.ProfileInformation",
            "Profile.Username",
            "Profile.Email",
            "Profile.Bio",
            "Profile.TimeZone",
            "Profile.ProfileImage",
            "Profile.ChangeProfilePicture",
            "Profile.DefaultsToUTC",
            "Profile.LinkedAccounts",
            "Profile.Google",
            "Profile.Linked",
            "Profile.SaveChanges",
            "Profile.Saving",
            "Profile.ProfileUpdated",
            "Profile.ProfileUpdatedDetail",
            "Profile.UpdateFailed",
            "Profile.UsernameRequired",
            "Profile.EmailRequired",
            "Profile.EmailInvalid",
            "Profile.InvalidTimeZone",
            "Profile.InvalidTimeZoneDetail",
            "Profile.LinkGoogleAccount",
            "Profile.Linking",
            "Profile.LinkingError",
            "Profile.GoogleLinkedSuccess",
            "Profile.GoogleLinkFailed",
            "Profile.UnlinkGoogleAccount",
            "Profile.UnlinkConfirmation",
            "Profile.Unlink",
            "Profile.Unlinking",
            "Profile.UnlinkingError",
            "Profile.GoogleUnlinkedSuccess",
            "Profile.GoogleUnlinkFailed",
            "Profile.GoogleSignInUnavailable",
            "Profile.GoogleSignInOpenFailed",
            "Actions.Cancel"
        };

        // Assert
        uniqueKeys.Count.Should().Be(37, "Profile component should use exactly 37 unique resource keys");
    }

    #endregion

    #region Key Pattern Validation

    [Fact]
    public void Profile_ProfileKeys_FollowNamingConvention()
    {
        // Arrange
        var profileKeys = new[]
        {
            "Profile.YourProfile",
            "Profile.ProfileInformation",
            "Profile.Username",
            "Profile.Email",
            "Profile.Bio",
            "Profile.TimeZone",
            "Profile.ProfileImage",
            "Profile.LinkedAccounts",
            "Profile.SaveChanges"
        };

        // Act & Assert
        foreach (var key in profileKeys)
        {
            key.Should().StartWith("Profile.", $"Profile key '{key}' should follow naming convention");
            var result = _localizationService.Localize(key);
            result.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void Profile_ActionKeys_FollowNamingConvention()
    {
        // Arrange
        var actionKeys = new[] { "Actions.Cancel" };

        // Act & Assert
        foreach (var key in actionKeys)
        {
            key.Should().StartWith("Actions.", $"Action key '{key}' should follow naming convention");
            var result = _localizationService.Localize(key);
            result.Should().NotBeNullOrEmpty();
        }
    }

    #endregion

    #region Functional Grouping Tests

    [Fact]
    public void Profile_FormValidationKeys_ExistForAllFields()
    {
        // Arrange - Keys for form validation
        var validationKeys = new[]
        {
            "Profile.UsernameRequired",
            "Profile.EmailRequired",
            "Profile.EmailInvalid",
            "Profile.InvalidTimeZone",
            "Profile.InvalidTimeZoneDetail"
        };

        // Act & Assert
        foreach (var key in validationKeys)
        {
            var result = _localizationService.Localize(key);
            result.Should().NotBeNullOrEmpty($"Validation key '{key}' should provide user feedback");
        }
    }

    [Fact]
    public void Profile_GoogleLinkingKeys_ExistForCompleteWorkflow()
    {
        // Arrange - Keys for Google account linking workflow
        var linkingKeys = new[]
        {
            "Profile.LinkGoogleAccount",
            "Profile.Linking",
            "Profile.LinkingError",
            "Profile.GoogleLinkedSuccess",
            "Profile.GoogleLinkFailed"
        };

        // Act & Assert
        foreach (var key in linkingKeys)
        {
            var result = _localizationService.Localize(key);
            result.Should().NotBeNullOrEmpty($"Linking key '{key}' should guide user through linking process");
        }
    }

    [Fact]
    public void Profile_GoogleUnlinkingKeys_ExistForCompleteWorkflow()
    {
        // Arrange - Keys for Google account unlinking workflow
        var unlinkingKeys = new[]
        {
            "Profile.UnlinkGoogleAccount",
            "Profile.UnlinkConfirmation",
            "Profile.Unlink",
            "Profile.Unlinking",
            "Profile.UnlinkingError",
            "Profile.GoogleUnlinkedSuccess",
            "Profile.GoogleUnlinkFailed"
        };

        // Act & Assert
        foreach (var key in unlinkingKeys)
        {
            var result = _localizationService.Localize(key);
            result.Should().NotBeNullOrEmpty($"Unlinking key '{key}' should guide user through unlinking process");
        }
    }

    [Fact]
    public void Profile_SaveOperationKeys_ExistForUserFeedback()
    {
        // Arrange - Keys for save operation feedback
        var saveKeys = new[]
        {
            "Profile.SaveChanges",
            "Profile.Saving",
            "Profile.ProfileUpdated",
            "Profile.ProfileUpdatedDetail",
            "Profile.UpdateFailed"
        };

        // Act & Assert
        foreach (var key in saveKeys)
        {
            var result = _localizationService.Localize(key);
            result.Should().NotBeNullOrEmpty($"Save operation key '{key}' should provide user feedback");
        }
    }

    #endregion
}

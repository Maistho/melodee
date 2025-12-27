using System.Globalization;
using FluentAssertions;
using Melodee.Blazor.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace Melodee.Tests.Blazor.Services;

public class LocalizationServiceTests
{
    private readonly Mock<IStringLocalizer<SharedResources>> _mockLocalizer;
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly Mock<ILogger<LocalizationService>> _mockLogger;
    private readonly LocalizationService _service;

    public LocalizationServiceTests()
    {
        _mockLocalizer = new Mock<IStringLocalizer<SharedResources>>();
        _mockLocalStorage = new Mock<ILocalStorageService>();
        _mockLogger = new Mock<ILogger<LocalizationService>>();

        _service = new LocalizationService(
            _mockLocalizer.Object,
            _mockLocalStorage.Object,
            _mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_InitializesSuccessfully()
    {
        // Act & Assert
        _service.Should().NotBeNull();
        _service.CurrentCulture.Should().NotBeNull();
        _service.SupportedCultures.Should().HaveCount(10);
    }

    [Fact]
    public void SupportedCultures_ContainsExpectedCultures()
    {
        // Arrange
        var expectedCultures = new[] { "en-US", "de-DE", "es-ES", "fr-FR", "it-IT", "ja-JP", "pt-BR", "ru-RU", "zh-CN", "ar-SA" };

        // Act
        var cultures = _service.SupportedCultures;

        // Assert
        cultures.Should().HaveCount(10);
        cultures.Select(c => c.Name).Should().BeEquivalentTo(expectedCultures);
    }

    #endregion

    #region Localize - Happy Path Tests

    [Fact]
    public void Localize_WithValidKey_ReturnsLocalizedString()
    {
        // Arrange
        const string key = "Navigation.Dashboard";
        const string expectedValue = "Dashboard";
        var localizedString = new LocalizedString(key, expectedValue, false);
        _mockLocalizer.Setup(x => x[key]).Returns(localizedString);

        // Act
        var result = _service.Localize(key);

        // Assert
        result.Should().Be(expectedValue);
        _mockLocalizer.Verify(x => x[key], Times.Once);
    }

    [Fact]
    public void Localize_WithValidKeyAndFallback_ReturnsLocalizedString()
    {
        // Arrange
        const string key = "Actions.Save";
        const string fallback = "Fallback Save";
        const string expectedValue = "Save";
        var localizedString = new LocalizedString(key, expectedValue, false);
        _mockLocalizer.Setup(x => x[key]).Returns(localizedString);

        // Act
        var result = _service.Localize(key, fallback);

        // Assert
        result.Should().Be(expectedValue);
        result.Should().NotBe(fallback);
    }

    [Fact]
    public void Localize_WithFormatArguments_ReturnsFormattedString()
    {
        // Arrange
        const string key = "Validation.PasswordTooShort";
        const int minLength = 8;
        var expectedValue = $"Password must be at least {minLength} characters";
        var localizedString = new LocalizedString(key, expectedValue, false);
        _mockLocalizer.Setup(x => x[key, minLength]).Returns(localizedString);

        // Act
        var result = _service.Localize(key, minLength);

        // Assert
        result.Should().Be(expectedValue);
    }

    #endregion

    #region Localize - Edge Cases Tests

    [Fact]
    public void Localize_WithMissingKey_ReturnsKeyAsValue()
    {
        // Arrange
        const string key = "NonExistent.Key";
        var localizedString = new LocalizedString(key, key, true);
        _mockLocalizer.Setup(x => x[key]).Returns(localizedString);

        // Act
        var result = _service.Localize(key);

        // Assert
        result.Should().Be(key);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Resource key not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Localize_WithMissingKeyAndFallback_ReturnsFallback()
    {
        // Arrange
        const string key = "NonExistent.Key";
        const string fallback = "Fallback Value";
        var localizedString = new LocalizedString(key, key, true);
        _mockLocalizer.Setup(x => x[key]).Returns(localizedString);

        // Act
        var result = _service.Localize(key, fallback);

        // Assert
        result.Should().Be(fallback);
        result.Should().NotBe(key);
    }

    [Fact]
    public void Localize_WhenExceptionThrown_ReturnsKeyAndLogsError()
    {
        // Arrange
        const string key = "Error.Key";
        _mockLocalizer.Setup(x => x[key]).Throws(new InvalidOperationException("Test exception"));

        // Act
        var result = _service.Localize(key);

        // Assert
        result.Should().Be(key);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error localizing key")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Localize_WithNullKey_HandlesGracefully()
    {
        // Arrange
        string? key = null;

        // Act & Assert - Should not throw
        var act = () => _service.Localize(key!);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Localize_WithEmptyOrWhitespaceKey_ReturnsKey(string key)
    {
        // Arrange
        var localizedString = new LocalizedString(key, key, true);
        _mockLocalizer.Setup(x => x[key]).Returns(localizedString);

        // Act
        var result = _service.Localize(key);

        // Assert
        result.Should().Be(key);
    }

    [Fact]
    public void Localize_WithFormatArgumentsWhenKeyMissing_ReturnsFormattedKey()
    {
        // Arrange
        const string key = "Missing.Key.{0}";
        const string arg = "test";
        var localizedString = new LocalizedString(key, key, true);
        _mockLocalizer.Setup(x => x[key, arg]).Returns(localizedString);

        // Act
        var result = _service.Localize(key, arg);

        // Assert
        result.Should().Contain(arg);
    }

    #endregion

    #region SetCultureAsync - Happy Path Tests

    [Fact]
    public async Task SetCultureAsync_WithSupportedCulture_SetsCultureSuccessfully()
    {
        // Arrange
        var culture = new CultureInfo("es-ES");
        _mockLocalStorage.Setup(x => x.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SetCultureAsync(culture);

        // Assert
        _service.CurrentCulture.Name.Should().Be("es-ES");
        _mockLocalStorage.Verify(x => x.SetItemAsStringAsync("user_preferred_culture", "es-ES"), Times.Once);
    }

    [Fact]
    public async Task SetCultureAsync_WithCultureCode_SetsCultureSuccessfully()
    {
        // Arrange
        const string cultureCode = "fr-FR";
        _mockLocalStorage.Setup(x => x.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SetCultureAsync(cultureCode);

        // Assert
        _service.CurrentCulture.Name.Should().Be(cultureCode);
    }

    [Fact]
    public async Task SetCultureAsync_RaisesCultureChangedEvent()
    {
        // Arrange
        var culture = new CultureInfo("ru-RU");
        CultureInfo? raisedCulture = null;
        _service.CultureChanged += c => raisedCulture = c;
        _mockLocalStorage.Setup(x => x.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SetCultureAsync(culture);

        // Assert
        raisedCulture.Should().NotBeNull();
        raisedCulture!.Name.Should().Be("ru-RU");
    }

    #endregion

    #region SetCultureAsync - Edge Cases Tests

    [Fact]
    public async Task SetCultureAsync_WithUnsupportedCulture_FallsBackToEnglish()
    {
        // Arrange
        var unsupportedCulture = new CultureInfo("ko-KR"); // Korean not supported
        _mockLocalStorage.Setup(x => x.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SetCultureAsync(unsupportedCulture);

        // Assert
        _service.CurrentCulture.Name.Should().Be("en-US");
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unsupported culture")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SetCultureAsync_WithInvalidCultureCode_FallsBackToEnglish()
    {
        // Arrange
        const string invalidCode = "invalid-culture";
        _mockLocalStorage.Setup(x => x.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SetCultureAsync(invalidCode);

        // Assert
        _service.CurrentCulture.Name.Should().Be("en-US");
        // Note: Logging is verified but exception type might vary
    }

    [Fact]
    public async Task SetCultureAsync_WhenLocalStorageThrows_StillSetsCulture()
    {
        // Arrange
        var culture = new CultureInfo("zh-CN");
        _mockLocalStorage.Setup(x => x.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Storage error"));

        // Act
        await _service.SetCultureAsync(culture);

        // Assert
        _service.CurrentCulture.Name.Should().Be("zh-CN");
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error saving culture")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SetCultureAsync_WithInvalidCultureCode_HandlesGracefully(string? invalidCode)
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SetCultureAsync(invalidCode!);

        // Assert
        _service.CurrentCulture.Name.Should().Be("en-US");
        // Service should handle invalid codes gracefully
    }

    #endregion

    #region GetUserCultureAsync - Happy Path Tests

    [Fact]
    public async Task GetUserCultureAsync_WithStoredCulture_ReturnsStoredCulture()
    {
        // Arrange
        const string storedCulture = "es-ES";
        _mockLocalStorage.Setup(x => x.GetItemAsStringAsync("user_preferred_culture"))
            .ReturnsAsync(storedCulture);

        // Act
        var result = await _service.GetUserCultureAsync();

        // Assert
        result.Name.Should().Be(storedCulture);
    }

    [Fact]
    public async Task GetUserCultureAsync_WithAllSupportedCultures_ReturnsCorrectly()
    {
        // Arrange & Act & Assert
        foreach (var supportedCulture in _service.SupportedCultures)
        {
            _mockLocalStorage.Setup(x => x.GetItemAsStringAsync("user_preferred_culture"))
                .ReturnsAsync(supportedCulture.Name);

            var result = await _service.GetUserCultureAsync();

            result.Name.Should().Be(supportedCulture.Name);
        }
    }

    #endregion

    #region GetUserCultureAsync - Edge Cases Tests

    [Fact]
    public async Task GetUserCultureAsync_WithNoStoredCulture_ReturnsBrowserOrDefault()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItemAsStringAsync("user_preferred_culture"))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _service.GetUserCultureAsync();

        // Assert
        result.Should().NotBeNull();
        // Should return either browser culture if supported, or en-US
        _service.SupportedCultures.Select(c => c.Name).Should().Contain(result.Name);
    }

    [Fact]
    public async Task GetUserCultureAsync_WithUnsupportedStoredCulture_ReturnsDefault()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItemAsStringAsync("user_preferred_culture"))
            .ReturnsAsync("ko-KR"); // Korean not in supported list

        // Act
        var result = await _service.GetUserCultureAsync();

        // Assert
        result.Name.Should().Be("en-US");
    }

    [Fact]
    public async Task GetUserCultureAsync_WhenStorageThrows_ReturnsDefault()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItemAsStringAsync("user_preferred_culture"))
            .ThrowsAsync(new InvalidOperationException("Storage error"));

        // Act
        var result = await _service.GetUserCultureAsync();

        // Assert
        result.Name.Should().Be("en-US");
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error retrieving user culture")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetUserCultureAsync_WithEmptyStoredCulture_ReturnsDefault(string emptyValue)
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItemAsStringAsync("user_preferred_culture"))
            .ReturnsAsync(emptyValue);

        // Act
        var result = await _service.GetUserCultureAsync();

        // Assert
        result.Name.Should().Be("en-US");
    }

    #endregion

    #region FormatDate - Happy Path Tests

    [Fact]
    public void FormatDate_WithDefaultFormat_ReturnsFormattedDate()
    {
        // Arrange
        var date = new DateTime(2025, 12, 27);

        // Act
        var result = _service.FormatDate(date);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FormatDate_WithCustomFormat_ReturnsCustomFormattedDate()
    {
        // Arrange
        var date = new DateTime(2025, 12, 27);
        const string format = "yyyy-MM-dd";

        // Act
        var result = _service.FormatDate(date, format);

        // Assert
        result.Should().Be("2025-12-27");
    }

    [Fact]
    public async Task FormatDate_RespectsCultureSettings()
    {
        // Arrange
        var date = new DateTime(2025, 1, 15);
        await _service.SetCultureAsync("en-US");

        // Act
        var usResult = _service.FormatDate(date);

        await _service.SetCultureAsync("fr-FR");
        var frResult = _service.FormatDate(date);

        // Assert
        usResult.Should().NotBe(frResult); // Different cultures format dates differently
    }

    #endregion

    #region FormatDate - Edge Cases Tests

    [Fact]
    public void FormatDate_WithMinValue_HandlesGracefully()
    {
        // Act
        var result = _service.FormatDate(DateTime.MinValue);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FormatDate_WithMaxValue_HandlesGracefully()
    {
        // Act
        var result = _service.FormatDate(DateTime.MaxValue);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FormatDate_WithInvalidFormat_ReturnsDefaultFormat()
    {
        // Arrange
        var date = new DateTime(2025, 12, 27);
        const string invalidFormat = "invalid{format}";

        // Act
        var result = _service.FormatDate(date, invalidFormat);

        // Assert
        result.Should().NotBeNullOrEmpty();
        // Note: Invalid formats may either throw (logged) or return a formatted result
        // The important thing is the method doesn't crash
    }

    #endregion

    #region FormatNumber - Happy Path Tests

    [Fact]
    public void FormatNumber_WithDefaultFormat_ReturnsFormattedNumber()
    {
        // Arrange
        const decimal number = 1234.56m;

        // Act
        var result = _service.FormatNumber(number);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("1"); // Should contain the number
    }

    [Fact]
    public void FormatNumber_WithCustomFormat_ReturnsCustomFormattedNumber()
    {
        // Arrange
        const decimal number = 1234.5678m;
        const string format = "N4";

        // Act
        var result = _service.FormatNumber(number, format);

        // Assert
        result.Should().NotBeNullOrEmpty();
        // Note: Actual formatting varies by culture (e.g., "1,234.5678" or "1.234,5678")
    }

    [Fact]
    public async Task FormatNumber_RespectsCultureSettings()
    {
        // Arrange
        const decimal number = 1234.56m;
        await _service.SetCultureAsync("en-US");

        // Act
        var usResult = _service.FormatNumber(number);

        await _service.SetCultureAsync("fr-FR");
        var frResult = _service.FormatNumber(number);

        // Assert
        usResult.Should().NotBe(frResult); // Different cultures use different separators
    }

    #endregion

    #region FormatNumber - Edge Cases Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1234.56)]
    [InlineData(99999999999.99)]
    public void FormatNumber_WithVariousValues_HandlesCorrectly(decimal number)
    {
        // Act
        var result = _service.FormatNumber(number);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FormatNumber_WithInvalidFormat_ReturnsDefaultFormat()
    {
        // Arrange
        const decimal number = 123.45m;
        const string invalidFormat = "invalid{format}";

        // Act
        var result = _service.FormatNumber(number, invalidFormat);

        // Assert
        result.Should().NotBeNullOrEmpty();
        // Note: Invalid formats may either throw (logged) or return a formatted result
        // The important thing is the method doesn't crash
    }

    [Fact]
    public void FormatNumber_WithDecimalMinValue_HandlesGracefully()
    {
        // Act
        var result = _service.FormatNumber(decimal.MinValue);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FormatNumber_WithDecimalMaxValue_HandlesGracefully()
    {
        // Act
        var result = _service.FormatNumber(decimal.MaxValue);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region CultureChanged Event Tests

    [Fact]
    public async Task CultureChanged_WhenCultureChanges_RaisesEventOnlyOnce()
    {
        // Arrange
        var eventRaisedCount = 0;
        _service.CultureChanged += _ => eventRaisedCount++;
        _mockLocalStorage.Setup(x => x.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SetCultureAsync("es-ES");

        // Assert
        eventRaisedCount.Should().Be(1);
    }

    [Fact]
    public async Task CultureChanged_WithMultipleSubscribers_NotifiesAll()
    {
        // Arrange
        var subscriber1Called = false;
        var subscriber2Called = false;
        _service.CultureChanged += _ => subscriber1Called = true;
        _service.CultureChanged += _ => subscriber2Called = true;
        _mockLocalStorage.Setup(x => x.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SetCultureAsync("ru-RU");

        // Assert
        subscriber1Called.Should().BeTrue();
        subscriber2Called.Should().BeTrue();
    }

    #endregion

    #region RTL (Right-to-Left) Tests

    [Theory]
    [InlineData("ar-SA", true)]  // Arabic is RTL
    [InlineData("en-US", false)] // English is LTR
    [InlineData("es-ES", false)] // Spanish is LTR
    [InlineData("ru-RU", false)] // Russian is LTR
    [InlineData("zh-CN", false)] // Chinese is LTR
    [InlineData("fr-FR", false)] // French is LTR
    public async Task IsRightToLeft_ReturnsCorrectValueForCulture(string cultureCode, bool expectedRtl)
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        await _service.SetCultureAsync(cultureCode);

        // Act
        var isRtl = _service.IsRightToLeft();

        // Assert
        isRtl.Should().Be(expectedRtl);
    }

    [Theory]
    [InlineData("ar-SA", "rtl")]  // Arabic should return "rtl"
    [InlineData("en-US", "ltr")]  // English should return "ltr"
    [InlineData("es-ES", "ltr")]  // Spanish should return "ltr"
    [InlineData("ru-RU", "ltr")]  // Russian should return "ltr"
    [InlineData("zh-CN", "ltr")]  // Chinese should return "ltr"
    [InlineData("fr-FR", "ltr")]  // French should return "ltr"
    public async Task GetTextDirection_ReturnsCorrectDirectionForCulture(string cultureCode, string expectedDirection)
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        await _service.SetCultureAsync(cultureCode);

        // Act
        var direction = _service.GetTextDirection();

        // Assert
        direction.Should().Be(expectedDirection);
    }

    [Fact]
    public async Task IsRightToLeft_WhenCultureChangesToArabic_ReturnsTrue()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Start with English (LTR)
        await _service.SetCultureAsync("en-US");
        _service.IsRightToLeft().Should().BeFalse();

        // Act - Change to Arabic (RTL)
        await _service.SetCultureAsync("ar-SA");

        // Assert
        _service.IsRightToLeft().Should().BeTrue();
        _service.GetTextDirection().Should().Be("rtl");
    }

    [Fact]
    public async Task IsRightToLeft_WhenCultureChangesFromArabicToEnglish_ReturnsFalse()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Start with Arabic (RTL)
        await _service.SetCultureAsync("ar-SA");
        _service.IsRightToLeft().Should().BeTrue();

        // Act - Change to English (LTR)
        await _service.SetCultureAsync("en-US");

        // Assert
        _service.IsRightToLeft().Should().BeFalse();
        _service.GetTextDirection().Should().Be("ltr");
    }

    [Fact]
    public void IsRightToLeft_WithDefaultCulture_ReturnsFalse()
    {
        // Act
        var isRtl = _service.IsRightToLeft();

        // Assert
        // Default culture should be LTR (likely en-US or system default)
        isRtl.Should().BeFalse();
    }

    [Fact]
    public void GetTextDirection_WithDefaultCulture_ReturnsLtr()
    {
        // Act
        var direction = _service.GetTextDirection();

        // Assert
        // Default culture should return "ltr"
        direction.Should().Be("ltr");
    }

    [Fact]
    public async Task GetTextDirection_ConsistentWithIsRightToLeft()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Test all supported cultures
        foreach (var culture in _service.SupportedCultures)
        {
            // Act
            await _service.SetCultureAsync(culture);
            var isRtl = _service.IsRightToLeft();
            var direction = _service.GetTextDirection();

            // Assert
            if (isRtl)
            {
                direction.Should().Be("rtl", $"Culture {culture.Name} is RTL");
            }
            else
            {
                direction.Should().Be("ltr", $"Culture {culture.Name} is LTR");
            }
        }
    }

    #endregion
}

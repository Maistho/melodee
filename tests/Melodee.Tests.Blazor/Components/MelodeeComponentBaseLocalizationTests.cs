using System.Globalization;
using System.Security.Claims;
using Bunit;
using FluentAssertions;
using Melodee.Blazor.Components.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Radzen;

namespace Melodee.Tests.Blazor.Components;

/// <summary>
/// Tests for MelodeeComponentBase localization functionality.
/// Uses a test component that inherits from MelodeeComponentBase.
/// </summary>
public class MelodeeComponentBaseLocalizationTests : BunitContext
{
    private readonly Mock<ILocalizationService> _mockLocalizationService;
    private readonly Mock<IMelodeeConfigurationFactory> _mockConfigurationFactory;
    private readonly Mock<IMelodeeConfiguration> _mockConfiguration;
    private readonly Mock<AuthenticationStateProvider> _mockAuthStateProvider;

    public MelodeeComponentBaseLocalizationTests()
    {
        _mockLocalizationService = new Mock<ILocalizationService>();
        _mockConfigurationFactory = new Mock<IMelodeeConfigurationFactory>();
        _mockConfiguration = new Mock<IMelodeeConfiguration>();
        _mockAuthStateProvider = new Mock<AuthenticationStateProvider>();

        // Setup configuration
        _mockConfigurationFactory.Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConfiguration.Object);

        _mockConfiguration.Setup(x => x.GetValue<int>(It.Is<string>(s => s.Contains("ToastAutoCloseTime"))))
            .Returns(3000);
        _mockConfiguration.Setup(x => x.GetValue<int>(It.Is<string>(s => s.Contains("DefaultPageSize"))))
            .Returns(25);
        _mockConfiguration.Setup(x => x.GetValue<int[]>(It.Is<string>(s => s.Contains("DefaultPageSizeOptions"))))
            .Returns([10, 20, 30]);

        // Setup authentication
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var authState = Task.FromResult(new AuthenticationState(claimsPrincipal));
        _mockAuthStateProvider.Setup(x => x.GetAuthenticationStateAsync()).Returns(authState);

        // Setup localization service defaults
        _mockLocalizationService.Setup(x => x.CurrentCulture)
            .Returns(new CultureInfo("en-US"));
        _mockLocalizationService.Setup(x => x.GetUserCultureAsync())
            .ReturnsAsync(new CultureInfo("en-US"));
        _mockLocalizationService.Setup(x => x.SetCultureAsync(It.IsAny<CultureInfo>()))
            .Returns(Task.CompletedTask);

        // Register services
        Services.AddSingleton(_mockLocalizationService.Object);
        Services.AddSingleton(_mockConfigurationFactory.Object);
        Services.AddSingleton(_mockConfiguration.Object);
        Services.AddSingleton(_mockAuthStateProvider.Object);
        Services.AddSingleton<DialogService>();
        Services.AddSingleton<NotificationService>();
        Services.AddSingleton<TooltipService>();

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Initialization Tests

    [Fact]
    public async Task OnInitializedAsync_LoadsUserCultureFromService()
    {
        // Arrange
        var expectedCulture = new CultureInfo("es-ES");
        _mockLocalizationService.Setup(x => x.GetUserCultureAsync())
            .ReturnsAsync(expectedCulture);

        // Act
        var cut = Render<TestLocalizableComponent>();
        await cut.InvokeAsync(() => Task.CompletedTask); // Ensure initialization completes

        // Assert
        _mockLocalizationService.Verify(x => x.GetUserCultureAsync(), Times.Once);
        _mockLocalizationService.Verify(x => x.SetCultureAsync(expectedCulture), Times.Once);
    }

    [Fact]
    public async Task OnInitializedAsync_WhenCultureServiceThrows_PropagatesException()
    {
        // Arrange
        _mockLocalizationService.Setup(x => x.GetUserCultureAsync())
            .ThrowsAsync(new InvalidOperationException("Culture error"));

        // Act & Assert - Should propagate exception (no error handling in component)
        var act = () => Render<TestLocalizableComponent>();

        await Task.Run(() => act.Should().Throw<InvalidOperationException>()
            .WithMessage("Culture error"));
    }

    #endregion

    #region L() Method - Simple Localization Tests

    [Fact]
    public void L_WithValidKey_ReturnsLocalizedString()
    {
        // Arrange
        const string key = "Navigation.Dashboard";
        const string expectedValue = "Dashboard";
        _mockLocalizationService.Setup(x => x.Localize(key))
            .Returns(expectedValue);

        var cut = Render<TestLocalizableComponent>();

        // Act
        var result = cut.Instance.TestL(key);

        // Assert
        result.Should().Be(expectedValue);
        _mockLocalizationService.Verify(x => x.Localize(key), Times.Once);
    }

    [Fact]
    public void L_WithMissingKey_ReturnsKey()
    {
        // Arrange
        const string key = "Missing.Key";
        _mockLocalizationService.Setup(x => x.Localize(key))
            .Returns(key);

        var cut = Render<TestLocalizableComponent>();

        // Act
        var result = cut.Instance.TestL(key);

        // Assert
        result.Should().Be(key);
    }

    [Theory]
    [InlineData("Navigation.Dashboard")]
    [InlineData("Actions.Save")]
    [InlineData("Auth.Login")]
    [InlineData("Messages.Loading")]
    public void L_WithVariousKeys_CallsServiceCorrectly(string key)
    {
        // Arrange
        _mockLocalizationService.Setup(x => x.Localize(key))
            .Returns($"Localized_{key}");

        var cut = Render<TestLocalizableComponent>();

        // Act
        var result = cut.Instance.TestL(key);

        // Assert
        result.Should().Contain("Localized_");
        _mockLocalizationService.Verify(x => x.Localize(key), Times.Once);
    }

    #endregion

    #region L() Method with Fallback Tests

    [Fact]
    public void L_WithFallback_WhenKeyExists_ReturnsLocalizedString()
    {
        // Arrange
        const string key = "Actions.Save";
        const string fallback = "Fallback Save";
        const string expectedValue = "Save";
        _mockLocalizationService.Setup(x => x.Localize(key, fallback))
            .Returns(expectedValue);

        var cut = Render<TestLocalizableComponent>();

        // Act
        var result = cut.Instance.TestLWithFallback(key, fallback);

        // Assert
        result.Should().Be(expectedValue);
        result.Should().NotBe(fallback);
    }

    [Fact]
    public void L_WithFallback_WhenKeyMissing_ReturnsFallback()
    {
        // Arrange
        const string key = "Missing.Key";
        const string fallback = "Default Text";
        _mockLocalizationService.Setup(x => x.Localize(key, fallback))
            .Returns(fallback);

        var cut = Render<TestLocalizableComponent>();

        // Act
        var result = cut.Instance.TestLWithFallback(key, fallback);

        // Assert
        result.Should().Be(fallback);
    }

    [Theory]
    [InlineData("", "Empty Key Fallback")]
    [InlineData(null, "Null Key Fallback")]
    [InlineData("   ", "Whitespace Key Fallback")]
    public void L_WithFallback_HandlesInvalidKeys(string? key, string fallback)
    {
        // Arrange
        _mockLocalizationService.Setup(x => x.Localize(key!, fallback))
            .Returns(fallback);

        var cut = Render<TestLocalizableComponent>();

        // Act
        var result = cut.Instance.TestLWithFallback(key!, fallback);

        // Assert
        result.Should().Be(fallback);
    }

    #endregion

    #region L() Method with Format Arguments Tests

    [Fact]
    public void L_WithFormatArgs_ReturnsFormattedString()
    {
        // Arrange
        const string key = "Messages.ItemCount";
        const int count = 5;
        const string expectedValue = "You have 5 items";
        _mockLocalizationService.Setup(x => x.Localize(key, count))
            .Returns(expectedValue);

        var cut = Render<TestLocalizableComponent>();

        // Act
        var result = cut.Instance.TestLWithArgs(key, count);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void L_WithMultipleFormatArgs_ReturnsFormattedString()
    {
        // Arrange
        const string key = "Messages.UserGreeting";
        const string name = "John";
        const int age = 30;
        const string expectedValue = "Hello John, you are 30 years old";
        _mockLocalizationService.Setup(x => x.Localize(key, name, age))
            .Returns(expectedValue);

        var cut = Render<TestLocalizableComponent>();

        // Act
        var result = cut.Instance.TestLWithArgs(key, name, age);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void L_WithNullFormatArg_HandlesGracefully()
    {
        // Arrange
        const string key = "Messages.WithNull";
        object? nullArg = null;
        _mockLocalizationService.Setup(x => x.Localize(key, nullArg!))
            .Returns("Message with null");

        var cut = Render<TestLocalizableComponent>();

        // Act
        var result = cut.Instance.TestLWithArgs(key, nullArg!);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region FormatDate Tests

    [Fact]
    public void FormatDate_WithValidDate_ReturnsFormattedString()
    {
        // Arrange
        var date = new DateTime(2025, 12, 27);
        const string expectedValue = "12/27/2025";
        _mockLocalizationService.Setup(x => x.FormatDate(date, null))
            .Returns(expectedValue);

        var cut = Render<TestLocalizableComponent>();

        // Act
        var result = cut.Instance.TestFormatDate(date);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void FormatDate_WithCustomFormat_ReturnsCustomFormattedString()
    {
        // Arrange
        var date = new DateTime(2025, 12, 27);
        const string format = "yyyy-MM-dd";
        const string expectedValue = "2025-12-27";
        _mockLocalizationService.Setup(x => x.FormatDate(date, format))
            .Returns(expectedValue);

        var cut = Render<TestLocalizableComponent>();

        // Act
        var result = cut.Instance.TestFormatDate(date, format);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("d")]
    [InlineData("D")]
    [InlineData("yyyy-MM-dd")]
    [InlineData("MM/dd/yyyy")]
    public void FormatDate_WithVariousFormats_CallsServiceCorrectly(string format)
    {
        // Arrange
        var date = DateTime.Now;
        _mockLocalizationService.Setup(x => x.FormatDate(date, format))
            .Returns($"Formatted: {format}");

        var cut = Render<TestLocalizableComponent>();

        // Act
        var result = cut.Instance.TestFormatDate(date, format);

        // Assert
        result.Should().Contain("Formatted:");
        _mockLocalizationService.Verify(x => x.FormatDate(date, format), Times.Once);
    }

    [Fact]
    public void FormatDate_WithMinValue_HandlesGracefully()
    {
        // Arrange
        _mockLocalizationService.Setup(x => x.FormatDate(DateTime.MinValue, null))
            .Returns("01/01/0001");

        var cut = Render<TestLocalizableComponent>();

        // Act
        var result = cut.Instance.TestFormatDate(DateTime.MinValue);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region FormatNumber Tests

    [Fact]
    public void FormatNumber_WithValidNumber_ReturnsFormattedString()
    {
        // Arrange
        const decimal number = 1234.56m;
        const string expectedValue = "1,234.56";
        _mockLocalizationService.Setup(x => x.FormatNumber(number, null))
            .Returns(expectedValue);

        var cut = Render<TestLocalizableComponent>();

        // Act
        var result = cut.Instance.TestFormatNumber(number);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void FormatNumber_WithCustomFormat_ReturnsCustomFormattedString()
    {
        // Arrange
        const decimal number = 1234.5678m;
        const string format = "N4";
        const string expectedValue = "1,234.5678";
        _mockLocalizationService.Setup(x => x.FormatNumber(number, format))
            .Returns(expectedValue);

        var cut = Render<TestLocalizableComponent>();

        // Act
        var result = cut.Instance.TestFormatNumber(number, format);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(0, "0.00")]
    [InlineData(-1234.56, "-1,234.56")]
    [InlineData(999999.99, "999,999.99")]
    public void FormatNumber_WithVariousNumbers_FormatsCorrectly(decimal number, string expected)
    {
        // Arrange
        _mockLocalizationService.Setup(x => x.FormatNumber(number, null))
            .Returns(expected);

        var cut = Render<TestLocalizableComponent>();

        // Act
        var result = cut.Instance.TestFormatNumber(number);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Component_AfterMultipleCultureChanges_MaintainsState()
    {
        // Arrange
        var cultures = new[] { "en-US", "es-ES", "fr-FR", "ru-RU" };
        _mockLocalizationService.Setup(x => x.Localize("Navigation.Dashboard"))
            .Returns("Dashboard");

        var cut = Render<TestLocalizableComponent>();

        // Act - Change culture multiple times
        foreach (var cultureCode in cultures)
        {
            var culture = new CultureInfo(cultureCode);
            _mockLocalizationService.Setup(x => x.GetUserCultureAsync())
                .ReturnsAsync(culture);
            _mockLocalizationService.Setup(x => x.CurrentCulture)
                .Returns(culture);
            await cut.InvokeAsync(() => Task.CompletedTask);
        }

        // Assert - Component should still be functional (can call test methods)
        var result = cut.Instance.TestL("Navigation.Dashboard");
        result.Should().NotBeNull();
        result.Should().Be("Dashboard");
    }

    #endregion
}

/// <summary>
/// Test component that inherits from MelodeeComponentBase to test localization methods.
/// </summary>
public class TestLocalizableComponent : MelodeeComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        // Render minimal markup for testing
        builder.OpenElement(0, "div");
        builder.AddContent(1, "Test Component");
        builder.CloseElement();
    }

    public string TestL(string key) => L(key);
    public string TestLWithFallback(string key, string fallback) => L(key, fallback);
    public string TestLWithArgs(string key, params object[] args) => L(key, args);
    public string TestFormatDate(DateTime date, string? format = null) => FormatDate(date, format);
    public string TestFormatNumber(decimal number, string? format = null) => FormatNumber(number, format);
}

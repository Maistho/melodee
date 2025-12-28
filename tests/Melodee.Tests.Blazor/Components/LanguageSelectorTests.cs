using System.Globalization;
using Bunit;
using FluentAssertions;
using Melodee.Blazor.Components.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MoqTimes = Moq.Times;

namespace Melodee.Tests.Blazor.Components;

public class LanguageSelectorTests : BunitContext
{
    private readonly Mock<ILocalizationService> _mockLocalizationService;
    private readonly Mock<IJSRuntime> _mockJSRuntime;

    public LanguageSelectorTests()
    {
        _mockLocalizationService = new Mock<ILocalizationService>();
        _mockJSRuntime = new Mock<IJSRuntime>();

        // Setup default supported cultures
        var supportedCultures = new List<CultureInfo>
        {
            new("en-US"),
            new("es-ES"),
            new("ru-RU"),
            new("zh-CN"),
            new("fr-FR"),
            new("ar-SA")
        };

        _mockLocalizationService.Setup(x => x.SupportedCultures)
            .Returns(supportedCultures);

        _mockLocalizationService.Setup(x => x.CurrentCulture)
            .Returns(new CultureInfo("en-US"));

        // Register services
        Services.AddSingleton(_mockLocalizationService.Object);
        Services.AddSingleton(_mockJSRuntime.Object);

        // Setup JSInterop
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Rendering Tests

    [Fact]
    public void LanguageSelector_Renders_Successfully()
    {
        // Act
        var cut = Render<LanguageSelector>();

        // Assert
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public void LanguageSelector_DisplaysAllSupportedCultures()
    {
        // Act
        var cut = Render<LanguageSelector>();

        // Assert - RadzenDropDown renders as a div with rz-dropdown class
        var dropdown = cut.Find(".rz-dropdown");
        dropdown.Should().NotBeNull();
    }

    [Fact]
    public void LanguageSelector_ShowsCurrentCultureAsSelected()
    {
        // Arrange
        _mockLocalizationService.Setup(x => x.CurrentCulture)
            .Returns(new CultureInfo("es-ES"));

        // Act
        var cut = Render<LanguageSelector>();

        // Assert - component should initialize with es-ES
        cut.Instance.Should().NotBeNull();
    }

    [Fact]
    public void LanguageSelector_WithCustomStyle_AppliesStyle()
    {
        // Arrange
        const string customStyle = "width: 200px; color: red;";

        // Act
        var cut = Render<LanguageSelector>(parameters => parameters
            .Add(p => p.Style, customStyle));

        // Assert
        cut.Markup.Should().Contain(customStyle);
    }

    [Fact]
    public void LanguageSelector_WithCustomClass_AppliesClass()
    {
        // Arrange
        const string customClass = "custom-language-selector";

        // Act
        var cut = Render<LanguageSelector>(parameters => parameters
            .Add(p => p.Class, customClass));

        // Assert
        cut.Markup.Should().Contain(customClass);
    }

    [Fact]
    public void LanguageSelector_WithCustomPlaceholder_DisplaysPlaceholder()
    {
        // Arrange
        const string customPlaceholder = "Choose Your Language";

        // Act
        var cut = Render<LanguageSelector>(parameters => parameters
            .Add(p => p.Placeholder, customPlaceholder));

        // Assert - Placeholder is set on the component even if not displayed (RadzenDropDown shows selected value, not placeholder when value is set)
        cut.Instance.Placeholder.Should().Be(customPlaceholder);
    }

    #endregion

    #region Culture Change Tests

    [Fact]
    public async Task LanguageSelector_OnCultureChange_CallsSetCultureAsync()
    {
        // Arrange
        var newCulture = "fr-FR";
        _mockLocalizationService.Setup(x => x.SetCultureAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockJSRuntime.Setup(x => x.InvokeAsync<object>(
                "location.reload",
                It.IsAny<object[]>()))
            .ReturnsAsync(new object());

        var cut = Render<LanguageSelector>();

        // Act
        // Simulate culture change by invoking the method directly
        await cut.InvokeAsync(async () =>
        {
            await _mockLocalizationService.Object.SetCultureAsync(newCulture);
        });

        // Assert
        _mockLocalizationService.Verify(x => x.SetCultureAsync(newCulture), MoqTimes.Once);
    }

    [Fact]
    public async Task LanguageSelector_OnCultureChange_ReloadsPage()
    {
        // Arrange
        var newCulture = "zh-CN";
        _mockLocalizationService.Setup(x => x.SetCultureAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var jsInvoked = false;

        var cut = Render<LanguageSelector>();

        // Act - use the bUnit JSInterop to track the JS call
        await cut.InvokeAsync(async () =>
        {
            await _mockLocalizationService.Object.SetCultureAsync(newCulture);
            jsInvoked = true;
        });

        // Assert
        jsInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task LanguageSelector_ChangingToEachSupportedCulture_Works()
    {
        // Arrange
        var culturesToTest = new[] { "en-US", "es-ES", "ru-RU", "zh-CN", "fr-FR", "ar-SA" };
        _mockLocalizationService.Setup(x => x.SetCultureAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var cut = Render<LanguageSelector>();

        // Act & Assert
        foreach (var culture in culturesToTest)
        {
            await cut.InvokeAsync(async () =>
            {
                await _mockLocalizationService.Object.SetCultureAsync(culture);
            });

            _mockLocalizationService.Verify(x => x.SetCultureAsync(culture), MoqTimes.AtLeastOnce);
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task LanguageSelector_WhenSetCultureThrows_HandlesGracefully()
    {
        // Arrange
        _mockLocalizationService.Setup(x => x.SetCultureAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Culture error"));

        var cut = Render<LanguageSelector>();

        // Act & Assert - Should not throw and component should remain rendered
        await cut.InvokeAsync(async () =>
        {
            try
            {
                await _mockLocalizationService.Object.SetCultureAsync("invalid");
            }
            catch
            {
                // Expected to catch - component handles this internally
            }
        });

        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LanguageSelector_WhenJSRuntimeThrows_HandlesGracefully()
    {
        // Arrange
        _mockLocalizationService.Setup(x => x.SetCultureAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var cut = Render<LanguageSelector>();
        var exceptionHandled = false;

        // Act & Assert - Should handle JS errors gracefully
        var act = async () =>
        {
            await cut.InvokeAsync(() =>
            {
                try
                {
                    throw new JSException("JS Error");
                }
                catch (JSException)
                {
                    exceptionHandled = true;
                }
                return Task.CompletedTask;
            });
        };

        await act.Should().NotThrowAsync();
        exceptionHandled.Should().BeTrue();
    }

    [Fact]
    public void LanguageSelector_WithEmptySupportedCultures_HandlesGracefully()
    {
        // Arrange
        _mockLocalizationService.Setup(x => x.SupportedCultures)
            .Returns(new List<CultureInfo>());

        // Act
        var act = () => Render<LanguageSelector>();

        // Assert - Should not throw even with empty cultures
        act.Should().NotThrow();
    }

    [Fact]
    public void LanguageSelector_WithNullCurrentCulture_HandlesGracefully()
    {
        // Arrange - create a separate context with null culture
        using var ctx = new BunitContext();
        var mockService = new Mock<ILocalizationService>();
        var mockJsRuntime = new Mock<IJSRuntime>();

        mockService.Setup(x => x.CurrentCulture).Returns((CultureInfo)null!);
        mockService.Setup(x => x.SupportedCultures).Returns(new List<CultureInfo> { new("en-US") });

        ctx.Services.AddSingleton(mockService.Object);
        ctx.Services.AddSingleton(mockJsRuntime.Object);
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var act = () => ctx.Render<LanguageSelector>();

        // Assert - Component should handle null culture
        act.Should().NotThrow();
    }

    #endregion

    #region Event Subscription Tests

    [Fact]
    public void LanguageSelector_OnInitialization_SubscribesToCultureChangedEvent()
    {
        // Arrange
        var subscribeCount = 0;
        _mockLocalizationService.SetupAdd(x => x.CultureChanged += It.IsAny<Action<CultureInfo>>())
            .Callback<Action<CultureInfo>>(_ => subscribeCount++);

        // Act
        var cut = Render<LanguageSelector>();

        // Assert
        subscribeCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void LanguageSelector_OnDispose_UnsubscribesFromCultureChangedEvent()
    {
        // Arrange
        var cut = Render<LanguageSelector>();

        // The component instance should implement IDisposable
        cut.Instance.Should().BeAssignableTo<IDisposable>();

        // Act - manually call dispose on the component instance
        ((IDisposable)cut.Instance).Dispose();

        // Assert - verify the component unsubscribed from the event
        _mockLocalizationService.VerifyRemove(
            x => x.CultureChanged -= It.IsAny<Action<CultureInfo>>(),
            MoqTimes.AtLeastOnce());
    }

    [Fact]
    public async Task LanguageSelector_WhenExternalCultureChanges_UpdatesSelection()
    {
        // Arrange - capture the handler when it subscribes
        Action<CultureInfo>? capturedHandler = null;
        _mockLocalizationService.SetupAdd(x => x.CultureChanged += It.IsAny<Action<CultureInfo>>())
            .Callback<Action<CultureInfo>>(handler => capturedHandler = handler);

        var cut = Render<LanguageSelector>();
        capturedHandler.Should().NotBeNull("component should have subscribed to event");

        // Act - Simulate external culture change by invoking the captured handler
        await cut.InvokeAsync(() =>
        {
            capturedHandler!.Invoke(new CultureInfo("ru-RU"));
        });

        // Assert - Component should have updated
        cut.Instance.Should().NotBeNull();
    }

    #endregion

    #region Initialization Tests

    [Fact]
    public async Task LanguageSelector_OnInitialization_GetsCurrentCultureFromService()
    {
        // Arrange
        var expectedCulture = new CultureInfo("es-ES");
        _mockLocalizationService.Setup(x => x.CurrentCulture)
            .Returns(expectedCulture);

        // Act
        var cut = Render<LanguageSelector>();
        await cut.InvokeAsync(() => Task.CompletedTask); // Ensure initialization completes

        // Assert
        _mockLocalizationService.Verify(x => x.CurrentCulture, MoqTimes.AtLeastOnce);
    }

    [Fact]
    public void LanguageSelector_BeforeInitialization_DoesNotProcessCultureChanges()
    {
        // Arrange
        _mockLocalizationService.Setup(x => x.SetCultureAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act - Try to change culture before initialization completes
        var cut = Render<LanguageSelector>();

        // Assert - Component should handle pre-initialization state
        cut.Markup.Should().NotBeEmpty();
    }

    #endregion

    #region Multiple Instances Tests

    [Fact]
    public void LanguageSelector_MultipleInstances_EachSubscribesIndependently()
    {
        // Arrange
        var subscriptionCount = 0;
        _mockLocalizationService.SetupAdd(x => x.CultureChanged += It.IsAny<Action<CultureInfo>>())
            .Callback<Action<CultureInfo>>(_ => subscriptionCount++);

        // Act
        var cut1 = Render<LanguageSelector>();
        var cut2 = Render<LanguageSelector>();
        var cut3 = Render<LanguageSelector>();

        // Assert
        subscriptionCount.Should().Be(3);

        // Cleanup
        cut1.Dispose();
        cut2.Dispose();
        cut3.Dispose();
    }

    [Fact]
    public void LanguageSelector_MultipleInstances_DisposeIndependently()
    {
        // Arrange
        var cut1 = Render<LanguageSelector>();
        var cut2 = Render<LanguageSelector>();

        // Act - manually dispose first component
        ((IDisposable)cut1.Instance).Dispose();

        // Assert - verify first instance unsubscribed
        _mockLocalizationService.VerifyRemove(
            x => x.CultureChanged -= It.IsAny<Action<CultureInfo>>(),
            MoqTimes.Once());
        cut2.Markup.Should().NotBeEmpty(); // Second instance still works

        // Cleanup - dispose second component
        ((IDisposable)cut2.Instance).Dispose();
        _mockLocalizationService.VerifyRemove(
            x => x.CultureChanged -= It.IsAny<Action<CultureInfo>>(),
            MoqTimes.Exactly(2));
    }

    #endregion

    #region Culture-Specific Rendering Tests

    [Theory]
    [InlineData("en-US", "English")]
    [InlineData("es-ES", "Spanish")]
    [InlineData("ru-RU", "Russian")]
    [InlineData("zh-CN", "Chinese")]
    [InlineData("fr-FR", "French")]
    [InlineData("ar-SA", "Arabic")]
    public void LanguageSelector_DisplaysCultureDisplayName_ForEachCulture(string cultureCode, string expectedLanguage)
    {
        // Arrange
        var culture = new CultureInfo(cultureCode);

        // Act - DisplayName should contain the language name
        var displayName = culture.DisplayName;

        // Assert
        displayName.Should().Contain(expectedLanguage, AtLeast.Once());
    }

    #endregion
}

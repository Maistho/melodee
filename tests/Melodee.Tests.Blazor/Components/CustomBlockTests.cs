using Bunit;
using FluentAssertions;
using Melodee.Blazor.Components;
using Melodee.Blazor.Services.CustomBlocks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Melodee.Tests.Blazor.Components;

/// <summary>
/// Tests for CustomBlock component, focusing on verifying proper MarkupString rendering.
/// </summary>
public class CustomBlockTests : BunitContext
{
    private readonly Mock<ICustomBlockService> _mockCustomBlockService;
    private readonly Mock<ILogger<CustomBlock>> _mockLogger;

    public CustomBlockTests()
    {
        _mockCustomBlockService = new Mock<ICustomBlockService>();
        _mockLogger = new Mock<ILogger<CustomBlock>>();

        // Register services
        Services.AddSingleton(_mockCustomBlockService.Object);
        Services.AddSingleton(_mockLogger.Object);
    }

    #region MarkupString Rendering Tests (Bug Fix Verification)

    [Fact]
    public void CustomBlock_WithHtmlContent_RendersAsHtmlNotText()
    {
        // Arrange - simulate a custom block with HTML content
        const string key = "login.top";
        const string htmlContent = "<div class=\"demo-info\"><strong>Demo Server</strong></div>";
        var result = CustomBlockResult.Success(key, htmlContent);

        _mockCustomBlockService.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key));

        // Assert - HTML should be rendered, not escaped
        cut.Markup.Should().Contain("<div class=\"demo-info\">", "HTML should be rendered as actual HTML");
        cut.Markup.Should().Contain("<strong>Demo Server</strong>", "HTML tags should be preserved");
        cut.Markup.Should().NotContain("&lt;div", "HTML should not be escaped");
        cut.Markup.Should().NotContain("&lt;strong", "HTML tags should not be entity-encoded");
    }

    [Fact]
    public void CustomBlock_WithMarkdownRenderedToHtml_DisplaysCorrectly()
    {
        // Arrange - simulate markdown that was rendered to HTML
        const string key = "test.markdown";
        const string renderedHtml = "<h2>Demo Server</h2><p>Username: <code>demo</code></p><ul><li>Feature 1</li><li>Feature 2</li></ul>";
        var result = CustomBlockResult.Success(key, renderedHtml);

        _mockCustomBlockService.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key));

        // Assert - Markdown-rendered HTML should appear correctly
        cut.Markup.Should().Contain("<h2>Demo Server</h2>");
        cut.Markup.Should().Contain("<code>demo</code>");
        cut.Markup.Should().Contain("<ul><li>Feature 1</li><li>Feature 2</li></ul>");
    }

    [Fact]
    public void CustomBlock_WithComplexHtml_PreservesStructure()
    {
        // Arrange - complex nested HTML structure
        const string key = "complex.test";
        const string complexHtml = @"
<div class=""alert alert-info"">
    <div class=""alert-header"">
        <h3>Important Notice</h3>
    </div>
    <div class=""alert-body"">
        <p>This is a <em>test</em> with <strong>nested</strong> elements.</p>
        <ul>
            <li>Item <span class=""highlight"">one</span></li>
            <li>Item <span class=""highlight"">two</span></li>
        </ul>
    </div>
</div>";
        var result = CustomBlockResult.Success(key, complexHtml);

        _mockCustomBlockService.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key));

        // Assert - Complex structure should be preserved
        cut.Markup.Should().Contain("<div class=\"alert alert-info\">");
        cut.Markup.Should().Contain("<h3>Important Notice</h3>");
        cut.Markup.Should().Contain("<em>test</em>");
        cut.Markup.Should().Contain("<strong>nested</strong>");
        cut.Markup.Should().Contain("<span class=\"highlight\">one</span>");
    }

    [Fact]
    public void CustomBlock_WithSanitizedHtml_DoesNotExecuteScripts()
    {
        // Arrange - HTML with potentially dangerous content (should already be sanitized by service)
        const string key = "sanitized.test";
        const string sanitizedHtml = "<div>Safe content only - scripts removed</div>";
        var result = CustomBlockResult.Success(key, sanitizedHtml);

        _mockCustomBlockService.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key));

        // Assert - Should render the sanitized content safely
        cut.Markup.Should().Contain("<div>Safe content only - scripts removed</div>");
        cut.Markup.Should().NotContain("<script>", "scripts should be removed by sanitization");
        cut.Markup.Should().NotContain("javascript:", "inline JS should be removed");
    }

    #endregion

    #region Wrapper Class Generation Tests

    [Fact]
    public void CustomBlock_GeneratesCorrectWrapperClass()
    {
        // Arrange
        const string key = "login.top";
        var result = CustomBlockResult.Success(key, "<div>Test</div>");

        _mockCustomBlockService.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key));

        // Assert
        cut.Markup.Should().Contain("class=\"custom-block custom-block--login-top");
    }

    [Fact]
    public void CustomBlock_WithDotInKey_ConvertsDotToDash()
    {
        // Arrange
        const string key = "some.page.location";
        var result = CustomBlockResult.Success(key, "<div>Test</div>");

        _mockCustomBlockService.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key));

        // Assert
        cut.Markup.Should().Contain("custom-block--some-page-location");
    }

    [Fact]
    public void CustomBlock_WithAdditionalCssClass_IncludesBothClasses()
    {
        // Arrange
        const string key = "test.key";
        const string additionalClass = "my-custom-class";
        var result = CustomBlockResult.Success(key, "<div>Test</div>");

        _mockCustomBlockService.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key)
            .Add(p => p.CssClass, additionalClass));

        // Assert
        cut.Markup.Should().Contain("custom-block custom-block--test-key my-custom-class");
    }

    #endregion

    #region Conditional Rendering Tests

    [Fact]
    public void CustomBlock_WhenNotFound_RendersNothing()
    {
        // Arrange
        const string key = "missing.block";
        var result = CustomBlockResult.NotFound(key);

        _mockCustomBlockService.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key));

        // Assert
        cut.Markup.Should().BeEmpty("component should render nothing when block is not found");
    }

    [Fact]
    public void CustomBlock_WithEmptyKey_RendersNothing()
    {
        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, string.Empty));

        // Assert
        cut.Markup.Should().BeEmpty("component should render nothing with empty key");
        _mockCustomBlockService.Verify(
            x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "service should not be called with empty key");
    }

    [Fact]
    public void CustomBlock_WithWhitespaceKey_RendersNothing()
    {
        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, "   "));

        // Assert
        cut.Markup.Should().BeEmpty("component should render nothing with whitespace key");
        _mockCustomBlockService.Verify(
            x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "service should not be called with whitespace key");
    }

    [Fact]
    public void CustomBlock_WithEmptyContent_RendersWrapperOnly()
    {
        // Arrange
        const string key = "empty.block";
        var result = CustomBlockResult.Success(key, string.Empty);

        _mockCustomBlockService.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key));

        // Assert - Should still render wrapper div even if content is empty
        cut.Markup.Should().Contain("custom-block custom-block--empty-block");
    }

    #endregion

    #region Service Integration Tests

    [Fact]
    public async Task CustomBlock_CallsService_WithCorrectKey()
    {
        // Arrange
        const string key = "test.service";
        var result = CustomBlockResult.Success(key, "<div>Test</div>");

        _mockCustomBlockService.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key));

        // Wait for async initialization
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        _mockCustomBlockService.Verify(
            x => x.GetAsync(key, It.IsAny<CancellationToken>()),
            Times.Once,
            "service should be called exactly once with the correct key");
    }

    [Fact]
    public async Task CustomBlock_OnParametersChanged_CallsServiceAgain()
    {
        // Arrange
        const string key1 = "first.key";
        const string key2 = "second.key";
        var result1 = CustomBlockResult.Success(key1, "<div>First</div>");
        var result2 = CustomBlockResult.Success(key2, "<div>Second</div>");

        _mockCustomBlockService.Setup(x => x.GetAsync(key1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result1);
        _mockCustomBlockService.Setup(x => x.GetAsync(key2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result2);

        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key1));

        // Act - change the key parameter using SetParametersAndRender
        await cut.InvokeAsync(() => cut.Instance.SetParametersAsync(
            ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                { nameof(CustomBlock.Key), key2 }
            })));

        // Assert
        _mockCustomBlockService.Verify(x => x.GetAsync(key1, It.IsAny<CancellationToken>()), Times.Once);
        _mockCustomBlockService.Verify(x => x.GetAsync(key2, It.IsAny<CancellationToken>()), Times.Once);
        cut.Markup.Should().Contain("Second");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task CustomBlock_WhenServiceThrows_HandlesGracefully()
    {
        // Arrange
        const string key = "error.test";
        _mockCustomBlockService.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key));

        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert - component should handle error and render nothing
        cut.Markup.Should().BeEmpty("component should render nothing when service throws");

        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unexpected error loading custom block")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once,
            "error should be logged");
    }

    [Fact]
    public async Task CustomBlock_WhenCancellationRequested_CompletesGracefully()
    {
        // Arrange
        const string key = "cancel.test";
        _mockCustomBlockService.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());

        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key));

        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert - component should handle cancellation gracefully
        cut.Markup.Should().BeEmpty();
    }

    #endregion

    #region Real-World Demo Scenarios

    [Fact]
    public void CustomBlock_DemoLoginTopScenario_RendersCorrectly()
    {
        // Arrange - simulate the actual demo login page scenario
        const string key = "login.top";
        const string demoHtml = @"
<div class=""demo-server-info rz-background-color-info-lighter rz-border-radius-3 rz-p-4 rz-mb-4"">
    <div class=""rz-text-align-center"">
        <h2 class=""rz-mb-2"">🎵 Melodee Demo Server</h2>
        <p class=""rz-mb-3"">Try out Melodee with the demo account</p>
        <div class=""demo-credentials rz-p-3 rz-background-color-base-100 rz-border-radius-2"">
            <p class=""rz-mb-1""><strong>Username:</strong> <code>demo</code></p>
            <p class=""rz-mb-0""><strong>Password:</strong> <code>Mel0deeR0cks!</code></p>
        </div>
    </div>
</div>";
        var result = CustomBlockResult.Success(key, demoHtml);

        _mockCustomBlockService.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key));

        // Assert - all demo content should be rendered properly
        cut.Markup.Should().Contain("🎵 Melodee Demo Server");
        cut.Markup.Should().Contain("Try out Melodee with the demo account");
        cut.Markup.Should().Contain("<code>demo</code>");
        cut.Markup.Should().Contain("<code>Mel0deeR0cks!</code>");
        cut.Markup.Should().Contain("demo-server-info");
        cut.Markup.Should().Contain("custom-block--login-top");

        // Verify HTML is rendered, not escaped
        cut.Markup.Should().NotContain("&lt;h2&gt;");
        cut.Markup.Should().NotContain("&lt;code&gt;");
    }

    [Fact]
    public void CustomBlock_WithRadzenClasses_PreservesRadzenStyling()
    {
        // Arrange - test that Radzen CSS classes are preserved
        const string key = "styled.block";
        const string radzenHtml = @"
<div class=""rz-background-color-primary rz-p-4"">
    <h3 class=""rz-text-align-center rz-mb-3"">Styled Content</h3>
    <p class=""rz-color-secondary"">With Radzen classes</p>
</div>";
        var result = CustomBlockResult.Success(key, radzenHtml);

        _mockCustomBlockService.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key));

        // Assert - Radzen classes should be preserved
        cut.Markup.Should().Contain("rz-background-color-primary");
        cut.Markup.Should().Contain("rz-p-4");
        cut.Markup.Should().Contain("rz-text-align-center");
        cut.Markup.Should().Contain("rz-mb-3");
        cut.Markup.Should().Contain("rz-color-secondary");
    }

    #endregion

    #region Parameter Tests

    [Fact]
    public void CustomBlock_WrapInCardParameter_IsReservedForFuture()
    {
        // Arrange
        const string key = "test.card";
        var result = CustomBlockResult.Success(key, "<div>Test</div>");

        _mockCustomBlockService.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key)
            .Add(p => p.WrapInCard, true));

        // Assert - parameter exists but doesn't affect rendering yet
        cut.Instance.WrapInCard.Should().BeTrue();
        // Note: When WrapInCard is implemented, add tests for actual card wrapping
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("custom-class")]
    [InlineData("multiple custom classes")]
    public void CustomBlock_WithVariousCssClasses_HandlesCorrectly(string? cssClass)
    {
        // Arrange
        const string key = "test.css";
        var result = CustomBlockResult.Success(key, "<div>Test</div>");

        _mockCustomBlockService.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var cut = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, key)
            .Add(p => p.CssClass, cssClass));

        // Assert
        cut.Instance.CssClass.Should().Be(cssClass);
        if (!string.IsNullOrEmpty(cssClass))
        {
            cut.Markup.Should().Contain(cssClass);
        }
    }

    #endregion

    #region Multiple Instance Tests

    [Fact]
    public void CustomBlock_MultipleInstances_RenderIndependently()
    {
        // Arrange
        var result1 = CustomBlockResult.Success("block.one", "<div>First Block</div>");
        var result2 = CustomBlockResult.Success("block.two", "<div>Second Block</div>");

        _mockCustomBlockService.Setup(x => x.GetAsync("block.one", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result1);
        _mockCustomBlockService.Setup(x => x.GetAsync("block.two", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result2);

        // Act
        var cut1 = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, "block.one"));
        var cut2 = Render<CustomBlock>(parameters => parameters
            .Add(p => p.Key, "block.two"));

        // Assert
        cut1.Markup.Should().Contain("First Block");
        cut1.Markup.Should().NotContain("Second Block");

        cut2.Markup.Should().Contain("Second Block");
        cut2.Markup.Should().NotContain("First Block");

        cut1.Markup.Should().Contain("custom-block--block-one");
        cut2.Markup.Should().Contain("custom-block--block-two");
    }

    #endregion
}

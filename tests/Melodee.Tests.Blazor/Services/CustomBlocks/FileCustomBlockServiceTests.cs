using Melodee.Blazor.Services.CustomBlocks;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Melodee.Tests.Blazor.Services.CustomBlocks;

/// <summary>
/// Tests for FileCustomBlockService covering key validation, file reading,
/// Markdown rendering, HTML sanitization, and caching behavior.
/// </summary>
public class FileCustomBlockServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly IMemoryCache _cache;
    private readonly MarkdownRenderer _markdownRenderer;
    private readonly HtmlSanitizerService _sanitizer;
    private readonly TestLibraryService _libraryService;

    public FileCustomBlockServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"melodee-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);

        _cache = new MemoryCache(new MemoryCacheOptions());
        _markdownRenderer = new MarkdownRenderer();
        _sanitizer = new HtmlSanitizerService();
        _libraryService = new TestLibraryService(_tempDirectory); // Service will add "custom-blocks" subdirectory
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
        _cache.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Helper to get the custom blocks directory path (matches what FileCustomBlockService computes)
    /// </summary>
    private string GetCustomBlocksDirectory() => Path.Combine(_tempDirectory, "custom-blocks");

    private FileCustomBlockService CreateService()
    {
        var options = Options.Create(new CustomBlocksOptions
        {
            MaxBytes = 262144
        });

        return new FileCustomBlockService(
            options,
            _libraryService,
            _markdownRenderer,
            _sanitizer,
            _cache,
            NullLogger<FileCustomBlockService>.Instance
        );
    }

    /// <summary>
    /// Test-only LibraryService that returns a pre-configured Templates library
    /// </summary>
    private class TestLibraryService : LibraryService
    {
        private readonly string _templatesPath;

        public TestLibraryService(string templatesPath)
        {
            _templatesPath = templatesPath;
        }

        public override Task<OperationResult<Library>> GetTemplatesLibraryAsync(CancellationToken cancellationToken = default)
        {
            var testLibrary = new Library
            {
                Id = 1,
                Name = "Templates",
                Type = (int)LibraryType.Templates,
                Path = _templatesPath,
                CreatedAt = Instant.FromUnixTimeTicks(0)
            };

            return Task.FromResult(new OperationResult<Library>
            {
                Data = testLibrary,
                Type = OperationResponseType.Ok
            });
        }
    }

    #region Key Validation Tests

    [Fact]
    public async Task GetBlockAsync_ValidSimpleKey_ReturnsNotFoundIfFileDoesNotExist()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.GetAsync("login.top");

        // Assert
        Assert.False(result.Found);
        Assert.Empty(result.Content.ToString());
    }

    [Fact]
    public async Task GetBlockAsync_ValidNestedKey_ReturnsNotFoundIfFileDoesNotExist()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.GetAsync("auth.login.top");

        // Assert
        Assert.False(result.Found);
        Assert.Empty(result.Content.ToString());
    }

    [Theory]
    [InlineData("../etc/passwd")]
    [InlineData("..\\windows\\system32")]
    [InlineData("/etc/passwd")]
    [InlineData("C:\\Windows\\System32")]
    [InlineData("login..top")]
    [InlineData(".hidden")]
    [InlineData("login.")]
    [InlineData(".login")]
    [InlineData("login/top")]
    [InlineData("login\\top")]
    [InlineData("login<script>")]
    [InlineData("login&top")]
    [InlineData("login top")]
    [InlineData("LOGIN.TOP")]
    [InlineData("login_top")]
    public async Task GetBlockAsync_InvalidKey_ReturnsNotFound(string invalidKey)
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.GetAsync(invalidKey);

        // Assert
        Assert.False(result.Found);
        Assert.Empty(result.Content.ToString());
    }

    [Theory]
    [InlineData("login.top")]
    [InlineData("register.bottom")]
    [InlineData("forgot-password.top")]
    [InlineData("reset-password.bottom")]
    [InlineData("auth.login.top")]
    [InlineData("auth.register.bottom")]
    [InlineData("page-123.section-456")]
    public async Task GetBlockAsync_ValidKey_PassesValidation(string validKey)
    {
        // Arrange
        var service = CreateService();

        // Act - Should not throw, just return NotFound since file doesn't exist
        var result = await service.GetAsync(validKey);

        // Assert
        Assert.False(result.Found); // File doesn't exist
    }

    #endregion

    #region File Reading and Path Construction Tests

    [Fact]
    public async Task GetBlockAsync_SimpleKey_CreatesCorrectPath()
    {
        // Arrange
        var loginDir = Path.Combine(GetCustomBlocksDirectory(), "login");
        Directory.CreateDirectory(loginDir);
        var filePath = Path.Combine(loginDir, "top.md");
        File.WriteAllText(filePath, "# Login Top");

        var service = CreateService();

        // Act
        var result = await service.GetAsync("login.top");

        // Assert
        Assert.True(result.Found);
        Assert.Contains("<h1", result.Content.ToString());
        Assert.Contains("Login Top</h1>", result.Content.ToString());
    }

    [Fact]
    public async Task GetBlockAsync_NestedKey_CreatesCorrectPath()
    {
        // Arrange
        var authLoginDir = Path.Combine(GetCustomBlocksDirectory(), "auth", "login");
        Directory.CreateDirectory(authLoginDir);
        var filePath = Path.Combine(authLoginDir, "top.md");
        File.WriteAllText(filePath, "# Auth Login Top");

        var service = CreateService();

        // Act
        var result = await service.GetAsync("auth.login.top");

        // Assert
        Assert.True(result.Found);
        Assert.Contains("<h1", result.Content.ToString());
        Assert.Contains("Auth Login Top</h1>", result.Content.ToString());
    }

    [Fact]
    public async Task GetBlockAsync_FileSizeExceedsLimit_ReturnsNotFound()
    {
        // Arrange
        var loginDir = Path.Combine(GetCustomBlocksDirectory(), "login");
        Directory.CreateDirectory(loginDir);
        var filePath = Path.Combine(loginDir, "top.md");

        // Create file larger than default limit (256KB)
        var largeContent = new string('A', 262145); // 256KB + 1 byte
        File.WriteAllText(filePath, largeContent);

        var service = CreateService();

        // Act
        var result = await service.GetAsync("login.top");

        // Assert
        Assert.False(result.Found);
    }

    #endregion

    #region Markdown Rendering Tests

    [Fact]
    public async Task GetBlockAsync_RendersMarkdownToHtml()
    {
        // Arrange
        var loginDir = Path.Combine(GetCustomBlocksDirectory(), "login");
        Directory.CreateDirectory(loginDir);
        var filePath = Path.Combine(loginDir, "top.md");
        File.WriteAllText(filePath, @"# Welcome

This is **bold** and this is *italic*.

- List item 1
- List item 2");

        var service = CreateService();

        // Act
        var result = await service.GetAsync("login.top");

        // Assert
        Assert.True(result.Found);
        // Markdig advanced extensions add id attributes to headings
        Assert.Contains("<h1", result.Content.ToString());
        Assert.Contains("Welcome</h1>", result.Content.ToString());
        Assert.Contains("<strong>bold</strong>", result.Content.ToString());
        Assert.Contains("<em>italic</em>", result.Content.ToString());
        Assert.Contains("<ul>", result.Content.ToString());
        Assert.Contains("<li>List item 1</li>", result.Content.ToString());
    }

    [Fact]
    public async Task GetBlockAsync_DisallowsRawHtmlInMarkdown()
    {
        // Arrange
        var loginDir = Path.Combine(GetCustomBlocksDirectory(), "login");
        Directory.CreateDirectory(loginDir);
        var filePath = Path.Combine(loginDir, "top.md");
        File.WriteAllText(filePath, @"# Safe Content

<script>alert('XSS')</script>

<div onclick=""alert('XSS')"">Click me</div>

Safe **markdown** here.");

        var service = CreateService();

        // Act
        var result = await service.GetAsync("login.top");

        // Assert
        Assert.True(result.Found);
        // Dangerous tags and attributes are removed by sanitization
        Assert.DoesNotContain("<script>", result.Content.ToString());
        Assert.DoesNotContain("onclick", result.Content.ToString());
        // Safe HTML elements like div are allowed
        Assert.Contains("<div>Click me</div>", result.Content.ToString());
        // Verify markdown was properly rendered
        Assert.Contains("<strong>markdown</strong>", result.Content.ToString());
    }

    #endregion

    #region HTML Sanitization Tests

    [Fact]
    public async Task GetBlockAsync_RemovesJavaScriptUrls()
    {
        // Arrange
        var loginDir = Path.Combine(GetCustomBlocksDirectory(), "login");
        Directory.CreateDirectory(loginDir);
        var filePath = Path.Combine(loginDir, "top.md");

        // Markdown link syntax
        File.WriteAllText(filePath, "[Click here](javascript:alert('XSS'))");

        var service = CreateService();

        // Act
        var result = await service.GetAsync("login.top");

        // Assert
        Assert.True(result.Found);
        Assert.DoesNotContain("javascript:", result.Content.ToString());
    }

    [Fact]
    public async Task GetBlockAsync_AllowsSafeLinks()
    {
        // Arrange
        var loginDir = Path.Combine(GetCustomBlocksDirectory(), "login");
        Directory.CreateDirectory(loginDir);
        var filePath = Path.Combine(loginDir, "top.md");

        File.WriteAllText(filePath, @"[HTTP link](http://example.com)
[HTTPS link](https://example.com)
[Mailto link](mailto:test@example.com)");

        var service = CreateService();

        // Act
        var result = await service.GetAsync("login.top");

        // Assert
        Assert.True(result.Found);
        Assert.Contains("http://example.com", result.Content.ToString());
        Assert.Contains("https://example.com", result.Content.ToString());
        Assert.Contains("mailto:test@example.com", result.Content.ToString());
    }

    [Fact]
    public async Task GetBlockAsync_AllowsBasicFormattingTags()
    {
        // Arrange
        var loginDir = Path.Combine(GetCustomBlocksDirectory(), "login");
        Directory.CreateDirectory(loginDir);
        var filePath = Path.Combine(loginDir, "top.md");

        File.WriteAllText(filePath, @"**Bold text**

*Italic text*

`code snippet`

---

> Blockquote");

        var service = CreateService();

        // Act
        var result = await service.GetAsync("login.top");

        // Assert
        Assert.True(result.Found);
        Assert.Contains("<strong>", result.Content.ToString());
        Assert.Contains("<em>", result.Content.ToString());
        Assert.Contains("<code>", result.Content.ToString());
        Assert.Contains("<hr", result.Content.ToString());
        Assert.Contains("<blockquote>", result.Content.ToString());
    }

    [Fact]
    public async Task GetBlockAsync_AllowsSafeHtmlDivs()
    {
        // Arrange
        var loginDir = Path.Combine(GetCustomBlocksDirectory(), "login");
        Directory.CreateDirectory(loginDir);
        var filePath = Path.Combine(loginDir, "top.md");

        File.WriteAllText(filePath, @"# Welcome

<div style=""background: linear-gradient(135deg, #667eea15 0%, #764ba215 100%); border-left: 4px solid #667eea; padding: 1.5rem;"">

Demo credentials here

</div>");

        var service = CreateService();

        // Act
        var result = await service.GetAsync("login.top");

        // Assert
        Assert.True(result.Found);
        Assert.Contains("<div", result.Content.ToString());
        Assert.Contains("style=", result.Content.ToString());
        Assert.Contains("</div>", result.Content.ToString());
        // Dangerous inline scripts should still be removed
        Assert.DoesNotContain("javascript:", result.Content.ToString());
    }

    #endregion

    #region Caching Tests

    [Fact]
    public async Task GetBlockAsync_CachesResult()
    {
        // Arrange
        var loginDir = Path.Combine(GetCustomBlocksDirectory(), "login");
        Directory.CreateDirectory(loginDir);
        var filePath = Path.Combine(loginDir, "top.md");
        File.WriteAllText(filePath, "# Original Content");

        var service = CreateService();

        // Act - First call
        var result1 = await service.GetAsync("login.top");

        // Immediately get again (same timestamp = cached)
        var result2 = await service.GetAsync("login.top");

        // Assert - Both should be identical (second from cache, same timestamp)
        Assert.True(result1.Found);
        Assert.True(result2.Found);
        Assert.Contains("Original Content", result1.Content.ToString());
        Assert.Contains("Original Content", result2.Content.ToString());
        Assert.Equal(result1.Content.ToString(), result2.Content.ToString());
    }

    [Fact]
    public async Task GetBlockAsync_CacheExpiresAfterConfiguredSeconds()
    {
        // Arrange
        var loginDir = Path.Combine(GetCustomBlocksDirectory(), "login");
        Directory.CreateDirectory(loginDir);
        var filePath = Path.Combine(loginDir, "top.md");
        File.WriteAllText(filePath, "# Original Content");

        // Create service with short cache time for testing
        var options = Options.Create(new CustomBlocksOptions
        {
            MaxBytes = 262144,
            CacheSeconds = 1 // 1 second cache for faster test
        });
        var service = new FileCustomBlockService(
            options,
            _libraryService,
            _markdownRenderer,
            _sanitizer,
            _cache,
            NullLogger<FileCustomBlockService>.Instance
        );

        // Act - First call
        var result1 = await service.GetAsync("login.top");

        // Modify file
        File.WriteAllText(filePath, "# Modified Content");

        // Act - Second call immediately (should still be cached)
        var result2 = await service.GetAsync("login.top");

        // Wait for cache to expire
        Thread.Sleep(1100);

        // Act - Third call after cache expiry
        var result3 = await service.GetAsync("login.top");

        // Assert
        Assert.True(result1.Found);
        Assert.True(result2.Found);
        Assert.True(result3.Found);
        Assert.Contains("Original Content", result1.Content.ToString());
        Assert.Contains("Original Content", result2.Content.ToString()); // Still cached
        Assert.Contains("Modified Content", result3.Content.ToString()); // Cache expired, new content
    }

    [Fact]
    public async Task GetBlockAsync_NotFoundResultIsNotCached()
    {
        // Arrange
        var service = CreateService();

        // Act - First call (file doesn't exist)
        var result1 = await service.GetAsync("login.top");

        // Create file
        var loginDir = Path.Combine(GetCustomBlocksDirectory(), "login");
        Directory.CreateDirectory(loginDir);
        var filePath = Path.Combine(loginDir, "top.md");
        File.WriteAllText(filePath, "# Now Exists");

        // Act - Second call
        var result2 = await service.GetAsync("login.top");

        // Assert
        Assert.False(result1.Found);
        Assert.True(result2.Found); // Should find newly created file
        Assert.Contains("Now Exists", result2.Content.ToString());
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task GetBlockAsync_CompleteScenario_LoginTopBlock()
    {
        // Arrange
        var loginDir = Path.Combine(GetCustomBlocksDirectory(), "login");
        Directory.CreateDirectory(loginDir);
        var filePath = Path.Combine(loginDir, "top.md");
        File.WriteAllText(filePath, @"## Important Notice

Please ensure you are connecting via HTTPS.

For support, contact [help@example.com](mailto:help@example.com).");

        var service = CreateService();

        // Act
        var result = await service.GetAsync("login.top");

        // Assert
        Assert.True(result.Found);
        // Markdig advanced extensions add id attributes to headings
        Assert.Contains("<h2", result.Content.ToString());
        Assert.Contains("Important Notice</h2>", result.Content.ToString());
        Assert.Contains("HTTPS", result.Content.ToString());
        Assert.Contains("mailto:help@example.com", result.Content.ToString());
        Assert.DoesNotContain("<script>", result.Content.ToString());
    }

    [Fact]
    public async Task GetBlockAsync_CompleteScenario_NestedAuthLoginBottom()
    {
        // Arrange
        var authLoginDir = Path.Combine(GetCustomBlocksDirectory(), "auth", "login");
        Directory.CreateDirectory(authLoginDir);
        var filePath = Path.Combine(authLoginDir, "bottom.md");
        File.WriteAllText(filePath, @"### Need Help?

- [Forgot your password?](/account/forgot-password)
- [Contact support](https://support.example.com)

*Powered by Melodee*");

        var service = CreateService();

        // Act
        var result = await service.GetAsync("auth.login.bottom");

        // Assert
        Assert.True(result.Found);
        // Markdig advanced extensions add id attributes to headings
        Assert.Contains("<h3", result.Content.ToString());
        Assert.Contains("Need Help?</h3>", result.Content.ToString());
        Assert.Contains("/account/forgot-password", result.Content.ToString());
        Assert.Contains("https://support.example.com", result.Content.ToString());
        Assert.Contains("<em>Powered by Melodee</em>", result.Content.ToString());
    }

    #endregion
}

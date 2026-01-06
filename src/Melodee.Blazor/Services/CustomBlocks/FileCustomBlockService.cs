using System.Text.RegularExpressions;
using Melodee.Common.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Melodee.Blazor.Services.CustomBlocks;

/// <summary>
/// File-based implementation of custom block service.
/// Loads Markdown files from Templates library under custom-blocks directory.
/// </summary>
public sealed partial class FileCustomBlockService : ICustomBlockService
{
    private readonly CustomBlocksOptions _options;
    private readonly LibraryService _libraryService;
    private readonly IMarkdownRenderer _markdownRenderer;
    private readonly IHtmlSanitizerService _htmlSanitizer;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FileCustomBlockService> _logger;

    // Regex for validating block keys - only lowercase alphanumeric and hyphens in each segment
    [GeneratedRegex(@"^[a-z0-9-]+(\.[a-z0-9-]+)*$", RegexOptions.Compiled)]
    private static partial Regex KeyValidationRegex();

    public FileCustomBlockService(
        IOptions<CustomBlocksOptions> options,
        LibraryService libraryService,
        IMarkdownRenderer markdownRenderer,
        IHtmlSanitizerService htmlSanitizer,
        IMemoryCache cache,
        ILogger<FileCustomBlockService> logger)
    {
        _options = options.Value;
        _libraryService = libraryService;
        _markdownRenderer = markdownRenderer;
        _htmlSanitizer = htmlSanitizer;
        _cache = cache;
        _logger = logger;
    }

    public async Task<CustomBlockResult> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        // Feature disabled check
        if (!_options.Enabled)
        {
            return CustomBlockResult.NotFound(key);
        }

        // Validate key format
        if (!IsValidKey(key))
        {
            _logger.LogWarning("Invalid custom block key rejected: {Key}", key);
            return CustomBlockResult.NotFound(key);
        }

        // Check cache first
        var cacheKey = GetCacheKey(key);
        if (_cache.TryGetValue<CustomBlockResult>(cacheKey, out var cachedResult) && cachedResult != null)
        {
            return cachedResult;
        }

        // Load and process the block
        var result = await LoadAndProcessBlockAsync(key, cancellationToken);

        // Only cache successful results to allow newly created files to be found immediately
        // Not-found results are cheap to recompute (just a file existence check)
        if (result.Found)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.CacheSeconds)
            };
            _cache.Set(cacheKey, result, cacheOptions);
        }

        return result;
    }

    private async Task<CustomBlockResult> LoadAndProcessBlockAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            // Convert key to file path (e.g., "login.top" -> "custom-blocks/login/top.md")
            var filePath = await GetFilePathAsync(key, cancellationToken);
            if (filePath == null || !File.Exists(filePath))
            {
                return CustomBlockResult.NotFound(key);
            }

            // Check file size before reading
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > _options.MaxBytes)
            {
                _logger.LogWarning("Custom block file too large: {Key} ({Size} bytes, max {MaxSize})",
                    key, fileInfo.Length, _options.MaxBytes);
                return CustomBlockResult.NotFound(key);
            }

            // Read and process the file
            var markdown = await File.ReadAllTextAsync(filePath, cancellationToken);
            if (string.IsNullOrWhiteSpace(markdown))
            {
                return CustomBlockResult.NotFound(key);
            }

            // Render Markdown to HTML (with HTML disabled for security)
            var html = _markdownRenderer.RenderToHtml(markdown);

            // Sanitize HTML to prevent injection attacks
            var sanitizedHtml = _htmlSanitizer.Sanitize(html);

            return CustomBlockResult.Success(key, sanitizedHtml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading custom block: {Key}", key);
            return CustomBlockResult.NotFound(key);
        }
    }

    private static bool IsValidKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        // Reject dangerous patterns
        if (key.Contains("..", StringComparison.Ordinal) ||
            key.Contains('/', StringComparison.Ordinal) ||
            key.Contains('\\', StringComparison.Ordinal) ||
            key.Contains('~', StringComparison.Ordinal))
        {
            return false;
        }

        // Validate format: only lowercase alphanumeric, hyphens, and dots as separators
        return KeyValidationRegex().IsMatch(key);
    }

    private async Task<string?> GetFilePathAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            // Get Templates library
            var libraryResult = await _libraryService.GetTemplatesLibraryAsync(cancellationToken);
            if (!libraryResult.IsSuccess || libraryResult.Data == null)
            {
                _logger.LogWarning("Templates library not found, custom blocks unavailable");
                return null;
            }

            // Convert "login.top" to "custom-blocks/login/top.md" under Templates library
            var segments = key.Split('.');
            var fileName = segments[^1] + ".md";
            var directorySegments = segments[..^1];

            var path = Path.Combine(libraryResult.Data.Path, "custom-blocks");
            foreach (var segment in directorySegments)
            {
                path = Path.Combine(path, segment);
            }

            return Path.Combine(path, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving custom block file path for key: {Key}", key);
            return null;
        }
    }

    private string GetCacheKey(string key)
    {
        // Include file timestamp in cache key to auto-invalidate when file changes
        // Note: GetFilePathAsync is async, so we'll use a simpler cache key
        // The cache expiry handles staleness
        return $"customblock:{key}";
    }
}

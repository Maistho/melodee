using System.Drawing;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Models;
using Melodee.Common.Utility;
using Microsoft.Extensions.Logging;

namespace Melodee.Common.Services;

/// <summary>
/// Service for managing theme packs (discovery, validation, import/export)
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Discover and load all theme packs from the theme library
    /// </summary>
    Task<IEnumerable<ThemePack>> DiscoverThemePacksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific theme pack by ID
    /// </summary>
    Task<ThemePack?> GetThemePackAsync(string themeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate a theme pack (structure, required files, contrast ratios)
    /// </summary>
    Task<(bool IsValid, List<string> Warnings)> ValidateThemePackAsync(string themePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import a theme pack from a zip file
    /// </summary>
    Task<(bool Success, string? ThemeId, string? Error)> ImportThemePackAsync(Stream zipStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export a theme pack as a zip file
    /// </summary>
    Task<Stream?> ExportThemePackAsync(string themeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a theme pack (non-built-in only)
    /// </summary>
    Task<(bool Success, string? Error)> DeleteThemePackAsync(string themeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extract CSS variable tokens from a theme.css file
    /// </summary>
    Dictionary<string, string> ExtractCssTokens(string cssContent);

    /// <summary>
    /// Calculate WCAG contrast ratio between two colors
    /// </summary>
    double CalculateContrastRatio(string foregroundColor, string backgroundColor);
}

public sealed class ThemeService(
    ILogger<ThemeService> logger,
    IMelodeeConfigurationFactory configurationFactory,
    LibraryService libraryService) : IThemeService
{
    private const string ThemeJsonFileName = "theme.json";
    private const string ThemeCssFileName = "theme.css";
    private const int MaxThemeUploadSizeMb = 50;

    private static readonly HashSet<string> BuiltInThemeIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "light",
        "dark"
    };

    public async Task<IEnumerable<ThemePack>> DiscoverThemePacksAsync(CancellationToken cancellationToken = default)
    {
        var themePacks = new List<ThemePack>();

        // Add built-in themes
        themePacks.AddRange(GetBuiltInThemes());

        // Discover custom themes from library path
        var themeLibraryPath = await GetThemeLibraryPathAsync(cancellationToken);

        if (string.IsNullOrEmpty(themeLibraryPath) || !Directory.Exists(themeLibraryPath))
        {
            // Theme library is optional - just return built-in themes
            logger.LogDebug("Theme library not configured, using built-in themes only");
            return themePacks;
        }

        try
        {
            var themeDirs = Directory.GetDirectories(themeLibraryPath);
            foreach (var themeDir in themeDirs)
            {
                try
                {
                    var themePack = await LoadThemePackFromDirectoryAsync(themeDir, cancellationToken);
                    if (themePack != null)
                    {
                        themePacks.Add(themePack);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to load theme pack from {Directory}", themeDir);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to discover theme packs from {Path}", themeLibraryPath);
        }

        return themePacks;
    }

    public async Task<ThemePack?> GetThemePackAsync(string themeId, CancellationToken cancellationToken = default)
    {
        var themes = await DiscoverThemePacksAsync(cancellationToken);
        return themes.FirstOrDefault(t => t.Id.Equals(themeId, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<(bool IsValid, List<string> Warnings)> ValidateThemePackAsync(string themePath, CancellationToken cancellationToken = default)
    {
        var warnings = new List<string>();

        // Check required files exist
        var themeJsonPath = Path.Combine(themePath, ThemeJsonFileName);
        var themeCssPath = Path.Combine(themePath, ThemeCssFileName);

        if (!File.Exists(themeJsonPath))
        {
            warnings.Add($"Required file '{ThemeJsonFileName}' not found");
            return (false, warnings);
        }

        if (!File.Exists(themeCssPath))
        {
            warnings.Add($"Required file '{ThemeCssFileName}' not found");
            return (false, warnings);
        }

        // Validate theme.json structure
        try
        {
            var jsonContent = await File.ReadAllTextAsync(themeJsonPath, cancellationToken);
            var metadata = JsonSerializer.Deserialize<ThemeMetadata>(jsonContent);

            if (metadata == null || string.IsNullOrWhiteSpace(metadata.Id) || string.IsNullOrWhiteSpace(metadata.Name))
            {
                warnings.Add("Invalid theme.json: missing 'id' or 'name'");
                return (false, warnings);
            }
        }
        catch (JsonException ex)
        {
            warnings.Add($"Invalid theme.json format: {ex.Message}");
            return (false, warnings);
        }

        // Validate theme.css tokens and contrast
        try
        {
            var cssContent = await File.ReadAllTextAsync(themeCssPath, cancellationToken);
            var tokens = ExtractCssTokens(cssContent);

            // Check required tokens
            var missingTokens = ThemeTokenRegistry.RequiredTokens
                .Where(token => !tokens.ContainsKey(token))
                .ToList();

            if (missingTokens.Any())
            {
                warnings.Add($"Missing required tokens: {string.Join(", ", missingTokens)}");
            }

            // Validate contrast ratios
            var config = await configurationFactory.GetConfigurationAsync();
            var enforceContrast = config.GetValue<bool>(SettingRegistry.ThemeEnforceContrastValidation);

            foreach (var (foreground, background) in ThemeTokenRegistry.RequiredContrastPairs)
            {
                if (tokens.TryGetValue(foreground, out var fgColor) && tokens.TryGetValue(background, out var bgColor))
                {
                    try
                    {
                        var ratio = CalculateContrastRatio(fgColor, bgColor);
                        if (ratio < 4.5) // WCAG AA minimum for normal text
                        {
                            var warning = $"Insufficient contrast: {foreground} on {background} ({ratio:F2}:1, minimum 4.5:1)";
                            warnings.Add(warning);

                            if (enforceContrast)
                            {
                                return (false, warnings);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        warnings.Add($"Could not validate contrast for {foreground}/{background}: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            warnings.Add($"Error validating theme.css: {ex.Message}");
        }

        return (warnings.Count == 0 || warnings.All(w => !w.Contains("Missing required") && !w.Contains("Invalid")), warnings);
    }

    public async Task<(bool Success, string? ThemeId, string? Error)> ImportThemePackAsync(Stream zipStream, CancellationToken cancellationToken = default)
    {
        var themeLibraryPath = await GetThemeLibraryPathAsync(cancellationToken);
        var config = await configurationFactory.GetConfigurationAsync();
        var maxUploadSizeMb = config.GetValue<int>(SettingRegistry.ThemeMaxUploadSizeMb, value => value <= 0 ? MaxThemeUploadSizeMb : value);

        if (string.IsNullOrEmpty(themeLibraryPath))
        {
            return (false, null, "Theme library not configured");
        }

        Directory.CreateDirectory(themeLibraryPath);

        // Check size limit
        if (zipStream.Length > maxUploadSizeMb * 1024 * 1024)
        {
            return (false, null, $"Theme pack exceeds maximum size of {maxUploadSizeMb}MB");
        }

        string? tempDir = null;
        try
        {
            // Extract to temporary directory first
            tempDir = Path.Combine(Path.GetTempPath(), $"melodee-theme-{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                // Validate against zip-slip attacks
                foreach (var entry in archive.Entries)
                {
                    var destPath = Path.GetFullPath(Path.Combine(tempDir, entry.FullName));
                    if (!destPath.StartsWith(tempDir, StringComparison.OrdinalIgnoreCase))
                    {
                        return (false, null, "Invalid zip file: path traversal detected");
                    }
                }

                archive.ExtractToDirectory(tempDir);
            }

            // Validate the theme pack
            var (isValid, warnings) = await ValidateThemePackAsync(tempDir, cancellationToken);
            if (!isValid)
            {
                return (false, null, $"Invalid theme pack: {string.Join(", ", warnings)}");
            }

            // Read theme ID
            var themeJsonPath = Path.Combine(tempDir, ThemeJsonFileName);
            var jsonContent = await File.ReadAllTextAsync(themeJsonPath, cancellationToken);
            var metadata = JsonSerializer.Deserialize<ThemeMetadata>(jsonContent);

            if (metadata == null || string.IsNullOrWhiteSpace(metadata.Id))
            {
                return (false, null, "Theme metadata missing or invalid");
            }

            // Check if theme already exists
            var targetDir = Path.Combine(themeLibraryPath, metadata.Id);
            if (Directory.Exists(targetDir))
            {
                return (false, null, $"Theme '{metadata.Id}' already exists");
            }

            // Move to final location
            Directory.Move(tempDir, targetDir);
            tempDir = null; // Prevent cleanup

            logger.LogInformation("Imported theme pack: {ThemeId}", metadata.Id);
            return (true, metadata.Id, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to import theme pack");
            return (false, null, ex.Message);
        }
        finally
        {
            if (tempDir != null && Directory.Exists(tempDir))
            {
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }

    public async Task<Stream?> ExportThemePackAsync(string themeId, CancellationToken cancellationToken = default)
    {
        var themePack = await GetThemePackAsync(themeId, cancellationToken);
        if (themePack == null || !Directory.Exists(themePack.BaseDirectory))
        {
            return null;
        }

        var memoryStream = new MemoryStream();
        try
        {
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var files = Directory.GetFiles(themePack.BaseDirectory, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var relativePath = Path.GetRelativePath(themePack.BaseDirectory, file);
                    archive.CreateEntryFromFile(file, relativePath);
                }
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to export theme pack: {ThemeId}", LogSanitizer.Sanitize(themeId));
            memoryStream.Dispose();
            return null;
        }
    }

    public async Task<(bool Success, string? Error)> DeleteThemePackAsync(string themeId, CancellationToken cancellationToken = default)
    {
        if (BuiltInThemeIds.Contains(themeId))
        {
            return (false, "Cannot delete built-in theme");
        }

        var themePack = await GetThemePackAsync(themeId, cancellationToken);
        if (themePack == null)
        {
            return (false, "Theme not found");
        }

        if (themePack.IsBuiltIn)
        {
            return (false, "Cannot delete built-in theme");
        }

        try
        {
            if (Directory.Exists(themePack.BaseDirectory))
            {
                Directory.Delete(themePack.BaseDirectory, true);
            }

            logger.LogInformation("Deleted theme pack: {ThemeId}", LogSanitizer.Sanitize(themeId));
            return (true, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete theme pack: {ThemeId}", LogSanitizer.Sanitize(themeId));
            return (false, ex.Message);
        }
    }

    public Dictionary<string, string> ExtractCssTokens(string cssContent)
    {
        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Match CSS variable declarations in :root or * selector
        // Pattern: --variable-name: value;
        var pattern = @"--([a-z0-9-]+)\s*:\s*([^;]+);";
        var matches = Regex.Matches(cssContent, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

        foreach (Match match in matches)
        {
            var varName = $"--{match.Groups[1].Value}";
            var varValue = match.Groups[2].Value.Trim();
            tokens[varName] = varValue;
        }

        return tokens;
    }

    public double CalculateContrastRatio(string foregroundColor, string backgroundColor)
    {
        var fgLuminance = GetRelativeLuminance(ParseColor(foregroundColor));
        var bgLuminance = GetRelativeLuminance(ParseColor(backgroundColor));

        var lighter = Math.Max(fgLuminance, bgLuminance);
        var darker = Math.Min(fgLuminance, bgLuminance);

        return (lighter + 0.05) / (darker + 0.05);
    }

    private static IEnumerable<ThemePack> GetBuiltInThemes()
    {
        // Radzen built-in themes - these don't have custom CSS files, they use RadzenTheme component
        return
        [
            new ThemePack
            {
                Id = "light",
                Name = "Light",
                Author = "Radzen",
                Version = "1.0.0",
                Description = "Clean, bright theme for daytime use (Radzen default)",
                IsBuiltIn = true,
                ThemeCssPath = null, // Uses RadzenTheme component
                BaseDirectory = null,
                HasWarnings = false,
                Metadata = new ThemeMetadata { Id = "light", Name = "Light", BaseTheme = "light" }
            },
            new ThemePack
            {
                Id = "dark",
                Name = "Dark",
                Author = "Radzen",
                Version = "1.0.0",
                Description = "Easy on the eyes theme for low-light environments (Radzen dark)",
                IsBuiltIn = true,
                ThemeCssPath = null, // Uses RadzenTheme component
                BaseDirectory = null,
                HasWarnings = false,
                Metadata = new ThemeMetadata { Id = "dark", Name = "Dark", BaseTheme = "dark" }
            }
        ];
    }

    private async Task<ThemePack?> LoadThemePackFromDirectoryAsync(string themeDir, CancellationToken cancellationToken)
    {
        var themeJsonPath = Path.Combine(themeDir, ThemeJsonFileName);
        var themeCssPath = Path.Combine(themeDir, ThemeCssFileName);

        if (!File.Exists(themeJsonPath) || !File.Exists(themeCssPath))
        {
            logger.LogWarning("Theme directory missing required files: {Directory}", themeDir);
            return null;
        }

        var jsonContent = await File.ReadAllTextAsync(themeJsonPath, cancellationToken);
        var metadata = JsonSerializer.Deserialize<ThemeMetadata>(jsonContent);

        if (metadata == null || string.IsNullOrWhiteSpace(metadata.Id))
        {
            logger.LogWarning("Invalid theme.json in {Directory}", themeDir);
            return null;
        }

        // Validate theme pack
        var (isValid, warnings) = await ValidateThemePackAsync(themeDir, cancellationToken);

        var previewImage = metadata.PreviewImage;
        if (!string.IsNullOrEmpty(previewImage) && !previewImage.StartsWith('/') && !previewImage.StartsWith("http"))
        {
            previewImage = $"/api/v1/themes/{metadata.Id}/file/{previewImage}";
        }

        var branding = metadata.Branding;
        if (branding != null)
        {
            var logo = branding.LogoImage;
            if (!string.IsNullOrEmpty(logo) && !logo.StartsWith('/') && !logo.StartsWith("http"))
            {
                logo = $"/api/v1/themes/{metadata.Id}/file/{logo}";
            }

            var favicon = branding.Favicon;
            if (!string.IsNullOrEmpty(favicon) && !favicon.StartsWith('/') && !favicon.StartsWith("http"))
            {
                favicon = $"/api/v1/themes/{metadata.Id}/file/{favicon}";
            }

            branding = branding with { LogoImage = logo, Favicon = favicon };
        }

        return new ThemePack
        {
            Id = metadata.Id,
            Name = metadata.Name,
            Author = metadata.Author,
            Version = metadata.Version,
            Description = metadata.Description,
            IsBuiltIn = false,
            PreviewImage = previewImage,
            ThemeCssPath = $"/api/v1/themes/{metadata.Id}/file/{ThemeCssFileName}",
            BaseDirectory = themeDir,
            Metadata = metadata with { Branding = branding, PreviewImage = previewImage },
            HasWarnings = warnings.Any(),
            WarningDetails = warnings.Any() ? warnings : null
        };
    }

    private static Color ParseColor(string colorString)
    {
        colorString = colorString.Trim();

        // Handle hex colors
        if (colorString.StartsWith('#'))
        {
            colorString = colorString.TrimStart('#');

            if (colorString.Length == 3)
            {
                // Expand shorthand hex (#RGB -> #RRGGBB)
                colorString = $"{colorString[0]}{colorString[0]}{colorString[1]}{colorString[1]}{colorString[2]}{colorString[2]}";
            }

            if (colorString.Length == 6)
            {
                var r = Convert.ToInt32(colorString.Substring(0, 2), 16);
                var g = Convert.ToInt32(colorString.Substring(2, 2), 16);
                var b = Convert.ToInt32(colorString.Substring(4, 2), 16);
                return Color.FromArgb(r, g, b);
            }
        }

        // Handle rgb() format
        if (colorString.StartsWith("rgb(") && colorString.EndsWith(")"))
        {
            var values = colorString.Substring(4, colorString.Length - 5).Split(',');
            if (values.Length == 3)
            {
                var r = int.Parse(values[0].Trim());
                var g = int.Parse(values[1].Trim());
                var b = int.Parse(values[2].Trim());
                return Color.FromArgb(r, g, b);
            }
        }

        // Fallback to white
        return Color.White;
    }

    private static double GetRelativeLuminance(Color color)
    {
        var r = GetSRgbComponent(color.R / 255.0);
        var g = GetSRgbComponent(color.G / 255.0);
        var b = GetSRgbComponent(color.B / 255.0);

        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }

    private static double GetSRgbComponent(double component)
    {
        return component <= 0.03928
            ? component / 12.92
            : Math.Pow((component + 0.055) / 1.055, 2.4);
    }

    private async Task<string?> GetThemeLibraryPathAsync(CancellationToken cancellationToken)
    {
        var libraryResult = await libraryService.GetThemeLibraryAsync(cancellationToken);
        return libraryResult.Data?.Path;
    }
}

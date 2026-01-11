using System.IO.Compression;
using System.Text.Json;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Models;
using Melodee.Common.Services;
using Moq;
using Serilog;

namespace Melodee.Tests.Common.Services;

public class ThemeServiceTests
{
    private readonly Mock<IMelodeeConfigurationFactory> _configurationFactoryMock;
    private readonly Mock<IMelodeeConfiguration> _configurationMock;
    private readonly string _testThemeLibraryPath;

    public ThemeServiceTests()
    {
        _testThemeLibraryPath = Path.Combine(Path.GetTempPath(), $"melodee-themes-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testThemeLibraryPath);

        _configurationMock = new Mock<IMelodeeConfiguration>();
        _configurationMock.Setup(x => x.GetValue<string>(SettingRegistry.ThemeLibraryPath, It.IsAny<Func<string?, string>>())).Returns(_testThemeLibraryPath);
        _configurationMock.Setup(x => x.GetValue<string>(SettingRegistry.ThemeLibraryPath, It.IsAny<Func<string?, string>>())).Returns(_testThemeLibraryPath);
        _configurationMock.Setup(x => x.GetValue<int>(SettingRegistry.ThemeMaxUploadSizeMb, It.IsAny<Func<int, int>>()))
            .Returns((string k, Func<int, int> cb) => cb != null ? cb(0) : 50);
        _configurationMock.Setup(x => x.GetValue<bool>(SettingRegistry.ThemeEnforceContrastValidation, It.IsAny<Func<bool, bool>>())).Returns(false);

        _configurationFactoryMock = new Mock<IMelodeeConfigurationFactory>();
        _configurationFactoryMock.Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_configurationMock.Object);
    }

    private ThemeService CreateService()
    {
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<ThemeService>>();
        return new ThemeService(loggerMock.Object, _configurationFactoryMock.Object);
    }

    [Fact]
    public async Task DiscoverThemePacksAsync_ShouldIncludeBuiltInThemes()
    {
        var service = CreateService();
        var themes = (await service.DiscoverThemePacksAsync()).ToList();

        Assert.Contains(themes, t => t.Id == "light" && t.IsBuiltIn);
        Assert.Contains(themes, t => t.Id == "dark" && t.IsBuiltIn);
    }

    [Fact]
    public async Task DiscoverThemePacksAsync_ShouldDiscoverCustomThemes()
    {
        // Create a custom theme
        var themeId = "custom-theme";
        var themeDir = Path.Combine(_testThemeLibraryPath, themeId);
        Directory.CreateDirectory(themeDir);

        var metadata = new ThemeMetadata
        {
            Id = themeId,
            Name = "Custom Theme"
        };
        await File.WriteAllTextAsync(Path.Combine(themeDir, "theme.json"), JsonSerializer.Serialize(metadata));
        var allTokensCss = @"
:root {
  --md-surface-0: #000000; --md-surface-1: #111111; --md-surface-2: #222222;
  --md-text-1: #ffffff; --md-text-2: #cccccc; --md-text-inverse: #000000; --md-muted: #888888;
  --md-border: #333333; --md-divider: #444444;
  --md-primary: #0000ff; --md-primary-contrast: #ffffff;
  --md-accent: #00ff00; --md-accent-contrast: #000000;
  --md-focus: #ffff00;
  --md-success: #00ff00; --md-warning: #ffa500; --md-error: #ff0000; --md-info: #0000ff;
  --md-table-header-bg: #111111; --md-table-header-text: #ffffff;
  --md-chip-bg: #222222; --md-chip-text: #ffffff;
  --md-font-family-base: Arial; --md-font-family-heading: Arial; --md-font-family-mono: monospace;
}";
        await File.WriteAllTextAsync(Path.Combine(themeDir, "theme.css"), allTokensCss);

        var service = CreateService();
        var themes = (await service.DiscoverThemePacksAsync()).ToList();

        Assert.Contains(themes, t => t.Id == themeId && !t.IsBuiltIn);
    }

    [Fact]
    public async Task ValidateThemePackAsync_ShouldReturnFalse_WhenRequiredFilesMissing()
    {
        var themeDir = Path.Combine(_testThemeLibraryPath, "invalid-theme");
        Directory.CreateDirectory(themeDir);

        var service = CreateService();
        var (isValid, warnings) = await service.ValidateThemePackAsync(themeDir);

        Assert.False(isValid);
        Assert.Contains(warnings, w => w.Contains("theme.json") || w.Contains("theme.css"));
    }

    [Fact]
    public async Task ValidateThemePackAsync_ShouldReturnWarnings_WhenContrastIsLow()
    {
        var themeDir = Path.Combine(_testThemeLibraryPath, "low-contrast-theme");
        Directory.CreateDirectory(themeDir);

        var metadata = new ThemeMetadata { Id = "low-contrast", Name = "Low Contrast" };
        await File.WriteAllTextAsync(Path.Combine(themeDir, "theme.json"), JsonSerializer.Serialize(metadata));
        
        // Use very similar colors for text and surface
        var css = ":root { --md-text-1: #000000; --md-surface-0: #000001; --md-surface-1: #000002; --md-primary: #000000; --md-text-inverse: #000001; --md-table-header-text: #000000; --md-table-header-bg: #000001; --md-chip-text: #000000; --md-chip-bg: #000001; }";
        await File.WriteAllTextAsync(Path.Combine(themeDir, "theme.css"), css);

        var service = CreateService();
        var (_, warnings) = await service.ValidateThemePackAsync(themeDir);

        Assert.NotEmpty(warnings);
        Assert.Contains(warnings, w => w.Contains("Insufficient contrast"));
    }

    [Fact]
    public async Task ImportThemePackAsync_ShouldPretectAgainstZipSlip()
    {
        var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // Try to create an entry that points outside the extraction directory
            var entry = archive.CreateEntry("../../../evil.txt");
            using var writer = new StreamWriter(entry.Open());
            writer.WriteLine("evil");
        }
        memoryStream.Position = 0;

        var service = CreateService();
        var (success, _, error) = await service.ImportThemePackAsync(memoryStream);

        Assert.False(success);
        Assert.Contains("path traversal", error);
    }

    [Fact]
    public async Task ImportExport_ShouldWorkRoundTrip()
    {
        var themeId = "roundtrip-theme";
        var themeDir = Path.Combine(_testThemeLibraryPath, "temp-source");
        Directory.CreateDirectory(themeDir);

        var metadata = new ThemeMetadata { Id = themeId, Name = "Round Trip Theme" };
        await File.WriteAllTextAsync(Path.Combine(themeDir, "theme.json"), JsonSerializer.Serialize(metadata));
        var allTokensCss = @"
:root {
  --md-surface-0: #000000; --md-surface-1: #111111; --md-surface-2: #222222;
  --md-text-1: #ffffff; --md-text-2: #cccccc; --md-text-inverse: #000000; --md-muted: #888888;
  --md-border: #333333; --md-divider: #444444;
  --md-primary: #0000ff; --md-primary-contrast: #ffffff;
  --md-accent: #00ff00; --md-accent-contrast: #000000;
  --md-focus: #ffff00;
  --md-success: #00ff00; --md-warning: #ffa500; --md-error: #ff0000; --md-info: #0000ff;
  --md-table-header-bg: #111111; --md-table-header-text: #ffffff;
  --md-chip-bg: #222222; --md-chip-text: #ffffff;
  --md-font-family-base: Arial; --md-font-family-heading: Arial; --md-font-family-mono: monospace;
}";
        await File.WriteAllTextAsync(Path.Combine(themeDir, "theme.css"), allTokensCss);

        var zipPath = Path.Combine(_testThemeLibraryPath, "theme.zip");
        ZipFile.CreateFromDirectory(themeDir, zipPath);

        var service = CreateService();
        
        // Import
        using (var importStream = File.OpenRead(zipPath))
        {
            var (success, importedThemeId, error) = await service.ImportThemePackAsync(importStream);
            Assert.True(success, error);
            Assert.Equal(themeId, importedThemeId);
        }

        // Export
        var exportStream = await service.ExportThemePackAsync(themeId);
        Assert.NotNull(exportStream);

        using (var archive = new ZipArchive(exportStream))
        {
            Assert.Contains(archive.Entries, e => e.Name == "theme.json");
            Assert.Contains(archive.Entries, e => e.Name == "theme.css");
        }
    }
}

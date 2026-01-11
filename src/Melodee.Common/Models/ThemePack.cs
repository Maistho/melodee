using System.Text.Json.Serialization;

namespace Melodee.Common.Models;

/// <summary>
/// Represents theme pack metadata from theme.json
/// </summary>
public sealed record ThemeMetadata
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("author")]
    public string? Author { get; init; }

    [JsonPropertyName("version")]
    public string? Version { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("previewImage")]
    public string? PreviewImage { get; init; }

    [JsonPropertyName("branding")]
    public ThemeBranding? Branding { get; init; }

    [JsonPropertyName("fonts")]
    public ThemeFonts? Fonts { get; init; }

    [JsonPropertyName("navMenu")]
    public ThemeNavMenu? NavMenu { get; init; }

    [JsonPropertyName("baseTheme")]
    public string BaseTheme { get; init; } = "light";
}

/// <summary>
/// Theme branding configuration
/// </summary>
public sealed record ThemeBranding
{
    [JsonPropertyName("logoImage")]
    public string? LogoImage { get; init; }

    [JsonPropertyName("favicon")]
    public string? Favicon { get; init; }
}

/// <summary>
/// Theme typography/font configuration
/// </summary>
public sealed record ThemeFonts
{
    [JsonPropertyName("base")]
    public string? Base { get; init; }

    [JsonPropertyName("heading")]
    public string? Heading { get; init; }

    [JsonPropertyName("mono")]
    public string? Mono { get; init; }
}

/// <summary>
/// Theme NavMenu visibility configuration
/// </summary>
public sealed record ThemeNavMenu
{
    [JsonPropertyName("hidden")]
    public List<string>? Hidden { get; init; }
}

/// <summary>
/// Represents a complete theme pack
/// </summary>
public sealed record ThemePack
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Author { get; init; }
    public string? Version { get; init; }
    public string? Description { get; init; }
    public bool IsBuiltIn { get; init; }
    public string? PreviewImage { get; init; }
    public string? ThemeCssPath { get; init; }
    public string? BaseDirectory { get; init; }
    public ThemeMetadata? Metadata { get; init; }
    public bool HasWarnings { get; init; }
    public List<string>? WarningDetails { get; init; }
}

/// <summary>
/// DTO for API responses listing available themes
/// </summary>
public sealed record ThemePackInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Author { get; init; }
    public string? Version { get; init; }
    public string? Description { get; init; }
    public bool IsBuiltIn { get; init; }
    public string? PreviewImage { get; init; }
    public bool HasWarnings { get; init; }
    public List<string>? WarningDetails { get; init; }
}

/// <summary>
/// Request to set user theme preference
/// </summary>
public sealed record SetUserThemeRequest
{
    [JsonPropertyName("themeId")]
    public required string ThemeId { get; init; }
}

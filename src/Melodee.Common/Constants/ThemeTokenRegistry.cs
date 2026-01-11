namespace Melodee.Common.Constants;

/// <summary>
/// Design system tokens contract for theme packs.
/// All themes must define these CSS variables in their theme.css file.
/// </summary>
public static class ThemeTokenRegistry
{
    // Surface/Background Colors
    public const string SurfaceLevel0 = "--md-surface-0";
    public const string SurfaceLevel1 = "--md-surface-1";
    public const string SurfaceLevel2 = "--md-surface-2";

    // Text Colors
    public const string TextPrimary = "--md-text-1";
    public const string TextSecondary = "--md-text-2";
    public const string TextInverse = "--md-text-inverse";
    public const string TextMuted = "--md-muted";

    // Borders & Dividers
    public const string Border = "--md-border";
    public const string Divider = "--md-divider";

    // Primary Colors
    public const string Primary = "--md-primary";
    public const string PrimaryContrast = "--md-primary-contrast";

    // Accent Colors
    public const string Accent = "--md-accent";
    public const string AccentContrast = "--md-accent-contrast";

    // Focus/Outline
    public const string Focus = "--md-focus";

    // Status Colors
    public const string Success = "--md-success";
    public const string Warning = "--md-warning";
    public const string Error = "--md-error";
    public const string Info = "--md-info";

    // Table-specific
    public const string TableHeaderBackground = "--md-table-header-bg";
    public const string TableHeaderText = "--md-table-header-text";

    // Chip/Badge/Pill
    public const string ChipBackground = "--md-chip-bg";
    public const string ChipText = "--md-chip-text";

    // Typography
    public const string FontFamilyBase = "--md-font-family-base";
    public const string FontFamilyHeading = "--md-font-family-heading";
    public const string FontFamilyMono = "--md-font-family-mono";

    /// <summary>
    /// Required contrast validation pairs (foreground, background)
    /// </summary>
    public static readonly (string Foreground, string Background)[] RequiredContrastPairs =
    [
        (TextPrimary, SurfaceLevel0),
        (TextPrimary, SurfaceLevel1),
        (TextInverse, Primary),
        (TableHeaderText, TableHeaderBackground),
        (ChipText, ChipBackground)
    ];

    /// <summary>
    /// All required tokens that must be present in a valid theme
    /// </summary>
    public static readonly string[] RequiredTokens =
    [
        SurfaceLevel0, SurfaceLevel1, SurfaceLevel2,
        TextPrimary, TextSecondary, TextInverse, TextMuted,
        Border, Divider,
        Primary, PrimaryContrast,
        Accent, AccentContrast,
        Focus,
        Success, Warning, Error, Info,
        TableHeaderBackground, TableHeaderText,
        ChipBackground, ChipText,
        FontFamilyBase, FontFamilyHeading, FontFamilyMono
    ];
}

/// <summary>
/// NavMenu item IDs for visibility control via theme packs
/// </summary>
public static class NavMenuItemRegistry
{
    public const string Home = "home";
    public const string Dashboard = "dashboard";
    public const string Stats = "stats";
    public const string Search = "search";
    public const string Artists = "artists";
    public const string Albums = "albums";
    public const string Songs = "songs";
    public const string Charts = "charts";
    public const string Libraries = "libraries";
    public const string NowPlaying = "nowplaying";
    public const string Jukebox = "jukebox";
    public const string PartyMode = "party";
    public const string Playlists = "playlists";
    public const string Podcasts = "podcasts";
    public const string RadioStations = "radiostations";
    public const string Requests = "requests";
    public const string Shares = "shares";
    public const string Users = "users";
    public const string Admin = "admin";
    public const string About = "about";

    /// <summary>
    /// All valid NavMenu item IDs
    /// </summary>
    public static readonly string[] AllItems =
    [
        Home, Dashboard, Stats, Search, Artists, Albums, Songs, Charts,
        Libraries, NowPlaying, Jukebox, PartyMode, Playlists, Podcasts,
        RadioStations, Requests, Shares, Users, Admin, About
    ];
}

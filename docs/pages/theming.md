# Theming Melodee

Melodee supports a powerful theming system that allows users to customize the visual appearance of the application. Themes can control colors, typography, and even the visibility of navigation menu items.

## Theme Structure

A theme is packaged as a folder containing at least two files:

1.  `theme.json`: Metadata and configuration.
2.  `theme.css`: CSS variables defining the design tokens.

### theme.json

```json
{
  "id": "my-custom-theme",
  "name": "My Custom Theme",
  "description": "A beautiful custom theme for Melodee.",
  "author": "Your Name",
  "version": "1.0.0",
  "baseTheme": "dark",
  "branding": {
    "logoImage": "logo.png",
    "favicon": "favicon.ico"
  },
  "fonts": {
    "base": "Inter, sans-serif",
    "heading": "Outfit, sans-serif",
    "mono": "Fira Code, monospace"
  },
  "navMenu": {
    "hidden": ["jukebox", "podcasts"]
  }
}
```

- `baseTheme`: Either `light` or `dark`. This determines the base Radzen theme used.
- `branding`: Optional customizations for the logo and favicon.
- `fonts`: Custom font families.
- `navMenu.hidden`: List of navigation item IDs to hide. Available IDs: `dashboard`, `stats`, `artists`, `albums`, `charts`, `libraries`, `nowplaying`, `jukebox`, `party`, `playlists`, `podcasts`, `radiostations`, `requests`, `songs`, `shares`, `users`, `admin`, `about`.

### theme.css

The CSS file must define a set of standardized design tokens as CSS variables under the `:root` selector.

```css
:root {
  /* Surface colors */
  --md-surface-0: #121212;
  --md-surface-1: #1e1e1e;
  --md-surface-2: #2c2c2c;

  /* Text colors */
  --md-text-1: #ffffff;
  --md-text-2: #b3b3b3;
  --md-text-inverse: #000000;
  --md-muted: #737373;

  /* Accent colors */
  --md-primary: #1db954;
  --md-primary-contrast: #ffffff;
  --md-accent: #1ed760;
  --md-accent-contrast: #000000;

  /* Status colors */
  --md-success: #1db954;
  --md-warning: #ffa500;
  --md-error: #f15555;
  --md-info: #2196f3;

  /* UI Elements */
  --md-border: #333333;
  --md-divider: #282828;
  --md-focus: #ffffff;
  --md-table-header-bg: #282828;
  --md-table-header-text: #ffffff;
  --md-chip-bg: #3e3e3e;
  --md-chip-text: #ffffff;

  /* Fonts (will be overridden by theme.json if provided) */
  --md-font-family-base: 'Inter', sans-serif;
  --md-font-family-heading: 'Outfit', sans-serif;
  --md-font-family-mono: 'monospace';
}
```

## Creating a Theme Pack

To create a theme pack for distribution:
1. Create a folder named after your theme ID.
2. Add `theme.json` and `theme.css`.
3. (Optional) Add assets like favicons within the same folder.
4. Zip the folder.

## Importing Themes

Administrators can import theme packs via the Settings > Themes section in the web interface or by placing the theme folder directly into the configured `ThemeLibraryPath`.

## Accessibility (Contrast Ratio)

Melodee enforces WCAG AA standards for contrast. When a theme is loaded, the system validates the contrast ratio of key color pairs (e.g., text vs. background). If the contrast is insufficient, a warning will be displayed in the logs or during import.

Standard pairs checked:
- Primary Text over Surface Level 0/1/2
- Primary Contrast over Primary
- Accent Contrast over Accent
- Table Header Text over Table Header BG
- Chip Text over Chip BG

# Theming Melodee

Melodee supports a powerful theming system that allows users to customize the visual appearance of the application. Themes can control colors, typography, and even the visibility of navigation menu items.

## Download Themes

Pre-built theme packs are available in the [Melodee repository's `/themes` directory](https://github.com/sphildreth/melodee/tree/main/themes). Download any theme zip and import it via **Admin > Themes** in your Melodee instance.

## Built-in Themes

Melodee includes several built-in themes:

### Radzen Standard Themes

These are the standard themes provided by the Radzen Blazor component library:

| Theme | Description |
|-------|-------------|
| **light** | Clean, bright theme (Radzen default light) |
| **dark** | Easy on the eyes for low-light environments (Radzen default dark) |
| default | Standard Radzen light theme |
| standard | Standard Radzen theme |
| humanistic | Warm, friendly appearance |
| software | Professional software look |
| material | Material Design inspired |
| standard-dark | Standard dark variant |
| humanistic-dark | Humanistic dark variant |
| software-dark | Software dark variant |
| material-dark | Material dark variant |

### Melodee Custom Themes

These themes extend Radzen's base themes with Melodee's brand colors:

| Theme | Base | Description |
|-------|------|-------------|
| **Melodee** | light | White/gray backgrounds with purple/magenta accents and gradient buttons |
| **Melodee Dark** | dark | Dark backgrounds with purple/magenta accents and gradient buttons |
| Synthwave | dark | Retro 80s neon aesthetic with magenta and cyan |
| Midnight Galaxy | dark | Deep space purple theme |
| Ocean Breeze | light | Calming blue ocean colors |
| Forest | light | Natural green earth tones |
| Sunset Vibes | light | Warm orange and coral sunset colors |

## Default Theme

The system default theme is **Radzen Dark**. If the Theme library is not configured or a custom theme fails to load, the application falls back to Radzen's built-in dark theme. Administrators can change the system default theme in **Admin > Themes**.

Users can select their preferred theme from the theme selector in the application header. The selected theme is stored in a cookie and persists across sessions.

## Theme Structure

A custom theme is packaged as a folder containing at least two files:

1. `theme.json`: Metadata and configuration
2. `theme.css`: CSS variables defining the design tokens

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

- `baseTheme`: Either `light` or `dark`. This determines which Radzen base theme CSS is loaded before your custom CSS.
- `branding`: Optional customizations for the logo and favicon.
- `fonts`: Custom font families.
- `navMenu.hidden`: List of navigation item IDs to hide. Available IDs: `dashboard`, `stats`, `artists`, `albums`, `charts`, `libraries`, `nowplaying`, `jukebox`, `party`, `playlists`, `podcasts`, `radiostations`, `requests`, `songs`, `shares`, `users`, `admin`, `about`.

### theme.css

The CSS file defines design tokens as CSS variables. Your theme CSS loads **after** the Radzen base theme, so you only need to override the variables you want to change.

#### Melodee Design Tokens

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

  /* Fonts */
  --md-font-family-base: 'Inter', sans-serif;
  --md-font-family-heading: 'Outfit', sans-serif;
  --md-font-family-mono: 'monospace';
}
```

#### Radzen Compatibility Variables

For full theme control, you should also override Radzen's CSS variables:

```css
:root {
  /* Primary color scheme */
  --rz-primary: #E040FB;
  --rz-primary-light: #F06EFF;
  --rz-primary-lighter: rgba(224, 64, 251, 0.16);
  --rz-primary-dark: #C020DB;
  --rz-primary-darker: #9C00B7;

  /* Secondary color scheme */
  --rz-secondary: #FF7043;
  --rz-secondary-light: #FF9A76;
  --rz-secondary-dark: #F4511E;

  /* Status colors */
  --rz-success: #4DB6AC;
  --rz-warning: #FFCA28;
  --rz-danger: #FF5252;
  --rz-info: #FFA726;

  /* Background colors */
  --rz-base-background-color: #FFFFFF;
  --rz-body-background-color: #FFFFFF;

  /* Text colors */
  --rz-text-color: #1a1a2e;
  --rz-text-secondary-color: #4a4a5a;

  /* Border colors */
  --rz-border-color: #E0E0E0;
  --rz-border-color-hover: #E040FB;
}
```

#### Custom Styling Example

You can also add component-specific CSS after the variables:

```css
/* Gradient primary buttons */
.rz-button.rz-primary {
  background: linear-gradient(135deg, #E040FB 0%, #FF7043 50%, #FFA726 100%);
  border: none;
}

/* Focus states */
.rz-textbox:focus {
  border-color: #E040FB;
  box-shadow: 0 0 0 3px rgba(224, 64, 251, 0.2);
}
```

## Creating a Theme Pack

To create a theme pack for distribution:

1. Create a folder named after your theme ID (e.g., `my-theme`)
2. Add `theme.json` with your theme metadata
3. Add `theme.css` with your design tokens and custom styles
4. (Optional) Add assets like logos or favicons
5. Zip the folder

## Importing Themes

Administrators can import theme packs via **Admin > Themes** in the web interface, or by placing the theme folder directly into the Themes library directory (configured in **Admin > Libraries**).

## Accessibility (Contrast Ratio)

Melodee enforces WCAG AA standards for contrast. When a theme is loaded, the system validates the contrast ratio of key color pairs (e.g., text vs. background). If the contrast is insufficient, a warning will be displayed in the logs or during import.

Standard pairs checked:

- Primary Text over Surface Level 0/1/2
- Primary Contrast over Primary
- Accent Contrast over Accent
- Table Header Text over Table Header BG
- Chip Text over Chip BG

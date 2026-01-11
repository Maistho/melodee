### Description
Add first-class theme support to Melodee so users can switch between built-in Light/Dark themes and install/share custom themes (“theme packs”) without rebuilding the server.

### Requirements

### 1) Built-in Themes
- Provide at minimum two built-in themes:
    - `Light`
    - `Dark`
- Theme selection must be available in the UI (e.g., user settings menu).
- Theme changes must apply immediately without page reload when possible.

### 2) Theme Preference Persistence
- Store theme preference per user (authenticated users) in the database.
- Provide a fallback for unauthenticated scenarios (if applicable) using local storage.
- If a user has no preference set, use a system default (configurable; default = Light).

### 3) Theme Packs (Extension Point)

#### 3.1 Theme pack location
- Support external theme packs deployable as files (no recompilation):
    - A theme pack is a folder under a configured themes library, e.g.:
        - `/data/themes/<themeId>/`

#### 3.2 Theme pack contents
A theme pack contains:
- `theme.json` (required): metadata + configuration (nav visibility, branding, fonts)
- `theme.css` (required): CSS variables and any overrides
- Optional assets:
    - images (logo, preview, background)
    - fonts (woff/woff2/ttf/otf)

#### 3.3 Discovery and lifecycle
- Theme packs must be discoverable at runtime:
    - The server scans the themes directory on startup.
    - Provide an admin-triggered **Rescan Themes** action so new packs can be added without restart.
- Provide import/export workflows:
    - Import: admin uploads a zipped theme pack to the Themes Library (must be only 1 of this type).
    - Export: download theme pack as zip.

### 4) Theme Tokens (Design System Contract)
Define a stable set of CSS variables (“design tokens”) that all themes can override.
- All first-party UI styling must be driven via tokens (avoid hardcoded colors in components).
- Tokens must cover at least:
    - surfaces/backgrounds
    - text colors (primary/secondary/muted/inverted)
    - borders/dividers
    - primary/accent + hover/active states
    - focus/outline color
    - status colors (success/warn/error/info)
    - table/list header background + header text
    - label/metadata text + backgrounds used in chips/pills/badges

**Minimum token set (example; can be extended but should be stable):**
- `--md-surface-0`, `--md-surface-1`, `--md-surface-2`
- `--md-text-1`, `--md-text-2`, `--md-text-inverse`, `--md-muted`
- `--md-border`, `--md-divider`
- `--md-primary`, `--md-primary-contrast`
- `--md-accent`, `--md-accent-contrast`
- `--md-focus`
- `--md-success`, `--md-warning`, `--md-error`, `--md-info`
- `--md-table-header-bg`, `--md-table-header-text`
- `--md-chip-bg`, `--md-chip-text`

### 5) Typography / Font Support (NEW)
Theme packs must be able to define font(s) used by the Blazor UI via theme settings.
- Theme supports setting:
    - Base UI font family
    - Heading font family (optional; defaults to base)
    - Monospace font family (optional)
- Themes may include local font files in the theme pack and reference them from `theme.css` using `@font-face`.
- Provide standard typography tokens (at minimum):
    - `--md-font-family-base`
    - `--md-font-family-heading`
    - `--md-font-family-mono`
- UI must apply these tokens consistently:
    - global body text uses base
    - headings and major titles use heading
    - code/log/mono UI uses mono

### 6) NavMenu Visibility Controls (NEW)
Theme packs must be able to hide NavMenu items from a theme file (no code changes required per theme).
- Define a stable set of **NavMenu item IDs** (strings) used for visibility control.
- `theme.json` can specify:
    - `navMenu.hidden`: list of item IDs to hide
- Hiding is purely a UI concern:
    - routes still exist and are governed by authorization as usual
    - hidden items must not appear in NavMenu (desktop + mobile)
- Provide a documented list of supported NavMenu IDs.

**Example NavMenu IDs (final list must match the actual app):**
- `home`, `search`, `artists`, `albums`, `songs`, `playlists`, `charts`, `shares`, `settings`, `admin`

### 7) Contrast & Accessibility (NEW)
Themes must allow proper contrast so text in headers, columns, labels, etc. remains easy to read with various colored backgrounds.

#### 7.1 Contrast targets
- Themes should meet **WCAG 2.x AA contrast** targets:
    - Normal text: >= 4.5:1
    - Large text (>= 18pt regular or 14pt bold): >= 3:1

#### 7.2 Required contrast pairs to validate
At minimum, validate these token pairs (theme must provide both values):
- `--md-text-1` on `--md-surface-0`
- `--md-text-1` on `--md-surface-1`
- `--md-text-inverse` on `--md-primary`
- `--md-table-header-text` on `--md-table-header-bg`
- `--md-chip-text` on `--md-chip-bg`

#### 7.3 Validation behavior
- On theme load/rescan/import:
    - Parse tokens from `theme.css` (at least CSS variable assignments in `:root`)
    - Compute contrast ratios for the required pairs
- If a theme fails validation:
    - Mark it as `hasWarnings=true` in theme listing (include which checks failed)
    - Admin UI must surface warnings clearly
    - Default behavior: allow selection but warn (admin can optionally enforce “block invalid themes” later)

### 8) Admin Controls & Safety
- Only admins can install/remove theme packs (admin-only by default).
- Validate theme pack structure on load:
    - required files present (`theme.json`, `theme.css`)
    - theme id uniqueness
    - basic size limits for uploaded zips (configurable; set a reasonable default)
    - zip-slip protection (no path traversal)
- If a selected theme pack is missing/invalid, fall back to default theme gracefully.

### 9) API + UI Integration

#### 9.1 API endpoints (minimum)
- `GET /api/v1/themes` (or equivalent):
    - returns built-in + installed theme packs with:
        - id, name, version, author, description
        - isBuiltIn
        - previewImage (optional)
        - hasWarnings + warningDetails (optional)
- `POST /api/v1/users/me/theme`:
    - sets user theme preference (themeId)
- Admin endpoints:
    - `POST /api/v1/admin/themes/rescan`
    - `POST /api/v1/admin/themes/import` (zip upload)
    - `GET /api/v1/admin/themes/{themeId}/export` (zip)
    - `DELETE /api/v1/admin/themes/{themeId}` (remove/uninstall)

#### 9.2 UI behavior
- Theme picker UI:
    - shows available themes, preview image if present
    - shows warnings badge if `hasWarnings=true`
- Applying a theme:
    - updates the active theme CSS link (or variables) without full reload when possible
    - applies typography tokens globally
    - applies nav visibility changes immediately (NavMenu re-renders)

### 10) Documentation
Add a “Theming” doc page that includes:
- where to place theme packs on disk
- `theme.json` schema (with examples)
- supported token list (design tokens)
- nav menu supported IDs and how to hide items
- typography/font examples (including local `@font-face`)
- how to package/share a theme
- troubleshooting (cache busting, invalid theme fallback, warning meanings)

---

## `theme.json` Schema (Example)

```json
{
  "id": "midnight",
  "name": "Midnight",
  "author": "Your Name",
  "version": "1.0.0",
  "description": "Dark, high-contrast theme with custom fonts.",
  "previewImage": "assets/preview.png",

  "branding": {
    "logoImage": "assets/logo.svg",
    "favicon": "assets/favicon.ico"
  },

  "fonts": {
    "base": "Inter, system-ui, -apple-system, Segoe UI, Roboto, Arial, sans-serif",
    "heading": "Inter, system-ui, -apple-system, Segoe UI, Roboto, Arial, sans-serif",
    "mono": "JetBrains Mono, ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace"
  },

  "navMenu": {
    "hidden": ["charts", "shares"]
  }
}
```

## `theme.css` Example (Skeleton)

```css
/* Optional local fonts */
@font-face {
  font-family: "Inter";
  src: url("./assets/fonts/Inter-Variable.woff2") format("woff2");
  font-display: swap;
}

/* Theme tokens */
:root {
  --md-font-family-base: Inter, system-ui, -apple-system, Segoe UI, Roboto, Arial, sans-serif;
  --md-font-family-heading: Inter, system-ui, -apple-system, Segoe UI, Roboto, Arial, sans-serif;
  --md-font-family-mono: "JetBrains Mono", ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;

  --md-surface-0: #0b0f17;
  --md-surface-1: #101827;
  --md-surface-2: #162033;

  --md-text-1: #e8eefc;
  --md-text-2: #b8c2dc;
  --md-text-inverse: #0b0f17;
  --md-muted: #93a1c6;

  --md-border: #24314f;
  --md-divider: #1b2742;

  --md-primary: #7aa2ff;
  --md-primary-contrast: #0b0f17;
  --md-accent: #7ef0c1;
  --md-accent-contrast: #0b0f17;

  --md-focus: #ffd86b;

  --md-success: #3ddc97;
  --md-warning: #ffcc66;
  --md-error: #ff6b6b;
  --md-info: #5bc0ff;

  --md-table-header-bg: #162033;
  --md-table-header-text: #e8eefc;

  --md-chip-bg: #24314f;
  --md-chip-text: #e8eefc;
}

/* Example global application of fonts/tokens */
body {
  font-family: var(--md-font-family-base);
  background: var(--md-surface-0);
  color: var(--md-text-1);
}

h1, h2, h3, h4, h5 {
  font-family: var(--md-font-family-heading);
}
```

### Success Criteria
- ✅ User can switch between Light and Dark in the UI and the preference persists across sessions/devices for that user.
- ✅ A custom theme pack can be installed by file deployment (drop-in folder under the configured themes directory) without rebuilding the server.
- ✅ Installed theme packs appear in the theme picker and can be selected by users.
- ✅ Selecting a theme updates the UI styling consistently (no mixed theme colors).
- ✅ Theme pack can define UI font(s) and they apply consistently across the Blazor UI.
- ✅ Theme pack can hide specified NavMenu items via `theme.json`, applied immediately.
- ✅ Theme validation identifies insufficient-contrast token pairs and surfaces warnings in admin UI/theme picker.
- ✅ Removing/invalidating a selected theme falls back safely to the default.
- ✅ Documentation exists describing how to create, install, validate, and share themes, including a working example theme pack.

## Out of Scope (for this issue)
- Per-page or per-component bespoke styling APIs beyond token usage (themes should work by overriding tokens).
- Per-user custom nav menu composition beyond hide/show (future enhancement).
- A full theme marketplace or remote theme repository (future enhancement).

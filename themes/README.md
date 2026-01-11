# Melodee Theme Library

This directory contains pre-built theme packs ready for administrators to import into their Melodee instance.

📖 **Full documentation**: [melodee.org/themes](https://melodee.org/themes)

## Available Themes

### Melodee Signature Themes

| Theme | Version | Description | Base |
|-------|---------|-------------|------|
| [melodee.zip](melodee.zip) | 1.0.0 | Official Melodee light theme - white/gray backgrounds with purple/magenta accents | light |
| [melodee-dark.zip](melodee-dark.zip) | 1.0.0 | Official Melodee dark theme - dark backgrounds with purple/magenta accents | dark |

### Community Themes

| Theme | Version | Description | Base |
|-------|---------|-------------|------|
| [forest.zip](forest.zip) | 1.0.0 | Natural green earth tones | light |
| [midnight-galaxy.zip](midnight-galaxy.zip) | 1.0.0 | Deep space purples with starry accents | dark |
| [ocean-breeze.zip](ocean-breeze.zip) | 1.0.0 | Calming ocean blues and teals | light |
| [sunset-vibes.zip](sunset-vibes.zip) | 1.0.0 | Warm orange and coral sunset colors | light |
| [synthwave.zip](synthwave.zip) | 1.0.0 | Retro 80s neon aesthetic with magenta and cyan | dark |

## Installation

1. Download the theme zip file you want
2. Go to **Admin > Themes** in your Melodee instance
3. Click **Import Theme** and select the zip file
4. The theme will appear in the theme selector

Alternatively, extract the zip directly into your Themes library directory (see **Admin > Libraries**).

## Versioning

Theme packs follow semantic versioning (`MAJOR.MINOR.PATCH`). The version is specified in the `theme.json` file inside each zip:

```json
{
  "id": "my-theme",
  "name": "My Theme",
  "version": "1.0.0",
  ...
}
```

- **MAJOR**: Breaking changes (e.g., required new CSS variables)
- **MINOR**: New features, backward compatible
- **PATCH**: Bug fixes, color adjustments

When Melodee introduces breaking changes to the theming system, themes will need a major version bump.

## Creating Custom Themes

See the [theming documentation](https://melodee.org/themes) for instructions on creating your own themes.

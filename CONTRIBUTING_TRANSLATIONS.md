# Contributing Translations to Melodee

Thank you for your interest in helping translate Melodee! Making the application accessible to users worldwide is a community effort, and we greatly appreciate your help.

## Table of Contents

- [Overview](#overview)
- [Supported Languages](#supported-languages)
- [Translation Status](#translation-status)
- [Getting Started](#getting-started)
- [How to Translate](#how-to-translate)
- [Translation Guidelines](#translation-guidelines)
- [Testing Your Translations](#testing-your-translations)
- [Submitting Translations](#submitting-translations)
- [For Developers: Adding New Keys](#for-developers-adding-new-keys)
- [FAQ](#faq)

## Overview

Melodee uses standard .NET resource files (`.resx`) for localization. These are XML files that contain key-value pairs where:
- **Key**: A unique identifier (e.g., `Actions.Save`, `Messages.WelcomeUser`)
- **Value**: The translated text for that language

All resource files are located in:
```
src/Melodee.Blazor/Resources/
```

## Supported Languages

| Language | Code | File | Status | RTL Support |
|----------|------|------|--------|-------------|
| English (US) | en-US | `SharedResources.resx` | ✅ 100% | No |
| Japanese | ja-JP | `SharedResources.ja-JP.resx` | ✅ 100% | No |
| Spanish | es-ES | `SharedResources.es-ES.resx` | ✅ 100% | No |
| Portuguese (Brazil) | pt-BR | `SharedResources.pt-BR.resx` | ✅ 100% | No |
| German | de-DE | `SharedResources.de-DE.resx` | ✅ 100% | No |
| Italian | it-IT | `SharedResources.it-IT.resx` | 🔄 32% | No |
| Russian | ru-RU | `SharedResources.ru-RU.resx` | ✅ 100% | No |
| Chinese (Simplified) | zh-CN | `SharedResources.zh-CN.resx` | ✅ 100% | No |
| French | fr-FR | `SharedResources.fr-FR.resx` | ✅ 100% | No |
| Arabic (Saudi Arabia) | ar-SA | `SharedResources.ar-SA.resx` | ✅ 100% | Yes |
| Dutch (Netherlands) | nl-NL | `SharedResources.nl-NL.resx` | 🔄 2% | No |
| Polish (Poland) | pl-PL | `SharedResources.pl-PL.resx` | 🔄 2% | No |
| Turkish (Turkey) | tr-TR | `SharedResources.tr-TR.resx` | 🔄 2% | No |
| Indonesian (Indonesia) | id-ID | `SharedResources.id-ID.resx` | 🔄 2% | No |
| Korean (Korea) | ko-KR | `SharedResources.ko-KR.resx` | 🔄 2% | No |
| Vietnamese (Vietnam) | vi-VN | `SharedResources.vi-VN.resx` | 🔄 2% | No |
| Persian (Iran) | fa-IR | `SharedResources.fa-IR.resx` | 🔄 2% | Yes |
| Ukrainian (Ukraine) | uk-UA | `SharedResources.uk-UA.resx` | 🔄 2% | No |
| Czech (Czechia) | cs-CZ | `SharedResources.cs-CZ.resx` | 🔄 2% | No |
| Swedish (Sweden) | sv-SE | `SharedResources.sv-SE.resx` | 🔄 2% | No |

## Translation Status

Entries marked with `[NEEDS TRANSLATION]` require translation. You can find the current count of untranslated entries by searching for this marker in each file.

To check status from the command line:
```bash
grep -c "NEEDS TRANSLATION" src/Melodee.Blazor/Resources/SharedResources.*.resx
```

**Want to add a new language?** Open an issue on GitHub to discuss adding support for your language.

## Getting Started

### Prerequisites

- Git
- A text editor (VS Code, Notepad++, Sublime Text, etc.)
- Optionally: .NET 10 SDK to test your translations locally

### Fork and Clone

```bash
# Fork the repository on GitHub, then:
git clone https://github.com/YOUR_USERNAME/melodee.git
cd melodee

# Create a branch for your translations
git checkout -b translations/your-language-code
```

## How to Translate

### Step 1: Open Your Language File

Navigate to `src/Melodee.Blazor/Resources/` and open the file for your language.

For example, for Spanish:
```
src/Melodee.Blazor/Resources/SharedResources.es-ES.resx
```

### Step 2: Find Entries Needing Translation

Search for `[NEEDS TRANSLATION]` in the file. These entries look like:

```xml
<data name="Actions.Export" xml:space="preserve">
  <value>[NEEDS TRANSLATION] Export</value>
</data>
```

### Step 3: Replace with Your Translation

Remove the `[NEEDS TRANSLATION]` prefix and replace the entire value with your translation:

```xml
<data name="Actions.Export" xml:space="preserve">
  <value>Exportar</value>
</data>
```

### Step 4: Preserve Placeholders

Many strings contain placeholders like `{0}`, `{1}`, etc. **These must be preserved** in your translation:

```xml
<!-- English -->
<data name="Messages.ItemCount" xml:space="preserve">
  <value>Found {0} items in {1} albums</value>
</data>

<!-- Spanish - placeholders preserved -->
<data name="Messages.ItemCount" xml:space="preserve">
  <value>Se encontraron {0} elementos en {1} álbumes</value>
</data>
```

### Step 5: Save and Validate

After making changes, run the validation script:

```bash
bash scripts/validate-resources.sh
```

This ensures all language files have consistent keys.

## Translation Guidelines

### Do's ✅

- **Keep translations concise** - UI space is often limited
- **Preserve placeholders** - `{0}`, `{1}`, etc. must remain in the translation
- **Match the tone** - Melodee uses a friendly, informal tone
- **Use native terms** - Translate technical terms appropriately for your locale
- **Be consistent** - Use the same translation for the same term throughout
- **Consider context** - The key name often hints at where the text appears (e.g., `Actions.` for buttons, `Messages.` for notifications)

### Don'ts ❌

- **Don't translate placeholders** - Keep `{0}`, `{1}` as-is
- **Don't translate key names** - Only translate the `<value>` content
- **Don't add HTML or formatting** - Keep translations as plain text
- **Don't use machine translation without review** - Always verify automated translations

### Key Naming Convention

Understanding key prefixes helps provide context:

| Prefix | Usage | Example |
|--------|-------|---------|
| `Actions.` | Buttons and clickable actions | `Actions.Save`, `Actions.Delete` |
| `Common.` | Shared labels used in many places | `Common.Name`, `Common.Description` |
| `Messages.` | User notifications and feedback | `Messages.SaveSuccess`, `Messages.Error` |
| `Navigation.` | Menu items and navigation | `Navigation.Dashboard`, `Navigation.Settings` |
| `Validation.` | Form validation errors | `Validation.Required`, `Validation.InvalidEmail` |
| `Dialog.` | Dialog titles and content | `Dialog.ConfirmDelete`, `Dialog.Cancel` |
| `Tooltip.` | Hover tooltips | `Tooltip.ClickToEdit` |
| `{PageName}.` | Page-specific text | `Dashboard.RecentActivity`, `Albums.FilterByGenre` |

### Examples of Good Translations

**Simple button text:**
```xml
<!-- English -->
<value>Save</value>

<!-- Good Spanish translation -->
<value>Guardar</value>
```

**Text with placeholder:**
```xml
<!-- English -->
<value>Welcome back, {0}!</value>

<!-- Good French translation -->
<value>Bon retour, {0} !</value>
```

**Longer message:**
```xml
<!-- English -->
<value>Are you sure you want to delete this album? This action cannot be undone.</value>

<!-- Good German translation -->
<value>Möchten Sie dieses Album wirklich löschen? Diese Aktion kann nicht rückgängig gemacht werden.</value>
```

## Testing Your Translations

### Running Locally

If you have .NET 10 SDK installed:

```bash
# Build and run
dotnet run --project src/Melodee.Blazor

# Open http://localhost:5157 in your browser
# Change language using the language selector in the header
```

### Visual Inspection

After running, check:
- Text doesn't overflow containers
- Buttons and labels are readable
- Numbers and dates format correctly
- RTL layout works (for Arabic)

## Submitting Translations

### 1. Validate Your Changes

```bash
bash scripts/validate-resources.sh
```

All checks must pass before submitting.

### 2. Commit Your Changes

```bash
git add src/Melodee.Blazor/Resources/SharedResources.YOUR-LANG.resx
git commit -m "translations(YOUR-LANG): translate X entries"
```

**Commit message examples:**
- `translations(es-ES): translate 50 action labels`
- `translations(ja-JP): complete settings page translations`
- `translations(de-DE): fix typo in welcome message`

### 3. Push and Create Pull Request

```bash
git push origin translations/your-language-code
```

Then create a Pull Request on GitHub with:
- Title: `translations(LANG): brief description`
- Description: List what you translated
- Any notes about translation choices

### 4. Review Process

A maintainer will review your translations. For languages maintainers don't speak, we may:
- Ask community members to verify
- Use review tools to check consistency
- Ask clarifying questions

## For Developers: Adding New Keys

**⚠️ CRITICAL**: When adding ANY new localization key, you **MUST** add it to ALL 20 language files in a SINGLE commit.

### Use the Helper Script (Recommended)

```bash
bash scripts/add-localization-key.sh "YourKey.Name" "English text here"
```

This automatically:
- Adds the key to all 20 resource files
- Uses the English value for en-US
- Adds `[NEEDS TRANSLATION]` prefix for other languages

### Manual Addition

If adding manually, you must update ALL files:
1. `SharedResources.resx` (English - base file)
2. `SharedResources.ar-SA.resx`
3. `SharedResources.cs-CZ.resx`
4. `SharedResources.de-DE.resx`
5. `SharedResources.es-ES.resx`
6. `SharedResources.fa-IR.resx`
7. `SharedResources.fr-FR.resx`
8. `SharedResources.id-ID.resx`
9. `SharedResources.it-IT.resx`
10. `SharedResources.ja-JP.resx`
11. `SharedResources.ko-KR.resx`
12. `SharedResources.nl-NL.resx`
13. `SharedResources.pl-PL.resx`
14. `SharedResources.pt-BR.resx`
15. `SharedResources.ru-RU.resx`
16. `SharedResources.sv-SE.resx`
17. `SharedResources.tr-TR.resx`
18. `SharedResources.uk-UA.resx`
19. `SharedResources.vi-VN.resx`
20. `SharedResources.zh-CN.resx`

### Validation

**Always run validation before committing:**
```bash
bash scripts/validate-resources.sh
```

The CI/CD pipeline will fail if keys are missing from any language file.

## FAQ

### Can I submit partial translations?

**Yes!** Every contribution helps. You don't need to translate everything at once.

### What if I'm unsure about a translation?

Add a comment in your PR explaining the uncertainty. The community can help review.

### Can I improve existing translations?

Absolutely! If you see a better way to phrase something, submit a PR with improvements.

### What if a placeholder format changes in different languages?

Keep the placeholders (`{0}`, `{1}`) but feel free to reorder them if your language requires a different word order. The numbers correspond to specific values, not positions.

### How do I handle plural forms?

Currently, Melodee uses simple placeholder-based translations. For complex plural rules, we use separate keys like `Messages.OneItem` and `Messages.MultipleItems`.

### Can I use regional variations?

For now, we use one file per language (e.g., `es-ES` for all Spanish). Regional variations can be discussed via GitHub issues.

### What tools can I use to edit .resx files?

- **Text editors**: VS Code, Sublime Text, Notepad++ (files are XML)
- **Visual Studio**: Built-in resource editor with a nice GUI
- **ResX Resource Manager**: Free VS extension for managing multiple resource files
- **Online**: XML editors or specialized .resx editors

### The validation script found missing keys. What do I do?

If you're only translating (not adding new keys), this shouldn't happen. If it does:
1. Check if someone else added keys recently
2. Pull the latest changes from main
3. Contact maintainers if the issue persists

## Recognition

Translation contributors are recognized in:
- Release notes when translations are added/improved
- The GitHub contributors page
- Special thanks in major releases

## Questions?

- **GitHub Discussions**: [Ask questions](https://github.com/melodee-project/melodee/discussions)
- **Discord**: [Join our community](https://discord.gg/bfMnEUrvbp)
- **Issues**: [Report problems](https://github.com/melodee-project/melodee/issues)

Thank you for helping make Melodee accessible to users worldwide! 🌍🎵

---
description: 'Localization requirements for Melodee.Blazor components and pages - MANDATORY: All language files must be updated together'
applyTo: '**/*.razor, **/*.razor.cs, **/Resources/**/*.resx'
---

# Blazor Localization Guidelines

## Overview

All Melodee.Blazor components and pages must be fully localized. The application supports 10 languages and all user-facing text must use localization keys.

## Supported Languages

| Language | Code | RTL |
|----------|------|-----|
| English (US) | en-US | No |
| German | de-DE | No |
| Spanish | es-ES | No |
| French | fr-FR | No |
| Italian | it-IT | No |
| Japanese | ja-JP | No |
| Portuguese (Brazil) | pt-BR | No |
| Russian | ru-RU | No |
| Chinese (Simplified) | zh-CN | No |
| Arabic | ar-SA | Yes |

## Component Requirements

### Inherit from MelodeeComponentBase

All pages and components that display user-facing text must inherit from `MelodeeComponentBase`:

```razor
@inherits MelodeeComponentBase
```

### Use Localization Helper Methods

`MelodeeComponentBase` provides these helper methods:

```csharp
// Basic localization
L("Key.Name")

// Localization with format arguments
L("Key.WithArgs", arg1, arg2)

// Localization with fallback
L("Key.Name", fallback: "Default text")

// Date formatting (culture-aware)
FormatDate(dateValue)
FormatDate(dateValue, "yyyy-MM-dd")

// Number formatting (culture-aware)
FormatNumber(numericValue)
```

### Examples

```razor
@* Page title *@
<PageTitle>@L("Navigation.Dashboard")</PageTitle>

@* Radzen components *@
<RadzenButton Text="@L("Actions.Save")" />
<RadzenTextBox Placeholder="@L("Forms.SearchPlaceholder")" />
<RadzenDataGridColumn Title="@L("Common.Name")" />

@* Text with arguments *@
<RadzenText>@L("Messages.ItemCount", items.Count)</RadzenText>

@* Conditional text *@
@if (isLoading)
{
    <span>@L("Messages.Loading")</span>
}
```

## Resource Key Naming Convention

Use dot-notation with these prefixes:

| Prefix | Usage | Example |
|--------|-------|---------|
| `Navigation.` | Menu items, breadcrumbs | `Navigation.Dashboard` |
| `Actions.` | Buttons, links | `Actions.Save`, `Actions.Delete` |
| `Common.` | Shared labels | `Common.Name`, `Common.Description` |
| `Messages.` | User messages, notifications | `Messages.SaveSuccess` |
| `Validation.` | Form validation | `Validation.Required` |
| `{PageName}.` | Page-specific text | `Dashboard.RecentlyPlayed` |
| `Dialog.` | Dialog titles/content | `Dialog.ConfirmDelete` |
| `Tooltip.` | Tooltip text | `Tooltip.LockedItem` |

## Adding New Localization Keys

**⚠️ CRITICAL REQUIREMENT**: When adding ANY new localization key, you **MUST** add it to ALL 10 language files in a SINGLE operation. Partial updates will cause CI/CD failures.

### Mandatory Process (NO EXCEPTIONS)

#### Option A: Use the Helper Script (RECOMMENDED)

Use the provided script to add a key to all 10 files automatically:

```bash
bash scripts/add-localization-key.sh "YourKey.Name" "English text here"
```

This will:
- Add the key to all 10 resource files
- Use the English value for en-US
- Add `[NEEDS TRANSLATION]` prefix for other languages
- Prevent you from forgetting any language file

Examples:
```bash
bash scripts/add-localization-key.sh "Actions.Export" "Export"
bash scripts/add-localization-key.sh "Messages.ExportSuccess" "Successfully exported {0} items"
```

#### Option B: Manual Addition

If you must add keys manually:

1. **Add key to base resource file**: `src/Melodee.Blazor/Resources/SharedResources.resx`
   
2. **IMMEDIATELY add the SAME key to ALL 9 translation files** in the same commit:
   - `SharedResources.de-DE.resx` (German)
   - `SharedResources.es-ES.resx` (Spanish)
   - `SharedResources.fr-FR.resx` (French)
   - `SharedResources.it-IT.resx` (Italian)
   - `SharedResources.ja-JP.resx` (Japanese)
   - `SharedResources.pt-BR.resx` (Portuguese - Brazil)
   - `SharedResources.ru-RU.resx` (Russian)
   - `SharedResources.zh-CN.resx` (Chinese - Simplified)
   - `SharedResources.ar-SA.resx` (Arabic - Saudi Arabia)

3. **For translations you don't know**: Use placeholder text with `[NEEDS TRANSLATION]` prefix:
   ```xml
   <data name="YourKey.Name" xml:space="preserve">
     <value>[NEEDS TRANSLATION] English text here</value>
   </data>
   ```

#### Final Steps (Required for Both Options)

1. **MANDATORY validation BEFORE committing**: 
   ```bash
   bash scripts/validate-resources.sh
   ```
   This script MUST pass before you commit. If it fails, you've missed a language file.

2. **Commit all 10 files together**: Never commit the base file without the translation files.

### Why This Matters

- The Localization Validation workflow runs on every push and will FAIL if keys are missing
- Missing keys break the application for users in those languages
- Partial updates require emergency fixes and delay deployment
- All 10 files must have identical key sets (1,274 keys as of 2025-12-30)

### Resource File Format

```xml
<data name="PageName.KeyName" xml:space="preserve">
  <value>Translated text here</value>
</data>
```

## What NOT to Do

```razor
@* BAD: Hardcoded English text *@
<RadzenButton Text="Save" />
<span>Loading...</span>
<RadzenDataGridColumn Title="Name" />

@* BAD: String interpolation without localization *@
<span>Welcome, @userName</span>

@* GOOD: Use localization *@
<RadzenButton Text="@L("Actions.Save")" />
<span>@L("Messages.Loading")</span>
<RadzenDataGridColumn Title="@L("Common.Name")" />
<span>@L("Messages.Welcome", userName)</span>
```

## RTL Support for Arabic

For Arabic (ar-SA), the layout automatically switches to RTL. The `MainLayout` handles this via:
- `dir="@LocalizationService.GetTextDirection()"` attribute
- CSS class `rtl-layout` or `ltr-layout`

No additional work is needed in individual components.

## Testing Localization

1. Run validation script: `bash scripts/validate-resources.sh`
2. Run localization tests: `dotnet test --filter "FullyQualifiedName~Localization"`
3. Manual testing: Switch languages via the LanguageSelector in the header

## Important Notes

- **Never run `dotnet format` on `.resx` files** - it corrupts the XML schema
- The `.editorconfig` has `generated_code = true` for `*.resx` to prevent this
- **All 10 language files must have identical keys** - this is enforced by CI/CD
- Missing keys will display the key name as fallback (e.g., "Dashboard.Title")
- **ALWAYS update all language files together in a single commit**
- Use the validation script before every commit touching resource files

## Pre-Commit Checklist for Resource File Changes

Before committing any changes to resource files, verify:

- [ ] Added/modified key exists in base `SharedResources.resx`
- [ ] Same key exists in all 9 translation files (de-DE, es-ES, fr-FR, it-IT, ja-JP, pt-BR, ru-RU, zh-CN, ar-SA)
- [ ] Used `[NEEDS TRANSLATION]` prefix for placeholder translations
- [ ] Ran `bash scripts/validate-resources.sh` successfully
- [ ] All 10 resource files are staged for commit together
- [ ] Validation script output shows all keys present for all languages

## Common Mistakes to Avoid

❌ **NEVER DO THIS:**
- Adding a key to only the base English file
- Committing base file separately from translation files
- Assuming translations can be "added later"
- Skipping the validation script

✅ **ALWAYS DO THIS:**
- Add keys to all 10 files in one operation
- Run validation script before committing
- Commit all resource files together
- Use placeholders for unknown translations

---
description: 'Localization requirements for Melodee.Blazor components and pages'
applyTo: '**/*.razor, **/*.razor.cs'
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

When adding new UI text:

1. **Add key to base resource file**: `src/Melodee.Blazor/Resources/SharedResources.resx`
2. **Add translations to ALL language files**:
   - `SharedResources.de-DE.resx`
   - `SharedResources.es-ES.resx`
   - `SharedResources.fr-FR.resx`
   - `SharedResources.it-IT.resx`
   - `SharedResources.ja-JP.resx`
   - `SharedResources.pt-BR.resx`
   - `SharedResources.ru-RU.resx`
   - `SharedResources.zh-CN.resx`
   - `SharedResources.ar-SA.resx`

3. **Run validation**: `bash scripts/validate-resources.sh`

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
- All 10 language files must have identical keys
- Missing keys will display the key name as fallback (e.g., "Dashboard.Title")

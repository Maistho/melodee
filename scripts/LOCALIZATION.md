# Localization Scripts

This directory contains scripts to help manage localization for the Melodee.Blazor application.

## Available Scripts

### 🔧 add-localization-key.sh

**Purpose**: Add a new localization key to ALL 10 language files at once.

**Usage**:
```bash
bash scripts/add-localization-key.sh "Key.Name" "English text"
```

**Examples**:
```bash
# Add a simple action
bash scripts/add-localization-key.sh "Actions.Export" "Export"

# Add a message with formatting
bash scripts/add-localization-key.sh "Messages.ExportSuccess" "Successfully exported {0} items"

# Add a validation message
bash scripts/add-localization-key.sh "Validation.EmailRequired" "Email address is required"
```

**What it does**:
- Adds the key to `SharedResources.resx` with the English value
- Adds the key to all 9 translation files with `[NEEDS TRANSLATION]` prefix
- Ensures all files stay synchronized
- Prevents CI/CD failures from missing keys

**Output**:
- ✓ Confirms addition to each language file
- ⚠ Warns if key already exists
- ✗ Shows errors if file structure is invalid

---

### ✅ validate-resources.sh

**Purpose**: Validate that all resource files have consistent keys.

**Usage**:
```bash
bash scripts/validate-resources.sh
```

**What it checks**:
- All 10 language files exist
- All files have the same number of keys
- No keys are missing from any language
- No extra keys exist in translation files

**When to run**:
- **BEFORE every commit** touching resource files
- After adding new keys (manually or via script)
- As part of the CI/CD pipeline (runs automatically)

**Output**:
- Shows key counts for each language
- Lists missing keys (if any)
- Lists extra keys (if any)
- Exit code 0 = success, 1 = validation failed

---

### 🌐 translate_resources.py

**Purpose**: Automatically translate placeholder `[NEEDS TRANSLATION]` entries using AI.

**Usage**:
```bash
python scripts/translate_resources.py
```

**What it does**:
- Finds all entries marked with `[NEEDS TRANSLATION]`
- Uses translation service to generate translations
- Updates resource files with proper translations

**Note**: Requires translation service configuration.

---

### 📝 complete_translations.py

**Purpose**: Check for and complete missing translations.

**Usage**:
```bash
python scripts/complete_translations.py
```

---

## Quick Reference

### Adding a New Localization Key

**Recommended workflow**:
```bash
# 1. Add the key to all files
bash scripts/add-localization-key.sh "YourKey.Name" "Your English text"

# 2. Validate
bash scripts/validate-resources.sh

# 3. Review changes
git diff src/Melodee.Blazor/Resources/

# 4. Commit all 10 files together
git add src/Melodee.Blazor/Resources/*.resx
git commit -m "Add localization key: YourKey.Name"
```

### Supported Languages

| Code | Language | File |
|------|----------|------|
| en-US | English (US) | `SharedResources.resx` |
| de-DE | German | `SharedResources.de-DE.resx` |
| es-ES | Spanish | `SharedResources.es-ES.resx` |
| fr-FR | French | `SharedResources.fr-FR.resx` |
| it-IT | Italian | `SharedResources.it-IT.resx` |
| ja-JP | Japanese | `SharedResources.ja-JP.resx` |
| pt-BR | Portuguese (Brazil) | `SharedResources.pt-BR.resx` |
| ru-RU | Russian | `SharedResources.ru-RU.resx` |
| zh-CN | Chinese (Simplified) | `SharedResources.zh-CN.resx` |
| ar-SA | Arabic (Saudi Arabia) | `SharedResources.ar-SA.resx` |

### Important Rules

⚠️ **ALWAYS**:
- Add keys to ALL 10 files in the same commit
- Run validation before committing
- Use `[NEEDS TRANSLATION]` for unknown translations
- Commit all resource files together

❌ **NEVER**:
- Add a key to only the base English file
- Commit base file separately from translation files
- Skip the validation script
- Run `dotnet format` on `.resx` files

### Troubleshooting

**Validation fails with "missing keys"**:
```bash
# Fix by adding missing keys to all files
bash scripts/add-localization-key.sh "MissingKey.Name" "English value"
bash scripts/validate-resources.sh
```

**CI/CD workflow fails**:
- Check that all 10 files have the same keys
- Run `bash scripts/validate-resources.sh` locally
- Ensure all resource files are committed together

**Key already exists error**:
- The key is already in the file, no action needed
- Check if you need to update the value instead

## See Also

- [Blazor Localization Instructions](../.github/instructions/blazor-localization.instructions.md)
- [Localization Validation Workflow](../.github/workflows/localization.yml)

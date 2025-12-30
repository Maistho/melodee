---
title: CLI - Configuration Commands
permalink: /cli/configuration/
layout: page
---

# Configuration Commands

The `configuration` branch provides commands for viewing and modifying Melodee configuration settings stored in the database.

## Overview

```bash
mcli configuration [COMMAND] [OPTIONS]
```

**Available Commands:**

| Command | Description |
|---------|-------------|
| `list` | List all configuration settings |
| `get` | Get a specific configuration setting value |
| `set` | Modify a configuration setting |

---

## configuration list

Lists all configuration settings stored in the database.

### Usage

```bash
mcli configuration list [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--raw` | | `false` | Output results in JSON format |
| `-c`, `--category` | | | Filter settings by category prefix |
| `--verbose` | | `true` | Output verbose debug and timing results |

### Examples

```bash
# List all settings
./mcli configuration list

# Filter by category
./mcli configuration list --category imaging

# JSON output
./mcli configuration list --raw
```

### Output

```
╭────────────────────────────────────────────┬─────────────────────────┬─────────────────────────────────────────╮
│ Key                                        │ Value                   │ Comment                                 │
├────────────────────────────────────────────┼─────────────────────────┼─────────────────────────────────────────┤
│ imaging.smallSize                          │ 160                     │ Small image dimension in pixels         │
│ imaging.mediumSize                         │ 320                     │ Medium image dimension in pixels        │
│ imaging.largeSize                          │ 640                     │ Large image dimension in pixels         │
│ imaging.minimumImageSize                   │ 300                     │ Minimum accepted image size             │
│ jobs.libraryProcess.cronExpression         │ 0 */5 * * * ?           │ Cron expression for library processing  │
│ processing.doDeleteOriginalAfterConversion │ true                    │ Delete original files after conversion  │
╰────────────────────────────────────────────┴─────────────────────────┴─────────────────────────────────────────╯

Total: 156 settings
```

### Configuration Categories

Settings are organized by category prefix:

| Category | Description |
|----------|-------------|
| `imaging.*` | Image processing and sizing |
| `jobs.*` | Background job scheduling |
| `processing.*` | Media processing options |
| `conversion.*` | Audio conversion settings |
| `scripting.*` | Pre/post processing scripts |
| `validation.*` | Validation rules |

---

## configuration get

Get the value of a specific configuration setting.

### Usage

```bash
mcli configuration get <KEY> [OPTIONS]
```

### Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `KEY` | Yes | The configuration key to retrieve |

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--raw` | | `false` | Output only the value (no formatting) |
| `--verbose` | | `true` | Output verbose debug and timing results |

### Examples

```bash
# Get a specific setting
./mcli configuration get imaging.smallSize

# Get raw value for scripting
./mcli configuration get imaging.smallSize --raw

# Use in shell scripts
SMALL_SIZE=$(./mcli configuration get imaging.smallSize --raw)
echo "Small image size: $SMALL_SIZE"
```

### Output

```
Key: imaging.smallSize
Value: 160
Comment: Small image dimension in pixels
```

### Raw Output

```
160
```

---

## configuration set

Modify a configuration setting value.

### Usage

```bash
mcli configuration set <KEY> <VALUE> [OPTIONS]
```

### Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `KEY` | Yes | The configuration key to modify |
| `VALUE` | Yes | The new value to set |

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--verbose` | | `true` | Output verbose debug and timing results |

### Examples

```bash
# Set image size
./mcli configuration set imaging.smallSize 200

# Set cron expression (quote special characters)
./mcli configuration set jobs.libraryProcess.cronExpression "0 */10 * * * ?"

# Set boolean value
./mcli configuration set processing.doDeleteOriginalAfterConversion false
```

### Output

```
✓ Setting updated successfully

Key: imaging.smallSize
Old Value: 160
New Value: 200
```

### Value Types

Configuration values are stored as strings but interpreted based on context:

| Type | Examples |
|------|----------|
| Integer | `160`, `1024`, `0` |
| Boolean | `true`, `false` |
| String | `"value"`, `path/to/file` |
| Cron Expression | `0 */5 * * * ?` |

### Common Settings

**Image Processing:**

```bash
# Set thumbnail sizes
./mcli configuration set imaging.smallSize 160
./mcli configuration set imaging.mediumSize 320
./mcli configuration set imaging.largeSize 640

# Set minimum image size requirement
./mcli configuration set imaging.minimumImageSize 300
```

**Job Scheduling:**

```bash
# Process library every 5 minutes
./mcli configuration set jobs.libraryProcess.cronExpression "0 */5 * * * ?"

# Run inbound processing hourly
./mcli configuration set jobs.inboundProcess.cronExpression "0 0 * * * ?"

# Disable a job (empty cron)
./mcli configuration set jobs.someJob.cronExpression ""
```

**Processing Options:**

```bash
# Keep original files after conversion
./mcli configuration set processing.doDeleteOriginalAfterConversion false

# Set maximum parallel processing
./mcli configuration set processing.maxParallelProcesses 4
```

---

## Configuration Management Best Practices

### Backup Before Changes

```bash
# Export current configuration
./mcli configuration list --raw > config-backup-$(date +%Y%m%d).json
```

### Environment-Specific Settings

Use different configuration for development and production:

```bash
# Development - faster processing, smaller images
MELODEE_APPSETTINGS_PATH="/etc/melodee/appsettings.Development.json" \
  ./mcli configuration set imaging.smallSize 100

# Production - full quality
MELODEE_APPSETTINGS_PATH="/etc/melodee/appsettings.json" \
  ./mcli configuration set imaging.smallSize 160
```

### Scripted Configuration

```bash
#!/bin/bash
# Apply standard configuration settings

settings=(
  "imaging.smallSize:160"
  "imaging.mediumSize:320"
  "imaging.largeSize:640"
  "processing.doDeleteOriginalAfterConversion:true"
)

for setting in "${settings[@]}"; do
  key="${setting%%:*}"
  value="${setting#*:}"
  ./mcli configuration set "$key" "$value"
done

echo "Configuration applied successfully"
```

---

## Troubleshooting

### Setting Not Found

```
Error: Setting 'invalid.key' not found
```

**Solution:** Use `configuration list` to see available settings:

```bash
./mcli configuration list | grep -i "keyword"
```

### Value Type Mismatch

```
Error: Cannot convert 'abc' to integer for setting 'imaging.smallSize'
```

**Solution:** Ensure the value matches the expected type. Check the current value for guidance:

```bash
./mcli configuration get imaging.smallSize
```

### Changes Not Taking Effect

Configuration changes require the Melodee service to reload. Either:

1. Restart the Melodee service
2. Wait for automatic configuration refresh (if enabled)
3. Trigger a configuration reload through the web interface

---

## See Also

- [Configuration Reference](/configuration-reference/) - Complete list of all settings
- [CLI Overview](/cli/) - Main CLI documentation
- [Jobs](/jobs/) - Background job configuration

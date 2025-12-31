---
title: Command Line Interface (CLI)
permalink: /cli/
---

# Command Line Interface (CLI)

The Melodee CLI (`mcli`) is a powerful command-line utility for managing music libraries, processing media files, and performing maintenance tasks. It provides direct access to Melodee's core functionality without requiring the web interface.

## Getting Started

### Installation

The CLI is included with Melodee and is built as a standalone executable:

```bash
# Build from source
dotnet build src/Melodee.Cli/Melodee.Cli.csproj

# Run from debug folder
cd src/Melodee.Cli/bin/Debug/net10.0
./mcli --help
```

### Configuration

The CLI uses the same configuration as the web application. You can specify a custom configuration file using an environment variable:

```bash
# Point to a specific appsettings.json file
export MELODEE_APPSETTINGS_PATH="/path/to/appsettings.json"

# Or inline
MELODEE_APPSETTINGS_PATH="/path/to/appsettings.json" ./mcli library list
```

**Environment Variables:**

| Variable | Description | Example |
|----------|-------------|---------|
| `MELODEE_APPSETTINGS_PATH` | Path to custom appsettings.json | `/etc/melodee/appsettings.json` |
| `ASPNETCORE_ENVIRONMENT` | Environment name | `Development`, `Production` |

## Command Structure

The CLI uses a hierarchical command structure:

```
mcli [BRANCH] [COMMAND] [OPTIONS]
```

## Available Commands

| Command | Description | Documentation |
|---------|-------------|---------------|
| [album](/cli/album/) | Album data management and statistics | [View Details](/cli/album/) |
| [artist](/cli/artist/) | Artist data management and statistics | [View Details](/cli/artist/) |
| [configuration](/cli/configuration/) | Manage Melodee configuration settings | [View Details](/cli/configuration/) |
| [doctor](/cli/doctor/) | Run environment and configuration diagnostics | [View Details](/cli/doctor/) |
| [file](/cli/file/) | File analysis and inspection tools | [View Details](/cli/file/) |
| [import](/cli/import/) | Import data from external sources | [View Details](/cli/import/) |
| [job](/cli/job/) | Run background jobs and maintenance tasks | [View Details](/cli/job/) |
| [library](/cli/library/) | Library management and operations | [View Details](/cli/library/) |
| [parser](/cli/parser/) | Parse and analyze media metadata files | [View Details](/cli/parser/) |
| [tags](/cli/tags/) | Display and manage media file tags | [View Details](/cli/tags/) |
| [validate](/cli/validate/) | Validate media files and metadata | [View Details](/cli/validate/) |

## Quick Examples

```bash
# List all libraries
./mcli library list

# Full scan workflow - process inbound → staging → storage → database
./mcli library scan

# Silent scan for cron jobs (no output, exit code only)
./mcli library scan --silent

# JSON scan output for scripting/automation
./mcli library scan --json

# Show album statistics
./mcli album stats

# Search for albums
./mcli album search "Beatles"

# Find albums added in the last 7 days
./mcli album search --since 7

# Search and sort by year (newest first)
./mcli album search --sort Year --sort-dir desc

# Bulk delete albums added in the last 5 days (with confirmation)
./mcli album search --since 5 --delete

# Search for an artist
./mcli artist search "Beatles"

# Find artists added in the last 30 days, sorted by album count
./mcli artist search --since 30 --sort Albums --sort-dir desc

# Find duplicate artists with high confidence
./mcli artist find-duplicates -m 0.9

# Find and merge duplicate artists
./mcli artist find-duplicates -m 0.95 --merge

# List background jobs with next run times
./mcli job list

# Run a specific job
./mcli job run -j ArtistHousekeepingJob

# Get a configuration value
./mcli configuration get imaging.smallSize
```

## Common Options

These options are available across most commands:

| Option | Description |
|--------|-------------|
| `-h`, `--help` | Show help information for the command |
| `--json` | Output results as JSON (structured output for automation) |
| `--raw` | Output in JSON format for scripting |
| `--silent` | Suppress all output (for cron jobs, returns exit code only) |
| `--verbose` | Output verbose debug and timing information (default: false) |

## Exit Codes

The CLI uses standard exit codes:

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | Error or failure |

These can be used in scripts for error handling:

```bash
#!/bin/bash
./mcli library stats --library "Storage"
if [ $? -eq 0 ]; then
    echo "Success!"
else
    echo "Failed!"
fi
```

## Troubleshooting

### "Invalid library Name" Error

**Problem:** The CLI can't find the library in the database.

**Solution:**
1. Check that you're using the correct configuration file:
   ```bash
   MELODEE_APPSETTINGS_PATH="/path/to/appsettings.json" ./mcli library list
   ```
2. Verify the library exists:
   ```bash
   ./mcli library list
   ```
3. Check connection string in `appsettings.json`

### Configuration Not Found

**Problem:** The CLI can't find `appsettings.json`.

**Solution:**
Use the `MELODEE_APPSETTINGS_PATH` environment variable:
```bash
export MELODEE_APPSETTINGS_PATH="/full/path/to/appsettings.json"
```

## See Also

- [Background Jobs](/jobs/) - Automated job scheduling
- [Libraries](/libraries/) - Library concepts and management
- [Configuration Reference](/configuration-reference/) - All configuration settings
- [API Reference](/api/) - REST API documentation

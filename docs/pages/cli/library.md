---
title: CLI - Library Commands
permalink: /cli/library/
layout: page
---

# Library Commands

The `library` branch provides commands for managing music libraries, processing media, and maintaining metadata.

## Overview

```bash
mcli library [COMMAND] [OPTIONS]
```

**Available Commands:**

| Command | Alias | Description |
|---------|-------|-------------|
| `list` | `ls` | List all libraries with their details |
| `stats` | `ss` | Show statistics for a specific library |
| `album-report` | `ar` | Show report of albums found in library |
| `clean` | `c` | Clean library and delete folders without media files |
| `rebuild` | `r` | Rebuild melodee metadata albums in library |
| `scan` | `s` | Scan libraries for database updates |
| `process` | `p` | Process media from inbound to staging |
| `move-ok` | `m` | Move 'Ok' status albums to another library |
| `purge` | | Purge library data from database |

---

## library list

Lists all libraries configured in the database with their details.

### Usage

```bash
mcli library list [OPTIONS]
```

### Options

| Option | Default | Description |
|--------|---------|-------------|
| `--raw` | `false` | Output in JSON format for scripting |
| `--verbose` | `true` | Include verbose debug output |

### Examples

```bash
# List all libraries
./mcli library list

# JSON output for scripting
./mcli library list --raw
```

### Output

```
╭──────────┬─────────┬──────────────────────────────┬─────────┬────────┬───────┬──────────────────┬───────────╮
│ Name     │ Type    │ Path                         │ Artists │ Albums │ Songs │    Last Scan     │   Status  │
├──────────┼─────────┼──────────────────────────────┼─────────┼────────┼───────┼──────────────────┼───────────┤
│ Inbound  │ Inbound │ /mnt/music/inbound           │       0 │      0 │     0 │       N/A        │    ✓ OK   │
│ Staging  │ Staging │ /mnt/music/staging           │      42 │    156 │  1892 │ 2024-12-30 13:00 │ ⚠ Needs   │
│ Storage  │ Storage │ /mnt/music/library           │   1,234 │ 15,678 │187234 │ 2024-12-30 13:05 │    ✓ OK   │
╰──────────┴─────────┴──────────────────────────────┴─────────┴────────┴───────┴──────────────────┴───────────╯

Total libraries: 3
⚠ 1 library(ies) need scanning
```

### Library Types

| Type | Description | Scanning |
|------|-------------|----------|
| Inbound | New files to be processed | N/A |
| Staging | Processed files awaiting review | Yes |
| Storage | Final destination for approved albums | Yes |
| UserImages | User-uploaded images | N/A |
| Playlist | Playlist files | N/A |

---

## library stats

Shows detailed statistics for a specific library.

### Usage

```bash
mcli library stats --library <NAME> [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--library` | `-l` | **Required** | Name of the library to analyze |
| `--borked` | | `false` | Show only issues, skip informational stats |
| `--raw` | | `false` | Output in JSON format |
| `--verbose` | | `true` | Include verbose debug output |

### Examples

```bash
# Show all statistics
./mcli library stats --library "Storage"

# Show only problems
./mcli library stats -l "Staging" --borked

# JSON output
./mcli library stats -l "Storage" --raw
```

### Output

```
╭─────────────────────────────────────────────╮
│ Library: Storage                            │
│ Path: /mnt/music/library                    │
│ Type: Storage                               │
╰─────────────────────────────────────────────╯

                Artist Statistics                 
╭─────────────────────┬───────────┬───────╮
│ Metric              │     Count │     % │
├─────────────────────┼───────────┼───────┤
│ Total Artists       │     1,234 │  100% │
│ With Images         │     1,200 │ 97.2% │
│ Locked              │        12 │  1.0% │
╰─────────────────────┴───────────┴───────╯

                Album Statistics                  
╭─────────────────────┬───────────┬───────╮
│ Metric              │     Count │     % │
├─────────────────────┼───────────┼───────┤
│ Total Albums        │    15,678 │  100% │
│ Ok Status           │    15,500 │ 98.9% │
│ Needs Attention     │       120 │  0.8% │
│ Missing Images      │        58 │  0.4% │
╰─────────────────────┴───────────┴───────╯
```

---

## library album-report

Generates a report of albums grouped by status.

### Usage

```bash
mcli library album-report --library <NAME> [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--library` | `-l` | **Required** | Name of the library |
| `--full` | | `false` | Show detailed list instead of summary |
| `--raw` | | `false` | Output in JSON format |
| `--verbose` | | `true` | Include verbose debug output |

### Examples

```bash
# Summary report
./mcli library album-report --library "Staging"

# Full detailed report
./mcli library album-report -l "Staging" --full
```

### Summary Output

```
╭────────────────────┬───────╮
│ Status             │ Count │
├────────────────────┼───────┤
│ ✓ Ok               │ 1,204 │
│ ⚠ Ok (Invalid)     │     3 │
│ ✗ HasNoImages      │    15 │
│ ✗ HasNoTracks      │     2 │
╰────────────────────┴───────╯

Use --full to see detailed list.
```

---

## library clean

⚠️ **DESTRUCTIVE OPERATION**

Removes directories without media files from a library.

### Usage

```bash
mcli library clean --library <NAME> [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--library` | `-l` | **Required** | Name of the library to clean |
| `--verbose` | | `true` | Include verbose debug output |

### What It Does

1. Scans library for all directories
2. Identifies directories without media files
3. Preserves image-only directories if parent has media
4. **Permanently deletes** empty/non-media directories

### Safety

- ❌ Won't run on locked libraries
- ✅ Safe for Staging and Storage libraries
- ⚠️ Use caution with Inbound libraries

### Example

```bash
./mcli library clean --library "Staging"
```

---

## library rebuild

Regenerates Melodee metadata files by reading music files.

### Usage

```bash
mcli library rebuild --library <NAME> [PATH] [OPTIONS]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `[PATH]` | Optional. Rebuild only this specific album path |

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--library` | `-l` | **Required** | Name of the library |
| `--only-missing` | | `true` | Only create missing metadata files |
| `--verbose` | | `true` | Include verbose debug output |

### Examples

```bash
# Create only missing metadata
./mcli library rebuild --library "Storage"

# Full rebuild (recreate all)
./mcli library rebuild -l "Storage" --only-missing false

# Rebuild specific album
./mcli library rebuild -l "Storage" "The Beatles/Abbey Road"
```

### Safety

- ✅ Non-destructive: Only creates/updates metadata files
- ❌ Won't run on Inbound or locked libraries

---

## library scan

Updates database from library filesystem.

### Usage

```bash
mcli library scan --library <NAME> [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--library` | `-l` | **Required** | Name of the library to scan |
| `--force` | | `false` | Force scan even if recently scanned |
| `--verbose` | | `true` | Include verbose debug output |

### What It Does

1. Reads `melodee.json` files from library
2. Updates database with album, artist, song information
3. Processes artwork and metadata
4. Records scan history

### Example

```bash
# Scan if changed
./mcli library scan --library "Storage"

# Force full scan
./mcli library scan -l "Storage" --force
```

---

## library process

Processes media from Inbound to Staging library.

### Usage

```bash
mcli library process --library <NAME> [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--library` | `-l` | **Required** | Name of the inbound library |
| `--copy` | | `true` | Copy files instead of moving |
| `--force` | | `true` | Override existing metadata files |
| `--limit` | | | Maximum albums to process |
| `--pre-script` | | | Script to run before processing |
| `--inbound` | | | Inbound path (path-based mode) |
| `--staging` | | | Staging path (path-based mode) |
| `--verbose` | | `true` | Include verbose debug output |

### What It Does

1. Scans inbound library for media files
2. Parses ID3/Vorbis tags
3. Extracts album artwork
4. Creates `melodee.json` metadata files
5. Moves/copies to staging

### Examples

```bash
# Process all from Inbound
./mcli library process --library "Inbound"

# Process 10 albums
./mcli library process -l "Inbound" --limit 10

# Path-based mode
./mcli library process --inbound "/mnt/incoming" --staging "/mnt/staging"
```

---

## library move-ok

Moves albums with "Ok" status between libraries.

### Usage

```bash
mcli library move-ok --library <FROM> --to-library <TO> [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--library` | `-l` | **Required** | Source library name |
| `--to-library` | | **Required** | Destination library name |
| `--from-path` | | | Source path (path-based mode) |
| `--to-path` | | | Destination path (path-based mode) |
| `--verbose` | | `true` | Include verbose debug output |

### Examples

```bash
# Move from Staging to Storage
./mcli library move-ok --library "Staging" --to-library "Storage"

# Path-based mode
./mcli library move-ok --from-path "/mnt/staging" --to-path "/mnt/library"
```

---

## library purge

⚠️ **DESTRUCTIVE OPERATION**

Purges all data for a library from the database.

### Usage

```bash
mcli library purge --library <NAME> [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--library` | `-l` | **Required** | Name of the library to purge |
| `--force` | | `false` | Ignore last scan timestamp |
| `--verbose` | | `true` | Include verbose debug output |

### What Gets Deleted

- All artist records for this library
- All album records
- All song records
- Library statistics

### What Is NOT Deleted

- Filesystem files (only database records)
- The library configuration itself

### Safety

- ❌ Won't run on locked libraries
- Confirmation required

### Example

```bash
./mcli library purge --library "Staging"
```

---

## Workflow Examples

### Daily Processing Pipeline

```bash
#!/bin/bash
# Complete processing pipeline

# 1. Process new inbound files
./mcli library process -l "Inbound"

# 2. Move approved albums to storage
./mcli library move-ok -l "Staging" --to-library "Storage"

# 3. Scan storage for updates
./mcli library scan -l "Storage"

# 4. Clean up staging
./mcli library clean -l "Staging"
```

### Full Library Rebuild

```bash
#!/bin/bash
# Complete library rebuild

LIBRARY="Storage"

echo "Starting full rebuild of $LIBRARY..."

# Clean first
./mcli library clean -l "$LIBRARY"

# Full metadata rebuild
./mcli library rebuild -l "$LIBRARY" --only-missing false

# Rescan into database
./mcli library scan -l "$LIBRARY" --force

echo "Rebuild complete"
```

---

## See Also

- [Libraries](/libraries/) - Library concepts
- [Album Commands](/cli/album/) - Album operations
- [CLI Overview](/cli/) - Main CLI documentation

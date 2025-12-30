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
| `scan` | `s` | **Full scan workflow** - process inbound → staging → storage → database |
| `process` | `p` | Process media from inbound to staging (step 1 only) |
| `move-ok` | `m` | Move 'Ok' status albums to another library |
| `purge` | | Purge library data from database |
| `validate` | `v` | Validate library integrity (DB vs disk consistency) |

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
| `--verbose` | `false` | Include verbose debug output |

### Examples

```bash
# List all libraries
./mcli library list

# JSON output for scripting
./mcli library list --raw
```

### Output

```
╭──────────┬─────────┬──────────────────────────────┬─────────┬────────┬───────┬─────────────────┬───────────╮
│ Name     │ Type    │ Path                         │ Artists │ Albums │ Songs │    Last Scan    │   Status  │
├──────────┼─────────┼──────────────────────────────┼─────────┼────────┼───────┼─────────────────┼───────────┤
│ Inbound  │ Inbound │ /mnt/music/inbound           │       0 │      0 │     0 │       N/A       │    ✓ OK   │
│ Staging  │ Staging │ /mnt/music/staging           │      42 │    156 │  1892 │ 20241230T130000 │ ⚠ Needs   │
│ Storage  │ Storage │ /mnt/music/library           │   1,234 │ 15,678 │187234 │ 20241230T130500 │    ✓ OK   │
╰──────────┴─────────┴──────────────────────────────┴─────────┴────────┴───────┴─────────────────┴───────────╯

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
| `--verbose` | | `false` | Include verbose debug output |

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
| `--verbose` | | `false` | Include verbose debug output |

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
| `--verbose` | | `false` | Include verbose debug output |

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
| `--verbose` | | `false` | Include verbose debug output |

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

Performs a **full library scan workflow** - the complete media ingestion pipeline.

### Usage

```bash
mcli library scan [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--force` | | `false` | Force processing even if recently scanned |
| `--verbose` | | `false` | Include verbose debug output |
| `--silent` | | `false` | Suppress all output (silent mode) |
| `--json` | | `false` | Output results as JSON (implies --silent for progress) |

### What It Does

This command orchestrates the **entire media ingestion pipeline** in sequence:

| Step | Job | Description |
|------|-----|-------------|
| 1 | LibraryInboundProcessJob | Processes raw files from inbound → staging |
| 2 | StagingAlbumRevalidationJob | Re-validates albums with invalid artists |
| 3 | StagingAutoMoveJob | Moves approved albums from staging → storage |
| 4 | LibraryInsertJob | Inserts albums from storage into database |

This is the **recommended way to add new music** - a single command that handles everything.

### Examples

```bash
# Standard scan - process everything end-to-end
./mcli library scan

# Force full reprocessing
./mcli library scan --force

# Silent mode for cron jobs (no output, exit code only)
./mcli library scan --silent

# JSON output for scripting and automation
./mcli library scan --json

# Check exit code in scripts
./mcli library scan --silent && echo "Scan successful" || echo "Scan failed"
```

### JSON Output Example

When using `--json`, the command outputs structured JSON:

```json
{
  "success": true,
  "durationSeconds": 145.23,
  "duration": "00:02:25",
  "steps": [
    { "name": "Processing inbound files", "success": true, "durationSeconds": 83.5 },
    { "name": "Revalidating staging albums", "success": true, "durationSeconds": 5.1 },
    { "name": "Moving approved albums to storage", "success": true, "durationSeconds": 12.3 },
    { "name": "Inserting albums into database", "success": true, "durationSeconds": 44.3 }
  ],
  "summary": {
    "inboundProcessing": { "newArtists": 5, "newAlbums": 12, "newSongs": 145 },
    "stagingRevalidation": { "albumsRevalidated": 3, "albumsNowValid": 2 },
    "storageTransfer": { "albumsMoved": 14 },
    "databaseInsert": { "artistsInserted": 5, "albumsInserted": 14, "songsInserted": 168 }
  },
  "errors": []
}
```

### Interactive Output

```
╭─────────────────────────────────────╮
│     Library Scan Configuration      │
├─────────────────────────────────────┤
│ Force Mode    No                    │
│ Verbose       No                    │
╰─────────────────────────────────────╯

✓ Processing inbound files           (01:23)
✓ Revalidating staging albums        (00:05)
✓ Moving approved albums to storage  (00:12)
✓ Inserting albums into database     (00:45)

── Library scan completed in 00:02:25 ──

╭─────────────────────────────────────╮
│           Scan Summary              │
├─────────────────────────────────────┤
│ Inbound Processing                  │
│   New artists discovered        5   │
│   New albums discovered        12   │
│   New songs discovered        145   │
├─────────────────────────────────────┤
│ Database Insert                     │
│   Artists inserted              5   │
│   Albums inserted              14   │
│   Songs inserted              168   │
╰─────────────────────────────────────╯
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
| `--verbose` | | `false` | Include verbose debug output |

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
| `--verbose` | | `false` | Include verbose debug output |

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
| `--verbose` | | `false` | Include verbose debug output |

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

## library validate

Validates library integrity by checking bidirectional consistency between database records and files on disk.

### Usage

```bash
mcli library validate --library <NAME> [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--library` | `-l` | **Required** | Name of the storage library to validate |
| `--fix` | | `false` | Remove orphaned database records |
| `--json` | | `false` | Output results as JSON |
| `--verbose` | | `false` | Include verbose debug output |

### What It Validates

1. **Database → Disk**: Checks that all artists, albums, and songs in the database have corresponding files/directories on disk
2. **Disk → Database**: Checks that all album directories with media files on disk are registered in the database

### Issues Detected

| Issue Type | Description | Fix Option |
|------------|-------------|------------|
| Orphaned Artists | Artist in DB but directory missing on disk | `--fix` removes from DB |
| Orphaned Albums | Album in DB but directory missing on disk | `--fix` removes from DB |
| Missing Songs | Song in DB but file missing on disk | `--fix` removes from DB |
| Unregistered Directories | Album directory on disk not in DB | Run `library scan` to add |

### Examples

```bash
# Basic validation - check for issues
./mcli library validate --library "Storage"

# JSON output for scripting
./mcli library validate -l "Storage" --json

# Auto-fix orphaned DB records
./mcli library validate -l "Storage" --fix

# Pipe JSON to jq for processing
./mcli library validate -l "Storage" --json | jq '.issues.orphanedAlbums | length'
```

### Example Output

```
╭───────────────────────────────────╮
│     Library Validation            │
├───────────────────────────────────┤
│ Library     Storage               │
│ Path        /mnt/music/library    │
│ Fix Mode    No                    │
╰───────────────────────────────────╯

✓ Validating database records against disk
✓ Validating disk directories against database

── Validation completed in 00:02.345 ──

╭─────────────────┬─────────┬────────╮
│ Category        │ Checked │ Issues │
├─────────────────┼─────────┼────────┤
│ Artists         │   1,234 │      0 │
│ Albums          │  15,678 │      3 │
│ Songs           │ 187,234 │     12 │
│ Disk Dirs       │  15,700 │      5 │
╰─────────────────┴─────────┴────────╯

✗ Library validation failed - 20 issue(s) found.
Tip: Use --fix to remove orphaned database records.
Tip: Run 'mcli library scan' to add unregistered directories.
```

### JSON Output Example

```json
{
  "success": false,
  "libraryName": "Storage",
  "libraryPath": "/mnt/music/library",
  "durationSeconds": 2.345,
  "summary": {
    "artistsChecked": 1234,
    "albumsChecked": 15678,
    "songsChecked": 187234,
    "directoriesScanned": 15700
  },
  "issues": {
    "orphanedArtists": [],
    "orphanedAlbums": [
      { "id": 123, "name": "Missing Album", "directory": "M/Mi/Missing Artist/Missing Album/" }
    ],
    "missingSongs": [
      { "id": 456, "title": "Lost Track", "fileName": "01 - Lost Track.mp3", "albumName": "Some Album" }
    ],
    "unregisteredDirectories": [
      "/mnt/music/library/N/Ne/New Artist/New Album"
    ]
  },
  "fixed_": null
}
```

### Safety

- ✅ Read-only by default (only reports issues)
- ⚠️ `--fix` modifies database (removes orphaned records)
- ❌ Only works with Storage libraries

---

## Workflow Examples

### Add New Music (Recommended)

```bash
# Single command to process all new music end-to-end
./mcli library scan
```

This is the simplest and recommended approach. It handles:
- Processing files from inbound
- Validating and revalidating albums
- Moving approved albums to storage
- Adding them to the database

### Force Full Reprocessing

```bash
# Force reprocess everything, ignoring timestamps
./mcli library scan --force
```

### Full Library Rebuild

```bash
#!/bin/bash
# Complete library rebuild (metadata regeneration)

LIBRARY="Storage"

echo "Starting full rebuild of $LIBRARY..."

# Clean first
./mcli library clean -l "$LIBRARY"

# Full metadata rebuild
./mcli library rebuild -l "$LIBRARY" --only-missing false

# Rescan into database
./mcli library scan --force

echo "Rebuild complete"
```

### Validate and Fix Library Integrity

```bash
#!/bin/bash
# Check and fix library integrity issues

LIBRARY="Storage"

echo "Validating library integrity..."

# First, check for issues
./mcli library validate -l "$LIBRARY"

# If issues found, fix orphaned DB records
./mcli library validate -l "$LIBRARY" --fix

# Add any unregistered directories to DB
./mcli library scan

echo "Library validation complete"
```

---

## See Also

- [Libraries](/libraries/) - Library concepts
- [Album Commands](/cli/album/) - Album operations
- [CLI Overview](/cli/) - Main CLI documentation

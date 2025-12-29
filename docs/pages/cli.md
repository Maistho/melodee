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

**Available Branches:**

- `library` - Library management and operations
- `configuration` - Manage Melodee configuration settings
- `file` - File analysis and inspection tools
- `import` - Import data from external sources
- `job` - Run background jobs and maintenance tasks
- `parser` - Parse and analyze media metadata files
- `validate` - Validate media files and metadata
- `tags` - Display and manage media file tags

## Quick Examples

```bash
# List all libraries
./mcli library list

# Show library statistics
./mcli library stats --library "Storage"

# Generate album status report
./mcli library album-report -l "Staging" --full

# Rebuild metadata files
./mcli library rebuild --library "Storage"

# Clean library
./mcli library clean -l "Staging"
```

---

## Library Commands

Manage music libraries, process media, and maintain metadata.

### `library list`

Lists all libraries configured in the database with their details.

**Usage:**
```bash
mcli library list [OPTIONS]
```

**Options:**

| Option | Alias | Description |
|--------|-------|-------------|
| `--raw` | | Output in raw tab-separated format for scripting |
| `--help` | `-h` | Show help information |

**Example:**
```bash
# List all libraries with details
./mcli library list

# Raw output for scripting
./mcli library list --raw
```

**Output:**
```
╭──────────────────────────────────────────────────────────────────────╮
│ Name    │ Type    │ Path                              │ Artists │ ... │
├──────────────────────────────────────────────────────────────────────┤
│ Inbound │ Inbound │ /mnt/music/inbound                │ 0       │ ... │
│ Staging │ Staging │ /mnt/music/staging                │ 42      │ ... │
│ Storage │ Storage │ /mnt/music/library                │ 1,234   │ ... │
╰──────────────────────────────────────────────────────────────────────╯

Total libraries: 3
⚠ 1 library(ies) need scanning
```

---

### `library stats`

Shows detailed statistics for a specific library, grouped by category.

**Usage:**
```bash
mcli library stats --library <NAME> [OPTIONS]
```

**Options:**

| Option | Alias | Description |
|--------|-------|-------------|
| `--library` | `-l` | **Required.** Name of the library to analyze |
| `--borked` | | Show only issues (warnings/errors), skip informational stats |
| `--raw` | | Output in raw tab-separated format |
| `--verbose` | | Include verbose debug output (default: true) |

**Example:**
```bash
# Show all statistics
./mcli library stats --library "Storage"

# Show only problems
./mcli library stats -l "Staging" --borked

# Raw output for scripting
./mcli library stats -l "Storage" --raw
```

**Output:**

Displays library information header, followed by categorized statistics panels for Artists, Albums, Songs, and additional information. Issues and warnings are highlighted in a separate section.

---

### `library album-report`

Generates a report of albums found in the library, grouped by status (e.g., "Ok", "HasNoImages", "HasNoTracks").

**Usage:**
```bash
mcli library album-report --library <NAME> [OPTIONS]
```

**Options:**

| Option | Alias | Description |
|--------|-------|-------------|
| `--library` | `-l` | **Required.** Name of the library to report on |
| `--full` | | Show full detailed report instead of summary |
| `--raw` | | Output in raw tab-separated format |
| `--verbose` | | Include verbose debug output (default: true) |

**Example:**
```bash
# Summary report (default)
./mcli library album-report --library "Staging"

# Full detailed report
./mcli library album-report -l "Staging" --full

# Raw output
./mcli library album-report -l "Staging" --raw
```

**Summary Output:**
```
╭────────────────────╮
│ Status │ Count     │
├────────────────────┤
│ ✓ Ok   │ 1,204     │
│ ⚠ Ok (Invalid) │ 3 │
│ ✗ HasNoImages │ 15 │
│ ✗ HasNoTracks │ 2  │
╰────────────────────╯

Use --full to see detailed list.
```

**Full Output:**

Displays a detailed table with all albums, their status, and specific issues.

---

### `library clean`

⚠️ **DESTRUCTIVE OPERATION**

Removes directories from a library that don't contain media files, helping to clean up empty or useless folders.

**Usage:**
```bash
mcli library clean --library <NAME> [OPTIONS]
```

**Options:**

| Option | Alias | Description |
|--------|-------|-------------|
| `--library` | `-l` | **Required.** Name of the library to clean |
| `--verbose` | | Include verbose debug output (default: true) |

**What It Does:**

1. Scans the library at the top level for all directories
2. Identifies directories without media files (music/audio)
3. Preserves image-only directories if the parent has media files
4. Permanently deletes empty/non-media directories
5. Reports progress with real-time updates

**Safety:**

- ❌ Won't run on locked libraries
- ✅ Safe for Staging and Storage libraries
- ⚠️ Use caution with Inbound libraries (incomplete uploads)

**Example:**
```bash
./mcli library clean --library "Staging"
```

**Output:**
```
╭─────────────────────────────────────╮
│ Library Clean Configuration         │
├─────────────────────────────────────┤
│ Library: Staging                    │
╰─────────────────────────────────────╯

✓ Completed: Library cleaned | Time: 00:02:15

Clean operation completed successfully
```

---

### `library rebuild`

Regenerates Melodee metadata files (`melodee.json`) for albums by reading and analyzing the actual music files in place. This command does **not** modify your music files.

**Usage:**
```bash
mcli library rebuild --library <NAME> [PATH] [OPTIONS]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `[PATH]` | Optional. Rebuild only this specific album path (e.g., "Artist/Album") |

**Options:**

| Option | Alias | Description |
|--------|-------|-------------|
| `--library` | `-l` | **Required.** Name of the library to rebuild |
| `--only-missing` | | Only create missing metadata files (default: true) |
| `--verbose` | | Include verbose debug output (default: true) |

**Modes:**

**Create Only Missing** (default):
- Only creates `melodee.json` files that don't exist
- Skips directories that already have metadata
- Fast, incremental updates

**Full Rebuild** (`--only-missing false`):
- Cleans the library first (removes directories without media)
- Recreates ALL `melodee.json` files from scratch
- Use when metadata is corrupted or needs a fresh start

**What It Does:**

1. Scans the library recursively for directories with music files
2. Reads metadata from music file tags (ID3, Vorbis, etc.)
3. Creates/recreates `melodee.json` files with standardized metadata
4. Shows progress with real-time updates

**Safety:**

- ✅ Non-destructive: Only creates/updates metadata files
- ✅ Safe for Staging and Storage libraries
- ❌ Won't run on Inbound libraries (files must be processed first)
- ❌ Won't run on locked libraries

**Examples:**
```bash
# Create only missing metadata files (default)
./mcli library rebuild --library "Storage"

# Full rebuild: recreate all metadata files
./mcli library rebuild -l "Storage" --only-missing false

# Rebuild a specific album
./mcli library rebuild -l "Storage" "The Beatles/Abbey Road"
```

**Output:**
```
╭─────────────────────────────────────╮
│ Library Rebuild Configuration       │
├─────────────────────────────────────┤
│ Library: Storage                    │
│ Mode: Create Only Missing           │
╰─────────────────────────────────────╯

✓ Completed: 1,234 directories rebuilt | Time: 00:15:42

Rebuild operation completed successfully
```

---

### `library scan`

Scans a library and updates the database with the latest album information from the filesystem.

**Usage:**
```bash
mcli library scan --library <NAME> [OPTIONS]
```

**Options:**

| Option | Alias | Description |
|--------|-------|-------------|
| `--library` | `-l` | **Required.** Name of the library to scan |
| `--force` | | Ignore last scan timestamp and force a full scan |
| `--verbose` | | Include verbose debug output (default: true) |

**What It Does:**

1. Reads `melodee.json` files from the library
2. Updates the database with album, artist, and song information
3. Processes album artwork and metadata
4. Records scan history for monitoring

**Skip Conditions:**

- Library hasn't changed since last scan (unless `--force` is used)
- Library is locked

**Example:**
```bash
# Scan if library has changed
./mcli library scan --library "Storage"

# Force a full scan
./mcli library scan -l "Storage" --force
```

---

### `library process`

Processes media files from the Inbound library into the Staging library. This is the main ingestion command.

**Usage:**
```bash
mcli library process --library <NAME> [OPTIONS]
```

**Options:**

| Option | Alias | Description |
|--------|-------|-------------|
| `--library` | `-l` | **Required.** Name of the inbound library to process |
| `--copy` | | Copy files instead of moving (default: true) |
| `--force` | | Override existing Melodee data files (default: true) |
| `--limit` | | Maximum number of albums to process, then quit |
| `--pre-script` | | Script to run before processing |
| `--inbound` | | Inbound path (use with `--staging` for path-based mode) |
| `--staging` | | Staging path (use with `--inbound` for path-based mode) |
| `--verbose` | | Include verbose debug output (default: true) |

**Modes:**

**Library Mode** (default):
- Uses library names from the database
- Requires configured Inbound and Staging libraries

**Path-Based Mode**:
- Uses explicit paths with `--inbound` and `--staging`
- Bypasses database library lookup
- Useful for one-off processing

**What It Does:**

1. Scans the inbound library for media files
2. Parses ID3/Vorbis tags and extracts album artwork
3. Validates and normalizes metadata
4. Creates `melodee.json` metadata files
5. Moves/copies processed albums to staging
6. Records processing history

**Examples:**
```bash
# Process all albums from Inbound to Staging
./mcli library process --library "Inbound"

# Process only 10 albums
./mcli library process -l "Inbound" --limit 10

# Path-based mode
./mcli library process --inbound "/mnt/incoming" --staging "/mnt/staging"

# Run a pre-processing script
./mcli library process -l "Inbound" --pre-script "/scripts/prepare.sh"
```

---

### `library move-ok`

Moves albums with "Ok" status from one library to another (typically Staging → Storage).

**Usage:**
```bash
mcli library move-ok --library <FROM> --to-library <TO> [OPTIONS]
```

**Options:**

| Option | Alias | Description |
|--------|-------|-------------|
| `--library` | `-l` | **Required.** Source library name |
| `--to-library` | | **Required.** Destination library name |
| `--from-path` | | Source path (use with `--to-path` for path-based mode) |
| `--to-path` | | Destination path (use with `--from-path` for path-based mode) |
| `--verbose` | | Include verbose debug output (default: true) |

**Modes:**

**Library Mode**:
- Uses library names from the database
- Only moves albums marked as "Ok" status

**Path-Based Mode**:
- Uses explicit paths with `--from-path` and `--to-path`
- Bypasses database library lookup

**Examples:**
```bash
# Move Ok albums from Staging to Storage
./mcli library move-ok --library "Staging" --to-library "Storage"

# Path-based mode
./mcli library move-ok --from-path "/mnt/staging" --to-path "/mnt/library"
```

---

### `library purge`

⚠️ **DESTRUCTIVE OPERATION**

Purges a library, deleting all artists, albums, songs, and resetting library statistics. This does **not** delete files from the filesystem, only database records.

**Usage:**
```bash
mcli library purge --library <NAME> [OPTIONS]
```

**Options:**

| Option | Alias | Description |
|--------|-------|-------------|
| `--library` | `-l` | **Required.** Name of the library to purge |
| `--force` | | Ignore last scan timestamp |
| `--verbose` | | Include verbose debug output (default: true) |

**Safety:**

- ❌ Won't run on locked libraries
- ⚠️ Only purges database records, not filesystem files

**Example:**
```bash
./mcli library purge --library "Staging"
```

---

## Configuration Commands

Manage Melodee configuration settings.

### `configuration set`

Modify Melodee configuration values.

**Usage:**
```bash
mcli configuration set [KEY] [VALUE]
```

**Example:**
```bash
./mcli configuration set jobs.libraryProcess.cronExpression "0 */5 * * * ?"
```

---

## File Commands

Analyze and inspect individual files.

### `file mpeg`

Analyzes an MPEG audio file and shows detailed information.

**Usage:**
```bash
mcli file mpeg <FILENAME>
```

**Example:**
```bash
./mcli file mpeg "/path/to/song.mp3"
```

---

## Import Commands

Import data from external sources.

### `import user-favorite-songs`

Imports user favorite songs from a CSV file.

**Usage:**
```bash
mcli import user-favorite-songs <CSV_FILE>
```

**Example:**
```bash
./mcli import user-favorite-songs "/path/to/favorites.csv"
```

---

## Job Commands

Run background maintenance tasks manually.

### `job artistsearchengine-refresh`

Refreshes the artist search engine database by updating local data from external search engines.

**Usage:**
```bash
mcli job artistsearchengine-refresh
```

**What It Does:**

- Updates local database of artist albums from search engines
- Refreshes artist metadata
- Syncs with external music databases

**Example:**
```bash
./mcli job artistsearchengine-refresh
```

---

### `job musicbrainz-update`

Downloads and updates the local MusicBrainz database used for metadata enrichment.

**Usage:**
```bash
mcli job musicbrainz-update
```

**What It Does:**

- Downloads MusicBrainz data dump
- Creates/updates local database
- Enables offline metadata lookups during scanning

**Example:**
```bash
./mcli job musicbrainz-update
```

---

## Parser Commands

Parse and analyze media metadata files.

### `parser parse`

Parses various music metadata files and extracts information. This is primarily a **debugging/diagnostic tool**.

**Usage:**
```bash
mcli parser parse <FILENAME> [OPTIONS]
```

**Options:**

| Option | Alias | Description |
|--------|-------|-------------|
| `--verbose` | | Show detailed parsing results in JSON format (default: true) |

**Supported File Types:**

**CUE Files (.cue)**:
- CUE sheets that describe track splitting
- Common in lossless releases (FLAC, APE)
- Contains track listings with timestamps

**NFO Files (.nfo)**:
- Info files with album metadata
- Common in scene releases
- Often contains ASCII art

**SFV Files (.sfv)**:
- Simple File Verification checksums
- Verifies file integrity
- Lists files with CRC32 hashes

**M3U Files (.m3u)**:
- Playlist files
- Lists audio files in order
- May contain metadata

**What It Does:**

1. Detects the file type
2. Parses using the appropriate parser
3. Extracts metadata (album, artist, tracks)
4. Validates the data
5. Outputs results in JSON format (when verbose)

**Examples:**
```bash
# Parse a CUE sheet
./mcli parser parse "/path/to/album/album.cue"

# Parse an NFO file
./mcli parser parse "/path/to/album/album.nfo"

# Parse with verbose output
./mcli parser parse "/path/to/album/album.sfv" --verbose

# Parse an M3U playlist
./mcli parser parse "/path/to/playlist.m3u"
```

**Use Cases:**

- Debugging CUE sheet parsing issues
- Testing NFO metadata extraction
- Validating file integrity with SFV files
- Troubleshooting import problems

---

## Validate Commands

Validate media files and metadata.

### `validate album`

Validates a Melodee metadata file (`melodee.json`).

**Usage:**
```bash
mcli validate album <FILENAME>
```

**Example:**
```bash
./mcli validate album "/path/to/album/melodee.json"
```

---

## Tags Commands

Display and manage media file tags.

### `tags show`

Shows all known ID3 tags from a media file.

**Usage:**
```bash
mcli tags show <FILENAME>
```

**Example:**
```bash
./mcli tags show "/path/to/song.mp3"
```

---

## Common Options

These options are available across most commands:

| Option | Description |
|--------|-------------|
| `--verbose` | Output verbose debug and timing information (default: true) |
| `--raw` | Output in raw tab-separated format for scripting |
| `-h`, `--help` | Show help information for the command |

---

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

---

## Scripting Examples

### Automated Library Maintenance

```bash
#!/bin/bash
# Daily library maintenance script

LIBRARY="Storage"
APPSETTINGS="/etc/melodee/appsettings.json"

export MELODEE_APPSETTINGS_PATH="$APPSETTINGS"

echo "Starting library maintenance for $LIBRARY..."

# Clean the library
./mcli library clean --library "$LIBRARY"

# Rebuild missing metadata
./mcli library rebuild --library "$LIBRARY"

# Scan for updates
./mcli library scan --library "$LIBRARY"

echo "Maintenance complete!"
```

### Generate Daily Report

```bash
#!/bin/bash
# Generate daily library report

LIBRARY="Storage"
REPORT_FILE="/var/log/melodee/report-$(date +%Y%m%d).txt"

./mcli library stats --library "$LIBRARY" --raw > "$REPORT_FILE"
./mcli library album-report --library "$LIBRARY" >> "$REPORT_FILE"

echo "Report saved to $REPORT_FILE"
```

### Batch Import Processing

```bash
#!/bin/bash
# Process inbound media in batches

BATCH_SIZE=50

while true; do
    echo "Processing batch of $BATCH_SIZE albums..."
    ./mcli library process --library "Inbound" --limit "$BATCH_SIZE"
    
    if [ $? -ne 0 ]; then
        echo "Processing failed, stopping."
        break
    fi
    
    # Check if there's more to process
    COUNT=$(./mcli library stats --library "Inbound" --raw | grep "Album count" | cut -f2)
    if [ "$COUNT" -eq 0 ]; then
        echo "All albums processed!"
        break
    fi
    
    sleep 5
done
```

---

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

### "Library is locked" Error

**Problem:** The library is marked as locked in the database.

**Solution:**
Unlock the library through the web interface or database:
```sql
UPDATE Libraries SET IsLocked = 0 WHERE Name = 'YourLibrary';
```

### Configuration Not Found

**Problem:** The CLI can't find `appsettings.json`.

**Solution:**
Use the `MELODEE_APPSETTINGS_PATH` environment variable:
```bash
export MELODEE_APPSETTINGS_PATH="/full/path/to/appsettings.json"
```

---

## See Also

- [Background Jobs](/jobs/) - Automated job scheduling
- [Libraries](/libraries/) - Library concepts and management
- [Configuration Reference](/configuration-reference/) - All configuration settings
- [API Reference](/api/) - REST API documentation

---
title: CLI - Album Commands
permalink: /cli/album/
layout: page
---

# Album Commands

The `album` branch provides commands for managing album data, searching albums, viewing statistics, and detecting image issues.

## Overview

```bash
mcli album [COMMAND] [OPTIONS]
```

**Available Commands:**

| Command | Alias | Description |
|---------|-------|-------------|
| `list` | `ls` | List albums in the database |
| `search` | `s` | Search for albums by name |
| `stats` | | Show album statistics grouped by status |
| `delete` | `rm` | Delete an album from the database |
| `image-issues` | `img` | Find albums with missing, invalid, or misnumbered images |

---

## album list

Lists albums in the database with their details.

### Usage

```bash
mcli album list [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--raw` | | `false` | Output results in JSON format |
| `-n`, `--limit` | | `50` | Maximum number of results to return |
| `-s`, `--status` | | | Filter by album status (Ok, New, Invalid) |
| `--verbose` | | `false` | Output verbose debug and timing results |

### Examples

```bash
# List first 50 albums
./mcli album list

# List 100 albums
./mcli album list -n 100

# List only albums with Ok status
./mcli album list --status Ok

# List albums with Invalid status
./mcli album list -s Invalid

# Output as JSON for scripting
./mcli album list --raw
```

### Output

```
╭────────────────────────────────────────────────────────────────────────────────────╮
│ Artist                         │ Album                                  │ Year │ Songs │ Status │
├────────────────────────────────────────────────────────────────────────────────────┤
│ The Beatles                    │ Abbey Road                             │ 1969 │    17 │ Ok     │
│ Pink Floyd                     │ The Dark Side of the Moon              │ 1973 │    10 │ Ok     │
│ Led Zeppelin                   │ IV                                     │ 1971 │     8 │ New    │
╰────────────────────────────────────────────────────────────────────────────────────╯

Showing 3 of 1,234 albums
```

---

## album search

Search for albums by name with optional date filtering, sorting, and bulk delete.

### Usage

```bash
mcli album search [QUERY] [OPTIONS]
```

### Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `QUERY` | No | Search query for album name. Use `*` or omit to match all albums. |

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--raw` | | `false` | Output results in JSON format |
| `-n`, `--limit` | | `25` | Maximum number of results to return |
| `--since` | | | Only show albums created within the last N days |
| `--sort` | | | Sort by column: Artist, Album, Year, Songs, Added, Status |
| `--sort-dir` | | `asc` | Sort direction: `asc` or `desc` |
| `--delete` | | `false` | ⚠️ Delete all albums matching the search criteria |
| `-y`, `--yes` | | `false` | Skip confirmation prompt when deleting |
| `--keep-files` | | `false` | Keep album files on disk when deleting (database only) |
| `--verbose` | | `false` | Output verbose debug and timing results |

### Examples

```bash
# Search for albums containing "dark"
./mcli album search "dark"

# Search with more results
./mcli album search "best of" -n 50

# Find all albums added in the last 7 days
./mcli album search --since 7

# Find all albums added today
./mcli album search --since 1

# Find albums added in last 30 days matching "beatles"
./mcli album search "beatles" --since 30

# JSON output for scripting
./mcli album search "greatest hits" --raw

# Get recently added albums as JSON
./mcli album search --since 7 --raw
```

### Sorting Examples

```bash
# Sort by artist name (A-Z)
./mcli album search --sort Artist

# Sort by artist name (Z-A)
./mcli album search --sort Artist --sort-dir desc

# Sort by year (oldest first)
./mcli album search --sort Year

# Sort by year (newest first)
./mcli album search --sort Year --sort-dir desc

# Albums added in last 10 days, sorted by added date (oldest first)
./mcli album search --since 10 --sort Added

# Albums added in last 10 days, sorted by added date (newest first)
./mcli album search --since 10 --sort Added --sort-dir desc

# Sort by song count (most songs first)
./mcli album search --sort Songs --sort-dir desc

# Sort all albums by status
./mcli album search --sort Status
```

### Bulk Delete Examples

⚠️ **WARNING: Delete operations are permanent and cannot be undone!**

```bash
# Delete all albums added in the last 5 days (with confirmation)
./mcli album search --since 5 --delete

# Delete albums matching "test" (with confirmation)
./mcli album search "test" --delete

# Delete without confirmation (USE WITH CAUTION)
./mcli album search --since 1 --delete -y

# Delete from database but keep files on disk
./mcli album search "duplicate" --delete --keep-files

# Delete all albums matching query, keeping files, no confirmation
./mcli album search "bad import" --delete --keep-files -y
```

### Output

```
Search results for: dark

╭────────────────────────────────────────────────────────────────────────────────────╮
│ Artist                         │ Album                                  │ Year │ Songs │ Status │
├────────────────────────────────────────────────────────────────────────────────────┤
│ Pink Floyd                     │ The Dark Side of the Moon              │ 1973 │    10 │ Ok     │
│ Metallica                      │ The Dark Side of Metallica             │ 1991 │    12 │ Ok     │
╰────────────────────────────────────────────────────────────────────────────────────╯

Found 2 matching albums (showing 2)
```

### Output with --since

When using `--since`, results are sorted by creation date (newest first) and include an "Added" column using ISO8601 format (YYYYMMDDTHHMMSS):

```
Albums created in the last 7 days

╭──────────────────────────┬────────────────────────────┬──────┬───────┬─────────────────┬────────╮
│ Artist                   │ Album                      │ Year │ Songs │      Added      │ Status │
├──────────────────────────┼────────────────────────────┼──────┼───────┼─────────────────┼────────┤
│ Taylor Swift             │ The Tortured Poets Dept.   │ 2024 │    16 │ 20241230T142300 │ Ok     │
│ Billie Eilish            │ Hit Me Hard and Soft       │ 2024 │    10 │ 20241229T091500 │ Ok     │
│ Sabrina Carpenter        │ Short n' Sweet             │ 2024 │    12 │ 20241228T184200 │ Ok     │
╰──────────────────────────┴────────────────────────────┴──────┴───────┴─────────────────┴────────╯

Found 3 matching albums (showing 3)
```

### Delete Confirmation

When using `--delete`, a confirmation prompt is shown with details about what will be deleted:

```
Albums created in the last 5 days

╭──────────────────────────┬────────────────────────────┬──────┬───────┬─────────────┬────────╮
│ Artist                   │ Album                      │ Year │ Songs │    Added    │ Status │
├──────────────────────────┼────────────────────────────┼──────┼───────┼─────────────┼────────┤
│ Test Artist              │ Test Album                 │ 2024 │     5 │ 12-30 10:00 │ Ok     │
│ Another Test             │ Bad Import                 │ 2024 │    12 │ 12-29 15:30 │ Ok     │
╰──────────────────────────┴────────────────────────────┴──────┴───────┴─────────────┴────────╯

Found 2 matching albums (showing 2)

───────────────────────  ⚠️  DESTRUCTIVE OPERATION  ⚠️  ───────────────────────

This will permanently delete:
  • 2 album(s)
  • 17 song(s)
  • All associated files on disk

This action cannot be undone!

Are you sure you want to delete these albums? [y/n] (n):
```

### Safety Features

- **Confirmation required**: By default, you must confirm before deletion
- **Locked albums skipped**: Albums marked as locked will not be deleted
- **Clear summary**: Shows exactly how many albums, songs, and files will be affected
- **Keep files option**: Use `--keep-files` to remove from database only
- **Progress indicator**: Shows deletion progress for large batch operations

---

## album stats

Show album statistics grouped by status with detailed breakdowns.

### Usage

```bash
mcli album stats [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--raw` | | `false` | Output results in JSON format |
| `--verbose` | | `false` | Output verbose debug and timing results |

### Examples

```bash
# Show album statistics
./mcli album stats

# JSON output for monitoring
./mcli album stats --raw
```

### Output

```
                    Album Statistics                    
╭─────────────────────┬───────────┬───────╮
│ Metric              │     Count │     % │
├─────────────────────┼───────────┼───────┤
│ Total Albums        │     1,234 │  100% │
│ Total Songs         │    15,678 │   --- │
│ ─────────────────── │ ───────── │ ───── │
│ Missing Images      │        15 │  1.2% │
│ Locked              │         3 │  0.2% │
╰─────────────────────┴───────────┴───────╯

               Albums by Status               
╭─────────┬───────────┬───────╮
│ Status  │     Count │     % │
├─────────┼───────────┼───────┤
│ Ok      │     1,200 │ 97.2% │
│ New     │        30 │  2.4% │
│ Invalid │         4 │  0.3% │
╰─────────┴───────────┴───────╯
```

### JSON Output

```json
{
  "TotalAlbums": 1234,
  "LockedAlbums": 3,
  "AlbumsWithNoImages": 15,
  "TotalSongs": 15678,
  "StatusCounts": [
    { "Status": "Ok", "Count": 1200 },
    { "Status": "New", "Count": 30 },
    { "Status": "Invalid", "Count": 4 }
  ]
}
```

---

## album delete

Delete an album from the database with optional file deletion.

### Usage

```bash
mcli album delete <ID> [OPTIONS]
```

### Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `ID` | Yes | Album ID to delete |

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--keep-files` | | `false` | Keep the album directory on disk (do not delete files) |
| `-y`, `--yes` | | `false` | Skip confirmation prompt |
| `--verbose` | | `false` | Output verbose debug and timing results |

### Examples

```bash
# Delete album (with confirmation, deletes files)
./mcli album delete 123

# Delete album but keep files on disk
./mcli album delete 123 --keep-files

# Delete without confirmation (scripting)
./mcli album delete 123 -y

# Delete from database only, no confirmation
./mcli album delete 123 --keep-files -y
```

### Output

```
Album: Abbey Road
Artist: The Beatles
Songs: 17
Directory: /mnt/music/library/The Beatles/Abbey Road

Delete album 'Abbey Road' and ALL files on disk? [y/n] (n): y

✓ Album 'Abbey Road' deleted successfully.
```

### Safety Notes

- ⚠️ **Locked albums cannot be deleted.** Unlock the album first.
- ⚠️ **Default behavior deletes files from disk.** Use `--keep-files` to preserve.
- The command shows album details and requires confirmation by default.

---

## album image-issues

Find albums with missing, invalid, or incorrectly numbered images.

### Usage

```bash
mcli album image-issues [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--raw` | | `false` | Output results in JSON format |
| `-n`, `--limit` | | `100` | Maximum number of results to return |
| `--missing` | | `true` | Include albums with missing images |
| `--invalid` | | `true` | Include albums with invalid images (wrong size, not square, etc.) |
| `--misnumbered` | | `true` | Include albums with incorrectly numbered images |
| `--verbose` | | `false` | Output verbose debug and timing results |

### Image Naming Convention

Melodee expects album images to follow this naming pattern:

```
i-XX-Type.jpg
```

Where:
- `i-` is a required prefix
- `XX` is a two-digit sequential number starting from `01`
- `Type` is the image type (e.g., `Front`, `Back`, `Inside`)

**Examples of valid image names:**
- `i-01-Front.jpg` (primary front cover)
- `i-02-Back.jpg` (back cover)
- `i-03-Inside.jpg` (inside artwork)

### Issue Types

**Missing:**
Albums with no images at all.

**Invalid:**
Images that fail validation:
- Not square (width ≠ height)
- Below minimum size
- Corrupted or unreadable

**Misnumbered:**
Images that don't follow sequential numbering:
- Gaps in numbering (e.g., `i-01`, `i-03` missing `i-02`)
- Wrong format (e.g., `cover.jpg` instead of `i-01-Front.jpg`)
- Starting number not `01`

### Examples

```bash
# Find all image issues
./mcli album image-issues

# Find only albums with missing images
./mcli album image-issues --invalid=false --misnumbered=false

# Find only albums with invalid images
./mcli album image-issues --missing=false --misnumbered=false

# Find only misnumbered images
./mcli album image-issues --missing=false --invalid=false

# Limit results
./mcli album image-issues -n 50

# JSON output for automation
./mcli album image-issues --raw
```

### Output

```
Scanning albums for image issues... ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 100% 0:00:05

               Missing Images (5)               
╭──────┬─────────────────────────┬────────────────────────────────┬─────────────────╮
│   ID │ Artist                  │ Album                          │ Details         │
├──────┼─────────────────────────┼────────────────────────────────┼─────────────────┤
│  123 │ The Beatles             │ Please Please Me                │ No images found │
│  456 │ Rolling Stones          │ Exile on Main St.              │ No images found │
╰──────┴─────────────────────────┴────────────────────────────────┴─────────────────╯

               Invalid Images (2)               
╭──────┬─────────────────────────┬────────────────────────────────┬──────────────────────────────────╮
│   ID │ Artist                  │ Album                          │ Details                          │
├──────┼─────────────────────────┼────────────────────────────────┼──────────────────────────────────┤
│  789 │ Pink Floyd              │ Animals                        │ i-01-Front.jpg: Image not square │
╰──────┴─────────────────────────┴────────────────────────────────┴──────────────────────────────────╯

               Misnumbered Images (3)               
╭──────┬─────────────────────────┬────────────────────────────────┬────────────────────────────────────╮
│   ID │ Artist                  │ Album                          │ Details                            │
├──────┼─────────────────────────┼────────────────────────────────┼────────────────────────────────────┤
│  101 │ Led Zeppelin            │ Houses of the Holy             │ i-03-Front.jpg (expected 01)       │
│  102 │ Queen                   │ A Night at the Opera           │ cover.jpg (invalid format)         │
╰──────┴─────────────────────────┴────────────────────────────────┴────────────────────────────────────╯

Found 10 albums with image issues
```

### JSON Output

```json
[
  {
    "AlbumId": 123,
    "AlbumName": "Please Please Me",
    "ArtistName": "The Beatles",
    "Directory": "/mnt/music/library/The Beatles/Please Please Me",
    "IssueType": "Missing",
    "Details": "No images found"
  },
  {
    "AlbumId": 789,
    "AlbumName": "Animals",
    "ArtistName": "Pink Floyd",
    "Directory": "/mnt/music/library/Pink Floyd/Animals",
    "IssueType": "Invalid",
    "Details": "i-01-Front.jpg: Image not square [800x600]"
  }
]
```

### Use Cases

- **Pre-release audit:** Check albums before moving to storage
- **Quality control:** Identify albums needing artwork attention
- **Automated monitoring:** Integrate with scripts for regular checks
- **Batch fixing:** Export JSON and process with external tools

---

## See Also

- [Artist Commands](/cli/artist/) - Artist data management
- [Library Commands](/cli/library/) - Library operations
- [CLI Overview](/cli/) - Main CLI documentation

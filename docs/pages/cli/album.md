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
| `--verbose` | | `true` | Output verbose debug and timing results |

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

Search for albums by name using normalized string matching.

### Usage

```bash
mcli album search <QUERY> [OPTIONS]
```

### Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `QUERY` | Yes | Search query for album name |

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--raw` | | `false` | Output results in JSON format |
| `-n`, `--limit` | | `25` | Maximum number of results to return |
| `--verbose` | | `true` | Output verbose debug and timing results |

### Examples

```bash
# Search for albums containing "dark"
./mcli album search "dark"

# Search with more results
./mcli album search "best of" -n 50

# JSON output for scripting
./mcli album search "greatest hits" --raw
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
| `--verbose` | | `true` | Output verbose debug and timing results |

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
| `--verbose` | | `true` | Output verbose debug and timing results |

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
| `--verbose` | | `true` | Output verbose debug and timing results |

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

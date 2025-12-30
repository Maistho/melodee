---
title: CLI - Artist Commands
permalink: /cli/artist/
layout: page
---

# Artist Commands

The `artist` branch provides commands for managing artist data, searching artists, viewing statistics, and identifying potential issues like duplicates or missing images.

## Overview

```bash
mcli artist [COMMAND] [OPTIONS]
```

**Available Commands:**

| Command | Alias | Description |
|---------|-------|-------------|
| `list` | `ls` | List artists in the database |
| `search` | `s` | Search for artists by name |
| `stats` | | Show artist statistics including missing images and potential duplicates |
| `delete` | `rm` | Delete an artist from the database |

---

## artist list

Lists artists in the database with their album and song counts.

### Usage

```bash
mcli artist list [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--raw` | | `false` | Output results in JSON format |
| `-n`, `--limit` | | `50` | Maximum number of results to return |
| `--verbose` | | `true` | Output verbose debug and timing results |

### Examples

```bash
# List first 50 artists
./mcli artist list

# List 100 artists
./mcli artist list -n 100

# Output as JSON for scripting
./mcli artist list --raw
```

### Output

```
╭─────────────────────────────┬────────┬───────┬───────┬────────┬──────────╮
│ Name                        │ Albums │ Songs │ Plays │ Rating │  Status  │
├─────────────────────────────┼────────┼───────┼───────┼────────┼──────────┤
│ The Beatles                 │     13 │   227 │  1542 │  4.8★  │    ✓     │
│ Pink Floyd                  │     15 │   164 │   892 │  4.7★  │    ✓     │
│ Led Zeppelin                │      9 │    92 │   456 │  4.5★  │ 🔒 Locked │
╰─────────────────────────────┴────────┴───────┴───────┴────────┴──────────╯

Showing 3 of 1,234 artists
```

### JSON Output

```json
[
  {
    "Id": 1,
    "ApiKey": "a1b2c3d4-...",
    "Name": "The Beatles",
    "NameNormalized": "beatles",
    "AlbumCount": 13,
    "SongCount": 227,
    "IsLocked": false,
    "LibraryId": 3,
    "CreatedAt": "2024-01-15T10:30:00Z",
    "LastPlayedAt": "2024-12-30T08:15:00Z",
    "PlayedCount": 1542,
    "CalculatedRating": 4.8
  }
]
```

---

## artist search

Search for artists by name using normalized string matching.

### Usage

```bash
mcli artist search <QUERY> [OPTIONS]
```

### Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `QUERY` | Yes | Search query for artist name |

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--raw` | | `false` | Output results in JSON format |
| `-n`, `--limit` | | `25` | Maximum number of results to return |
| `--verbose` | | `true` | Output verbose debug and timing results |

### Examples

```bash
# Search for artists containing "beatles"
./mcli artist search "beatles"

# Search with more results
./mcli artist search "john" -n 50

# JSON output for scripting
./mcli artist search "pink" --raw
```

### Output

```
Search results for: beatles

╭─────────────────────────────┬──────────────────────┬────────┬───────┬────────╮
│ Name                        │ Alternate Names      │ Albums │ Songs │ Rating │
├─────────────────────────────┼──────────────────────┼────────┼───────┼────────┤
│ The Beatles                 │ Beatles, Fab Four    │     13 │   227 │  4.8★  │
│ The Bootleg Beatles         │ ---                  │      2 │    24 │  3.5★  │
╰─────────────────────────────┴──────────────────────┴────────┴───────┴────────╯

Found 2 matching artists (showing 2)
```

---

## artist stats

Show artist statistics including counts, missing images, and potential duplicate detection.

### Usage

```bash
mcli artist stats [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--raw` | | `false` | Output results in JSON format |
| `--verbose` | | `true` | Output verbose debug and timing results |

### Examples

```bash
# Show artist statistics
./mcli artist stats

# JSON output for monitoring
./mcli artist stats --raw
```

### Output

```
                   Artist Statistics                   
╭─────────────────────┬───────────┬───────╮
│ Metric              │     Count │     % │
├─────────────────────┼───────────┼───────┤
│ Total Artists       │     1,234 │  100% │
│ Total Albums        │    15,678 │   --- │
│ Total Songs         │   187,234 │   --- │
│ ─────────────────── │ ───────── │ ───── │
│ Missing Images      │        23 │  1.9% │
│ No Albums           │         5 │  0.4% │
│ Locked              │        12 │  1.0% │
│ Ready to Process    │         8 │  0.6% │
╰─────────────────────┴───────────┴───────╯

              Potential Duplicates              
╭─────────────────────────────┬───────╮
│ Normalized Name             │ Count │
├─────────────────────────────┼───────┤
│ beatles                     │     3 │
│ prince                      │     2 │
│ queen                       │     2 │
╰─────────────────────────────┴───────╯

⚠ Found 3 artist names with potential duplicates
```

### Duplicate Detection

The stats command detects potential duplicate artists by grouping them by their normalized name. Normalized names:
- Are lowercase
- Remove articles ("The", "A", "An")
- Remove special characters
- Collapse whitespace

**Example duplicates:**
- "The Beatles" and "Beatles" both normalize to "beatles"
- "Prince" and "PRINCE" both normalize to "prince"

### JSON Output

```json
{
  "TotalArtists": 1234,
  "LockedArtists": 12,
  "ArtistsWithNoImages": 23,
  "ArtistsWithNoAlbums": 5,
  "ArtistsReadyToProcess": 8,
  "TotalAlbums": 15678,
  "TotalSongs": 187234,
  "PotentialDuplicates": [
    { "Name": "beatles", "Count": 3 },
    { "Name": "prince", "Count": 2 },
    { "Name": "queen", "Count": 2 }
  ]
}
```

### Use Cases

- **Quality control:** Identify artists needing images or attention
- **Duplicate cleanup:** Find and merge duplicate artist entries
- **Monitoring:** Track library health over time
- **Reporting:** Generate statistics for dashboards

---

## artist delete

Delete an artist and all associated albums from the database.

### Usage

```bash
mcli artist delete <ID> [OPTIONS]
```

### Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `ID` | Yes | Artist ID to delete |

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--keep-files` | | `false` | Keep the artist directory on disk (do not delete files) |
| `-y`, `--yes` | | `false` | Skip confirmation prompt |
| `--verbose` | | `true` | Output verbose debug and timing results |

### Examples

```bash
# Delete artist (with confirmation, deletes files)
./mcli artist delete 42

# Delete artist but keep files on disk
./mcli artist delete 42 --keep-files

# Delete without confirmation (scripting)
./mcli artist delete 42 -y

# Delete from database only, no confirmation
./mcli artist delete 42 --keep-files -y
```

### Output

```
Artist: The Beatles
Albums: 13
Songs: 227
Directory: /mnt/music/library/The Beatles

Delete artist 'The Beatles' and ALL files on disk? [y/n] (n): y

✓ Artist 'The Beatles' deleted successfully.
```

### What Gets Deleted

When deleting an artist:
1. **Database records:**
   - Artist record
   - All associated albums
   - All associated songs
   - All contributor associations
   - User ratings and stars for this artist

2. **Filesystem (unless `--keep-files`):**
   - Artist directory
   - All album subdirectories
   - All music files and images

### Safety Notes

- ⚠️ **Locked artists cannot be deleted.** Unlock the artist first through the web interface.
- ⚠️ **Default behavior deletes files from disk.** Use `--keep-files` to preserve files.
- ⚠️ **This is a destructive operation.** The command shows details and requires confirmation by default.
- ✅ **Library statistics are updated** after deletion to maintain accurate counts.

### Preserving Files

When using `--keep-files`:
1. The command temporarily moves the artist directory to a backup location
2. Deletes the database records
3. Restores the directory to its original location
4. Files remain intact for potential re-import or manual handling

---

## Workflow Examples

### Finding and Cleaning Up Duplicates

```bash
# 1. Find potential duplicates
./mcli artist stats | grep -A 20 "Potential Duplicates"

# 2. Search for specific duplicate
./mcli artist search "beatles"

# 3. Review each artist in web interface or use API to compare

# 4. Delete the duplicate (keep files if unsure)
./mcli artist delete 789 --keep-files
```

### Batch Processing with Scripts

```bash
#!/bin/bash
# Find artists without images and export for review

./mcli artist stats --raw | \
  jq -r '.PotentialDuplicates[] | "\(.Name): \(.Count) entries"' > duplicates.txt

echo "Potential duplicates saved to duplicates.txt"
```

### Integration with Monitoring

```bash
#!/bin/bash
# Daily artist health check

STATS=$(./mcli artist stats --raw)

NO_IMAGES=$(echo "$STATS" | jq '.ArtistsWithNoImages')
DUPLICATES=$(echo "$STATS" | jq '.PotentialDuplicates | length')

if [ "$NO_IMAGES" -gt 10 ]; then
    echo "WARNING: $NO_IMAGES artists missing images"
fi

if [ "$DUPLICATES" -gt 0 ]; then
    echo "WARNING: $DUPLICATES potential duplicate artist names found"
fi
```

---

## See Also

- [Album Commands](/cli/album/) - Album data management
- [Library Commands](/cli/library/) - Library operations
- [CLI Overview](/cli/) - Main CLI documentation

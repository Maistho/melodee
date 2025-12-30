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
| `--verbose` | | `false` | Output verbose debug and timing results |

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

Search for artists by name with optional date filtering, sorting, and bulk delete.

### Usage

```bash
mcli artist search [QUERY] [OPTIONS]
```

### Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `QUERY` | No | Search query for artist name. Use `*` or omit to match all artists. |

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--raw` | | `false` | Output results in JSON format |
| `-n`, `--limit` | | `25` | Maximum number of results to return |
| `--since` | | | Only show artists created within the last N days |
| `--sort` | | | Sort by column: Name, Albums, Songs, Added, Rating |
| `--sort-dir` | | `asc` | Sort direction: `asc` or `desc` |
| `--delete` | | `false` | ⚠️ Delete all artists matching the search criteria |
| `-y`, `--yes` | | `false` | Skip confirmation prompt when deleting |
| `--keep-files` | | `false` | Keep artist files on disk when deleting (database only) |
| `--verbose` | | `false` | Output verbose debug and timing results |

### Examples

```bash
# Search for artists containing "beatles"
./mcli artist search "beatles"

# Search with more results
./mcli artist search "john" -n 50

# Find all artists added in the last 7 days
./mcli artist search --since 7

# Find all artists added today
./mcli artist search --since 1

# JSON output for scripting
./mcli artist search "pink" --raw

# Get recently added artists as JSON
./mcli artist search --since 7 --raw
```

### Sorting Examples

```bash
# Sort by name (A-Z)
./mcli artist search --sort Name

# Sort by name (Z-A)
./mcli artist search --sort Name --sort-dir desc

# Sort by album count (most albums first)
./mcli artist search --sort Albums --sort-dir desc

# Sort by song count (most songs first)
./mcli artist search --sort Songs --sort-dir desc

# Artists added in last 10 days, sorted by added date (oldest first)
./mcli artist search --since 10 --sort Added

# Artists added in last 10 days, sorted by added date (newest first)
./mcli artist search --since 10 --sort Added --sort-dir desc

# Sort by rating (highest first)
./mcli artist search --sort Rating --sort-dir desc
```

### Bulk Delete Examples

⚠️ **WARNING: Delete operations are permanent and cannot be undone!**

```bash
# Delete all artists added in the last 5 days (with confirmation)
./mcli artist search --since 5 --delete

# Delete artists matching "test" (with confirmation)
./mcli artist search "test" --delete

# Delete without confirmation (USE WITH CAUTION)
./mcli artist search --since 1 --delete -y

# Delete from database but keep files on disk
./mcli artist search "duplicate" --delete --keep-files

# Delete all artists matching query, keeping files, no confirmation
./mcli artist search "bad import" --delete --keep-files -y
```

### Output

```
Search results for: beatles

╭─────────────────────────────┬────────┬───────┬────────╮
│ Name                        │ Albums │ Songs │ Rating │
├─────────────────────────────┼────────┼───────┼────────┤
│ The Beatles                 │     13 │   227 │  4.8★  │
│ The Bootleg Beatles         │      2 │    24 │  3.5★  │
╰─────────────────────────────┴────────┴───────┴────────╯

Found 2 matching artists (showing 2)
```

### Output with --since

When using `--since`, results are sorted by creation date (newest first) and include an "Added" column using ISO8601 format (YYYYMMDDTHHMMSS):

```
Artists created in the last 7 days

╭─────────────────────────────┬────────┬───────┬─────────────────┬────────╮
│ Name                        │ Albums │ Songs │      Added      │ Rating │
├─────────────────────────────┼────────┼───────┼─────────────────┼────────┤
│ New Artist                  │      2 │    24 │ 20241230T142300 │   ---  │
│ Another New Artist          │      1 │    10 │ 20241229T091500 │   ---  │
╰─────────────────────────────┴────────┴───────┴─────────────────┴────────╯

Found 2 matching artists (showing 2)
```

### Delete Confirmation

When using `--delete`, a confirmation prompt is shown with details about what will be deleted:

```
Artists created in the last 5 days

╭─────────────────────────────┬────────┬───────┬─────────────────┬────────╮
│ Name                        │ Albums │ Songs │      Added      │ Rating │
├─────────────────────────────┼────────┼───────┼─────────────────┼────────┤
│ Test Artist                 │      2 │    15 │ 20241230T100000 │   ---  │
│ Bad Import Artist           │      3 │    32 │ 20241229T153000 │   ---  │
╰─────────────────────────────┴────────┴───────┴─────────────────┴────────╯

Found 2 matching artists (showing 2)

───────────────────────  ⚠️  DESTRUCTIVE OPERATION  ⚠️  ───────────────────────

This will permanently delete:
  • 2 artist(s)
  • 5 album(s)
  • 47 song(s)
  • All associated files on disk

This action cannot be undone!

Are you sure you want to delete these artists? [y/n] (n):
```

### Safety Features

- **Confirmation required**: By default, you must confirm before deletion
- **Locked artists skipped**: Artists marked as locked will not be deleted
- **Clear summary**: Shows exactly how many artists, albums, songs, and files will be affected
- **Keep files option**: Use `--keep-files` to remove from database only
- **Progress indicator**: Shows deletion progress for large batch operations

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
| `--verbose` | | `false` | Output verbose debug and timing results |

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
| `--verbose` | | `false` | Output verbose debug and timing results |

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

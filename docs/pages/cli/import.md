---
title: CLI - Import Commands
permalink: /cli/import/
layout: page
---

# Import Commands

The `import` branch provides commands for importing data from external sources into Melodee.

## Overview

```bash
mcli import [COMMAND] [OPTIONS]
```

**Available Commands:**

| Command | Description |
|---------|-------------|
| `user-favorite-songs` | Import user favorite songs from a CSV file |

---

## import user-favorite-songs

Imports user favorite songs from a CSV file, marking them as starred in the database.

### Usage

```bash
mcli import user-favorite-songs <CSV_FILE> [OPTIONS]
```

### Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `CSV_FILE` | Yes | Path to the CSV file containing favorite songs |

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--verbose` | | `true` | Output verbose debug and timing results |

### CSV Format

The CSV file should contain song information for matching. Supported columns:

| Column | Required | Description |
|--------|----------|-------------|
| `artist` | Yes | Artist name |
| `album` | No | Album name (improves matching accuracy) |
| `title` | Yes | Song title |
| `track_number` | No | Track number on album |

### Example CSV

```csv
artist,album,title,track_number
The Beatles,Abbey Road,Come Together,1
Pink Floyd,The Dark Side of the Moon,Time,4
Led Zeppelin,IV,Stairway to Heaven,4
Queen,A Night at the Opera,Bohemian Rhapsody,11
```

### Examples

```bash
# Import favorites from CSV
./mcli import user-favorite-songs "/path/to/favorites.csv"

# Import with verbose output
./mcli import user-favorite-songs "/path/to/favorites.csv" --verbose
```

### Output

```
Importing user favorites from: /path/to/favorites.csv

Processing 100 entries...
╭────────────────────┬───────╮
│ Status             │ Count │
├────────────────────┼───────┤
│ ✓ Matched & Starred│    87 │
│ ⚠ Not Found        │    10 │
│ ⚠ Multiple Matches │     3 │
╰────────────────────┴───────╯

Import complete: 87 songs starred
```

### Matching Logic

The import process attempts to match songs using:

1. **Exact match:** Artist name + Song title + Album name (if provided)
2. **Normalized match:** Case-insensitive, ignoring special characters
3. **Fuzzy match:** Allows for slight variations in naming

### Not Found Report

When songs can't be matched, they're reported:

```
Songs not found:
- Artist: "The Beetles", Title: "Help!" (possible typo: "The Beatles")
- Artist: "Unknown Artist", Title: "Unknown Song"
```

### Multiple Match Handling

When multiple songs match:

```
Multiple matches found:
- Artist: "Queen", Title: "We Will Rock You"
  → 3 versions found (different albums)
  → First match used
```

### Use Cases

- **Migration:** Import favorites from another music player
- **Backup restore:** Restore starred songs from export
- **Bulk starring:** Mark many songs as favorites at once

### Creating Export Files

From other services:

**Spotify:** Use third-party tools to export liked songs to CSV

**iTunes/Apple Music:** 
```bash
# Export playlist to CSV using AppleScript or third-party tools
```

**Last.fm:**
```bash
# Use Last.fm API to export loved tracks
```

### Scripting Example

```bash
#!/bin/bash
# Import favorites for multiple users

for user_file in /backups/favorites/*.csv; do
    username=$(basename "$user_file" .csv)
    echo "Importing favorites for: $username"
    ./mcli import user-favorite-songs "$user_file"
done
```

---

## See Also

- [CLI Overview](/cli/) - Main CLI documentation
- [Artist Commands](/cli/artist/) - Artist data management
- [Album Commands](/cli/album/) - Album data management

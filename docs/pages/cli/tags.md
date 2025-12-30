---
title: CLI - Tags Commands
permalink: /cli/tags/
layout: page
---

# Tags Commands

The `tags` branch provides commands for displaying and analyzing media file tags (ID3, Vorbis Comments, etc.).

## Overview

```bash
mcli tags [COMMAND] [OPTIONS]
```

**Available Commands:**

| Command | Description |
|---------|-------------|
| `show` | Display all tags from a media file |

---

## tags show

Displays all known tags from a media file including ID3v1, ID3v2, Vorbis Comments, APE tags, and other metadata.

### Usage

```bash
mcli tags show <FILENAME> [OPTIONS]
```

### Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `FILENAME` | Yes | Path to the media file to analyze |

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--verbose` | | `true` | Output verbose debug and timing results |

### Supported Formats

| Format | Tag Types |
|--------|-----------|
| MP3 | ID3v1, ID3v2.3, ID3v2.4, APE |
| FLAC | Vorbis Comments, ID3v2 |
| OGG | Vorbis Comments |
| M4A/AAC | iTunes/MP4 tags |
| WAV | INFO chunks, ID3v2 |
| WMA | ASF metadata |

### Examples

```bash
# Show tags from MP3 file
./mcli tags show "/path/to/song.mp3"

# Show tags from FLAC file
./mcli tags show "/path/to/song.flac"
```

### Output

```
╭─────────────────────────────────────────────────────────────────────────────╮
│ File: /path/to/song.mp3                                                     │
│ Format: MPEG Audio (MP3)                                                    │
│ Tag Types: ID3v2.4, ID3v1                                                   │
╰─────────────────────────────────────────────────────────────────────────────╯

                          ID3v2.4 Tags                          
╭─────────────────────────┬───────────────────────────────────────────────────╮
│ Tag                     │ Value                                             │
├─────────────────────────┼───────────────────────────────────────────────────┤
│ TIT2 (Title)            │ Come Together                                     │
│ TPE1 (Artist)           │ The Beatles                                       │
│ TALB (Album)            │ Abbey Road                                        │
│ TRCK (Track)            │ 1/17                                              │
│ TYER (Year)             │ 1969                                              │
│ TCON (Genre)            │ Rock                                              │
│ TPOS (Disc)             │ 1/1                                               │
│ TPE2 (Album Artist)     │ The Beatles                                       │
│ TCMP (Compilation)      │ 0                                                 │
│ APIC (Picture)          │ Front Cover (image/jpeg, 45.2 KB)                 │
│ TXXX:MusicBrainz Album Id│ 12345678-1234-1234-1234-123456789012              │
╰─────────────────────────┴───────────────────────────────────────────────────╯

                          ID3v1 Tags                          
╭─────────────────────────┬───────────────────────────────────────────────────╮
│ Tag                     │ Value                                             │
├─────────────────────────┼───────────────────────────────────────────────────┤
│ Title                   │ Come Together                                     │
│ Artist                  │ The Beatles                                       │
│ Album                   │ Abbey Road                                        │
│ Year                    │ 1969                                              │
│ Genre                   │ Rock (17)                                         │
│ Track                   │ 1                                                 │
╰─────────────────────────┴───────────────────────────────────────────────────╯
```

### Common Tags

**Standard Tags:**

| ID3v2 Frame | Vorbis Comment | Description |
|-------------|----------------|-------------|
| TIT2 | TITLE | Song title |
| TPE1 | ARTIST | Performing artist |
| TALB | ALBUM | Album name |
| TRCK | TRACKNUMBER | Track number |
| TYER/TDRC | DATE | Year/date |
| TCON | GENRE | Genre |
| TPE2 | ALBUMARTIST | Album artist |
| TPOS | DISCNUMBER | Disc number |
| APIC | (embedded) | Album artwork |

**Extended Tags:**

| ID3v2 Frame | Vorbis Comment | Description |
|-------------|----------------|-------------|
| TXXX:MusicBrainz Album Id | MUSICBRAINZ_ALBUMID | MusicBrainz release ID |
| TXXX:MusicBrainz Artist Id | MUSICBRAINZ_ARTISTID | MusicBrainz artist ID |
| TXXX:MusicBrainz Track Id | MUSICBRAINZ_TRACKID | MusicBrainz recording ID |
| TXXX:ISRC | ISRC | International Standard Recording Code |

### FLAC Output Example

```
╭─────────────────────────────────────────────────────────────────────────────╮
│ File: /path/to/song.flac                                                    │
│ Format: FLAC Audio                                                          │
│ Tag Types: Vorbis Comments                                                  │
╰─────────────────────────────────────────────────────────────────────────────╯

                       Vorbis Comments                       
╭─────────────────────────┬───────────────────────────────────────────────────╮
│ Tag                     │ Value                                             │
├─────────────────────────┼───────────────────────────────────────────────────┤
│ TITLE                   │ Come Together                                     │
│ ARTIST                  │ The Beatles                                       │
│ ALBUM                   │ Abbey Road                                        │
│ TRACKNUMBER             │ 1                                                 │
│ TRACKTOTAL              │ 17                                                │
│ DATE                    │ 1969                                              │
│ GENRE                   │ Rock                                              │
│ ALBUMARTIST             │ The Beatles                                       │
│ DISCNUMBER              │ 1                                                 │
│ DISCTOTAL               │ 1                                                 │
╰─────────────────────────┴───────────────────────────────────────────────────╯

                    Embedded Pictures                    
╭───────┬────────────┬────────────┬──────────╮
│ Index │ Type       │ Format     │ Size     │
├───────┼────────────┼────────────┼──────────┤
│ 0     │ Front      │ image/jpeg │ 245.3 KB │
│ 1     │ Back       │ image/jpeg │ 198.7 KB │
╰───────┴────────────┴────────────┴──────────╯
```

### Use Cases

**Debugging Tag Issues:**
```bash
# Why isn't the artist showing correctly?
./mcli tags show "/path/to/song.mp3" | grep -i artist
```

**Verifying Metadata:**
```bash
# Check if MusicBrainz IDs are present
./mcli tags show "/path/to/song.flac" | grep -i musicbrainz
```

**Comparing Tag Versions:**
```bash
# Compare ID3v1 vs ID3v2 tags
./mcli tags show "/path/to/song.mp3"
```

### Scripting Examples

**Check for Missing Tags:**
```bash
#!/bin/bash
# Find files missing album artwork

for file in *.mp3; do
    if ! ./mcli tags show "$file" | grep -q "APIC\|Picture"; then
        echo "Missing artwork: $file"
    fi
done
```

**Export Tag Summary:**
```bash
#!/bin/bash
# Create a tag inventory

echo "File,Title,Artist,Album" > tags.csv
for file in *.mp3; do
    title=$(./mcli tags show "$file" | grep "TIT2" | cut -d'│' -f3 | xargs)
    artist=$(./mcli tags show "$file" | grep "TPE1" | cut -d'│' -f3 | xargs)
    album=$(./mcli tags show "$file" | grep "TALB" | cut -d'│' -f3 | xargs)
    echo "\"$file\",\"$title\",\"$artist\",\"$album\"" >> tags.csv
done
```

---

## See Also

- [File Commands](/cli/file/) - Analyze MPEG files
- [Parser Commands](/cli/parser/) - Parse metadata files
- [CLI Overview](/cli/) - Main CLI documentation

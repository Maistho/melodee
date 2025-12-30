---
title: CLI - Validate Commands
permalink: /cli/validate/
layout: page
---

# Validate Commands

The `validate` branch provides commands for validating media files and Melodee metadata.

## Overview

```bash
mcli validate [COMMAND] [OPTIONS]
```

**Available Commands:**

| Command | Description |
|---------|-------------|
| `album` | Validate a Melodee metadata file (melodee.json) |

---

## validate album

Validates a Melodee metadata file (`melodee.json`) and reports any issues found.

### Usage

```bash
mcli validate album <FILENAME> [OPTIONS]
```

### Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `FILENAME` | Yes | Path to the melodee.json file to validate |

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--verbose` | | `true` | Output verbose debug and timing results |

### Examples

```bash
# Validate a melodee.json file
./mcli validate album "/path/to/album/melodee.json"

# Validate with verbose output
./mcli validate album "/path/to/album/melodee.json" --verbose
```

### Output (Valid File)

```
╭─────────────────────────────────────────────────────────────────────────────╮
│ Validation Results                                                          │
├─────────────────────────────────────────────────────────────────────────────┤
│ File: /path/to/album/melodee.json                                           │
│ Status: ✓ Valid                                                             │
├─────────────────────────────────────────────────────────────────────────────┤
│ Album: Abbey Road                                                           │
│ Artist: The Beatles                                                         │
│ Year: 1969                                                                  │
│ Tracks: 17                                                                  │
│ Images: 2                                                                   │
╰─────────────────────────────────────────────────────────────────────────────╯
```

### Output (Invalid File)

```
╭─────────────────────────────────────────────────────────────────────────────╮
│ Validation Results                                                          │
├─────────────────────────────────────────────────────────────────────────────┤
│ File: /path/to/album/melodee.json                                           │
│ Status: ✗ Invalid                                                           │
├─────────────────────────────────────────────────────────────────────────────┤
│ Errors Found:                                                               │
├─────────────────────────────────────────────────────────────────────────────┤
│ ✗ Missing required field: Artist                                            │
│ ✗ Track 3: Duration is 0                                                    │
│ ✗ No album artwork found                                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│ Warnings:                                                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│ ⚠ Track 5: Missing MusicBrainz ID                                           │
│ ⚠ Genre not specified                                                       │
╰─────────────────────────────────────────────────────────────────────────────╯
```

### Validation Rules

**Required Fields:**

| Field | Description |
|-------|-------------|
| Artist | Album artist name |
| Album | Album title |
| Tracks | At least one track |

**Track Validation:**

| Rule | Description |
|------|-------------|
| Title | Each track must have a title |
| Track Number | Must be a positive integer |
| Duration | Must be greater than 0 |
| File Reference | Referenced audio file must exist |

**Image Validation:**

| Rule | Description |
|------|-------------|
| Front Cover | At least one front cover image |
| Format | Images must be JPEG, PNG, or WebP |
| Size | Images should meet minimum size requirements |

**Metadata Quality:**

| Rule | Severity | Description |
|------|----------|-------------|
| MusicBrainz IDs | Warning | Recommended for accurate matching |
| Genre | Warning | Helps with categorization |
| Year | Warning | Release year recommended |

### melodee.json Structure

```json
{
  "Artist": "The Beatles",
  "Album": "Abbey Road",
  "Year": 1969,
  "Genre": "Rock",
  "Tracks": [
    {
      "Number": 1,
      "Title": "Come Together",
      "Duration": 259,
      "File": "01 - Come Together.mp3",
      "MusicBrainzId": "12345678-1234-1234-1234-123456789012"
    }
  ],
  "Images": [
    {
      "Type": "Front",
      "File": "i-01-Front.jpg"
    }
  ],
  "MusicBrainzAlbumId": "87654321-4321-4321-4321-210987654321"
}
```

### Use Cases

**Pre-Import Validation:**
```bash
# Validate before moving to library
./mcli validate album "/staging/Artist/Album/melodee.json"
if [ $? -eq 0 ]; then
    echo "Album is valid, ready to move"
else
    echo "Fix issues before importing"
fi
```

**Batch Validation:**
```bash
#!/bin/bash
# Validate all melodee.json files in staging

find /staging -name "melodee.json" | while read file; do
    if ! ./mcli validate album "$file" > /dev/null 2>&1; then
        echo "Invalid: $(dirname "$file")"
    fi
done
```

**Quality Report:**
```bash
#!/bin/bash
# Generate validation report

echo "Validation Report - $(date)" > report.txt
echo "=========================" >> report.txt

find /library -name "melodee.json" | while read file; do
    echo "" >> report.txt
    echo "Album: $(dirname "$file")" >> report.txt
    ./mcli validate album "$file" 2>&1 >> report.txt
done
```

### Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Validation passed (no errors) |
| 1 | Validation failed (errors found) |

Use exit codes in scripts:

```bash
./mcli validate album "$file"
case $? in
    0) echo "Valid" ;;
    1) echo "Invalid - needs attention" ;;
esac
```

### Common Issues

**Missing melodee.json:**
```
Error: File not found: /path/to/album/melodee.json
```
**Solution:** Run `library rebuild` to generate metadata files.

**Corrupted JSON:**
```
Error: Invalid JSON syntax at line 15
```
**Solution:** Manually fix the JSON or regenerate with `library rebuild`.

**Missing Audio Files:**
```
Error: Track 3 references missing file: 03 - Song.mp3
```
**Solution:** Ensure all referenced audio files exist in the album directory.

---

## See Also

- [Library Commands](/cli/library/) - Rebuild metadata files
- [Album Commands](/cli/album/) - Album management
- [CLI Overview](/cli/) - Main CLI documentation

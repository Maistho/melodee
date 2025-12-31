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

Validates a Melodee metadata file (`melodee.json`) and reports any issues found. Can also validate all albums for a given artist.

### Usage

```bash
mcli validate album [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--file` | | | Path to the melodee.json file to validate |
| `--apiKey` | | | ApiKey of an album to validate |
| `--artistApiKey` | | | ApiKey of an artist to validate all albums for |
| `--library` | | | Name of Library (used with --id) |
| `--id` | | | Id of Melodee Data File to validate (used with --library) |
| `--verbose` | | `false` | Output verbose debug and timing results |

### Examples

```bash
# Validate a melodee.json file
./mcli validate album --file "/path/to/album/melodee.json"

# Validate an album by ApiKey
./mcli validate album --apiKey "12345678-1234-1234-1234-123456789012"

# Validate all albums for an artist by ApiKey
./mcli validate album --artistApiKey "87654321-4321-4321-4321-210987654321"

# Validate with verbose output
./mcli validate album --file "/path/to/album/melodee.json" --verbose
```

### Output (Single Album - Valid)

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

### Output (Artist Albums Validation)

```
╭─────────────────────────────────────────────────────────────────────────────╮
│ Artist Albums Validation Summary                                            │
├───────────┬─────────────────────────────────────────────────────────────────┤
│ Property  │ Value                                                           │
├───────────┼─────────────────────────────────────────────────────────────────┤
│ Artist    │ The Beatles                                                     │
│ Total     │ 13                                                              │
│ Valid     │ 11                                                              │
│ Invalid   │ 2                                                               │
│ Status    │ ✗ Issues Found                                                  │
╰───────────┴─────────────────────────────────────────────────────────────────╯

╭─────────────────────────────────────────────────────────────────────────────╮
│ Album Details                                                               │
├──────────────────────────────┬──────┬────────┬───────┬───────┬──────────────┤
│ Album                        │ Year │ Status │ Dir   │ Cover │ Issues       │
├──────────────────────────────┼──────┼────────┼───────┼───────┼──────────────┤
│ Abbey Road                   │ 1969 │ ✓      │ ✓     │ ✓     │ 0            │
│ Let It Be                    │ 1970 │ ✗      │ ✗     │ ✗     │ 2            │
│ ...                          │      │        │       │       │              │
╰──────────────────────────────┴──────┴────────┴───────┴───────┴──────────────╯

Issues Found:

Let It Be (1970):
  ✗ Album directory does not exist: /library/The Beatles/Let It Be
  ✗ melodee.json file not found
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

**Directory Validation (Artist Mode):**

| Rule | Description |
|------|-------------|
| Directory Exists | Album directory must exist on disk |
| melodee.json | Metadata file must be present |
| Cover Image | At least one image file present |

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

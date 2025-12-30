---
title: CLI - File Commands
permalink: /cli/file/
layout: page
---

# File Commands

The `file` branch provides commands for analyzing and inspecting individual media files.

## Overview

```bash
mcli file [COMMAND] [OPTIONS]
```

**Available Commands:**

| Command | Description |
|---------|-------------|
| `mpeg` | Analyze MPEG audio files and show detailed information |

---

## file mpeg

Analyzes an MPEG audio file and displays detailed technical information. This is primarily a **diagnostic tool** for troubleshooting media file issues.

### Usage

```bash
mcli file mpeg <FILENAME> [OPTIONS]
```

### Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `FILENAME` | Yes | Path to the MPEG audio file to analyze |

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--verbose` | | `true` | Output verbose debug and timing results |

### Examples

```bash
# Analyze an MP3 file
./mcli file mpeg "/path/to/song.mp3"

# Check if a file is valid MPEG
./mcli file mpeg "/path/to/suspicious-file.mp3"
```

### Output

```
╭─────────────────────────────────────────────────────────────╮
│ MPEG File Analysis                                          │
├─────────────────────────────────────────────────────────────┤
│ File: /path/to/song.mp3                                     │
│ Size: 8.4 MB                                                │
│ Valid MPEG: ✓ Yes                                           │
├─────────────────────────────────────────────────────────────┤
│ Format Information                                          │
├─────────────────────────────────────────────────────────────┤
│ MPEG Version: MPEG-1 Layer 3                                │
│ Bitrate: 320 kbps (CBR)                                     │
│ Sample Rate: 44100 Hz                                       │
│ Channels: Stereo                                            │
│ Duration: 3:42                                              │
├─────────────────────────────────────────────────────────────┤
│ Frame Analysis                                              │
├─────────────────────────────────────────────────────────────┤
│ Total Frames: 8,532                                         │
│ First Frame Offset: 2,048 bytes                             │
│ Frame Consistency: 100%                                     │
╰─────────────────────────────────────────────────────────────╯
```

### Information Displayed

**File Properties:**
- File path and size
- MPEG validity check

**Format Details:**
- MPEG version (MPEG-1, MPEG-2, MPEG-2.5)
- Layer (Layer I, II, or III)
- Bitrate (CBR or VBR average)
- Sample rate
- Channel mode (Mono, Stereo, Joint Stereo, Dual Channel)
- Duration

**Frame Analysis:**
- Total frame count
- First frame offset (detects ID3v2 headers)
- Frame consistency percentage

### Use Cases

- **Troubleshooting:** Identify why a file won't play or import
- **Quality check:** Verify bitrate and encoding quality
- **Corruption detection:** Check for frame consistency issues
- **Format verification:** Confirm file is actually valid MPEG

### Invalid File Output

```
╭─────────────────────────────────────────────────────────────╮
│ MPEG File Analysis                                          │
├─────────────────────────────────────────────────────────────┤
│ File: /path/to/file.mp3                                     │
│ Size: 4.2 MB                                                │
│ Valid MPEG: ✗ No                                            │
├─────────────────────────────────────────────────────────────┤
│ Issues Found:                                               │
│ - No valid MPEG frame header found                          │
│ - File may be corrupted or not an MPEG audio file           │
╰─────────────────────────────────────────────────────────────╯
```

### Scripting Example

```bash
#!/bin/bash
# Check all MP3 files in a directory

for file in *.mp3; do
    echo "Checking: $file"
    ./mcli file mpeg "$file" | grep "Valid MPEG"
done
```

---

## See Also

- [Tags Commands](/cli/tags/) - View ID3 tags from media files
- [Parser Commands](/cli/parser/) - Parse metadata files
- [CLI Overview](/cli/) - Main CLI documentation

---
title: Jukebox
permalink: /jukebox/
---

# Jukebox

Melodee's Jukebox feature enables server-side audio playback, allowing music to play directly on the server hardware. This is ideal for home audio setups, party scenarios, or any situation where you want centralized playback control.

## Overview

The Jukebox feature provides:

- **Server-Side Playback**: Audio plays on the server, not on client devices
- **Multiple Backend Support**: Choose between MPV or MPD as the audio backend
- **OpenSubsonic API Compatibility**: Control playback from any Subsonic-compatible client
- **Party Mode Integration**: Combine with Party Mode for collaborative playlist control
- **Web UI Monitoring**: View playback status and backend information from the Melodee interface

## Use Cases

### Home Audio Server

Connect your Melodee server to speakers or an amplifier, and control music playback from your phone, tablet, or computer. Everyone in the household can queue songs without needing to physically access the server.

### Party/Event Audio

Set up a shared queue where guests can add songs. The server plays music through connected speakers while you maintain control over volume and playback.

### Multi-Room Audio

With MPD backend, integrate with multi-room audio solutions like Snapcast for synchronized playback across multiple rooms.

## Prerequisites

Before enabling Jukebox, ensure you have:

1. **Audio Output**: Your server must have audio output capability (speakers, DAC, or audio interface)
2. **Backend Installed**: Either MPV or MPD must be installed on the server
3. **Admin Access**: Jukebox configuration requires administrator privileges

### Installing MPV (Recommended)

MPV is a lightweight, powerful media player with excellent audio support.

**Ubuntu/Debian:**
```bash
sudo apt install mpv
```

**Fedora:**
```bash
sudo dnf install mpv
```

**Arch Linux:**
```bash
sudo pacman -S mpv
```

**macOS:**
```bash
brew install mpv
```

### Installing MPD (Alternative)

MPD (Music Player Daemon) is a flexible server-side audio player.

**Ubuntu/Debian:**
```bash
sudo apt install mpd
```

**Fedora:**
```bash
sudo dnf install mpd
```

**Configuration:** MPD requires a configuration file (`/etc/mpd.conf` or `~/.config/mpd/mpd.conf`). Ensure the audio output is configured for your hardware.

## Enabling Jukebox

Jukebox is disabled by default. To enable it:

1. Navigate to **Admin → Settings** in the Melodee web interface
2. Find the **Jukebox** category
3. Set `jukebox.enabled` to `true`
4. Set `jukebox.backend.type` to either `mpv` or `mpd`
5. Configure the backend-specific settings (see below)
6. Save settings and restart Melodee (or wait for settings to reload)

Once enabled, the **Jukebox** menu item appears in the sidebar navigation.

## Configuration

### General Settings

| Setting | Description | Default |
|---------|-------------|---------|
| `jukebox.enabled` | Enable/disable Jukebox feature | `false` |
| `jukebox.backend.type` | Playback backend: `mpv` or `mpd` | `mpv` |

### MPV Backend Settings

| Setting | Description | Default |
|---------|-------------|---------|
| `jukebox.mpv.path` | Path to MPV executable | `/usr/bin/mpv` |
| `jukebox.mpv.audioDevice` | Audio device name (empty for default) | (empty) |
| `jukebox.mpv.extraArgs` | Additional MPV command-line arguments | (empty) |
| `jukebox.mpv.socketPath` | IPC socket path (auto-generated if empty) | (empty) |
| `jukebox.mpv.initialVolume` | Initial volume level (0-100) | `50` |
| `jukebox.mpv.enableDebugOutput` | Enable verbose MPV logging | `false` |

#### Finding Audio Devices (MPV)

To list available audio devices on Linux:

```bash
mpv --audio-device=help
```

Example output:
```
'pulse/alsa_output.pci-0000_00_1f.3.analog-stereo' (Built-in Audio Analog Stereo)
'alsa/hw:0,0' (Direct ALSA device)
```

Use the device string (e.g., `pulse/alsa_output.pci-0000_00_1f.3.analog-stereo`) in the `audioDevice` setting.

### MPD Backend Settings

| Setting | Description | Default |
|---------|-------------|---------|
| `jukebox.mpd.instanceName` | Display name for this MPD instance | `MPD` |
| `jukebox.mpd.host` | MPD server hostname | `localhost` |
| `jukebox.mpd.port` | MPD server port | `6600` |
| `jukebox.mpd.password` | MPD password (if required) | (empty) |
| `jukebox.mpd.timeoutMs` | Connection timeout in milliseconds | `5000` |
| `jukebox.mpd.initialVolume` | Initial volume level (0-100) | `50` |
| `jukebox.mpd.enableDebugOutput` | Enable verbose MPD logging | `false` |

## Using Jukebox

### From the Web Interface

Navigate to **Jukebox** in the sidebar to view:

- **Connection Status**: Whether the backend is connected
- **Playback Status**: Current playback state (playing, stopped)
- **Backend Info**: Backend type and version information
- **Capabilities**: What features the backend supports
- **Now Playing**: Currently playing track (when applicable)

**Note:** Playback controls in the web interface are for monitoring only. To control playback, use a Subsonic-compatible client or the Party Mode feature.

### From Subsonic Clients

Any Subsonic-compatible client can control Jukebox playback using the jukebox API endpoints:

| Action | API Endpoint |
|--------|--------------|
| Get Status | `jukeboxControl?action=status` |
| Start Playback | `jukeboxControl?action=start` |
| Stop Playback | `jukeboxControl?action=stop` |
| Skip to Next | `jukeboxControl?action=skip` |
| Set Volume | `jukeboxControl?action=setGain&gain=0.5` |
| Add to Queue | `jukeboxControl?action=add&id=<songId>` |
| Clear Queue | `jukeboxControl?action=clear` |
| Remove from Queue | `jukeboxControl?action=remove&index=0` |
| Shuffle Queue | `jukeboxControl?action=shuffle` |

#### Client Support

| Client | Jukebox Support | Notes |
|--------|-----------------|-------|
| DSub | ✅ Full | Dedicated Jukebox mode |
| Symfonium | ✅ Full | Jukebox mode in settings |
| Ultrasonic | ⚠️ Partial | Basic playback control |
| play:Sub | ❌ None | Not supported |
| Sonixd | ❌ None | Not supported |

### With Party Mode

Jukebox integrates with Party Mode for collaborative playback:

1. Enable Jukebox in settings
2. Create or join a Party Mode session
3. Select the Jukebox backend as the playback endpoint
4. Guests can add songs to the queue while Jukebox handles playback

See [Party Mode](/party-mode/) for more details.

## Permissions

| Feature | Admin | Editor | User |
|---------|-------|--------|------|
| Enable/Disable Jukebox | ✅ | ❌ | ❌ |
| Configure Backend | ✅ | ❌ | ❌ |
| View Status | ✅ | ✅ | ✅ |
| Control Playback | ✅ | ❌ | ❌ |
| Add to Queue (API) | ✅ | ✅ | ✅ |

## Troubleshooting

### Backend Not Connecting

1. **Verify Installation**: Ensure MPV or MPD is installed and accessible
   ```bash
   which mpv  # or: which mpd
   ```

2. **Check Path**: Verify the executable path in settings matches the actual location

3. **Test Manually**: Try playing audio directly
   ```bash
   mpv --no-video /path/to/audio/file.mp3
   ```

4. **Check Permissions**: Ensure the Melodee service user has permission to access the audio device

5. **View Logs**: Enable debug output in settings and check Melodee logs

### No Audio Output

1. **Check Audio Device**: Verify the correct audio device is configured

2. **Test Audio System**:
   ```bash
   # Test with speaker-test (ALSA)
   speaker-test -c 2 -t wav
   
   # Test with paplay (PulseAudio)
   paplay /usr/share/sounds/alsa/Front_Center.wav
   ```

3. **Check Volume**: Ensure system volume and Jukebox volume are not muted

4. **Review Permissions**: Audio group membership may be required
   ```bash
   sudo usermod -a -G audio melodee
   ```

### MPD Connection Refused

1. **Verify MPD is Running**:
   ```bash
   systemctl status mpd
   ```

2. **Check Port**: Ensure MPD is listening on the configured port
   ```bash
   ss -tlnp | grep 6600
   ```

3. **Test Connection**:
   ```bash
   nc -zv localhost 6600
   ```

4. **Check Bind Address**: MPD may only listen on certain addresses; check `mpd.conf`

### IPC Socket Issues (MPV)

1. **Check Socket Path**: Ensure the directory exists and is writable

2. **Delete Stale Socket**: Remove old socket files if MPV crashed
   ```bash
   rm /tmp/melodee-mpv-*
   ```

3. **Use Auto-Generated Path**: Leave `socketPath` empty to auto-generate

## Best Practices

1. **Use a Dedicated Audio Device**: For best quality, use a USB DAC or dedicated sound card

2. **Set Appropriate Initial Volume**: Start with a moderate volume (50%) to avoid surprises

3. **Enable Debug Logging Initially**: Helps diagnose issues during setup, disable for production

4. **Test Before Events**: Always verify playback works before hosting parties or events

5. **Monitor Resource Usage**: MPV and MPD are lightweight, but long playlists may use memory

6. **Consider Headless Operation**: For dedicated audio servers, MPV's `--no-video` mode (default) is ideal

## API Reference

### OpenSubsonic Jukebox API

```
# Get jukebox status
GET /rest/jukeboxControl?action=status

# Start playback
GET /rest/jukeboxControl?action=start

# Stop playback
GET /rest/jukeboxControl?action=stop

# Skip to next track
GET /rest/jukeboxControl?action=skip&index=1

# Set volume (0.0 to 1.0)
GET /rest/jukeboxControl?action=setGain&gain=0.75

# Add song to queue
GET /rest/jukeboxControl?action=add&id=<songId>

# Clear queue
GET /rest/jukeboxControl?action=clear

# Remove from queue by index
GET /rest/jukeboxControl?action=remove&index=2

# Shuffle queue
GET /rest/jukeboxControl?action=shuffle

# Set playback position (seconds)
GET /rest/jukeboxControl?action=skip&index=0&offset=30
```

### Response Format

```json
{
  "subsonic-response": {
    "status": "ok",
    "jukeboxStatus": {
      "currentIndex": 0,
      "playing": true,
      "gain": 0.75,
      "position": 142,
      "entry": [
        {
          "id": "12345",
          "title": "Song Title",
          "artist": "Artist Name",
          "album": "Album Name",
          "duration": 240
        }
      ]
    }
  }
}
```

---

Have questions about Jukebox? Open an issue on GitHub or check the [API documentation](/api/) for technical details.

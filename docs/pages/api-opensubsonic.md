---
title: OpenSubsonic API
permalink: /api-opensubsonic/
---

# OpenSubsonic API

Melodee implements the OpenSubsonic API specification, providing compatibility with the wide ecosystem of Subsonic and OpenSubsonic clients. This API enables seamless integration with popular music streaming applications.

## Overview

The OpenSubsonic API is based on the Subsonic 1.16.1 specification with OpenSubsonic extensions. It provides:

- **Full Subsonic Compatibility**: Works with legacy Subsonic clients
- **OpenSubsonic Extensions**: Enhanced features for modern clients
- **Real-time Transcoding**: On-the-fly format conversion
- **Scrobbling**: Last.fm and internal play tracking
- **Playlist Management**: Full CRUD operations
- **Multi-user Support**: Per-user libraries and preferences

## Endpoint Structure

All OpenSubsonic endpoints are available under the `/rest/` path:

```
http://your-server:port/rest/{endpoint}.view
```

Or using the simplified format (without `.view`):

```
http://your-server:port/rest/{endpoint}
```

## Authentication

OpenSubsonic supports multiple authentication methods:

### Token-Based (Recommended)

```
GET /rest/ping?u=username&t=token&s=salt&v=1.16.1&c=myapp
```

Where:
- `u` = username
- `t` = MD5(password + salt)
- `s` = random salt string
- `v` = API version
- `c` = client name

### API Key Authentication (Melodee Extension)

```
GET /rest/ping?apiKey=your-api-key&v=1.16.1&c=myapp
```

### Legacy Password (Not Recommended)

```
GET /rest/ping?u=username&p=password&v=1.16.1&c=myapp
```

## Response Formats

Responses are available in JSON (default) or XML:

```
GET /rest/ping?f=json  # JSON response
GET /rest/ping?f=xml   # XML response
```

### JSON Response Structure

```json
{
  "subsonic-response": {
    "status": "ok",
    "version": "1.16.1",
    "type": "Melodee",
    "serverVersion": "1.0.0.0",
    "openSubsonic": true,
    ...
  }
}
```

### Error Response

```json
{
  "subsonic-response": {
    "status": "failed",
    "version": "1.16.1",
    "error": {
      "code": 40,
      "message": "Wrong username or password"
    }
  }
}
```

## Core Endpoints

### System

| Endpoint | Description |
|----------|-------------|
| ping | Test connectivity and authentication |
| getLicense | Get server license information |
| getOpenSubsonicExtensions | List supported OpenSubsonic extensions |

### Browsing

| Endpoint | Description |
|----------|-------------|
| getMusicFolders | Get all music libraries |
| getIndexes | Get artist index (A-Z listing) |
| getArtists | Get all artists |
| getArtist | Get artist details and albums |
| getAlbum | Get album details and songs |
| getSong | Get song details |
| getGenres | Get all genres |
| getMusicDirectory | Get directory contents |

### Album/Song Lists

| Endpoint | Description |
|----------|-------------|
| getAlbumList | Get albums by various criteria |
| getAlbumList2 | Get albums (ID3 tag based) |
| getRandomSongs | Get random songs |
| getSongsByGenre | Get songs by genre |
| getNowPlaying | Get currently playing songs |
| getStarred | Get starred items |
| getStarred2 | Get starred items (ID3 based) |

### Searching

| Endpoint | Description |
|----------|-------------|
| search2 | Search artists, albums, songs |
| search3 | Search (ID3 tag based) |

### Playlists

| Endpoint | Description |
|----------|-------------|
| getPlaylists | Get all playlists |
| getPlaylist | Get playlist details |
| createPlaylist | Create new playlist |
| updatePlaylist | Update playlist |
| deletePlaylist | Delete playlist |

### Media Retrieval

| Endpoint | Description |
|----------|-------------|
| stream | Stream audio file |
| download | Download audio file |
| getCoverArt | Get album/artist artwork |
| getLyrics | Get song lyrics |
| getAvatar | Get user avatar |

### User Data

| Endpoint | Description |
|----------|-------------|
| star | Star an item |
| unstar | Unstar an item |
| setRating | Set item rating |
| scrobble | Submit scrobble |
| getUser | Get user information |

### Media Annotation

| Endpoint | Description |
|----------|-------------|
| getSimilarSongs | Get similar songs |
| getSimilarSongs2 | Get similar songs (ID3 based) |
| getTopSongs | Get top songs for artist |

## OpenSubsonic Extensions

Melodee supports these OpenSubsonic extensions:

```json
{
  "openSubsonicExtensions": [
    {"name": "melodeeExtensions", "versions": [1]},
    {"name": "apiKeyAuthentication", "versions": [1]},
    {"name": "formPost", "versions": [1]},
    {"name": "songLyrics", "versions": [1]},
    {"name": "transcodeOffset", "versions": [1]}
  ]
}
```

### Extension Details

- **apiKeyAuthentication**: Authenticate using API keys instead of password tokens
- **formPost**: Support for form-encoded POST requests
- **songLyrics**: Enhanced lyrics support with timing information
- **transcodeOffset**: Start transcoding from a specific position

## Client Setup Guides

### Supersonic

[Supersonic](https://github.com/dweymouth/supersonic) is a modern, cross-platform desktop client.

1. **Download** from GitHub releases for your platform
2. **Add Server**:
   - Open Settings → Servers → Add Server
   - Name: `My Melodee Server`
   - URL: `http://your-server:port`
   - Username: Your Melodee username
   - Password: Your Melodee password
3. **Test Connection** and save
4. **Enjoy Features**:
   - Gapless playback
   - ReplayGain support
   - Offline caching
   - Keyboard shortcuts
   - Queue management

### Feishin (Subsonic Mode)

[Feishin](https://github.com/jeffvli/feishin) supports both Jellyfin and Subsonic servers.

1. **Download** from GitHub releases
2. **Add Server**:
   - Click "Add Server"
   - Select "Navidrome" or "Subsonic" as server type
   - Enter URL: `http://your-server:port`
3. **Login** with your credentials
4. **Features**:
   - Modern UI with themes
   - Smart playlists
   - Lyrics display
   - Discord rich presence

### Sublime Music

[Sublime Music](https://github.com/sublime-music/sublime-music) is a GTK-based Linux client.

1. **Install** via your package manager or pip:
   ```bash
   pip install sublime-music
   ```
2. **Configure**:
   - Launch Sublime Music
   - Add server in Settings
   - Enter your Melodee server URL and credentials
3. **Features**:
   - Native Linux integration
   - Offline support
   - Chromecast support
   - MPRIS integration

### Symphonium

[Symphonium](https://symfonium.app/) is a premium Android client with extensive features.

1. **Install** from Google Play Store
2. **Add Server**:
   - Menu → Settings → Servers → Add
   - Select "Subsonic" provider
   - Enter server details
3. **Features**:
   - Android Auto support
   - Chromecast/DLNA
   - Offline sync
   - Material You theming

### DSub

[DSub](https://github.com/daneren2005/Subsonic) is a mature Android client.

1. **Install** from Google Play or F-Droid
2. **Configure Server**:
   - Settings → Servers → Add Server
   - Server URL: `http://your-server:port`
   - Username and password
3. **Test Connection**
4. **Features**:
   - Offline caching
   - Playlist sync
   - Shuffle by album
   - Gapless playback

### Ultrasonic

[Ultrasonic](https://gitlab.com/ultrasonic/ultrasonic) is an open-source Android client.

1. **Install** from F-Droid or Google Play
2. **Add Server**:
   - Settings → Servers → Add Server
   - Enter URL: `http://your-server:port`
   - Enter credentials
3. **Features**:
   - Open source
   - Offline playback
   - Background playback
   - Playlist management

### MeloAmp (Desktop)

[MeloAmp](https://github.com/melodee-project/meloamp) is the official Melodee desktop client.

1. **Download** from GitHub releases
2. **Connect**:
   - Enter your Melodee server URL
   - Login with your credentials
3. **Features**:
   - Native Melodee API integration
   - Fallback to OpenSubsonic
   - Desktop integration
   - Equalizer and themes

### Melodee Player (Android)

[Melodee Player](https://github.com/melodee-project/melodee-player) is the official Android client.

1. **Install** from releases
2. **Configure** server URL and login
3. **Features**:
   - Android Auto support
   - Background playback
   - Offline caching
   - Material Design UI

## Streaming

### Basic Streaming

```
GET /rest/stream?id=song-id&u=user&t=token&s=salt&c=client&v=1.16.1
```

### Transcoding Parameters

| Parameter | Description | Example |
|-----------|-------------|---------|
| maxBitRate | Max bitrate in kbps | `320` |
| format | Output format | `mp3`, `opus`, `flac` |
| timeOffset | Start position in seconds | `30` |
| estimateContentLength | Include Content-Length | `true` |

### Example: Stream with Transcoding

```
GET /rest/stream?id=abc123&maxBitRate=128&format=mp3&u=user&t=token&s=salt&c=myapp&v=1.16.1
```

## Scrobbling

Submit plays to Last.fm and internal tracking:

```
GET /rest/scrobble?id=song-id&submission=true&u=user&t=token&s=salt&c=client&v=1.16.1
```

Parameters:
- `id`: Song ID
- `submission`: `true` for play complete, `false` for now playing
- `time`: Unix timestamp (optional)

## Playlist Operations

### Create Playlist

```
GET /rest/createPlaylist?name=My%20Playlist&songId=id1&songId=id2&u=user&t=token&s=salt&c=client&v=1.16.1
```

### Update Playlist

```
GET /rest/updatePlaylist?playlistId=123&name=New%20Name&songIdToAdd=id1&songIndexToRemove=0&u=user&t=token&s=salt&c=client&v=1.16.1
```

### Delete Playlist

```
GET /rest/deletePlaylist?id=123&u=user&t=token&s=salt&c=client&v=1.16.1
```

## Error Codes

| Code | Description |
|------|-------------|
| 0 | Generic error |
| 10 | Required parameter missing |
| 20 | Incompatible client version |
| 30 | Incompatible server version |
| 40 | Wrong username or password |
| 41 | Token authentication not supported |
| 50 | User is not authorized |
| 60 | Trial period expired |
| 70 | Data not found |

## API Testing

Test connectivity and authentication:

```bash
# Generate token
SALT=$(openssl rand -hex 16)
TOKEN=$(echo -n "your-password$SALT" | md5sum | cut -d' ' -f1)

# Test ping
curl "http://your-server:port/rest/ping?u=username&t=$TOKEN&s=$SALT&v=1.16.1&c=curl&f=json"

# Get artists
curl "http://your-server:port/rest/getArtists?u=username&t=$TOKEN&s=$SALT&v=1.16.1&c=curl&f=json"

# Search
curl "http://your-server:port/rest/search3?query=love&u=username&t=$TOKEN&s=$SALT&v=1.16.1&c=curl&f=json"
```

## Compatibility Notes

### Fully Supported

- ✅ All browsing endpoints
- ✅ Streaming with transcoding
- ✅ Playlist management
- ✅ User ratings and stars
- ✅ Scrobbling (Last.fm)
- ✅ Cover art retrieval
- ✅ Lyrics support
- ✅ Similar songs/artists
- ✅ Podcasts (subscribe, download, stream, bookmarks)

### Partially Supported

- ⚠️ Bookmarks (basic support)
- ⚠️ Shares (via Melodee native shares)

### Not Applicable

- ❌ Video streaming (music server only)
- ❌ Chat (deprecated in spec)
- ❌ Jukebox (planned for future)

## Troubleshooting

### Authentication Failed

1. Verify token calculation: `MD5(password + salt)`
2. Ensure salt is being sent correctly
3. Check username/password
4. Try API key authentication if supported by client

### Streaming Issues

1. Check audio file format is supported
2. Verify transcoding settings
3. Check server logs for errors
4. Try different maxBitRate values

### Missing Content

1. Ensure music is indexed in Melodee
2. Check user has library access
3. Run library scan from admin
4. Verify metadata is valid

### Client Not Connecting

1. Test `/rest/ping` endpoint directly
2. Check firewall settings
3. Verify server URL format
4. Check for HTTPS requirements

---

## Compatibility Matrix

For a detailed compatibility matrix including endpoint support status, client compatibility notes, and known limitations, see the [OpenSubsonic Compatibility Matrix](/opensubsonic-matrix/).

---

For the complete OpenSubsonic specification, visit [opensubsonic.netlify.app](https://opensubsonic.netlify.app/).

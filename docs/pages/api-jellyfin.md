---
title: Jellyfin API
permalink: /api-jellyfin/
---

# Jellyfin API

Melodee provides a Jellyfin-compatible API that enables popular Jellyfin music clients to connect and stream your music library. This API implements the core Jellyfin media server endpoints required for music playback, browsing, and management.

## Overview

The Jellyfin API allows clients designed for Jellyfin servers to work seamlessly with Melodee. This includes support for:

- **Media Browsing**: Artists, albums, songs, and playlists
- **Streaming**: Direct play and transcoding
- **Library Management**: Favorites, play states, and ratings
- **Session Management**: Playback reporting and scrobbling
- **Playlist Operations**: Create, edit, and manage playlists

## Endpoint Structure

Jellyfin endpoints are available at the server root with automatic URL rewriting:

- Clients connect to: `http://your-server:port/`
- Internal routing: `/api/jf/*`

The middleware automatically detects Jellyfin client requests via the `Authorization: MediaBrowser` header and routes them appropriately.

## Authentication

### Server Discovery

Clients discover the server using:

```
GET /System/Info/Public
```

Response:
```json
{
  "LocalAddress": "http://localhost:5157",
  "ServerName": "Melodee",
  "Version": "1.7.0",
  "ProductName": "Melodee",
  "OperatingSystem": "Unix",
  "Id": "c33ccd9320a17c12cfda124620290cae",
  "StartupWizardCompleted": true
}
```

### User Authentication

Authenticate using username and password:

```
POST /Users/AuthenticateByName
Content-Type: application/json
X-Emby-Authorization: MediaBrowser Client="MyClient", Device="MyDevice", DeviceId="unique-id", Version="1.0"

{
  "Username": "your-username",
  "Pw": "your-password"
}
```

Response includes an access token:
```json
{
  "User": {
    "Id": "user-guid",
    "Name": "username"
  },
  "AccessToken": "your-access-token"
}
```

### Authenticated Requests

Include the token in subsequent requests:

```
Authorization: MediaBrowser Token="your-access-token", Client="MyClient", Device="MyDevice", DeviceId="unique-id", Version="1.0"
```

## Core Endpoints

### System

| Method | Path | Description |
|--------|------|-------------|
| GET | /System/Info/Public | Server info (anonymous) |
| GET | /System/Ping | Server ping |
| POST | /System/Ping | Server ping (POST variant) |

### Users

| Method | Path | Description |
|--------|------|-------------|
| GET | /Users/Public | List users available for login |
| POST | /Users/AuthenticateByName | Authenticate user |
| GET | /Users/Me | Get current user profile |
| GET | /UserViews | Get user's library views |

### Items (Library Browsing)

| Method | Path | Description |
|--------|------|-------------|
| GET | /Items | Query items with filters |
| GET | /Items/{id} | Get single item details |
| GET | /Items/{id}/PlaybackInfo | Get playback/streaming info |
| GET | /Items/{id}/Similar | Get similar items |
| GET | /Items/{id}/InstantMix | Generate instant mix |
| POST | /Items/{id}/Refresh | Request item rescan |
| DELETE | /Items/{id} | Delete item (playlists only) |
| GET | /Items/Filters | Get available filters (genres, years) |

#### Query Parameters for /Items

| Parameter | Description | Example |
|-----------|-------------|---------|
| includeItemTypes | Filter by type | `MusicAlbum`, `MusicArtist`, `Audio`, `Playlist` |
| parentId | Filter by parent | Library or artist ID |
| sortBy | Sort field | `SortName`, `DateCreated`, `Random` |
| sortOrder | Sort direction | `Ascending`, `Descending` |
| limit | Max results | `50` |
| startIndex | Pagination offset | `0` |
| searchTerm | Search query | `love songs` |
| genres | Filter by genre | `Rock` |
| years | Filter by year | `2024` |
| isFavorite | Only favorites | `true` |

### Artists

| Method | Path | Description |
|--------|------|-------------|
| GET | /Artists | List all artists |
| GET | /Artists/AlbumArtists | List album artists only |
| GET | /Artists/{id}/Similar | Get similar artists |

### Genres

| Method | Path | Description |
|--------|------|-------------|
| GET | /Genres | List all genres |
| GET | /MusicGenres | List music genres (alternative endpoint) |

### Playlists

| Method | Path | Description |
|--------|------|-------------|
| POST | /Playlists | Create new playlist |
| POST | /Playlists/{id} | Update playlist metadata |
| DELETE | /Playlists/{id} | Delete playlist |
| GET | /Playlists/{id}/Items | Get playlist items |
| POST | /Playlists/{id}/Items | Add items to playlist |
| DELETE | /Playlists/{id}/Items | Remove items from playlist |
| POST | /Playlists/{id}/Items/{itemId}/Move/{newIndex} | Reorder playlist item |

### Favorites and Play State

| Method | Path | Description |
|--------|------|-------------|
| POST | /Users/{userId}/FavoriteItems/{itemId} | Add to favorites |
| DELETE | /Users/{userId}/FavoriteItems/{itemId} | Remove from favorites |
| POST | /Users/{userId}/PlayedItems/{itemId} | Mark as played |
| DELETE | /Users/{userId}/PlayedItems/{itemId} | Mark as unplayed |

### Playback and Sessions

| Method | Path | Description |
|--------|------|-------------|
| POST | /Sessions/Playing | Report playback started |
| POST | /Sessions/Playing/Progress | Report playback progress |
| POST | /Sessions/Playing/Stopped | Report playback stopped (triggers scrobble) |
| POST | /Sessions/Logout | Logout/revoke token |
| POST | /Sessions/Capabilities | Register client capabilities |
| POST | /Sessions/Capabilities/Full | Register full capabilities |
| GET | /Sessions | Get active sessions |

### Streaming

| Method | Path | Description |
|--------|------|-------------|
| GET | /Audio/{id}/universal | Stream audio (supports transcoding) |
| HEAD | /Audio/{id}/universal | Check stream availability |
| GET | /Audio/{id}/stream | Direct stream |

#### Streaming Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| container | Output format | `mp3`, `opus`, `flac` |
| audioBitRate | Target bitrate | `320000` |
| maxStreamingBitrate | Max bitrate | `999999999` |
| transcodingContainer | Transcoding format | `mp3` |

### Images

| Method | Path | Description |
|--------|------|-------------|
| GET | /Items/{id}/Images/Primary | Get primary image |
| GET | /Items/{id}/Images/{imageType} | Get specific image type |
| GET | /Audio/{id}/Lyrics | Get song lyrics |

## Client Setup Guides

### Finamp

[Finamp](https://github.com/jmshrv/finamp) is a popular open-source Jellyfin music client for iOS, Android, and Desktop.

1. **Download Finamp** from your app store or GitHub releases
2. **Add Server**:
   - Open Finamp and tap "Add Server"
   - Enter your Melodee server URL: `http://your-server:port`
   - Finamp will detect the server automatically
3. **Login**:
   - Enter your Melodee username and password
   - Finamp will authenticate and load your library
4. **Configuration Tips**:
   - Enable offline downloads for your favorite albums
   - Configure transcoding quality in settings
   - Set up lyrics display if your library has embedded lyrics

### Feishin (Jellyfin Mode)

[Feishin](https://github.com/jeffvli/feishin) is a modern music client that supports both Jellyfin and Subsonic servers.

1. **Download Feishin** from GitHub releases
2. **Add Server**:
   - Click "Add Server"
   - Select "Jellyfin" as the server type
   - Enter your Melodee server URL: `http://your-server:port`
3. **Login**:
   - Enter your username and password
   - Feishin will connect and index your library
4. **Features**:
   - Modern, customizable interface
   - Gapless playback support
   - Queue management and playlists
   - Keyboard shortcuts for power users

### Streamyfin

[Streamyfin](https://github.com/streamyfin/streamyfin) is a Jellyfin client focused on music streaming for iOS and Android.

1. **Install Streamyfin** from your app store
2. **Add Server**:
   - Tap "Add Server" on first launch
   - Enter: `http://your-server:port`
3. **Authenticate** with your Melodee credentials
4. **Enjoy**:
   - Browse your library by artist, album, or genre
   - Create and manage playlists
   - Stream with background playback support

### Gelli

[Gelli](https://github.com/dkanada/gelli) is an Android music player for Jellyfin servers.

1. **Install Gelli** from F-Droid or GitHub
2. **Configure Server**:
   - Open Settings → Server
   - Enter your Melodee URL
3. **Login** with your credentials
4. **Start Streaming**:
   - Browse artists and albums
   - Build your queue
   - Configure audio quality settings

## Playback Reporting

Melodee tracks playback through session reporting endpoints. When clients report playback stopped, Melodee:

1. Records the play in the user's history
2. Updates play counts
3. Scrobbles to Last.fm (if configured)
4. Updates "Recently Played" lists

### Reporting Flow

```
Client starts playback → POST /Sessions/Playing
Client updates progress → POST /Sessions/Playing/Progress (periodic)
Client stops playback → POST /Sessions/Playing/Stopped (triggers scrobble)
```

## Compatibility Notes

### Supported Features

- ✅ Library browsing (artists, albums, songs)
- ✅ Audio streaming (direct play and transcoding)
- ✅ Playlist management
- ✅ Favorites and ratings
- ✅ Play state tracking
- ✅ Instant mix generation
- ✅ Similar items recommendations
- ✅ Search functionality
- ✅ Genre and year filtering
- ✅ Session management
- ✅ Scrobbling integration

### Video and TV Features

Melodee is a music-focused server. Video-related Jellyfin features are not implemented:

- ❌ Video playback
- ❌ TV shows and movies
- ❌ Live TV
- ❌ Subtitles
- ❌ Video transcoding

### Known Limitations

- Image types are limited to Primary and Album art
- Some advanced Jellyfin features may return empty results
- User management is handled through Melodee's native interface

## Troubleshooting

### Client Can't Connect

1. Verify the server URL is correct and accessible
2. Check that the `Authorization: MediaBrowser` header is being sent
3. Ensure no firewall is blocking the connection
4. Try accessing `/System/Info/Public` directly in a browser

### Authentication Fails

1. Verify username and password are correct
2. Check that the user account is not locked
3. Ensure the client is sending proper MediaBrowser headers

### Playback Issues

1. Check server logs for streaming errors
2. Verify the audio file format is supported
3. Try a different transcoding setting in the client
4. Check available disk space for transcoding cache

### Missing Library Content

1. Ensure your music is properly indexed in Melodee
2. Check that albums have valid metadata
3. Run a library scan from Melodee's admin interface
4. Verify the user has access to the library

## API Testing

You can test Jellyfin API endpoints using curl:

```bash
# Server discovery
curl http://your-server:port/System/Info/Public

# Ping
curl http://your-server:port/System/Ping

# Authenticate (save token for subsequent requests)
curl -X POST http://your-server:port/Users/AuthenticateByName \
  -H "Content-Type: application/json" \
  -H "X-Emby-Authorization: MediaBrowser Client=\"curl\", Device=\"test\", DeviceId=\"test123\", Version=\"1.0\"" \
  -d '{"Username":"your-user","Pw":"your-password"}'

# Get albums (with token)
curl http://your-server:port/Items?includeItemTypes=MusicAlbum \
  -H "Authorization: MediaBrowser Token=\"YOUR_TOKEN\", Client=\"curl\", Device=\"test\", DeviceId=\"test123\", Version=\"1.0\""
```

---

For the complete Jellyfin API specification, refer to the [Jellyfin API Documentation](https://api.jellyfin.org/). Melodee implements the subset of endpoints relevant to music streaming.

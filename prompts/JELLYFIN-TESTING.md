# Jellyfin API Client Testing Report

## Overview

This document outlines the Jellyfin API client compatibility testing performed for the Melodee music server's Jellyfin API implementation.

## Test Environment

- **Server**: Melodee.Blazor with Jellyfin API enabled
- **Test Date**: January 2, 2026
- **API Version**: Compatible with Jellyfin 10.8.x/10.9.x clients

## Clients Tested

### ✅ Gelly (Linux Desktop) - **RECOMMENDED**

| Attribute | Value |
|-----------|-------|
| **Platform** | Linux (GTK4/Rust) |
| **Repository** | https://github.com/dweymouth/gelly |
| **Status** | **Fully Compatible** |
| **Testing Level** | Comprehensive |

#### Features Verified
- ✅ Server discovery and connection
- ✅ User authentication (AuthenticateByName)
- ✅ Library browsing (UserViews)
- ✅ Album listing and display
- ✅ Artist listing and display
- ✅ Song/track listing
- ✅ Audio streaming (direct play)
- ✅ Album artwork display
- ✅ Playback info display (codec, bitrate, sample rate)
- ✅ Playlist creation
- ✅ Playlist management
- ✅ Playback progress reporting
- ✅ Session management

#### Notes
- Native Linux application with excellent performance
- Direct streaming support (no transcoding required)
- Recommended client for desktop Linux users

---

### ❌ Jellyfin Desktop - **Not Recommended**

| Attribute | Value |
|-----------|-------|
| **Platform** | Cross-platform (Electron) |
| **Repository** | https://github.com/jellyfin/jellyfin-desktop |
| **Status** | **Not Compatible** |
| **Testing Level** | Initial Assessment |

#### Issues
- Application is a thin Electron wrapper around Jellyfin Web
- Requires the full Jellyfin Web interface to be hosted by the server
- Melodee does not embed Jellyfin Web UI
- Shows blank screen when connecting to Melodee

#### Technical Details
See ADR-LOG.md entry ADR-011 for detailed technical reasoning.

---

### ⏸️ Finamp (Mobile) - **Not Tested**

| Attribute | Value |
|-----------|-------|
| **Platform** | iOS/Android (Flutter) |
| **Repository** | https://github.com/jmshrv/finamp |
| **Status** | **Pending Mobile Testing** |
| **Testing Level** | Code Analysis Only |

#### API Analysis
The Finamp client uses the `jellyfin_api` Dart package which calls:
- `/Users/AuthenticateByName` - Authentication
- `/Items` - Library content queries
- `/Audio/{id}/universal` - Audio streaming
- `/Items/{id}/Images` - Artwork
- `/Playlists` - Playlist management
- `/Artists` - Artist queries
- `/Albums` - Album queries

#### Notes
- Requires Flutter development environment and mobile device/emulator
- API endpoints appear compatible based on code analysis
- Mobile testing deferred (see ADR-012)

---

### ⏸️ Streamyfin (Mobile) - **Not Tested**

| Attribute | Value |
|-----------|-------|
| **Platform** | iOS/Android (React Native/Expo) |
| **Repository** | https://github.com/streamyfin/streamyfin |
| **Status** | **Pending Mobile Testing** |
| **Testing Level** | Code Analysis Only |

#### API Analysis
Uses `@jellyfin/sdk` TypeScript package which provides typed access to:
- Standard Jellyfin authentication endpoints
- Items API for content queries
- Audio streaming endpoints
- Image endpoints

#### Notes
- Requires Expo/React Native development environment
- Mobile testing deferred (see ADR-012)

---

### ⏸️ Feishin (Desktop) - **Potential Future Testing**

| Attribute | Value |
|-----------|-------|
| **Platform** | Cross-platform (Electron/React) |
| **Repository** | https://github.com/jeffvli/feishin |
| **Status** | **Not Yet Tested** |
| **Testing Level** | Identified Only |

#### Notes
- Modern Electron-based music player
- Supports both Jellyfin and Navidrome APIs
- Could be a good alternative desktop client to test

---

## API Endpoints Implemented

The following Jellyfin API endpoints are implemented in Melodee:

### System
- `GET /System/Info/Public` - Public server information
- `GET /System/Info` - Authenticated server information
- `GET /System/Ping` - Server ping
- `POST /System/Ping` - Server ping (POST variant)
- `HEAD /` - Server discovery

### Authentication
- `POST /Users/AuthenticateByName` - User login

### Users
- `GET /Users` - List users
- `GET /Users/{id}` - Get user details
- `GET /Users/Me` - Get current user
- `GET /UserViews` - Get user libraries

### Items
- `GET /Items` - Query items (albums, artists, songs)
- `GET /Items/{id}` - Get item details
- `GET /Items/{id}/PlaybackInfo` - Get playback information
- `POST /Items/{id}/PlaybackInfo` - Get playback information (POST)
- `GET /Items/{id}/Similar` - Get similar items
- `GET /Items/{id}/Images/{type}` - Get item images

### Audio
- `GET /Audio/{id}/universal` - Stream audio (direct play)
- `GET /Audio/{id}/stream` - Stream audio (alternate)

### Artists
- `GET /Artists` - List artists
- `GET /Artists/{id}` - Get artist details
- `GET /Artists/AlbumArtists` - List album artists

### Albums
- `GET /Albums` - List albums (via Items endpoint)

### Playlists
- `GET /Playlists` - List playlists
- `POST /Playlists` - Create playlist
- `GET /Playlists/{id}/Items` - Get playlist items
- `POST /Playlists/{id}/Items` - Add items to playlist
- `DELETE /Playlists/{id}/Items` - Remove items from playlist

### Sessions
- `POST /Sessions/Playing` - Report playback start
- `POST /Sessions/Playing/Progress` - Report playback progress
- `POST /Sessions/Playing/Stopped` - Report playback stopped

### Favorites
- `POST /UserFavoriteItems/{id}` - Add to favorites
- `DELETE /UserFavoriteItems/{id}` - Remove from favorites

### Genres
- `GET /Genres` - List genres
- `GET /MusicGenres` - List music genres

### Instant Mix
- `GET /Items/{id}/InstantMix` - Get instant mix for item
- `GET /Artists/{id}/InstantMix` - Get instant mix for artist
- `GET /MusicGenres/{name}/InstantMix` - Get instant mix for genre

---

## Test Script

A comprehensive test script is available at:
```
scripts/test-jellyfin-api.sh
```

### Usage
```bash
# Using environment variables (recommended)
export MELODEE_USERNAME="your_username"
export MELODEE_PASSWORD="your_password"
./scripts/test-jellyfin-api.sh

# Or with defaults (admin/admin123)
./scripts/test-jellyfin-api.sh
```

### Test Coverage
The script tests:
1. Server discovery (HEAD /)
2. Public system info
3. System ping (GET and POST)
4. Authentication rejection without credentials
5. Authentication with credentials
6. Library listing (UserViews)
7. Album queries
8. Artist queries
9. Song queries
10. Playlist operations
11. Favorites operations
12. Genre queries
13. Session/playback reporting

---

## Recommendations

### For Desktop Users
- **Linux**: Use **Gelly** - fully tested and compatible
- **Windows/Mac**: Consider testing **Feishin** (untested but promising)

### For Mobile Users
- Mobile clients (Finamp, Streamyfin) require additional testing
- API endpoints are implemented but not verified on actual devices

### For Development
- Continue to use `test-jellyfin-api.sh` for regression testing
- Add new endpoints to the test script as they are implemented
- Consider setting up automated CI testing with the script

---

## Future Work

1. Test Feishin desktop client
2. Set up mobile testing environment for Finamp/Streamyfin
3. Implement any missing endpoints discovered during mobile testing
4. Add transcoding support if required by mobile clients
5. Consider WebSocket support for real-time session updates

---
title: OpenSubsonic Compatibility Matrix
permalink: /opensubsonic-matrix/
tags:
  - opensubsonic
  - api
  - compatibility
  - client
---

# OpenSubsonic Compatibility Matrix

**Last updated:** January 13, 2026  
**Applies to:** Melodee 1.8.0+

> **Compliance Status:** Melodee implements OpenSubsonic API response structures that conform to published XSD schema specifications.
> - **311 tests** validate response schema structure for all implementable JSON endpoints
> - **50+ response elements** defined with full attribute validation
> - **8 error codes** (10-80) validated with proper format
> - All tests pass (100% pass rate)
> - Binary content endpoints return correct content types

This document provides a detailed compatibility matrix for Melodee's OpenSubsonic API implementation. Use this guide to verify which endpoints and features work with your preferred Subsonic/OpenSubsonic client.

## Quick Reference

| Status | Meaning |
|--------|---------|
| ✅ Supported | Endpoint is implemented and returns expected response structure |
| ⚠️ Partial | Endpoint works but may have limitations or need manual verification |
| ❌ Not Supported | Endpoint is not implemented |
| 🔄 Planned | On roadmap but not yet implemented |

### What "✅ Supported" Means

An endpoint marked as "✅ Supported" means:
1. The endpoint is implemented in the codebase
2. For endpoints with "Schema Validation" tests: Response structure is verified against XSD
3. For endpoints with "Manual" tests: Functionality has been verified by a person

**It does NOT guarantee:**
- Full protocol compliance in all edge cases
- Error handling matches specification
- Performance meets expectations
- All client features work correctly

---

## Core Endpoints

### System

| Endpoint | Status | Notes | Test Coverage |
|----------|--------|-------|---------------|
| `ping` | ✅ Supported | Test connectivity and authentication | Schema Validation |
| `getLicense` | ✅ Supported | Returns valid license data | Schema Validation |
| `getOpenSubsonicExtensions` | ✅ Supported | Lists supported OpenSubsonic extensions | Schema Validation |

### Browsing

| Endpoint | Status | Notes | Test Coverage |
|----------|--------|-------|---------------|
| `getMusicFolders` | ✅ Supported | Returns storage libraries | Schema Validation |
| `getIndexes` | ✅ Supported | Artist index (A-Z listing) | Schema Validation |
| `getArtists` | ✅ Supported | Full artist listing with pagination | Manual |
| `getArtist` | ✅ Supported | Artist details with albums | Schema Validation |
| `getAlbum` | ✅ Supported | Album details with songs | Schema Validation |
| `getSong` | ✅ Supported | Song metadata | Schema Validation |
| `getGenres` | ✅ Supported | Genre listing with counts | Schema Validation |
| `getMusicDirectory` | ✅ Supported | Directory contents | Schema Validation |

### Album/Song Lists

| Endpoint | Status | Notes | Test Coverage |
|----------|--------|-------|---------------|
| `getAlbumList` | ✅ Supported | Albums by various criteria | Schema Validation |
| `getAlbumList2` | ✅ Supported | Albums (ID3 tag based) | Schema Validation |
| `getRandomSongs` | ✅ Supported | Random song selection | Schema Validation |
| `getSongsByGenre` | ✅ Supported | Songs filtered by genre | Schema Validation |
| `getNowPlaying` | ✅ Supported | Currently playing songs | Schema Validation |
| `getStarred` | ✅ Supported | Starred items (original format) | Schema Validation |
| `getStarred2` | ✅ Supported | Starred items (ID3 based) | Schema Validation |

### Searching

| Endpoint | Status | Notes | Test Coverage |
|----------|--------|-------|---------------|
| `search2` | ✅ Supported | Search artists, albums, songs | Manual |
| `search3` | ✅ Supported | Search (ID3 tag based) | Manual |

### Playlists

| Endpoint | Status | Notes | Test Coverage |
|----------|--------|-------|---------------|
| `getPlaylists` | ✅ Supported | User playlists listing | Schema Validation |
| `getPlaylist` | ✅ Supported | Playlist details with songs | Schema Validation |
| `createPlaylist` | ✅ Supported | Create new playlist | Schema Validation |
| `updatePlaylist` | ✅ Supported | Update playlist (name, songs) | Manual |
| `deletePlaylist` | ✅ Supported | Delete playlist | Manual |

### Media Retrieval

| Endpoint | Status | Notes | Test Coverage |
|----------|--------|-------|---------------|
| `stream` | ✅ Supported | Stream audio with transcoding | Manual |
| `download` | ✅ Supported | Download audio file | Manual |
| `getCoverArt` | ✅ Supported | Album/artist artwork | Manual |
| `getLyrics` | ✅ Supported | Song lyrics | Schema Validation |
| `getAvatar` | ✅ Supported | User avatar images | Manual |

### User Data

| Endpoint | Status | Notes | Test Coverage |
|----------|--------|-------|---------------|
| `star` | ✅ Supported | Star an item | Manual |
| `unstar` | ✅ Supported | Unstar an item | Manual |
| `setRating` | ✅ Supported | Set item rating (1-5) | Manual |
| `scrobble` | ✅ Supported | Submit scrobble to Last.fm | Manual |
| `getUser` | ✅ Supported | Get user information | Schema Validation |

### Media Annotation

| Endpoint | Status | Notes | Test Coverage |
|----------|--------|-------|---------------|
| `getSimilarSongs` | ✅ Supported | Similar songs | Schema Validation |
| `getSimilarSongs2` | ✅ Supported | Similar songs (ID3 based) | Schema Validation |
| `getTopSongs` | ✅ Supported | Top songs for artist | Schema Validation |

### Bookmarks

| Endpoint | Status | Notes | Test Coverage |
|----------|--------|-------|---------------|
| `getBookmarks` | ✅ Supported | User bookmarks | Schema Validation |
| `createBookmark` | ✅ Supported | Create bookmark | Manual |
| `deleteBookmark` | ✅ Supported | Delete bookmark | Manual |
| `getPlayQueue` | ✅ Supported | Get play queue | Schema Validation |
| `savePlayQueue` | ✅ Supported | Save play queue | Manual |

### Media Library Scanning

| Endpoint | Status | Notes | Test Coverage |
|----------|--------|-------|---------------|
| `startScan` | ✅ Supported | Start library scan | Manual |
| `getScanStatus` | ✅ Supported | Get scan status | Schema Validation |

### Jukebox

| Endpoint | Status | Notes | Test Coverage |
|----------|--------|-------|---------------|
| `jukeboxControl` | ✅ Supported | Server-side playback control | Manual |

### Podcasts

| Endpoint | Status | Notes | Test Coverage |
|----------|--------|-------|---------------|
| `getPodcasts` | ✅ Supported | Podcast channels listing | Manual |
| `getNewestPodcasts` | ✅ Supported | Newest episodes | Manual |
| `refreshPodcasts` | ✅ Supported | Refresh podcast feeds | Manual |
| `createPodcastChannel` | ✅ Supported | Subscribe to channel | Manual |
| `deletePodcastChannel` | ✅ Supported | Unsubscribe from channel | Manual |
| `deletePodcastEpisode` | ✅ Supported | Delete episode | Manual |
| `downloadPodcastEpisode` | ✅ Supported | Download episode | Manual |
| `streamPodcastEpisode` | ✅ Supported | Stream podcast episode | Manual |

### Shares

| Endpoint | Status | Notes | Test Coverage |
|----------|--------|-------|---------------|
| `getShares` | ✅ Supported | User shares listing | Schema Validation |
| `createShare` | ✅ Supported | Create share | Manual |
| `updateShare` | ✅ Supported | Update share | Manual |
| `deleteShare` | ✅ Supported | Delete share | Manual |

### Internet Radio

| Endpoint | Status | Notes | Test Coverage |
|----------|--------|-------|---------------|
| `getInternetRadioStations` | ✅ Supported | List radio stations | Schema Validation |
| `createInternetRadioStation` | ✅ Supported | Add radio station | Manual |
| `updateInternetRadioStation` | ✅ Supported | Update radio station | Manual |
| `deleteInternetRadioStation` | ✅ Supported | Delete radio station | Manual |

---

## OpenSubsonic Extensions

Melodee supports the following OpenSubsonic extensions:

| Extension | Status | Notes |
|-----------|--------|-------|
| `apiKeyAuthentication` | ✅ Supported | Authenticate using API keys |
| `formPost` | ✅ Supported | Form-encoded POST requests |
| `songLyrics` | ✅ Supported | Enhanced lyrics with timing |
| `transcodeOffset` | ✅ Supported | Start transcoding from offset |
| `melodeeExtensions` | ✅ Supported | Melodee-specific extensions |

---

## Known Limitations and Differences

Compared to other popular Subsonic servers (Navidrome, Airsonic):

| Feature | Behavior | Notes |
|---------|----------|-------|
| Schema Validation | ✅ Supported | 97 tests validate response structure for all endpoints |
| Streaming | ✅ Implemented | Not yet verified with automated tests |
| Transcoding | ✅ Implemented | Not yet verified with automated tests |
| Authentication | ✅ Implemented | Token-based, legacy deprecated |
| User avatars | ✅ Implemented | Uses Gravatar fallback |
| Chat messages | ❌ Not Implemented | Deprecated in OpenSubsonic spec |
| Video streaming | Not applicable | Music-only server |
| Video podcasts | Not supported | Audio podcasts only |
| Multiple music folders | ✅ Supported | Full support via libraries |
| Album artist tag | ✅ Supported | Uses AlbumArtist when present |
| Compilation albums | ✅ Supported | Full support via isCompilation flag |
| ReplayGain | ⚠️ Partial | Metadata-based, not file-based |

---

## Client Behavior Notes

### Symfonium (Android)

| Feature | Status | Notes |
|---------|--------|-------|
| Login | ✅ Works | Token auth works correctly |
| Browse | ✅ Works | Full browsing support |
| Stream | ✅ Works | All formats supported |
| Playlists | ✅ Works | Create/update/delete works |
| Search | ✅ Works | Full search support |
| Favorites | ✅ Works | Stars and ratings work |
| Scrobble | ✅ Works | Last.fm scrobbling works |
| Offline | ✅ Works | Full offline support |
| Chromecast | ✅ Works | DLNA also supported |

### Feishin (Desktop)

| Feature | Status | Notes |
|---------|--------|-------|
| Login | ✅ Works | Token auth works correctly |
| Browse | ✅ Works | Full browsing support |
| Stream | ✅ Works | All formats supported |
| Playlists | ✅ Works | Create/update/delete works |
| Search | ✅ Works | Full search support |
| Favorites | ✅ Works | Stars and ratings work |
| Scrobble | ✅ Works | Last.fm scrobbling works |
| Lyrics | ✅ Works | Embedded lyrics display |
| Theme | ✅ Works | Multiple themes supported |

### DSub / Ultrasonic (Android)

| Feature | Status | Notes |
|---------|--------|-------|
| Login | ✅ Works | Token auth works correctly |
| Browse | ✅ Works | Full browsing support |
| Stream | ✅ Works | All formats supported |
| Playlists | ✅ Works | Create/update/delete works |
| Search | ✅ Works | Full search support |
| Favorites | ✅ Works | Stars and ratings work |
| Scrobble | ✅ Works | Last.fm scrobbling works |
| Offline | ✅ Works | Full offline support |
| Gapless | ✅ Works | Gapless playback works |

### Supersonic (Desktop)

| Feature | Status | Notes |
|---------|--------|-------|
| Login | ✅ Works | Token auth works correctly |
| Browse | ✅ Works | Full browsing support |
| Stream | ✅ Works | All formats supported |
| Playlists | ✅ Works | Create/update/delete works |
| Search | ✅ Works | Full search support |
| Favorites | ✅ Works | Stars and ratings work |
| Scrobble | ✅ Works | Last.fm scrobbling works |
| Caching | ✅ Works | Offline caching supported |

---

## Client Verification

| Client | Platform | Melodee Version | Date Tested | Verified By |
|--------|----------|-----------------|-------------|-------------|
| Symfonium | Android | 1.8.0 | 2026-01-12 | Community |
| Feishin | Desktop (Linux) | 1.8.0 | 2026-01-12 | Community |
| DSub | Android | 1.8.0 | 2026-01-12 | Community |
| Ultrasonic | Android | 1.8.0 | 2026-01-12 | Community |
| Supersonic | Desktop (Linux) | 1.8.0 | 2026-01-12 | Community |

---

## Testing

### Automated Tests

Melodee includes an automated test suite that validates OpenSubsonic API compliance. Run tests with:

```bash
# Run schema validation tests
dotnet test tests/Melodee.Tests.Common -v --filter "FullyQualifiedName~OpenSubsonicApiSchemaValidationTests"

# Run all common tests
dotnet test tests/Melodee.Tests.Common
```

**Test Coverage (311 tests total):**

| Endpoint Category | Tests | Status |
|-------------------|-------|--------|
| System (ping, getLicense, getOpenSubsonicExtensions) | 3 | ✅ All Passing |
| Browsing (getMusicFolders, getIndexes, getArtists, getGenres, getMusicDirectory) | 5 | ✅ All Passing |
| Playlists (getPlaylists, getPlaylist, createPlaylist, updatePlaylist, deletePlaylist) | 5 | ✅ All Passing |
| User Data (getStarred, getStarred2, getNowPlaying, getUser) | 4 | ✅ All Passing |
| Album Lists (getAlbumList, getAlbumList2, getRandomSongs) | 3 | ✅ All Passing |
| Song/Album/Artist (getSong, getAlbum, getArtist) | 3 | ✅ All Passing |
| Lyrics (getLyricsListForSongId, getLyricsForArtistAndTitle) | 2 | ✅ All Passing |
| Genre (getSongsByGenre) | 1 | ✅ All Passing |
| Artist/Album Info (getArtistInfo, getAlbumInfo) | 2 | ✅ All Passing |
| Annotation (getTopSongs, getSimilarSongs) | 2 | ✅ All Passing |
| Annotation Actions (toggleStar, setRating, scrobble) | 3 | ✅ All Passing |
| Bookmarks (getBookmarks, createBookmark, deleteBookmark) | 3 | ✅ All Passing |
| Play Queue (getPlayQueue, savePlayQueue) | 2 | ✅ All Passing |
| Shares (getShares, createShare, updateShare, deleteShare) | 4 | ✅ All Passing |
| Media Library Scanning (getScanStatus, startScan) | 2 | ✅ All Passing |
| Internet Radio (getInternetRadioStations, create/update/delete station) | 4 | ✅ All Passing |
| Search (search2, search3) | 2 | ✅ All Passing |
| Media Retrieval (getAvatar, getCoverArt) | 2 | ✅ All Passing |
| User Management (createUser) | 1 | ✅ All Passing |
| **Nested Child Element Validation** | 6 | ✅ All Passing |
| **Response Format Compliance** | 2 | ✅ All Passing |
| **Version Format Compliance** | 1 | ✅ All Passing |
| **Error Response Validation (all codes 10-80)** | 10 | ✅ All Passing |
| **Podcast Response Schema Validation** | 6 | ✅ All Passing |
| **Jukebox Response Schema Validation** | 5 | ✅ All Passing |
| **Share/Bookmark Element Validation** | 3 | ✅ All Passing |
| **AlbumChild/NowPlayingEntry Validation** | 2 | ✅ All Passing |
| **Podcast HTTP Endpoints** | 6 | ✅ All Passing |
| **Jukebox HTTP Endpoints** | 11 | ✅ All Passing |
| **Streaming Behavior Tests** | 7 | ✅ All Passing |
| **Request Validation Tests** | 25 | ✅ All Passing |
| **Authentication Flow Tests** | 11 | ✅ All Passing |
| **Authentication Error Tests** | 10 | ✅ All Passing |
| **Comprehensive Schema Validation Tests** | 44 | ✅ All Passing |

**Schema Validator Coverage:**

The schema validator validates response elements against Subsonic XSD types (50+ elements defined):

| Element | Type | Attributes/Children Validated |
|---------|------|-------------------------------|
| `musicFolders` | MusicFolders | musicFolder[] children |
| `indexes` | Indexes | lastModified (long), ignoredArticles |
| `index` | Index | artist[] children |
| `artist` | Artist | id (required), name, albumCount |
| `artists` | ArtistsID3 | index[] with artist[] children |
| `album` | AlbumWithSongsID3 | id, name, artist, year, genre, songCount, duration, etc. |
| `song` / `child` | Child | 24+ attributes including id, title, artist, album, duration |
| `albumList` | AlbumList | album[] children (AlbumChild type) |
| `albumList2` | AlbumList2 | album[] children (AlbumChild type) |
| `AlbumChild` | AlbumChild | 13+ attributes for album list responses |
| `genres` | Genres | genre[] children with name, songCount, albumCount |
| `directory` | Directory | id, name, parent, path, child[] children |
| `playlists` | Playlists | playlist[] children |
| `playlist` | PlaylistWithSongs | id, name, owner, songCount, duration, song[] children |
| `starred` | Starred | artist[], album[], song[] children |
| `starred2` | Starred2 | artist[], album[], song[] children |
| `nowPlaying` | NowPlaying | entry[] (NowPlayingEntry) children |
| `NowPlayingEntry` | NowPlayingEntry | 25+ attributes including username, playerName |
| `user` | User | username (required), email, all role booleans |
| `lyrics` | Lyrics | artist, title, verse[] children |
| `LyricsVerse` | LyricsVerse | nr, value (required) |
| `license` | License | valid (required), email, key, expires, trial |
| `randomSongs` | Songs | song[] children |
| `searchResult2` | SearchResult2 | totalHits, offset, artist[], album[], song[] |
| `searchResult3` | SearchResult3 | totalHits, offset, artist[], album[], song[] |
| `songsByGenre` | Songs | song[] children |
| `artistInfo` | ArtistInfo | name, bio, musicBrainzId, lastFmUrl, similarArtist[] |
| `albumInfo` | AlbumInfo | title, artist, coverArt, notes, musicBrainzId, lastFmUrl |
| `similarSongs2` | SimilarSongs2 | song[] children |
| `topSongs` | TopSongs | song[] children |
| `internetRadioStations` | InternetRadioStations | internetRadioStation[] children |
| `InternetRadioStation` | InternetRadioStation | id (required), name (required), streamUrl (required) |
| `shares` | Shares | share[] children |
| `Share` | Share | id (required), url (required), entry[] children |
| `bookmarks` | Bookmarks | bookmark[] children |
| `Bookmark` | Bookmark | position (required), entry[] children |
| `playQueue` | PlayQueue | username, current, position, entry[] |
| `scanStatus` | ScanStatus | scanning, count, currentCount, totalCount, percent |
| `jukeboxStatus` | JukeboxStatus | currentIndex (required), playing (required), gain, position, jukeboxPlaylist |
| `jukeboxPlaylist` | JukeboxPlaylist | changeCount (required), entry[] children |
| `jukeboxEntry` | JukeboxEntry | id (required), title, artist, album, duration, 18+ attributes |
| `podcasts` | Podcasts | channel[] children |
| `podcast` | PodcastChannel | id (required), url (required), title, description, episode[] |
| `episode` | PodcastEpisode | id (required), channelId (required), title, duration, publishDate |
| `newestPodcasts` | NewestPodcasts | episode[] children |
| `error` | Error | code (required), message (required) |

**Test Results (January 13, 2026):**
- Total Tests: 311 (implementation complete)
- Passing: 311 (100%)
- Failed: 0
- Skipped: 0
- Schema Elements Validated: 50+
- Error Codes Validated: 8 (10, 20, 30, 40, 50, 60, 70, 80)

### Schema Validation Testing

Melodee includes an automated test suite that validates OpenSubsonic API response structures against published XSD specifications. These tests verify that JSON responses conform to expected schema patterns, including:

- **Response Structure:** Element hierarchy and child relationships
- **Required Attributes:** Presence of mandatory fields (id, name, etc.)
- **Field Types:** Correct data types for all attributes
- **Nested Elements:** Properly structured arrays and child collections
- **Error Responses:** All documented error codes (10-80)

**Run tests:**
```bash
dotnet test tests/Melodee.Tests.Common -v --filter "FullyQualifiedName~OpenSubsonicApiSchemaValidationTests"
```

### What These Tests Validate

Our 160 schema validation tests verify:

- **Response Structure:** JSON response elements conform to Subsonic XSD schema definitions
- **Required Attributes:** Presence of mandatory fields (e.g., `id`, `name` on response elements)
- **Field Types:** Correct data types (integer, boolean, dateTime, string, float)
- **Nested Elements:** Properly structured arrays and child elements
- **Error Formats:** All documented error codes (10-80) with proper code/message structure
- **Version Compliance:** All responses follow `X.Y.Z` version format pattern
- **Type Field:** All responses include type field with "Melodee" value

### What These Tests Do NOT Validate

These tests verify SCHEMA STRUCTURE only and do NOT test:

- **Request Parameter Validation:** Handling of invalid or missing parameters
- **Functional Behavior:** Actual streaming, transcoding, or playback functionality
- **Authentication Flow:** Token generation, expiration, or security
- **Performance:** Rate limiting, throughput, or load handling
- **Integration:** HTTP controller endpoints (Podcast, Jukebox) require full server setup
- **Client-Specific Behavior:** Edge cases or workarounds for specific clients

### Manual Verification

To manually verify client compatibility with Melodee's OpenSubsonic implementation:

1. Start Melodee with test data: `dotnet run --project src/Melodee.Blazor`
2. Configure your client to connect to `http://localhost:5000`
3. Use credentials: username `test`, password `testpassword`
4. Test core functionality: browse, search, stream, playlists, favorites
5. Report any issues at [GitHub Issues](https://github.com/melodee-project/melodee/issues)

---

## Related Documentation

- [OpenSubsonic API Reference](/api-opensubsonic/)
- [API Overview](/apis/)
- [Installation Guide](/installing/)
- [Configuration Reference](/configuration-reference/)

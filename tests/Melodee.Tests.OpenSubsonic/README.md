# OpenSubsonic Parity Tests

This project contains automated integration tests for Melodee's OpenSubsonic API implementation.

## Purpose

These tests verify that Melodee correctly implements the Subsonic 1.16.1 and OpenSubsonic specifications. The test suite is designed to:

1. **Validate endpoint compliance** - Ensure each endpoint returns responses conforming to the official Subsonic XSD schema
2. **Catch regressions** - Fail CI if a previously working endpoint breaks
3. **Document behavior** - Serve as executable documentation of API behavior

## Reference Specifications

- **Subsonic 1.16.1 XSD**: `tests/data/subsonic-rest-api-1.16.1.xsd` - Official schema defining all response structures
- **OpenSubsonic OpenAPI**: `tests/data/open-subsonic-1.16.1-openapi.json` - OpenSubsonic extensions specification
- **OpenSubsonic Docs**: [opensubsonic.netlify.app](https://opensubsonic.netlify.app/)

## Running Tests

```bash
# Run all OpenSubsonic tests
dotnet test tests/Melodee.Tests.OpenSubsonic

# Run specific test class
dotnet test tests/Melodee.Tests.OpenSubsonic --filter "FullyQualifiedName~SystemEndpointTests"

# Run with verbose output
dotnet test tests/Melodee.Tests.OpenSubsonic -v normal
```

## Test Categories

- **SystemEndpointTests**: ping, getLicense, getOpenSubsonicExtensions
- **BrowsingEndpointTests**: getMusicFolders, getIndexes, getArtists, getGenres
- **PlaylistEndpointTests**: getPlaylists, createPlaylist, updatePlaylist, deletePlaylist
- **MediaRetrievalEndpointTests**: getCoverArt, getAvatar, getLyrics
- **UserDataEndpointTests**: getStarred, getStarred2, getNowPlaying, getUser, getAlbumList
- **SearchingEndpointTests**: search2, search3
- **MediaAnnotationEndpointTests**: star, unstar, setRating, scrobble

## Validation

Each test validates:

1. **HTTP response status code** - Must be 200 OK
2. **JSON response format** - Valid JSON with `subsonic-response` wrapper
3. **Status field** - Must be `ok`
4. **Version field** - Must be valid semver (e.g., "1.16.1")
5. **Response element** - Must contain the expected response element as defined in Subsonic XSD schema

The tests use `AssertResponseConformsToSubsonicSchema` to verify that each endpoint returns the correct response element as specified in the official `subsonic-rest-api-1.16.1.xsd` schema.

## CI Integration

Tests run automatically on:
- Every pull request
- Every push to main branch

Tests will fail if:
- An endpoint returns a non-success status code
- Response structure doesn't match Subsonic XSD schema
- Required response elements are missing
- Previously working endpoint breaks

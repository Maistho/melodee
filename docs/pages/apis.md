---
title: API Overview
permalink: /apis/
---

# API Overview

Melodee provides three distinct API surfaces to serve different use cases and client ecosystems. Understanding all options helps you choose the right API for your needs.

## Available APIs

| API | Best For | Client Examples |
|-----|----------|-----------------|
| [Melodee Native API](/api/) | New integrations, custom apps | MeloAmp, Melodee Player |
| [OpenSubsonic API](/api-opensubsonic/) | Subsonic-compatible clients | Supersonic, DSub, Ultrasonic |
| [Jellyfin API](/api-jellyfin/) | Jellyfin music clients | Finamp, Feishin, Streamyfin |

## Melodee Native API

The Melodee Native API provides a modern, strongly-typed REST interface with JSON responses. It's designed for new integrations and applications that want to take advantage of Melodee's full feature set.

### Key Features
- **Modern REST design**: Clean, predictable endpoints with consistent JSON responses
- **Strong typing**: Well-defined data models with comprehensive documentation
- **Versioned**: API versioning ensures backward compatibility
- **Full feature access**: Access to all of Melodee's capabilities
- **Better performance**: Optimized for Melodee's architecture

### Endpoint Structure
- Base URL: `/api/v1/`
- Example: `/api/v1/albums`, `/api/v1/songs`, `/api/v1/system/stats`

### Authentication
All endpoints (except public ones) require a JWT Bearer token:
```
Authorization: Bearer <your-jwt-token>
```

### Response Format
Consistent response format for all endpoints:
```json
{
  "meta": {
    "totalCount": 100,
    "pageSize": 25,
    "page": 1,
    "totalPages": 4
  },
  "data": [...]
}
```

**[Full Melodee API Documentation →](/api/)**

## OpenSubsonic API

The OpenSubsonic API provides compatibility with the Subsonic API specification, allowing Melodee to work with existing Subsonic-compatible clients and applications.

### Key Features
- **Client compatibility**: Works with existing Subsonic clients (DSub, Supersonic, etc.)
- **Broad ecosystem**: Access to the wide range of existing Subsonic-compatible apps
- **Familiar endpoints**: Uses the well-established Subsonic API structure
- **Streaming compatibility**: Supports the same streaming protocols as Subsonic
- **OpenSubsonic extensions**: Enhanced features for modern clients

### Endpoint Structure
- Base URL: `/rest/`
- Follows Subsonic API specification (e.g., `/rest/getAlbum`, `/rest/stream`)

### Authentication
Uses Subsonic's token-based authentication:
- Token: `MD5(password + salt)`
- Parameters: `u`, `t`, `s`, `v`, `c`

### Compatible Clients
- Supersonic, DSub, Ultrasonic, Sublime Music, Symphonium, Feishin (Subsonic mode)

**[Full OpenSubsonic API Documentation →](/api-opensubsonic/)**

## Jellyfin API

The Jellyfin API enables popular Jellyfin music clients to connect to Melodee. This provides access to the growing ecosystem of Jellyfin-compatible music applications.

### Key Features
- **Jellyfin client support**: Works with Finamp, Feishin, Streamyfin, Gelli
- **Full music browsing**: Artists, albums, songs, playlists, genres
- **Streaming**: Direct play and transcoding support
- **Playback tracking**: Session reporting and scrobbling integration
- **Playlist management**: Create, edit, and sync playlists

### Endpoint Structure
- Clients connect to: `http://server:port/`
- Internal routing: `/api/jf/*`
- Automatic detection via `Authorization: MediaBrowser` header

### Authentication
Uses Jellyfin's MediaBrowser authentication:
```
Authorization: MediaBrowser Token="token", Client="app", Device="device", DeviceId="id", Version="1.0"
```

### Compatible Clients
- Finamp (iOS, Android, Desktop)
- Feishin (Desktop, Jellyfin mode)
- Streamyfin (iOS, Android)
- Gelli (Android)

**[Full Jellyfin API Documentation →](/api-jellyfin/)**

## Choosing the Right API

### Use the Melodee Native API when:
- Building new applications or integrations
- You need access to Melodee-specific features (requests, charts, smart playlists)
- Performance is critical
- You prefer modern REST conventions
- You want strongly-typed responses

### Use the OpenSubsonic API when:
- Integrating with existing Subsonic-compatible clients
- You have existing Subsonic-based integrations
- You need compatibility with third-party mobile apps
- Working with clients designed for Subsonic/Navidrome

### Use the Jellyfin API when:
- You prefer Jellyfin ecosystem clients
- Using Finamp, Feishin, or Streamyfin
- You want feature parity with Jellyfin music features
- Migrating from a Jellyfin server

## API Documentation and Exploration

### Interactive API Documentation
Melodee provides interactive API documentation via Scalar where you can explore and test the native Melodee API:

- **Melodee API (Scalar UI)**: Available at `/scalar/v1` when Melodee is running
- **OpenAPI Specification**: Available at `/openapi/v1.json` for download

The Scalar UI allows you to:
- View detailed endpoint documentation
- Test API calls directly from the browser
- See example requests and responses
- Understand required parameters and authentication
- Download the OpenAPI specification for use with code generators

### External Documentation
- **OpenSubsonic API**: [opensubsonic.netlify.app](https://opensubsonic.netlify.app/)
- **Jellyfin API**: [api.jellyfin.org](https://api.jellyfin.org/)

## Client Compatibility Matrix

| Client | Platform | Melodee API | OpenSubsonic | Jellyfin |
|--------|----------|-------------|--------------|----------|
| MeloAmp | Desktop | ✅ Primary | ✅ Fallback | ❌ |
| Melodee Player | Android | ✅ Primary | ✅ Fallback | ❌ |
| Supersonic | Desktop | ❌ | ✅ | ❌ |
| Feishin | Desktop | ❌ | ✅ | ✅ |
| Finamp | Mobile/Desktop | ❌ | ❌ | ✅ |
| Streamyfin | Mobile | ❌ | ❌ | ✅ |
| DSub | Android | ❌ | ✅ | ❌ |
| Ultrasonic | Android | ❌ | ✅ | ❌ |
| Symphonium | Android | ❌ | ✅ | ❌ |
| Sublime Music | Linux | ❌ | ✅ | ❌ |
| Gelli | Android | ❌ | ❌ | ✅ |

## API Performance Considerations

### Melodee Native API
- Optimized for Melodee's data structures
- Generally fastest response times
- Most efficient data serialization
- Best error handling and validation

### OpenSubsonic API
- Transformation layer for compatibility
- Well-tested across many clients
- Extensive caching for performance
- Established streaming protocols

### Jellyfin API
- URL rewriting middleware
- Efficient media browsing
- Session-based tracking
- Transcoding integration

## Getting Started

1. **Explore the API**: Visit `/scalar/v1` on your Melodee instance to interactively explore the Melodee API
2. **Create an account**: Register on your Melodee instance
3. **Get authentication**: Obtain JWT token, Subsonic token, or Jellyfin token depending on API
4. **Choose your client**: Select based on your platform and preferred API
5. **Test endpoints**: Use the Scalar UI or curl to test before implementing
6. **Download specs**: Get OpenAPI spec at `/openapi/v1.json` for code generation

All three APIs provide access to Melodee's powerful music management features, so choose based on your specific needs and existing client ecosystem.
---
title: API Overview
permalink: /apis/
---

# API Overview

Melodee provides two distinct API surfaces to serve different use cases: the native Melodee API and the OpenSubsonic-compatible API. Understanding both options helps you choose the right API for your needs.

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
All endpoints (except public ones) require an API key:
```
Authorization: Bearer <your-api-key>
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

## OpenSubsonic API

The OpenSubsonic API provides compatibility with the Subsonic API specification, allowing Melodee to work with existing Subsonic-compatible clients and applications.

### Key Features
- **Client compatibility**: Works with existing Subsonic clients (DSub, Subsonic, etc.)
- **Broad ecosystem**: Access to the wide range of existing Subsonic-compatible apps
- **Familiar endpoints**: Uses the well-established Subsonic API structure
- **Streaming compatibility**: Supports the same streaming protocols as Subsonic

### Endpoint Structure
- Base URL: `/api/rest/`
- Follows Subsonic API specification (e.g., `/api/rest/getAlbum.view`, `/api/rest/stream`)

### Authentication
Uses Subsonic's traditional authentication method:
- Username and password or token-based authentication
- Follows Subsonic API specification for auth parameters

## API Documentation and Exploration

### Interactive API Documentation
Melodee provides interactive API documentation via Swagger UI where you can explore and test both APIs:

- **Swagger UI**: Available at `/swagger` when Melodee is running
- **OpenAPI Specification**: Available at `/swagger/v1/swagger.json`

The Swagger UI allows you to:
- View detailed endpoint documentation
- Test API calls directly from the browser
- See example requests and responses
- Understand required parameters and authentication

### API Reference
- **Melodee API**: Documented in the `/api/` section of this documentation
- **OpenSubsonic API**: Compatible with standard Subsonic API documentation

## Choosing the Right API

### Use the Melodee Native API when:
- Building new applications or integrations
- You need access to Melodee-specific features
- Performance is critical
- You prefer modern REST conventions
- You want strongly-typed responses

### Use the OpenSubsonic API when:
- Integrating with existing Subsonic-compatible clients
- You have existing Subsonic-based integrations
- You need compatibility with third-party apps
- Working with mobile clients designed for Subsonic

## Client Compatibility

### Melodee Native API Clients
- **MeloAmp**: Official desktop client optimized for the native API
- **Custom integrations**: Built specifically for Melodee's native API

### OpenSubsonic Compatible Clients
- **DSub**: Android client
- **Sublimemusic**: Cross-platform client
- **Feishin**: Modern client with web-based UI
- **Airsonic-Advanced**: Web-based client
- **And many more**: Any Subsonic-compatible client should work

## API Performance Considerations

### Melodee Native API
- Optimized for Melodee's data structures
- Generally faster response times
- More efficient data serialization
- Better error handling

### OpenSubsonic API
- May have additional transformation overhead
- Designed for compatibility first
- Well-understood performance characteristics
- Extensive testing across different clients

## Getting Started

1. **Explore the APIs**: Visit `/swagger` on your Melodee instance to interactively explore both APIs
2. **Get an API key**: Create an account and generate an API key in the web interface
3. **Choose your API**: Select based on your use case and client requirements
4. **Test endpoints**: Use the Swagger UI to test endpoints before implementing
5. **Read detailed docs**: Check the `/api/` section for detailed Melodee API documentation

Both APIs provide access to Melodee's powerful music management features, so choose based on your specific needs and existing client ecosystem.
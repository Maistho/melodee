# Melodee API

This API is intended for Melodee applications and is designed to have these advantages over the terrible OpenSubsonic
API:

* Performance as a priority
* All list operations are paginated
* Name of methods and return objects are semantic
* Return objects are as light as possible
* Paginated requests include a "meta" property that outlines pagination data in a uniform best practices manner
* Standardized error responses with error code, message, and correlation ID

## Authentication

All requests (save Song Stream) are expected to have Bearer Tokens provided in the `Authorization` header.
This bearer token can be obtained by calling the `/api/v1/users/authenticate` endpoint.

## API Routes

All versioned API endpoints follow the pattern `/api/v{version}/[controller]`:

- **Albums**: `/api/v1/albums`
- **Artists**: `/api/v1/artists`
- **Playlists**: `/api/v1/playlists`
- **Scrobble**: `/api/v1/scrobble`
- **Search**: `/api/v1/search`
- **Songs**: `/api/v1/songs`
- **System**: `/api/v1/system`
- **Users**: `/api/v1/users`

## Song Streaming (Out of Band)

The song streaming endpoint is intentionally **non-versioned** and uses HMAC authentication instead of Bearer tokens.

**Route**: `/song/stream/{songApiKey}/{userApiKey}/{authToken}`

This design is intentional because:
1. Many JavaScript/React audio controls don't handle Bearer tokens well
2. The HMAC token binds to user, song, and client IP for security
3. Separate versioning allows proxy/caching strategies distinct from the main API

## Error Responses

All error responses follow a standardized format:

```json
{
  "code": "ERROR_CODE",
  "message": "Human-readable error message",
  "correlationId": "request-trace-id"
}
```

Error codes include: `UNAUTHORIZED`, `FORBIDDEN`, `NOT_FOUND`, `BAD_REQUEST`, `VALIDATION_ERROR`, `TOO_MANY_REQUESTS`, `BLACKLISTED`, `USER_LOCKED`, `INTERNAL_ERROR`


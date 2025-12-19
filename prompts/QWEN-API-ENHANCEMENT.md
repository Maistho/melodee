# Melodee API Enhancement Recommendations

Recommended API Endpoints for extending Melodee's functionality.

## Common Response Formats

### Error Response
All non-2xx responses return errors in this format:
```json
{
  "code": "string", // e.g. "VALIDATION_ERROR"
  "message": "string",
  "correlationId": "string",
  "details": "object" // optional, additional context
}
```

### Type Conventions
These examples use descriptive type hints:
- `guid`: JSON `string` containing a UUID
- `double`: JSON `number`
- `boolean`: JSON `true`/`false`
- Date/time values are ISO 8601 strings (UTC), e.g. `"2025-12-19T17:30:00Z"`

### Pagination Meta
Paginated list responses include a `meta` object using a consistent shape:
```json
{
  "totalCount": "int",
  "pageSize": "int",
  "currentPage": "int",
  "totalPages": "int",
  "hasNext": "boolean",
  "hasPrevious": "boolean"
}
```

**HTTP Status Codes**:
- `200` - Success
- `201` - Created (for POST creating new resources)
- `400` - Bad Request (invalid input)
- `401` - Unauthorized (missing/invalid authentication)
- `403` - Forbidden (insufficient permissions)
- `404` - Not Found
- `429` - Too Many Requests (rate limited)
- `500` - Internal Server Error

---

## POST `/api/v1/playback/settings`
**Description**: Save user playback preferences
**Authentication**: Required

**Notes**:
- Omitted fields are left unchanged (partial update semantics).
- `crossfadeDuration` is expressed in seconds and must be `>= 0`.
- `replayGain` and `audioQuality` must be one of the documented enum values.

**Request Body**:
```json
{
  "crossfadeDuration": "double", // in seconds
  "gaplessPlayback": "boolean",
  "volumeNormalization": "boolean",
  "replayGain": "string", // "none", "track", "album"
  "audioQuality": "string", // "low", "medium", "high", "lossless"
  "equalizerPreset": "string"
}
```
**Response**:
```json
{
  "success": "boolean"
}
```

## GET `/api/v1/playback/settings`
**Description**: Get user playback preferences
**Authentication**: Required

**Notes**:
- If a user has never saved settings, server defaults are returned.
- `lastUsedDevice` is read-only and may be omitted or null if unknown.

**Response**:
```json
{
  "crossfadeDuration": "double",
  "gaplessPlayback": "boolean",
  "volumeNormalization": "boolean",
  "replayGain": "string",
  "audioQuality": "string",
  "equalizerPreset": "string",
  "lastUsedDevice": "string" // read-only, set automatically by the system
}
```

## POST `/api/v1/equalizer/presets`
**Description**: Create or update equalizer preset
**Authentication**: Required

**Notes**:
- Presets are user-scoped.
- Upsert behavior: if a preset with the same `name` already exists for the user, it is updated; otherwise a new preset is created.
- If `isDefault=true`, the server will ensure this is the only default preset for the user.
- Each band `frequency` must be `> 0` (Hz).

**Request Body**:
```json
{
  "name": "string",
  "bands": [
    {
      "frequency": "double", // Hz
      "gain": "double" // dB
    }
  ],
  "isDefault": "boolean"
}
```
**Response**:
```json
{
  "id": "guid",
  "name": "string",
  "bands": [
    {
      "frequency": "double",
      "gain": "double"
    }
  ],
  "isDefault": "boolean"
}
```

## GET `/api/v1/equalizer/presets`
**Description**: Get available equalizer presets
**Authentication**: Required
**Parameters**:
- `page` (int, optional): Page number (default: 1)
- `limit` (int, optional): Results per page (default: 20, max: 100)
**Response**: 
```json
{
  "presets": [
    {
      "id": "guid",
      "name": "string",
      "bands": [
        {
          "frequency": "double",
          "gain": "double"
        }
      ],
      "isDefault": "boolean"
    }
  ],
  "meta": {
    "totalCount": "int",
    "pageSize": "int",
    "currentPage": "int",
    "totalPages": "int",
    "hasNext": "boolean",
    "hasPrevious": "boolean"
  }
}
```

## POST `/api/v1/search/advanced`
**Description**: Advanced search with multiple criteria
**Authentication**: Required

**Notes**:
- All fields are optional; if `types` is omitted or empty, all types are searched.
- Range filters (`year`, `bpm`, `duration`) must satisfy `min <= max`.
- `page`/`limit` apply to the overall result set; the server may return fewer items per type depending on `types` and filtering.

**Request Body**:
```json
{
  "query": "string",
  "filters": {
    "year": {
      "min": "int",
      "max": "int"
    },
    "bpm": {
      "min": "double",
      "max": "double"
    },
    "duration": {
      "min": "double", // in seconds
      "max": "double"
    },
    "genre": ["string"],
    "mood": ["string"],
    "key": "string",
    "artist": "string",
    "album": "string"
  },
  "types": ["string"], // "song", "album", "artist", "playlist"
  "sortBy": "string", // "relevance", "date", "popularity", "rating"
  "sortOrder": "string", // "asc", "desc"
  "page": "int",
  "limit": "int"
}
```
**Response**: 
```json
{
  "results": {
    "songs": [...],
    "albums": [...],
    "artists": [...],
    "playlists": [...]
  },
  "meta": {
    "totalCount": "int",
    "pageSize": "int",
    "currentPage": "int",
    "totalPages": "int",
    "hasNext": "boolean",
    "hasPrevious": "boolean"
  }
}
```

## GET `/api/v1/search/similar/{id}/{type}`
**Description**: Find similar content (artist/album/song)
**Authentication**: Required

**Notes**:
- `similarityScore` is a relative score (higher means more similar); clients should not assume a specific scale beyond numeric ordering.

**Parameters**: 
- `id` (guid): ID of the reference item
- `type` (string): "artist", "album", or "song"
- `limit` (int, optional): Number of similar items (default: 10)
**Response**: 
```json
{
  "similar": [
    {
      "id": "guid",
      "name": "string",
      "type": "string", // "artist", "album", "song"
      "similarityScore": "double",
      "imageUrl": "string"
    }
  ]
}
```

## POST `/api/v1/playlists/smart`
**Description**: Create smart playlist with rules
**Authentication**: Required

**Notes**:
- Rule `value` depends on `operator`:
  - `equals` / `greaterThan` / `lessThan`: a single primitive value
  - `between`: an object like `{ "min": ..., "max": ... }`
  - `contains`: a string value
- `limit` caps the number of tracks included when the rules are evaluated.

**Request Body**:
```json
{
  "name": "string",
  "description": "string",
  "rules": [
    {
      "field": "string", // "genre", "year", "rating", "playCount", etc.
      "operator": "string", // "equals", "contains", "greaterThan", "lessThan", "between"
      "value": "any"
    }
  ],
  "limit": "int", // Max number of tracks
  "autoUpdate": "boolean" // Whether to auto-update playlist
}
```
**Response**: 
```json
{
  "id": "guid",
  "name": "string",
  "description": "string",
  "rules": [...],
  "trackCount": "int",
  "autoUpdate": "boolean"
}
```

## GET `/api/v1/recommendations`
**Description**: Get personalized recommendations
**Authentication**: Required

**Notes**:
- Recommendations are generated at request-time and are not paginated.
- If `type` is omitted, the response may contain a mixed set of recommendation types.

**Parameters**: 
- `limit` (int, optional): Number of recommendations (default: 20)
- `type` (string, optional): "song", "album", "artist"
- `category` (string, optional): "discover", "similar", "missed", "based_on_recent"
**Response**: 
```json
{
  "recommendations": [
    {
      "id": "guid",
      "name": "string",
      "type": "string",
      "artist": "string",
      "reason": "string", // Why this was recommended
      "imageUrl": "string"
    }
  ],
  "category": "string"
}
```

## GET `/api/v1/audio/features/{id}`
**Description**: Get detailed audio features
**Authentication**: Required
**Parameters**: 
- `id` (guid): Song ID
**Response**: 
```json
{
  "id": "guid",
  "tempo": "double", // BPM
  "key": "string", // Musical key
  "mode": "string", // "major", "minor"
  "timeSignature": "int",
  "acousticness": "double", // 0.0 to 1.0
  "danceability": "double", // 0.0 to 1.0
  "energy": "double", // 0.0 to 1.0
  "instrumentalness": "double", // 0.0 to 1.0
  "liveness": "double", // 0.0 to 1.0
  "loudness": "double", // in dB
  "speechiness": "double", // 0.0 to 1.0
  "valence": "double" // 0.0 to 1.0 (musical positiveness)
}
```

## GET `/api/v1/audio/bpm`
**Description**: Get tracks within BPM range
**Authentication**: Required

**Notes**:
- `min` and `max` must satisfy `min <= max`.

**Parameters**: 
- `min` (double, required): Minimum BPM
- `max` (double, required): Maximum BPM
- `page` (int, optional): Page number (default: 1)
- `limit` (int, optional): Results per page (default: 50, max: 100)
**Response**: 
```json
{
  "tracks": [
    {
      "id": "guid",
      "title": "string",
      "artist": "string",
      "bpm": "double"
    }
  ],
  "meta": {
    "totalCount": "int",
    "pageSize": "int",
    "currentPage": "int",
    "totalPages": "int",
    "hasNext": "boolean",
    "hasPrevious": "boolean"
  }
}
```

## GET `/api/v1/analytics/listening`
**Description**: Get detailed listening statistics
**Authentication**: Required
**Parameters**: 
- `period` (string, optional): "day", "week", "month", "year", "all_time" (default: "week")
**Response**: 
```json
{
  "period": "string",
  "totalPlayTime": "double", // in seconds
  "totalTracksPlayed": "int",
  "topArtists": [
    {
      "id": "guid",
      "name": "string",
      "playCount": "int",
      "playTime": "double"
    }
  ],
  "topAlbums": [...],
  "topGenres": [...],
  "listeningByTimeOfDay": [
    {
      "hour": "int", // 0-23
      "playTime": "double"
    }
  ],
  "listeningByDayOfWeek": [
    {
      "day": "string", // "monday", "tuesday", etc.
      "playTime": "double"
    }
  ]
}
```

## GET `/api/v1/analytics/top/{period}`
**Description**: Get top content for period
**Authentication**: Required
**Parameters**: 
- `period` (string): "day", "week", "month", "year", "all_time"
- `type` (string): "song", "album", "artist"
- `limit` (int, optional): Number of items (default: 10)
**Response**: 
```json
{
  "period": "string",
  "type": "string",
  "items": [
    {
      "id": "guid",
      "name": "string",
      "playCount": "int",
      "playTime": "double",
      "rank": "int"
    }
  ]
}
```


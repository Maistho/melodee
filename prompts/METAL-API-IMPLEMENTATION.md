# Metal API Image Plugin Implementation Guide

Create Metal API–backed implementations of `IArtistImageSearchEnginePlugin` and `IAlbumImageSearchEnginePlugin` using the swagger at https://www.metal-api.dev/swagger/v1/swagger.json. Follow existing plugin patterns in `Melodee.Common/Plugins/SearchEngine` (e.g., LastFm, Deezer, ITunes, Spotify).

## 1) API surface (per swagger)

Base URL: `https://www.metal-api.dev`

Endpoints:
- `GET /search/bands/name/{name}` → `BandSearchResult` (id, name, genre, country, link)
- `GET /search/bands/genre/{genre}` → `BandSearchResult`
- `GET /search/albums/title/{title}` → `AlbumSearchResult` (id, title, band{…}, type, date, link)
- `GET /albums/{albumId}` → `Album` (id, name, type, releaseDate, catalogID, label, format, limitations, reviews, **coverUrl**, songs[])
- `GET /bands/{bandId}` → swagger claims `Album` schema; verify actual payload before coding.

Notes:
- Swagger shows singular objects for search responses (not arrays); confirm real shape at runtime.
- Sample calls on 2025-12-14 returned HTTP 500; implement robust error handling and make behavior tolerant of server errors/timeouts.

## 2) Common Metal API client

1. Add a small HTTP client (e.g., `MetalApiClient`) in `src/Melodee.Common/Plugins/SearchEngine/MetalApi/`.
2. Provide methods:
   - `Task<BandSearchResult[]?> SearchBandsByNameAsync(string name, CancellationToken token)`
   - `Task<AlbumSearchResult[]?> SearchAlbumsByTitleAsync(string title, CancellationToken token)`
   - `Task<Album?> GetAlbumAsync(string albumId, CancellationToken token)`
   - Optional: `Task<object?> GetBandAsync(string bandId, CancellationToken token)` once shape is confirmed.
3. Use `HttpClient` with `Accept: application/json`. No auth keys are documented; keep configurable base URL in case it changes.
4. Deserialize with case-insensitive options; allow both object and array payloads to cope with swagger inconsistencies.
5. For non-success (>=400) return a failed `OperationResult` or `null` per existing plugin conventions; never throw silently—log status, URL, and traceId if present.

## 3) Mapping to `ImageSearchResult`

Album covers:
- Source: `Album.coverUrl` from `/albums/{albumId}`.
- Map to `ImageSearchResult`:
  - `FromPlugin` = `"Metal API"`
  - `MediaUrl` = `ThumbnailUrl` = `coverUrl`
  - `Title` = album name/title
  - `ArtistMusicBrainzId`, etc.: leave null unless the API returns equivalents
  - `ReleaseDate` = parsed `releaseDate` or `date`
  - `Rank`: start with 10 for exact matches, lower for partials
  - `UniqueId` = hash of `coverUrl`
- If `coverUrl` is missing or empty, skip the result.

Artist images:
- The API exposes band data but no explicit image field. Implement a best-effort strategy:
  - Use `/search/bands/name/{name}` (and optional `/genre/{genre}`) to find the band.
  - If the band payload eventually exposes an image/cover/link with artwork, map it to `ImageSearchResult` as above.
  - If no image is available, optionally fall back to the band’s most relevant album cover (by searching albums whose title contains the band name) and mark `Title` to indicate it is derived from album art.
  - When nothing usable is found, return a successful `OperationResult` with empty `Data` (not an exception).

## 4) Album image plugin (`IAlbumImageSearchEnginePlugin`)

1. Class name suggestion: `MetalApiAlbumImageSearchEngine`.
2. Implement `DoAlbumImageSearch(AlbumQuery query, int maxResults, CancellationToken token = default)`.
3. Flow:
   - Validate `query.Name`/`query.Year`; fail fast with `OperationResult` message if missing.
   - Call `SearchAlbumsByTitleAsync(query.NameNormalized)`; tolerate object vs array responses.
   - Filter matches by artist (if `query.Artist` provided) and release year proximity.
   - For each candidate (up to `maxResults`), call `GetAlbumAsync(id)` to fetch `coverUrl`; map to `ImageSearchResult`.
   - Order by rank (exact title + artist match highest), then by release date; truncate to `maxResults`.
   - `StopProcessing`: choose `false` unless you want to halt the service once a cover is found.

## 5) Artist image plugin (`IArtistImageSearchEnginePlugin`)

1. Class name suggestion: `MetalApiArtistImageSearchEngine`.
2. Implement `DoArtistImageSearch(ArtistQuery query, int maxResults, CancellationToken token = default)`.
3. Flow:
   - Validate `query.Name`; return failure `OperationResult` if empty.
   - Call `SearchBandsByNameAsync(query.NameNormalized)`; if API returns singular object, wrap into an array.
   - If payload includes an image/cover URL, map directly.
   - Otherwise, derive images from album covers: search albums whose titles contain the artist name, fetch covers, and tag results with `Title = $"{bandName} album art"` and a lower rank.
   - Deduplicate on `MediaUrl`; order by rank descending; truncate to `maxResults`.

## 6) Plugin metadata and wiring

- Implement `IPlugin` members: stable `Id` (GUID), `DisplayName = "Metal API"`, `SortOrder` similar to other HTTP-based plugins, `IsEnabled` configurable.
- Expose a small options class (base URL, enable flag, optional timeout).
- Register in the search engine services where other plugins are instantiated, mirroring `AlbumImageSearchEngineService` and `ArtistImageSearchEngineService` patterns. Keep disabled by default if the API remains unstable.

## 7) Error handling & resilience

- Treat 5xx/timeout as non-fatal: return `OperationResult` with an error message and empty data so the service can continue to other providers.
- Honor `CancellationToken`.
- Log URL, status code, and any `traceId` from the API error payload.
- Add lightweight retry or backoff only if consistent with existing plugins (otherwise, single attempt).

## 8) Testing strategy

- Unit-test the client with stubbed `HttpMessageHandler` to simulate:
  - 200 with object payload, 200 with array payload
  - 500 responses and timeouts
  - Missing `coverUrl`
- Unit-test mappers to ensure ranking, deduping, and truncation honor `maxResults`.
- Unit-test both plugins to validate:
  - Input validation failures
  - Empty responses yield empty successful `OperationResult`
  - Album cover mapping populates `ImageSearchResult` correctly
  - Fallback to album-derived artist images when no band image exists

## 9) Open questions / validation steps

- Confirm whether `/search/*` returns single objects or arrays; adjust deserialization accordingly.
- Verify `/bands/{bandId}` actual schema (swagger points to `Album` incorrectly).
- Check if any auth header/rate limit exists; add config hooks if discovered.
- Once the live API stops returning 500s, capture sample payloads and update mappers/tests with accurate fields.

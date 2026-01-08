# Artist Search Engines Implementation Plan

This document outlines a phased approach to implementing artist search engines for AMG, Discogs, iTunes, Last.fm, and WikiData. These will integrate with the existing `ArtistSearchEngineService` pattern used by MusicBrainz and Spotify.

## Overview

Each search engine will implement the `IArtistSearchEnginePlugin` interface and follow the plugin architecture established in `src/Melodee.Common/Plugins/SearchEngine/`. The results from these plugins will be normalized into `ArtistSearchResult` objects that the UI can display.

## Current Architecture

```
ArtistSearchEngineService
├── IArtistSearchEnginePlugin (interface)
│   ├── MusicBrainzArtistSearchEnginePlugin (existing)
│   ├── SpotifySearchEnginePlugin (existing)
│   └── New plugins to add...
```

Each plugin provides:
- `Id` - Unique GUID identifier
- `DisplayName` - Human-readable name (e.g., "Discogs")
- `IsEnabled` - Whether the plugin is enabled
- `SortOrder` - Execution order
- `DoArtistSearchAsync()` - Performs the actual search

## Cross-cutting Concerns (Applies to all plugins)

- **Input validation & normalization**
  - Treat all user-provided values as untrusted.
  - Reject empty/whitespace queries, trim, and cap length (e.g., 256 chars).
  - AMG IDs must be digits only.
  - Encode outbound requests correctly (URL/query-encode parameters; do not HTML-encode).
- **HTTP client hygiene**
  - Use `IHttpClientFactory` for all outbound calls and set a per-provider `User-Agent` where required.
  - Use reasonable timeouts and always pass the `CancellationToken`.
- **Resilience / rate limits**
  - Retry only transient failures (timeouts, 5xx) and handle HTTP 429 with `Retry-After`.
  - Do not retry non-rate-limited 4xx responses.
- **Concurrency throttling**
  - Limit concurrent outbound requests per provider (e.g., `SemaphoreSlim`) to avoid self-inflicted rate-limit storms.
- **Failure behavior**
  - A single plugin failing must not fail the overall search; log and return an empty result set for that provider.

---

## Phase 1: Research and Foundation

### Goals
- Finalize API research and authentication requirements
- Create base plugin infrastructure
- Set up project structure and NuGet dependencies

### Deliverables
- Documented API specifications for each service
- Project references confirmed
- Plugin base classes created
- Plugin discovery/registration approach confirmed (DI container)

### API Research Summary

#### 1. All Music Guide (AMG)

**Status:** No public API available.

**Decision:** Use iTunes lookup by AMG ID only. Do not implement AMG via web scraping.

**Approach:**
- Endpoint: `https://itunes.apple.com/lookup?amgArtistId={ID}`
- Returns: Artist metadata, images, genres
- Authentication: None required
- Rate limits: Not documented (Apple standard)

---

#### 2. Discogs

**Status:** Public REST API v2.0 available.

**API Details:**
- Base URL: `https://api.discogs.com/`
- Documentation: https://www.discogs.com/developers
- Search endpoint: `https://api.discogs.com/database/search`
- Artist endpoint: `https://api.discogs.com/artists/{id}`

**Authentication:**
- User agent required (app name + email)
- OAuth for write operations (not needed for search)
- Rate limits: 60 requests/minute for unauthenticated, higher for authenticated

**.NET Libraries Available:**
1. **DiscogsApiClient** (NuGet: `DiscogsApiClient`)
   - GitHub: https://github.com/damidhagor/DiscogsApiClient
   - Status: Active maintenance
   - .NET 6.0+

2. **ParkSquare.Discogs** (NuGet: `ParkSquare.Discogs`)
   - GitHub: https://github.com/parksq/dotnet-core-discogs
   - Simple REST wrapper

3. **DiscogsConnect** (GitHub, older)
   - https://github.com/christophdebaene/DiscogsConnect

**Search Parameters:**
```
q={artist_name}
type=artist
format=artist
per_page=25
page=1
```

**Response includes:**
- Artist name, id, thumbnail, images
- Real name, name variations
- Profile, aliases
- URI links

---

#### 3. iTunes / Apple Music

**Decision:** Use the iTunes Search API (Option A) only.

**iTunes Search API (Public, Free)**
- Base URL: `https://itunes.apple.com/search`
- Documentation: https://developer.apple.com/library/archive/documentation/AudioVideo/Conceptual/iTuneSearchAPI
- No API key required
- Rate limits: Not documented

**Parameters:**
```
term={artist_name}
media=music
entity=artist
limit=25
```

**Features:**
- Free, no authentication
- Returns: Artist name, id, genres, link to iTunes
- Also supports lookup by ID: `https://itunes.apple.com/lookup?id={itunes_id}`
- Supports lookup by AMG ID: `https://itunes.apple.com/lookup?amgArtistId={amg_id}`

**Decision:** Do not implement the Apple Music API (Option B).

---

#### 4. Last.fm

**Status:** Public API available with API key registration.

**API Details:**
- Base URL: `https://ws.audioscrobbler.com/2.0/`
- Documentation: https://www.last.fm/api
- API key registration: https://www.last.fm/api/account/create

**Search endpoint:**
```
method=artist.search
artist={artist_name}
api_key={API_KEY}
format=json
```

**Parameters:**
- `limit` (optional): Results per page, default 30
- `page` (optional): Page number

**Authentication:**
- API key required (free)
- No OAuth for read operations

**.NET Libraries Available:**
1. **lastfm-net** (NuGet: `lastfm-net`)
   - GitHub: https://github.com/inflatablefriends/lastfm
   - Status: Active, modern .NET
   - 99 stars on GitHub

2. **Last.fm** (GitHub: avatar29A/Last.fm)
   - Older implementation

3. **LastSharp** (GitHub: anthonyvscode/LastSharp)
   - Older, may need updates

**Response includes:**
- Artist name, mbid (MusicBrainz ID), url
- Streamable flag
- Image URLs (various sizes)
- Listeners count

---

#### 5. WikiData

**Status:** Free, open SPARQL endpoint available.

**API Details:**
- SPARQL endpoint: `https://query.wikidata.org/sparql`
- SPARQL (JSON) API: `https://query.wikidata.org/sparql?format=json&query={SPARQL}`
- No API key required
- Rate limits: ~60 requests/second (with User-Agent)

**SPARQL Query Example (request JSON bindings):**
```sparql
SELECT DISTINCT ?artist ?artistLabel ?mbid ?spotify ?image WHERE {
  SERVICE wikibase:mwapi {
    bd:serviceParam wikibase:api "EntitySearch";
                    wikibase:endpoint "www.wikidata.org";
                    mwapi:search "radiohead";
                    mwapi:language "en".
    ?artist wikibase:apiOutputItem mwapi:item.
  }

  # Prefer entities that are musical artists or bands.
  ?artist wdt:P31/wdt:P279* ?instanceOf.
  FILTER(?instanceOf IN (wd:Q177220, wd:Q215380))

  OPTIONAL { ?artist wdt:P434 ?mbid. }     # MusicBrainz artist ID
  OPTIONAL { ?artist wdt:P2207 ?spotify. } # Spotify artist ID
  OPTIONAL { ?artist wdt:P18 ?image. }     # image

  SERVICE wikibase:label { bd:serviceParam wikibase:language "en". }
}
LIMIT 25
```

**Wikidata Properties for Artists:**
- P434: MusicBrainz artist ID
- P2207: Spotify artist ID
- P18: Image
- P136: Genre
- P856: Official website

**Advantages:**
- Completely free, no API key
- Rich linked data
- Cross-references to other databases (MusicBrainz, Spotify, etc.)

**Considerations:**
- SPARQL learning curve
- Query optimization important for performance
- Use the SPARQL JSON bindings format (`format=json`) for simple parsing
- Always set a descriptive `User-Agent` and handle HTTP 429 (`Retry-After`)

---

## Phase 2: Core Plugin Implementation

### Goals
- Implement Discogs search engine plugin
- Implement iTunes search engine plugin
- Implement Last.fm search engine plugin
- Create shared infrastructure (DTOs, configuration)

### Deliverables
- 3 new search engine plugins implemented
- Configuration settings for each
- Unit tests for each plugin

### Implementation Order

#### 2.1 Discogs Plugin

**File Location:** `src/Melodee.Common/Plugins/SearchEngine/Discogs/DiscogsArtistSearchEnginePlugin.cs`

**Configuration Settings Required:**
- `searchEngine.discogs.enabled` (bool, default: false)
- `searchEngine.discogs.userAgent` (string, required)
- `searchEngine.discogs.userToken` (string, optional)
- Treat `userToken` as a secret (do not log it)

**Implementation Steps:**
1. Create `DiscogsSearchEngineSettings` class
2. Validate and normalize inputs (trim, length cap, URL-encode query parameters)
3. Implement `IArtistSearchEnginePlugin` interface
4. Implement HTTP client with rate limiting
5. Parse Discogs API response
6. Transform to `ArtistSearchResult`
7. Add unit tests

---

#### 2.2 iTunes Plugin

**File Location:** `src/Melodee.Common/Plugins/SearchEngine/ITunes/ITunesArtistSearchEnginePlugin.cs`

**Configuration Settings Required:**
- `searchEngine.itunes.enabled` (bool, default: false)
- `searchEngine.itunes.countryCode` (string, default: "US")

**Implementation Steps:**
1. Create `ITunesSearchEngineSettings` class
2. Validate and normalize inputs (trim, length cap, URL-encode `term`)
3. Implement `IArtistSearchEnginePlugin` interface
4. Call iTunes Search API
5. Parse JSON response
6. Transform to `ArtistSearchResult`
7. Add unit tests

---

#### 2.3 Last.fm Plugin

**File Location:** `src/Melodee.Common/Plugins/SearchEngine/LastFm/LastFmArtistSearchEnginePlugin.cs`

**Configuration Settings Required:**
- `searchEngine.lastfm.enabled` (bool, default: false)
- `searchEngine.lastfm.apiKey` (string, required)
- Treat `apiKey` as a secret (do not log it)

**Implementation Steps:**
1. Create `LastFmSearchEngineSettings` class
2. Validate and normalize inputs (trim, length cap, URL-encode `artist`)
3. Implement `IArtistSearchEnginePlugin` interface
4. Call Last.fm API with API key
5. Parse JSON response
6. Transform to `ArtistSearchResult`
7. Add unit tests

---

## Phase 3: Advanced Plugins

### Goals
- Implement WikiData search engine plugin
- Implement AMG lookup via iTunes
- Create caching strategy

### Deliverables
- WikiData search engine plugin
- AMG ID lookup integration
- Performance testing

#### 3.1 WikiData Plugin

**File Location:** `src/Melodee.Common/Plugins/SearchEngine/WikiData/WikiDataArtistSearchEnginePlugin.cs`

**Configuration Settings Required:**
- `searchEngine.wikidata.enabled` (bool, default: false)

**Implementation Steps:**
1. Create `WikiDataSearchEngineSettings` class
2. Validate and normalize inputs (trim, length cap, escape search term for SPARQL)
3. Implement `IArtistSearchEnginePlugin` interface
4. Build SPARQL queries dynamically
5. Call WikiData SPARQL endpoint
6. Parse SPARQL results (JSON format)
7. Transform to `ArtistSearchResult`
8. Add unit tests

**SPARQL Query Template:**
```sparql
SELECT DISTINCT ?artist ?artistLabel ?mbid ?spotify ?image WHERE {
  SERVICE wikibase:mwapi {
    bd:serviceParam wikibase:api "EntitySearch";
                    wikibase:endpoint "www.wikidata.org";
                    mwapi:search "{SEARCH_TERM}";
                    mwapi:language "en";
                    mwapi:limit {LIMIT}.
    ?artist wikibase:apiOutputItem mwapi:item.
  }

  ?artist wdt:P31/wdt:P279* ?instanceOf.
  FILTER(?instanceOf IN (wd:Q177220, wd:Q215380))

  OPTIONAL { ?artist wdt:P434 ?mbid. }
  OPTIONAL { ?artist wdt:P2207 ?spotify. }
  OPTIONAL { ?artist wdt:P18 ?image. }
  SERVICE wikibase:label { bd:serviceParam wikibase:language "en". }
}
LIMIT {LIMIT}
```

---

#### 3.2 AMG ID Lookup

**Strategy:** Use iTunes API to resolve AMG IDs.

**UI Integration:** `src/Melodee.Blazor/Components/Dialogs/ArtistLookupDialog.cs`
- Add "Lookup by AMG ID" feature
- The dialog should call an injected service (not make HTTP calls directly) to perform the iTunes lookup
- Returned results should be normalized to the same `ArtistSearchResult` model as other providers

**Flow:**
1. User clicks AMG ID lookup button
2. Enters AMG ID (e.g., "468749")
3. Service calls: `https://itunes.apple.com/lookup?amgArtistId=468749`
4. Parse response to get artist name, metadata
5. Return candidates to the dialog

---

## Phase 4: Integration and UI

### Goals
- Update ArtistEdit.razor to enable/disable buttons based on config
- Update ArtistLookupDialog to show all enabled providers
- Add provider filtering in UI
- Update settings UI in admin section

### Deliverables
- Updated UI with new search providers
- Admin settings page updates
- Integration testing

#### 4.1 UI Updates

**ArtistEdit.razor Updates:**
```csharp
// Add configuration checks
private bool _isDiscogsEnabled;
private bool _isITunesEnabled;
private bool _isLastFmEnabled;
private bool _isWikiDataEnabled;

// Update button disabled states
<RadzenButton Icon="search" Text="@L(\"Actions.Search\")"
              Click="@(() => SearchForExternalButtonClick(\"AmgId\"))"
              Disabled="@(!_isITunesEnabled && !_isMusicBrainzEnabled && !_isSpotifyEnabled)"/>
```

**ArtistLookupDialog Updates:**
- Show all enabled providers in filter dropdown
- Display provider badges on results
- Track which provider returned each result

---

#### 4.2 Admin Settings

**New Settings in Database:**
- `searchEngine.discogs.enabled`
- `searchEngine.discogs.userAgent`
- `searchEngine.discogs.userToken`
- `searchEngine.itunes.enabled`
- `searchEngine.itunes.countryCode`
- `searchEngine.lastfm.enabled`
- `searchEngine.lastfm.apiKey`
- `searchEngine.wikidata.enabled`

**Admin UI Page:** `src/Melodee.Blazor/Components/Pages/Admin/SearchEngineSettings.razor`

---

## Phase 5: Polish and Optimization

### Goals
- Implement caching for all plugins
- Add error handling and retry logic
- Performance optimization
- Documentation

### Deliverables
- Cached search results
- Retry policies
- Performance benchmarks
- API documentation

#### 5.1 Caching Strategy

- Use a shorter TTL by default (e.g., 1-4 hours) and make it configurable.
- Include relevant configuration in the cache key (e.g., iTunes `countryCode`) to avoid cross-config pollution.

```csharp
var ttl = TimeSpan.FromHours(2); // configurable
var configHash = $"country={countryCode}";
var cacheKey = $"artist_search_{provider}_{normalizedQuery}_{limit}_{configHash}";

var cached = await _cacheManager.GetAsync<ArtistLookupResult>(cacheKey);
if (cached is not null) return cached;

// Perform search...
await _cacheManager.SetAsync(cacheKey, result, ttl);
```

#### 5.2 Retry Policy

```csharp
// Retry transient failures and respect rate limits.
// - Retry 5xx and network failures.
// - For 429, prefer Retry-After if present.
// - Do not retry other 4xx.
var policy = Policy<HttpResponseMessage>
    .Handle<HttpRequestException>()
    .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests || (int)r.StatusCode >= 500)
    .WaitAndRetryAsync(3, (outcome, retryAttempt, _) =>
    {
        if (outcome.Result?.StatusCode == System.Net.HttpStatusCode.TooManyRequests &&
            outcome.Result.Headers.RetryAfter?.Delta is { } retryAfter)
        {
            return retryAfter;
        }

        return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
    });
```

---

## Configuration Reference

### New Settings Summary

| Setting | Type | Default | Required | Description |
|---------|------|---------|----------|-------------|
| `searchEngine.discogs.enabled` | bool | false | No | Enable Discogs search |
| `searchEngine.discogs.userAgent` | string | - | Yes | App name + email |
| `searchEngine.discogs.userToken` | string | - | No | Discogs personal access token (optional for higher rate limits) |
| `searchEngine.itunes.enabled` | bool | false | No | Enable iTunes search |
| `searchEngine.itunes.countryCode` | string | "US" | No | Store country code |
| `searchEngine.lastfm.enabled` | bool | false | No | Enable Last.fm search |
| `searchEngine.lastfm.apiKey` | string | - | Yes | Last.fm API key |
| `searchEngine.wikidata.enabled` | bool | false | No | Enable WikiData search |

---

## Dependencies to Add

```xml
<!-- For Discogs (optional, can use HttpClient directly) -->
<PackageReference Include="DiscogsApiClient" Version="4.0.0" />

<!-- For Last.fm (optional, can use HttpClient directly) -->
<PackageReference Include="lastfm-net" Version="1.0.0" />

```

**Note:** Consider using raw HttpClient for all plugins to minimize dependencies and control rate limiting.

---

## Testing Strategy

### Unit Tests
- Mock HTTP responses for each API
- Test query normalization
- Test result transformation
- Test error handling
- Edge cases:
  - Empty/whitespace queries
  - Very long artist names
  - Unicode/diacritics (e.g., Björk)
  - HTTP 429 + Retry-After handling
  - Timeouts/network failures
  - Malformed/partial JSON responses

### Integration Tests
- Prefer recorded fixtures / mocked HTTP in CI to avoid rate limits and flakiness
- Optional manual/live tests can call real APIs with strict throttling
- Validate response parsing with representative samples (e.g., Radiohead, Beyoncé)

### Performance Tests
- Measure response times
- Test caching effectiveness
- Load testing with concurrent requests

---

---

## Success Criteria

1. **Functional Requirements:**
   - [ ] Artist search works via Discogs
   - [ ] Artist search works via iTunes
   - [ ] Artist search works via Last.fm
   - [ ] Artist search works via WikiData
   - [ ] AMG ID lookup via iTunes

2. **Performance Requirements:**
   - [ ] Search completes in < 5 seconds
   - [ ] Cached results return in < 100ms
   - [ ] Rate limits respected

3. **Quality Requirements:**
   - [ ] 85% code coverage
   - [ ] All plugins follow same pattern
   - [ ] Configuration via admin UI
   - [ ] Documentation complete

---

## References

### Existing Plugins for Reference
- `src/Melodee.Common/Plugins/SearchEngine/MusicBrainz/MusicBrainzArtistSearchEnginePlugin.cs`
- `src/Melodee.Common/Plugins/SearchEngine/Spotify/Spotify.cs`

### API Documentation
- Discogs: https://www.discogs.com/developers
- iTunes: https://developer.apple.com/library/archive/documentation/AudioVideo/Conceptual/iTuneSearchAPI
- Last.fm: https://www.last.fm/api
- WikiData: https://www.wikidata.org/wiki/Wikidata:SPARQL_query_service
- AMG via iTunes: https://developer.apple.com/library/archive/documentation/AudioVideo/Conceptual/iTuneSearchAPI/LookupExamples.html

### .NET Libraries
- DiscogsApiClient: https://github.com/damidhagor/DiscogsApiClient
- lastfm-net: https://github.com/inflatablefriends/lastfm

# Melodee Query Language (MQL) — Implementation SPEC

**Status:** Draft (implementation-ready)

## 1. Problem statement
Melodee’s current search experience is primarily **text matching** (normalized `Contains` on a small set of fields) and some **simple filter-by** mechanisms via `PagedRequest.FilterBy` (`FilterOperatorInfo[]`). Power users with large libraries need:

- Field-specific search (artist/album/title/etc.)
- Numeric/date comparisons (year, rating, duration, bpm, plays)
- Boolean logic (AND/OR/NOT + parentheses)
- Time-relative predicates (added:last-week)
- Saving queries as “smart playlists”
- Autocomplete/suggestions for fields/operators/known values

This spec proposes **MQL**, a user-facing query language that compiles into **safe, parameterized, efficient database queries**.

## 2. Goals / Non-goals

### Goals
- Provide an expressive query syntax that feels natural to music library management.
- Compile MQL into **EF Core IQueryable filters**, reusing the existing `PagedRequest` + service filtering patterns.
- Make it safe by default (no string-concatenated SQL, no Dynamic LINQ injection).
- Make it fast for large libraries (indexes, query planning, sensible defaults/limits).
- Support “smart playlists” by persisting query text and re-evaluating on demand.

### Non-goals (v1)
- Full-text search engine integration (e.g., Lucene/Elastic). (Future enhancement.)
- Cross-entity joins beyond the library domain model (e.g., remote musicbrainz queries).
- Arbitrary regex executed inside the database for every query (we’ll gate this carefully).

## 3. Current architecture touchpoints (repo observations)
- Search primarily uses `SearchService.DoSearchAsync()` and entity-specific services (e.g., `AlbumService.ListAsync`, `SongService.ListAsync`) with `PagedRequest.FilterBy` and `FilterOperatorInfo`.
- Several services implement safe filter logic with EF Core expressions; some areas use `System.Linq.Dynamic.Core` (notably `LibraryService` history listing). **For MQL we should avoid Dynamic LINQ**.
- `FilterOperator` supports `Equals/NotEquals/< <= > >=/Contains/StartsWith/EndsWith/...` which maps nicely to MQL operators.

## 4. User-facing query language

### 4.1 Syntax overview
MQL is a sequence of terms that can be combined with boolean operators.

- **Free text term**: `pink floyd` (searches default fields for the chosen entity)
- **Field filter**: `artist:"Pink Floyd"` `album:Abbey`
- **Comparison**: `year:>=2000` `rating:>3.5` `duration:<300` `bpm:>120`
- **Range**: `year:1970-1980` (inclusive)
- **Boolean**: `(rock OR metal) AND NOT live`
- **Regex**: `title:/.*remix.*/i`
- **Aggregations / top**: `top:10 genre:Jazz` (see §4.7)

**Whitespace** separates tokens, but quoted strings preserve spaces.

### 4.2 Operator precedence
From highest to lowest:

1. Parentheses `( ... )`
2. Unary `NOT`
3. `AND`
4. `OR`

When users omit operators between terms, default is `AND`.

### 4.3 Literals
- **String**: `Beatles`, `"Pink Floyd"`, supports escaping `\"`.
- **Number**: integer or decimal (culture-invariant, dot decimal).
- **Guid**: for internal API keys when needed (rare for user input).
- **Date**:
  - Absolute: `2026-01-06`
  - Relative shortcuts: `today`, `yesterday`, `last-week`, `last-month`, `last-year`
  - Relative duration: `-7d`, `-3w`, `-12h` (interpreted as “since now minus X”)
- **Time (seconds)**: `duration:<300` means 300 seconds (5 minutes)

### 4.4 Fields (v1)
MQL fields are **entity-scoped**. We start with **Songs**, then extend to Albums and Artists.

#### Songs fields (confirmed mappings)
| Field | Type | Example | DB mapping (confirmed) |
|---|---|---|---|
| `title` | string | `title:"Time"` | `Song.TitleNormalized` |
| `artist` | string | `artist:"Pink Floyd"` | `Song.Album.Artist.NameNormalized` |
| `album` | string | `album:"The Wall"` | `Song.Album.NameNormalized` |
| `genre` | string | `genre:Jazz` | `Song.Genres` (string array) |
| `mood` | string | `mood:Chill` | `Song.Moods` (string array) |
| `year` | number | `year:1979` `year:1970-1980` | `Song.Album.ReleaseDate.Year` |
| `duration` | number | `duration:<300` | `Song.Duration` stored as **milliseconds**; MQL uses **seconds** (convert `seconds*1000`) |
| `bpm` | number | `bpm:>120` | `Song.BPM` |
| `rating` | number | `rating:>=4` | user-specific: `Song.UserSongs.First(us => us.UserId == userId).Rating` |
| `plays` | number | `plays:0` `plays:>10` | user-specific: `Song.UserSongs.First(...).PlayedCount` |
| `starred` | bool | `starred:true` | user-specific: `Song.UserSongs.First(...).IsStarred` |
| `starredAt` | date/range | `starredAt:last-week` | user-specific: `Song.UserSongs.First(...).StarredAt` |
| `lastPlayedAt` | date/range | `lastPlayedAt:-30d` | user-specific: `Song.UserSongs.First(...).LastPlayedAt` |

Notes:
- The service layer already includes `Include(s => s.UserSongs.Where(us => us.UserId == userId))`, so these become cheap nav property predicates.
- `Song.PlayedCount` and `Song.CalculatedRating` exist as **global** counters; MQL uses the user-specific fields above. We may optionally add `globalPlays`/`globalRating` later.

#### Albums fields (confirmed mappings + user-scoped options)
| Field | Type | Example | DB mapping (confirmed) |
|---|---|---|---|
| `album` / `name` | string | `album:"Abbey Road"` | `Album.NameNormalized` |
| `artist` | string | `artist:Beatles` | `Album.Artist.NameNormalized` |
| `year` | number | `year:1969` | `Album.ReleaseDate.Year` |
| `duration` | number | `duration:<3600` | `Album.Duration` stored as **milliseconds**; MQL uses **seconds** |
| `genre` | string | `genre:Rock` | `Album.Genres` (string array) |
| `mood` | string | `mood:Chill` | `Album.Moods` (string array) |
| `rating` | number | `rating:>=4` | user-specific: `Album.UserAlbums.First(ua => ua.UserId == userId).Rating` |
| `plays` | number | `plays:>0` | user-specific: `Album.UserAlbums.First(...).PlayedCount` |
| `starred` | bool | `starred:true` | user-specific: `Album.UserAlbums.First(...).IsStarred` |
| `starredAt` | date/range | `starredAt:last-month` | user-specific: `Album.UserAlbums.First(...).StarredAt` |
| `lastPlayedAt` | date/range | `lastPlayedAt:-30d` | user-specific: `Album.UserAlbums.First(...).LastPlayedAt` |
| `added` | date/range | `added:-30d` | `Album.CreatedAt` (from `DataModelBase`) |

Notes:
- `Album.PlayedCount` / `Album.CalculatedRating` exist as global aggregates (inherited from `MetaDataModelBase`). If we want explicit global fields, consider `globalPlays` / `globalRating` aliases.

#### Artists fields (confirmed mappings + user-scoped options)
| Field | Type | Example | DB mapping (confirmed) |
|---|---|---|---|
| `artist` / `name` | string | `artist:"Miles Davis"` | `Artist.NameNormalized` (+ `AlternateNames` for free-text) |
| `rating` | number | `rating:>=4` | user-specific: `Artist.UserArtists.First(ua => ua.UserId == userId).Rating` |
| `starred` | bool | `starred:true` | user-specific: `Artist.UserArtists.First(...).IsStarred` |
| `starredAt` | date/range | `starredAt:last-year` | user-specific: `Artist.UserArtists.First(...).StarredAt` |
| `plays` | number | `plays:>0` | global: `Artist.PlayedCount` (inherited from `MetaDataModelBase`) |
| `added` | date/range | `added:last-month` | `Artist.CreatedAt` (from `DataModelBase`) |

Notes:
- Unlike songs/albums, `UserArtist` currently does not store a per-user `PlayedCount`, so `plays:` is global-only for artists (unless we extend the schema).

## 5. API design

### 5.1 Parse endpoint (autocomplete + validation)
Add an endpoint to validate and return a parse tree + normalized form.

- `POST /api/v1/query/parse`
- Request: `{ "entity": "songs|albums|artists", "query": "..." }`
- Response (200):
  - `normalizedQuery`: canonical spacing/casing
  - `ast`: simplified nodes for UI introspection
  - `warnings`: e.g., “regex will run client-side”, “top capped at 100”
- Response (400): structured validation error:
  - `errorCode`: `MQL_PARSE_ERROR`, `MQL_UNKNOWN_FIELD`, `MQL_INVALID_LITERAL`, etc.
  - `message`: user-friendly
  - `position`: index + length for UI highlighting

### 5.2 Search endpoints
Integrate MQL into listing endpoints without breaking existing contracts.

Option A (lowest friction): add a new optional query param `q` for MQL; keep current paging/order params.

Rules:
- If `q` is present and non-empty, MQL drives filtering (and may override order/limit via `top`).
- If `q` is absent, existing behavior remains.

## 6. Implementation plan

### 6.1 Components (new)

#### D. Compiler to EF Core
Compile `BoundQuery` into an `Expression<Func<TEntity,bool>>`.

Key principles:
- No string-based `Where("...")`
- Parameterized expressions only
- Avoid client evaluation, unless explicitly intended (regex fallback)

We can reuse existing filtering patterns by adding:
- `PagedRequestExtensions.ApplyMql(...)` that sets `FilterBy`/`OrderBy` appropriately.

However, `FilterBy` doesn’t model parentheses well. So for boolean logic we likely need:
- A new pathway in services: `ListByPredicateAsync(pagedRequest, Expression<Func<TEntity,bool>> predicate, ...)` or a new request object.

**Recommendation:** Introduce a parallel query pipeline for MQL:
- Keep `PagedRequest.FilterBy` for simple UI filters.
- Add MQL support by applying the compiled predicate directly to `IQueryable<TEntity>` in each service.

## 9. Melodee.Blazor UI requirements (no API; DI services only)

### 9.1 Overview
MQL must be usable directly inside the `Melodee.Blazor` application.

Key requirement: **The Blazor UI must not call HTTP API endpoints to execute MQL.**
Instead, it should execute queries by injecting the relevant in-process services (e.g., `SearchService` or a future dedicated `MqlSearchService`) into the Razor page via DI and calling them directly.

### 9.2 Entry point: “Advanced” button on Search page
- On the existing search results page (`Search.razor`, currently routed as `/search/{Query}`), add an **Advanced** button.
- Clicking **Advanced** navigates to a dedicated advanced search page, passing the current query as an optional seed.

Suggested route:
- `/search/advanced` (optional query string param, e.g. `?q=...`)

### 9.3 Advanced Search page (MQL editor)
Create a new Blazor page that provides a full-featured MQL editing and execution experience.

Functional requirements:
- A multi-line editor for MQL input (full query text).
- “Run” action that executes the current MQL string.
- Render matches (songs/albums/artists/playlists) as results.
- Preserve existing UX patterns used in Blazor (spinner via `MainLayoutProxyService`, Radzen components, localization via `L(...)`).

Technical requirements:
- The page must inject and call services directly, for example:
  - `SearchService` (current basic search service) and/or
  - A future `MqlSearchService` / `MqlQueryCompiler` that compiles and executes MQL.
- The page must resolve the current user context the same way existing pages do (via `ClaimsPrincipal` / `CurrentUser`), and pass user identifiers into service calls as needed for user-scoped fields (`rating`, `plays`, `starred`, etc.).
- No usage of `SearchController` / HTTP endpoints for running the query.

Suggested UI elements (v1):
- MQL editor area (multi-line text box) with:
  - placeholder examples
  - optional “Format” / “Validate” buttons (can be added later)
- Result type toggles (songs/albums/artists/playlists) similar to `SearchInclude`
- “Run” button
- Results area using existing result components (e.g., `ArtistDataInfoCardComponent` for artists).

### 9.4 Expected behavior and edge cases
- If the MQL query is empty: show no results and a helpful message.
- If MQL parsing/binding fails: show a user-friendly validation message and highlight the error location if available.
- Enforce the same safety limits described elsewhere in this spec (max query length, max results, regex guards).

### 9.5 Testing expectations
- Add Blazor component tests (where test infrastructure exists) for:
  - “Advanced” navigation from search page
  - Running a simple MQL query and rendering results
  - Handling parse errors

---

## 10. Milestones
**Milestone 1 (week 1): Songs MQL MVP**
- Parse endpoint (validate + normalize)
- Basic song queries (field filters, ranges, bool logic)
- Top N results + default sort
- Compile to EF Core expressions

**Milestone 2 (week 2): Expand song features**
- Date/time predicates (e.g., `last-week`)
- User-specific fields (e.g., `rating`, `plays`)
- Regex support (guarded)

**Milestone 3 (week 3): Albums/Artists + testing**
- Add album/artist fields + mappings
- Cross-entity queries (e.g., `artist:"Miles Davis" AND album:KindOfBlue`)
- Performance testing + optimizations

## 11. Open questions / decisions needed
1. **Meaning of `added` for songs**: we have `Song.CreatedAt` (library import time) and `UserSong.StarredAt` (user action time). This spec proposes:
   - `added:` → `Song.CreatedAt` (library-centric)
   - `starredAt:` → `UserSong.StarredAt` (user-centric)
   If you want “added” to be per-user, we should rename the library field to `imported:` and reserve `added:` for per-user.
2. **Regex strategy**: DB-native vs guarded in-process vs disallow in v1.
3. **Smart playlist schema**: does `Playlists` table already support a “smart query” discriminator/columns, or do we add a new table/type?

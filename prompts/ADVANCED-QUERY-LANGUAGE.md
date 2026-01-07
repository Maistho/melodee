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
- **Range**: `year:1970-1980` (inclusive, means `>= 1970 AND <= 1980`)
- **Boolean**: `(rock OR metal) AND NOT live`
- **Regex**: `title:/.*remix.*/i` (guarded, see §4.5)
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
  - Relative duration: `-7d`, `-3w`, `-12h` (interpreted as "since now minus X")
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
| `rating` | number | `rating:>=4` | user-specific: `Song.UserSongs.Where(us => us.UserId == userId).Select(us => us.Rating).FirstOrDefault()` |
| `plays` | number | `plays:0` `plays:>10` | user-specific: `Song.UserSongs.Where(us => us.UserId == userId).Select(us => us.PlayedCount).FirstOrDefault()` |
| `starred` | bool | `starred:true` | user-specific: `Song.UserSongs.Where(us => us.UserId == userId).Select(us => us.IsStarred).FirstOrDefault()` |
| `starredAt` | date/range | `starredAt:last-week` | user-specific: `Song.UserSongs.Where(us => us.UserId == userId).Select(us => us.StarredAt).FirstOrDefault()` |
| `lastPlayedAt` | date/range | `lastPlayedAt:-30d` | user-specific: `Song.UserSongs.Where(us => us.UserId == userId).Select(us => us.LastPlayedAt).FirstOrDefault()` |

**Security Note:** All user-specific fields use `Where(...).FirstOrDefault()` pattern to avoid N+1 queries and ensure proper parameterization. The service layer must include `Include(s => s.UserSongs.Where(us => us.UserId == userId))` for efficient loading.

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
| `rating` | number | `rating:>=4` | user-specific: `Album.UserAlbums.Where(ua => ua.UserId == userId).Select(ua => ua.Rating).FirstOrDefault()` |
| `plays` | number | `plays:>0` | user-specific: `Album.UserAlbums.Where(ua => ua.UserId == userId).Select(ua => ua.PlayedCount).FirstOrDefault()` |
| `starred` | bool | `starred:true` | user-specific: `Album.UserAlbums.Where(ua => ua.UserId == userId).Select(ua => ua.IsStarred).FirstOrDefault()` |
| `starredAt` | date/range | `starredAt:last-month` | user-specific: `Album.UserAlbums.Where(ua => ua.UserId == userId).Select(ua => ua.StarredAt).FirstOrDefault()` |
| `lastPlayedAt` | date/range | `lastPlayedAt:-30d` | user-specific: `Album.UserAlbums.Where(ua => ua.UserId == userId).Select(ua => ua.LastPlayedAt).FirstOrDefault()` |
| `added` | date/range | `added:-30d` | `Album.CreatedAt` (from `DataModelBase`) |

Notes:
- `Album.PlayedCount` / `Album.CalculatedRating` exist as global aggregates (inherited from `MetaDataModelBase`). If we want explicit global fields, consider `globalPlays` / `globalRating` aliases.

#### Artists fields (confirmed mappings + user-scoped options)
| Field | Type | Example | DB mapping (confirmed) |
|---|---|---|---|
| `artist` / `name` | string | `artist:"Miles Davis"` | `Artist.NameNormalized` (+ `AlternateNames` for free-text) |
| `rating` | number | `rating:>=4` | user-specific: `Artist.UserArtists.Where(ua => ua.UserId == userId).Select(ua => ua.Rating).FirstOrDefault()` |
| `starred` | bool | `starred:true` | user-specific: `Artist.UserArtists.Where(ua => ua.UserId == userId).Select(ua => ua.IsStarred).FirstOrDefault()` |
| `starredAt` | date/range | `starredAt:last-year` | user-specific: `Artist.UserArtists.Where(ua => ua.UserId == userId).Select(ua => ua.StarredAt).FirstOrDefault()` |
| `plays` | number | `plays:>0` | global: `Artist.PlayedCount` (inherited from `MetaDataModelBase`) |
| `added` | date/range | `added:last-month` | `Artist.CreatedAt` (from `DataModelBase`) |

Notes:
- Unlike songs/albums, `UserArtist` currently does not store a per-user `PlayedCount`, so `plays:` is global-only for artists (unless we extend the schema).

### 4.5 Regex Support & Security Controls

**⚠️ CRITICAL SECURITY REQUIREMENT:** Regex patterns must be strictly controlled to prevent ReDoS attacks and data exposure.

#### Implementation Strategy
Regex support is **guarded** and **optional** in v1:

1. **Server-side regex** (preferred for small result sets):
   - Only available for PostgreSQL (native `~` operator)
   - Query must return ≤ 1000 records before regex evaluation
   - Pattern complexity limited to 100 characters
   - Timeout: 500ms per query

2. **Client-side regex** (fallback):
   - Results are first filtered by other criteria
   - Regex applied to limited result set in application layer
   - Maximum result set size: 1000 records

3. **Validation rules**:
   ```csharp
   const int MAX_REGEX_PATTERN_LENGTH = 100;
   const int MAX_RESULT_SET_FOR_REGEX = 1000;
   const int REGEX_TIMEOUT_MS = 500;
   
   // Prohibited patterns (ReDoS prevention)
   private static readonly string[] ProhibitedPatterns = new[] {
       @"(.*)*", @"(.+)+", @"([a-z]*)*", @"([a-z]+)+"
   };
   ```

4. **Alternative for v1**: Consider **disabling regex entirely** and providing:
   - `contains:` (substring match)
   - `startsWith:` (prefix match)
   - `endsWith:` (suffix match)
   - `wildcard:` (SQL LIKE with `%` and `_`)

**Reference:** OWASP A03:2021 - Injection, Performance Optimization - Input Validation

## 5. API design

### 5.1 Parse endpoint (autocomplete + validation)
Add an endpoint to validate and return a parse tree + normalized form.

- `POST /api/v1/query/parse`
- Request: `{ "entity": "songs|albums|artists", "query": "..." }`
- Response (200):
  - `normalizedQuery`: canonical spacing/casing
  - `ast`: simplified nodes for UI introspection
  - `warnings`: e.g., "regex will run client-side", "top capped at 100"
- Response (400): structured validation error:
  - `errorCode`: `MQL_PARSE_ERROR`, `MQL_UNKNOWN_FIELD`, `MQL_INVALID_LITERAL`, `MQL_REGEX_TOO_COMPLEX`, etc.
  - `message`: user-friendly
  - `position`: index + length for UI highlighting
  - `suggestions`: array of suggested fixes (e.g., field name corrections)

**Security Requirements:**
- Rate limit: 10 requests per minute per user
- Input size limit: 500 characters
- Timeout: 200ms

### 5.2 Search endpoints
Integrate MQL into listing endpoints without breaking existing contracts.

Option A (lowest friction): add a new optional query param `q` for MQL; keep current paging/order params.

Rules:
- If `q` is present and non-empty, MQL drives filtering (and may override order/limit via `top`).
- If `q` is absent, existing behavior remains.

**Security Requirements:**
- All MQL queries must be logged (sanitized) for audit purposes
- Implement query complexity scoring to prevent DoS
- Maximum of 20 field filters per query
- Maximum recursion depth for parentheses: 10 levels

## 6. Implementation plan

### 6.1 Components (new)

#### A. Input Validation Layer
```csharp
public class MqlValidationResult
{
    public bool IsValid { get; set; }
    public List<MqlError> Errors { get; set; }
    public int ComplexityScore { get; set; }
}

public class MqlValidator
{
    private const int MAX_QUERY_LENGTH = 500;
    private const int MAX_FIELD_COUNT = 20;
    private const int MAX_RECURSION_DEPTH = 10;
    private const int MAX_REGEX_COMPLEXITY = 100;
    
    public MqlValidationResult Validate(string query, string entity)
    {
        // 1. Length check
        if (query.Length > MAX_QUERY_LENGTH)
            return Invalid("MQL_QUERY_TOO_LONG", $"Maximum length is {MAX_QUERY_LENGTH}");
        
        // 2. Field count check (count colons)
        var fieldCount = query.Count(c => c == ':');
        if (fieldCount > MAX_FIELD_COUNT)
            return Invalid("MQL_TOO_MANY_FIELDS", $"Maximum {MAX_FIELD_COUNT} field filters allowed");
        
        // 3. Recursion depth for parentheses
        var depth = 0;
        var maxDepth = 0;
        foreach (var c in query)
        {
            if (c == '(') depth++;
            if (c == ')') depth--;
            if (depth > maxDepth) maxDepth = depth;
            if (depth < 0) return Invalid("MQL_UNBALANCED_PARENS", "Unbalanced parentheses");
        }
        if (maxDepth > MAX_RECURSION_DEPTH)
            return Invalid("MQL_TOO_DEEP", $"Maximum nesting depth is {MAX_RECURSION_DEPTH}");
        
        // 4. Regex validation
        var regexMatches = Regex.Matches(query, @"/(.+?)/(i|g|ig)?");
        foreach (Match match in regexMatches)
        {
            var pattern = match.Groups[1].Value;
            if (pattern.Length > MAX_REGEX_COMPLEXITY)
                return Invalid("MQL_REGEX_TOO_COMPLEX", $"Regex pattern too long (max {MAX_REGEX_COMPLEXITY})");
            
            if (ProhibitedPatterns.Any(p => pattern.Contains(p)))
                return Invalid("MQL_REGEX_DANGEROUS", "Dangerous regex pattern detected");
        }
        
        return new MqlValidationResult { IsValid = true, ComplexityScore = CalculateComplexity(query) };
    }
}
```

#### B. Parser
- Tokenizer with position tracking for error reporting
- AST generation with node types for each operator/literal
- **No Dynamic LINQ** - pure expression tree building

#### C. Compiler to EF Core
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

#### D. Security & Performance Monitoring
```csharp
public class MqlQueryMetrics
{
    public string QueryHash { get; set; }
    public long ExecutionTimeMs { get; set; }
    public int ResultCount { get; set; }
    public int ComplexityScore { get; set; }
    public bool UsedRegex { get; set; }
    public string UserId { get; set; }
    public DateTime Timestamp { get; set; }
}

// Log slow/complex queries for review
public class MqlSecurityMonitor
{
    private const int SLOW_QUERY_THRESHOLD = 1000; // ms
    private const int HIGH_COMPLEXITY_THRESHOLD = 50;
    
    public void LogQuery(MqlQueryMetrics metrics)
    {
        if (metrics.ExecutionTimeMs > SLOW_QUERY_THRESHOLD)
            _logger.LogWarning("Slow MQL query detected: {QueryHash} took {Time}ms", 
                metrics.QueryHash, metrics.ExecutionTimeMs);
        
        if (metrics.ComplexityScore > HIGH_COMPLEXITY_THRESHOLD)
            _logger.LogWarning("High complexity MQL query: {QueryHash} score {Score}", 
                metrics.QueryHash, metrics.ComplexityScore);
    }
}
```

## 7. Security Testing Requirements

### 7.1 Test Coverage
Before implementation, add the following test categories:

```csharp
public class MqlSecurityTests
{
    [Fact]
    public void ReDoS_Prevention()
    {
        // Should reject or timeout on dangerous patterns
        var dangerous = new[] { "(.*)*", "(.+)+", "([a-z]*)*" };
        foreach (var pattern in dangerous)
        {
            var result = _validator.Validate($"title:/{pattern}/", "songs");
            Assert.False(result.IsValid);
        }
    }
    
    [Fact]
    public void SQL_Injection_Prevention()
    {
        // Should not allow SQL keywords to bypass
        var malicious = new[] {
            "title:' OR 1=1 --",
            "artist:\"; DROP TABLE Songs; --",
            "album: UNION SELECT * FROM Users"
        };
        foreach (var query in malicious)
        {
            var result = _validator.Validate(query, "songs");
            Assert.False(result.IsValid);
        }
    }
    
    [Fact]
    public void Field_Authorization()
    {
        // Ensure user can't query fields they shouldn't access
        // (e.g., admin-only fields, other users' data)
    }
    
    [Fact]
    public void DoS_Resource_Limits()
    {
        // Query with 100 fields should fail
        // Query with 20 levels of nesting should fail
        // Very long query should fail
    }
    
    [Fact]
    public void Regex_Guards()
    {
        // Pattern length limit
        // Result set size limit
        // Timeout enforcement
    }
}
```

### 7.2 Fuzzing Tests
```bash
# Run during CI
dotnet test --filter "Category=Security"
# Use fuzzing library for parser input
```

## 8. Performance Testing Requirements

### 8.1 Benchmark Scenarios
```csharp
[Fact]
public void Performance_LargeLibrary()
{
    // Test with 100k+ songs
    // Measure query compilation time
    // Measure execution time
    // Verify no N+1 queries
}
```

### 8.2 Index Requirements
Before go-live, verify indexes exist:
```sql
-- Required indexes for MQL performance
CREATE INDEX IX_Song_TitleNormalized ON Songs (TitleNormalized);
CREATE INDEX IX_Song_AlbumId ON Songs (AlbumId);
CREATE INDEX IX_Album_NameNormalized ON Albums (NameNormalized);
CREATE INDEX IX_Album_ArtistId ON Albums (ArtistId);
CREATE INDEX IX_Album_ReleaseDate ON Albums (ReleaseDate);
CREATE INDEX IX_UserSong_UserId_SongId ON UserSongs (UserId, SongId);
CREATE INDEX IX_UserSong_Rating ON UserSongs (UserId, Rating);
-- etc.
```

## 9. Melodee.Blazor UI requirements (no API; DI services only)

### 9.1 Overview
MQL must be usable directly inside the `Melodee.Blazor` application.

Key requirement: **The Blazor UI must not call HTTP API endpoints to execute MQL.**
Instead, it should execute queries by injecting the relevant in-process services (e.g., `SearchService` or a future `MqlSearchService`) into the Razor page via DI and calling them directly.

### 9.2 Entry point: "Advanced" button on Search page
- On the existing search results page (`Search.razor`, currently routed as `/search/{Query}`), add an **Advanced** button.
- Clicking **Advanced** navigates to a dedicated advanced search page, passing the current query as an optional seed.

Suggested route:
- `/search/advanced` (optional query string param, e.g. `?q=...`)

### 9.3 Advanced Search page (MQL editor)
Create a new Blazor page that provides a full-featured MQL editing and execution experience.

Functional requirements:
- A multi-line editor for MQL input (full query text).
- "Run" action that executes the current MQL string.
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
  - optional "Format" / "Validate" buttons (can be added later)
- Result type toggles (songs/albums/artists/playlists) similar to `SearchInclude`
- "Run" button
- Results area using existing result components (e.g., `ArtistDataInfoCardComponent` for artists).

### 9.4 Expected behavior and edge cases
- If the MQL query is empty: show no results and a helpful message.
- If MQL parsing/binding fails: show a user-friendly validation message and highlight the error location if available.
- Enforce the same safety limits described elsewhere in this spec (max query length, max results, regex guards).

### 9.5 Testing expectations
- Add Blazor component tests (where test infrastructure exists) for:
  - "Advanced" navigation from search page
  - Running a simple MQL query and rendering results
  - Handling parse errors

---

## 10. Milestones
**Milestone 1 (week 1): Songs MQL MVP**
- Parse endpoint (validate + normalize)
- Basic song queries (field filters, ranges, bool logic)
- Top N results + default sort
- Compile to EF Core expressions
- **Security:** Input validation layer with all limits
- **Testing:** Unit tests for validation, security tests

**Milestone 2 (week 2): Expand song features**
- Date/time predicates (e.g., `last-week`)
- User-specific fields (e.g., `rating`, `plays`)
- Regex support (guarded) OR alternative operators
- **Performance:** Index verification and query plan analysis
- **Testing:** Fuzzing tests, performance benchmarks

**Milestone 3 (week 3): Albums/Artists + testing**
- Add album/artist fields + mappings
- Cross-entity queries (e.g., `artist:"Miles Davis" AND album:KindOfBlue`)
- Performance testing + optimizations
- **Security:** End-to-end security review
- **Testing:** Integration tests, load tests

## 11. Open questions / decisions needed
1. **Meaning of `added` for songs**: we have `Song.CreatedAt` (library import time) and `UserSong.StarredAt` (user action time). This spec proposes:
   - `added:` → `Song.CreatedAt` (library-centric)
   - `starredAt:` → `UserSong.StarredAt` (user-centric)
   If you want "added" to be per-user, we should rename the library field to `imported:` and reserve `added:` for per-user.

2. **Regex strategy**: DB-native vs guarded in-process vs disallow in v1.
   - **Recommendation:** Start without regex, provide `contains:`, `startsWith:`, `endsWith:`, `wildcard:` as alternatives
   - If regex is required, implement with strict guards (§4.5)

3. **Smart playlist schema**: does `Playlists` table already support a "smart query" discriminator/columns, or do we add a new table/type?

4. **Security decision**: Should we log all MQL queries for audit purposes? (Recommended: yes, sanitized)

5. **Performance decision**: What is acceptable query response time for 100k song library? (Recommended: < 500ms for simple queries, < 2s for complex)

## 12. Security Checklist (Pre-Implementation)

Before writing any code, verify:
- [ ] All input validation rules defined and tested
- [ ] Rate limiting strategy documented
- [ ] Regex guards specified (or alternative operators chosen)
- [ ] N+1 query prevention strategy documented
- [ ] Field authorization requirements identified
- [ ] DoS protection measures in place
- [ ] Security test coverage requirements defined
- [ ] Audit logging requirements documented

## 13. Performance Checklist (Pre-Implementation)

Before writing any code, verify:
- [ ] Required database indexes identified
- [ ] Query complexity scoring algorithm defined
- [ ] Result set size limits specified
- [ ] Timeout values defined
- [ ] Benchmark scenarios documented
- [ ] Monitoring/alerting strategy for slow queries

---

**Document Version:** 1.1  
**Last Updated:** 2026-01-06  
**Security Review Status:** Required  
**Performance Review Status:** Required

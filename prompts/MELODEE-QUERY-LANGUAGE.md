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

#### 4.1.1 Free-Text Search Sanitization

**⚠️ CRITICAL SECURITY REQUIREMENT:** Free-text search input must be sanitized to prevent injection attacks and regex bypass.

Free-text terms map to `Contains` or `ILike` queries. Without sanitization, special characters can cause:
- Syntax errors in regex patterns
- Potential injection if escaping is inconsistent
- Unpredictable query results

**Implementation:**

```csharp
/// <summary>
/// Sanitizes free-text search input to prevent injection and regex bypass.
/// </summary>
public static class MqlTextSanitizer
{
    // Characters that have special meaning in various contexts
    private static readonly char[] SpecialChars = 
    { 
        '\'', '"', '\\', ';', '-', '(', ')', '[', ']', 
        '{', '}', '|', '*', '?', '.', '+', '^', '$', 
        '<', '>', '#', '&', '%', '~', '`', '\n', '\r', '\t'
    };

    // Patterns that could bypass validation if not escaped
    private static readonly string[] DangerousPatterns =
    {
        "--", "; DROP", "; DELETE", "; UPDATE", "UNION SELECT",
        "EXEC(", "xp_", "0x", "\\x"
    };

    /// <summary>
    /// Sanitizes input for free-text search, escaping special characters
    /// that could cause injection or parsing issues.
    /// </summary>
    public static string SanitizeForFreeText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sanitized = new StringBuilder(input.Length);
        
        foreach (var c in input)
        {
            if (SpecialChars.Contains(c))
                sanitized.Append('\\');
            sanitized.Append(c);
        }

        var result = sanitized.ToString();
        
        // Check for dangerous patterns after sanitization
        foreach (var pattern in DangerousPatterns)
        {
            if (result.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                // Flag for review but don't necessarily reject
                // User might legitimately search for "DROP" in a title
                MqlSecurityMonitor.Instance.LogWarning(
                    $"Potential dangerous pattern detected: {pattern}");
            }
        }

        return result;
    }

    /// <summary>
    /// Sanitizes field values that will be embedded in regex patterns.
    /// </summary>
    public static string SanitizeForRegex(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            // Escape regex metacharacters
            if (Regex.Escape(c.ToString()).Length > 1)
                sb.Append('\\');
            sb.Append(c);
        }
        return sb.ToString();
    }
}
```

**Integration Point:**

```csharp
// In MqlValidator.cs - Validate method
public MqlValidationResult Validate(string query, string entity)
{
    // ... existing validation ...

    // New: Sanitize free-text terms
    var freeTextTerms = ExtractFreeTextTerms(query);
    foreach (var term in freeTextTerms)
    {
        var sanitized = MqlTextSanitizer.SanitizeForFreeText(term);
        if (sanitized != term)
        {
            // Log normalization but continue validation with sanitized version
            AddWarning($"Free-text term normalized: '{term}' -> '{sanitized}'");
        }
    }

    return result;
}
```

**Reference:** OWASP A03:2021 - Injection, Security and OWASP Guidelines

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

#### 4.4.1 Eager Loading Requirements for User-Scoped Fields

**⚠️ CRITICAL PERFORMANCE REQUIREMENT:** User-scoped fields (`rating`, `plays`, `starred`, etc.) require **eager loading** via `Include()` to avoid N+1 query problems.

**The N+1 Problem Illustrated:**

Without eager loading:
```csharp
// ❌ BAD: Triggers N+1 queries
var songs = await context.Songs.Where(MqlCompiler.Compile("rating:>=4")).ToListAsync();
// For each song, querying UserSongs:
foreach (var song in songs)
{
    var rating = song.UserSongs.FirstOrDefault(us => us.UserId == userId)?.Rating; // N+1!
}
```

With eager loading:
```csharp
// ✅ GOOD: Single query with JOIN
var songs = await context.Songs
    .Include(s => s.UserSongs.Where(us => us.UserId == userId))
    .Where(MqlCompiler.Compile("rating:>=4"))
    .ToListAsync();
```

**Required Include Patterns by Entity:**

**Songs:**
```csharp
IQueryable<Song> query = _dbContext.Songs
    .AsNoTracking()
    .Include(s => s.UserSongs.Where(us => us.UserId == userId))  // REQUIRED
    .Include(s => s.Album)
    .Include(s => s.Album.Artist);
```

**Albums:**
```csharp
IQueryable<Album> query = _dbContext.Albums
    .AsNoTracking()
    .Include(a => a.UserAlbums.Where(ua => ua.UserId == userId))  // REQUIRED
    .Include(a => a.Artist);
```

**Artists:**
```csharp
IQueryable<Artist> query = _dbContext.Artists
    .AsNoTracking()
    .Include(a => a.UserArtists.Where(ua => ua.UserId == userId))  // REQUIRED
    .Include(a => a.Songs.Take(0));  // Optional: prevents lazy loading
```

**Service Layer Integration:**

The service layer **MUST** apply these includes before passing to MQL compiler:

```csharp
public async Task<PagedResult<Song>> SearchSongsAsync(
    MqlQuery query, 
    Guid userId, 
    CancellationToken cancellationToken)
{
    var songQuery = _dbContext.Songs
        .AsNoTracking()
        .Include(s => s.UserSongs.Where(us => us.UserId == userId))
        .Include(s => s.Album)
            .ThenInclude(a => a.Artist);

    // Now safe to compile and apply MQL
    var predicate = _mqlCompiler.Compile<Song>(query.NormalizedQuery);
    var filteredQuery = songQuery.Where(predicate);

    return await filteredQuery.ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
}
```

**Verification Tests:**

Add integration tests to verify no N+1 queries:

```csharp
public class MqlN1QueryTests : IntegrationTestBase
{
    [Fact]
    public async Task UserScopedFields_DoNotCauseN1Queries()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = "rating:>=4 plays:>10";
        
        // Act
        using var activity = new ActivitySource("Test").StartActivity();
        var metrics = new List<DbMetric>();
        
        _dbContext.GetInfrastructure<ILogger<SqlLoggerCategory.Database>>()
            .ShouldCaptureLogging = true;
        
        var songs = await _songService.SearchSongsAsync(
            new MqlQuery(query), 
            userId);

        // Assert - verify query count
        // This is a conceptual test - actual implementation depends on logging setup
        var queryCount = GetQueryCount(); // Implementation-specific
        Assert.Equal(1, queryCount); // Should be single query, not N+1
    }
}
```

**Reference:** Performance Optimization - Avoid N+1 Queries

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

#### 5.1.1 API Response Examples

**Success Response (200 OK):**

**Request:**
```http
POST /api/v1/query/parse
Content-Type: application/json

{
  "entity": "songs",
  "query": "artist:\"Pink Floyd\" AND year:>=1970"
}
```

**Response:**
```json
{
  "normalizedQuery": "artist:\"Pink Floyd\" AND year:>=1970",
  "ast": {
    "type": "AndExpression",
    "left": {
      "type": "FieldExpression",
      "field": "artist",
      "operator": "Equals",
      "value": "Pink Floyd"
    },
    "right": {
      "type": "FieldExpression",
      "field": "year",
      "operator": "GreaterThanOrEqual",
      "value": 1970
    }
  },
  "warnings": [],
  "estimatedComplexity": 2,
  "valid": true
}
```

**Error Responses (400 Bad Request):**

**Parse Error - Unbalanced Parentheses:**

**Request:**
```http
POST /api/v1/query/parse
Content-Type: application/json

{
  "entity": "songs",
  "query": "artist:\"Pink Floyd\" AND (year:>=1970"
}
```

**Response:**
```json
{
  "errorCode": "MQL_PARSE_ERROR",
  "message": "Unbalanced parentheses: missing closing ')'",
  "position": {
    "start": 42,
    "end": 42,
    "line": 1,
    "column": 43
  },
  "suggestions": [
    {
      "text": "Add closing parenthesis",
      "cursorPosition": 42
    }
  ],
  "normalizedQuery": "artist:\"Pink Floyd\" AND (year:>=1970)",
  "context": {
    "prefix": "artist:\"Pink Floyd\" AND (year:>=1970",
    "suffix": "",
    "highlight": {
      "start": 42,
      "end": 42
    }
  },
  "timestamp": "2026-01-06T14:30:00Z"
}
```

**Unknown Field:**

**Request:**
```http
POST /api/v1/query/parse
Content-Type: application/json

{
  "entity": "songs",
  "query": "artistt:Beatles"
}
```

**Response:**
```json
{
  "errorCode": "MQL_UNKNOWN_FIELD",
  "message": "Unknown field 'artistt'. Did you mean 'artist'?",
  "position": {
    "start": 0,
    "end": 7
  },
  "suggestions": [
    {
      "text": "artist",
      "confidence": 0.95,
      "description": "Search by artist name"
    }
  ],
  "similarFields": [
    {
      "name": "artist",
      "distance": 1,
      "description": "Artist name field"
    }
  ],
  "validFields": ["title", "artist", "album", "genre", "year", "duration", "bpm", "rating", "plays", "starred", "starredAt", "lastPlayedAt"],
  "timestamp": "2026-01-06T14:30:00Z"
}
```

**Invalid Literal:**

**Request:**
```http
POST /api/v1/query/parse
Content-Type: application/json

{
  "entity": "songs",
  "query": "year:invalid"
}
```

**Response:**
```json
{
  "errorCode": "MQL_INVALID_LITERAL",
  "message": "Invalid literal for field 'year': 'invalid'",
  "details": {
    "field": "year",
    "expectedType": "number",
    "receivedValue": "invalid",
    "examples": [
      "year:1979",
      "year:>=2000",
      "year:1970-1980"
    ]
  },
  "position": {
    "start": 5,
    "end": 12
  },
  "suggestions": [
    {
      "text": "year:1979",
      "description": "Match specific year"
    },
    {
      "text": "year:>=1970",
      "description": "Match year greater than or equal"
    }
  ],
  "timestamp": "2026-01-06T14:30:00Z"
}
```

**Regex Too Complex:**

**Request:**
```http
POST /api/v1/query/parse
Content-Type: application/json

{
  "entity": "songs",
  "query": "title:/((.*)*){1000}/"
}
```

**Response:**
```json
{
  "errorCode": "MQL_REGEX_TOO_COMPLEX",
  "message": "Regex pattern exceeds complexity limits",
  "details": {
    "patternLength": 19,
    "maxLength": 100,
    "warning": "Pattern may cause ReDoS vulnerability",
    "suggestedAlternatives": [
      {
        "operator": "contains",
        "example": "title:contains(\"remix\")",
        "description": "Simple substring match"
      },
      {
        "operator": "startsWith",
        "example": "title:startsWith(\"remix\")",
        "description": "Prefix match"
      },
      {
        "operator": "endsWith",
        "example": "title:endsWith(\"remix\")",
        "description": "Suffix match"
      },
      {
        "operator": "wildcard",
        "example": "title:wildcard(\"*remix*\")",
        "description": "SQL LIKE pattern match"
      }
    ]
  },
  "position": {
    "start": 6,
    "end": 25
  },
  "timestamp": "2026-01-06T14:30:00Z"
}
```

**Forbidden Field (Authorization):**

**Request:**
```http
POST /api/v1/query/parse
Content-Type: application/json

{
  "entity": "songs",
  "query": "rating:>=4"
}
```

**Response:**
```json
{
  "errorCode": "MQL_FORBIDDEN_FIELD",
  "message": "Cannot query user-specific field 'rating' without authentication",
  "details": {
    "field": "rating",
    "fieldType": "user-scoped",
    "requiresAuth": true
  },
  "documentation": "https://api.melodee.io/docs/mql#user-scoped-fields",
  "timestamp": "2026-01-06T14:30:00Z"
}
```

**Rate Limited (429 Too Many Requests):**

**Response:**
```json
{
  "errorCode": "RATE_LIMIT_EXCEEDED",
  "message": "Rate limit exceeded. Please try again later.",
  "details": {
    "limit": 10,
    "window": "per minute",
    "retryAfter": 30,
    "resetAt": "2026-01-06T14:31:00Z"
  },
  "headers": {
    "Retry-After": "30",
    "X-RateLimit-Limit": "10",
    "X-RateLimit-Remaining": "0",
    "X-RateLimit-Reset": "2026-01-06T14:31:00Z"
  },
  "timestamp": "2026-01-06T14:30:00Z"
}
```

#### 5.1.2 Frontend Error Display Guidelines

```typescript
interface MqlError {
  errorCode: string;
  message: string;
  position?: {
    start: number;
    end: number;
    line?: number;
    column?: number;
  };
  suggestions?: Array<{
    text: string;
    description?: string;
    cursorPosition?: number;
  }>;
  similarFields?: Array<{
    name: string;
    distance: number;
    description: string;
  }>;
}

// Example error display component
function ErrorDisplay({ error }: { error: MqlError }) {
  return (
    <div className="mql-error">
      <div className="error-header">
        <span className={`error-icon ${error.errorCode}`} />
        <span className="error-code">{error.errorCode}</span>
      </div>
      
      <p className="error-message">{error.message}</p>
      
      {error.position && (
        <QueryHighlighter
          query={currentQuery}
          errorPosition={error.position}
        />
      )}
      
      {error.suggestions && (
        <div className="suggestions">
          <h4>Did you mean?</h4>
          <ul>
            {error.suggestions.map((s, i) => (
              <li key={i}>
                <button onClick={() => applySuggestion(s.text)}>
                  {s.text}
                </button>
                {s.description && <span> - {s.description}</span>}
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
```

**Reference:** Markdown Content Rules

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

#### 6.1.1 Expression Tree Caching

**⚠️ PERFORMANCE REQUIREMENT:** Compiled expressions must be cached to avoid recompilation overhead.

Parsing and compiling MQL queries to `Expression<Func<TEntity, bool>>` involves:
- Tokenization and parsing
- AST construction
- Expression tree building

For repeated queries (e.g., smart playlists), this is wasteful. Caching compiled expressions improves:
- Response time for saved smart playlists
- Performance for complex boolean logic
- Database query plan reuse

**Implementation:**

```csharp
/// <summary>
/// Caches compiled MQL expressions to avoid recompilation overhead.
/// </summary>
public class MqlExpressionCache
{
    private static readonly MemoryCache<string, CachedExpression> _cache = new(
        new MemoryCacheOptions
        {
            SizeLimit = 1000, // Maximum number of entries
            ExpirationScanFrequency = TimeSpan.FromMinutes(5)
        });

    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(30);
    private readonly IMqlLogger _logger;

    public MqlExpressionCache(IMqlLogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets or creates a compiled expression for the given normalized query.
    /// </summary>
    /// <typeparam name="TEntity">Entity type for the expression</typeparam>
    /// <param name="normalizedQuery">Normalized MQL query string</param>
    /// <param name="factory">Factory to create expression if not cached</param>
    /// <param name="ttl">Optional TTL for this entry</param>
    /// <returns>Compiled expression</returns>
    public Expression<Func<TEntity, bool>> GetOrCreate<TEntity>(
        string normalizedQuery,
        Func<Expression<Func<TEntity, bool>>> factory,
        TimeSpan? ttl = null)
    {
        var cacheKey = $"{typeof(TEntity).Name}:{normalizedQuery}";
        
        if (_cache.TryGetValue(cacheKey, out CachedExpression cached))
        {
            _logger.LogDebug("Cache hit for query: {Query}", normalizedQuery);
            return (Expression<Func<TEntity, bool>>)cached.Expression;
        }

        var stopwatch = Stopwatch.StartNew();
        var expression = factory();
        stopwatch.Stop();

        var entry = new CachedExpression
        {
            Expression = expression,
            CreatedAt = DateTime.UtcNow,
            HitCount = 1,
            CompilationTimeMs = stopwatch.ElapsedMilliseconds
        };

        _cache.Set(cacheKey, entry, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? _defaultTtl,
            Size = 1,
            ExpirationTokens = { new CancellationChangeToken(CancellationToken.None) }
        });

        _logger.LogDebug(
            "Compiled and cached expression for query: {Query} ({Time}ms)",
            normalizedQuery,
            stopwatch.ElapsedMilliseconds);

        return expression;
    }

    /// <summary>
    /// Clears entries for a specific entity type.
    /// </summary>
    public void ClearForEntity(string entityTypeName)
    {
        // Iterate and remove matching entries
        // In production, use named cache partitions for efficiency
    }

    /// <summary>
    /// Gets cache statistics for monitoring.
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        return new CacheStatistics
        {
            Count = _cache.Count,
            // Additional metrics can be tracked with custom implementation
        };
    }

    private class CachedExpression
    {
        public LambdaExpression Expression { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int HitCount { get; set; }
        public long CompilationTimeMs { get; set; }
    }
}
```

**Cache Invalidation Strategy:**

```csharp
/// <summary>
/// Handles cache invalidation based on data changes.
/// </summary>
public class MqlCacheInvalidator
{
    private readonly MqlExpressionCache _cache;

    // Subscribe to entity change events
    public void OnEntityChanged<TEntity>(TEntity entity)
    {
        // Invalidate all cache entries that might be affected
        // For simplicity, invalidate entire entity cache
        // Production may need more granular invalidation
        _cache.ClearForEntity(typeof(TEntity).Name);
        
        MqlSecurityMonitor.Instance.LogInfo(
            $"Cache invalidated for {typeof(TEntity).Name} due to entity change");
    }
}
```

**Reference:** Performance Optimization Best Practices - Caching

#### 6.1.2 User-Scoped Field Authorization

**⚠️ CRITICAL SECURITY REQUIREMENT:** User-scoped fields require explicit authorization checks.

User-specific fields (`rating`, `plays`, `starred`, etc.) expose private user data. Without authorization checks:
- Users could query `rating:>0` across all users to enumerate listening habits
- Privacy violations through data aggregation
- Potential stalking or surveillance risks

**Implementation:**

```csharp
/// <summary>
/// Handles authorization for MQL queries with user-scoped fields.
/// </summary>
public class MqlAuthorizationService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMqlFieldInfoProvider _fieldProvider;

    public MqlAuthorizationService(
        ICurrentUserService currentUserService,
        IMqlFieldInfoProvider fieldProvider)
    {
        _currentUserService = currentUserService;
        _fieldProvider = fieldProvider;
    }

    /// <summary>
    /// Checks if the current user is authorized to query the specified fields.
    /// </summary>
    public AuthorizationResult AuthorizeQuery(
        string query,
        string entityType,
        Guid? targetUserId = null)
    {
        var parsedQuery = _fieldProvider.ParseQuery(query);
        var userScopedFields = parsedQuery.Fields
            .Where(f => _fieldProvider.IsUserScoped(f.Name, entityType))
            .ToList();

        if (!userScopedFields.Any())
            return AuthorizationResult.Success();

        var currentUserId = _currentUserService.UserId;
        
        // Allow querying own data
        if (targetUserId == null || targetUserId == currentUserId)
            return AuthorizationResult.Success();

        // Block queries on other users' private data
        return AuthorizationResult.Failure(
            ErrorCodes.MQL_FORBIDDEN_USER_DATA,
            "Cannot query user-specific fields for other users",
            new AuthorizationDetails
            {
                RequestedUserId = targetUserId,
                CurrentUserId = currentUserId,
                RestrictedFields = userScopedFields.Select(f => f.Name).ToList()
            });
    }

    /// <summary>
    /// Validates that the query doesn't attempt to enumerate other users.
    /// </summary>
    public async Task<AuthorizationResult> AuthorizeFullQueryAsync(
        MqlBoundQuery query,
        CancellationToken cancellationToken)
    {
        var violations = new List<AuthorizationViolation>();

        // Check for user ID enumeration patterns
        if (query.ContainsUserIdEnumeration())
        {
            violations.Add(new AuthorizationViolation
            {
                Type = ViolationType.UserEnumeration,
                Message = "Query attempts to enumerate user data"
            });
        }

        // Check for cross-user rating comparisons
        if (query.HasCrossUserComparisons())
        {
            violations.Add(new AuthorizationViolation
            {
                Type = ViolationType.CrossUserComparison,
                Message = "Cross-user comparisons are not allowed"
            });
        }

        if (violations.Any())
            return AuthorizationResult.Failure(violations);

        return AuthorizationResult.Success();
    }
}
```

**Integration in Validator:**

```csharp
public class MqlValidator
{
    private readonly MqlAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;

    public MqlValidationResult Validate(string query, string entity)
    {
        // ... existing validation ...

        // New: Authorization check for user-scoped fields
        var targetUserId = ExtractTargetUserId(query); // If implementing cross-user queries
        var authResult = _authorizationService.AuthorizeQuery(query, entity, targetUserId);
        
        if (!authResult.IsAuthorized)
        {
            return Invalid(
                ErrorCodes.MQL_FORBIDDEN_FIELD,
                "Cannot query user-specific fields: " + string.Join(", ", authResult.Details.RestrictedFields),
                additionalDetails: new
                {
                    reason = "User-scoped fields require authentication",
                    fields = authResult.Details.RestrictedFields
                });
        }

        return new MqlValidationResult { IsValid = true };
    }
}
```

**Security Test Cases:**

```csharp
public class MqlAuthorizationTests
{
    [Fact]
    public void UserQueryingOwnData_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userService.Setup(u => u.UserId).Returns(userId);
        
        // Act
        var result = _authService.AuthorizeQuery(
            "rating:>=4 plays:>0",
            "songs",
            targetUserId: userId);

        // Assert
        Assert.True(result.IsAuthorized);
    }

    [Fact]
    public void UserQueryingOtherUserData_ShouldFail()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _userService.Setup(u => u.UserId).Returns(currentUserId);
        
        // Act
        var result = _authService.AuthorizeQuery(
            "rating:>=4 plays:>0",
            "songs",
            targetUserId: otherUserId);

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Equal(ErrorCodes.MQL_FORBIDDEN_USER_DATA, result.ErrorCode);
    }

    [Fact]
    public void CrossUserEnumeration_ShouldBeBlocked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userService.Setup(u => u.UserId).Returns(userId);
        
        // Act
        var result = _authService.AuthorizeQuery(
            "userId:!=CURRENT_USER rating:>=4",  // Attempt to enumerate
            "songs");

        // Assert
        Assert.False(result.IsAuthorized);
    }
}
```

**Reference:** Security and OWASP Guidelines - A01: Broken Access Control

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

### 7.3 Comprehensive Testing Requirements

**⚠️ REQUIREMENT:** The MQL implementation must meet comprehensive testing standards before production release.

#### 7.3.1 Test Coverage Targets

| Component | Minimum Coverage | Target Coverage |
|-----------|-----------------|-----------------|
| Parser (Tokenizer + AST) | 90% | 95% |
| Validator | 95% | 100% |
| Compiler (Expression Tree) | 85% | 90% |
| Authorization Service | 100% | 100% |
| Text Sanitizer | 90% | 95% |
| Cache Service | 80% | 90% |
| **Overall Minimum** | **85%** | **90%** |

**CI Enforcement:**
```yaml
# azure-pipelines.yml or GitHub Actions
- task: DotNetCoreCLI@2
  displayName: 'Run tests with coverage'
  inputs:
    command: test
    projects: '**/*Tests.csproj'
    arguments: '--collect:"XPlat Code Coverage" --verbosity minimal'
    
- task: PublishCodeCoverageResults@1
  displayName: 'Publish coverage report'
  inputs:
    codeCoverageTool: 'Cobertura'
    summaryFileLocation: '$(Build.SourcesDirectory)/**/coverage.cobertura.xml'
    
- task: DotNetCoreCLI@2
  displayName: 'Verify coverage thresholds'
  inputs:
    command: custom
    custom: 'coverage'
    arguments: 'verify --threshold 85 --verbose'
```

#### 7.3.2 Integration Tests

**Full Pipeline Integration Tests:**

```csharp
/// <summary>
/// Tests the complete MQL query pipeline: Parse → Compile → Execute
/// </summary>
public class MqlFullPipelineIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task Parse_Compile_Execute_FullPipeline()
    {
        // Arrange
        var query = "artist:\"Pink Floyd\" AND year:>=1970";
        
        // Act - Parse
        var parseResult = await _parseService.ParseAsync(query, "songs");
        Assert.True(parseResult.IsValid);
        
        // Act - Compile
        var expression = await _compiler.CompileAsync<Song>(parseResult.NormalizedQuery);
        
        // Act - Execute
        var results = await _songService.QueryAsync(expression, userId);
        
        // Assert
        Assert.NotNull(results);
        Assert.All(results.Items, song => 
            Assert.Contains("Pink Floyd", song.Album?.Artist?.Name ?? ""));
    }

    [Fact]
    public async Task SmartPlaylist_Reevaluation()
    {
        // Arrange - Create a smart playlist
        var playlist = new SmartPlaylist
        {
            Query = "rating:>=4 AND plays:>10",
            UserId = userId,
            LastResultCount = 0
        };
        await _playlistService.CreateAsync(playlist);
        
        // Act - Re-evaluate
        var results = await _mqlService.ExecuteAsync(playlist.Query, userId);
        
        // Assert
        Assert.NotNull(results);
        Assert.Equal(playlist.LastResultCount, results.TotalCount);
    }

    [Fact]
    public async Task CrossEntity_Query_AlbumsAndSongs()
    {
        // Arrange
        var query = "artist:\"Miles Davis\" AND album:\"Kind of Blue\"";
        
        // Act
        var songs = await _songService.SearchAsync(query, userId);
        var albums = await _albumService.SearchAsync(query, userId);
        
        // Assert
        Assert.NotEmpty(songs);
        Assert.NotEmpty(albums);
        Assert.All(songs.Items, s => 
            Assert.Equal("Kind of Blue", s.Album?.Name));
    }

    [Fact]
    public async Task UserScopedFields_WithAuthentication()
    {
        // Arrange
        var query = "rating:>=4 plays:>0 starred:true";
        
        // Act
        var results = await _songService.SearchAsync(query, authenticatedUserId);
        
        // Assert
        Assert.NotNull(results);
        Assert.All(results.Items, song => 
        {
            var userSong = song.UserSongs.FirstOrDefault(us => us.UserId == authenticatedUserId);
            Assert.NotNull(userSong);
            Assert.True(userSong.Rating >= 4);
            Assert.True(userSong.PlayedCount > 0);
            Assert.True(userSong.IsStarred);
        });
    }
}
```

#### 7.3.3 Load and Stress Testing

**Concurrency and Performance Tests:**

```csharp
/// <summary>
/// Load tests for MQL under concurrent access
/// </summary>
public class MqlLoadTests
{
    private const int CONCURRENT_USERS = 100;
    private const int MAX_ACCEPTABLE_LATENCY_MS = 5000;

    [Fact]
    public async Task ConcurrentQueries_UnderLoad()
    {
        // Arrange - 100 concurrent users executing queries
        var queries = new[]
        {
            "artist:Beatles",
            "year:>=1970 AND genre:Rock",
            "rating:>4",
            "plays:>100",
            "album:\"Abbey Road\""
        };
        
        var tasks = Enumerable.Range(0, CONCURRENT_USERS)
            .Select(async i =>
            {
                var query = queries[i % queries.Length];
                var sw = Stopwatch.StartNew();
                var result = await _service.SearchSongsAsync(query, userId);
                sw.Stop();
                return (Result: result, Latency: sw.ElapsedMilliseconds);
            });
        
        var results = await Task.WhenAll(tasks);
        
        // Assert - All should complete within acceptable latency
        Assert.All(results, r => 
            Assert.True(r.Latency < MAX_ACCEPTABLE_LATENCY_MS, 
                $"Query took {r.Latency}ms, budget is {MAX_ACCEPTABLE_LATENCY_MS}ms"));
    }

    [Fact]
    public async Task HighVolume_SustainedLoad()
    {
        // Arrange - 1000 queries over 60 seconds
        var queries = Enumerable.Range(0, 1000)
            .Select(i => $"artist:Artist{i % 100} AND year:>={1960 + (i % 60)}")
            .ToList();
        
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var latencies = new ConcurrentBag<long>();
        
        // Act
        await Parallel.ForEachAsync(queries, new ParallelOptions 
        { 
            CancellationToken = cts.Token,
            MaxDegreeOfParallelism = 50
        }, async (query, ct) =>
        {
            var sw = Stopwatch.StartNew();
            await _service.SearchSongsAsync(query, userId);
            sw.Stop();
            latencies.Add(sw.ElapsedMilliseconds);
        });
        
        // Assert
        var p95 = GetPercentile(latencies, 95);
        var p99 = GetPercentile(latencies, 99);
        
        Assert.True(p95 < 1000, $"P95 latency {p95}ms exceeds 1s");
        Assert.True(p99 < 2000, $"P99 latency {p99}ms exceeds 2s");
    }

    [Fact]
    public async Task Cache_Effectiveness_UnderLoad()
    {
        // Arrange - Same query repeated by many users
        var query = "artist:\"The Beatles\"";
        var cacheHits = 0;
        var totalRequests = 100;
        
        // Act - Execute many unique queries
        var tasks = Enumerable.Range(0, totalRequests)
            .Select(async i =>
            {
                var sw = Stopwatch.StartNew();
                var result = await _service.SearchSongsAsync(query, userId);
                sw.Stop();
                
                // Check if cache was hit (second call should be much faster)
                if (sw.ElapsedMilliseconds < 10)
                    Interlocked.Increment(ref cacheHits);
            });
        
        await Task.WhenAll(tasks);
        
        // Assert - At least 50% should be cache hits
        var hitRate = (double)cacheHits / totalRequests;
        Assert.True(hitRate > 0.5, $"Cache hit rate {hitRate:P} below 50%");
    }
}
```

#### 7.3.4 Performance Regression Testing

**Performance Budget Enforcement:**

```csharp
/// <summary>
/// Performance regression tests with budget enforcement
/// </summary>
public class MqlPerformanceRegressionTests
{
    [Fact]
    public async Task SimpleQuery_PerformanceBudget()
    {
        // Budget: < 200ms for simple queries
        const int BUDGET_MS = 200;
        
        var query = "artist:Beatles";
        var sw = Stopwatch.StartNew();
        await _service.SearchSongsAsync(query, userId);
        sw.Stop();
        
        Assert.True(sw.ElapsedMilliseconds < BUDGET_MS, 
            $"Simple query took {sw.ElapsedMilliseconds}ms, budget is {BUDGET_MS}ms");
    }

    [Fact]
    public async Task MediumQuery_PerformanceBudget()
    {
        // Budget: < 500ms for medium complexity queries
        const int BUDGET_MS = 500;
        
        var query = "artist:\"Pink Floyd\" AND year:>=1970 AND genre:Rock";
        var sw = Stopwatch.StartNew();
        await _service.SearchSongsAsync(query, userId);
        sw.Stop();
        
        Assert.True(sw.ElapsedMilliseconds < BUDGET_MS, 
            $"Medium query took {sw.ElapsedMilliseconds}ms, budget is {BUDGET_MS}ms");
    }

    [Fact]
    public async Task ComplexQuery_PerformanceBudget()
    {
        // Budget: < 2s for complex boolean logic
        const int BUDGET_MS = 2000;
        
        var query = "(artist:Beatles OR artist:Pink Floyd) AND (year:1970-1980 OR year:1990-2000) AND NOT live AND rating:>=4";
        var sw = Stopwatch.StartNew();
        await _service.SearchSongsAsync(query, userId);
        sw.Stop();
        
        Assert.True(sw.ElapsedMilliseconds < BUDGET_MS, 
            $"Complex query took {sw.ElapsedMilliseconds}ms, budget is {BUDGET_MS}ms");
    }

    [Fact]
    public async Task CompilationTime_WithinBudget()
    {
        // Budget: < 50ms for expression compilation
        const int BUDGET_MS = 50;
        
        var query = "artist:Beatles AND year:>=1970";
        var sw = Stopwatch.StartNew();
        await _compiler.CompileAsync<Song>(query);
        sw.Stop();
        
        Assert.True(sw.ElapsedMilliseconds < BUDGET_MS, 
            $"Compilation took {sw.ElapsedMilliseconds}ms, budget is {BUDGET_MS}ms");
    }
}
```

#### 7.3.5 Edge Case Tests

**Boundary and Edge Case Coverage:**

```csharp
/// <summary>
/// Comprehensive edge case testing
/// </summary>
public class MqlEdgeCaseTests
{
    [Fact]
    public void EmptyQuery_ReturnsHelpfulMessage()
    {
        // Arrange & Act
        var result = _validator.Validate("", "songs");
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("MQL_EMPTY_QUERY", result.ErrorCode);
    }

    [Fact]
    public void OnlyWhitespace_HandledGracefully()
    {
        // Arrange & Act
        var result = _validator.Validate("   \t\n  ", "songs");
        
        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void MaxLengthQuery_Accepted()
    {
        // Arrange
        var query = new string('a', 500); // Exactly max length
        
        // Act
        var result = _validator.Validate(query, "songs");
        
        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void OverMaxLengthQuery_Rejected()
    {
        // Arrange
        var query = new string('a', 501); // Over max length
        
        // Act
        var result = _validator.Validate(query, "songs");
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("MQL_QUERY_TOO_LONG", result.ErrorCode);
    }

    [Fact]
    public void SpecialCharacters_InFreeText_Handled()
    {
        // Arrange
        var query = "title:\"Test's (special) chars:here\"";
        
        // Act
        var result = _validator.Validate(query, "songs");
        
        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UnicodeCharacters_InQuery_Handled()
    {
        // Arrange
        var query = "artist:\"日本語テスト\" AND title:\"🎵 emoji\"";
        
        // Act
        var result = _validator.Validate(query, "songs");
        
        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void VeryLongFieldValue_Handled()
    {
        // Arrange
        var longValue = new string('x', 10000);
        var query = $"artist:\"{longValue}\"";
        
        // Act
        var result = _validator.Validate(query, "songs");
        
        // Assert - Should handle gracefully (may truncate or reject)
        Assert.NotNull(result);
    }

    [Fact]
    public void NestedParentheses_MaxDepth_Rejected()
    {
        // Arrange - 11 levels of nesting (max is 10)
        var query = new StringBuilder();
        for (int i = 0; i < 11; i++)
            query.Append('(');
        query.Append("artist:Beatles");
        for (int i = 0; i < 11; i++)
            query.Append(')');
        
        // Act
        var result = _validator.Validate(query.ToString(), "songs");
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("MQL_TOO_DEEP", result.ErrorCode);
    }

    [Fact]
    public void UnbalancedParentheses_Detected()
    {
        // Arrange
        var query = "artist:Beatles AND (year:>=1970";
        
        // Act
        var result = _validator.Validate(query, "songs");
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("MQL_UNBALANCED_PARENS", result.ErrorCode);
    }

    [Fact]
    public void MaxFieldCount_AtLimit_Accepted()
    {
        // Arrange - Exactly 20 fields
        var query = string.Join(" AND ", Enumerable.Range(0, 20)
            .Select(i => $"field{i}:value{i}"));
        
        // Act
        var result = _validator.Validate(query, "songs");
        
        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void OverMaxFieldCount_Rejected()
    {
        // Arrange - 21 fields (over limit)
        var query = string.Join(" AND ", Enumerable.Range(0, 21)
            .Select(i => $"field{i}:value{i}"));
        
        // Act
        var result = _validator.Validate(query, "songs");
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("MQL_TOO_MANY_FIELDS", result.ErrorCode);
    }

    [Fact]
    public void AllOperators_HandledCorrectly()
    {
        // Test all supported operators
        var queries = new[]
        {
            "year:=1970",
            "year:!=1970",
            "year:<1970",
            "year:<=1970",
            "year:>1970",
            "year:>=1970",
            "year:1970-1980",
            "title:contains(\"test\")",
            "title:startsWith(\"test\")",
            "title:endsWith(\"test\")",
            "title:wildcard(\"*test*\")",
            "starred:true",
            "starred:false"
        };
        
        foreach (var query in queries)
        {
            var result = _validator.Validate(query, "songs");
            Assert.True(result.IsValid, $"Failed for query: {query}");
        }
    }
}
```

#### 7.3.6 Cache Testing

**Cache Behavior Verification:**

```csharp
/// <summary>
/// Tests for expression cache behavior
/// </summary>
public class MqlCacheTests
{
    [Fact]
    public async Task SameQuery_CacheHit_SecondTime()
    {
        // Arrange
        var query = "artist:Beatles";
        
        // Act - First call (cache miss)
        var sw1 = Stopwatch.StartNew();
        await _service.SearchSongsAsync(query, userId);
        sw1.Stop();
        
        // Act - Second call (should be cache hit)
        var sw2 = Stopwatch.StartNew();
        await _service.SearchSongsAsync(query, userId);
        sw2.Stop();
        
        // Assert - Second call should be significantly faster
        Assert.True(sw2.ElapsedMilliseconds < sw1.ElapsedMilliseconds / 2,
            "Cache hit should be at least 2x faster");
    }

    [Fact]
    public async Task DifferentQuery_DifferentCacheEntry()
    {
        // Arrange
        var query1 = "artist:Beatles";
        var query2 = "artist:Pink Floyd";
        
        // Act
        await _service.SearchSongsAsync(query1, userId);
        await _service.SearchSongsAsync(query2, userId);
        await _service.SearchSongsAsync(query1, userId); // Should still be cache hit for query1
        
        // Assert - Verify cache statistics
        var stats = _cache.GetStatistics();
        Assert.Equal(2, stats.Count); // Only 2 unique queries
    }

    [Fact]
    public async Task Cache_Invalidation_OnDataChange()
    {
        // Arrange
        var query = "artist:Beatles";
        await _service.SearchSongsAsync(query, userId);
        
        // Act - Invalidate cache
        await _cacheInvalidator.OnEntityChanged<Song>(new Song());
        
        // Act - Query again
        var sw = Stopwatch.StartNew();
        await _service.SearchSongsAsync(query, userId);
        sw.Stop();
        
        // Assert - Should be cache miss (slow)
        Assert.True(sw.ElapsedMilliseconds > 50);
    }

    [Fact]
    public async Task Cache_Expiration_AfterTTL()
    {
        // Arrange
        var query = "artist:Beatles";
        var shortTtl = TimeSpan.FromSeconds(1);
        
        // Act - First call
        await _service.SearchSongsAsync(query, userId, shortTtl);
        
        // Wait for expiration
        await Task.Delay(TimeSpan.FromSeconds(2));
        
        // Act - Second call (should be cache miss)
        var sw = Stopwatch.StartNew();
        await _service.SearchSongsAsync(query, userId);
        sw.Stop();
        
        // Assert - Should be cache miss
        Assert.True(sw.ElapsedMilliseconds > 50);
    }

    [Fact]
    public async Task Cache_MemoryLimit_Enforced()
    {
        // Arrange - Fill cache with many queries
        var queries = Enumerable.Range(0, 1500)
            .Select(i => $"artist:Artist{i}")
            .ToList();
        
        // Act - Execute many unique queries
        foreach (var query in queries)
        {
            await _service.SearchSongsAsync(query, userId);
        }
        
        // Assert - Cache should respect size limit (1000 entries)
        var stats = _cache.GetStatistics();
        Assert.True(stats.Count <= 1000);
    }
}
```

#### 7.3.7 Contract Testing

**API Contract Validation:**

```csharp
/// <summary>
/// Tests for API contract compliance
/// </summary>
public class MqlApiContractTests
{
    [Fact]
    public async Task ParseEndpoint_ReturnsValidSchema()
    {
        // Arrange
        var request = new MqlParseRequest { Entity = "songs", Query = "artist:Beatles" };
        
        // Act
        var response = await _apiClient.ParseAsync(request);
        
        // Assert - Validate JSON schema
        var schema = new
        {
            type = "object",
            required = new[] { "normalizedQuery", "ast", "warnings", "estimatedComplexity", "valid" },
            properties = new
            {
                normalizedQuery = new { type = "string" },
                ast = new { type = "object" },
                warnings = new { type = "array" },
                estimatedComplexity = new { type = "integer" },
                valid = new { type = "boolean" }
            }
        };
        
        Assert.True(ValidateSchema(response, schema));
    }

    [Fact]
    public async Task ParseEndpoint_Error_ReturnsValidSchema()
    {
        // Arrange
        var request = new MqlParseRequest { Entity = "songs", Query = "invalidfield:test" };
        
        // Act
        var response = await _apiClient.ParseAsync(request);
        
        // Assert - Validate error schema
        var errorSchema = new
        {
            type = "object",
            required = new[] { "errorCode", "message", "position", "suggestions" },
            properties = new
            {
                errorCode = new { type = "string" },
                message = new { type = "string" },
                position = new { type = "object" },
                suggestions = new { type = "array" }
            }
        };
        
        Assert.True(ValidateSchema(response, errorSchema));
    }

    [Fact]
    public async Task AllErrorCodes_ReturnCorrectStructure()
    {
        // Test all documented error codes return proper structure
        var errorCodes = new[]
        {
            "MQL_PARSE_ERROR",
            "MQL_UNKNOWN_FIELD",
            "MQL_INVALID_LITERAL",
            "MQL_REGEX_TOO_COMPLEX",
            "MQL_QUERY_TOO_LONG",
            "MQL_TOO_MANY_FIELDS",
            "MQL_TOO_DEEP",
            "MQL_UNBALANCED_PARENS",
            "MQL_FORBIDDEN_FIELD",
            "MQL_REGEX_DANGEROUS"
        };
        
        foreach (var errorCode in errorCodes)
        {
            var result = _validator.Validate("invalid:query", "songs");
            // Each error should have consistent structure
            Assert.NotNull(result.ErrorCode);
            Assert.NotNull(result.Message);
            Assert.NotNull(result.Position);
        }
    }
}
```

#### 7.3.8 Mutation Testing

**Test Robustness Verification:**

```yaml
# mutation-testing.yml - Run periodically (not in every CI build)
name: Mutation Testing

on:
  schedule:
    - cron: '0 0 * * 0'  # Weekly

jobs:
  mutation-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.x
      
      - name: Install Stryker
        run: dotnet tool install -g dotnet-stryker
      
      - name: Run mutation tests
        run: |
          dotnet stryker \
            --project-file **/Melodee.Mql.csproj \
            --reporters html,console \
            --threshold-break 70 \
            --threshold 80
      
      - name: Upload mutation report
        uses: actions/upload-artifact@v4
        with:
          name: mutation-report
          path: '**/StrykerOutput/**/report.html'
```

**Mutation Coverage Requirements:**

| Category | Minimum Score |
|----------|---------------|
| Security-critical paths | 90% |
| Authorization logic | 95% |
| Input validation | 85% |
| Error handling | 80% |
| **Overall** | **80%** |

#### 7.3.9 Test Organization

**Recommended Test Project Structure:**

```
tests/
├── Melodee.Mql.Tests/
│   ├── Unit/
│   │   ├── Parser/
│   │   │   ├── TokenizerTests.cs
│   │   │   ├── AstBuilderTests.cs
│   │   │   └── NormalizerTests.cs
│   │   ├── Validator/
│   │   │   ├── ValidationRulesTests.cs
│   │   │   ├── SanitizationTests.cs
│   │   │   └── ComplexityScoringTests.cs
│   │   ├── Compiler/
│   │   │   ├── ExpressionBuilderTests.cs
│   │   │   └── TypeCoercionTests.cs
│   │   ├── Authorization/
│   │   │   ├── FieldAuthorizationTests.cs
│   │   │   └── UserEnumerationTests.cs
│   │   └── Cache/
│   │       ├── CacheHitTests.cs
│   │       └── CacheInvalidationTests.cs
│   ├── Integration/
│   │   ├── FullPipelineTests.cs
│   │   ├── SmartPlaylistTests.cs
│   │   └── CrossEntityTests.cs
│   ├── Performance/
│   │   ├── PerformanceBudgetTests.cs
│   │   └── BenchmarkTests.cs
│   ├── Load/
│   │   ├── ConcurrencyTests.cs
│   │   └── StressTests.cs
│   ├── Contract/
│   │   ├── ApiSchemaTests.cs
│   │   └── ResponseFormatTests.cs
│   └── EdgeCases/
│       ├── BoundaryTests.cs
│       ├── SpecialCharacterTests.cs
│       └── UnicodeTests.cs
├── Melodee.Mql.Security.Tests/
│   ├── InjectionPreventionTests.cs
│   ├── ReDoSPreventionTests.cs
│   └── AuthorizationTests.cs
└── Melodee.Mql.Blazor.Tests/
    ├── ComponentTests.cs
    └── IntegrationTests.cs
```

**Reference:** Code Review Generic Instructions - Testing Standards, Performance Optimization Best Practices


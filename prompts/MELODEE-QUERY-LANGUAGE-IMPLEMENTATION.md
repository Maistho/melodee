# Melodee Query Language (MQL) — Implementation Plan

**Status:** In Progress - Phase 5 Complete  
**Spec Reference:** [MELODEE-QUERY-LANGUAGE.md](./MELODEE-QUERY-LANGUAGE.md)

## Overview

This document outlines the phased implementation plan for MQL. Each phase builds upon the previous, with clear deliverables and acceptance criteria. Phases are designed to be independently testable and deployable.

**Quick Links:**
- [Phase Map](#phase-map) - Checklist of all phases
- [Phase Details](#phase-details) - Detailed implementation guidance per phase
- [Dependencies Between Phases](#dependencies-between-phases) - Phase ordering
- [Coding Agent Prompt Template](#coding-agent-prompt-template) - Template for delegating phases

---

## Phase Map

### Foundation
- [x] **Phase 1: Core Infrastructure & Models**
- [x] **Phase 2: Tokenizer & Lexer**
- [x] **Phase 3: Parser & AST**

### Core Compilation
- [x] **Phase 4: Input Validation Layer**
- [x] **Phase 5: Expression Tree Compiler (Songs)**
- [ ] **Phase 6: Expression Tree Caching**

### Security & Authorization
- [ ] **Phase 7: Security Controls & Sanitization**
- [ ] **Phase 8: User-Scoped Field Authorization**

### API & Integration
- [ ] **Phase 9: Parse API Endpoint**
- [ ] **Phase 10: Search Integration (Songs)**
- [ ] **Phase 11: Album & Artist Entity Support**

### Advanced Features
- [ ] **Phase 12: Smart Playlist Persistence**
- [ ] **Phase 13: Autocomplete & Suggestions**
- [ ] **Phase 14: Regex Support (Guarded)**

### Testing & Production Readiness
- [ ] **Phase 15: Comprehensive Testing Suite**
- [ ] **Phase 16: Performance Optimization & Monitoring**
- [ ] **Phase 17: Documentation & UI Components**

---

## Phase Details

### Phase 1: Core Infrastructure & Models

**Objective:** Establish the foundational data structures, error codes, and project scaffolding for MQL.

**Deliverables:**

1. **Create project structure:**
   ```
   src/Melodee.Mql/
   ├── Melodee.Mql.csproj
   ├── Models/
   │   ├── MqlToken.cs
   │   ├── MqlTokenType.cs
   │   ├── MqlAstNode.cs
   │   ├── MqlFieldInfo.cs
   │   ├── MqlValidationResult.cs
   │   ├── MqlParseResult.cs
   │   └── MqlError.cs
   ├── Constants/
   │   ├── MqlErrorCodes.cs
   │   ├── MqlConstants.cs
   │   └── MqlOperators.cs
   └── Interfaces/
       ├── IMqlTokenizer.cs
       ├── IMqlParser.cs
       ├── IMqlValidator.cs
       └── IMqlCompiler.cs
   ```

2. **Token types enumeration (`MqlTokenType.cs`):**
   - `FreeText`, `FieldName`, `Colon`, `Operator`, `StringLiteral`, `NumberLiteral`, `DateLiteral`, `BooleanLiteral`
   - `And`, `Or`, `Not`, `LeftParen`, `RightParen`
   - `Range`, `Regex`, `Wildcard`
   - `EndOfInput`, `Unknown`

3. **AST node types (`MqlAstNode.cs`):**
   - `FreeTextNode` - unqualified search term
   - `FieldExpressionNode` - `field:operator:value`
   - `BinaryExpressionNode` - `left AND|OR right`
   - `UnaryExpressionNode` - `NOT expression`
   - `GroupNode` - parenthesized expression
   - `RangeNode` - `field:min-max`

4. **Field metadata (`MqlFieldInfo.cs`):**
   ```csharp
   public record MqlFieldInfo(
       string Name,
       string[] Aliases,
       MqlFieldType Type,
       string DbMapping,
       bool IsUserScoped,
       string? Description);
   ```

5. **Error codes (`MqlErrorCodes.cs`):**
   - `MQL_PARSE_ERROR`, `MQL_UNKNOWN_FIELD`, `MQL_INVALID_LITERAL`
   - `MQL_REGEX_TOO_COMPLEX`, `MQL_REGEX_DANGEROUS`
   - `MQL_QUERY_TOO_LONG`, `MQL_TOO_MANY_FIELDS`, `MQL_TOO_DEEP`
   - `MQL_UNBALANCED_PARENS`, `MQL_FORBIDDEN_FIELD`, `MQL_EMPTY_QUERY`
   - `MQL_FORBIDDEN_USER_DATA`, `RATE_LIMIT_EXCEEDED`

6. **Constants (`MqlConstants.cs`):**
   ```csharp
   public const int MAX_QUERY_LENGTH = 500;
   public const int MAX_FIELD_COUNT = 20;
   public const int MAX_RECURSION_DEPTH = 10;
   public const int MAX_REGEX_PATTERN_LENGTH = 100;
   public const int MAX_RESULT_SET_FOR_REGEX = 1000;
   public const int REGEX_TIMEOUT_MS = 500;
   public const int PARSE_RATE_LIMIT_PER_MINUTE = 10;
   public const int PARSE_TIMEOUT_MS = 200;
   ```

7. **Field registry for Songs, Albums, Artists:**
   - Create `MqlFieldRegistry.cs` with static dictionaries mapping field names to `MqlFieldInfo`
   - Include all fields from spec §4.4 (Songs), Albums, Artists sections

**Acceptance Criteria:**
- [ ] Project compiles without errors
- [ ] All model classes have XML documentation
- [ ] Field registry contains all fields from spec
- [ ] Unit tests verify field registry completeness

---

### Phase 2: Tokenizer & Lexer

**Objective:** Implement lexical analysis to convert raw query strings into a stream of tokens with position tracking.

**Deliverables:**

1. **Tokenizer implementation (`MqlTokenizer.cs`):**
   ```csharp
   public interface IMqlTokenizer
   {
       IEnumerable<MqlToken> Tokenize(string query);
   }
   ```

2. **Token structure with position tracking:**
   ```csharp
   public record MqlToken(
       MqlTokenType Type,
       string Value,
       int StartPosition,
       int EndPosition,
       int Line,
       int Column);
   ```

3. **Tokenization rules:**
   - Whitespace: skip but track position
   - Quoted strings: `"..."` with escape handling (`\"`, `\\`)
   - Field:value pairs: `artist:value` or `artist:"value"`
   - Operators: `AND`, `OR`, `NOT` (case-insensitive)
   - Comparison operators: `:=`, `:!=`, `:<`, `:<=`, `:>`, `:>=`
   - Range syntax: `1970-1980`
   - Parentheses: `(`, `)`
   - Regex patterns: `/pattern/flags`
   - Numbers: integers and decimals (culture-invariant)
   - Date literals: `2026-01-06`, relative dates (`today`, `yesterday`, `last-week`, `-7d`)
   - Boolean literals: `true`, `false`

4. **Edge case handling:**
   - Unclosed quotes → generate error token with position
   - Invalid escape sequences → generate error token
   - Consecutive operators → preserve for parser to error

5. **Unit tests (`MqlTokenizerTests.cs`):**
   - Simple field:value pairs
   - Quoted strings with spaces
   - Boolean operators
   - Comparison operators
   - Range syntax
   - Date literals (absolute and relative)
   - Mixed queries
   - Error cases (unclosed quotes, invalid escapes)

**Acceptance Criteria:**
- [ ] Tokenizer handles all syntax from spec §4.1-§4.3
- [ ] Position tracking accurate for error reporting
- [ ] 95%+ code coverage on tokenizer
- [ ] Performance: tokenize 500-char query in <5ms

---

### Phase 3: Parser & AST

**Objective:** Transform token stream into an Abstract Syntax Tree (AST) respecting operator precedence.

**Deliverables:**

1. **Parser implementation (`MqlParser.cs`):**
   ```csharp
   public interface IMqlParser
   {
       MqlParseResult Parse(IEnumerable<MqlToken> tokens, string entityType);
   }
   
   public record MqlParseResult(
       bool IsValid,
       MqlAstNode? Ast,
       string NormalizedQuery,
       List<MqlError> Errors,
       List<string> Warnings);
   ```

2. **Recursive descent parser with precedence:**
   - Precedence (highest to lowest): Parentheses → NOT → AND → OR
   - Default operator between terms: AND
   - Grammar rules:
     ```
     Query      → OrExpr
     OrExpr     → AndExpr ('OR' AndExpr)*
     AndExpr    → UnaryExpr ('AND'? UnaryExpr)*
     UnaryExpr  → 'NOT'? Primary
     Primary    → '(' Query ')' | FieldExpr | FreeText
     FieldExpr  → Field ':' Operator? Value
     ```

3. **AST node implementations:**
   - `FreeTextNode(string Text, MqlToken Token)`
   - `FieldExpressionNode(string Field, MqlOperator Op, object Value, MqlToken Token)`
   - `BinaryExpressionNode(MqlBinaryOp Op, MqlAstNode Left, MqlAstNode Right)`
   - `UnaryExpressionNode(MqlUnaryOp Op, MqlAstNode Operand)`
   - `GroupNode(MqlAstNode Inner)`
   - `RangeNode(string Field, object Min, object Max, MqlToken Token)`

4. **Query normalization:**
   - Canonical spacing and casing
   - Explicit operators where defaults applied
   - Normalized field names (lowercase)

5. **Error recovery:**
   - Continue parsing after errors where possible
   - Collect all errors with positions
   - Provide context for error messages

6. **Unit tests (`MqlParserTests.cs`):**
   - Simple field expressions
   - Boolean combinations (AND, OR, NOT)
   - Operator precedence verification
   - Parentheses grouping
   - Range expressions
   - Free text terms
   - Error cases with position verification

**Acceptance Criteria:**
- [ ] Parser correctly handles all examples from spec §4.1
- [ ] Operator precedence matches spec §4.2
- [ ] Normalized query output is canonical
- [ ] Error messages include position information
- [ ] 90%+ code coverage on parser

---

### Phase 4: Input Validation Layer

**Objective:** Implement comprehensive input validation to prevent DoS and ensure query safety.

**Deliverables:**

1. **Validator implementation (`MqlValidator.cs`):**
   ```csharp
   public interface IMqlValidator
   {
       MqlValidationResult Validate(string query, string entityType);
   }
   
   public record MqlValidationResult(
       bool IsValid,
       List<MqlError> Errors,
       List<string> Warnings,
       int ComplexityScore);
   ```

2. **Validation rules (from spec §6.1.A):**
   - Query length: max 500 characters
   - Field count: max 20 field filters
   - Recursion depth: max 10 levels of parentheses
   - Balanced parentheses check
   - Known field validation per entity type
   - Operator validity for field type
   - Literal type validation (number for year, date for added, etc.)

3. **Complexity scoring:**
   - Base score: 1 per field filter
   - +1 per boolean operator
   - +2 per nested parenthesis level
   - +5 per regex pattern
   - Warn if complexity > 10, reject if > 20

4. **Field validation:**
   - Validate field exists for entity type
   - Suggest similar fields (Levenshtein distance) for typos
   - Validate operator valid for field type

5. **Unit tests (`MqlValidatorTests.cs`):**
   - Query length limits
   - Field count limits
   - Recursion depth limits
   - Unknown field detection with suggestions
   - Invalid literal types
   - Complexity scoring accuracy

**Acceptance Criteria:**
- [ ] All limits from spec §5.2 enforced
- [ ] Field suggestions work with ≤2 edit distance
- [ ] Complexity scoring deterministic
- [ ] 95%+ code coverage on validator

---

### Phase 5: Expression Tree Compiler (Songs)

**Objective:** Compile validated AST into EF Core `Expression<Func<Song, bool>>` predicates.

**Deliverables:**

1. **Compiler implementation (`MqlCompiler.cs`):**
   ```csharp
   public interface IMqlCompiler<TEntity> where TEntity : class
   {
       Expression<Func<TEntity, bool>> Compile(
           MqlAstNode ast, 
           Guid? userId = null);
   }
   ```

2. **Expression building for Songs:**
   - String fields: `Contains`, `StartsWith`, `EndsWith`, `Equals` on normalized columns
   - Numeric fields: comparison operators mapping to `Expression.LessThan`, etc.
   - Date fields: comparison with `DateTime` values, relative date resolution
   - Boolean fields: direct equality
   - Range expressions: `value >= min && value <= max`
   - Array fields (genres, moods): `Any()` with contains

3. **User-scoped field handling:**
   - Rating, plays, starred, starredAt, lastPlayedAt require userId
   - Generate subquery: `s.UserSongs.Where(us => us.UserId == userId).Select(...).FirstOrDefault()`
   - Must be used with eager loading (documented requirement)

4. **Boolean expression composition:**
   - AND: `Expression.AndAlso`
   - OR: `Expression.OrElse`
   - NOT: `Expression.Not`

5. **Duration conversion:**
   - MQL uses seconds, DB stores milliseconds
   - Apply `* 1000` conversion in expression

6. **Free text handling:**
   - Default fields for Songs: title, artist, album
   - Combine with OR: `title.Contains(term) || artist.Contains(term) || album.Contains(term)`

7. **Unit tests (`MqlCompilerTests.cs`):**
   - Each field type compilation
   - Boolean combinations
   - User-scoped fields with userId
   - Duration conversion
   - Free text expansion

**Acceptance Criteria:**
- [x] All Songs fields from spec §4.4 compile correctly
- [x] Generated expressions are parameterized (no string concatenation)
- [x] User-scoped fields require userId parameter
- [x] No client-side evaluation warnings from EF Core
- [x] 85%+ code coverage on compiler

---

### Phase 6: Expression Tree Caching

**Objective:** Cache compiled expressions to avoid recompilation overhead for repeated queries.

**Deliverables:**

1. **Cache implementation (`MqlExpressionCache.cs`):**
   ```csharp
   public interface IMqlExpressionCache
   {
       Expression<Func<TEntity, bool>> GetOrCreate<TEntity>(
           string normalizedQuery,
           Func<Expression<Func<TEntity, bool>>> factory,
           TimeSpan? ttl = null) where TEntity : class;
       
       void Clear<TEntity>() where TEntity : class;
       
       MqlCacheStatistics GetStatistics();
   }
   ```

2. **Cache key strategy:**
   - Key: `{EntityTypeName}:{NormalizedQuery}:{UserId}`
   - User-scoped queries include userId in key

3. **Cache configuration:**
   - Default TTL: 30 minutes
   - Max entries: 1000
   - Eviction: LRU policy
   - Size tracking per entry

4. **Cache statistics:**
   - Hit count, miss count, hit rate
   - Entry count, memory estimate
   - Last eviction time

5. **Cache invalidation (`MqlCacheInvalidator.cs`):**
   - Subscribe to entity change events
   - Clear cache for affected entity types
   - Manual invalidation endpoint for admin

6. **Integration with DI:**
   - Register as singleton
   - Inject into `MqlCompiler`

7. **Unit tests (`MqlCacheTests.cs`):**
   - Cache hit on repeated query
   - Different queries create different entries
   - TTL expiration
   - Manual invalidation
   - Size limits enforced

**Acceptance Criteria:**
- [ ] Second query execution 50%+ faster (cache hit)
- [ ] Cache respects memory limits
- [ ] Statistics accurately reported
- [ ] Thread-safe for concurrent access

---

### Phase 7: Security Controls & Sanitization

**Objective:** Implement input sanitization and security monitoring to prevent injection attacks.

**Deliverables:**

1. **Text sanitizer (`MqlTextSanitizer.cs`):**
   ```csharp
   public static class MqlTextSanitizer
   {
       public static string SanitizeForFreeText(string input);
       public static string SanitizeForRegex(string input);
       public static bool ContainsDangerousPatterns(string input);
   }
   ```

2. **Sanitization rules (from spec §4.1.1):**
   - Escape special characters: `'`, `"`, `\`, `;`, `-`, `(`, `)`, `[`, `]`, `{`, `}`, `|`, `*`, `?`, `.`, `+`, `^`, `$`, `<`, `>`, `#`, `&`, `%`, `~`, `` ` ``, newlines
   - Detect dangerous patterns: `--`, `; DROP`, `; DELETE`, `UNION SELECT`, `EXEC(`, `xp_`, `0x`

3. **Security monitor (`MqlSecurityMonitor.cs`):**
   ```csharp
   public interface IMqlSecurityMonitor
   {
       void LogWarning(string message, string? query = null);
       void LogViolation(string errorCode, string message, string query);
       Task<SecurityMetrics> GetMetricsAsync(TimeSpan window);
   }
   ```

4. **Regex guards (from spec §4.5):**
   - Pattern length: max 100 characters
   - Prohibited patterns: `(.*)*`, `(.+)+`, `([a-z]*)*`, `([a-z]+)+`
   - Timeout enforcement: 500ms per evaluation
   - Result set limit: 1000 records before regex evaluation

5. **Rate limiting preparation:**
   - Define rate limit interface
   - Prepare for integration (actual implementation in Phase 9)

6. **Unit tests (`MqlSecurityTests.cs`):**
   - SQL injection prevention
   - ReDoS pattern detection
   - Dangerous pattern detection
   - Sanitization edge cases

**Acceptance Criteria:**
- [ ] All injection patterns from spec rejected
- [ ] ReDoS patterns blocked
- [ ] Security events logged
- [ ] No false positives on legitimate queries

---

### Phase 8: User-Scoped Field Authorization

**Objective:** Implement authorization checks for user-specific fields to prevent data leakage.

**Deliverables:**

1. **Authorization service (`MqlAuthorizationService.cs`):**
   ```csharp
   public interface IMqlAuthorizationService
   {
       AuthorizationResult AuthorizeQuery(
           string query, 
           string entityType, 
           Guid? currentUserId,
           Guid? targetUserId = null);
       
       bool IsUserScopedField(string fieldName, string entityType);
   }
   ```

2. **User-scoped fields identification:**
   - Songs: `rating`, `plays`, `starred`, `starredAt`, `lastPlayedAt`
   - Albums: `rating`, `plays`, `starred`, `starredAt`, `lastPlayedAt`
   - Artists: `rating`, `starred`, `starredAt`

3. **Authorization rules:**
   - Anonymous users: block all user-scoped fields
   - Authenticated users: allow querying own data only
   - Block cross-user enumeration patterns
   - Block userId field access except for admin

4. **Error responses:**
   - `MQL_FORBIDDEN_FIELD`: user-scoped field without auth
   - `MQL_FORBIDDEN_USER_DATA`: attempting to query other user's data

5. **Integration with validator:**
   - Call authorization check during validation
   - Inject `ICurrentUserService` dependency

6. **Unit tests (`MqlAuthorizationTests.cs`):**
   - Anonymous user blocked from user-scoped fields
   - Authenticated user can query own data
   - Cross-user queries blocked
   - Non-user-scoped fields always allowed

**Acceptance Criteria:**
- [ ] Anonymous users cannot use rating, plays, starred, etc.
- [ ] Users can only query their own user-scoped data
- [ ] Clear error messages with field names
- [ ] 100% coverage on authorization logic

---

### Phase 9: Parse API Endpoint

**Objective:** Expose MQL parsing and validation via REST API for client-side validation and autocomplete.

**Deliverables:**

1. **API controller (`MqlController.cs`):**
   ```csharp
   [ApiController]
   [Route("api/v1/query")]
   public class MqlController : ControllerBase
   {
       [HttpPost("parse")]
       public Task<ActionResult<MqlParseResponse>> ParseAsync(
           [FromBody] MqlParseRequest request);
   }
   ```

2. **Request/Response DTOs:**
   ```csharp
   public record MqlParseRequest(string Entity, string Query);
   
   public record MqlParseResponse(
       string NormalizedQuery,
       MqlAstDto? Ast,
       List<string> Warnings,
       int EstimatedComplexity,
       bool Valid);
   
   public record MqlErrorResponse(
       string ErrorCode,
       string Message,
       MqlPositionDto? Position,
       List<MqlSuggestionDto> Suggestions,
       DateTime Timestamp);
   ```

3. **AST serialization:**
   - Serialize AST nodes to JSON-friendly format
   - Include node types and values
   - Exclude internal implementation details

4. **Error response formatting:**
   - Match all examples from spec §5.1.1
   - Include position, suggestions, context

5. **Rate limiting:**
   - 10 requests per minute per user (from spec)
   - Return 429 with retry-after header

6. **Security:**
   - Input size limit: 500 characters
   - Timeout: 200ms
   - Audit logging for all requests

7. **Unit tests (`MqlControllerTests.cs`):**
   - Success response format
   - Each error code response format
   - Rate limiting enforcement
   - Input size validation

**Acceptance Criteria:**
- [ ] Response format matches spec §5.1.1 exactly
- [ ] Rate limiting enforced
- [ ] Timeout enforced
- [ ] All error codes return proper structure

---

### Phase 10: Search Integration (Songs)

**Objective:** Integrate MQL into existing song search endpoints without breaking existing contracts.

**Deliverables:**

1. **Service extension (`SongService.cs` modification):**
   ```csharp
   public async Task<PagedResult<SongDto>> SearchAsync(
       PagedRequest request,
       string? mqlQuery,
       Guid userId,
       CancellationToken cancellationToken);
   ```

2. **Query parameter addition:**
   - Add optional `q` parameter to `GET /api/v1/songs`
   - If `q` present: use MQL filtering
   - If `q` absent: existing behavior unchanged

3. **Eager loading for user-scoped fields:**
   - Include `UserSongs.Where(us => us.UserId == userId)`
   - Include `Album` and `Album.Artist` for artist field

4. **Query pipeline:**
   ```
   Request → Validate(q) → Parse(q) → Compile<Song>(ast) → ApplyToQuery(dbQuery)
   ```

5. **Logging:**
   - Log normalized query (sanitized)
   - Log execution time
   - Log result count

6. **Backward compatibility:**
   - `FilterBy` still works when `q` is absent
   - `OrderBy` respected unless `top:` in MQL
   - Pagination works with MQL

7. **Integration tests (`SongSearchIntegrationTests.cs`):**
   - MQL query returns correct results
   - Combined with pagination
   - User-scoped fields work
   - Backward compatibility with FilterBy

**Acceptance Criteria:**
- [ ] Existing API contracts unchanged
- [ ] MQL queries return correct results
- [ ] Performance within budget (< 500ms for typical queries)
- [ ] No N+1 query problems

---

### Phase 11: Album & Artist Entity Support

**Objective:** Extend MQL support to Album and Artist entities.

**Deliverables:**

1. **Album compiler (`MqlAlbumCompiler.cs`):**
   - Implement `IMqlCompiler<Album>`
   - Map all fields from spec §4.4 Albums section
   - Handle user-scoped fields via `UserAlbums`

2. **Artist compiler (`MqlArtistCompiler.cs`):**
   - Implement `IMqlCompiler<Artist>`
   - Map all fields from spec §4.4 Artists section
   - Handle user-scoped fields via `UserArtists`
   - Note: `plays` is global-only for artists

3. **Service integration:**
   - Add MQL support to `AlbumService.SearchAsync`
   - Add MQL support to `ArtistService.SearchAsync`

4. **API endpoints:**
   - `GET /api/v1/albums?q=...`
   - `GET /api/v1/artists?q=...`

5. **Field registry updates:**
   - Add album-specific aliases (`name` → `album`)
   - Add artist-specific aliases (`name` → `artist`)

6. **Unit tests:**
   - Album field compilation
   - Artist field compilation
   - Cross-entity field resolution

**Acceptance Criteria:**
- [ ] All Album fields from spec compile correctly
- [ ] All Artist fields from spec compile correctly
- [ ] Entity-specific field validation works
- [ ] Integration tests pass for all entities

---

### Phase 12: Smart Playlist Persistence

**Objective:** Enable saving MQL queries as smart playlists that re-evaluate on demand.

**Deliverables:**

1. **Database model (`SmartPlaylist.cs`):**
   ```csharp
   public class SmartPlaylist : DataModelBase
   {
       public Guid UserId { get; set; }
       public string Name { get; set; }
       public string MqlQuery { get; set; }
       public string EntityType { get; set; } // "songs", "albums"
       public int LastResultCount { get; set; }
       public Instant? LastEvaluatedAt { get; set; }
       public bool IsPublic { get; set; }
   }
   ```

2. **EF Core migration:**
   - Create `SmartPlaylists` table
   - Index on `UserId`
   - Index on `IsPublic`

3. **Service (`SmartPlaylistService.cs`):**
   ```csharp
   public interface ISmartPlaylistService
   {
       Task<SmartPlaylist> CreateAsync(CreateSmartPlaylistRequest request, Guid userId);
       Task<PagedResult<SongDto>> EvaluateAsync(Guid playlistId, PagedRequest paging, Guid userId);
       Task<SmartPlaylist> UpdateAsync(Guid playlistId, UpdateSmartPlaylistRequest request, Guid userId);
       Task DeleteAsync(Guid playlistId, Guid userId);
   }
   ```

4. **API endpoints:**
   - `POST /api/v1/playlists/smart` - create
   - `GET /api/v1/playlists/smart/{id}/evaluate` - execute query
   - `PUT /api/v1/playlists/smart/{id}` - update
   - `DELETE /api/v1/playlists/smart/{id}` - delete

5. **Validation on save:**
   - Validate MQL query syntax
   - Reject invalid queries
   - Store normalized form

6. **Re-evaluation:**
   - On `evaluate` call: parse, compile, execute
   - Update `LastResultCount` and `LastEvaluatedAt`
   - Use expression cache

7. **Integration tests:**
   - Create and evaluate smart playlist
   - Update query and re-evaluate
   - Public playlist visibility

**Acceptance Criteria:**
- [ ] Smart playlists persist correctly
- [ ] Re-evaluation returns current results
- [ ] Invalid queries rejected on save
- [ ] Only owner can modify private playlists

---

### Phase 13: Autocomplete & Suggestions

**Objective:** Provide intelligent autocomplete suggestions for field names, operators, and known values.

**Deliverables:**

1. **Suggestion service (`MqlSuggestionService.cs`):**
   ```csharp
   public interface IMqlSuggestionService
   {
       Task<List<MqlSuggestion>> GetSuggestionsAsync(
           string partialQuery,
           string entityType,
           int cursorPosition);
   }
   
   public record MqlSuggestion(
       string Text,
       string Type, // "field", "operator", "value", "keyword"
       string? Description,
       int InsertPosition);
   ```

2. **Context detection:**
   - After space: suggest fields or keywords
   - After field name: suggest `:`
   - After `:`: suggest operators or values
   - After operator: suggest values

3. **Field suggestions:**
   - All valid fields for entity type
   - Fuzzy matching for partial input
   - Include description

4. **Value suggestions:**
   - Genre: suggest known genres from library
   - Mood: suggest known moods from library
   - Artist: suggest known artist names (top N)
   - Year: suggest range (e.g., `1970-1980`)
   - Boolean: `true`, `false`

5. **Keyword suggestions:**
   - `AND`, `OR`, `NOT`
   - `top:`, relative dates

6. **API endpoint:**
   - `POST /api/v1/query/suggest`
   - Request: `{ "entity": "songs", "query": "art", "cursorPosition": 3 }`
   - Response: list of suggestions

7. **Performance:**
   - Cache genre/mood/artist lists
   - Limit suggestions to 10
   - Response time < 100ms

**Acceptance Criteria:**
- [ ] Suggestions contextually appropriate
- [ ] Field suggestions include descriptions
- [ ] Known values suggested for appropriate fields
- [ ] Response time < 100ms

---

### Phase 14: Regex Support (Guarded)

**Objective:** Implement optional, guarded regex support with security controls.

**Deliverables:**

1. **Regex parser extension:**
   - Recognize `/pattern/flags` syntax
   - Extract pattern and flags (i for case-insensitive)
   - Generate `RegexExpressionNode`

2. **Pattern validation:**
   - Length: max 100 characters
   - Prohibited patterns check
   - Timeout wrapper for evaluation

3. **Database-side regex (PostgreSQL only):**
   ```csharp
   // EF.Functions.ILike for case-insensitive
   // Raw SQL for ~ operator when needed
   ```

4. **Client-side fallback:**
   - Compile to `Regex` object
   - Apply to limited result set (< 1000)
   - Add warning to response

5. **Feature flag:**
   - `MqlOptions.EnableRegex` (default: false)
   - Require explicit opt-in

6. **Alternative operators (always available):**
   - `contains(value)` → `Contains`
   - `startsWith(value)` → `StartsWith`
   - `endsWith(value)` → `EndsWith`
   - `wildcard(pattern)` → SQL `LIKE`

7. **Unit tests:**
   - Valid regex compilation
   - Invalid regex rejection
   - Prohibited pattern detection
   - Timeout enforcement
   - Alternative operators

**Acceptance Criteria:**
- [ ] Regex disabled by default
- [ ] All guards from spec §4.5 enforced
- [ ] Alternative operators work without regex
- [ ] No ReDoS vulnerability possible

---

### Phase 15: Comprehensive Testing Suite

**Objective:** Achieve comprehensive test coverage meeting spec §7.3 requirements.

**Deliverables:**

1. **Test project structure:**
   ```
   tests/Melodee.Mql.Tests/
   ├── Unit/
   │   ├── Parser/
   │   ├── Validator/
   │   ├── Compiler/
   │   ├── Authorization/
   │   └── Cache/
   ├── Integration/
   ├── Performance/
   ├── Load/
   ├── Contract/
   └── EdgeCases/
   ```

2. **Coverage targets (from spec §7.3.1):**
   - Parser: 95%
   - Validator: 100%
   - Compiler: 90%
   - Authorization: 100%
   - Sanitizer: 95%
   - Cache: 90%
   - Overall: 90%

3. **Integration tests (from spec §7.3.2):**
   - Full pipeline: Parse → Compile → Execute
   - Smart playlist re-evaluation
   - Cross-entity queries
   - User-scoped fields with authentication

4. **Load tests (from spec §7.3.3):**
   - 100 concurrent users
   - P95 < 1000ms, P99 < 2000ms
   - Cache effectiveness > 50%

5. **Performance regression tests (from spec §7.3.4):**
   - Simple query: < 200ms
   - Medium query: < 500ms
   - Complex query: < 2000ms
   - Compilation: < 50ms

6. **Edge case tests (from spec §7.3.5):**
   - Empty query
   - Max length query
   - Special characters
   - Unicode
   - All operators

7. **Cache tests (from spec §7.3.6):**
   - Cache hit verification
   - Invalidation
   - Expiration
   - Memory limits

8. **CI integration:**
   - Coverage enforcement in pipeline
   - Performance budgets as test assertions

**Acceptance Criteria:**
- [ ] All coverage targets met
- [ ] All test categories implemented
- [ ] CI pipeline enforces coverage
- [ ] No flaky tests

---

### Phase 16: Performance Optimization & Monitoring

**Objective:** Optimize query execution and implement production monitoring.

**Deliverables:**

1. **Query optimization:**
   - Index recommendations for MQL fields
   - Query plan analysis
   - N+1 query prevention verification

2. **Database indexes:**
   ```sql
   CREATE INDEX IX_Songs_TitleNormalized ON Songs(TitleNormalized);
   CREATE INDEX IX_Songs_AlbumId ON Songs(AlbumId);
   CREATE INDEX IX_Albums_NameNormalized ON Albums(NameNormalized);
   CREATE INDEX IX_Albums_ReleaseDate ON Albums(ReleaseDate);
   CREATE INDEX IX_Artists_NameNormalized ON Artists(NameNormalized);
   CREATE INDEX IX_UserSongs_UserId_SongId ON UserSongs(UserId, SongId);
   ```

3. **Metrics collection:**
   - Query execution time
   - Cache hit rate
   - Parse time
   - Compile time
   - Error rate by error code

4. **Health endpoint:**
   - Cache statistics
   - Recent query latency percentiles
   - Error counts

5. **Alerting thresholds:**
   - P95 > 2s → warning
   - P99 > 5s → alert
   - Error rate > 5% → alert
   - Cache hit rate < 30% → warning

6. **Logging enhancement:**
   - Structured logging for all MQL operations
   - Query fingerprints for aggregation
   - User context (anonymized)

7. **Dashboard (Grafana/similar):**
   - Query volume over time
   - Latency percentiles
   - Error breakdown
   - Cache performance

**Acceptance Criteria:**
- [ ] All performance budgets met under load
- [ ] Monitoring dashboards operational
- [ ] Alerts configured
- [ ] Index recommendations documented

---

### Phase 17: Documentation & UI Components

**Objective:** Provide comprehensive documentation and UI components for MQL.

**Deliverables:**

1. **User documentation:**
   - MQL syntax guide with examples
   - Field reference by entity type
   - Operator reference
   - Error message explanations
   - Best practices

2. **API documentation:**
   - OpenAPI/Swagger annotations
   - Request/response examples
   - Error response documentation
   - Rate limiting documentation

3. **Developer documentation:**
   - Architecture overview
   - Extension points
   - Adding new fields
   - Adding new entities
   - Security considerations

4. **Blazor UI components:**
   - `MqlSearchInput` component with autocomplete
   - `MqlErrorDisplay` component
   - `MqlQueryBuilder` visual builder (optional)

5. **Error display integration:**
   - Highlight error position in input
   - Show suggestions inline
   - One-click suggestion application

6. **Localization:**
   - Error messages in supported languages
   - Field descriptions localized
   - Operator descriptions localized

7. **Help tooltips:**
   - Field-level help
   - Syntax examples
   - Link to full documentation

**Acceptance Criteria:**
- [ ] User can learn MQL from documentation
- [ ] All API endpoints documented
- [ ] UI components functional and accessible
- [ ] Error display helps users fix issues

---

## Dependencies Between Phases

```
Phase 1 ──┬── Phase 2 ── Phase 3 ──┬── Phase 4 ── Phase 5 ── Phase 6
          │                        │
          │                        └── Phase 7 ── Phase 8
          │
          └── Phase 9 (after 5) ── Phase 10 ── Phase 11 ── Phase 12
                                                    │
                                                    └── Phase 13
                                                    
Phase 14 (after 5, 7)

Phase 15 (after 1-14)

Phase 16 (after 10-11)

Phase 17 (after 9-13)
```

---

## Risk Considerations

1. **Performance:** Complex boolean queries may generate inefficient SQL. Mitigation: query plan analysis, index optimization.

2. **Security:** Regex support introduces ReDoS risk. Mitigation: strict guards, disabled by default, alternative operators.

3. **Backward Compatibility:** Existing API contracts must not break. Mitigation: optional `q` parameter, existing behavior preserved.

4. **User Adoption:** Complex syntax may deter users. Mitigation: autocomplete, visual builder, comprehensive documentation.

---

## Coding Agent Prompt Template

```
Implement MQL Phase {N}: {Phase Name}

Read these files first:
- prompts/MELODEE-QUERY-LANGUAGE.md - Full spec
- prompts/MELODEE-QUERY-LANGUAGE-IMPLEMENTATION.md - Phase {N} details
- .github/instructions/csharp.instructions.md - C# standards
- .github/instructions/testing.instructions.md - Test conventions

Follow patterns in src/Melodee.Common/ and src/Melodee.Services/.
Write tests in tests/Melodee.Mql.Tests/.
Run `dotnet format` and `dotnet build` and `dotnet test --filter "Tokenizer"` before completing.
Report: files changed, acceptance criteria status, test results.
Update: @prompts/MELODEE-QUERY-LANGUAGE-IMPLEMENTATION.md mark Phase complete if all criteria is met and all tests pass.
```

**Example**

```
Implement MQL Phase 6: Expression Tree Caching

Read these files first:
- prompts/MELODEE-QUERY-LANGUAGE.md - Full spec (§4.1-4.3 for syntax)
- prompts/MELODEE-QUERY-LANGUAGE-IMPLEMENTATION.md - Phase 1 details
- .github/instructions/csharp.instructions.md - C# standards
- .github/instructions/testing.instructions.md - Test conventions

Follow patterns in src/Melodee.Common/ and src/Melodee.Services/.
Write tests in tests/Melodee.Mql.Tests/.
Run `dotnet format` and `dotnet build` and `dotnet test --filter "Tokenizer"` before completing.
Report: files changed, acceptance criteria status, test results.
Update: @prompts/MELODEE-QUERY-LANGUAGE-IMPLEMENTATION.md mark Phase complete if all criteria is met and all tests pass.
```

---

## Success Criteria

Implementation is complete when:
- [x] 5 of 17 phases complete (Phases 1-5)
- [ ] All 17 phases marked complete
- [ ] Test coverage targets met (90%+ overall)
- [ ] Performance budgets met under load
- [ ] Security tests pass (injection, ReDoS, authorization)
- [ ] Documentation complete and reviewed
- [ ] UI components functional in Melodee.Blazor

## Curated Album Charts — Requirements (v1)

### Summary

Add a feature that lets administrators create **ordered, curated charts of albums** (e.g., “Top 500 Albums of All Time (Rolling Stone)”, “Top 50 Progressive Rock Albums of 2025”). Each chart:

- Stores a **ranked list** of album entries (rank → artist → album → year).
- Automatically **links** each entry to matching albums and artists already in the database.
- Can optionally expose a **generated playlist** that contains all tracks from all linked albums in **chart order**.

This document uses **“Chart”** as the primary term.

> Architectural constraint (v1): The public API is **read-only** and exists only for API consumers to fetch chart information.
> All admin creation/editing/import/linking is performed exclusively in the **Blazor admin UI** (server-side), not via public API endpoints.

---

### Data model

#### Entities

##### `Chart`
- `Id`
- `Slug` (unique)
- `Title`
- `SourceName` (e.g., “Rolling Stone”, “Pitchfork”; optional)
- `SourceUrl` (optional)
- `Year` (optional)
- `Description` (optional)
- `Tags` (optional; uses ToTags() and FromTags() extension methods)
- `IsVisible` (published)
- `IsGeneratedPlaylistEnabled`
- `CreatedAt`, `UpdatedAt`

##### `ChartItem`
- `Id`
- `ChartId`
- `Rank` (integer; unique per chart)
- `ArtistName` (as imported/displayed)
- `AlbumTitle` (as imported/displayed)
- `ReleaseYear` (optional; as imported/displayed)
- `LinkedArtistId` (nullable when not found in database)
- `LinkedAlbumId` (nullable when not found in database)
- `LinkStatus` (e.g., `Unlinked`, `Linked`, `Ambiguous`, `Ignored`)
- `LinkConfidence` (optional numeric score, if you already have a scoring scheme)
- `LinkNotes` (optional; for admin-only explanations)
- `CreatedAt`, `UpdatedAt`

#### Linking consistency rules
- If `LinkedAlbumId` is set, the system MUST be able to determine the album’s primary artist from the database and should automatically set `LinkedArtistId` accordingly.
- `LinkedArtistId` may be set without `LinkedAlbumId` (e.g., the artist was found but the album match is missing/ambiguous).
- Manual resolution in the admin UI must persist and should not be overwritten by background re-linking unless explicitly requested.

#### Relationships
- `Chart` 1→N `ChartItem`
- `ChartItem` N→1 `Artist` (optional link)
- `ChartItem` N→1 `Album` (optional link)

#### Constraints & indexes
- Unique index on `Chart.Slug`
- Unique index on (`ChartId`, `Rank`)
- Consider an index on (`ChartId`, `LinkedAlbumId`) for playlist generation and debugging

---

### Goals

1. Allow admins to create and maintain **ranked album lists** using CSV import.
2. Automatically link imported rows to existing **Album** and **Artist** entities.
3. Provide a user-facing experience to browse charts and jump to **Album** and **Artist** details.
4. Optionally generate a **dynamic playlist** for a chart that appears under Playlists and plays tracks in a deterministic order.
5. Provide a **read-only API** for external consumers to retrieve chart information and chart-derived playlists.

---

### API requirements (v1, read-only)

> The API is intended for **consumption only**. It MUST NOT expose endpoints that allow creating, mutating, importing, linking, or deleting charts.
> All admin operations happen in the Blazor admin UI and execute server-side within the application boundary.

#### Charts (read-only)
- `GET /api/v1/charts` (visible-only; supports paging/filtering by tags/year/source)
  - Tag filtering semantics (v1): case-insensitive exact match; multiple tags use AND semantics (chart must contain all requested tags).
- `GET /api/v1/charts/{id|slug}` (detail + items)

#### Generated playlist (read-only)
- `GET /api/v1/playlists` includes chart-generated virtual playlists (or provide a dedicated endpoint)
- `GET /api/v1/playlists/{playlistId}` returns track list for the synthetic playlist
  - `playlistId` could be `chart:{chartId}`

---

### Admin UI requirements (Blazor)

#### Chart editor
- Metadata form fields + toggles
- CSV paste area and/or file upload
- Preview grid with:
  - Validation errors
  - Link status badges
  - “Resolve” action for ambiguous/unlinked rows
- Save / Re-link / Publish toggles

#### Resolve modal
- Shows row details
- Shows candidates with confidence
- Search box to find album
- Option: mark ignored/unresolved

> Note: Admin UI actions call server-side application services directly and are protected by `Charts.Manage`.
> These actions are intentionally not exposed as public HTTP endpoints.

---

### Validation & error handling

- CSV parsing errors must include row numbers.
- Reject duplicate ranks and non-integer ranks.
- If title duplicates create slug conflict, auto-suffix (`-2`, `-3`).
- If admin saves a chart with 0 items, allow but warn.

---

### Non-functional requirements

#### Performance
- Import preview + linking should handle:
  - 500 items (common for “Top 500”) within ~2–5 seconds on a typical home server
  - 5,000 items within a reasonable time (may require background job; optional)
- Avoid N+1 queries when linking; batch lookups and cache normalized tokens.

#### Reliability
- Save is transactional:
  - either all items persist successfully or none do
- Re-linking must not corrupt manual selections

#### Security
- Admin UI actions require admin role/permission (`Charts.Manage`).
- Public API is read-only and should not accept write operations for charts.
- CSV input must be treated as untrusted:
  - no formula evaluation
  - sanitize displayed strings for UI

#### Observability
- Log import operations: count, linked/unlinked/ambiguous, duration
- Include correlation id for troubleshooting

#### Internationalization
- Support non-English artist/album names (Unicode normalization).
- Sorting should be stable under locale rules (or use invariant rules consistently).

---

### Migration & compatibility

- No change required for existing user libraries.
- Feature is additive: new entities/tables only.

---

### Open questions (capture for v2)
1. Should users be able to create their own charts, or admin-only forever?
2. Do we want “draft/published” states beyond a simple `IsVisible`?
3. Should charts support **multiple versions** (e.g., 2024, 2025) under a series?
4. How aggressively should fuzzy matching run by default, and what threshold?
5. Should generated playlists be cached/materialized for faster browsing?

---

### Unit testing requirements

> Unit tests should focus on the server-side application services and domain logic that power the Blazor admin UI.
> The public API surface is read-only and should be covered by contract-level tests that verify it does not allow mutation.

#### Scope (what must be unit-tested)
- CSV parsing + validation (rank, required fields, year parsing)
- Slug generation + uniqueness behavior
- Linking logic (exact match, fuzzy match, ambiguity detection, manual overrides)
- Chart item ordering rules
- Generated playlist track ordering
- Permission gating at the service level (e.g., `Charts.Manage` required for mutation methods)

#### Happy path tests (minimum)
1. **Create chart**
   - Given valid chart metadata, when creating a chart, then it persists with a unique slug and default flags.

2. **CSV import + preview**
   - Given a well-formed CSV with unique ranks, when parsing, then preview returns items in rank order with no validation errors.

3. **Linking**
   - Given imported items that exactly match an existing album and artist, when linking runs, then `LinkedAlbumId`, derived `LinkedArtistId`, and `LinkStatus=Linked` are set.

4. **Generated playlist ordering**
   - Given a chart with linked albums, when generating a synthetic playlist, then tracks are returned ordered by:
     1) chart rank,
     2) album track number/disc ordering (or your canonical track ordering),
     3) stable tiebreakers (e.g., track id).

5. **Read-only API rule is not violated**
   - Given the API module, verify only `GET` endpoints exist for charts (and chart playlists), and mutation endpoints are not registered.

#### Edge case tests (minimum)
- **CSV validation**
  - Empty input returns a clear validation error.
  - Non-integer rank is rejected.
  - Duplicate rank is rejected.
  - Missing artist/album fields are rejected.
  - Very large rank values are accepted/rejected according to defined bounds (decide and test).

- **Slug conflicts**
  - Two charts with the same title produce `slug` and `slug-2` (and increment beyond).

- **Ambiguous linking**
  - Multiple candidate albums match: `LinkStatus=Ambiguous` with candidate list (or a deterministic ambiguity marker) and no `LinkedAlbumId` set.

- **Manual resolution persistence**
  - When an item has been manually linked, a re-link operation does not overwrite it unless an explicit overwrite flag is provided.

- **Partial link**
  - Artist matches but album does not: `LinkedArtistId` can be set while `LinkedAlbumId` remains null and status reflects not-fully-linked.

- **Generated playlist with missing links**
  - Items with no `LinkedAlbumId` are excluded from the generated playlist (or handled per your chosen rule) and ordering of the remaining tracks remains stable.

- **Transactional save**
  - Inject a persistence failure mid-save and assert no partial chart items persist.

- **Authorization**
  - Mutation service methods fail/deny when called without `Charts.Manage`.

#### Test style
- Use Arrange/Act/Assert structure.
- Prefer deterministic tests: no reliance on current time, random values, or external network.
- Use an in-memory database or test database fixture (consistent with existing project conventions).

---

### Acceptance test checklist (high-level)

- [ ] Admin can create a chart with required metadata and unique slug.
- [ ] Admin can paste CSV and see preview with row-level validation.
- [ ] Save persists items ordered by rank.
- [ ] Linking correctly matches exact and fuzzy cases; ambiguous rows are flagged.
- [ ] Admin can manually resolve ambiguous/unlinked rows.
- [ ] User can browse visible charts and open details.
- [ ] Album/artist links navigate correctly.
- [ ] Generated playlist appears only when enabled and has tracks.
- [ ] Playlist track order is correct: rank order, then track order.
- [ ] Re-linking does not overwrite manual links unless explicitly requested.
- [ ] Unit tests cover CSV parsing/validation (happy path + edge cases).
- [ ] Unit tests cover linking (exact, ambiguous, manual override persistence).
- [ ] Unit tests cover generated playlist ordering.
- [ ] Tests assert the public API exposes read-only chart endpoints only (no mutations).

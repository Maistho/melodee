# Statistics Page (Stats) — Implementation Plan

## Phase map (checkbox tracker)
- [x] **Phase 0 — User timezone support**: add `User.TimeZoneId` (default UTC) + profile editor + timezone-aware formatting
- [ ] **Phase 1 — Play history schema**: add `UserSongPlayHistory` table + indexes + migration
- [ ] **Phase 2 — Record play events**: write play rows when scrobbling / playing
- [ ] **Phase 3 — StatisticsService expansion**: add timeseries/toplist aggregation methods (ALL shaping/flattening in service)
- [ ] **Phase 4 — Unit tests**: add tests for *new* stats methods + fill gaps for existing public methods
- [ ] **Phase 5 — Stats page UI**: new `/stats` page (DI `StatisticsService`) + NavMenu link + charts/tables/cards
- [ ] **Phase 6 — Role-based sections**: add Editor/Admin panels with additional stats

---

## Goal
Add a new authenticated Blazor page `Stats` that shows **graphical**, **tabular**, and **card**-based statistics for the **current user**, with additional sections for **Editors** and **Admins**.

Constraints / requirements:
- The Blazor page will **DI `StatisticsService` directly** (no HTTP controllers/endpoints for this requirement).
- All data shaping (grouping, bucketing, flattening arrays like genres, top-N selection, etc.) must happen **in services**, not in `.razor` pages.

This plan is derived from the current EF Core schema (`MelodeeDbContext`) and the existing play-recording pipeline in `MelodeeScrobbler`.

---

## What the schema supports today (relevant fields)

### Growth / added-over-time
Most tables inherit `DataModelBase`:
- `CreatedAt` (NodaTime `Instant`)

So we can trend accurately:
- `Song.CreatedAt`, `Album.CreatedAt`, `Artist.CreatedAt`, `User.CreatedAt`, `Playlist.CreatedAt`, `Share.CreatedAt`, etc.

### Plays
Today you have counters + last-play timestamps:
- Global: `Song/Album/Artist.PlayedCount`, `LastPlayedAt`
- Per-user: `UserSong.PlayedCount`, `UserSong.LastPlayedAt` (+ similar for albums)

This is good for **top played** and **most recent**, but **not** for historical “plays per day”.

### Where plays are currently recorded
`src/Melodee.Common/Plugins/Scrobbling/MelodeeScrobbler.cs`:
- increments `Artists/Albums/Songs.PlayedCount` + sets `LastPlayedAt`
- increments or inserts `UserSong` (`PlayedCount` + `LastPlayedAt`)

This is the correct hook to also write to a play history table.

---

## New required schema: `UserSongPlayHistory`

### Why
To support **Songs Played Per Day** (and other historical analytics) accurately, we need a per-play event table.

### Entity/table definition (required)
Create a new EF Core model `UserSongPlayHistory` (name can be adjusted, but keep intent explicit).

Suggested columns:
- `Id` (int, PK, identity)
- `UserId` (int, **required**, FK → `Users.Id`)
- `SongId` (int, **required**, FK → `Songs.Id`)
- `PlayedAt` (Instant, **required**) — when the play/scrobble was recorded
- `Client` (string, required, e.g. scrobbler/client name; consistent with `ShareActivity.Client`)
- `ByUserAgent` (string?, optional)
- `IpAddress` (string?, optional)
- `SecondsPlayed` (int?, optional) — if available, allows completion/skip analytics
- `Source` (smallint/int, required) — enum (e.g. Stream, Share, Radio, Unknown)

Suggested indexes:
- `(UserId, PlayedAt)` for per-user timeseries queries
- `(SongId, PlayedAt)` for song timeseries queries
- `(PlayedAt)` for global timeseries queries

EF relationships:
- `User` navigation
- `Song` navigation

### Migration tasks
- Add `DbSet<UserSongPlayHistory>` to `MelodeeDbContext`
- Add new model in `src/Melodee.Common/Data/Models/UserSongPlayHistory.cs`
- Create EF migration under `src/Melodee.Common/Migrations/`

---

## Proposed statistics to show on the new `Stats` page

### A) User-focused stats (default)

#### Cards (KPIs)
- **Total plays (you)**: `sum(UserSongPlayHistory)` over selected time window
- **Songs played per day (you)**: today/7d/30d totals + chart
- **Estimated listening time (you)**: `sum(SecondsPlayed)` if available, otherwise `sum(Song.Duration)` per play (approx)
- **Favorites**: counts of `UserSong.StarredAt != null`, `UserAlbum.StarredAt != null`, `UserArtist.StarredAt != null`
- **Ratings**: counts of rated songs/albums/artists

#### Charts
- **Songs Played Per Day (you)** ✅ (accurate): bucket `UserSongPlayHistory.PlayedAt` by day
- **Songs Added Per Day** ✅ (accurate): bucket `Song.CreatedAt` by day
- **Albums Added Per Month**: bucket `Album.CreatedAt` by month
- **Top genres you play**: aggregate plays by genre using `Song.Genres` (flattened in service)
- **Format/quality breakdown** (bar/pie): by `Song.ContentType`, bitrate buckets, bit depth

#### Tables
- **Top played songs (you)**: aggregate from `UserSongPlayHistory` grouped by song
- **Recently played (you)**: latest `UserSongPlayHistory.PlayedAt`
- **Most played artists (you)**: join via `Song → Album → Artist`


### B) Editor-focused stats (visible to Editors/Admins)
- **Items missing images**: artists/albums/songs with `ImageCount` null/0
- **Metadata freshness**: counts where `LastMetaDataUpdatedAt == null`
- **Metadata status distribution**: `MetaDataStatus` breakdown


### C) Admin-focused stats (visible to Admins)
- **Active users**: by `Users.LastActivityAt` (1/7/30 days)
- **Search volume & performance**: `SearchHistories` count per day + p50/p95 duration
- **Share views per day**: `ShareActivities` per day
- **Scan performance**: `LibraryScanHistories` durations and counts

---

## Service-first shaping rule (no data manipulation in Razor)

The `Stats.razor` page should:
- pick filters (date range, grouping)
- call `StatisticsService` methods
- render returned DTOs

The `StatisticsService` should:
- perform all bucketing/grouping
- flatten genre arrays
- apply ordering/top-N
- return already UI-ready data (time series points + table rows + KPIs)

---

## Implementation tasks by phase

### Phase 0 — User timezone support
- [x] Add `User.TimeZoneId` (IANA timezone id) with default `UTC`.
- [x] Add profile UI to edit TimeZoneId (default UTC) and validate it.
- [x] Add timezone-aware `FormatInstant` helper so existing timestamps in the UI display in the user’s timezone.
- [ ] Ensure all new time-series bucketing in `StatisticsService` uses the user timezone (not UTC) when converting `Instant → LocalDate`.

### Phase 1 — Play history schema
- [ ] Add `UserSongPlayHistory` model + EF configuration (attributes + indexes)
- [ ] Add `DbSet<UserSongPlayHistory>` to `MelodeeDbContext`
- [ ] Create migration + ensure DB update works

### Phase 2 — Record play events
- [ ] In `MelodeeScrobbler.Scrobble(...)`, insert a `UserSongPlayHistory` row for each scrobble
  - `UserId` from `user.Id`
  - `SongId` from `scrobble.SongId`
  - `PlayedAt = now`
  - `Client = nameof(MelodeeScrobbler)` (or use `scrobble.Client` if available)
  - `Source` = Stream (or Unknown until more wiring)
  - optionally populate `ByUserAgent` / `IpAddress` if available at this layer
- [ ] Decide dedupe behavior (if needed):
  - simplest: one row per scrobble callback
  - optional: prevent duplicates within N seconds per user+song

### Phase 3 — StatisticsService expansion
Add new DTOs (in `Melodee.Common/Models` or alongside `Statistic`):
- [ ] `TimeSeriesPoint` (day + value + optional series)
- [ ] `TopItemStat` (label/value + optional ApiKey)

Add new `StatisticsService` methods (all shaping happens here):
- [ ] `GetUserSongPlaysPerDay(userApiKey, start, end)` → `TimeSeriesPoint[]`
- [ ] `GetSongsAddedPerDay(start, end)` → `TimeSeriesPoint[]`
- [ ] `GetUserTopPlayedSongs(userApiKey, start, end, topN)` → `TopItemStat[]`
- [ ] `GetUserRecentlyPlayedSongs(userApiKey, start, end, topN)` → `TopItemStat[]`
- [ ] `GetUserTopGenresByPlays(userApiKey, start, end, topN)` → `TopItemStat[]` (flatten genres in service)

### Phase 4 — Unit tests (new + coverage gaps)
Tests live in `tests/Melodee.Tests.Common/Common/Services/StatisticsServiceTests.cs` using SQLite in-memory.

Add tests for **new methods**:
- [ ] plays-per-day returns correct day buckets (including zero-fill days in range if desired)
- [ ] top played songs matches expected counts
- [ ] songs added per day matches CreatedAt bucketing
- [ ] genre aggregation correctly flattens arrays and groups case-insensitively

Fill gaps for **existing public methods** in `StatisticsService` (currently missing direct tests):
- [ ] `GetAlbumCountAsync` returns the album count and correct category/type
- [ ] `GetArtistCountAsync` returns the artist count and correct category/type
- [ ] `GetSongCountAsync` returns the song count and correct category/type

### Phase 5 — Stats page UI (DI StatisticsService)
- [ ] Add `src/Melodee.Blazor/Components/Pages/Stats.razor`:
  - `@page "/stats"`
  - `@inherits MelodeeComponentBase`
  - inject `StatisticsService`
  - call `await base.OnInitializedAsync();`
- [ ] Add Nav link in `Components/Layout/MainLayout.razor`:
  - `RadzenPanelMenuItem Text="Stats" Path="/stats" Icon="bar_chart"`

#### Move Dashboard “Statistics” badges to Stats page
Dashboard currently renders statistics via `_statistics` (loaded from `StatisticsService.GetStatisticsAsync()`) inside:
`src/Melodee.Blazor/Components/Pages/Dashboard.razor` → the `RadzenPanel` titled **"Statistics"** (currently `class="hide-below-1024"`).

- [ ] Copy/move the entire `@foreach (var statistic in _statistics)` block that renders **`RadzenBadge`** elements (including the title/value formatting) from `Dashboard.razor` into `Stats.razor`.
  - Keep the existing value formatting logic:
    - if `statistic.Type == StatisticType.Count` format as padded int
    - else display `statistic.Data`
- [ ] Remove the `Statistics` panel from `Dashboard.razor` to reduce dashboard clutter.
- [ ] Keep `Dashboard.razor`’s other sections (pins + latest artists/albums) unchanged.
- [ ] Optional: If dashboard no longer needs the full statistics load, remove the `_statistics` field + `GetStatisticsAsync()` call from `Dashboard.razor`.

#### Base Stats page content
- [ ] Implement UI sections:
  - KPI cards row
  - **Existing summary badges** (moved from dashboard) rendered from `GetStatisticsAsync()`
  - line chart for plays-per-day
  - line chart for songs-added-per-day
  - table for top played songs

### Phase 6 — Role-based sections
- [ ] Editor panel: missing images, metadata freshness
- [ ] Admin panel: searches per day, shares per day, scan performance

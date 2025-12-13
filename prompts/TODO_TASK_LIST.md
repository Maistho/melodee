# Melodee TODO Task List

This document aggregates outstanding TODOs and unimplemented methods across the repository. Items are grouped by domain and organized into phases to guide implementation. Check items off as they are completed.

Last updated: 2025-12-13T16:06:22Z

## Solution map (projects in `Melodee.sln`)

- **`src/Melodee.Blazor`**: ASP.NET Core + Blazor UI, controllers for **Melodee** and **OpenSubsonic** REST endpoints.
- **`src/Melodee.Common`**: Core domain models, services, plugins (search engines, scrobblers), serialization, scanning.
- **`src/Melodee.Cli`**: CLI utilities (useful for admin/maintenance workflows).
- **`tests/Melodee.Tests.Common`**: Service/domain tests.
- **`tests/Melodee.Tests.Blazor`**: Blazor/controller/UI-oriented tests.
- **`benchmarks/Melodee.Benchmarks`**: Benchmarks (should not be required to complete TODOs).

## How to use this list (for coding agents)

- Prefer implementing each checkbox as a **single, reviewable PR-sized change** (even if you’re working locally).
- For each item, “done” means:
  - No `NotImplementedException`/TODO left in the referenced method(s).
  - Behavior is covered by an existing test or a new test in the appropriate test project.
  - If the feature depends on external APIs (Spotify/Last.fm/iTunes), it must **fail gracefully** when credentials are missing.

## Completed (historical reference)

These were previously tracked here and are already done:

- [x] Songs API: implement `SongById(Guid id)` (`src/Melodee.Blazor/Controllers/Melodee/SongsController.cs`)
- [x] Subsonic Auth: handle JWT auth branch (`src/Melodee.Common/Services/OpenSubsonicApiService.cs`)
- [x] JSON Converter: implement `OpenSubsonicResponseModelConvertor.Read` (`src/Melodee.Common/Serialization/Convertors/OpenSubsonicResponseModelConvertor.cs`)
- [x] Library deletion: implement `LibraryService.DeleteAsync(int[])` (`src/Melodee.Common/Services/LibraryService.cs`)
- [x] ID3v2 writer: ID3v2.4 tag writing (`src/Melodee.Common/Metadata/AudioTags/Writers/Id3v2TagWriter.cs`)
- [x] IdSharpMetaTag: `UpdateSongAsync(...)` (`src/Melodee.Common/Plugins/MetaData/Song/IdSharpMetaTag.cs`)
- [x] Fill TODO fields in album/song DTO conversions (`AlbumExtensions.cs`, `SongExtensions.cs`)

## Outstanding work (phased)

### Phase 1 — External search engines & scrobbling (credentials-aware)

> Implementation rule: when API keys are missing, return a **successful empty result** with a clear message (don’t throw).

- [ ] iTunes: implement artist search
  - File: `src/Melodee.Common/Plugins/SearchEngine/ITunes/ITunesSearchEngine.cs`
  - Scope:
    - Implement `DoArtistSearchAsync(...)` and any required internal parsing helpers.
    - Use the existing HTTP/client patterns in the repo (and centralize serialization if possible).
  - Acceptance criteria:
    - Returns `PagedResult<ArtistSearchResult>` ordered by best match.
    - Handles timeouts / non-200 responses gracefully.

- [ ] Last.fm: implement artist search
  - File: `src/Melodee.Common/Plugins/SearchEngine/LastFm/LastFm.cs`
  - Scope:
    - Implement `DoArtistSearchAsync(...)`.
    - Respect configuration-driven API key/secret (see `SettingRegistry` usages in other plugins).
  - Acceptance criteria:
    - No `NotImplementedException` remains.
    - If keys are absent → returns empty results + message.

- [ ] Spotify: implement “top songs for artist” search
  - File: `src/Melodee.Common/Plugins/SearchEngine/Spotify/Spotify.cs`
  - Method: `DoArtistTopSongsSearchAsync(int forArtist, int maxResults, ...)`
  - Notes:
    - This currently returns `"Spotify Not implemented"`.
    - Decide how `forArtist` maps to Spotify identifiers (likely via stored `SpotifyId` on artist metadata or an intermediate lookup).
  - Acceptance criteria:
    - Returns a `PagedResult<SongSearchResult>` (even if limited fields) and does not require UI.

- [ ] Last.fm scrobbling: session key support
  - File: `src/Melodee.Common/Plugins/Scrobbling/LastFmScrobbler.cs`
  - Scope:
    - Ensure `NowPlaying`/scrobble calls use a **user-specific session key**.
    - Add/update persistence for the session key (where user credentials/settings are stored today).
  - Acceptance criteria:
    - No TODO about session handling remains.
    - Network failures retry safely (use existing resiliency approach if present; otherwise minimal retry w/ backoff).

### Phase 2 — OpenSubsonic endpoints (API completeness)

- [ ] Implement Similar Songs endpoints
  - File: `src/Melodee.Blazor/Controllers/OpenSubsonic/BrowsingController.cs`
  - Endpoints: `getSimilarSongs`, `getSimilarSongs2` (see TODO at top of controller)
  - Scope:
    - Add controller routes + call(s) into `OpenSubsonicApiService`.
    - Define how similarity is computed (minimum viable: same artist, same album, shared genres/tags, or play-history based).
  - Acceptance criteria:
    - Endpoints return valid OpenSubsonic response models (and honor auth).
    - Pagination/count parameters behave predictably.

- [ ] Jukebox control endpoint behavior
  - File: `src/Melodee.Blazor/Controllers/OpenSubsonic/JukeboxController.cs`
  - Endpoint: `jukeboxControl`
  - Decision required:
    - Either implement a minimal server-side jukebox state machine, **or** explicitly return `410 Gone`/"not supported" consistently for all jukebox routes.
  - Acceptance criteria:
    - No placeholder-only controller remains; behavior is explicit and documented in-code.

### Phase 3 — Blazor UI actions (remove `NotImplementedException` + complete flows)

- [ ] ImageSearchUpload: handle Radzen Upload events
  - File: `src/Melodee.Blazor/Components/Components/ImageSearchUpload.razor`
  - Methods: `OnChange(byte[] value, string name)`, `OnError(UploadErrorEventArgs args, string name)`
  - Acceptance criteria:
    - Uploading an image feeds into the existing image-search/selection UI and calls `OnUpdateCallback`.
    - Errors show a user-visible notification (Radzen `NotificationService`).

- [ ] IdentifyAlbum: handle upload events
  - File: `src/Melodee.Blazor/Components/Components/IdentifyAlbum.razor`
  - Acceptance criteria:
    - Upload works end-to-end (store temporarily, trigger identify/search, display results or errors).

- [ ] AlbumDetail: multi-storage move prompt
  - File: `src/Melodee.Blazor/Components/Pages/Media/AlbumDetail.razor`
  - Method: `MoveButtonClick()`
  - Scope:
    - When `LibraryService.GetStorageLibrariesAsync()` returns more than 1, prompt user to choose target library.
  - Acceptance criteria:
    - No throw when >1 storage library exists.

- [ ] AlbumDetail: “Add Artist” button action
  - File: `src/Melodee.Blazor/Components/Pages/Media/AlbumDetail.razor`
  - Method: `AddNewArtistButtonClick()`
  - Acceptance criteria:
    - Navigates to (or opens) `ArtistEdit` with pre-filled artist name (and whatever IDs are available).

- [ ] ArtistDetail: drag-and-drop (and paste) upload for artist images
  - File: `src/Melodee.Blazor/Components/Pages/Data/ArtistDetail.razor`
  - Related JS: `src/Melodee.Blazor/wwwroot/js/FileDropZone.js` (currently unused)
  - Scope:
    - Add a visible drop zone in the Artist “Images” section.
    - Wire JS interop to `initializeFileDropZone(...)` so dropping/pasting images triggers the existing `OnArtistImageUpload` flow.
  - Acceptance criteria:
    - Dragging files onto the drop zone uploads the same as selecting via `<InputFile>`.
    - Pasting an image (clipboard file) into the drop zone uploads as well.

- [ ] ArtistEdit: external search integration
  - File: `src/Melodee.Blazor/Components/Pages/Data/ArtistEdit.razor`
  - Method: `SearchForExternalButtonClick(string amgid)`
  - Acceptance criteria:
    - Calls into the search engine service layer and populates UI with selectable results.

- [ ] AlbumEdit: external search integration
  - File: `src/Melodee.Blazor/Components/Pages/Media/AlbumEdit.razor`
  - Method: `SearchForExternalButtonClick(string amgid)`

- [ ] PlaylistDetail: image set/lock/unlock
  - File: `src/Melodee.Blazor/Components/Pages/Data/PlaylistDetail.razor`
  - Methods: `SetPlaylistImageButtonClick()`, `UnlockButtonClick()`, `LockButtonClick()`

- [ ] Library page: multi-library move prompt + clean action
  - File: `src/Melodee.Blazor/Components/Pages/Media/Library.razor`
  - Scope:
    - Prompt for destination when multiple storage libraries exist.
    - Implement `CleanButtonClick()` (expected to remove missing files / orphan records; align with existing scanning/cleanup services).

- [ ] LibraryDetail: Edit button behavior
  - File: `src/Melodee.Blazor/Components/Pages/Data/LibraryDetail.razor`
  - Method: `EditButtonClick()`

- [ ] Albums/Songs grids: unimplemented actions
  - Files: `src/Melodee.Blazor/Components/Pages/Data/Albums.razor`, `src/Melodee.Blazor/Components/Pages/Data/Songs.razor`
  - Acceptance criteria:
    - Any remaining action handlers no longer throw.

### Phase 4 — Service/domain polish (correctness)

- [ ] SongService: implement song deletion
  - File: `src/Melodee.Common/Services/SongService.cs`
  - Method: `DeleteAsync(int[] songIds, ...)` (currently throws `NotImplementedException`)
  - Scope:
    - Decide policy: DB-only delete vs. also delete media file on disk.
    - Handle referential integrity (contributors, play history/now playing, user-song relations, caches).
  - Acceptance criteria:
    - All UI call-sites that delete songs succeed (e.g., album “Delete song” / “Delete selected songs”).
    - Add/adjust tests in `tests/Melodee.Tests.Common` (see SongServiceTests section below).

- [ ] ServiceBase: CRC hash discrepancy
  - File: `src/Melodee.Common/Services/ServiceBase.cs`
  - Goal:
    - Determine why `song.CrcHash` can differ from the computed CRC and fix root cause (path normalization, stream reading, encoding, etc.).
  - Acceptance criteria:
    - Add a focused regression test for CRC computation.

- [ ] ArtistExtensions: decide `Url` mapping
  - File: `src/Melodee.Common/Data/Models/Extensions/ArtistExtensions.cs`
  - Goal:
    - Either populate a canonical URL (and document source), or remove/leave blank intentionally (but no lingering TODO).

### Phase 5 — Tests & verification

- [ ] SongServiceTests: update DeleteAsync expectations after implementation
  - File: `tests/Melodee.Tests.Common/Common/Services/SongServiceTests.cs`
  - Scope:
    - Replace the current `DeleteAsync_ThrowsNotImplementedException` test with assertions matching the new delete behavior.

- [ ] UserServiceTests: verify “bus event published” assertion
  - File: `tests/Melodee.Tests.Common/Common/Services/UserServiceTests.cs`
  - Acceptance criteria:
    - Tests assert the correct bus publish behavior using the existing test/mocking patterns in the repo.

---

## Index of References

For quick grep reference (current as of last updated), these locations contain `NotImplementedException` and/or active TODOs:

**tests**
- `tests/Melodee.Tests.Common/Common/Services/SongServiceTests.cs` (DeleteAsync test currently expects `NotImplementedException`)
- `tests/Melodee.Tests.Common/Common/Services/UserServiceTests.cs` (TODO: bus event published assertion)

**src**
- `src/Melodee.Blazor/Components/Components/ImageSearchUpload.razor` (upload handlers `NotImplementedException`)
- `src/Melodee.Blazor/Components/Components/IdentifyAlbum.razor` (upload handlers `NotImplementedException`)
- `src/Melodee.Blazor/Components/Pages/Media/AlbumDetail.razor` (multi-storage move prompt + Add Artist button `NotImplementedException`)
- `src/Melodee.Blazor/Components/Pages/Media/Library.razor` (multi-storage prompt + clean button `NotImplementedException`)
- `src/Melodee.Blazor/Components/Pages/Data/ArtistEdit.razor` (external search button `NotImplementedException`)
- `src/Melodee.Blazor/Components/Pages/Media/ArtistEdit.razor` (external search button `NotImplementedException`)
- `src/Melodee.Blazor/Components/Pages/Data/AlbumEdit.razor` (external search button `NotImplementedException`)
- `src/Melodee.Blazor/Components/Pages/Media/AlbumEdit.razor` (external search button `NotImplementedException`)
- `src/Melodee.Blazor/Components/Pages/Data/PlaylistDetail.razor` (image/lock/unlock `NotImplementedException`)
- `src/Melodee.Blazor/Components/Pages/Data/LibraryDetail.razor` (Edit button `NotImplementedException`)
- `src/Melodee.Blazor/Components/Pages/Data/Albums.razor` (action handler `NotImplementedException`)
- `src/Melodee.Blazor/Components/Pages/Data/Songs.razor` (action handler `NotImplementedException`)
- `src/Melodee.Blazor/Controllers/OpenSubsonic/BrowsingController.cs` (TODO: similar songs endpoints)
- `src/Melodee.Blazor/Controllers/OpenSubsonic/JukeboxController.cs` (TODO: jukeboxControl)
- `src/Melodee.Common/Plugins/SearchEngine/ITunes/ITunesSearchEngine.cs` (`NotImplementedException`)
- `src/Melodee.Common/Plugins/SearchEngine/LastFm/LastFm.cs` (`NotImplementedException`)
- `src/Melodee.Common/Services/SongService.cs` (`DeleteAsync` throws `NotImplementedException`)
- `src/Melodee.Common/Plugins/Scrobbling/LastFmScrobbler.cs` (TODO: session key handling)
- `src/Melodee.Common/Services/ServiceBase.cs` (TODO: CRC hash discrepancy)
- `src/Melodee.Common/Services/OpenSubsonicApiService.cs` (TODO/NotImplemented branches remain)
- `src/Melodee.Common/Data/Models/Extensions/ArtistExtensions.cs` (TODO: Url mapping)
- `src/Melodee.Common/Plugins/SearchEngine/Spotify/Spotify.cs` (TODO: resiliency; top-songs currently returns "not implemented")

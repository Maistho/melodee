# Melodee TODO Task List

This document aggregates outstanding TODOs and unimplemented methods across the repository. Items are grouped by domain and organized into phases to guide implementation. Check items off as they are completed.

Last updated: 2025-12-13T16:59:21.198Z

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

> Decision (ADR-0003): credentials live in the **Settings** table; missing/invalid creds **disable** the integration; external calls must use DI `ICacheManager` caching.

> Implementation rule: when API keys are missing, return a **successful empty result** with a clear message (don’t throw).

- [ ] iTunes: implement artist search
  - File: `src/Melodee.Common/Plugins/SearchEngine/ITunes/ITunesSearchEngine.cs`
  - Scope:
    - Implement `DoArtistSearchAsync(...)` and any required internal parsing helpers.
    - Use the existing HTTP/client patterns in the repo (and centralize serialization if possible).
    - Cache results via DI `ICacheManager`.
  - Acceptance criteria:
    - Returns `PagedResult<ArtistSearchResult>` ordered by best match.
    - Handles timeouts / non-200 responses gracefully.

- [ ] Last.fm: implement artist search
  - File: `src/Melodee.Common/Plugins/SearchEngine/LastFm/LastFm.cs`
  - Scope:
    - Implement `DoArtistSearchAsync(...)`.
    - Read API key/secret from Settings; missing/invalid disables integration.
    - Cache results via DI `ICacheManager`.
  - Acceptance criteria:
    - No `NotImplementedException` remains.
    - If keys are absent → returns empty results + message.

- [ ] Spotify: implement “top songs for artist” search
  - File: `src/Melodee.Common/Plugins/SearchEngine/Spotify/Spotify.cs`
  - Method: `DoArtistTopSongsSearchAsync(int forArtist, int maxResults, ...)`
  - Notes:
    - This currently returns `"Spotify Not implemented"`.
    - Identifier mapping (decided): `forArtist` maps to `Artist.SpotifyId`.
      - If `SpotifyId` is missing: log a clear warning and return an empty result.
    - Use Settings-based credentials; missing/invalid disables integration; cache via `ICacheManager`.
  - Acceptance criteria:
    - Returns a `PagedResult<SongSearchResult>` (even if limited fields) and does not require UI.

- [ ] Last.fm scrobbling: session key support
  - File: `src/Melodee.Common/Plugins/Scrobbling/LastFmScrobbler.cs`
  - Decisions:
    - Follow `prompts/ADR-LOG.md` (ADR-0004):
      - Obtain session key via `auth.getSession` (web auth); do not use `auth.getMobileSession`.
      - Store per-user session key in `User.LastFmSessionKey`.
      - On invalid session errors: clear session key and require re-link.
  - Scope:
    - Ensure `NowPlaying`/scrobble calls include the user session key.
    - Add/link UI flow for user to connect/disconnect Last.fm (token exchange endpoint).
    - Credentials (`apiKey`/`sharedSecret`) still come from Settings; missing/invalid disables integration.
  - Acceptance criteria:
    - No TODO about session handling remains.
    - Missing session key for user => no-op success (predictable behavior).
    - Invalid session => key cleared and user must re-auth.

### Phase 2 — OpenSubsonic endpoints (API completeness)

- [ ] Implement Similar Songs endpoints
  - File: `src/Melodee.Blazor/Controllers/OpenSubsonic/BrowsingController.cs`
  - Endpoints: `getSimilarSongs`, `getSimilarSongs2` (see TODO at top of controller)
  - Similarity decision (decided): **admin-managed** via `ArtistRelationType.Similar`
    - Primary signal: songs from artists related by `ArtistRelation` where `Type == Similar`.
    - Do not rely on third-party providers for similarity.
    - Rationale captured in `prompts/ADR-LOG.md` (ADR-0002).
  - Scope:
    - Add controller routes + call(s) into `OpenSubsonicApiService`.
    - Resolve `id` → Song/Artist, then collect similar artists, then return a stable set of songs (respect `count`).
  - Acceptance criteria:
    - Endpoints return valid OpenSubsonic response models (and honor auth).
    - `count` behaves predictably; no duplicates; excludes the requested song.

- [x] Jukebox control endpoint behavior: return 410 Gone (not supported)
  - File: `src/Melodee.Blazor/Controllers/OpenSubsonic/JukeboxController.cs`
  - Endpoint: `jukeboxControl`
  - Decision:
    - Do not implement server-side playback; always return `410 Gone` consistently.
  - Notes:
    - Rationale captured in `prompts/ADR-LOG.md` (ADR-0001).

### Phase 3 — Blazor UI actions (remove `NotImplementedException` + complete flows)

> Upload/storage rules (decided):
> - User avatars are stored as **files on disk** under the path of the **single** `LibraryType.UserImages` library.
>   - There must be exactly one `Library` row with `Type == UserImages`; treat 0 or >1 as configuration error.
> - Add Setting `system.maxUploadSize` (bytes) default **5MB** and enforce it for all Blazor uploads.
> - When a user is deleted, delete any uploaded avatar (verified: already implemented in `UserService.DeleteAsync`).

- [ ] System: add `system.maxUploadSize` setting (default 5MB)
  - Files:
    - `src/Melodee.Common/Constants/SettingRegistry.cs` (add key)
    - `src/Melodee.Common/Data/MelodeeDbContext.cs` (seed default value)
  - Acceptance criteria:
    - Setting exists in DB after initialization/migrations.

- [ ] Blazor: enforce max upload size from `system.maxUploadSize`
  - Scope:
    - Replace usages of `MelodeeConfiguration.MaximumUploadFileSize` with config-driven value.
    - Affected files include (at least):
      - `src/Melodee.Blazor/Components/Pages/Data/UserEdit.razor` (avatar upload)
      - `src/Melodee.Blazor/Components/Components/ImageSearchUpload.razor`
      - `src/Melodee.Blazor/Components/Pages/Data/AlbumDetail.razor` (image + file upload)
      - `src/Melodee.Blazor/Components/Pages/Data/ArtistDetail.razor` (image upload)
  - Acceptance criteria:
    - Uploads larger than the limit fail predictably with a user-visible message.

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
  - Decision (decided):
    - Use a **dialog** that prompts the user to pick the destination library **every time** (do not persist a default).
      - Rationale: destination may vary based on rules/organization (e.g., artist name, library layout).
  - Scope:
    - When `LibraryService.GetStorageLibrariesAsync()` returns more than 1, show dialog to choose target library.
  - Acceptance criteria:
    - No throw when >1 storage library exists.
    - User must explicitly pick a destination each time.

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
    - Implement `CleanButtonClick()`
  - Clean semantics (decided):
    - Intended for **Admin** users.
    - **Filesystem is the authority**.
    - For each media file found on disk without a corresponding DB record:
      - Create DB records as needed (Artist, Album, Song, etc.).
    - For each DB record that no longer has a media file on disk:
      - Delete the song record.
      - If **all** songs for an album are missing => delete the entire album.
      - If **some** songs are missing => set `AlbumStatus = Invalid`.
  - Dry run (decided):
    - Provide a **dry-run** option that performs discovery only (no writes) and shows a **summary report** in a dialog (counts + representative examples) before executing the real clean.

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
  - Policy (decided):
    - Only **Admin** or **Editor** users may delete songs.
    - Perform a **cascade delete** of dependent records as needed.
    - Delete the song from the DB **and** delete the associated media file from disk (if present).
  - Scope:
    - Enforce role checks at the API/UI boundary (and/or service boundary if `UserInfo` is available).
    - Handle referential integrity (contributors, play history/now playing, user-song relations, caches).
    - Delete file safely (missing file should not hard-fail; log + continue).
  - Acceptance criteria:
    - All UI call-sites that delete songs succeed for Admin/Editor and are blocked for non-privileged users.
    - Add/adjust tests in `tests/Melodee.Tests.Common` (see SongServiceTests section below).

- [ ] ServiceBase: CRC hash discrepancy
  - File: `src/Melodee.Common/Services/ServiceBase.cs`
  - Ground truth (decided):
    - DB column `Song.FileHash` must **always** equal `CRC32.Calculate(...)` computed over the **entire file bytes** (read full stream).
  - Goal:
    - Identify why the computed CRC can differ from stored hash and make the system consistently compute/store/compare using `Song.FileHash` as above.
  - Acceptance criteria:
    - Add a focused regression test that validates `CRC32.Calculate(fileBytes)` equals `Song.FileHash` for a known test file.

- [ ] ArtistExtensions: implement OpenSubsonic `artistImageUrl` + `lastFmUrl`
  - File: `src/Melodee.Common/Data/Models/Extensions/ArtistExtensions.cs`
  - Decision (decided):
    - OpenSubsonic `artistImageUrl` must be a **directly-renderable image URL** served by Melodee (not an external homepage URL).
      - Use Melodee’s `/images/{artistApiKey}/{size}` style endpoint (or equivalent used elsewhere).
    - OpenSubsonic `ArtistInfo.lastFmUrl`:
      - If the artist DB field `LastFmId` is not blank, construct `https://www.last.fm/music/{LastFmId}`.
      - If `LastFmId` is blank, leave `lastFmUrl` blank.
  - Acceptance criteria:
    - `ToApiArtist(...)` no longer passes a placeholder string.
    - `GetArtistInfo(2)` returns `lastFmUrl` only when `LastFmId` is present.

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

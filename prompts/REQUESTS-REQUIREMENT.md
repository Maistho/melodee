## Overview

This document defines requirements for adding **Requests** to Melodee.
A *request* is created by an authenticated user to:

- Request a missing **album** or **song** be added to the library
- Request a correction to an existing **artist** or **album**

Typical flow:

1. A user searches in Melodee and cannot find what they want (no results, incomplete results, etc.).
2. The user clicks **Request** and submits as much detail as possible.
3. An administrator reviews the request, acquires the album/song, adds it to Melodee.
4. The request is marked **Completed** (manually or automatically when a strict match is detected).

## User flows

### Creating a request

- A **Request** call-to-action should be available from the search experience **at all times** (always visible), since the app cannot reliably know whether a user found what they were looking for.
- The request creation form is available on a dedicated **Request** page.

### Viewing and discussing a request

- The request detail view is visible to **all authenticated users**.
- The detail view includes a **comment thread** where all authenticated users can discuss the request.
  - Users can **reply** to an existing comment.
  - The UI must display comments in a **threaded** manner (parent comment + replies).

## Data model

### Request categories

A request must have a `category`:

- **AddAlbum** (request a new album be added)
- **AddSong** (request a new song be added)
- **ArtistCorrection** (report a problem with an existing artist)
- **AlbumCorrection** (report a problem with an existing album)
- **General** (library gap / unknown title)

### Request fields

- `category` **required** (see Request categories above)
- `description` (Markdown supported) **required**
  - Example descriptions:
    - "I heard this new song from Elton John with Dua Lipa last night on TV."
    - "That new song about baking cookies."
    - "You don't have all the albums for Elton John, you are missing a lot from the 80s."
- `artistName` (optional)
- `targetArtistApiKey` (optional; used for ArtistCorrection requests; Blazor route uses the artist ApiKey)
- `albumTitle` (optional; used for AddAlbum / AlbumCorrection requests)
- `targetAlbumApiKey` (optional; used for AlbumCorrection requests; Blazor route uses the album ApiKey)
- `songTitle` (optional; used for AddSong requests)
- `releaseYear` (optional)
- Optional helper fields (recommended for usability): `externalUrl` (Spotify/Bandcamp/YouTube), `notes`

Validation guidance:

- `description` is required.
- For **AddAlbum** requests, `albumTitle` is strongly recommended.
- For **AddSong** requests, `songTitle` is strongly recommended.
- For **ArtistCorrection** requests, `targetArtistApiKey` or `artistName` is strongly recommended.
- For **AlbumCorrection** requests, `targetAlbumApiKey` or `albumTitle` is strongly recommended.
- For **General** requests, structured fields may be empty.

### Request status

The request has a status.

Display labels vs API values:

- **Pending** (`Pending`) (default)
- **In Progress** (`InProgress`)
- **Completed** (`Completed`)
- **Rejected / Unable to Fulfill** (`Rejected`)

Allowed transitions (recommended):

- Pending → In Progress (admin)
- Pending/In Progress → Rejected (admin)
- Pending/In Progress/Rejected → Completed (admin or request creator)
- Completed → (no transitions allowed)

## Permissions

- Any **authenticated user** can:
  - Create requests
  - View all requests
  - Comment on requests
- The request **creator** can:
  - Edit their request
  - Delete their request
  - Mark their request as **Completed** at any time
- An **Administrator** can:
  - Edit/delete any request
  - Change request status (In Progress / Completed / Rejected)

Note: **Rejected / Unable to Fulfill** is admin-only.

## REST API requirements (Melodee API clients; non-admin)

These endpoints are the **client-facing** Melodee REST API requirements for Requests.
They intentionally **exclude admin-only operations** (e.g., reject/unreject, set In Progress, edit/delete others).

Important:

- The **Blazor UI (including administrators)** MUST NOT call the Melodee REST API for Requests (or anything else).
- The Blazor UI should use **direct in-process services via DI/IOC** (e.g., `RequestService`, `RequestCommentService`, `RequestActivityService`) rather than HTTP calls to `/api/...`.
- The REST API is intended for **external client applications**.

Conventions:

- Base route: `api/v{version}/...` (v1).
- Auth: JWT Bearer (`Authorization: Bearer <token>`), consistent with other Melodee controllers.
- Pagination: endpoints that return collections MUST return `{ meta, data }` where `meta` is `PaginationMetadata`.
- Status values in JSON and query params use the API values: `Pending`, `InProgress`, `Completed`, `Rejected`.

Minimum response fields (shape guidance):

- Requests should include at least: `apiKey`, `category`, `description`, `artistName`, `albumTitle`, `songTitle`, `releaseYear`, `status`, `createdAt`, `createdByUser`.
- Comments should include at least: `apiKey`, `requestApiKey`, `body`, `parentCommentApiKey`, `isSystem`, `createdAt`, `createdByUser`.

Note: Do not expose or use numeric PK `id` values in any URLs or UI navigation.

### Requests

- `GET /api/v1/requests?page={page}&pageSize={pageSize}&query={query?}&mine={true?}&status={Pending|InProgress|Completed|Rejected?}&artistApiKey={artistApiKey?}&albumApiKey={albumApiKey?}&songApiKey={songApiKey?}`
  - Any authenticated user.
  - Response shape: `{ meta, data }`.
  - By default, returns all requests (subject to authentication).
  - `mine=true` filters to requests created by the current user.
  - Entity filters:
    - `artistApiKey` filters to requests about the given artist (including `targetArtistApiKey` matches and name-based matches when possible).
    - `albumApiKey` filters to requests about the given album (including `targetAlbumApiKey` matches and title-based matches when possible).
    - `songApiKey` filters to requests about the given song (best-effort; usually only available when requests are created from a song detail page).
- `POST /api/v1/requests`
  - Any authenticated user.
  - Creates a request in `Pending` status.
- `GET /api/v1/requests/{requestApiKey}`
  - Any authenticated user.
- `PUT /api/v1/requests/{requestApiKey}`
  - Request creator only.
  - Allowed updates are limited to user-editable fields (e.g., `description`, `category`, `artistName`, `albumTitle`, `songTitle`, `releaseYear`, `externalUrl`, `notes`).
  - Must not allow non-admin clients to set admin-only fields/status transitions (e.g., `InProgress`, `Rejected`).
- `POST /api/v1/requests/{requestApiKey}/complete`
  - Request creator only; idempotent.
  - Marks the request as `Completed`.
- `DELETE /api/v1/requests/{requestApiKey}`
  - Request creator only.
  - Constraint: only allowed while `Pending`.

### Request comments

Comment behavior (minimum):

- Comments support **Markdown**.
- Comments support **replies** (threaded discussion).
  - Each comment may optionally have `parentCommentApiKey` referencing another comment.
  - If `parentCommentApiKey` is set, it must reference a comment on the same request.
- Top-level comments are returned in chronological order (oldest → newest).
- Within a thread, replies are returned in chronological order (oldest → newest).
- API responses must include `parentCommentApiKey` so UIs can render threads.
- System comments are immutable and visually distinguished from user comments.

Endpoints:

- `GET /api/v1/requests/{requestApiKey}/comments?page={page}&pageSize={pageSize}`
  - Any authenticated user.
  - Response shape: `{ meta, data }`.
- `POST /api/v1/requests/{requestApiKey}/comments`
  - Any authenticated user.
  - Adds a comment to the request.
  - Supports replies via optional `parentCommentApiKey`.

Error/permission expectations:

- `401` for missing/invalid token.
- `403` when authenticated but not permitted (e.g., editing someone else's request).
- `404` when `{requestApiKey}` is not found.

## Pages and UI requirements

### Search results

- Provide a **Request** action that is **always visible** from search results (regardless of whether results are shown).

### Artist and album detail pages

- On the artist detail view (`/data/artist/{artistApiKey}`):
  - Provide a **Request Change** action that opens the Request creation form with:
    - `category = ArtistCorrection`
    - `targetArtistApiKey` pre-filled
    - `artistName` pre-filled
    - `fromUrl`/back navigation set to the current page
  - Do **not** show a "View Requests" button.
  - Add a **Requests** `RadzenTreeItem` immediately after **Relationships**.
    - When selected, show a collection of request cards filtered to `artistApiKey={artistApiKey}`.
    - When the user clicks a request card, navigate to the request detail page.
- On the album detail view (`/data/album/{albumApiKey}`):
  - Provide a **Request Change** action that opens the Request creation form with:
    - `category = AlbumCorrection`
    - `targetAlbumApiKey` pre-filled
    - `albumTitle` and `artistName` pre-filled (when available)
    - `fromUrl`/back navigation set to the current page
  - Do **not** show a "View Requests" button.
  - Add a **Requests** `RadzenTreeItem` immediately after **Images**.
    - When selected, show a collection of request cards filtered to `albumApiKey={albumApiKey}`.
    - When the user clicks a request card, navigate to the request detail page.

### Requests index page

- Add a navbar item: **Requests**.
- The index page shows a paged list of requests with:
  - Search box (filters by text across title/artist/description)
  - Pagination
  - Filter to show only "My Requests"
  - Status filter (Pending/In Progress/Completed/Rejected)
  - Entity filters:
    - Artist filter (by artist ApiKey)
    - Album filter (by album ApiKey)
    - Song filter (by song ApiKey)
  - Default sort order: newest first
    - Optionally allow sorting by artist/album title
    - Optionally allow sorting by request creation date
    - Optionally allow sorting by request status

### Request detail page

- Show request metadata (category, artist/title fields, year, created by, created date, status).
- Provide actions:
  - **Back to Search** (returns the user to the prior search page, ideally preserving the query)
  - **Edit** (per permissions above)
  - **Delete** (per permissions above)
  - **Mark as In Progress** (admin)
  - **Mark as Complete** (admin or request creator)
  - **Reject / Unable to Fulfill** (admin)
- Provide the comment list + add comment form (all authenticated users).

## Notifications / activity indicator

Melodee should provide an in-application notification mechanism for request activity.

### Navbar activity badge (dot)

- Show a small activity indicator (dot) on the **Requests** navbar item when the current user has unread activity on one or more **in-scope** requests.
- Navbar notifications apply to requests where the user is the request creator **or** they have commented on the request.
- The indicator is intentionally not a numeric count (users may have many requests).

### What counts as activity

Request `lastActivityAt` should be updated when any of the following occur:

- New user comment
- New system comment
- Status change (Pending/In Progress/Completed/Rejected)
- Reopen (if implemented)

The current user’s own actions must not cause their indicator to light up.

### Read/unread tracking model

To compute unread activity efficiently:

- Store on the request:
  - `lastActivityAt`
  - `lastActivityUserId` (nullable for system activity)
- Store per user + request:
  - `lastSeenAt`

Unread evaluation (conceptual):

- A request has unread activity for a user when:
  - `lastActivityAt > lastSeenAt`, AND
  - `lastActivityUserId != currentUserId`.

Scopes:

- Navbar activity indicator considers requests where the user is the request creator **or** they have commented.
- The dashboard "Request Activity" section considers only requests authored by the current user.

Implementation detail:

- A user is considered a commenter if they have authored at least one comment on the request.
  This can be derived from the comments table or maintained as a denormalized participant flag for performance.

### When to update lastSeenAt

- When a user opens a request detail page, set `lastSeenAt = now` for that request.
- When a user marks a request as In Progress or Completed, set `lastSeenAt = now` for that request.
- When a user views the Requests index page, mark visible requests as seen.

### Dashboard component: Request Activity

Add a dashboard component/section titled **Request Activity**.

Visibility:

- Only show this dashboard section when the current user has at least one **authored request** with unread activity.

What to include:

- Show a small card/list item per request with unread activity.
- Only include requests authored by the current user.

Card fields (minimum):

- Request category + summary text
  - AddAlbum: `artistName` + " - " + `albumTitle`
  - AddSong: `artistName` + " - " + `songTitle`
  - Otherwise: first line of `description`
- Current status
- Last activity timestamp and actor (system/admin/username)
- Short activity hint (e.g., “New comment”, “Status changed to In Progress”)
- Link/button to open the request detail page (which marks it as seen)

Sorting/paging guidance:

- Sort by most recent activity first.
- Show up to N items (e.g., 10) with an option to navigate to the Requests index.

### API support (non-admin)

Note: These endpoints are for **non-admin external clients**. The Blazor UI MUST use services directly via DI/IOC and must not call these endpoints.

Endpoints:

- `GET /api/v1/requests/activity`
  - Returns whether the user has unread request activity.
  - Suggested response: `{ "hasUnread": true }`
- `GET /api/v1/requests/activity/authored?page={page}&pageSize={pageSize}`
  - Returns the current user's authored requests that have unread activity.
  - Response shape: `{ meta, data }` (sorted by most recent activity).
- `POST /api/v1/requests/{requestApiKey}/seen`
  - Marks the request as seen for the current user (sets `lastSeenAt = now`).
  - Should only be allowed when the request is in-scope for the user (creator or commenter).

## Auto-completion / matching behavior

When new media is added to Melodee, open requests can be checked for strict matches and auto-completed.

- On new **albums** added:
  - If request category is **AddAlbum**, and the request has a complete set of match fields, mark as Completed when there is a 100% match on:
    - `artistName`, `albumTitle`, and (if provided) `releaseYear`
- On new **songs** added:
  - If request category is **AddSong**, and the request has a complete set of match fields, mark as Completed when there is a 100% match on:
    - `artistName`, `songTitle`, and (if provided) `releaseYear`

When a request is marked as **Completed**, Melodee will add a **system comment** linking to the relevant entity when possible.

Link format (Blazor UI):

- Artist: `/data/artist/{artistApiKey}`
- Album: `/data/album/{albumApiKey}`
- Song: `/data/song/{songApiKey}`

Which entity is linked:

- **AddAlbum** (auto-completed): link to the matched album (and optionally its artist).
- **AddSong** (auto-completed): link to the matched song (and optionally its album/artist).
- **ArtistCorrection**: link to `targetArtistApiKey` when provided.
- **AlbumCorrection**: link to `targetAlbumApiKey` when provided.

Normalization rules must be defined to make “100% match” unambiguous (recommended):

- Case-insensitive compare
- Trim whitespace
- Normalize punctuation/diacritics consistently
- If `releaseYear` is not provided on the request, do not require it for matching
- The trigger point for the check will be on events:
    - New artist added
    - New album added
    - New song added

## Testing requirements

- Add high coverage **unit tests** for Requests, including both happy path and edge cases.
- Tests should cover:
  - Request creation/update/delete permission constraints
  - Status transitions (including invalid transitions)
  - Comment creation including reply/thread behavior (`parentCommentApiKey` validation)
  - Activity tracking semantics (unread/seen rules, last-activity updates)
  - Auto-completion strict matching and normalization rules

## Documentation updates (post-implementation)

After implementing this feature:

- Update the repository root `README.md` to mention the Requests feature and where to find it in the UI/API.
- Update and/or create user documentation in the Docsy site (`docs/`) covering:
  - How to create/view/manage requests in the UI
  - What statuses mean (including Rejected)
  - How request activity notifications work (navbar dot + dashboard section)
  - Melodee API client endpoints for Requests

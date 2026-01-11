# Melodee Party Mode & Jukebox Manual Test Walkthrough

This guide outlines the steps to manually verify the Party Mode and Jukebox functionality, ensuring recent fixes (SignalR, Security, Rate Limiting) are working as expected.

## Prerequisites

1.  **Configuration**: Ensure `appsettings.json` (or your environment variables) has Jukebox enabled if testing Jukebox specific features.
    ```json
    "Jukebox": {
      "Enabled": true,
      "BackendType": "Mpd" // or any configured backend
    }
    ```
2.  **Users**: You will need at least two distinct users (e.g., `admin` and `user1`, or use Incognito mode for the second user).

---

## Part 1: Party Mode (User Session)

### 1. Create a Party Session (Host)
1.  Log in as **User A** (Host).
2.  Navigate to the **Party Mode** dashboard (usually via the main menu or `/party`).
3.  Click **Create Session**.
4.  Enter a session name (e.g., "Friday Vibes").
5.  **Verify**:
    *   You are redirected to the Party Session view.
    *   Status shows "Active".
    *   You are listed as "Owner".

### 2. Join a Session (Guest & Security Check)
1.  Open a new browser window (Incognito) and log in as **User B**.
2.  Navigate to **Party Mode**.
3.  Click **Join Session**.
4.  Enter the **Session Code** from User A's screen.
5.  **Verify**:
    *   User B joins successfully.
    *   **SignalR Check**: User A's screen immediately updates the "Participants" list *without* a page refresh.

### 3. Queue Synchronization (SignalR)
1.  **User A**: Browse the library and "Add to Queue".
2.  **Verify**:
    *   Item appears in User A's queue.
    *   Item appears in User B's queue *instantly*.
3.  **User B**: Add a different song to the queue.
4.  **Verify**:
    *   Both screens show 2 songs in the correct order.

### 4. Playback Control & Role Security
1.  **User A (Host)**: Click **Play**.
    *   **Verify**: Playback starts. User B's "Now Playing" bar updates to show the song playing (might take 1-2s for heartbeat sync).
2.  **User A**: Click **Skip**.
    *   **Verify**: Skips to the next track.
3.  **User B (Listener)**: Attempt to click **Skip** or **Pause**.
    *   **Expectation**: The action should fail (UI might hide buttons, or show an error toast "Listeners cannot control playback"). *If buttons are visible and you click them, check the Network tab for a 403 Forbidden response.*

### 5. Modifying Rights (Optional)
1.  **User A**: Promote User B to **DJ**.
2.  **User B**: Click **Skip**.
    *   **Verify**: Action now succeeds.
3.  **User B**: Try to Skip again immediately (within 10 seconds).
    *   **Verify**: Action fails or is ignored (Rate Limit/Cooldown active).

---

## Part 2: Jukebox Mode (Subsonic API)

This tests the "Server-side Jukebox" functionality typically used by Subsonic clients (like DSub, iSub, or the Web UI acting as Jukebox controller).

### 1. Status Check
*   **Tool**: Browser or `curl`.
*   **Request**:
    ```
    GET /rest/jukeboxControl.view?u=admin&p=admin&v=1.16.1&c=test&f=json&action=status
    ```
*   **Verify**:
    *   Returns HTTP 200.
    *   JSON contains `jukeboxStatus` with `currentIndex`, `playing`, etc.
    *   *Self-healing Check*: If this is the fiirst run, verify the "Subsonic Jukebox" session was automatically created in the database.

### 2. Metadata Popluation
*   **Request**: `action=get` (Get Playlist).
*   **Verify**:
    *   If items exist, the response contains valid `title`, `artist`, `album` fields (not "Unknown Artist").

### 3. Add & Skip
1.  **Add**: `action=add&id={ValidSongId}`.
    *   Verify `status` shows updated playlist size.
2.  **Start**: `action=start`.
    *   Verify `playing: true`.
3.  **Skip**: `action=skip&index=0`.
    *   Verify `currentIndex` changes or song changes.
4.  **Rate Limit**: Call `action=skip` twice rapidly.
    *   Verify the second call returns an error or status indicating conflict/cooldown (depending on client handling, API returns 409).

---

## Validation Checklist

- [ ] **Crash Test**: Jukebox service starts without crashing (Fixed Guid).
- [ ] **Real-time**: Queue updates appear on all clients without refresh.
- [ ] **Security**: Listeners cannot Skip/Pause.
- [ ] **Data**: Jukebox playlist shows real Song/Artist names.

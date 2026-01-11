# Melodee Party Mode & Jukebox Manual Test Walkthrough

This guide outlines the steps to manually verify the Party Mode and Jukebox functionality, ensuring recent fixes (SignalR, Security, Rate Limiting) are working as expected.

## Prerequisites

1.  **Configuration**: Jukebox settings are now stored in the database via the Settings service. Configure them through the admin UI or directly in the database:
    
    | Setting Key | Description | Example Value |
    |-------------|-------------|---------------|
    | `jukebox.enabled` | Enable/disable Jukebox | `true` |
    | `jukebox.backendType` | Backend type (`mpv` or `mpd`) | `mpv` |
    
    **For MPV backend**, also configure:
    | Setting Key | Description | Example Value |
    |-------------|-------------|---------------|
    | `mpv.path` | Path to MPV executable | `/usr/bin/mpv` |
    | `mpv.audioDevice` | Audio output device | `auto` |
    | `mpv.socketPath` | IPC socket path (optional) | `/tmp/mpv-socket` |
    | `mpv.initialVolume` | Initial volume (0.0-1.0) | `0.8` |
    
    **For MPD backend**, configure:
    | Setting Key | Description | Example Value |
    |-------------|-------------|---------------|
    | `mpd.host` | MPD server hostname | `localhost` |
    | `mpd.port` | MPD server port | `6600` |
    | `mpd.password` | MPD password (if required) | `` |
    
2.  **Users**: You will need at least two distinct users (e.g., `admin` and `user1`, or use Incognito mode for the second user).

3.  **For MPV testing**: Ensure MPV is installed on the server:
    ```bash
    # Ubuntu/Debian
    sudo apt install mpv
    
    # macOS
    brew install mpv
    ```

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

### 1. Jukebox UI Page
1.  Log in as an **Admin** user.
2.  Navigate to `/jukebox` in the Blazor application.
3.  **Verify**:
    *   If Jukebox is disabled, a "Jukebox is not enabled" message appears with configuration hint.
    *   If enabled, the status card shows connection status and backend info.
    *   Playback controls (Play, Pause, Stop, Skip) are visible only to admin users.
    *   Non-admin users see a message that controls are admin-only.

### 2. Status Check (API)
*   **Tool**: Browser or `curl`.
*   **Request**:
    ```
    GET /rest/jukeboxControl.view?u=admin&p=admin&v=1.16.1&c=test&f=json&action=status
    ```
*   **Verify**:
    *   Returns HTTP 200.
    *   JSON contains `jukeboxStatus` with `currentIndex`, `playing`, etc.
    *   *Self-healing Check*: If this is the first run, verify the "Subsonic Jukebox" session was automatically created in the database.

### 3. Metadata Population
*   **Request**: `action=get` (Get Playlist).
*   **Verify**:
    *   If items exist, the response contains valid `title`, `artist`, `album` fields (not "Unknown Artist").

### 4. Add & Skip
1.  **Add**: `action=add&id={ValidSongApiKey}`.
    *   Verify `status` shows updated playlist size.
2.  **Start**: `action=start`.
    *   Verify `playing: true`.
3.  **Skip**: `action=skip&index=0`.
    *   Verify `currentIndex` changes or song changes.
4.  **Rate Limit**: Call `action=skip` twice rapidly.
    *   Verify the second call returns an error or status indicating conflict/cooldown (depending on client handling, API returns 409).

---

## Part 3: MPV Backend Testing

### 1. Backend Initialization
1.  Ensure `jukebox.enabled` = `true` and `jukebox.backendType` = `mpv` in settings.
2.  Navigate to `/jukebox` in the Blazor UI.
3.  **Verify**:
    *   Status shows "Connected" (green badge).
    *   Backend info displays MPV version (e.g., "mpv 0.35.1").

### 2. File Playback
1.  Add a song to the Jukebox queue via the Subsonic API.
2.  Call `action=start`.
3.  **Verify**:
    *   Audio plays through the server's audio output.
    *   Position updates in the status response.
    *   Volume control works via `action=setGain&gain=0.5`.

### 3. IPC Communication
1.  Check the MPV socket path (default: `/tmp/mpv-melodee-{guid}.sock`).
2.  **Verify**: Socket file exists while MPV is running.
3.  Stop playback via `action=stop`.
4.  **Verify**: MPV process remains idle but connected.

---

## Validation Checklist

- [ ] **Crash Test**: Jukebox service starts without crashing (Fixed Guid session).
- [ ] **Real-time**: Queue updates appear on all clients without refresh.
- [ ] **Security**: Listeners cannot Skip/Pause in Party Mode.
- [ ] **Admin Controls**: Jukebox UI controls only visible to admins.
- [ ] **Data**: Jukebox playlist shows real Song/Artist names.
- [ ] **Settings**: All Jukebox/MPV/MPD settings are read from database (not appsettings.json).
- [ ] **MPV IPC**: MPV backend connects via Unix socket and responds to commands.
- [ ] **File Resolution**: `PlaySongAsync` correctly resolves file paths from SongApiKey.

---

## Troubleshooting

### Jukebox shows "Not Enabled"
- Check `jukebox.enabled` setting in the database Settings table.
- Verify the setting value is `true` (string).

### MPV Backend shows "Disconnected"
- Verify MPV is installed: `mpv --version`
- Check `mpv.path` setting points to correct executable.
- Check server logs for IPC connection errors.
- Ensure the socket path is writable.

### No Audio Output
- Verify `mpv.audioDevice` setting (use `auto` for default).
- Test MPV directly: `mpv /path/to/test.mp3`
- Check server audio permissions.

### "Song not found" errors
- Verify the SongApiKey exists in the database.
- Check that the song file exists at the resolved path:
  `{Library.Path}/{Artist.Directory}/{Album.Directory}/{Song.FileName}`

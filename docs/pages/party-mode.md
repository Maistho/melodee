---
title: Party Mode
permalink: /party-mode/
---

# Party Mode

Party Mode enables collaborative music listening sessions where multiple users can contribute to and control a shared playlist. It's perfect for parties, gatherings, or any time you want to share music control with friends and family.

## Overview

Party Mode provides:

- **Shared Queues**: Multiple users can add songs to a common playlist
- **Real-Time Sync**: All participants see queue changes instantly
- **Flexible Playback**: Route audio to any participant's device or the server's Jukebox
- **Access Control**: Session owners can moderate participants and lock queues
- **Mobile-Friendly**: Full functionality on phones and tablets

## Concepts

### Sessions

A Party Mode session is a collaborative listening space with:

- **Unique URL**: Each session has a shareable link
- **Owner**: The user who created the session (has full control)
- **Participants**: Users who have joined the session
- **Queue**: The ordered list of songs to play
- **Active Endpoint**: The device or backend playing music

### Participants

Users in a session have different roles:

| Role | Description | Capabilities |
|------|-------------|--------------|
| **Owner** | Session creator | Full control over session, queue, and participants |
| **DJ** | Promoted participant | Can modify queue and control playback |
| **Guest** | Regular participant | Can add songs to queue, view queue |

### Endpoints

An endpoint is where the music plays. Party Mode supports:

| Endpoint Type | Description |
|---------------|-------------|
| **Browser** | Audio plays in the participant's web browser |
| **Server Jukebox** | Audio plays through the server's audio output (requires [Jukebox](/jukebox/)) |
| **External** | Reserved for future integrations |

## Getting Started

### Creating a Session

1. Navigate to any album, artist, or playlist in Melodee
2. Click the **Party Mode** button (🎉) or select **Start Party** from the menu
3. Configure session options:
   - **Session Name**: A friendly name for your party
   - **Privacy**: Public (discoverable) or private (invite-only)
   - **Queue Lock**: Whether only DJs/owner can modify the queue
4. Click **Create Session**
5. Share the session URL with participants

### Joining a Session

**Via Direct Link:**
1. Open the party session URL shared by the owner
2. Sign in if prompted (or join as guest if allowed)
3. You're now a participant!

**Via Session Browser:**
1. Navigate to **Party Mode** in the main menu
2. Browse active public sessions
3. Click **Join** on any session

### Adding Songs

1. Browse artists, albums, or use search within the party session
2. Click the **Add to Queue** button on any song
3. The song appears in the shared queue immediately

**Bulk Add:**
- Add entire albums with **Add Album to Queue**
- Add artist's top songs with **Add Popular**
- Add from playlists with **Add Playlist**

## Session Controls

### For Session Owners

As the session owner, you have full control:

#### Queue Management
- **Reorder**: Drag songs to change play order
- **Remove**: Delete any song from the queue
- **Clear**: Remove all songs from the queue
- **Lock/Unlock**: Control who can modify the queue

#### Participant Management
- **Promote to DJ**: Give a participant DJ privileges
- **Demote**: Remove DJ privileges from a participant
- **Kick**: Remove a participant from the session
- **Ban**: Permanently block a user from the session

#### Playback Control
- **Select Endpoint**: Choose where music plays (browser or Jukebox)
- **Play/Pause**: Control playback
- **Skip**: Move to the next song
- **Seek**: Jump to a specific position in the current song
- **Volume**: Adjust playback volume (endpoint-dependent)

#### Session Settings
- **Rename**: Change the session name
- **Privacy**: Switch between public and private
- **End Session**: Terminate the session for all participants

### For Participants

Regular participants can:

- View the current queue
- Add songs to the queue (when unlocked)
- See what's currently playing
- Leave the session at any time

DJs additionally can:

- Reorder and remove songs
- Control playback
- Skip to next song

## Playback Endpoints

### Browser Endpoint

Play audio directly in your web browser:

**Advantages:**
- No additional setup required
- Works on any device with a browser
- Individual volume control
- Headphone friendly

**Limitations:**
- Audio plays on one device only
- Browser tab must remain open
- Mobile devices may interrupt background audio

**To use browser endpoint:**
1. Click the **Endpoint** button in the party player
2. Select **This Browser**
3. Audio begins playing in your browser

### Server Jukebox Endpoint

Route audio through the server's audio output:

**Advantages:**
- Centralized playback for all participants
- Consistent audio quality
- Works with home audio systems
- No client-side processing

**Requirements:**
- [Jukebox feature](/jukebox/) must be enabled
- Server must have audio output configured
- Admin must have configured the backend (MPV or MPD)

**To use Jukebox endpoint:**
1. Click the **Endpoint** button in the party player
2. Select **Server Jukebox** (only available if enabled)
3. Audio plays through the server's speakers

## Real-Time Features

Party Mode uses WebSocket connections for real-time updates:

- **Queue Changes**: See songs added/removed instantly
- **Playback State**: Current song and position sync across all participants
- **Participant Updates**: See who joins or leaves
- **Chat Messages**: (Future feature) Communicate with other participants

### Connection Status

The party interface shows connection status:

| Status | Meaning |
|--------|---------|
| 🟢 Connected | Real-time updates active |
| 🟡 Reconnecting | Temporarily disconnected, attempting reconnect |
| 🔴 Offline | Not connected; refresh to reconnect |

## Permissions Matrix

| Action | Owner | DJ | Guest |
|--------|-------|-----|-------|
| View queue | ✅ | ✅ | ✅ |
| Add songs | ✅ | ✅ | ✅* |
| Remove songs | ✅ | ✅ | ❌ |
| Reorder queue | ✅ | ✅ | ❌ |
| Control playback | ✅ | ✅ | ❌ |
| Manage participants | ✅ | ❌ | ❌ |
| Change settings | ✅ | ❌ | ❌ |
| End session | ✅ | ❌ | ❌ |

\* When queue is unlocked

## Use Cases

### House Party

1. Set up speakers connected to your Melodee server
2. Enable [Jukebox](/jukebox/) with MPV backend
3. Create a Party Mode session
4. Share the QR code or URL with guests
5. Guests add songs from their phones
6. Music plays through your speakers

### Family Road Trip

1. Connect a phone/tablet to the car's audio system
2. Create a Party Mode session with browser endpoint
3. Everyone in the car joins the session
4. Take turns adding songs to the queue
5. The driver's device plays the music

### Remote Listening Party

1. Create a private Party Mode session
2. Share the link with remote friends
3. Each person uses their own browser endpoint
4. Everyone queues songs and listens simultaneously
5. Coordinate via video chat for reactions

## Best Practices

### For Hosts

1. **Test Before the Event**: Ensure Jukebox works and audio levels are appropriate
2. **Create a Starter Queue**: Pre-populate with some songs to set the mood
3. **Consider Queue Locking**: Lock the queue if you want curated playlists
4. **Promote Trusted DJs**: Give DJ access to friends who will help manage
5. **Monitor the Queue**: Watch for inappropriate additions
6. **Have a Backup Plan**: Download key playlists in case of network issues

### For Participants

1. **Respect the Queue**: Don't flood with many songs at once
2. **Match the Mood**: Add songs that fit the event's vibe
3. **Check What's Queued**: Avoid adding duplicates
4. **Use the Search**: Find specific songs rather than browsing endlessly
5. **Ask Before Skipping**: Communicate with the host about skips

## Troubleshooting

### Can't Join Session

1. **Check URL**: Ensure you have the correct session link
2. **Session Active**: The session may have ended
3. **Sign In**: Some sessions require authentication
4. **Permissions**: You may have been banned from the session

### No Audio Playing

1. **Check Endpoint**: Ensure the correct playback endpoint is selected
2. **Browser Permissions**: Allow audio playback in your browser
3. **Volume**: Check browser volume, system volume, and Jukebox volume
4. **Tab Focus**: Some browsers pause audio in background tabs

### Songs Not Adding

1. **Queue Locked**: The owner may have locked the queue
2. **Rate Limit**: Too many additions too quickly
3. **Connection Lost**: Check your connection status

### Out of Sync

1. **Refresh**: Reload the page to resync
2. **Check Connection**: Verify WebSocket connection is active
3. **Clear Cache**: Clear browser cache and rejoin

### Jukebox Not Available

1. **Feature Disabled**: An admin must enable Jukebox
2. **Backend Error**: The MPV/MPD backend may not be running
3. **Permissions**: Only the owner can select Jukebox as endpoint

## API Reference

### Session Management

```
# Create session
POST /api/v1/party/sessions
Body: { "name": "My Party", "isPublic": true }

# Get session
GET /api/v1/party/sessions/{apiKey}

# Update session
PATCH /api/v1/party/sessions/{apiKey}
Body: { "name": "New Name", "isQueueLocked": true }

# Delete session
DELETE /api/v1/party/sessions/{apiKey}

# List public sessions
GET /api/v1/party/sessions?publicOnly=true
```

### Participants

```
# Join session
POST /api/v1/party/sessions/{apiKey}/join

# Leave session
POST /api/v1/party/sessions/{apiKey}/leave

# Get participants
GET /api/v1/party/sessions/{apiKey}/participants

# Update participant role
PATCH /api/v1/party/sessions/{apiKey}/participants/{userId}
Body: { "role": "DJ" }

# Kick participant
DELETE /api/v1/party/sessions/{apiKey}/participants/{userId}
```

### Queue

```
# Get queue
GET /api/v1/party/sessions/{apiKey}/queue

# Add to queue
POST /api/v1/party/sessions/{apiKey}/queue
Body: { "songId": 12345 }

# Remove from queue
DELETE /api/v1/party/sessions/{apiKey}/queue/{itemApiKey}

# Reorder queue
POST /api/v1/party/sessions/{apiKey}/queue/reorder
Body: { "itemApiKey": "...", "newIndex": 5 }

# Clear queue
DELETE /api/v1/party/sessions/{apiKey}/queue
```

### Playback

```
# Get playback state
GET /api/v1/party/sessions/{apiKey}/playback

# Update playback
POST /api/v1/party/sessions/{apiKey}/playback
Body: { "action": "play" | "pause" | "stop" | "next" | "previous" }

# Seek
POST /api/v1/party/sessions/{apiKey}/playback/seek
Body: { "positionSeconds": 120 }

# Set volume
POST /api/v1/party/sessions/{apiKey}/playback/volume
Body: { "volume": 0.75 }
```

### Endpoints

```
# Get available endpoints
GET /api/v1/party/sessions/{apiKey}/endpoints

# Set active endpoint
POST /api/v1/party/sessions/{apiKey}/endpoints/{endpointApiKey}/activate

# Detach endpoint
POST /api/v1/party/sessions/{apiKey}/endpoints/{endpointApiKey}/detach
```

---

Have questions about Party Mode? Open an issue on GitHub or check the [API documentation](/api/) for technical details.

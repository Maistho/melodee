---
title: Scrobbling
permalink: /scrobbling/
---

# Scrobbling

Scrobbling is the process of tracking and recording what music you listen to. When you play a song, your music client sends information about that playback to the server, enabling features like "Now Playing" displays, listening history, play counts, and integration with external services like Last.fm.

## How Scrobbling Works

Scrobbling in Melodee follows the OpenSubsonic/Subsonic protocol standard and consists of two types of events:

### Now Playing

When you start playing a song, your client sends a "Now Playing" notification to the server. This:

- Marks the song as currently being played by you
- Updates the "Now Playing" page in the Melodee UI
- Allows other users to see what you're listening to (if enabled)

### Played (Submission)

When you finish listening to a song (typically after listening to at least 50% or 4 minutes), your client sends a "Played" submission. This:

- Records the play in your listening history
- Increments the song's play count
- Clears the "Now Playing" status
- Can trigger scrobbles to external services (Last.fm, Libre.fm)

## Why Use Scrobbling?

Scrobbling provides several benefits:

| Feature | Description |
|---------|-------------|
| **Listening History** | Track what you've listened to over time |
| **Play Counts** | See which songs and albums you play most |
| **Now Playing** | Share what you're currently listening to |
| **Statistics** | Generate insights about your listening habits |
| **Recommendations** | Enable personalized music recommendations |
| **External Integration** | Sync plays to Last.fm, Libre.fm, and other services |

## Enabling Scrobbling in Your Client

Most Subsonic-compatible music clients support scrobbling, but it's often disabled by default. Here's how to enable it in popular clients:

### Symfonium (Android)

1. Open **Settings**
2. Navigate to **Playback**
3. Enable **"Scrobble to server"**

### DSub (Android)

1. Open **Settings**
2. Go to **Playback**
3. Enable **"Scrobble"**

### Sonixd (Desktop)

1. Open **Settings**
2. Find the scrobbling section
3. Enable **"Enable scrobbling"**

### Sublime Music (Desktop)

1. Open **Preferences**
2. Enable **"Submit plays to server"**

### Ultrasonic (Android)

1. Open **Settings**
2. Navigate to **Music & Playback**
3. Enable **"Scrobble plays"**

### play:Sub (iOS)

1. Open **Settings**
2. Enable **"Scrobbling"**

### Substreamer (Multi-platform)

1. Open **Settings**
2. Find **Server settings**
3. Enable **"Scrobble plays to server"**

## Troubleshooting

### "Now Playing" Page is Empty

If the "Now Playing" page doesn't show your currently playing song:

1. **Check client settings**: Ensure scrobbling is enabled in your music client
2. **Verify server connection**: Make sure your client can communicate with Melodee
3. **Test with a different client**: Try a client known to support scrobbling (like Symfonium or DSub)

### Play Counts Not Updating

If your play counts aren't incrementing:

1. **Listen long enough**: Most clients require 50% of the song or 4 minutes before submitting
2. **Check scrobble settings**: Some clients have separate settings for "now playing" vs "submission"
3. **Review server logs**: Check Melodee logs for scrobble requests

### External Scrobbling (Last.fm)

To scrobble to external services like Last.fm:

1. Configure your Last.fm credentials in Melodee settings
2. Enable external scrobbling in the configuration
3. Plays will be forwarded to Last.fm when submitted

## API Details

For developers building clients, scrobbling uses the following endpoints:

### OpenSubsonic API

```
GET/POST /rest/scrobble
Parameters:
  - id: Song ID (required)
  - submission: true for played, false for now playing (default: true)
  - time: Timestamp of playback (optional, milliseconds since epoch)
```

### Native Melodee API

```
POST /api/v1/Scrobble
Body:
{
  "songId": "<guid>",
  "scrobbleType": "NowPlaying" | "Played",
  "playedDuration": <seconds>,
  "playerName": "<client name>"
}
```

## Best Practices

- **Enable scrobbling** in your preferred client for the best Melodee experience
- **Keep clients updated** to ensure proper scrobbling protocol support
- **Check "Now Playing"** in the Melodee UI to verify scrobbling is working
- **Review your history** periodically to ensure plays are being recorded

## Privacy Considerations

- Scrobble data is stored in your Melodee instance
- "Now Playing" visibility can be controlled by user permissions
- External scrobbling (Last.fm) is optional and requires explicit configuration
- All scrobble data can be exported or deleted per user

---

Have questions about scrobbling? Open an issue on GitHub or check the [API documentation](/api/) for technical details.

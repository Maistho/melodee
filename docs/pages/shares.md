---
title: Shares
permalink: /shares/
---

# Shares

Shares in Melodee allow you to create shareable links to your music that can be accessed by anyone—even people without a Melodee account. Share a favorite song with a friend, send an album link to someone, or create a playlist link for a party.

## What Are Shares?

A share is a unique, short URL that provides access to a specific piece of content in your library:

- **Songs**: Share a single track
- **Albums**: Share an entire album with all its songs
- **Artists**: Share an artist's profile and discography
- **Playlists**: Share a curated playlist

Shares work without requiring the recipient to log in or have an account on your Melodee server.

## How Shares Work

1. **Create a share** for any song, album, artist, or playlist
2. **Get a unique URL** with a short, shareable ID (e.g., `/share/abc123xyz`)
3. **Send the link** to anyone you want to share with
4. **Recipients access** the content through a public page—no login required

## Share Features

| Feature | Description |
|---------|-------------|
| **Unique URLs** | Each share has a short, unique ID for easy sharing |
| **Expiration** | Optionally set an expiration date after which the share becomes invalid |
| **Download Option** | Allow or disallow downloads of shared content |
| **Visit Tracking** | See how many times your share has been accessed |
| **Descriptions** | Add custom descriptions to explain what you're sharing |

## Creating Shares

### From the Melodee UI

1. Navigate to any song, album, artist, or playlist
2. Click the **Share** button or icon
3. Configure share options:
   - **Description**: Optional note about what you're sharing
   - **Expiration**: When the share should expire (or never)
   - **Downloadable**: Whether recipients can download the content
4. Copy the generated share link

### From Music Clients

Some Subsonic-compatible clients support creating shares:

- Look for a "Share" or "Create Link" option in song/album context menus
- The client will create the share and provide you with the URL

## Share Options

### Expiration

Control how long a share remains valid:

| Option | Use Case |
|--------|----------|
| **No expiration** | Permanent links for friends and family |
| **1 day** | Quick share for immediate listening |
| **1 week** | Short-term sharing |
| **1 month** | Medium-term access |
| **Custom date** | Specific expiration needs |

Expired shares return a "Share has expired" message to visitors.

### Download Permission

Control whether recipients can download the shared content:

- **Downloadable**: Recipients can download files (songs, albums)
- **Stream only**: Recipients can only stream, not download

Consider your bandwidth and storage when enabling downloads for shares.

## Accessing Shared Content

### For Recipients

When someone receives a share link:

1. Click the link to open the share page
2. View information about the shared content (cover art, track listing, etc.)
3. Play the content directly in their browser
4. Download if permitted by the share creator

No account or login required.

### Public Share Page

The public share page displays:

- **Cover art** or artist image
- **Title** and description
- **Track listing** (for albums and playlists)
- **Play controls** for streaming
- **Download button** (if enabled)

## Managing Shares

### Viewing Your Shares

1. Navigate to **Shares** in the Melodee UI
2. See all shares you've created
3. View statistics (visit count, last visited, expiration status)

### Editing Shares

Update existing shares:

- Change the description
- Modify expiration date
- Toggle download permission

Note: The share URL remains the same when editing.

### Deleting Shares

Remove shares you no longer want active:

1. Select the share(s) to delete
2. Confirm deletion
3. The share URL immediately becomes invalid

Deleted shares cannot be recovered.

## Share Types

### Song Shares

Share a single track:

- Displays song title, artist, and album
- Shows cover art
- Provides a play button for streaming
- Optional download of the single track

### Album Shares

Share an entire album:

- Displays album cover, title, and artist
- Lists all tracks with track numbers and durations
- Play the entire album or individual tracks
- Download the complete album (if enabled)

### Artist Shares

Share an artist profile:

- Displays artist image and name
- Shows artist information
- Links to the artist's albums and songs
- Great for introducing someone to an artist

### Playlist Shares

Share a curated playlist:

- Displays playlist name and description
- Lists all songs in playlist order
- Play the entire playlist or jump to specific tracks
- Download all songs (if enabled)

## Use Cases

### Sharing with Friends

Send a friend a link to:
- A song you think they'll love
- A new album discovery
- Your "Road Trip" playlist

### Event Playlists

Create a playlist for an event and share:
- Party playlist for guests to preview
- Wedding reception music for the couple to review
- Workout playlist for gym buddies

### Music Discovery

Share interesting finds:
- Obscure artist you discovered
- Classic album recommendation
- Genre-specific playlist

### Temporary Access

Use expiring shares for:
- Preview access before a music swap
- Limited-time promotional sharing
- Time-boxed listening sessions

## Privacy and Security

### What Recipients Can See

- The shared content (song, album, artist, or playlist)
- Cover art and metadata
- Your description (if provided)

### What Recipients Cannot See

- Your username or personal information
- Other content in your library
- Your listening history or statistics
- Other users' shares

### Best Practices

- Use expiring shares for sensitive or temporary content
- Disable downloads if you're concerned about redistribution
- Delete shares you no longer need
- Monitor visit counts for unexpected access patterns

## API Access

Shares are accessible via the Melodee API:

```
# List your shares
GET /api/v1/shares

# Get share by API key
GET /api/v1/shares/{apiKey}

# Create a new share
POST /api/v1/shares
Body: {
  "shareType": "Song|Album|Artist|Playlist",
  "resourceId": "<guid>",
  "description": "Optional description",
  "isDownloadable": true|false,
  "expiresAt": "2024-12-31T23:59:59Z" (optional)
}

# Update a share
PUT /api/v1/shares/{apiKey}
Body: {
  "description": "Updated description",
  "isDownloadable": true|false,
  "expiresAt": "2024-12-31T23:59:59Z"
}

# Delete a share
DELETE /api/v1/shares/{apiKey}

# Public access (no auth required)
GET /api/v1/shares/public/{shareUniqueId}
```

## OpenSubsonic API

Shares are also available through the OpenSubsonic API:

```
# Get all shares
GET /rest/getShares

# Create a share
GET /rest/createShare?id=<songId>&description=<text>&expires=<timestamp>

# Update a share
GET /rest/updateShare?id=<shareId>&description=<text>&expires=<timestamp>

# Delete a share
GET /rest/deleteShare?id=<shareId>
```

## Troubleshooting

### Share Link Not Working

- Check if the share has expired
- Verify the share hasn't been deleted
- Ensure the shared content still exists in your library

### Cannot Create Shares

- Verify your account has the "Share" capability enabled
- Check that you're not locked out of your account
- Ensure the content you're sharing exists

### Downloads Not Available

- The share creator may have disabled downloads
- Check your browser's download settings
- Large albums may take time to prepare for download

---

Have questions about shares? Open an issue on GitHub or check the [API documentation](/api/) for technical details.

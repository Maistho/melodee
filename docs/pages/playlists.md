---
title: Playlists
permalink: /playlists/
---

# Playlists

Melodee supports two types of playlists: **Dynamic Playlists** that automatically populate based on user actions, and **User Playlists** that you create and manage manually.

## Dynamic Playlists

Dynamic playlists are automatically generated and updated based on your listening behavior and ratings. You don't need to manually add songs—they appear automatically when you interact with your music.

### Your Favorite Songs

A personal playlist containing all songs you've "liked" or starred.

**How songs are added:**
- Click the heart/star icon on any song
- Use the "like" or "favorite" action in your music client
- Songs appear instantly in this playlist

**How songs are removed:**
- Unlike or unstar the song
- The song is immediately removed from the playlist

This playlist is **private to you**—other users have their own "Your Favorite Songs" playlist.

### Your Rated Songs

A personal playlist containing all songs you've given a rating (1-5 stars).

**How songs are added:**
- Rate any song from 1 to 5 stars
- Use your client's rating feature or the Melodee UI
- Songs appear instantly in this playlist

**How songs are removed:**
- Remove the rating (set to 0 or unrated)
- The song is immediately removed from the playlist

This playlist is **private to you** and sorted by rating (highest rated songs first).

### Rated Songs

A **global playlist** showing all songs that any user has rated greater than zero. This is a community-curated collection of quality music.

**How songs are added:**
- When any user rates a song 1-5 stars, it appears in this playlist
- Multiple users rating the same song doesn't create duplicates

**How songs are removed:**
- Only when all user ratings for a song are removed
- As long as one user has rated it, the song remains

This playlist is **visible to all users** and provides a great way to discover music that others in your household or organization have enjoyed.

## Dynamic Playlist Summary

| Playlist | Scope | Trigger | Visibility |
|----------|-------|---------|------------|
| Your Favorite Songs | Personal | Like/Star a song | Only you |
| Your Rated Songs | Personal | Rate a song (1-5) | Only you |
| Rated Songs | Global | Any user rates a song | All users |

## User Playlists

User playlists are traditional playlists that you create and manually curate. You have full control over which songs are included and their order.

### Creating Playlists

You can create playlists through:

- **Melodee UI**: Navigate to Playlists and click "Create New Playlist"
- **Music Clients**: Most Subsonic-compatible clients support playlist creation

### Managing Playlists via Clients

Most Subsonic-compatible music clients support full playlist management:

#### Symfonium (Android)
- Long-press a song → "Add to playlist"
- Create new playlists from the playlist screen
- Reorder songs by drag-and-drop

#### DSub (Android)
- Menu on any song → "Add to Playlist"
- Playlist management in the Playlists tab
- Supports playlist editing and deletion

#### Sonixd (Desktop)
- Right-click a song → "Add to Playlist"
- Full playlist editor with drag-and-drop reordering
- Create, rename, and delete playlists

#### Sublime Music (Desktop)
- Add songs via context menu
- Manage playlists in the sidebar
- Supports M3U import/export

#### play:Sub (iOS)
- Tap the menu on any song → "Add to Playlist"
- Create and manage playlists in the Playlists tab

### Playlist Operations

| Operation | Melodee UI | Client Support |
|-----------|------------|----------------|
| Create playlist | ✓ | Most clients |
| Add songs | ✓ | Most clients |
| Remove songs | ✓ | Most clients |
| Reorder songs | ✓ | Some clients |
| Rename playlist | ✓ | Some clients |
| Delete playlist | ✓ | Most clients |
| Public/Private toggle | ✓ | Limited |

### Playlist Visibility

- **Private playlists**: Only visible to you (default)
- **Public playlists**: Visible to all users on the server

Toggle visibility in the Melodee UI playlist settings.

## Likes vs Ratings

Understanding the difference between likes and ratings:

| Action | Effect | Dynamic Playlist |
|--------|--------|------------------|
| **Like/Star** | Binary (on/off) | Your Favorite Songs |
| **Rate 1-5** | Granular preference | Your Rated Songs, Rated Songs (global) |

**Pro tip**: You can both like AND rate a song. Liking adds it to "Your Favorite Songs" while rating adds it to "Your Rated Songs" and the global "Rated Songs" playlist.

## API Details

For developers building clients:

### OpenSubsonic API

```
# Get playlists
GET /rest/getPlaylists

# Get playlist contents
GET /rest/getPlaylist?id=<playlistId>

# Create playlist
GET /rest/createPlaylist?name=<name>

# Update playlist (add/remove songs)
GET /rest/updatePlaylist?playlistId=<id>&songIdToAdd=<songId>&songIndexToRemove=<index>

# Delete playlist
GET /rest/deletePlaylist?id=<playlistId>

# Star/unstar a song (for favorites)
GET /rest/star?id=<songId>
GET /rest/unstar?id=<songId>

# Set rating
GET /rest/setRating?id=<songId>&rating=<0-5>
```

### Native Melodee API

```
# Star/unstar
POST /api/v1/Songs/starred/{songId}/{isStarred}

# Set rating
POST /api/v1/Songs/setrating/{songId}/{rating}
```

## Best Practices

- **Use likes for quick favorites**: Fast way to build a "best of" collection
- **Use ratings for nuanced preferences**: 5-star system helps with sorting and recommendations
- **Check global Rated Songs**: Great for discovering what others enjoy
- **Create themed playlists**: Organize by mood, genre, activity, or occasion
- **Sync across clients**: Playlists created in any client appear everywhere

## Tips for Discovery

1. **Browse "Rated Songs"** to see what's popular across all users
2. **Sort "Your Rated Songs"** by rating to find your top-rated tracks
3. **Use "Your Favorite Songs"** as a quick-access playlist for everyday listening
4. **Create playlists from dynamic playlists** by adding songs you discover

---

Have questions about playlists? Open an issue on GitHub or check the [API documentation](/api/) for technical details.

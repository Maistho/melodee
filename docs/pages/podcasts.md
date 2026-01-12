---
title: Podcasts
permalink: /podcasts/
---

# Podcasts

Melodee includes full podcast support, allowing you to subscribe to your favorite shows, automatically download new episodes, track your listening progress, and pick up where you left off across devices.

## Overview

The podcast feature in Melodee provides:

- **Channel Subscriptions**: Subscribe to podcasts via RSS feed URL or discover new shows through iTunes
- **Automatic Downloads**: Configure auto-download for new episodes on a per-channel basis
- **Per-Channel Settings**: Customize refresh intervals and storage limits for each subscription
- **Playback Tracking**: Resume playback exactly where you left off with automatic bookmarking
- **Episode Search**: Search across all episodes in your subscribed podcasts
- **OPML Import/Export**: Migrate your subscriptions to and from other podcast apps
- **Dashboard Pinning**: Pin your favorite podcasts to the dashboard for quick access

## Getting Started

### Subscribing to a Podcast

There are three ways to add podcasts to your library:

#### 1. Add by Feed URL

If you know the RSS feed URL of a podcast:

1. Navigate to **Data → Podcasts** in the main menu
2. Click the **Add Channel** button
3. Paste the podcast's RSS feed URL
4. Click **Add**

The podcast will be fetched, and all available episodes will be listed.

#### 2. Discover via iTunes

To browse and search the iTunes podcast directory:

1. Navigate to **Data → Podcasts**
2. Click the **Discover** button
3. Search for podcasts by name, topic, or keyword
4. Click **Subscribe** on any podcast to add it to your library

#### 3. Import from OPML

If you're migrating from another podcast app:

1. Export your subscriptions as OPML from your current app
2. Navigate to **Data → Podcasts** in Melodee
3. Click the **OPML** button
4. Choose the **Import** tab
5. Either upload your OPML file or paste the content directly
6. Click **Import**

Melodee will add all podcasts from the OPML file, skipping any duplicates.

## Managing Subscriptions

### Pinning Podcasts to Dashboard

You can pin your favorite podcasts to the dashboard for quick access:

1. Navigate to the podcast channel detail page
2. Click the **pin icon** in the channel header (next to the title)
3. The podcast will appear in your Dashboard's pinned items section

To unpin a podcast:

1. Click the pin icon again on the channel detail page, or
2. Remove it from your Dashboard pinned items

Pinned podcasts show:
- The podcast artwork
- The podcast title
- Direct link to the channel page

This feature is user-specific—each user can pin different podcasts to their own dashboard.

### Channel Settings

Each podcast channel can be configured independently. Click the **Settings** button on any channel detail page to access:

| Setting | Description | Default |
|---------|-------------|---------|
| **Auto-download** | Automatically download new episodes when the feed is refreshed | Off |
| **Refresh interval** | How often to check for new episodes (in hours). Set to 0 to use the global schedule | Global |
| **Max downloaded episodes** | Limit the number of downloaded episodes per channel. Set to 0 for unlimited | Unlimited |
| **Max storage** | Limit total storage used by this channel (in MB). Set to 0 for unlimited | Unlimited |

### Refreshing Feeds

Podcasts are automatically refreshed based on their configured interval or the global schedule. You can also:

- **Refresh a single channel**: Click the refresh button on the channel detail page
- **Refresh all channels**: Click "Refresh All" on the main Podcasts page

### Deleting a Subscription

To unsubscribe from a podcast:

1. Go to the channel detail page
2. Click the **Delete** button
3. Confirm the deletion

This will remove the subscription and all downloaded episode files. Episode metadata is not recoverable after deletion.

## Episodes

### Downloading Episodes

Episodes can be downloaded for offline listening:

- **Manual download**: Click the download button on any episode
- **Auto-download**: Enable auto-download in channel settings to automatically queue new episodes

Downloaded episodes are stored in the podcast library directory and organized by user and channel.

### Episode Status

Each episode shows its current status:

| Status | Icon | Description |
|--------|------|-------------|
| Not Downloaded | — | Episode available for streaming or download |
| Queued | 🕐 | Episode is waiting to be downloaded |
| Downloading | ⬇️ | Download in progress |
| Downloaded | ✓ | Episode is available offline |
| Failed | ⚠️ | Download failed (check error message) |

### Streaming vs Downloaded

You can stream episodes directly without downloading them. However, downloaded episodes:

- Play faster (no buffering)
- Are available offline
- Don't count against bandwidth limits

### Deleting Episodes

To delete a downloaded episode:

1. Click the delete button on the episode row
2. Confirm the deletion

This removes the downloaded file but keeps the episode metadata. You can re-download it later.

## Playback & Bookmarks

### Resume Playback

Melodee automatically tracks your playback position. When you start playing an episode:

- Your position is saved periodically
- Resuming later continues from where you stopped
- Works across different devices and clients

### Bookmarks

You can manually create bookmarks with notes:

1. While playing an episode, use the bookmark feature
2. Add an optional comment to remember why you bookmarked
3. Return to the bookmark later from the episode detail page

### Play History

View your listening history for any episode:

- Total times played
- Last played date
- Playback positions over time

## Episode Search

Search across all episodes in your subscribed podcasts:

1. Navigate to **Data → Podcasts**
2. Click the **Search Episodes** button
3. Enter your search terms (minimum 3 characters)
4. Results show matches from episode titles and descriptions

Search results link directly to the episode's channel for easy access.

## OPML Import/Export

### Exporting Your Subscriptions

To backup or migrate your subscriptions:

1. Navigate to **Data → Podcasts**
2. Click the **OPML** button
3. Choose the **Export** tab
4. Click **Download OPML**

The exported file follows the OPML 2.0 standard and is compatible with most podcast applications.

### Importing Subscriptions

To import from another podcast app:

1. Export OPML from your source application
2. In Melodee, navigate to **Data → Podcasts**
3. Click the **OPML** button
4. Choose the **Import** tab
5. Upload the file or paste the OPML content
6. Click **Import**

The import shows results including:

- Successfully imported podcasts
- Skipped duplicates (already subscribed)
- Failed imports with error details

## Background Jobs

Melodee runs several background jobs to manage podcasts automatically:

| Job | Purpose | Default Schedule |
|-----|---------|------------------|
| **PodcastRefreshJob** | Fetches new episodes from subscribed feeds | Every 30 minutes |
| **PodcastDownloadJob** | Downloads queued episodes | Every 5 minutes |
| **PodcastCleanupJob** | Enforces retention policies | Daily at 3 AM |
| **PodcastRecoveryJob** | Resets stuck downloads, cleans temp files | Every hour |

### Refresh Behavior

The refresh job checks each channel's `NextSyncAt` timestamp:

- Channels with a custom refresh interval use that interval
- Channels without a custom interval use the global schedule
- Channels with consecutive failures use exponential backoff

### Download Quotas

Downloads respect several limits:

- **Global concurrent downloads**: Maximum downloads across all users
- **Per-user concurrent downloads**: Maximum downloads per user
- **Per-channel episode limit**: Maximum downloaded episodes per channel
- **Per-channel storage limit**: Maximum storage per channel
- **Per-user storage quota**: Maximum total podcast storage per user

When a limit is reached, new downloads are blocked until space is freed.

### Retention Policies

The cleanup job supports three retention modes (configured globally):

1. **Keep for X days**: Delete episodes downloaded more than N days ago
2. **Keep last N episodes**: Keep only the most recent N downloaded episodes per channel
3. **Keep unplayed only**: Delete episodes that have been fully played

These policies help manage storage automatically without manual intervention.

## API Access

### OpenSubsonic API

Melodee implements the OpenSubsonic podcast endpoints:

```
# List podcast channels
GET /rest/getPodcasts

# Get episodes for a channel
GET /rest/getPodcasts?id=<channelId>&includeEpisodes=true

# Subscribe to a podcast
GET /rest/createPodcastChannel?url=<feedUrl>

# Refresh a channel
GET /rest/refreshPodcasts?id=<channelId>

# Delete a channel
GET /rest/deletePodcastChannel?id=<channelId>

# Download an episode
GET /rest/downloadPodcastEpisode?id=<episodeId>

# Delete an episode
GET /rest/deletePodcastEpisode?id=<episodeId>
```

### Native Melodee API

The Melodee REST API provides additional podcast features:

```
# List channels
GET /api/v1/podcasts/channels

# Create channel
POST /api/v1/podcasts/channels
Body: { "feedUrl": "https://..." }

# Update channel settings
PATCH /api/v1/podcasts/channels/{id}
Body: { "autoDownloadEnabled": true, "refreshIntervalHours": 12 }

# Search episodes
GET /api/v1/podcasts/episodes/search?query=<term>&limit=50

# Discover podcasts (iTunes)
GET /api/v1/podcasts/discover/search?term=<query>&limit=25

# Export OPML
GET /api/v1/podcasts/opml/export

# Import OPML
POST /api/v1/podcasts/opml/import
Body: { "opmlContent": "<opml>...</opml>" }

# Episode playback tracking
POST /api/v1/podcasts/episodes/{id}/play
Body: { "positionSeconds": 1234 }

# Get/set bookmark
GET /api/v1/podcasts/episodes/{id}/bookmark
PUT /api/v1/podcasts/episodes/{id}/bookmark
Body: { "positionSeconds": 1234, "comment": "Good part" }
```

## Configuration

Podcast settings can be configured in the admin settings panel:

| Setting | Description | Default |
|---------|-------------|---------|
| `PodcastEnabled` | Enable/disable podcast feature globally | true |
| `PodcastDownloadMaxConcurrentGlobal` | Maximum concurrent downloads across all users | 3 |
| `PodcastDownloadMaxConcurrentPerUser` | Maximum concurrent downloads per user | 2 |
| `PodcastDownloadMaxEnclosureBytes` | Maximum episode file size | 500 MB |
| `PodcastQuotaMaxBytesPerUser` | Maximum total podcast storage per user | 10 GB |
| `PodcastHttpTimeoutSeconds` | HTTP timeout for feed fetching | 30 |
| `PodcastHttpMaxRedirects` | Maximum HTTP redirects to follow | 5 |
| `PodcastRetentionDownloadedEpisodesInDays` | Auto-delete episodes older than N days | 0 (disabled) |
| `PodcastRetentionKeepLastNEpisodes` | Keep only last N episodes per channel | 0 (disabled) |
| `PodcastRetentionKeepUnplayedOnly` | Auto-delete played episodes | false |
| `PodcastRecoveryStuckDownloadThresholdMinutes` | Reset downloads stuck longer than N minutes | 30 |
| `PodcastRecoveryOrphanedUsageThresholdHours` | Delete temp files older than N hours | 24 |

## Client Compatibility

Podcast functionality works with OpenSubsonic-compatible clients that support the podcast endpoints:

| Client | Podcast Support | Notes |
|--------|-----------------|-------|
| DSub | ✅ Full | Subscribe, download, play, bookmark |
| Symfonium | ✅ Full | All features supported |
| Ultrasonic | ✅ Full | All features supported |
| Melodee Web UI | ✅ Full | Full feature access |
| Sonixd | ⚠️ Partial | Basic playback |
| play:Sub | ⚠️ Partial | Basic playback |

## Troubleshooting

### Feed Won't Load

1. Verify the RSS feed URL is correct and accessible
2. Check if the feed requires authentication (not supported)
3. Look for error messages in the channel status
4. Try refreshing the feed manually

### Episodes Won't Download

1. Check if storage quotas have been reached
2. Verify the episode URL is accessible
3. Check for download errors in the episode status
4. Review the PodcastDownloadJob logs

### Playback Position Not Saving

1. Ensure you're logged in
2. Check that the client is sending scrobble/bookmark requests
3. Verify the PodcastPlaybackService is running

### Missing Episodes After Refresh

1. Some podcasts limit their feed to recent episodes
2. Check the feed source for episode availability
3. Previously downloaded episodes remain available

---

Have questions about podcasts? Open an issue on GitHub or check the [API documentation](/api/) for technical details.

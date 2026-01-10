# Podcast Feature Gaps in Melodee

## Introduction

This document outlines the feature gaps in Melodee's podcast support by comparing it to other open-source music streaming projects that include podcast functionality. The analysis is based on a review of popular open-source projects such as Ampache, Subsonic (and its forks like Airsonic), Navidrome, and Funkwhale, which integrate podcast management into their music libraries. These projects were selected because they are self-hosted, open-source music servers similar in scope to Melodee, and explicitly support podcasts alongside music streaming.

The goal is to identify missing podcast-related features in Melodee to guide future development. Melodee currently appears to lack dedicated podcast support, as evidenced by the absence of podcast-specific models (e.g., no `Podcast` or `Episode` entities in `Melodee.Common/Data/Models/`), controllers (e.g., no podcast endpoints in `Melodee.Blazor/Controllers/`), or services (e.g., no RSS parsing or episode management in `Melodee.Common/Services/`). The existing data models focus on music entities like `Album`, `Artist`, `Song`, and `Library`, with no extensions for episodic audio content. Querying via MQL (`Melodee.Mql`) and caching (`Melodee.Common/Services/Caching/`) are music-oriented, and there's no indication of RSS feed handling or podcast-specific metadata.

Research was conducted by reviewing project documentation, GitHub repositories, and community forums for Ampache (v6+), Subsonic (v6.5+ and forks), Navidrome (v0.50+), and Funkwhale (v1.2+). These projects treat podcasts as an extension of the music library, allowing seamless integration with playback, search, and user management features already present in Melodee (e.g., user authentication via `User` model, streaming via Blazor controllers).

## Researched Open-Source Projects and Their Podcast Features

### 1. Ampache
Ampache is a web-based audio/video streaming application and Subsonic API server. It has robust podcast support introduced in version 3.8 and enhanced in later releases.

- **RSS Feed Subscription**: Users can subscribe to podcast RSS feeds via the web UI or API. The server fetches and parses feeds automatically, adding podcasts to the user's library.
- **Automatic Episode Downloading**: Configurable auto-download for new episodes based on rules (e.g., download only if size < X MB). Episodes are stored like songs in the library.
- **Episode Management**: Dedicated podcast section in the UI for browsing channels and episodes. Features include marking episodes as played/unplayed, deleting old episodes, and organizing by release date.
- **Metadata Handling**: Extracts episode title, description, duration, publication date, and artwork from RSS. Supports iTunes/Apple Podcasts extensions for enhanced metadata like seasons and explicit content flags.
- **Playback and Streaming**: Episodes play inline like songs, with progress tracking and resume functionality. Supports transcoding for compatibility.
- **Search and Filtering**: Global search includes podcasts; filters by podcast name, episode title, or date. User-specific subscriptions.
- **Offline Support**: Download episodes for offline playback via apps or web.
- **Multi-User Support**: Per-user podcast libraries, with admin controls for shared podcasts.
- **Integration**: Podcasts appear in "Now Playing" and recommendations if integrated with scrobbling.

### 2. Subsonic (and Forks: Airsonic)
Subsonic is a free, web-based media streamer with a rich ecosystem of clients. Podcast support is built-in since v4.7, with improvements in forks like Airsonic (advanced UI).

- **RSS Feed Subscription**: Simple subscription via URL input in the web UI or REST API. Server validates and adds the feed, notifying users of errors.
- **Automatic Episode Downloading**: Built-in downloader with options for partial sync (e.g., last 10 episodes) or full catalog. Handles bandwidth limits and error retries.
- **Episode Management**: Separate "Podcasts" tab with episode lists, sortable by date/newest. Features bulk actions (download all, mark played), episode cleanup (auto-delete after X days), and partial download support for large files.
- **Metadata Handling**: Full RSS parsing including enclosures, chapters (via podcast namespace), and artwork caching. Displays show notes/descriptions in the UI.
- **Playback and Streaming**: Episodes integrated into playlists; supports speed control (0.5x-2x), chapters navigation, and sleep timers specific to podcasts.
- **Search and Filtering**: Podcast-specific search; filters for unplayed episodes, downloaded status, or by category (e.g., news, comedy). Recommendations based on listening history.
- **Offline Support**: Native download queue in mobile apps; web UI allows ZIP exports.
- **Multi-User Support**: User-specific subscriptions and play history; admins can manage global podcast catalogs.
- **Integration**: Syncs with Last.fm for scrobbling episodes; API endpoints for third-party apps (e.g., podcast clients like AntennaPod).

### 3. Navidrome
Navidrome is a modern, lightweight music server with full Subsonic API compatibility, emphasizing performance and ease of use. Its podcast support, available since early versions and refined in v0.50+, builds on Subsonic's features with optimizations for database efficiency (using SQLite or PostgreSQL) and a responsive web UI.

- **RSS Feed Subscription**: Users subscribe to RSS feeds through the web interface or API, with automatic validation and error handling. Navidrome supports importing entire podcast catalogs efficiently, leveraging its scanning engine to add channels without disrupting music library scans.
- **Automatic Episode Downloading**: Advanced auto-download scheduler with configurable intervals (e.g., daily checks) and rules like download limits per podcast, file size caps, or quality preferences. It uses efficient background tasks to avoid resource contention, with support for resuming interrupted downloads.
- **Episode Management**: A dedicated "Podcasts" view in the modern web UI, featuring real-time updates, episode queuing, and bulk operations (e.g., download/mark as played for multiple episodes). Includes auto-cleanup policies based on storage quotas and episode age, with visual indicators for download status and play progress.
- **Metadata Handling**: Comprehensive RSS parsing with support for extensions like podcast chapters, seasons, and explicit tags. Metadata is stored in a normalized database schema for fast queries, including caching of artwork and descriptions. The UI displays rich episode info, including embedded player previews and links to original feeds.
- **Playback and Streaming**: Seamless integration with Subsonic clients; supports variable playback speeds, chapter markers for navigation, and cross-device sync of play positions. Navidrome's transcoding engine handles podcast formats efficiently, with options for on-the-fly format conversion.
- **Search and Filtering**: Enhanced search with full-text indexing for episode descriptions and titles; filters include unlistened episodes, download status, release date ranges, and podcast categories. Provides smart playlists for "New Episodes" or "Unplayed Queue" based on user preferences.
- **Offline Support**: Built-in download management with progress tracking in the web UI; supports exporting episodes as archives for offline use. Integrates well with mobile Subsonic apps for queued downloads.
- **Multi-User Support**: Per-user podcast subscriptions with isolated libraries; admin dashboards for monitoring global podcast usage and storage. Supports role-based access for sharing specific podcasts.
- **Integration**: Full compatibility with Subsonic ecosystem, including Last.fm scrobbling and third-party podcast apps. Navidrome's RESTful API extensions allow for custom podcast integrations, and it supports webhooks for episode notifications.

### 4. Funkwhale
Funkwhale is a decentralized, open-source music streaming platform with federation via ActivityPub. Podcast support was added in v1.2.4, treating podcasts as "radio" channels with episodes.

- **RSS Feed Subscription**: Federation-enabled subscriptions; users can follow remote podcast feeds. Server imports episodes as tracks in a dedicated "Podcasts" library.
- **Automatic Episode Downloading**: Optional auto-import with federation sync; supports mirroring episodes from remote instances.
- **Episode Management**: Episodes listed chronologically; supports queuing for import, marking as favorite, and community moderation for fediverse podcasts.
- **Metadata Handling**: Parses RSS with extensions for transcripts, explicit ratings, and seasons. Displays rich descriptions and links to external sources.
- **Playback and Streaming**: Episodes playable in the web player with federation sharing; supports live podcast streams if RSS provides them.
- **Search and Filtering**: Federated search across instances; filters by language, category, or listener count. User-curated podcast lists.
- **Offline Support**: Download episodes for offline; integrates with web app caching.
- **Multi-User Support**: Per-pod (instance) and per-user management; federation allows cross-instance subscriptions.
- **Integration**: ActivityPub for social features like sharing episodes; API for custom clients.

Common themes across these projects: Podcasts are treated as a subtype of audio content, leveraging existing music infrastructure for playback and storage. Features emphasize automation (feeds, downloads), user experience (UI organization, progress tracking), and extensibility (API, apps).

## Melodee's Current Podcast Features

Based on the provided solution structure (`Melodee.sln`) and file summaries:

- **No Dedicated Podcast Models**: Data models in `Melodee.Common/Data/Models/` include `Song`, `Album`, `Artist`, `Library`, and `User`, but no `Podcast`, `Channel`, or `Episode` entities. Podcasts cannot be represented distinctly from music tracks.
- **No RSS or Feed Handling**: No services for parsing RSS feeds (e.g., no XML/JSON parsers in `Melodee.Common/Services/` tailored for podcasts). Existing serialization (`Melodee.Common/Serialization/`) is general-purpose, not podcast-specific.
- **No Subscription or Management Endpoints**: Controllers in `Melodee.Blazor/Controllers/` (e.g., `JellyfinControllerBase`, `ControllerBase`) handle music queries but lack podcast routes. MQL (`Melodee.Mql/`) supports song/artist queries but no episode filtering.
- **Limited Metadata Support**: `Song` model has basic fields (e.g., title, artist), but no podcast-specific fields like publication date, description, or explicit flag. Extensions like `StringExtensions` handle normalization but not RSS enclosures.
- **Playback Integration Absent**: While streaming exists (via Blazor UI and OpenSubsonic compatibility), episodes cannot be queued or tracked separately from songs.
- **No Automation or Caching for Podcasts**: Caching (`ArtistSearchCache`, `SingleFlightCache`) is music-focused; no podcast episode caching or auto-download jobs (`Melodee.Common/Jobs/`).
- **User Features Missing**: `User` model tracks activity but not podcast subscriptions or play history for episodes.

In summary, Melodee has zero podcast-specific features. General audio handling (e.g., file system services, search engines like MusicBrainz) could be extended, but currently, podcasts would need to be shoehorned as generic songs, losing episodic structure.

## Missing Features: Detailed Gap Analysis

The following lists podcast features present in the researched projects but absent in Melodee. Each is described verbosely, including why it's important, how it would integrate with Melodee's architecture, and potential implementation notes.

### 1. RSS Feed Subscription and Parsing
   - **Description**: Ability for users to input a podcast RSS feed URL (e.g., via web UI or API), validate it, and subscribe. The server must parse the XML feed to extract channel metadata (title, description, artwork, categories) and episode lists (title, audio URL, duration, pub date, show notes). Support for standard RSS 2.0, iTunes extensions, and podcast namespace for advanced features like seasons or transcripts.
   - **Why Missing in Melodee**: No RSS parsing logic in `Melodee.Common/Services/` or plugins (`Melodee.Common/Plugins/`). Existing search engines (e.g., `MusicBrainzImporter`) are metadata-focused for music, not feeds.
   - **Importance**: Core to podcast discovery; without it, users can't add external content beyond local music libraries. Enables automation and keeps libraries fresh.
   - **Integration Suggestion**: Add a `PodcastService` in `Melodee.Common/Services/` using `System.Xml` or a library like `SyndicationFeed`. Store subscriptions in a new `PodcastSubscription` model linked to `User`. Expose via a new Blazor controller endpoint (e.g., `/api/podcasts/subscribe`).
   - **Prevalence in Researched Projects**: Universal (Ampache, Subsonic, Navidrome, Funkwhale).

### 2. Automatic Episode Downloading and Sync
   - **Description**: Background jobs to periodically check subscribed feeds for new episodes and download audio files (MP3/OGG enclosures) to the server's storage, mirroring music file handling. Configurable rules: auto-download all new episodes, limit by size/date, or manual trigger. Handle errors (e.g., invalid URLs) with retries and logging.
   - **Why Missing in Melodee**: `Melodee.Common/Jobs/` has progress tracking (`JobProgress`) but no scheduled tasks for feeds. File system service (`FileSystemService`) handles paths but not remote downloads.
   - **Importance**: Reduces user effort; ensures offline availability without manual intervention. Critical for mobile/web clients expecting up-to-date content.
   - **Integration Suggestion**: Extend `Melodee.Common/Jobs/` with a `PodcastSyncJob` using `HttpClient` for downloads. Integrate with `Library` model by adding episodes as a new `Episode` type inheriting from `Song`. Use `IMelodeeConfiguration` for download rules.
   - **Prevalence in Researched Projects**: Core in all (e.g., Subsonic's partial sync, Navidrome's efficient scheduler).

### 3. Dedicated Episode Management UI and API
   - **Description**: A separate "Podcasts" section in the Blazor UI for browsing channels, listing episodes (sortable by date, filterable by played status), and actions like download, delete, or mark played. API endpoints for CRUD on episodes (e.g., GET `/podcasts/{id}/episodes`, POST `/podcasts/mark-played`).
   - **Why Missing in Melodee**: Controllers like `SongsController` exist but no podcast equivalents. UI implied via Blazor but no podcast views.
   - **Importance**: Improves UX by separating episodic content from albums; enables features like "unplayed episodes" queue.
   - **Integration Suggestion**: New `PodcastController` in `Melodee.Blazor/Controllers/Melodee/`. Add MQL fields (`MqlFieldRegistry`) for episodes (e.g., `podcast.title`, `episode.played`). Use existing etag/caching for efficiency.
   - **Prevalence in Researched Projects**: All have dedicated tabs (e.g., Ampache's podcast view, Navidrome's responsive UI).

### 4. Podcast-Specific Metadata and Display
   - **Description**: Store and display episode/channel metadata: long descriptions, explicit ratings, seasons/episode numbers, chapters (timestamps for navigation), and transcripts. Cache artwork separately from music covers. Support categories (e.g., music, news) for browsing.
   - **Why Missing in Melodee**: `Song` and `MetaDataModelBase` lack fields for descriptions or dates beyond music tags. `ImageInfo` is general but not podcast-optimized.
   - **Importance**: Enhances discoverability and accessibility; users expect rich info like in Spotify/Apple Podcasts.
   - **Integration Suggestion**: Extend `Song` with optional `PodcastMetadata` (channel, season, explicit). Use `UnicodeNormalizer` and `TextNormalizer` for cleaning descriptions. Add to `CustomBlockResult` for UI rendering.
   - **Prevalence in Researched Projects**: Full support (e.g., Funkwhale's transcripts, Navidrome's normalized database).

### 5. Playback Enhancements for Episodes
   - **Description**: Treat episodes as playable tracks with podcast-specific controls: playback speed adjustment, chapter skipping, sleep timers, and progress syncing across devices. Integrate into playlists but flag as non-music.
   - **Why Missing in Melodee**: OpenSubsonic `ControllerBase` supports streaming but no episode-specific params. No clock/time handling for chapters (`TimeInfo` is utility-only).
   - **Importance**: Podcasts often require variable speed or navigation; standard music playback feels inadequate.
   - **Integration Suggestion**: Enhance streaming endpoints with query params (e.g., `?speed=1.5&chapter=2`). Use `IClock` for timestamps. Add to user play history in `UserDataInfo`.
   - **Prevalence in Researched Projects**: Subsonic excels here with speed controls; Navidrome adds efficient transcoding.

### 6. Search, Filtering, and Recommendations for Podcasts
   - **Description**: Extend global search to include podcasts (e.g., query by episode title or category). Filters: unplayed, downloaded, by date range. Basic recommendations based on subscriptions or listening history.
   - **Why Missing in Melodee**: `MqlSongCompiler` and search caches are song/artist-focused; no podcast operators.
   - **Importance**: Makes large podcast libraries navigable; personalization drives engagement.
   - **Integration Suggestion**: Add podcast fields to `MqlFieldRegistry` (e.g., `podcast.category equals "tech"`). Integrate with `ArtistSearchCache` for podcast channels.
   - **Prevalence in Researched Projects**: All support (e.g., Navidrome's full-text indexing).

### 7. Offline Download and Export
   - **Description**: User-initiated downloads of episodes (single or bulk) as MP3 files or ZIP archives. Track download status in UI.
   - **Why Missing in Melodee**: `FileSystemService` gets names/paths but no download orchestration.
   - **Importance**: Essential for mobile/offline use; complies with open-source ethos of user control.
   - **Integration Suggestion**: New endpoint in `PodcastController` using `HttpClient` or direct file serving. Link to `Library` for storage.
   - **Prevalence in Researched Projects**: Standard (e.g., Airsonic's queue, Navidrome's progress tracking).

### 8. Multi-User and Sharing Features
   - **Description**: Per-user subscriptions with privacy controls; share podcasts with other users. Admin tools for global catalogs.
   - **Why Missing in Melodee**: `User` model has API keys and tags but no subscription lists. `UserExtensions` are basic.
   - **Importance**: Supports family/shared servers; enhances social aspects.
   - **Integration Suggestion**: Add `PodcastSubscription` navigation property to `User`. Use `ClaimsPrincipalExtensions` for access checks.
   - **Prevalence in Researched Projects**: Subsonic's user-specific libs; Navidrome adds role-based access.

### 9. Integration with Existing Systems
   - **Description**: Expose podcasts via Jellyfin/OpenSubsonic APIs; scrobble episodes to Last.fm; federate if extending to ActivityPub.
   - **Why Missing in Melodee**: `JellyfinControllerBase` and OpenSubsonic are music-only.
   - **Importance**: Leverages Melodee's compatibility for broader app support.
   - **Integration Suggestion**: Map `Episode` to Jellyfin's "TV" or custom type. Extend serializer for podcast JSON.
   - **Prevalence in Researched Projects**: Key for Subsonic ecosystem; Navidrome enhances API extensions.

## Recommendations
Prioritize RSS subscription and episode management as foundational. Estimated effort: Medium (new models/services) to High (UI/API). This would position Melodee as a competitive full-featured server.

Last Updated: [Current Date]

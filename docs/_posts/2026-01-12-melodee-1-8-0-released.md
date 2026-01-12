---
title: "Melodee 1.8.0 Released"
date: 2026-01-12
badges:
  - type: info
    tag: release
---

We're excited to announce the release of Melodee 1.8.0, packed with new features for an enhanced music streaming experience!

<!--more-->

## What's New in 1.8.0

### Party Mode
Collaborative listening sessions with shared queues and synchronized playback control. Perfect for gatherings or remote listening parties with friends and family.

- Create and manage party sessions
- Shared queue management
- Real-time playback synchronization
- Participant roles and permissions

### Jukebox Mode
Server-side audio playback via MPV or MPD backends, enabling whole-home audio setups.

- Control playback from any device
- Support for MPV and MPD audio backends
- Ideal for dedicated audio endpoints

### Podcast Support
Full podcast subscription and playback support with tracking.

- Subscribe to podcasts via RSS or OPML import
- Automatic episode downloads
- Playback position tracking
- Integration with OpenSubsonic podcast APIs

### Custom Theming
Personalize Melodee's appearance with custom themes.

- Create custom color schemes
- Custom fonts and branding
- Hide/show navigation items
- Theme pack import/export

### Music Charts
Curated album charts updated automatically.

- Billboard chart integration
- Multiple chart sources
- Automatic updates via background jobs

### Additional Improvements

- **Jellyfin API compatibility** - Use Jellyfin clients with Melodee
- **Deezer search engine** - Additional metadata source
- **Enhanced scrobbling** - Improved Last.fm integration
- **Performance improvements** - Faster library scanning and streaming

## Upgrading

To upgrade to 1.8.0, follow the [upgrade guide](/upgrade/).

Database migrations will run automatically on startup.

## Documentation

- [Party Mode Guide](/party-mode/)
- [Jukebox Setup](/jukebox/)
- [Podcast Configuration](/podcasts/)
- [Theming Guide](/theming/)
- [Charts Configuration](/charts/)

## Thank You

Thanks to everyone who contributed to this release through bug reports, feature requests, and pull requests!

Questions or feedback? Join our [Discord community](https://discord.gg/bfMnEUrvbp) or open an issue on [GitHub](https://github.com/melodee-project/melodee/issues).

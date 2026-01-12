---
layout: page
title: Melodee
permalink: /
---
# Melodee Music System

Melodee is a self‑hosted music management & streaming platform. It ingests disorganized audio files; cleans, normalizes & enriches metadata; stages human edits; then serves a pristine library over both the OpenSubsonic protocol and a native JSON REST API.

Think of it as a blend of:

- A streaming server (e.g. [Navidrome](https://github.com/navidrome/navidrome))
- A tag & artwork editor (e.g. [Mp3Tag](https://www.mp3tag.de/en/))
- An automated metadata enrichment + library quality pipeline

## Demo Server
Experience Melodee before installing! Our official demo server is available at:

**🎧 [https://demo.melodee.org](https://demo.melodee.org)**

- **Username**: `demo`
- **Password**: `melodee`

This server has sample permissively licensed music files for testing purposes. The demo server resets periodically, and data resets every 24 hours.

## End‑to‑End Flow

1. Inbound scan detects new files in the inbound volume.
2. Ingestion converts/transcodes (if needed), normalizes tags, applies regex cleanup & validation.
3. Items move to Staging for optional manual metadata & artwork curation.
4. Approved items are published into one or more Storage Libraries.
5. Indexed metadata powers fast search, browsing & streaming via APIs.

## Feature Highlights

- Media normalization & configurable tag rewrite rules
- Regex driven cleanup (featuring/with removal, numbering fixes, stray tokens)
- Multi‑stage pipeline (Inbound ➜ Staging ➜ Storage)
- Pluggable metadata & artwork fetch (MusicBrainz local cache, Last.FM, Spotify, iTunes, Deezer)
- Real‑time transcoding (MP3, Ogg, Opus, etc.) with range & partial streaming
- Cron‑like job scheduler (scans, enrichment, cleanup, background sync)
- Multi‑library federation (spread storage across NAS / mounts)
- Blazor Server UI for metadata, artwork, users, config & monitoring
- [Party Mode](/party-mode/) - Collaborative listening with shared queues
- [Jukebox](/jukebox/) - Server-side audio playback via MPV/MPD
- [Podcasts](/podcasts/) - Subscribe, download, and stream podcasts
- [Custom Theming](/theming/) - Personalize colors, fonts, and branding
- [Music Charts](/charts/) - Curated album charts from Billboard and more
- [Scrobbling](/scrobbling/) - Last.fm integration for play tracking
- OpenSubsonic & Jellyfin API compatibility
- Native REST API (versioned) for custom integrations
- User features: starring, ratings, playlists, play history

## Tested OpenSubsonic Clients

- Airsonic (refix)
- Dsub
- Feishin
- Symphonium
- Sublime Music
- Supersonic
- Ultrasonic

## Quick Links

- [Installation](/installing/) - Get Melodee up and running
- [Configuration](/configuration/) - Tune settings for your environment
- [Libraries](/libraries/) - Understand the library concept
- [API Reference](/api/) - OpenSubsonic + Native API documentation
- [News](/news/) - Changelog and announcements
- [About](/about/) - Project direction and philosophy

## Contributing

Found a gap or want to propose an improvement? Open a discussion or issue: {{ site.repo }}/issues — Documentation PRs are especially welcome.

---

Happy streaming! 🎵

# Melodee Market Comparison & Opportunities

## Scope and inputs

This document compares Melodee’s current feature set to other common self-hosted music streaming servers and identifies opportunities to:

- Close feature gaps that block adoption.
- Create differentiated capabilities that competitors struggle to match.

### Where Melodee is behind typical expectations

- **Podcasts / radio / “all audio types”** parity with Airsonic-Advanced / LMS ecosystems.
- **Whole-home playback endpoints** (AirPlay/Chromecast/Snapcast/Sonos-style) compared to OwnTone/LMS/Volumio.
- **Out-of-the-box ecosystem visibility** (e.g., Navidrome has widely-known “simple server” positioning and managed hosting options).

## Opportunities (prioritized)

### 1) Close feature gaps that block switching

#### 1.1 Add podcasts as an optional module (gap-closer)

- Why it matters
  - Airsonic-Advanced and several “audio hub” servers include podcasts; users migrating expect it.
  - Even if Melodee stays “music-first,” a podcasts module reduces reasons to run a second server.
- Scope idea
  - Treat podcasts as a separate library type + ingestion pipeline stage (RSS ingestion → episodes → playback).
  - Expose via: native API + (optionally) OpenSubsonic podcast endpoints.

#### 1.2 Jukebox / Party mode (gap-closer + differentiator if done well)

- Why it matters
  - Subsonic ecosystem has “jukebox” semantics; users also search for “party mode” / shared playback.
- Differentiated take
  - Make **Party Mode** the primary feature: shared sessions + shared queue + a designated playback endpoint (web player first).
  - Keep server-side playback optional via explicitly configured backends (Snapcast/MPD/Chromecast/etc.).
  - See: `design/requirements/JUKEBOX-REQUIREMENTS.md` (ADR-0007).

#### 1.3 Internet radio: clarify the gap and close it

Melodee already has **Radio Stations** today; what’s missing is the broader “internet radio experience” users expect from Subsonic-style servers.

- What Melodee does today (already implemented)
  - **A curated station list**: `RadioStation` records with `Name`, `StreamUrl`, and `HomePageUrl`.
  - **Admin-managed catalog**
    - Blazor page: `/data/radiostations` (list/search/delete; stations can be locked).
    - OpenSubsonic endpoints exist and work: `getInternetRadioStations`, `createInternetRadioStation`, `updateInternetRadioStation`, `deleteInternetRadioStation`.
    - Mutation is **admin-only** in the OpenSubsonic implementation.
  - **Global stations, not personal stations**: OpenSubsonic returns the full station list (shared across users), not a per-user library.

- Define the difference
  - **Radio Stations (what Melodee has)**: “A stored URL list” that clients can treat as playable items.
  - **Internet Radio (what users mean)**: a complete feature area around *discovery, metadata, reliability, and personalization* for live streams.

- What Melodee does not do today (the missing features)
  - **Discovery and sourcing**
    - No built-in directory/search (e.g., Icecast/SHOUTcast-style directories) and no OPML/import workflows.
    - No “curated packs” (genre/country/language presets) that make radio feel turnkey.
  - **Live metadata (“Now Playing”)**
    - No parsing/exposure of ICY metadata / Ogg/Vorbis comments for current track/program.
    - No station-level now-playing history.
  - **Station UX completeness**
    - No logo/artwork fetching/caching.
    - No tags/genre/country fields surfaced as first-class station facets (even if tags exist at the data-model level, the experience isn’t positioned as internet radio).
  - **Reliability/ops**
    - No health checks to detect broken streams, excessive redirects, TLS issues, geoblocks.
    - No monitoring/alerting and no “station last ok / last error” diagnostics.
  - **Personalization**
    - No per-user favorites/pins ordering for stations.
    - No per-user visibility (hide stations not relevant to a user).
  - **Playback proxying/transcoding**
    - No server-side proxy option to normalize streams (TLS quirks, headers, timeouts) or transcode radio to a known-good format/bitrate.
    - No buffering/time-shifting/recording (optional, but common in “audio hub” servers).

- Why it matters
  - Airsonic-Advanced and many OpenSubsonic clients expect at least “stations + metadata + a stable playback story.”
  - Clarifying this prevents Melodee from underselling what it already supports while still naming the real gaps.

- Suggested path to close the gap (incremental)
  1. **Position and polish existing capability**: make add/edit flows obvious in the UI; add per-user favorites/pins.
  2. **Add station health + now-playing**: periodic probe job + metadata capture surfaced in UI and (optionally) OpenSubsonic responses.
  3. **Optional discovery layer**: add import and a lightweight directory integration (even curated presets go a long way).
  4. **Optional proxy/transcode**: a “radio proxy” endpoint for web UI and problematic streams.

### 2) Double down on Melodee’s unique ingestion advantage

#### 2.1 “Library health” scoring + guided remediation

- Concept
  - Give every album/artist a health score (tags completeness, art quality, replaygain availability, loudness scan status, duplicate suspicion).
  - Provide a remediation queue (“Fix these 25 albums to improve overall library quality from 71 → 85”).
- Why it wins
  - Competitors largely stop at “scan and show.” Melodee can be the *quality* product.

#### 2.2 Duplicate detection and merge workflow (explicitly called out as future)

- Go beyond basic detection
  - Cluster by (MusicBrainz ID, Spotify ID, normalized artist/album/title, duration fingerprint).
  - Offer merge tooling and “choose canonical release” UX.
- Adoption impact
  - Large library users churn when duplicates and variants get messy.

#### 2.3 Ingestion “profiles” per source

- Examples
  - “Bandcamp downloads profile” (filename patterns, common tag cleanup).
  - “Soulseek profile” (aggressive normalization).
  - “Vinyl rip profile” (artwork and track numbering heuristics).
- Why it wins
  - Makes Melodee feel magical for the real-world messy library problem.

### 3) Reduce adoption friction (distribution and onboarding)

#### 3.1 Managed hosting option / one-click deployments

- Navidrome explicitly partners with a managed hosting provider; that reduces barrier-to-entry.
- Options
  - Official images + turnkey Helm chart + “supported” PaaS install guide.
  - A hosted “Melodee Cloud” partner program for community hosting.

#### 3.2 “First 15 minutes” onboarding path

- Make the first-run experience unmissable:
  - Add library paths wizard + recommended defaults.
  - A guided demo dataset import (optional).
  - Explain inbound/staging/storage with a single screen and a progress bar.

#### 3.3 Opinionated presets for common homelab hardware

- Example profiles
  - Raspberry Pi + USB SSD
  - NAS storage + small compute node
  - “Big iron” large library
- Outcome
  - Better defaults reduce the “it’s slow / why isn’t it scanning?” early churn.

### 4) Expand “community library” features (Melodee’s social edge)

#### 4.1 Requests as a full workflow (household/club mode)

- Additions
  - Voting/upvotes on requests.
  - Request “bounties” (not money—just priority points).
  - SLA/assignment and notifications.
- Why it wins
  - Makes Melodee feel purpose-built for families, friend groups, and clubs.

#### 4.2 Collaborative playlists (with guardrails)

- Features
  - Playlist roles (owner/editor/viewer).
  - “Event playlist” mode: anyone with link can suggest tracks.
- Adoption impact
  - Competes with the *social* part of commercial platforms.

### 5) Differentiation via analytics and discovery

#### 5.1 Personal and household listening analytics

- Examples
  - “Year in review,” “most played,” “discovery from friends,” “newly added you haven’t played.”
- Why it wins
  - Many servers track plays, but few turn it into delightful product surfaces.

#### 5.2 Smart radios / stations powered by MQL

- Concept
  - A “station” is an MQL query + a shuffle/decay strategy.
  - Expose as a first-class object (shareable, subscribable).
- This leverages an existing Melodee advantage (MQL) into a flagship feature.

## Suggested positioning (how to sell Melodee today)

- **“The only self-hosted music server with a real ingestion pipeline.”**
  - Drop zone + staging review + automated promotion.
- **“Runs your household music like a service.”**
  - Requests, shares, automation, multi-API compatibility.
- **“Bring your clients.”**
  - Subsonic + Jellyfin client ecosystems supported, plus native API for first-party apps.

## Recommended next bets (highest ROI)

1. Duplicate detection + merge workflow (ties directly to ingestion pipeline value).
2. Library health scoring + remediation queue (turns “pipeline” into a daily-use advantage).
3. Requests enhancements (voting/notifications) to lean into shared-library adoption.
4. Internet radio (quick completeness win).
5. Podcasts (optional module) to remove a common “I still need X” blocker.

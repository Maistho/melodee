<div align="center">
  <img src="graphics/melodee_logo.png" alt="Melodee Logo" height="120px" />

  # Melodee

  **A music system designed to manage and stream music libraries with tens of millions of songs**

  [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
  [![.NET](https://github.com/sphildreth/melodee/actions/workflows/dotnet.yml/badge.svg)](https://github.com/sphildreth/melodee/actions/workflows/dotnet.yml)
  [![CodeQL](https://github.com/sphildreth/melodee/actions/workflows/codeql.yml/badge.svg)](https://github.com/sphildreth/melodee/actions/workflows/codeql.yml)
  [![Discord](https://img.shields.io/discord/1337921126210211943)](https://discord.gg/bfMnEUrvbp)

  [Features](#features) • [Quick Start](#quick-start) • [Documentation](#documentation) • [Contributing](#contributing) • [Support](#support)
</div>

---

## 🎵 Overview

Melodee is a comprehensive music management and streaming system built with .NET 10 and Blazor. It provides a complete solution for processing, organizing, and serving large music libraries through both RESTful and OpenSubsonic-compatible APIs.

Designed with homelab enthusiasts in mind, Melodee runs efficiently on a wide range of hardware from single-board computers like Raspberry Pi to full server setups, making it perfect for self-hosted music streaming in home environments.

### Key Capabilities

- **📁 Smart Media Processing**: Automatically converts, cleans, and validates inbound media
- **🎛️ Staging Workflow**: Optional manual editing before adding to production libraries
- **⚡ Automatic Ingestion**: Drop files → play music (validated albums flow through automatically)
- **🔄 Automated Jobs**: Cron-based scheduling with intelligent job chaining
- **📝 User Requests**: Submit and track requests for missing albums/songs, with automatic completion when matches are detected
- **🎵 OpenSubsonic API**: Compatible with popular Subsonic and OpenSubsonic clients
- **🌐 Melodee API**: Fast RESTful API for custom integrations
- **🌐 Modern Web UI**: Blazor Server interface with Radzen UI components
- **🐳 Container Ready**: Full Docker/Podman support with PostgreSQL

![Melodee Web Interface](graphics/Snapshot_2025-02-04_23-06-24.png)

## 🎶 Music Ingestion Pipeline

Melodee features a fully automated music ingestion pipeline. Simply drop your music files into the inbound folder and they'll be processed, validated, and made available for streaming—typically within 15-20 minutes.

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│     INBOUND     │     │     STAGING     │     │     STORAGE     │     │    DATABASE     │
│   (Drop zone)   │────▶│    (Review)     │────▶│   (Published)   │────▶│   (Playable)    │
│                 │     │                 │     │                 │     │                 │
│  Drop files     │     │  Validated &    │     │  Final home     │     │  Indexed &      │
│  here           │     │  ready albums   │     │  for music      │     │  streamable     │
└─────────────────┘     └─────────────────┘     └─────────────────┘     └─────────────────┘
        │                       │                       │                       │
        ▼                       ▼                       ▼                       ▼
  LibraryInbound          StagingAuto             LibraryInsert            API Clients
    ProcessJob             MoveJob                   Job                   can stream
   (every 10 min)        (every 15 min)          (chains auto)
```

### How It Works

| Step | What Happens | Automatic? |
|------|--------------|------------|
| **1. Drop** | Place music files (MP3, FLAC, etc.) in the inbound folder | You do this |
| **2. Process** | Files are scanned, metadata extracted, validated, and moved to staging | ✅ Automatic |
| **3. Review** | Albums marked "Ok" are automatically promoted; others await manual review | ✅ Automatic for valid albums |
| **4. Move** | Validated albums move from staging to storage library | ✅ Automatic |
| **5. Index** | Albums in storage are indexed into the database | ✅ Automatic |
| **6. Stream** | Music is available via OpenSubsonic API and web player | Ready to play! |

### Automatic vs Manual Mode

**Automatic Mode (Default)**: Jobs chain together—when one completes successfully, it triggers the next. Well-tagged music flows from inbound to playable without intervention.

**Manual Mode**: Trigger jobs individually from the admin UI for troubleshooting or when you want to review albums before promotion. Manual triggers don't chain, giving you full control.

### When Manual Review is Needed

Albums that don't pass validation (missing tags, artwork issues, etc.) stay in staging for manual review. You can:
- Edit metadata and artwork in the web UI
- Mark albums as "Ok" when ready
- Use the "Move Ok" button to promote them
- Delete albums that shouldn't be imported

## 🚀 Quick Start

### Prerequisites

- [Podman](https://podman.io/) or Docker
- [Podman Compose](https://github.com/containers/podman-compose) (for Podman users)

### 🐍 Automated Setup (Recommended)

For a fully automated setup, run our Python setup script:

```bash
# Download and run the setup script
curl -O https://raw.githubusercontent.com/melodee-project/melodee/main/scripts/setup_melodee.py
python3 scripts/setup_melodee.py
```

The script will:
- Check for required dependencies (Git, Docker/Podman)
- Clone the Melodee repository (if not already present)
- Generate a secure environment configuration
- Build and start the containers automatically
- Wait for the service to be healthy
- Provide you with the URL to access the Blazor Admin UI

### 🐳 Manual Deploy with Podman

1. **Clone the repository**
   ```bash
   git clone https://github.com/melodee-project/melodee.git
   cd melodee
   ```

2. **Configure environment variables**
   ```bash
   # Copy and edit the example environment file
   cp example.env .env
   nano .env
   ```

   Update the following variables in `.env`:
   ```bash
   # Database password (change this!)
   DB_PASSWORD=your_secure_password_here

   # Port configuration
   MELODEE_PORT=8080
   ```

3. **Deploy the application**
   ```bash
   # Using Podman Compose
   podman-compose up -d

   # Or using Docker Compose
   docker compose up -d
   ```

4. **Access the application**
   - 📚 Read the [documentation](https://melodee.org) on how to get started
   - Web Interface: http://localhost:8080
   - Register as a new user, the first user will be setup as administrator
   - Configure clients to connect to your server
   - Enjoy an ad-free and self-hosted music streaming service 🎉

### 📦 Updating Melodee

To update to the latest version:

```bash
# Stop the current deployment
podman-compose down

# Pull the latest changes
git pull origin main

# Rebuild and restart
podman-compose up -d --build
```

> **Note**: Database migrations are handled automatically during container startup.

## 🏠 Homelab Deployment

Melodee is designed to run in homelab environments with support for various hardware configurations:

- **Single Board Computers**: Raspberry Pi 4/5, Rock 5B, Odroid N2+
- **Home Servers**: Intel NUC, custom builds, used desktops
- **NAS Integration**: Mount external storage for large music collections
- **Container Orchestration**: Docker Compose, Podman Compose, or Docker Swarm

For detailed homelab deployment guides, check out our [documentation](https://melodee.org).

### 🗂️ Volume Management

Melodee uses several persistent volumes for data storage:

| Volume | Purpose | Description                                 |
|--------|---------|---------------------------------------------|
| `melodee_storage` | Music Library | Processed and organized music files         |
| `melodee_inbound` | Incoming Media | New media files to be processed             |
| `melodee_staging` | Staging Area | Media ready for manual review               |
| `melodee_user_images` | User Content | User-uploaded avatars            |
| `melodee_playlists` | Playlists | Admin defined (json based) dynamic playlists |
| `melodee_db_data` | Database | PostgreSQL data                             |

To backup your data:
```bash
# Backup volumes
podman volume export melodee_storage > melodee_storage_backup.tar
podman volume export melodee_db_data > melodee_db_backup.tar

# Restore volumes
podman volume import melodee_storage melodee_storage_backup.tar
podman volume import melodee_db_data melodee_db_backup.tar
```

## ✨ Features

### 🎛️ Media Processing Pipeline

1. **Inbound Processing**
   - Converts media to standard formats
   - Applies regex-based metadata cleanup rules
   - Validates file integrity and metadata completeness
   - Extracts and validates album artwork

2. **Staging Management**
   - Automatic promotion of validated ("Ok") albums
   - Manual editing of metadata for albums needing review
   - Album art management and replacement
   - Quality control workflow with status tracking

3. **Production Libraries**
   - Automated scanning and indexing
   - Multiple storage library support
   - Real-time database updates
   - Artist, album, and song relationship management

### 🔄 Background Jobs

Melodee includes a comprehensive job scheduling system powered by Quartz.NET:

| Job | Purpose | Default Schedule |
|-----|---------|------------------|
| **LibraryInboundProcessJob** | Scans inbound, processes files to staging | Every 10 minutes |
| **StagingAutoMoveJob** | Moves "Ok" albums from staging to storage | Every 15 minutes (+ chained) |
| **LibraryInsertJob** | Indexes storage albums into database | Daily at midnight (+ chained) |
| **ArtistHousekeepingJob** | Cleans up artist data and relationships | Daily at midnight |
| **ArtistSearchEngineHousekeepingJob** | Updates artist search index | Daily at midnight |
| **ChartUpdateJob** | Links chart entries to albums | Daily at 2 AM |
| **MusicBrainzUpdateDatabaseJob** | Updates local MusicBrainz cache | Monthly |
| **NowPlayingCleanupJob** | Cleans stale now-playing entries | Every 5 minutes |

Jobs can be manually triggered, paused, or monitored from the admin UI at `/admin/jobs`.

### 🔌 Plugin Architecture

- **Media Format Support**: AAC, AC3, M4A, FLAC, OGG, APE, MP3, WAV, WMA, and more
- **Metadata Sources**: iTunes, Last.FM, MusicBrainz, Spotify, Brave Search
- **File Parsers**: NFO, M3U, SFV, CUE sheet metadata files

### 🌍 Multi-Language Support

Melodee features comprehensive localization with support for 10 languages:

| Language | Code | RTL Support |
|----------|------|-------------|
| English (US) | en-US | - |
| Arabic | ar-SA | ✅ |
| Chinese (Simplified) | zh-CN | - |
| French | fr-FR | - |
| German | de-DE | - |
| Italian | it-IT | - |
| Japanese | ja-JP | - |
| Portuguese (Brazil) | pt-BR | - |
| Russian | ru-RU | - |
| Spanish | es-ES | - |

- **Language Selector**: Easy switching via dropdown in the header
- **Preference Persistence**: Your language choice is saved in browser storage
- **RTL Support**: Full right-to-left layout support for Arabic

### 🌐 OpenSubsonic API

Full compatibility with Subsonic 1.16.1 and OpenSubsonic specifications:

- Real-time transcoding (including OGG and Opus)
- Playlist management
- User authentication and permissions
- Album art and metadata serving
- Scrobbling support (Last.fm)

#### Tested Clients

- [MeloAmp (desktop)](https://github.com/melodee-project/meloamp)
- [Melodee Player (Android Auto)](https://github.com/melodee-project/melodee-player)
- [Airsonic (refix)](https://github.com/tamland/airsonic-refix)
- [Dsub](https://github.com/DataBiosphere/dsub)
- [Feishin](https://github.com/jeffvli/feishin)
- [Symphonium](https://symfonium.app/)
- [Sublime Music](https://github.com/sublime-music/sublime-music)
- [Supersonic](https://github.com/dweymouth/supersonic)
- [Ultrasonic](https://gitlab.com/ultrasonic/ultrasonic)

### 🔐 Authentication

Melodee supports multiple authentication methods:

- **Username/Password**: Traditional authentication with JWT tokens
- **Google Sign-In**: OAuth 2.0 authentication via Google (configurable)

#### API Authentication

All API endpoints (except public endpoints) require a Bearer token:

```bash
curl -H "Authorization: Bearer <JWT_TOKEN>" https://your-server/api/v1/albums
```

#### Token Refresh

Melodee implements secure token rotation:
- **Access tokens**: Short-lived (15 minutes default)
- **Refresh tokens**: Long-lived (30 days default) with automatic rotation

#### Google Sign-In Configuration

To enable Google Sign-In, configure the following in `appsettings.json`:

```json
{
  "Auth": {
    "Google": {
      "Enabled": true,
      "ClientId": "your-google-client-id.apps.googleusercontent.com",
      "AllowedHostedDomains": [],
      "AutoLinkEnabled": false
    },
    "SelfRegistrationEnabled": true
  }
}
```

For API documentation including authentication endpoints, access `/scalar/v1` on a running instance. Download the OpenAPI specification at `/openapi/v1.json`. For OpenSubsonic API, refer to the [OpenSubsonic specification](https://opensubsonic.netlify.app/).

## 🏗️ Architecture

### Components

| Component | Description | Technology |
|-----------|-------------|------------|
| **Melodee.Blazor** | Web UI and OpenSubsonic API server | Blazor Server, Radzen UI |
| **Melodee.Cli** | Command-line interface | .NET Console App |
| **Melodee.Common** | Shared libraries and services | .NET Class Library |

### System Requirements

- **.NET 10.0** or later
- **PostgreSQL 17** (included in container deployment)
- **2GB RAM** minimum (4GB recommended)
- **Storage**: Varies based on music library size

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/melodee-project/melodee.git
   cd melodee
   ```

2. **Install .NET 10 SDK**
   ```bash
   # Follow instructions at https://dotnet.microsoft.com/download
   ```

3. **Run locally**
   ```bash
   dotnet run --project src/Melodee.Blazor
   ```

### Code of Conduct

This project adheres to the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md).

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 💬 Support

- **Melodee Music System**: [Home](https://melodee.org)
- **Discord**: [Join our community](https://discord.gg/bfMnEUrvbp)
- **Issues**: [GitHub Issues](https://github.com/sphildreth/melodee/issues)
- **Discussions**: [GitHub Discussions](https://github.com/sphildreth/melodee/discussions)

## 📚 Documentation

For comprehensive documentation, including installation guides, configuration options, homelab deployment strategies, and API references, visit [https://www.melodee.org](https://www.melodee.org).

## 🙏 Acknowledgments

- Built with [.NET 10](https://dotnet.microsoft.com/)
- UI powered by [Radzen Blazor Components](https://blazor.radzen.com/)
- Job scheduling by [Quartz.NET](https://www.quartz-scheduler.net/)
- Compatible with [OpenSubsonic](https://opensubsonic.netlify.app/) specification
- Music metadata from [MusicBrainz](https://musicbrainz.org/), [Last.FM](https://last.fm/), [Spotify](https://spotify.com/), and [Brave Search](https://brave.com/search/api/)

---

<div align="center">
  Made with ❤️ by the Melodee community
</div>

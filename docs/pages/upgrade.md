---
title: Upgrade Guide
permalink: /upgrade/
---

# Upgrade Guide

This guide covers upgrading Melodee from any previous version to the latest release. Melodee is designed for seamless upgrades with automatic database migrations.

## Before You Upgrade

### 1. Check Current Version

```bash
# Via API
curl -s http://localhost:8080/api/v1/system/info | jq .version

# Or check the UI footer
```

### 2. Review Release Notes

Check the [News](/news/) page or [GitHub Releases](https://github.com/melodee-project/melodee/releases) for:
- Breaking changes
- New required configuration
- Deprecated features
- Known issues

### 3. Backup Your Data

**Always backup before upgrading, especially for major version changes.**

```bash
# Quick backup of critical volumes
BACKUP_DIR="melodee-backup-$(date +%Y%m%d)"
mkdir -p "$BACKUP_DIR"

# Database (most critical)
podman volume export melodee_db_data > "$BACKUP_DIR/db_data.tar"

# User data
podman volume export melodee_playlists > "$BACKUP_DIR/playlists.tar"
podman volume export melodee_user_images > "$BACKUP_DIR/user_images.tar"

# Configuration
cp .env "$BACKUP_DIR/.env.backup"
```

See [Backup & Recovery](/backup/) for comprehensive backup strategies.

## Upgrade Methods

### Method 1: Using the Setup Script (Recommended)

The safest and easiest way to upgrade:

```bash
cd /path/to/melodee
git fetch --tags
git checkout v1.8.0  # Or: git pull origin main for latest
python3 scripts/run-container-setup.py --update
```

The script will:
1. Show current container status
2. Build a fresh image with the latest code
3. Stop existing containers gracefully
4. Start new containers with updated image
5. Run database migrations automatically
6. Wait for health checks to pass
7. Report success or any issues

**For automated/CI deployments:**

```bash
git pull && python3 scripts/run-container-setup.py --update --yes
```

### Method 2: Manual Docker/Podman Commands

```bash
cd /path/to/melodee

# Get latest code
git fetch --tags
git checkout v1.8.0  # Specific version
# Or: git pull origin main  # Latest development

# Rebuild image
podman compose build --no-cache
# Or: docker compose build --no-cache

# Recreate containers (data preserved)
podman compose down
podman compose up -d
# Or: docker compose down && docker compose up -d

# Verify health
podman compose ps
curl -s http://localhost:8080/api/v1/system/info
```

### Method 3: Using Pre-built Images (Coming Soon)

```bash
# Pull latest image from registry
docker pull ghcr.io/melodee-project/melodee:1.8.0

# Update compose.yml to use the image
# Then recreate containers
docker compose down
docker compose up -d
```

## What Happens During Upgrade

### Automatic Database Migrations

Melodee uses Entity Framework Core migrations that run automatically on startup:

1. Container starts
2. `entrypoint.sh` runs the EF migration bundle
3. Migrations apply any schema changes
4. Application starts normally

**Migration is idempotent** - running it multiple times is safe. Already-applied migrations are skipped.

### Data Preservation

| Component | Location | Preserved? |
|-----------|----------|------------|
| Database | `melodee_db_data` volume | ✅ Yes |
| Music library | `melodee_storage` volume | ✅ Yes |
| Podcasts | `melodee_storage/podcasts` | ✅ Yes |
| Themes | `melodee_storage/themes` | ✅ Yes |
| User images | `melodee_user_images` volume | ✅ Yes |
| Playlists | `melodee_playlists` volume | ✅ Yes |
| Templates | `melodee_templates` volume | ✅ Yes |
| Logs | `melodee_logs` volume | ✅ Yes |
| `.env` config | Host filesystem | ✅ Yes |
| Container image | Replaced | ❌ Updated |

## Version-Specific Upgrade Notes

### Upgrading to 1.8.0

**From 1.7.x:**

1.8.0 introduces several new features with database schema changes:
- Party Mode tables
- Podcast support tables
- Theme library support
- Jukebox settings

**Steps:**
```bash
git checkout v1.8.0
python3 scripts/run-container-setup.py --update
```

**New configuration options (optional):**
- Podcast settings are disabled by default
- Party Mode is available immediately
- Jukebox requires MPV or MPD backend configuration
- Theme library is created automatically at `/storage/themes/`

**No manual intervention required** - all migrations run automatically.

### Upgrading to 1.7.x

**From 1.6.x:**

Standard upgrade process applies. No breaking changes.

## Troubleshooting Upgrades

### Migration Failures

If migrations fail to apply:

```bash
# Check container logs
podman compose logs melodee.blazor | grep -i "migration\|error"

# Manually run migrations (if needed)
podman compose exec melodee.blazor /app/efbundle --connection "Host=melodee-db;..."
```

### Container Won't Start

```bash
# Check for errors
podman compose logs -f

# Verify database is running
podman compose ps melodee-db

# Try recreating from scratch (data preserved)
podman compose down
podman compose up -d
```

### Health Check Failures

```bash
# Check health status
podman inspect melodee_melodee.blazor_1 --format='{{.State.Health.Status}}'

# View health check output
podman inspect melodee_melodee.blazor_1 --format='{{range .State.Health.Log}}{{.Output}}{{end}}'

# Common causes:
# - Database not ready yet (wait longer)
# - Port conflict (check MELODEE_PORT)
# - Memory issues (increase container limits)
```

### Rolling Back

If an upgrade causes issues:

```bash
# Stop current containers
podman compose down

# Checkout previous version
git checkout v1.7.6  # Or your previous version

# Rebuild and start
podman compose build
podman compose up -d
```

**Database rollback (if needed):**

```bash
# Restore database backup
podman compose down
podman volume rm melodee_db_data
podman volume create melodee_db_data
podman volume import melodee_db_data < backup/db_data.tar
podman compose up -d
```

## Upgrade Checklist

- [ ] Noted current version
- [ ] Reviewed release notes for target version
- [ ] Backed up database volume
- [ ] Backed up `.env` configuration
- [ ] Fetched latest code/tags
- [ ] Ran upgrade command
- [ ] Verified containers are healthy
- [ ] Verified version via API/UI
- [ ] Tested basic functionality (login, browse, play)
- [ ] Checked logs for any warnings

## Automated Upgrades

For homelabs that want automatic updates, consider:

### Watchtower (for pre-built images)

```yaml
services:
  watchtower:
    image: containrrr/watchtower
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
    command: --interval 86400 melodee.blazor
```

### Scheduled Script

```bash
#!/bin/bash
# /etc/cron.weekly/melodee-upgrade

cd /path/to/melodee
git pull origin main
python3 scripts/run-container-setup.py --update --yes

# Notify on failure
if [ $? -ne 0 ]; then
    echo "Melodee upgrade failed" | mail -s "Upgrade Alert" admin@example.com
fi
```

## Getting Help

- [Discord Community](https://discord.gg/bfMnEUrvbp) - Quick help from the community
- [GitHub Issues](https://github.com/melodee-project/melodee/issues) - Bug reports
- [GitHub Discussions](https://github.com/melodee-project/melodee/discussions) - Questions and feature requests

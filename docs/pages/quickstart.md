---
title: Quick Start for Homelabs
permalink: /quickstart/
---

# Quick Start Guide for Homelab Enthusiasts

This guide provides a fast path to getting Melodee running in your homelab environment with best practices for home deployments.

## Prerequisites

Before starting, ensure you have:

- A server or SBC with Docker/Podman installed
- At least 4GB RAM (8GB+ recommended for large collections)
- Sufficient storage for your music library
- Basic knowledge of Docker and container management
- A domain name or subdomain (optional but recommended)

## Step 1: Prepare Your Environment

### System Preparation
```bash
# Update your system
sudo apt update && sudo apt upgrade -y

# Install Docker (Ubuntu/Debian)
sudo apt install docker.io docker-compose -y
sudo usermod -aG docker $USER
newgrp docker

# Or install Podman (alternative)
sudo apt install podman podman-compose -y
```

### Storage Setup
For homelab deployments, prepare your music storage:

```bash
# Create directories for your music (if using bind mounts instead of volumes)
mkdir -p ~/melodee/{storage,inbound,staging,user-images,playlists}
```

## Step 2: Deploy Melodee

### Automated Deployment (Recommended)
For the easiest setup, use our Python setup script which automates the entire process:

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

### Manual Deployment
If you prefer to set up manually:

#### Clone and Configure
```bash
# Clone the repository
git clone https://github.com/sphildreth/melodee.git
cd melodee

# Copy environment file
cp example.env .env

# Edit the environment file with your preferences
nano .env
```

Key settings for homelab deployment:
- Set a strong `DB_PASSWORD`
- Adjust `MELODEE_PORT` if needed
- Consider increasing `DB_MIN_POOL_SIZE` and `DB_MAX_POOL_SIZE` for better performance

#### Start the Services
```bash
# Using Docker Compose
docker-compose up -d

# Or using Podman Compose
podman-compose up -d
```

## Step 3: Initial Configuration

### Access the Web Interface
1. Open your browser and navigate to `http://your-server-ip:8080` (or your configured port)
2. Create your first user account (this will be the administrator)
3. Log in with your new credentials

### Configure Library Paths
1. Navigate to the Configuration section
2. Set up your library paths:
   - **Inbound**: Where you'll place new music files
   - **Staging**: Where files go after initial processing
   - **Storage**: Where final processed music is stored

### Set Up Metadata Providers
1. Go to the metadata configuration
2. Add API keys for providers you want to use:
   - MusicBrainz (no API key needed)
   - Last.FM (free API key)
   - Spotify (free API key)
   - iTunes (no API key needed)
   - Brave Search (for artwork, requires API key)

## Step 4: Homelab Optimization

### Reverse Proxy Setup (Recommended)
For homelab deployments, set up a reverse proxy with SSL:

**Nginx Proxy Manager:**
1. Deploy Nginx Proxy Manager alongside Melodee
2. Create a proxy host for your Melodee instance
3. Enable SSL with Let's Encrypt

**Traefik:**
Add labels to your compose file:
```yaml
services:
  melodee.blazor:
    # ... existing config ...
    labels:
      - "traefik.enable=true"
      - "traefik.http.routers.melodee.rule=Host(`music.yourdomain.com`)"
      - "traefik.http.routers.melodee.tls=true"
      - "traefik.http.routers.melodee.entrypoints=websecure"
```

### Hardware Optimization
Based on your homelab hardware:

**For SBCs (Raspberry Pi, etc.):**
- Limit concurrent transcoding jobs
- Use lower quality transcoding settings
- Monitor temperature during processing

**For Full Servers:**
- Increase database connection pool sizes
- Enable more concurrent processing jobs
- Configure transcoding for higher quality output

## Step 5: Add Your Music

### Initial Music Import
1. Place your music files in the **inbound** directory
2. Go to the Jobs section in the web interface
3. Start a manual scan to process your music
4. Monitor the staging area for files that need manual review
5. Approve and promote files to storage

### Ongoing Music Management
- New music goes to the inbound directory
- Processing happens automatically based on your schedule
- Review staging items regularly for manual metadata fixes

## Step 6: Configure Clients

### OpenSubsonic Compatible Clients
Melodee is compatible with many existing Subsonic clients:
- **MeloAmp**: Official desktop client
- **Dsub**: Android client
- **Sublimemusic**: Cross-platform client
- **Feishin**: Modern client with web-based UI

### Client Configuration
- **Server URL**: Your Melodee server address (e.g., `https://music.yourdomain.com`)
- **Username/Password**: Your Melodee account credentials
- **API Path**: Usually auto-detected, but may need to be set to `/api/rest` for OpenSubsonic compatibility

## Step 7: Set Up Automation

### Backup Strategy
Create a backup script for your homelab:

```bash
#!/bin/bash
# backup-melodee.sh

BACKUP_DIR="/backup/melodee/$(date +%Y-%m-%d)"
mkdir -p "$BACKUP_DIR"

# Stop services briefly for consistent backup
docker-compose down

# Backup database
docker exec melodee-db pg_dump -U melodeeuser -d melodeedb | gzip > "$BACKUP_DIR/db_backup.sql.gz"

# Export volumes
docker run --rm -v melodee_storage:/volume -v "$BACKUP_DIR:/backup" alpine tar czf /backup/storage.tar.gz -C /volume .
docker run --rm -v melodee_user_images:/volume -v "$BACKUP_DIR:/backup" alpine tar czf /backup/user_images.tar.gz -C /volume .
docker run --rm -v melodee_playlists:/volume -v "$BACKUP_DIR:/backup" alpine tar czf /backup/playlists.tar.gz -C /volume .

# Start services
docker-compose up -d

echo "Backup completed: $BACKUP_DIR"
```

Schedule with cron:
```bash
# Daily backup at 2 AM
0 2 * * * /path/to/backup-melodee.sh
```

### Maintenance Jobs
Configure regular maintenance tasks:
- Database optimization
- Log rotation
- Storage cleanup

## Step 8: Monitoring

### System Monitoring
Monitor your homelab deployment with:

```bash
# Check service status
docker-compose ps

# Monitor resource usage
docker stats melodee-blazor melodee-db

# Check application logs
docker-compose logs -f melodee-blazor
```

### Health Checks
Melodee provides health endpoints:
- `/health` - Basic health check
- `/health/ready` - Readiness check for orchestration tools

## Troubleshooting Common Issues

### Performance Issues
- **Slow UI**: Check database connection settings and storage performance
- **High CPU**: Monitor transcoding jobs and adjust settings
- **Memory issues**: Increase container memory limits

### Connection Issues
- **Can't access web UI**: Check port configuration and firewall
- **Client can't connect**: Verify API compatibility and authentication

### Storage Issues
- **Running out of space**: Monitor volume sizes and configure cleanup
- **Slow music access**: Ensure storage volumes are on fast drives

## Next Steps

### Advanced Configuration
- Explore advanced metadata configuration
- Set up automated music import workflows
- Configure user permissions and sharing

### Community Resources
- Join the Discord community
- Check GitHub for updates and issues
- Share your homelab setup in the community forums

### Hardware Upgrades
- Consider SSD for database volume
- Add more RAM for larger collections
- Upgrade network for better streaming performance

This quick start guide should get your Melodee homelab deployment up and running. For more detailed information on specific topics, explore the other documentation sections.
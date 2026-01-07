---
title: Homelab Deployment
permalink: /homelab/
---

# Homelab Deployment Guide

This guide provides comprehensive instructions for deploying Melodee in a homelab environment, covering everything from basic setup to advanced configurations.

## Quick Start

For first-time setup, use the automated setup script:

```bash
# Clone the repository
git clone https://github.com/sphildreth/melodee.git
cd melodee

# Run the setup script (detects podman or docker automatically)
python3 scripts/run-container-setup.py

# Or setup and start containers in one step
python3 scripts/run-container-setup.py --start
```

The setup script will:
- Detect your container runtime (podman or docker)
- **Offer to install podman** if no container runtime is found (supports Debian/Ubuntu, Fedora, RHEL/CentOS, Arch, openSUSE, and macOS with Homebrew)
- Generate secure passwords and JWT tokens
- Create a properly configured `.env` file
- Optionally start the containers

After setup, access Melodee at `http://localhost:8080`

## System Requirements

### Minimum Requirements
- **CPU**: Dual-core processor (Intel/AMD x64 or ARM64)
- **RAM**: 2GB (4GB recommended for libraries with thousands of tracks)
- **Storage**: 100GB+ for application and database (additional space for music library)
- **Network**: 100 Mbps Ethernet (1 Gbps recommended for multi-user streaming)

### Recommended for Large Libraries
- **CPU**: Quad-core or higher (for parallel transcoding and scanning)
- **RAM**: 8GB+ (for efficient metadata processing)
- **Storage**: SSD for database volume, separate drives for media
- **Network**: Gigabit Ethernet or higher

## Hardware Recommendations

### Single-Board Computers (SBCs)
- **Raspberry Pi 4 (8GB)**: Suitable for small collections (<5,000 tracks)
- **Raspberry Pi 5**: Better performance for medium collections
- **Rock 5B/5E**: ARM64 with better performance than Pi
- **Odroid N2+**: Good for media processing

### NAS Integration
For homelabs with existing NAS:
- Mount music library via NFS/SMB to the `storage` volume
- Keep database on local SSD for performance
- Use the `inbound` volume on local storage for processing

### VM Setup
- **CPU**: Enable passthrough for transcoding acceleration
- **RAM**: Allocate based on library size
- **Storage**: Use separate virtual disks for each volume type

## Network Configuration

### Port Requirements
- **8080**: Default web interface and API (configurable via MELODEE_PORT)
- **80/443**: If using reverse proxy
- **32400**: If integrating with other media servers (optional)

### Firewall Rules
```bash
# Example iptables rules
iptables -A INPUT -p tcp --dport 80 -j ACCEPT
iptables -A INPUT -p tcp --dport 443 -j ACCEPT
iptables -A INPUT -p tcp --dport 8080 -j ACCEPT  # Only if not behind proxy
```

## Container Orchestration

### Docker Compose with Reverse Proxy

```yaml
name: melodee

services:
  nginx-proxy:
    image: nginxproxy/nginx-proxy
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - conf:/etc/nginx/conf.d
      - vhost:/etc/nginx/vhost.d
      - html:/usr/share/nginx/html
      - certs:/etc/nginx/certs:ro
      - /var/run/docker.sock:/tmp/docker.sock:ro
    restart: unless-stopped

  acme-companion:
    image: nginxproxy/acme-companion
    depends_on:
      - nginx-proxy
    volumes:
      - certs:/etc/nginx/certs:rw
      - acme:/etc/acme.sh
      - vhost:/etc/nginx/vhost.d
      - html:/usr/share/nginx/html
      - /var/run/docker.sock:/var/run/docker.sock:ro
    restart: unless-stopped

  melodee-db:
    image: docker.io/library/postgres:17
    environment:
      POSTGRES_DB: melodeedb
      POSTGRES_USER: melodeeuser
      POSTGRES_PASSWORD: ${DB_PASSWORD}
      PGUSER: melodeeuser
    volumes:
      - db_data:/var/lib/postgresql/data
    restart: unless-stopped
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U melodeeuser -d melodeedb"]
      interval: 10s
      timeout: 5s
      retries: 5

  melodee.blazor:
    image: melodee:latest
    build:
      context: .
      dockerfile: ${DOCKERFILE_PATH:-Dockerfile}
      tags:
        - "melodee:latest"
    depends_on:
      melodee-db:
        condition: service_healthy
    environment:
      - DB_PASSWORD=${DB_PASSWORD}
      - ConnectionStrings__DefaultConnection=Host=melodee-db;Port=5432;Database=melodeedb;Username=melodeeuser;Password=${DB_PASSWORD};Pooling=true;MinPoolSize=${DB_MIN_POOL_SIZE:-10};MaxPoolSize=${DB_MAX_POOL_SIZE:-50};SSL Mode=Disable;Include Error Detail=true
      - ConnectionStrings__MusicBrainzConnection=Data Source=/app/storage/_search-engines/musicbrainz/musicbrainz.db
      - ConnectionStrings__ArtistSearchEngineConnection=Data Source=/app/storage/_search-engines/artistSearchEngine.db;Cache=Shared
      - DB_MIN_POOL_SIZE=${DB_MIN_POOL_SIZE:-10}
      - DB_MAX_POOL_SIZE=${DB_MAX_POOL_SIZE:-50}
      - VIRTUAL_HOST=music.yourdomain.com
      - LETSENCRYPT_HOST=music.yourdomain.com
    volumes:
      - storage:/app/storage
      - inbound:/app/inbound
      - staging:/app/staging
      - user_images:/app/user-images
      - playlists:/app/playlists
      - templates:/app/templates
      - logs:/app/Logs
    restart: unless-stopped
    user: "0:0"
    entrypoint: ["/entrypoint.sh"]
    healthcheck:
      test: ["CMD-SHELL", "curl -fsS http://localhost:8080/health || exit 1"]
      interval: 30s
      timeout: 5s
      start_period: 60s
      retries: 3
    deploy:
      resources:
        limits:
          cpus: "1.00"
          memory: 1g

volumes:
  db_data:
    name: melodee_db_data
  storage:
    name: melodee_storage
  inbound:
    name: melodee_inbound
  staging:
    name: melodee_staging
  user_images:
    name: melodee_user_images
  playlists:
    name: melodee_playlists
  templates:
    name: melodee_templates
  logs:
    name: melodee_logs
  conf:
  vhost:
  html:
  certs:
  acme:
```

### Proxmox Deployment

For homelabs using Proxmox as their virtualization platform, Melodee can be deployed in several ways:

#### Option A: Container Deployment in LXC

Deploy Melodee using an LXC container with Docker support:

1. **Create an LXC container**:
   - Use Ubuntu/Debian template
   - Allocate 2-4 CPU cores, 4-8GB RAM
   - 20-100GB disk space (adjust based on needs)

2. **Install Docker in the container**:
   ```bash
   apt update && apt install -y docker.io docker-compose-plugin
   ```

3. **Deploy Melodee**:
   Follow the standard Docker Compose installation process inside the container.

#### Option B: VM with Docker

Create a dedicated VM for Melodee:

1. **Create VM specifications**:
   - 2-4 vCPUs
   - 4-8GB RAM
   - 100GB+ storage (SSD recommended for database)
   - Bridge network interface

2. **Install operating system**:
   - Ubuntu Server 22.04 LTS or later
   - Enable SSH access

3. **Install Docker and deploy**:
   ```bash
   # Install Docker
   curl -fsSL https://get.docker.com | sh
   systemctl enable docker
   usermod -aG docker $USER

   # Clone and deploy Melodee
   git clone https://github.com/sphildreth/melodee.git
   cd melodee
   
   # Run setup script to generate secure configuration
   python3 scripts/run-container-setup.py --start
   ```

#### Option C: Using Proxmox Backup Server (PBS)

For backup integration with Proxmox:

1. **Configure PBS for container backups**:
   - Set up backup schedules for Melodee containers
   - Include database volume in backup policies
   - Test restore procedures regularly

2. **Snapshot strategy**:
   - Create VM/container snapshots before major updates
   - Use Proxmox's built-in snapshot features
   - Coordinate with application-level backups

#### Proxmox-Specific Optimizations

**Resource Allocation**:
- Use dedicated CPU cores when possible
- Assign sufficient RAM for metadata processing
- Consider using SSD storage for database VM/container

**Network Configuration**:
- Use bridge networking for consistent IP addressing
- Configure port forwarding if running behind NAT
- Consider VLAN setup for media traffic isolation

**Storage Options**:
- Use local storage for database performance
- Mount network storage (CIFS/NFS) for music library
- Configure ZFS with compression for space efficiency

**Monitoring Integration**:
- Use Proxmox's built-in monitoring for system metrics
- Export application metrics via API calls
- Set up Proxmox notifications for critical events

### Docker Swarm Setup

For high availability in homelabs:

```bash
# Initialize swarm
docker swarm init

# Create overlay network
docker network create --driver overlay --attachable melodee-network

# Deploy as stack
docker stack deploy -c compose.yml melodee
```

## Media Library Management

### Mounting External Storage

For homelabs with large music collections:

```bash
# Mount NAS share to storage volume
sudo mount -t nfs your-nas:/music /mnt/music
docker volume create melodee_storage
# Then bind mount to container
```

### Volume Management

| Volume | Purpose | Backup Strategy |
|--------|---------|-----------------|
| `melodee_db_data` | Database | Daily dumps, off-site backup |
| `melodee_storage` | Processed music library | Incremental backup, off-site |
| `melodee_inbound` | New media for processing | Temporary, no backup needed |
| `melodee_staging` | Media awaiting approval | Periodic backup |
| `melodee_user_images` | User avatars | Backup with media |
| `melodee_playlists` | User playlists | Backup with media |
| `melodee_templates` | Email templates | Backup with configuration |
| `melodee_logs` | Application logs | Rotate and archive |

## Monitoring & Maintenance

### System Monitoring

**Disk Usage:**
```bash
# Monitor storage volumes
docker exec -it melodee.blazor df -h
# Check database size
docker exec -it melodee-db psql -U melodeeuser -d melodeedb -c "SELECT pg_size_pretty(pg_database_size('melodeedb'));"
```

**Resource Usage:**
```bash
# Monitor container resources
docker stats melodee.blazor melodee-db
```

### Automated Maintenance

**Cleanup Script:**
```bash
#!/bin/bash
# cleanup-melodee.sh

# Remove old containers
docker container prune -f

# Remove unused images
docker image prune -f

# Check disk usage
echo "Disk usage:"
docker system df

# Optional: Rotate logs
docker exec melodee.blazor logrotate -f /etc/logrotate.d/melodee
```

**Schedule with cron:**
```bash
# Weekly cleanup
0 3 * * 0 /path/to/cleanup-melodee.sh
```

### Health Checks

**System Status:**
```bash
# Check service health
docker compose ps
# Check application health endpoint
curl http://localhost:8080/health
```

## Backup & Recovery

### Backup Strategy

**Full Backup:**
```bash
#!/bin/bash
# full-backup.sh

BACKUP_DIR="/backup/melodee/$(date +%Y-%m-%d)"
mkdir -p "$BACKUP_DIR"

# Stop services to ensure consistency
docker compose down

# Export volumes
docker run --rm -v melodee_db_data:/volume -v "$BACKUP_DIR:/backup" alpine tar czf /backup/db_backup.tar.gz -C /volume .

# For other volumes
docker run --rm -v melodee_storage:/volume -v "$BACKUP_DIR:/backup" alpine tar czf /backup/storage_backup.tar.gz -C /volume .

# Start services
docker compose up -d

echo "Backup completed: $BACKUP_DIR"
```

**Incremental Backup:**
```bash
# Database only backup
docker exec melodee-db pg_dump -U melodeeuser -d melodeedb > /backup/melodee_db_$(date +%Y%m%d_%H%M%S).sql
```

### Recovery Process

1. **Stop services:** `docker compose down`
2. **Restore database:** 
   ```bash
   docker run --rm -v /backup:/backup -v melodee_db_data:/volume alpine tar xzf /backup/db_backup.tar.gz -C /volume
   ```
3. **Restore media:** 
   ```bash
   docker run --rm -v /backup:/backup -v melodee_storage:/volume alpine tar xzf /backup/storage_backup.tar.gz -C /volume
   ```
4. **Start services:** `docker compose up -d`

## Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| High CPU during initial scan | This is normal; scans run in background |
| Database connection errors | Check `DB_PASSWORD` in .env file |
| Container won't start | Run `docker logs melodee.blazor` for details |
| Slow streaming | Check network bandwidth and proxy buffer settings |
| Missing artwork | Ensure metadata providers are configured with API keys |

### Performance Tuning

**For Large Libraries:**
- Increase database connection pool size
- Adjust scan intervals to avoid system overload
- Use SSDs for database volume
- Configure transcoding quality based on client network capabilities

**For Multiple Users:**
- Adjust concurrent stream limits
- Monitor resource usage during peak times
- Consider hardware upgrades if needed

## Security Best Practices

### Access Control
- Use strong passwords for all accounts
- Enable 2FA if available
- Regularly rotate API keys
- Monitor access logs

### Network Security
- Always use HTTPS with valid certificates
- Restrict access to admin functions
- Use fail2ban or similar for brute force protection
- Keep containers updated

### Data Security
- Encrypt backup files
- Use encrypted volumes for sensitive data
- Regular security audits
- Keep up with security patches

## Scaling Considerations

### When to Scale Up
- Multiple concurrent users streaming
- Libraries exceeding 50,000 tracks
- Slow UI response times
- High CPU/memory usage during scans

### Scaling Options
- **Vertical**: Add more CPU/RAM to existing server
- **Horizontal**: Add caching layers, separate database server
- **Hybrid**: Combine both approaches

## Community Resources

- **Discord**: Join the Melodee community for real-time help
- **GitHub**: Report issues and request features
- **Documentation**: Keep checking for updates
- **Forums**: Share your homelab experiences and learn from others

This guide covers the essential aspects of deploying Melodee in a homelab environment. For specific scenarios or advanced configurations, consult the main documentation or reach out to the community.

---
title: Hardware & Performance
permalink: /hardware/
---

# Hardware & Performance Guide

This guide covers hardware recommendations and performance optimization strategies for running Melodee in various homelab environments, from single-board computers to full server setups.

## System Architecture Overview

Melodee has three main resource-intensive components:
1. **Database** (PostgreSQL): Handles metadata indexing and queries
2. **Media Processing**: Handles transcoding, normalization, and validation
3. **Streaming**: Handles concurrent audio streams to clients

Understanding these components helps optimize your hardware choices.

## Hardware Recommendations by Scale

### Small Libraries (<5,000 tracks)
- **CPU**: Dual-core 2.0GHz+ (Intel/AMD x64 or ARM64)
- **RAM**: 2GB minimum, 4GB recommended
- **Storage**: 
  - System: 100GB SSD
  - Media: Any drive with sufficient space
- **Network**: 100 Mbps Ethernet
- **Examples**: 
  - Raspberry Pi 4 (4GB RAM) with external USB drive
  - Old laptop with 4GB+ RAM
  - Intel NUC with low-end processor

### Medium Libraries (5,000 - 50,000 tracks)
- **CPU**: Quad-core 2.5GHz+ 
- **RAM**: 4GB minimum, 8GB recommended
- **Storage**:
  - System: 100GB+ SSD
  - Database: SSD recommended for performance
  - Media: SSD cache or fast spinning drives
- **Network**: Gigabit Ethernet
- **Examples**:
  - Raspberry Pi 5 or Rock 5B
  - Used desktop with i5/i7 processor
  - Home server with ECC RAM

### Large Libraries (50,000+ tracks)
- **CPU**: 6+ cores, 3.0GHz+ (Intel i7/Ryzen 5 or better)
- **RAM**: 16GB minimum, 32GB+ recommended
- **Storage**:
  - System: Fast NVMe SSD
  - Database: NVMe or fast SSD
  - Media: Multiple drives in RAID or separate volumes
- **Network**: 10Gbps recommended for multi-user scenarios
- **Examples**:
  - High-end NAS (Synology RS series, QNAP)
  - Custom home server with ECC RAM
  - Enterprise-grade hardware repurposed

## Single-Board Computers (SBCs)

### Raspberry Pi Series
- **Pi 4 (4GB)**: Suitable for small collections, limited concurrent streaming
- **Pi 4 (8GB)**: Better for medium collections, light concurrent use
- **Pi 5**: Significantly better performance, recommended for medium collections
- **Storage**: Use fast USB 3.0+ SSD for best performance

### Alternative SBCs
- **Rock 5B/5E**: ARM64 with better performance than Pi, good transcoding
- **Odroid N2+**: Good for media processing, ARM64
- **Pine64 ROCK64**: Budget option, ARM64
- **ASUS Tinker Board**: x86 compatibility, good performance

### SBC Optimization Tips
```bash
# Increase swap space for SBCs
sudo dphys-swapfile swapoff
sudo nano /etc/dphys-swapfile
# Set CONF_SWAPSIZE=2048
sudo dphys-swapfile setup
sudo dphys-swapfile swapon
```

## NAS Integration

### Mounting Strategies
```bash
# NFS mount for media library
sudo mount -t nfs -o vers=4.2,nofail,intr,tcp,rsize=1048576,wsize=1048576 your-nas:/music /mnt/music

# SMB mount
sudo mount -t cifs //your-nas/music /mnt/music -o username=youruser,password=yourpass,uid=1000,gid=1000
```

### Performance Considerations
- Use NFS v4.2 or SMB 3.0+ for best performance
- Ensure gigabit or 10Gbps network connection
- Use dedicated network for NAS if possible
- Consider local cache for frequently accessed files

## Storage Configuration

### Volume Optimization
```
Database Volume: SSD recommended (high IOPS)
Storage Volume: Fast drives for frequently accessed media
Inbound Volume: Local SSD for processing speed
Staging Volume: Local storage for temporary files
User Images: Fast access storage
Playlists: Fast access storage
Logs: Can be on slower storage
```

### RAID Configurations
- **RAID 1**: Mirroring for database volume (redundancy + performance)
- **RAID 5/6**: Good balance for large media libraries
- **RAID 10**: Best performance for high-concurrency scenarios

## Performance Tuning

### Database Optimization

**PostgreSQL Configuration** (in container environment):
```
DB_MIN_POOL_SIZE=10
DB_MAX_POOL_SIZE=50
DB_CONNECTION_TIMEOUT=30
```

**Custom PostgreSQL Settings** (if using external DB):
```sql
-- In postgresql.conf
shared_buffers = 25% of RAM
effective_cache_size = 50% of RAM
work_mem = 4MB
maintenance_work_mem = 256MB
```

### Media Processing

**Transcoding Settings:**
- Use hardware acceleration if available (VA-API, NVENC)
- Configure appropriate quality settings based on client bandwidth
- Use lossless formats for storage, transcode for streaming

**Concurrent Processing:**
- Limit concurrent transcodes to avoid CPU saturation
- Monitor CPU temperature during processing
- Use separate processing schedule during off-peak hours

### Memory Management

**Container Resource Limits:**
```yaml
deploy:
  resources:
    limits:
      cpus: '2.0'
      memory: 4G
    reservations:
      cpus: '0.5'
      memory: 1G
```

**System Memory:**
- Reserve 1GB+ for system processes
- Monitor memory usage during scans
- Consider swap space for SBCs with limited RAM

## Network Optimization

### Bandwidth Requirements
- **Single stream**: 320 kbps (MP3) to 1.4 Mbps (CD quality FLAC)
- **Multiple streams**: Plan for 10x concurrent streams
- **Transcoding**: Additional CPU load, consider quality settings

### Quality of Service (QoS)
```bash
# Example QoS with tc (Traffic Control)
tc qdisc add dev eth0 root handle 1: htb default 10
tc class add dev eth0 parent 1: classid 1:1 htb rate 100mbit
tc class add dev eth0 parent 1:1 classid 1:10 htb rate 80mbit ceil 100mbit
```

## Monitoring & Metrics

### System Monitoring Tools

**Docker Stats:**
```bash
# Monitor container resources
docker stats --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemPerc}}\t{{.NetIO}}\t{{.BlockIO}}"
```

**Custom Monitoring Script:**
```bash
#!/bin/bash
# melodee-monitor.sh

echo "=== Melodee System Monitor ==="
echo "Date: $(date)"
echo "System Load: $(uptime | awk -F'load average:' '{print $2}')"
echo "Memory Usage:"
free -h
echo "Disk Usage:"
df -h | grep -E 'melodee|music'
echo "Container Stats:"
docker stats --no-stream
```

### Performance Metrics to Track
- CPU utilization during scans and streaming
- Memory usage patterns
- Disk I/O performance
- Network throughput
- Database query response times

## Hardware-Specific Optimizations

### Intel Hardware
- Enable Quick Sync for hardware transcoding
- Use Intel drivers for best performance
- Consider Intel NUC for compact solutions

### AMD Hardware
- Use appropriate drivers for transcoding acceleration
- Ryzen processors offer good multi-core performance
- Consider AMD-based NAS solutions

### ARM Platforms
- Ensure Docker images are built for ARM64
- Monitor thermal throttling
- Use active cooling for sustained performance

## Troubleshooting Performance Issues

### Common Performance Problems

**Slow Initial Scans:**
- Solution: Ensure database is on fast storage
- Check: `docker exec melodee-db iostat -x 1`

**High CPU During Streaming:**
- Solution: Adjust transcoding quality settings
- Check: CPU temperature and thermal throttling

**Slow UI Response:**
- Solution: Increase database connection pool
- Check: Memory usage and swap activity

### Diagnostic Commands
```bash
# Check system resources
top -p $(pgrep -f melodee)
# Check container resources
docker stats melodee-blazor melodee-db
# Check disk performance
dd if=/dev/zero of=/tmp/test bs=1M count=1000 oflag=dsync
```

## Scaling Strategies

### Vertical Scaling
- Add more CPU cores
- Increase RAM
- Upgrade storage to faster drives
- Improve network connection

### Horizontal Scaling (Future)
- Database read replicas
- Load balancing across multiple instances
- Distributed transcoding
- CDN for artwork and static assets

## Power Efficiency

### For Always-On Systems
- Use energy-efficient hardware
- Configure power management
- Monitor power consumption
- Consider renewable energy sources

### Scheduling
- Use cron jobs for off-peak processing
- Schedule maintenance during low-usage hours
- Consider power management for non-critical functions

This guide provides comprehensive information for optimizing Melodee performance based on your hardware setup. For specific configurations or advanced scenarios, consult the community or open an issue for additional guidance.

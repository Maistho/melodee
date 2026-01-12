# Melodee Onboarding Improvement Backlog

> **Created**: 2026-01-05  
> **Purpose**: Ideas for improving the new user onboarding experience for Melodee homelab deployments

---

## Overview

This document contains prioritized suggestions for making Melodee easier to set up and get running for new users, particularly those deploying in homelab environments.

## Priority Matrix

| Priority | Effort | Impact | Recommendation |
|----------|--------|--------|----------------|
| 🔴 High | Low | High | Do first |
| 🟠 Medium | Medium | High | Plan for next release |
| 🟡 Low | High | Medium | Backlog for later |
| ⚪ Idea | Variable | Unknown | Needs validation |

---

## 🔴 Quick Wins (Low Effort, High Impact)

### 1. First-Run Setup Wizard in Web UI

**Priority**: 🔴 High  
**Effort**: Medium (3-5 days)  
**Impact**: High - Dramatically improves first-time user experience

**Description**:  
When a user hits the app for the first time (no admin exists), show a guided wizard instead of just a registration form.

**Proposed Steps**:
1. **Welcome**: Brief intro, what Melodee does
2. **Create Admin**: Username, email, password
3. **Library Paths**: Configure/validate storage paths
4. **Music Source**: Point to inbound folder, explain pipeline
5. **Metadata Providers** (optional): API keys for enhanced metadata
6. **Initial Scan**: Trigger first scan with progress indicator
7. **Complete**: "Your library is being indexed... Here's how to connect clients"

**Technical Notes**:
- Store "setup_completed" flag in database or settings
- Redirect to wizard if flag is false and no admin exists
- Allow skipping optional steps
- Show progress indicator during initial scan

**Acceptance Criteria**:
- [ ] Wizard appears on first access when no admin exists
- [ ] All steps validate input before proceeding
- [ ] User can go back to previous steps
- [ ] Optional steps can be skipped
- [ ] Wizard only appears once (flag persisted)

---

### 2. Health Dashboard on Landing Page

**Priority**: 🔴 High  
**Effort**: Low (1-2 days)  
**Impact**: High - Users immediately see what's working/broken

**Description**:  
Show system status at a glance, especially useful for new users to understand if their setup is working.

**Proposed Components**:
```
System Status
├── ✅ Database connected
├── ✅ Storage accessible (2.3 TB free)
├── ⚠️ No music indexed yet - [Start scan]
├── ❌ Inbound folder empty - [How to add music]
└── ✅ API healthy
```

**Technical Notes**:
- Create health check endpoint that returns structured status
- Display on dashboard or dedicated status page
- Include actionable links for each issue
- Auto-refresh every 30 seconds

**Acceptance Criteria**:
- [ ] Shows database connection status
- [ ] Shows storage path accessibility and free space
- [ ] Shows music library statistics (albums, songs, artists)
- [ ] Shows inbound folder status
- [ ] Provides actionable links for issues

---

### 3. Interactive "Getting Started" Checklist

**Priority**: 🔴 High  
**Effort**: Low (1-2 days)  
**Impact**: Medium - Gamifies onboarding, shows progress

**Description**:  
Persistent checklist in the UI that tracks onboarding progress and celebrates milestones.

**Proposed Checklist**:
```
Getting Started with Melodee
├── [x] Create admin account
├── [x] Configure library paths
├── [ ] Add your first music files
├── [ ] Run initial library scan
├── [ ] Connect your first client app
└── [ ] Play your first song 🎉
```

**Technical Notes**:
- Track progress in user settings or separate table
- Show in sidebar or as dismissible banner
- Celebrate completion with confetti/toast
- Allow hiding after completion

**Acceptance Criteria**:
- [ ] Checklist visible to new admin users
- [ ] Items auto-complete when actions are performed
- [ ] Manual completion option for items that can't be auto-detected
- [ ] Dismissible after completion
- [ ] Celebration animation on 100% completion

---

### 4. Sample Music Bundle

**Priority**: 🟠 Medium  
**Effort**: Low (1 day)  
**Impact**: Medium - Users see something working immediately

**Description**:  
Include a small Creative Commons music sample that gets auto-copied to inbound on first run, so users see the pipeline working immediately.

**Proposed Implementation**:
- Include 2-3 CC-licensed tracks in repo (or download on first run)
- Copy to inbound folder during first-run wizard
- User sees real music flowing through pipeline
- Can be disabled via setting

**Sources for CC Music**:
- Free Music Archive
- Jamendo
- ccMixter
- Incompetech

**Technical Notes**:
- Keep bundle small (<50MB)
- Include proper attribution
- Make it optional/skippable
- Consider downloading from CDN instead of bundling in repo

**Acceptance Criteria**:
- [ ] Sample music available during first-run
- [ ] Proper CC attribution included
- [ ] User can skip/disable sample import
- [ ] Sample processes through full pipeline successfully

---

## 🟠 Medium Effort Improvements

### 5. One-Line Install Script

**Priority**: 🟠 Medium  
**Effort**: Medium (2-3 days)  
**Impact**: High - Dramatically lowers barrier to entry

**Description**:  
Single command to get Melodee running:
```bash
curl -fsSL https://melodee.org/install.sh | bash
```

**Proposed Functionality**:
1. Detect OS (Ubuntu, Debian, Fedora, macOS, etc.)
2. Check/install prerequisites (Podman/Docker, Python, git)
3. Clone repository
4. Run container setup script
5. Open browser to http://localhost:8080

**Technical Notes**:
- Host script on melodee.org or raw GitHub
- Support major Linux distros + macOS
- Provide Windows instructions separately (WSL2)
- Include `--dry-run` option to show what would happen
- Checksum verification for security

**Script Options**:
```bash
curl -fsSL https://melodee.org/install.sh | bash -s -- --help
curl -fsSL https://melodee.org/install.sh | bash -s -- --dry-run
curl -fsSL https://melodee.org/install.sh | bash -s -- --no-start
```

**Acceptance Criteria**:
- [ ] Works on Ubuntu 22.04/24.04
- [ ] Works on Debian 12
- [ ] Works on Fedora 39/40
- [ ] Works on macOS (with Homebrew)
- [ ] Provides clear error messages for unsupported systems
- [ ] `--dry-run` shows all commands without executing

---

### 6. Pre-built Container Images on GHCR

**Priority**: 🔴 High  
**Effort**: Medium (2-3 days)  
**Impact**: High - Eliminates 10+ minute build wait

**Description**:  
Publish pre-built images to GitHub Container Registry so users can pull instead of build.

**Proposed Usage**:
```bash
# Instead of building locally
podman pull ghcr.io/melodee-project/melodee:latest
podman compose up -d
```

**Technical Notes**:
- GitHub Actions workflow to build and push on release/tag
- Multi-arch support (amd64, arm64 for Raspberry Pi)
- Tagged versions + `latest`
- Update compose.yml to use GHCR image by default (with local build fallback)

**Workflow Triggers**:
- On version tag (v1.0.0, etc.)
- On main branch (as `latest`)
- Manual dispatch for testing

**Acceptance Criteria**:
- [ ] Images published to ghcr.io/melodee-project/melodee
- [ ] amd64 and arm64 architectures supported
- [ ] compose.yml updated to pull from GHCR
- [ ] Local build still works as fallback
- [ ] Images tagged with version and `latest`

---

### 7. Client Connection QR Codes

**Priority**: 🟠 Medium  
**Effort**: Low-Medium (2-3 days)  
**Impact**: Medium - Makes mobile setup trivial

**Description**:  
Generate QR codes in the web UI that mobile apps can scan to auto-configure server connection.

**Proposed Implementation**:
- "Connect Client" page showing QR codes
- Different QR for each API type (OpenSubsonic, Jellyfin)
- Includes server URL, optionally username
- Instructions for each popular client

**QR Content Format**:
```
# OpenSubsonic
subsonic://username@server.example.com:8080

# Jellyfin
jellyfin://server.example.com:8080
```

**Supported Clients**:
- Finamp (Jellyfin)
- Symfonium (Subsonic)
- Ultrasonic (Subsonic)
- Feishin (Both)
- Streamyfin (Jellyfin)

**Technical Notes**:
- Use QRCode.js or similar library
- Detect server's external URL if behind reverse proxy
- Allow manual URL override

**Acceptance Criteria**:
- [ ] QR codes generate correctly for OpenSubsonic
- [ ] QR codes generate correctly for Jellyfin
- [ ] Instructions provided for each major client
- [ ] Manual URL entry option available
- [ ] Works with reverse proxy configurations

---

### 8. Guided Troubleshooter

**Priority**: 🟡 Low  
**Effort**: Medium (3-5 days)  
**Impact**: Medium - Reduces support requests

**Description**:  
Interactive troubleshooting wizard in the UI that helps diagnose common issues.

**Proposed Flows**:

**"Music not appearing?"**
1. Check if inbound has files → Show file count
2. Check if jobs are running → Show job status
3. Check for validation errors → Show staging issues
4. Check library scan status → Show indexed count

**"Can't connect client?"**
1. Test API endpoint → Show health check result
2. Verify credentials → Test authentication
3. Check network → Show server URL/port
4. Firewall check → Common ports to open

**"Slow performance?"**
1. Show resource usage (CPU, RAM, disk I/O)
2. Check database query performance
3. Suggest optimizations based on library size

**Acceptance Criteria**:
- [ ] At least 3 troubleshooting flows implemented
- [ ] Each step provides actionable information
- [ ] Links to documentation for complex issues
- [ ] Option to generate support bundle

---

### 9. Email/Notification on First Successful Import

**Priority**: 🟡 Low  
**Effort**: Low (1 day)  
**Impact**: Low-Medium - Nice touch for engagement

**Description**:  
When the first album successfully makes it through the pipeline, celebrate!

**Proposed Implementation**:
- Toast notification in UI: "Your first album is ready! 🎉"
- Optional email notification (if email configured)
- Link to play the album or connect clients
- Only triggers once (first album ever)

**Acceptance Criteria**:
- [ ] Toast appears when first album is indexed
- [ ] Email sent if email is configured
- [ ] Only triggers once per installation
- [ ] Includes link to play or connect clients

---

## 🟡 Higher Effort Features

### 10. Homelab Platform Integrations

**Priority**: 🟠 Medium  
**Effort**: High (varies by platform)  
**Impact**: High - Reaches users where they already are

#### 10a. Unraid Community App

**Effort**: Medium (3-5 days)  
**Impact**: High - Large homelab user base

**Requirements**:
- XML template for Community Applications
- Pre-configured paths (/mnt/user/appdata, /mnt/user/media)
- Icon and description
- WebUI link configuration

**Resources**:
- https://forums.unraid.net/topic/38582-plug-in-community-applications/

---

#### 10b. TrueNAS SCALE App

**Effort**: Medium (3-5 days)  
**Impact**: Medium - Growing NAS platform

**Requirements**:
- Helm chart or TrueCharts format
- ix_values.yaml for SCALE-specific config
- Storage class configuration

**Resources**:
- https://truecharts.org/manual/development/

---

#### 10c. CasaOS App

**Effort**: Low (1-2 days)  
**Impact**: Medium - Popular with beginners

**Requirements**:
- docker-compose format app definition
- Icon and metadata
- Submit to CasaOS app store

**Resources**:
- https://github.com/IceWhaleTech/CasaOS-AppStore

---

#### 10d. Portainer Template

**Effort**: Low (1 day)  
**Impact**: Medium - Common Docker management tool

**Requirements**:
- Stack template JSON
- Environment variable definitions
- Logo URL

**Resources**:
- https://docs.portainer.io/advanced/app-templates

---

### 11. Ansible/Terraform Deployment

**Priority**: 🟡 Low  
**Effort**: High (5-7 days)  
**Impact**: Medium - For advanced users

**Description**:  
Infrastructure-as-code deployment for advanced homelabbers.

**Ansible Playbook Features**:
```bash
ansible-playbook melodee.yml -i inventory
```
- Idempotent deployment
- Optional reverse proxy setup (Traefik/Nginx)
- Automated backup configuration
- SSL certificate provisioning
- Multi-host support for HA

**Terraform Module Features**:
- Cloud provider support (AWS, GCP, Azure, DigitalOcean)
- VPS provisioning
- DNS configuration
- Load balancer setup

**Acceptance Criteria**:
- [ ] Ansible playbook deploys Melodee end-to-end
- [ ] Playbook is idempotent
- [ ] Configurable via variables
- [ ] Documentation for common scenarios

---

### 12. Music Import Assistants

**Priority**: 🟡 Low  
**Effort**: High (varies)  
**Impact**: High - Reduces migration friction

#### 12a. Import from Existing Media Servers

**Description**: Point to existing Plex/Jellyfin/Navidrome library and import:
- Music file locations
- Metadata and artwork
- Playlists
- Play history and ratings

**Supported Sources**:
- Plex (via API or database)
- Jellyfin (via API)
- Navidrome (via database)
- Airsonic/Subsonic (via API)

---

#### 12b. Import from Streaming Services

**Description**: Connect to streaming service to import playlists as "wanted" lists.

**Supported Services**:
- Spotify (OAuth)
- Apple Music (via MusicKit)
- Last.fm (for play history)
- YouTube Music

**Features**:
- Import playlists as request lists
- Match existing library tracks
- Track what's missing

---

### 13. Video Walkthrough Integration

**Priority**: 🟡 Low  
**Effort**: Medium (content creation)  
**Impact**: Medium - Helps visual learners

**Description**:  
Embed or link short video tutorials at key points in the UI.

**Proposed Videos**:
| Location | Video | Duration |
|----------|-------|----------|
| Setup wizard | "Quick setup walkthrough" | 2 min |
| First scan | "How the music pipeline works" | 1 min |
| Client setup | "Connect Finamp in 30 seconds" | 30 sec |
| Troubleshooting | "Common issues and fixes" | 3 min |

**Technical Notes**:
- Host on YouTube or self-host
- Embed as modal or sidebar panel
- "Don't show again" option

---

## ⚪ Experimental Ideas

### 14. "Melodee Doctor" CLI Command

**Priority**: ⚪ Idea  
**Effort**: Medium (3-4 days)  
**Impact**: Medium - Great for support

**Description**:  
Comprehensive diagnostic command for troubleshooting.

```bash
python3 scripts/run-container-setup.py --doctor
```

**Output**:
```
Melodee Doctor v1.0
==================

System Checks:
  ✅ Container runtime: podman 4.9.0
  ✅ Compose: podman-compose 1.0.6
  ✅ Disk space: 847 GB free
  ✅ Memory: 16 GB (2 GB required)
  ✅ Port 8080: available

Container Checks:
  ✅ melodee-db: running, healthy
  ✅ melodee.blazor: running, healthy

Application Checks:
  ✅ Database connection: OK (23ms)
  ✅ Storage paths: all accessible
  ⚠️ Inbound folder: empty
  ✅ API health: OK

Recommendations:
  • Add music files to inbound folder to get started
  • Consider enabling Brave Search for better artwork

Generate support bundle? (y/N):
```

**Features**:
- Comprehensive system checks
- Container status and health
- Application health checks
- Recommendations based on findings
- Support bundle generation (sanitized logs, config, etc.)

---

### 15. Demo Mode

**Priority**: ⚪ Idea  
**Effort**: Medium (3-4 days)  
**Impact**: Medium - Try before committing

**Description**:  
Let users explore Melodee with sample data before setting up their own library.

```bash
python3 scripts/run-container-setup.py --demo
```

**Features**:
- Pre-populated with sample music and metadata
- Full UI exploration
- Read-only mode (no permanent changes)
- "Reset to clean" option
- "Convert to real installation" wizard

---

### 16. Progressive Disclosure Configuration

**Priority**: ⚪ Idea  
**Effort**: Medium (3-4 days)  
**Impact**: Medium - Less overwhelming

**Description**:  
Tiered configuration UI based on user expertise.

**Modes**:
- **Simple**: Just the essentials (paths, port, credentials)
- **Advanced**: All settings with explanations and tooltips
- **Expert**: Raw JSON/YAML editor with validation

**Implementation**:
- Toggle in settings to switch modes
- Remember preference per user
- Show "Advanced" badge on hidden settings
- Search across all settings regardless of mode

---

### 17. Community-Contributed "Recipes"

**Priority**: ⚪ Idea  
**Effort**: Low (documentation)  
**Impact**: Medium - Community building

**Description**:  
Document common homelab configurations as step-by-step guides.

**Proposed Recipes**:
| Recipe | Description |
|--------|-------------|
| Melodee + Traefik + Let's Encrypt | HTTPS with auto-renewal |
| Melodee on Raspberry Pi 5 | ARM optimization, SD card considerations |
| Melodee with NFS-mounted library | Network storage setup |
| Melodee behind Cloudflare Tunnel | Zero-trust access |
| Melodee + Authentik | SSO integration |
| Melodee HA with PostgreSQL replication | High availability setup |

**Format**:
- Step-by-step markdown guides
- Include docker-compose snippets
- Troubleshooting section
- Community contributions via PR

---

## Implementation Roadmap Suggestion

### Phase 1: Quick Wins (Next Release)
1. Health Dashboard
2. Getting Started Checklist
3. Pre-built Container Images (GHCR)

### Phase 2: Core Onboarding (Following Release)
4. First-Run Setup Wizard
5. One-Line Install Script
6. Client QR Codes

### Phase 3: Platform Expansion
7. Unraid Community App
8. CasaOS App
9. Portainer Template

### Phase 4: Advanced Features
10. Melodee Doctor CLI
11. Guided Troubleshooter
12. Import Assistants

---

## Decision Log

| Date | Item | Decision | Notes |
|------|------|----------|-------|
| | | | |

---

## References

- [Unraid Community Apps](https://forums.unraid.net/topic/38582-plug-in-community-applications/)
- [TrueCharts Development](https://truecharts.org/manual/development/)
- [CasaOS App Store](https://github.com/IceWhaleTech/CasaOS-AppStore)
- [Portainer App Templates](https://docs.portainer.io/advanced/app-templates)
- [Creative Commons Music Sources](https://creativecommons.org/about/program-areas/arts-culture/arts-culture-resources/legalmusicforvideos/)

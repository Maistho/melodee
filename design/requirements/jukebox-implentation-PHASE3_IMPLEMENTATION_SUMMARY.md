# Phase 3 Implementation Summary - Endpoint Registry + Capability Model

## Overview

Phase 3 of the Melodee Party Mode feature implements the Endpoint Registry and Capability Model, enabling dynamic playback device management with capability-aware UI controls.

## Changes Made

### 1. Database Migration (`src/Melodee.Common/Migrations/`)

**New Files:**
- `20260110180000_Phase3_EndpointRegistry.cs` - Migration adding:
  - `IsEndpointOffline` column to `PartySessions` table
  - `PartyAuditEvents` table for audit trail

- `20260110180000_Phase3_EndpointRegistry.Designer.cs` - EF Core migration designer file

**Modified Files:**
- `MelodeeDbContextModelSnapshot.cs` - Updated to include new `PartyAuditEvent` entity and `IsEndpointOffline` field

### 2. SignalR Configuration (`src/Melodee.Blazor/Program.cs`)

**Changes:**
- Added `builder.Services.AddSignalR()` in service configuration section
- Added `app.MapHub<PartyHub>("/party-hub")` endpoint mapping

### 3. Localization Keys (`src/Melodee.Blazor/Resources/`)

**New keys added to all 10 resource files:**
- `PartyMode.Endpoint.Title` - "Playback Device"
- `PartyMode.Endpoint.NoEndpoint` - "No playback device attached"
- `PartyMode.Endpoint.Attach` - "Attach Device"
- `PartyMode.Endpoint.Detach` - "Detach Device"
- `PartyMode.Endpoint.Attaching` - "Attaching device..."
- `PartyMode.Endpoint.Detaching` - "Detaching device..."
- `PartyMode.Endpoint.AttachSuccess` - "Device attached successfully"
- `PartyMode.Endpoint.DetachSuccess` - "Device detached"
- `PartyMode.Endpoint.AttachError` - "Failed to attach device"
- `PartyMode.Endpoint.DetachError` - "Failed to detach device"
- `PartyMode.Endpoint.Offline` - "Offline"
- `PartyMode.Endpoint.Online` - "Online"
- `PartyMode.Endpoint.Stale` - "Connection unstable"
- `PartyMode.Endpoint.LastSeen` - "Last seen"
- `PartyMode.Endpoint.Available` - "Available Devices"
- `PartyMode.Endpoint.Current` - "Current Device"
- `PartyMode.Endpoint.Capabilities` - "Capabilities"
- `PartyMode.Endpoint.Capability.Play` - "Play"
- `PartyMode.Endpoint.Capability.Pause` - "Pause"
- `PartyMode.Endpoint.Capability.Skip` - "Skip"
- `PartyMode.Endpoint.Capability.Seek` - "Seek"
- `PartyMode.Endpoint.Capability.Volume` - "Volume"
- `PartyMode.Endpoint.Capability.Position` - "Position"
- `PartyMode.Endpoint.OfflineWarning` - "Playback device is offline. Music may not play."
- `PartyMode.Endpoint.SelectDevice` - "Select a playback device"
- `PartyMode.Endpoint.WebPlayer` - "Web Player"
- `PartyMode.Endpoint.SystemBackend` - "System Audio"

### 4. PartyPlayer Component (`src/Melodee.Blazor/Components/Components/PartyPlayer.razor`)

**Changes:**
- Added `PartySessionEndpointCapabilities? Capabilities` parameter
- Added capability-based control enabling:
  - `CanControl` - Master control flag
  - `CanPlayPause` - Play/Pause button state
  - `CanSkip` - Skip track button state
  - `CanSeek` - Seek/Rewind/Forward button state
  - `CanSetVolume` - Volume slider state
- Updated all control methods to check capabilities before API calls

### 5. Endpoint Registry Controller (`src/Melodee.Blazor/Controllers/Melodee/PartySessionEndpointRegistryController.cs`)

**New API endpoints:**
- `GET /api/v1/endpoints` - Gets all endpoints visible to the current user
- `GET /api/v1/endpoints/for-session/{sessionId:guid}` - Gets endpoints available for a session
- `POST /api/v1/endpoints/{endpointId:guid}/attach` - Attaches an endpoint to a session
- `POST /api/v1/endpoints/{endpointId:guid}/detach` - Detaches an endpoint from its session

### 6. PartySessionEndpointSelector Component (`src/Melodee.Blazor/Components/Components/PartySessionEndpointSelector.razor`)

**Features:**
- Displays list of available endpoints for a session
- Shows endpoint status (active, stale, online/offline)
- Allows attaching/detaching endpoints
- Refreshes endpoint list on changes

### 7. Staleness Service (`src/Melodee.Common/Services/PartyMode/PartySessionEndpointStalenessService.cs`)

**Methods:**
- `IsStale(PartySessionEndpoint endpoint)` - Checks if an endpoint is stale
- `GetStaleEndpointsAsync()` - Gets all stale endpoints
- `MarkSessionsWithStaleEndpointsAsync()` - Marks sessions with stale endpoints

### 8. Endpoint Capabilities Model (`src/Melodee.Common/Models/PartyMode/PartySessionEndpointCapabilities.cs`)

**Properties:**
- `CanPlay`, `CanPause`, `CanSkip`, `CanSeek`, `CanSetVolume`, `CanReportPosition`
- `AudioDevice`, `MaxVolume`, `MinVolume`, `SupportedFormats`, `DisplayName`

**Factory Methods:**
- `WebPlayerDefault()` - Creates default web player capabilities
- `SystemBackendDefault(backendType)` - Creates capabilities for system backends (mpv, mpd)

### 9. Unit Tests

**New Test Files:**
- `tests/Melodee.Tests.Common/Services/PartySessionEndpointStalenessServiceTests.cs` - Tests for:
  - `IsStale()` with null LastSeenAt
  - `IsStale()` with recent timestamp
  - `IsStale()` with old timestamp
  - Custom stale threshold support

- `tests/Melodee.Tests.Common/Models/PartySessionEndpointCapabilitiesTests.cs` - Tests for:
  - Default capabilities
  - Web player defaults
  - System backend defaults (mpv, mpd)
  - `CanControl()` method
  - JSON serialization round-trip

## Files Modified/Created Summary

### New Files
| File | Description |
|------|-------------|
| `src/Melodee.Common/Migrations/20260110180000_Phase3_EndpointRegistry.cs` | Database migration |
| `src/Melodee.Common/Migrations/20260110180000_Phase3_EndpointRegistry.Designer.cs` | Migration designer |
| `src/Melodee.Blazor/Controllers/Melodee/PartySessionEndpointRegistryController.cs` | Endpoint registry API controller |
| `src/Melodee.Blazor/Components/Components/PartySessionEndpointSelector.razor` | Endpoint selection UI component |
| `src/Melodee.Common/Services/PartyMode/PartySessionEndpointStalenessService.cs` | Endpoint staleness detection service |
| `src/Melodee.Common/Models/PartyMode/PartySessionEndpointCapabilities.cs` | Endpoint capabilities model |
| `tests/Melodee.Tests.Common/Services/PartySessionEndpointStalenessServiceTests.cs` | Staleness service tests |
| `tests/Melodee.Tests.Common/Models/PartySessionEndpointCapabilitiesTests.cs` | Capabilities model tests |

### Modified Files
| File | Changes |
|------|---------|
| `src/Melodee.Blazor/Program.cs` | Added SignalR configuration |
| `src/Melodee.Common/Migrations/MelodeeDbContextModelSnapshot.cs` | Added PartyAuditEvent entity, IsEndpointOffline field |
| `src/Melodee.Blazor/Resources/SharedResources.*.resx` (10 files) | Added 26 new localization keys |
| `src/Melodee.Blazor/Components/Components/PartyPlayer.razor` | Added capabilities support |
| `src/Melodee.Blazor/Components/Pages/PartySession.razor` | Added endpoint selector panel |
| `src/Melodee.Common/Data/Models/PartyAuditEvent.cs` | Removed duplicate CreatedAt property |

## Renamed Classes (from original implementation)

The following classes were renamed to follow naming conventions:
- `EndpointCapabilities` → `PartySessionEndpointCapabilities`
- `EndpointRegistryController` → `PartySessionEndpointRegistryController`
- `EndpointSelector` → `PartySessionEndpointSelector`
- `IEndpointStalenessService` → `IPartySessionEndpointStalenessService`
- `EndpointStalenessService` → `PartySessionEndpointStalenessService`

## Known Issues

### Pre-existing Issues (from Phase 2)
The following files have compilation errors that were not introduced by Phase 3:
- `PartySessionEndpointRegistryService.cs` - Missing CreatedAt initialization
- `PartyAuditService.cs` - OperationResult initialization issues
- `PartyQueueService.cs` - ICacheManager extension methods missing
- `PartyPlaybackService.cs` - OperationResult initialization issues

These issues need to be addressed separately.

## Next Steps

1. **Fix Pre-existing Compilation Errors**
   - Address issues in Phase 2 service files
   - Ensure all OperationResult types are properly initialized
   - Add missing ICacheManager extension methods

2. **Run Database Migration**
   ```bash
   dotnet ef database update --project src/Melodee.Common/Melodee.Common.csproj
   ```

3. **Run Tests**
   ```bash
   dotnet test tests/Melodee.Tests.Common/Services/PartySessionEndpointStalenessServiceTests.cs
   dotnet test tests/Melodee.Tests.Common/Models/PartySessionEndpointCapabilitiesTests.cs
   ```

4. **Translate Localization Keys**
   - Provide translations for the 26 new keys in all 10 languages
   - Remove `[NEEDS TRANSLATION]` markers

## Testing Commands

```bash
# Build the solution
dotnet build --no-restore

# Run specific tests
dotnet test tests/Melodee.Tests.Common/Services/PartySessionEndpointStalenessServiceTests.cs
dotnet test tests/Melodee.Tests.Common/Models/PartySessionEndpointCapabilitiesTests.cs

# Create/update database
dotnet ef database update --project src/Melodee.Common/Melodee.Common.csproj

# Generate a new migration if changes are made
dotnet ef migrations add Phase3_EndpointRegistry_Update --project src/Melodee.Common/Melodee.Common.csproj
```

## Implementation Notes

- The staleness threshold is configurable via `PartyModeOptions.EndpointStaleSeconds` (default: 30 seconds)
- Endpoint capabilities are JSON-serialized and stored in `PartySessionEndpoint.CapabilitiesJson`
- The `PartyPlayer` component gracefully handles null capabilities (defaults to all controls enabled)
- SignalR hub is available at `/party-hub` for real-time updates
- Party audit events track all significant session activities
- One active endpoint per session at a time
- Only session Owner/DJ can attach system endpoints

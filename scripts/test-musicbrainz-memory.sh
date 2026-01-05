#!/usr/bin/env bash
set -euo pipefail

# MusicBrainz Memory Test Script
# Tests the MusicBrainz import with memory monitoring
# Target: Stay under 2GB memory during full import

MUSICBRAINZ_DATA="/mnt/incoming/melodee_test/search-engine-storage/musicbrainz"
STAGING_PATH="$MUSICBRAINZ_DATA/staging/mbdump"
MEMORY_LIMIT_MB=2048

echo "╔══════════════════════════════════════════════════════════════╗"
echo "║     MusicBrainz Import Memory Test                           ║"
echo "║     Target: Stay under 2GB memory during full import         ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo

# Check data exists
if [[ ! -d "$STAGING_PATH" ]]; then
    echo "ERROR: MusicBrainz staging data not found at: $STAGING_PATH"
    exit 1
fi

# Count records
ARTIST_COUNT=$(wc -l < "$STAGING_PATH/artist" 2>/dev/null || echo "0")
RELEASE_COUNT=$(wc -l < "$STAGING_PATH/release" 2>/dev/null || echo "0")

echo "Data statistics:"
echo "  Artists:  $ARTIST_COUNT"
echo "  Releases: $RELEASE_COUNT"
echo

# Build the project first
echo "Building Melodee.Blazor..."
cd /home/steven/source/melodee
dotnet build src/Melodee.Blazor/Melodee.Blazor.csproj -c Release --verbosity quiet

echo
echo "========================================================================"
echo "To run the actual import test, start the application in a container:"
echo "========================================================================"
echo
echo "1. Start container with memory limit:"
echo "   docker compose up -d"
echo
echo "2. Watch container memory usage:"
echo "   docker stats melodee --no-stream"
echo
echo "3. Trigger MusicBrainz import via the web UI:"
echo "   - Go to http://localhost:8080/admin/settings"
echo "   - Find 'Search Engine: MusicBrainz: Enabled' and set to true"
echo "   - The background job will start importing"
echo
echo "4. Monitor memory during import:"
echo "   watch -n 1 'docker stats melodee --no-stream'"
echo
echo "5. Verify search works after import:"
echo "   - Search for artist 'Men At Work'"
echo "   - Should find album 'Cargo' (1983)"
echo "   - MusicBrainz Artist ID: 395cc503-63b5-4a0b-a20a-604e3fcacea2"
echo "   - MusicBrainz Release ID: 517346ce-cd49-4cfa-831e-0546b871708a"
echo
echo "Expected results:"
echo "  - Peak memory should stay under ${MEMORY_LIMIT_MB}MB"
echo "  - Import should complete without OOM"
echo "  - Search should find 'Men At Work' and 'Cargo'"

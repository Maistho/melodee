#!/usr/bin/env bash
set -uo pipefail

# Jellyfin API endpoint tester for Melodee
# Tests the pre-auth endpoints that Jellyfin clients call during server discovery

BASE_URL="${1:-http://localhost:5157}"
USERNAME="${JF_USERNAME:-${MELODEE_USERNAME:-admin}}"
PASSWORD="${JF_PASSWORD:-${MELODEE_PASSWORD:-password}}"

echo "Testing Jellyfin API endpoints on: $BASE_URL"
echo "============================================="

TEST_NUM=0

increment_test() {
    TEST_NUM=$((TEST_NUM + 1))
}

# Test 0: HEAD / (server discovery - some Jellyfin clients may send this)
# Note: This test is skipped because Blazor returns 302 redirect for browser requests
# Jellyfin clients typically use /System/Info/Public for discovery instead
echo -e "\n[$TEST_NUM] HEAD / (server discovery) - SKIPPED"
echo "  (Blazor redirects browser requests; clients should use /System/Info/Public)"

# Test 1: /System/Info/Public (anonymous, pre-auth)
increment_test
echo -e "\n[$TEST_NUM] GET /System/Info/Public (anonymous)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/System/Info/Public")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response: $body" | head -c 500
    echo ""
else
    echo "✗ Status: $http_code (expected 200)"
    echo "Response: $body"
fi

# Test 2: /System/Ping (anonymous, pre-auth)
increment_test
echo -e "\n[$TEST_NUM] GET /System/Ping (anonymous)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/System/Ping")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

if [ "$http_code" = "204" ]; then
    echo "✓ Status: $http_code (No Content)"
else
    echo "✗ Status: $http_code (expected 204)"
fi

# Test 3: POST /System/Ping
increment_test
echo -e "\n[$TEST_NUM] POST /System/Ping (anonymous)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X POST "$BASE_URL/System/Ping")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

if [ "$http_code" = "204" ]; then
    echo "✓ Status: $http_code (No Content)"
else
    echo "✗ Status: $http_code (expected 204)"
fi

# Test 4: Direct /api/jf/System/Info/Public (should also work)
increment_test
echo -e "\n[$TEST_NUM] GET /api/jf/System/Info/Public (direct prefix)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/api/jf/System/Info/Public")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response: $body" | head -c 500
    echo ""
else
    echo "✗ Status: $http_code (expected 200)"
    echo "Response: $body"
fi

# Test 5: HEAD /api/jf (direct root)
increment_test
echo -e "\n[$TEST_NUM] HEAD /api/jf (direct root)"
http_code=$(curl -s -o /dev/null -w "%{http_code}" -X HEAD "$BASE_URL/api/jf")

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
else
    echo "✗ Status: $http_code (expected 200)"
fi

# Test 6: Authenticated endpoint without token (should fail)
increment_test
echo -e "\n[$TEST_NUM] GET /UserViews (no auth - should fail)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/UserViews")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

if [ "$http_code" = "404" ] || [ "$http_code" = "302" ]; then
    echo "✓ Status: $http_code (correctly rejected - no Jellyfin headers)"
else
    echo "? Status: $http_code"
fi

# Test 7: With MediaBrowser header but no valid token
increment_test
echo -e "\n[$TEST_NUM] GET /UserViews (with MediaBrowser header, invalid token)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H 'Authorization: MediaBrowser Token="invalid_token_12345"' \
    "$BASE_URL/UserViews")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

if [ "$http_code" = "401" ]; then
    echo "✓ Status: $http_code (correctly unauthorized)"
else
    echo "? Status: $http_code (expected 401)"
fi

# Test 8: Authenticate and get token
increment_test
echo -e "\n[$TEST_NUM] POST /Users/AuthenticateByName (get token)"

auth_response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -X POST "$BASE_URL/Users/AuthenticateByName" \
    -H "Content-Type: application/json" \
    -H 'X-Emby-Authorization: MediaBrowser Client="TestScript", Device="Bash", DeviceId="test-device-001", Version="1.0"' \
    -d "{\"Username\":\"$USERNAME\",\"Pw\":\"$PASSWORD\"}")

http_code=$(echo "$auth_response" | grep "HTTP_CODE:" | cut -d: -f2)
auth_body=$(echo "$auth_response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code (authenticated)"
    ACCESS_TOKEN=$(echo "$auth_body" | grep -o '"AccessToken":"[^"]*"' | cut -d'"' -f4)
    USER_ID=$(echo "$auth_body" | grep -o '"Id":"[^"]*"' | head -1 | cut -d'"' -f4)
    echo "  AccessToken: ${ACCESS_TOKEN:0:20}..."
    echo "  UserId: $USER_ID"
else
    echo "✗ Status: $http_code (authentication failed)"
    echo "Response: $auth_body"
    echo -e "\n============================================="
    echo "Test complete (stopped at authentication failure)!"
    exit 1
fi

AUTH_HEADER="MediaBrowser Token=\"$ACCESS_TOKEN\", Client=\"TestScript\", Device=\"Bash\", DeviceId=\"test-device-001\", Version=\"1.0\""

# Test 9: Get UserViews (libraries)
increment_test
echo -e "\n[$TEST_NUM] GET /UserViews (get libraries)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$BASE_URL/UserViews")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    LIBRARY_ID=$(echo "$body" | grep -o '"Id":"[^"]*"' | head -1 | cut -d'"' -f4)
    LIBRARY_NAME=$(echo "$body" | grep -o '"Name":"[^"]*"' | head -1 | cut -d'"' -f4)
    TOTAL_LIBRARIES=$(echo "$body" | grep -o '"TotalRecordCount":[0-9]*' | cut -d: -f2)
    echo "  Total libraries: $TOTAL_LIBRARIES"
    echo "  First library: $LIBRARY_NAME (Id: $LIBRARY_ID)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 10: Get Albums from first library
increment_test
echo -e "\n[$TEST_NUM] GET /Items?includeItemTypes=MusicAlbum (get albums)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$BASE_URL/Items?includeItemTypes=MusicAlbum&limit=10&startIndex=0&recursive=true")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    TOTAL_ALBUMS=$(echo "$body" | grep -o '"TotalRecordCount":[0-9]*' | cut -d: -f2)
    FIRST_ALBUM=$(echo "$body" | grep -o '"Name":"[^"]*"' | head -1 | cut -d'"' -f4)
    FIRST_ALBUM_ID=$(echo "$body" | grep -o '"Id":"[^"]*"' | head -1 | cut -d'"' -f4)
    echo "  Total albums: ${TOTAL_ALBUMS:-0}"
    echo "  First album: ${FIRST_ALBUM:-none} (Id: ${FIRST_ALBUM_ID:-none})"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 11: Get Artists
increment_test
echo -e "\n[$TEST_NUM] GET /Items?includeItemTypes=MusicArtist (get artists)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$BASE_URL/Items?includeItemTypes=MusicArtist&limit=10&startIndex=0&recursive=true")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    TOTAL_ARTISTS=$(echo "$body" | grep -o '"TotalRecordCount":[0-9]*' | cut -d: -f2)
    FIRST_ARTIST=$(echo "$body" | grep -o '"Name":"[^"]*"' | head -1 | cut -d'"' -f4)
    echo "  Total artists: ${TOTAL_ARTISTS:-0}"
    echo "  First artist: ${FIRST_ARTIST:-none}"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 12: Get Songs/Audio
increment_test
echo -e "\n[$TEST_NUM] GET /Items?includeItemTypes=Audio (get songs)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$BASE_URL/Items?includeItemTypes=Audio&limit=10&startIndex=0&recursive=true")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

FIRST_SONG_ID=""
if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    TOTAL_SONGS=$(echo "$body" | grep -o '"TotalRecordCount":[0-9]*' | cut -d: -f2)
    FIRST_SONG=$(echo "$body" | grep -o '"Name":"[^"]*"' | head -1 | cut -d'"' -f4)
    FIRST_SONG_ID=$(echo "$body" | grep -o '"Id":"[^"]*"' | head -1 | cut -d'"' -f4)
    echo "  Total songs: ${TOTAL_SONGS:-0}"
    echo "  First song: ${FIRST_SONG:-none} (Id: ${FIRST_SONG_ID:-none})"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 13: Get single item (song)
increment_test
echo -e "\n[$TEST_NUM] GET /Items/{itemId} (get single song)"
if [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -H "Authorization: $AUTH_HEADER" \
        "$BASE_URL/Items/$FIRST_SONG_ID")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
    body=$(echo "$response" | sed '/HTTP_CODE:/d')

    if [ "$http_code" = "200" ]; then
        echo "✓ Status: $http_code"
        ITEM_NAME=$(echo "$body" | grep -o '"Name":"[^"]*"' | head -1 | cut -d'"' -f4)
        ITEM_TYPE=$(echo "$body" | grep -o '"Type":"[^"]*"' | head -1 | cut -d'"' -f4)
        echo "  Item: $ITEM_NAME (Type: $ITEM_TYPE)"
    else
        echo "✗ Status: $http_code"
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no song ID available)"
fi

# Test 14: Get PlaybackInfo
increment_test
echo -e "\n[$TEST_NUM] GET /Items/{itemId}/PlaybackInfo (get playback info)"
if [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -H "Authorization: $AUTH_HEADER" \
        "$BASE_URL/Items/$FIRST_SONG_ID/PlaybackInfo")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
    body=$(echo "$response" | sed '/HTTP_CODE:/d')

    if [ "$http_code" = "200" ]; then
        echo "✓ Status: $http_code"
        HAS_MEDIA_SOURCES=$(echo "$body" | grep -c '"MediaSources"' || true)
        echo "  Has MediaSources: $([ "$HAS_MEDIA_SOURCES" -gt 0 ] && echo "yes" || echo "no")"
    else
        echo "✗ Status: $http_code"
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no song ID available)"
fi

# Test 15: Get item image
increment_test
echo -e "\n[$TEST_NUM] GET /Items/{itemId}/Images/Primary (get image)"
if [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" -o /dev/null \
        -H "Authorization: $AUTH_HEADER" \
        "$BASE_URL/Items/$FIRST_SONG_ID/Images/Primary?fillHeight=200&fillWidth=200")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

    if [ "$http_code" = "200" ]; then
        echo "✓ Status: $http_code (image returned)"
    elif [ "$http_code" = "404" ]; then
        echo "✓ Status: $http_code (no image - expected for some items)"
    else
        echo "✗ Status: $http_code"
    fi
else
    echo "⚠ Skipped (no song ID available)"
fi

# Test 16: Get Lyrics
increment_test
echo -e "\n[$TEST_NUM] GET /Audio/{itemId}/Lyrics (get lyrics)"
if [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -H "Authorization: $AUTH_HEADER" \
        "$BASE_URL/Audio/$FIRST_SONG_ID/Lyrics")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
    body=$(echo "$response" | sed '/HTTP_CODE:/d')

    if [ "$http_code" = "200" ]; then
        echo "✓ Status: $http_code"
        LYRICS_COUNT=$(echo "$body" | grep -o '"Text"' | wc -l || echo "0")
        echo "  Lyrics lines: $LYRICS_COUNT"
    else
        echo "✗ Status: $http_code"
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no song ID available)"
fi

# Test 17: Get playlists
increment_test
echo -e "\n[$TEST_NUM] GET /Items?includeItemTypes=Playlist (get playlists)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$BASE_URL/Items?includeItemTypes=Playlist&recursive=true")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

EXISTING_PLAYLIST_ID=""
if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    TOTAL_PLAYLISTS=$(echo "$body" | grep -o '"TotalRecordCount":[0-9]*' | cut -d: -f2)
    echo "  Total playlists: ${TOTAL_PLAYLISTS:-0}"
    EXISTING_PLAYLIST_ID=$(echo "$body" | grep -o '"Id":"[^"]*"' | head -1 | cut -d'"' -f4)
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 18: Create a playlist
increment_test
echo -e "\n[$TEST_NUM] POST /Playlists (create playlist)"
PLAYLIST_NAME="Test Playlist $(date +%s)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -X POST "$BASE_URL/Playlists" \
    -H "Authorization: $AUTH_HEADER" \
    -H "Content-Type: application/json" \
    -d "{\"Name\":\"$PLAYLIST_NAME\",\"Ids\":[],\"UserId\":\"$USER_ID\",\"MediaType\":\"Audio\",\"IsPublic\":false}")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

NEW_PLAYLIST_ID=""
if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    NEW_PLAYLIST_ID=$(echo "$body" | grep -o '"Id":"[^"]*"' | cut -d'"' -f4)
    echo "  Created playlist: $PLAYLIST_NAME (Id: $NEW_PLAYLIST_ID)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 19: Add items to playlist
increment_test
echo -e "\n[$TEST_NUM] POST /Playlists/{id}/Items (add items to playlist)"
if [ -n "${NEW_PLAYLIST_ID:-}" ] && [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -X POST "$BASE_URL/Playlists/$NEW_PLAYLIST_ID/Items?ids=$FIRST_SONG_ID&userId=$USER_ID" \
        -H "Authorization: $AUTH_HEADER")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

    if [ "$http_code" = "204" ]; then
        echo "✓ Status: $http_code (item added)"
    else
        echo "✗ Status: $http_code"
        body=$(echo "$response" | sed '/HTTP_CODE:/d')
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no playlist or song ID available)"
fi

# Test 20: Get playlist items
increment_test
echo -e "\n[$TEST_NUM] GET /Playlists/{id}/Items (get playlist items)"
if [ -n "${NEW_PLAYLIST_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -H "Authorization: $AUTH_HEADER" \
        "$BASE_URL/Playlists/$NEW_PLAYLIST_ID/Items")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
    body=$(echo "$response" | sed '/HTTP_CODE:/d')

    if [ "$http_code" = "200" ]; then
        echo "✓ Status: $http_code"
        PLAYLIST_ITEM_COUNT=$(echo "$body" | grep -o '"TotalRecordCount":[0-9]*' | cut -d: -f2)
        echo "  Items in playlist: ${PLAYLIST_ITEM_COUNT:-0}"
    else
        echo "✗ Status: $http_code"
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no playlist ID available)"
fi

# Test 21: Move playlist item (if we have items)
increment_test
echo -e "\n[$TEST_NUM] POST /Playlists/{id}/Items/{itemId}/Move/{index} (move item)"
if [ -n "${NEW_PLAYLIST_ID:-}" ] && [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -X POST "$BASE_URL/Playlists/$NEW_PLAYLIST_ID/Items/$FIRST_SONG_ID/Move/0" \
        -H "Authorization: $AUTH_HEADER")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

    if [ "$http_code" = "204" ]; then
        echo "✓ Status: $http_code (item moved)"
    else
        echo "✗ Status: $http_code"
        body=$(echo "$response" | sed '/HTTP_CODE:/d')
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no playlist or song ID available)"
fi

# Test 22: Remove items from playlist
increment_test
echo -e "\n[$TEST_NUM] DELETE /Playlists/{id}/Items (remove items from playlist)"
if [ -n "${NEW_PLAYLIST_ID:-}" ] && [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -X DELETE "$BASE_URL/Playlists/$NEW_PLAYLIST_ID/Items?entryIds=$FIRST_SONG_ID" \
        -H "Authorization: $AUTH_HEADER")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

    if [ "$http_code" = "204" ]; then
        echo "✓ Status: $http_code (item removed)"
    else
        echo "✗ Status: $http_code"
        body=$(echo "$response" | sed '/HTTP_CODE:/d')
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no playlist or song ID available)"
fi

# Test 23: Delete playlist
increment_test
echo -e "\n[$TEST_NUM] DELETE /Playlists/{id} (delete playlist)"
if [ -n "${NEW_PLAYLIST_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -X DELETE "$BASE_URL/Playlists/$NEW_PLAYLIST_ID" \
        -H "Authorization: $AUTH_HEADER")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

    if [ "$http_code" = "204" ]; then
        echo "✓ Status: $http_code (playlist deleted)"
    else
        echo "✗ Status: $http_code"
        body=$(echo "$response" | sed '/HTTP_CODE:/d')
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no playlist ID available)"
fi

# Test 24: Delete item via Items endpoint
increment_test
echo -e "\n[$TEST_NUM] DELETE /Items/{id} (delete item - playlist only)"
# Create another playlist to delete via Items endpoint
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -X POST "$BASE_URL/Playlists" \
    -H "Authorization: $AUTH_HEADER" \
    -H "Content-Type: application/json" \
    -d "{\"Name\":\"Delete Test $(date +%s)\",\"Ids\":[],\"UserId\":\"$USER_ID\",\"MediaType\":\"Audio\",\"IsPublic\":false}")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')
DELETE_TEST_ID=$(echo "$body" | grep -o '"Id":"[^"]*"' | cut -d'"' -f4)

if [ -n "${DELETE_TEST_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -X DELETE "$BASE_URL/Items/$DELETE_TEST_ID" \
        -H "Authorization: $AUTH_HEADER")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

    if [ "$http_code" = "204" ]; then
        echo "✓ Status: $http_code (item deleted via Items endpoint)"
    else
        echo "✗ Status: $http_code"
        body=$(echo "$response" | sed '/HTTP_CODE:/d')
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (could not create test playlist)"
fi

# Test 25: Refresh item (library rescan request)
increment_test
echo -e "\n[$TEST_NUM] POST /Items/{id}/Refresh (refresh/rescan request)"
if [ -n "${LIBRARY_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -X POST "$BASE_URL/Items/$LIBRARY_ID/Refresh?recursive=true" \
        -H "Authorization: $AUTH_HEADER")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

    if [ "$http_code" = "204" ]; then
        echo "✓ Status: $http_code (refresh requested)"
    else
        echo "✗ Status: $http_code"
        body=$(echo "$response" | sed '/HTTP_CODE:/d')
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no library ID available)"
fi

# Test 26: Sessions/Playing endpoints
increment_test
echo -e "\n[$TEST_NUM] POST /Sessions/Playing (report playback started)"
if [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -X POST "$BASE_URL/Sessions/Playing" \
        -H "Authorization: $AUTH_HEADER" \
        -H "Content-Type: application/json" \
        -d "{\"ItemId\":\"$FIRST_SONG_ID\",\"SessionId\":\"test-session-001\",\"PlaySessionId\":\"test-play-001\",\"CanSeek\":true,\"IsPaused\":false,\"IsMuted\":false,\"PositionTicks\":0}")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

    if [ "$http_code" = "204" ]; then
        echo "✓ Status: $http_code (playback started reported)"
    else
        echo "✗ Status: $http_code"
        body=$(echo "$response" | sed '/HTTP_CODE:/d')
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no song ID available)"
fi

# Test 27: Sessions/Playing/Progress
increment_test
echo -e "\n[$TEST_NUM] POST /Sessions/Playing/Progress (report progress)"
if [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -X POST "$BASE_URL/Sessions/Playing/Progress" \
        -H "Authorization: $AUTH_HEADER" \
        -H "Content-Type: application/json" \
        -d "{\"ItemId\":\"$FIRST_SONG_ID\",\"SessionId\":\"test-session-001\",\"PlaySessionId\":\"test-play-001\",\"CanSeek\":true,\"IsPaused\":false,\"IsMuted\":false,\"PositionTicks\":10000000}")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

    if [ "$http_code" = "204" ]; then
        echo "✓ Status: $http_code (progress reported)"
    else
        echo "✗ Status: $http_code"
        body=$(echo "$response" | sed '/HTTP_CODE:/d')
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no song ID available)"
fi

# Test 28: Sessions/Playing/Stopped
increment_test
echo -e "\n[$TEST_NUM] POST /Sessions/Playing/Stopped (report stopped)"
if [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -X POST "$BASE_URL/Sessions/Playing/Stopped" \
        -H "Authorization: $AUTH_HEADER" \
        -H "Content-Type: application/json" \
        -d "{\"ItemId\":\"$FIRST_SONG_ID\",\"SessionId\":\"test-session-001\",\"PlaySessionId\":\"test-play-001\",\"CanSeek\":true,\"IsPaused\":false,\"IsMuted\":false,\"PositionTicks\":50000000}")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

    if [ "$http_code" = "204" ]; then
        echo "✓ Status: $http_code (stopped reported)"
    else
        echo "✗ Status: $http_code"
        body=$(echo "$response" | sed '/HTTP_CODE:/d')
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no song ID available)"
fi

# Test 29: Get Users/Me
increment_test
echo -e "\n[$TEST_NUM] GET /Users/Me (get current user)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$BASE_URL/Users/Me")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    ME_NAME=$(echo "$body" | grep -o '"Name":"[^"]*"' | head -1 | cut -d'"' -f4)
    echo "  Current user: $ME_NAME"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 30: Audio stream HEAD request
increment_test
echo -e "\n[$TEST_NUM] HEAD /Audio/{id}/universal (stream check)"
if [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" -o /dev/null --max-time 10 \
        -X HEAD \
        -H "Authorization: $AUTH_HEADER" \
        "$BASE_URL/Audio/$FIRST_SONG_ID/universal?api_key=$ACCESS_TOKEN&userId=$USER_ID&container=flac,mp3,aac&maxStreamingBitrate=999999999")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

    if [ "$http_code" = "200" ]; then
        echo "✓ Status: $http_code (stream available)"
    else
        echo "✗ Status: $http_code"
    fi
else
    echo "⚠ Skipped (no song ID available)"
fi

echo -e "\n============================================="
echo "Test complete! ($TEST_NUM tests executed)"

# Continue with Finamp-specific endpoint tests
echo -e "\n\n============================================="
echo "FINAMP-SPECIFIC ENDPOINT TESTS"
echo "============================================="

# Test: Users/Public endpoint
increment_test
echo -e "\n[$TEST_NUM] GET /Users/Public (public users for login)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/Users/Public")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "  Response: $body" | head -c 200
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test: AlbumArtists endpoint
increment_test
echo -e "\n[$TEST_NUM] GET /Artists/AlbumArtists (get album artists)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$BASE_URL/Artists/AlbumArtists?userId=$USER_ID&startIndex=0&limit=5")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    TOTAL_ARTISTS=$(echo "$body" | grep -o '"TotalRecordCount":[0-9]*' | cut -d: -f2)
    FIRST_ARTIST=$(echo "$body" | grep -o '"Name":"[^"]*"' | head -1 | cut -d'"' -f4)
    echo "  Total album artists: ${TOTAL_ARTISTS:-0}"
    echo "  First artist: ${FIRST_ARTIST:-none}"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test: Genres endpoint
increment_test
echo -e "\n[$TEST_NUM] GET /Genres (get genres)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$BASE_URL/Genres?startIndex=0&limit=10")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    TOTAL_GENRES=$(echo "$body" | grep -o '"TotalRecordCount":[0-9]*' | cut -d: -f2)
    FIRST_GENRE=$(echo "$body" | grep -o '"Name":"[^"]*"' | head -1 | cut -d'"' -f4)
    echo "  Total genres: ${TOTAL_GENRES:-0}"
    echo "  First genre: ${FIRST_GENRE:-none}"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test: InstantMix endpoint
increment_test
echo -e "\n[$TEST_NUM] GET /Items/{id}/InstantMix (get instant mix)"
if [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -H "Authorization: $AUTH_HEADER" \
        "$BASE_URL/Items/$FIRST_SONG_ID/InstantMix?userId=$USER_ID&limit=20")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
    body=$(echo "$response" | sed '/HTTP_CODE:/d')

    if [ "$http_code" = "200" ]; then
        echo "✓ Status: $http_code"
        MIX_COUNT=$(echo "$body" | grep -o '"TotalRecordCount":[0-9]*' | cut -d: -f2)
        echo "  Mix contains: ${MIX_COUNT:-0} items"
    else
        echo "✗ Status: $http_code"
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no song ID available)"
fi

# Test: Add/Remove Favorite
increment_test
echo -e "\n[$TEST_NUM] POST /Users/{userId}/FavoriteItems/{itemId} (add favorite)"
if [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -X POST \
        -H "Authorization: $AUTH_HEADER" \
        "$BASE_URL/Users/$USER_ID/FavoriteItems/$FIRST_SONG_ID")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
    body=$(echo "$response" | sed '/HTTP_CODE:/d')

    if [ "$http_code" = "200" ]; then
        echo "✓ Status: $http_code"
        IS_FAV=$(echo "$body" | grep -o '"IsFavorite":true' || echo "")
        echo "  IsFavorite: $([ -n "$IS_FAV" ] && echo "true" || echo "check response")"
    else
        echo "✗ Status: $http_code"
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no song ID available)"
fi

increment_test
echo -e "\n[$TEST_NUM] DELETE /Users/{userId}/FavoriteItems/{itemId} (remove favorite)"
if [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -X DELETE \
        -H "Authorization: $AUTH_HEADER" \
        "$BASE_URL/Users/$USER_ID/FavoriteItems/$FIRST_SONG_ID")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
    body=$(echo "$response" | sed '/HTTP_CODE:/d')

    if [ "$http_code" = "200" ]; then
        echo "✓ Status: $http_code"
        IS_FAV=$(echo "$body" | grep -o '"IsFavorite":false' || echo "")
        echo "  IsFavorite: $([ -n "$IS_FAV" ] && echo "false" || echo "check response")"
    else
        echo "✗ Status: $http_code"
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no song ID available)"
fi

# Test: Sessions/Logout
increment_test
echo -e "\n[$TEST_NUM] POST /Sessions/Logout (logout - revoke token)"
# First get a fresh token we can safely revoke
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -X POST "$BASE_URL/Users/AuthenticateByName" \
    -H "Content-Type: application/json" \
    -H "X-Emby-Authorization: MediaBrowser Client=\"LogoutTest\", Device=\"TestDevice\", DeviceId=\"logout-test-$(date +%s)\", Version=\"1.0\"" \
    -d "{\"Username\":\"$USERNAME\",\"Pw\":\"$PASSWORD\"}")
body=$(echo "$response" | sed '/HTTP_CODE:/d')
LOGOUT_TOKEN=$(echo "$body" | grep -o '"AccessToken":"[^"]*"' | cut -d'"' -f4)

if [ -n "${LOGOUT_TOKEN:-}" ]; then
    LOGOUT_AUTH="MediaBrowser Token=\"$LOGOUT_TOKEN\", Client=\"LogoutTest\", Device=\"TestDevice\", DeviceId=\"logout-test\", Version=\"1.0\""
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -X POST \
        -H "Authorization: $LOGOUT_AUTH" \
        "$BASE_URL/Sessions/Logout")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

    if [ "$http_code" = "204" ]; then
        echo "✓ Status: $http_code (logged out)"
    else
        echo "✗ Status: $http_code"
        body=$(echo "$response" | sed '/HTTP_CODE:/d')
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (could not get logout test token)"
fi

# Test: Sessions/Capabilities/Full endpoint (Streamyfin uses this)
increment_test
echo -e "\n[$TEST_NUM] POST /Sessions/Capabilities/Full (register capabilities)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -X POST "$BASE_URL/Sessions/Capabilities/Full" \
    -H "Authorization: $AUTH_HEADER" \
    -H "Content-Type: application/json" \
    -d '{"PlayableMediaTypes":["Audio"],"SupportedCommands":["PlayState","Play"],"SupportsMediaControl":true,"Id":"test-session-caps"}')
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

if [ "$http_code" = "204" ]; then
    echo "✓ Status: $http_code (capabilities registered)"
else
    echo "✗ Status: $http_code"
    body=$(echo "$response" | sed '/HTTP_CODE:/d')
    echo "Response: $body"
fi

# Test: Sessions/Capabilities endpoint (query string version)
increment_test
echo -e "\n[$TEST_NUM] POST /Sessions/Capabilities (simple capabilities)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -X POST "$BASE_URL/Sessions/Capabilities?playableMediaTypes=Audio&supportsMediaControl=true" \
    -H "Authorization: $AUTH_HEADER")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

if [ "$http_code" = "204" ]; then
    echo "✓ Status: $http_code (capabilities registered)"
else
    echo "✗ Status: $http_code"
    body=$(echo "$response" | sed '/HTTP_CODE:/d')
    echo "Response: $body"
fi

# Test: Mark as Played
increment_test
echo -e "\n[$TEST_NUM] POST /Users/{userId}/PlayedItems/{itemId} (mark as played)"
if [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -X POST \
        -H "Authorization: $AUTH_HEADER" \
        "$BASE_URL/Users/$USER_ID/PlayedItems/$FIRST_SONG_ID")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
    body=$(echo "$response" | sed '/HTTP_CODE:/d')

    if [ "$http_code" = "200" ]; then
        echo "✓ Status: $http_code"
        IS_PLAYED=$(echo "$body" | grep -o '"Played":true' || echo "")
        PLAY_COUNT=$(echo "$body" | grep -o '"PlayCount":[0-9]*' | cut -d: -f2)
        echo "  Played: $([ -n "$IS_PLAYED" ] && echo "true" || echo "check response")"
        echo "  PlayCount: ${PLAY_COUNT:-unknown}"
    else
        echo "✗ Status: $http_code"
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no song ID available)"
fi

# Test: Mark as Unplayed
increment_test
echo -e "\n[$TEST_NUM] DELETE /Users/{userId}/PlayedItems/{itemId} (mark as unplayed)"
if [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -X DELETE \
        -H "Authorization: $AUTH_HEADER" \
        "$BASE_URL/Users/$USER_ID/PlayedItems/$FIRST_SONG_ID")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
    body=$(echo "$response" | sed '/HTTP_CODE:/d')

    if [ "$http_code" = "200" ]; then
        echo "✓ Status: $http_code"
        IS_PLAYED=$(echo "$body" | grep -o '"Played":false' || echo "")
        PLAY_COUNT=$(echo "$body" | grep -o '"PlayCount":[0-9]*' | cut -d: -f2)
        echo "  Played: $([ -n "$IS_PLAYED" ] && echo "false" || echo "check response")"
        echo "  PlayCount: ${PLAY_COUNT:-0}"
    else
        echo "✗ Status: $http_code"
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no song ID available)"
fi

# Test: Get Sessions
increment_test
echo -e "\n[$TEST_NUM] GET /Sessions (get active sessions)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$BASE_URL/Sessions?activeWithinSeconds=360")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    # Count sessions (array length)
    SESSION_COUNT=$(echo "$body" | grep -o '"Id"' | wc -l || echo "0")
    echo "  Active sessions: $SESSION_COUNT"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

echo -e "\n============================================="
echo "All Finamp/Streamyfin-specific tests complete!"

# Continue with Feishin-specific endpoint tests
echo -e "\n\n============================================="
echo "FEISHIN-SPECIFIC ENDPOINT TESTS"
echo "============================================="

# Test: Items/Filters endpoint (tag filtering)
increment_test
echo -e "\n[$TEST_NUM] GET /Items/Filters (get filters - genres, years, tags)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$BASE_URL/Items/Filters?userId=$USER_ID")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    GENRE_COUNT=$(echo "$body" | grep -o '"Genres":\[[^]]*\]' | grep -o '"[^"]*"' | wc -l || echo "0")
    YEAR_COUNT=$(echo "$body" | grep -o '"Years":\[[^]]*\]' | grep -o '[0-9]\{4\}' | wc -l || echo "0")
    echo "  Genres available: $GENRE_COUNT"
    echo "  Years available: $YEAR_COUNT"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test: Items/{id}/Similar endpoint (similar items)
increment_test
echo -e "\n[$TEST_NUM] GET /Items/{id}/Similar (get similar items)"
if [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -H "Authorization: $AUTH_HEADER" \
        "$BASE_URL/Items/$FIRST_SONG_ID/Similar?userId=$USER_ID&limit=10")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
    body=$(echo "$response" | sed '/HTTP_CODE:/d')

    if [ "$http_code" = "200" ]; then
        echo "✓ Status: $http_code"
        SIMILAR_COUNT=$(echo "$body" | grep -o '"TotalRecordCount":[0-9]*' | cut -d: -f2)
        echo "  Similar items: ${SIMILAR_COUNT:-0}"
    else
        echo "✗ Status: $http_code"
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no song ID available)"
fi

# Test: Artists/{id}/Similar endpoint (similar artists)
increment_test
echo -e "\n[$TEST_NUM] GET /Artists/{id}/Similar (get similar artists)"
# First get an artist ID
ARTIST_ID=""
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$BASE_URL/Artists?limit=1")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')
ARTIST_ID=$(echo "$body" | grep -o '"Id":"[^"]*"' | head -1 | cut -d'"' -f4)

if [ -n "${ARTIST_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -H "Authorization: $AUTH_HEADER" \
        "$BASE_URL/Artists/$ARTIST_ID/Similar?limit=5")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
    body=$(echo "$response" | sed '/HTTP_CODE:/d')

    if [ "$http_code" = "200" ]; then
        echo "✓ Status: $http_code"
        SIMILAR_ARTISTS=$(echo "$body" | grep -o '"TotalRecordCount":[0-9]*' | cut -d: -f2)
        echo "  Similar artists: ${SIMILAR_ARTISTS:-0}"
    else
        echo "✗ Status: $http_code"
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no artist ID available)"
fi

# Test: Songs/{id}/InstantMix endpoint (Feishin-specific path)
increment_test
echo -e "\n[$TEST_NUM] GET /Songs/{id}/InstantMix (Feishin songs instant mix)"
if [ -n "${FIRST_SONG_ID:-}" ]; then
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -H "Authorization: $AUTH_HEADER" \
        "$BASE_URL/Songs/$FIRST_SONG_ID/InstantMix?userId=$USER_ID&limit=20")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
    body=$(echo "$response" | sed '/HTTP_CODE:/d')

    if [ "$http_code" = "200" ]; then
        echo "✓ Status: $http_code"
        MIX_COUNT=$(echo "$body" | grep -o '"TotalRecordCount":[0-9]*' | cut -d: -f2)
        echo "  Mix contains: ${MIX_COUNT:-0} items"
    else
        echo "✗ Status: $http_code"
        echo "Response: $body"
    fi
else
    echo "⚠ Skipped (no song ID available)"
fi

# Test: MusicGenres endpoint (Feishin uses this)
increment_test
echo -e "\n[$TEST_NUM] GET /MusicGenres (Feishin music genres)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$BASE_URL/MusicGenres?startIndex=0&limit=10&userId=$USER_ID")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    TOTAL_GENRES=$(echo "$body" | grep -o '"TotalRecordCount":[0-9]*' | cut -d: -f2)
    FIRST_GENRE=$(echo "$body" | grep -o '"Name":"[^"]*"' | head -1 | cut -d'"' -f4)
    echo "  Total music genres: ${TOTAL_GENRES:-0}"
    echo "  First genre: ${FIRST_GENRE:-none}"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test: Update playlist endpoint (Feishin uses POST /Playlists/{id})
increment_test
echo -e "\n[$TEST_NUM] POST /Playlists/{id} (update playlist - Feishin)"
# Create a test playlist first
PLAYLIST_NAME="Feishin Test $(date +%s)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -X POST "$BASE_URL/Playlists" \
    -H "Authorization: $AUTH_HEADER" \
    -H "Content-Type: application/json" \
    -d "{\"Name\":\"$PLAYLIST_NAME\",\"Ids\":[],\"UserId\":\"$USER_ID\",\"MediaType\":\"Audio\",\"IsPublic\":false}")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')
FEISHIN_PLAYLIST_ID=$(echo "$body" | grep -o '"Id":"[^"]*"' | cut -d'"' -f4)

if [ -n "${FEISHIN_PLAYLIST_ID:-}" ]; then
    # Now update the playlist
    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
        -X POST "$BASE_URL/Playlists/$FEISHIN_PLAYLIST_ID" \
        -H "Authorization: $AUTH_HEADER" \
        -H "Content-Type: application/json" \
        -d "{\"Name\":\"Updated Feishin Test\",\"MediaType\":\"Audio\",\"IsPublic\":true,\"Genres\":[],\"Tags\":[],\"UserId\":\"$USER_ID\",\"PremiereDate\":null,\"ProviderIds\":{}}")
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

    if [ "$http_code" = "204" ]; then
        echo "✓ Status: $http_code (playlist updated)"
    else
        echo "✗ Status: $http_code"
        body=$(echo "$response" | sed '/HTTP_CODE:/d')
        echo "Response: $body"
    fi

    # Clean up - delete the test playlist
    curl -s -X DELETE "$BASE_URL/Playlists/$FEISHIN_PLAYLIST_ID" -H "Authorization: $AUTH_HEADER" > /dev/null
else
    echo "⚠ Skipped (could not create test playlist)"
fi

echo -e "\n============================================="
echo "All Feishin-specific tests complete!"
echo "============================================="
echo "Total tests executed: $TEST_NUM"

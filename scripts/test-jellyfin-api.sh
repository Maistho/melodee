#!/usr/bin/env bash
set -euo pipefail

# Jellyfin API endpoint tester for Melodee
# Tests the pre-auth endpoints that Jellyfin clients call during server discovery

BASE_URL="${1:-http://localhost:5157}"

echo "Testing Jellyfin API endpoints on: $BASE_URL"
echo "============================================="

# Test 0: HEAD / (server discovery - Jellyfin clients send this first)
echo -e "\n[0] HEAD / (server discovery)"
http_code=$(curl -s -o /dev/null -w "%{http_code}" -X HEAD "$BASE_URL/")

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
else
    echo "✗ Status: $http_code (expected 200)"
fi

# Test 1: /System/Info/Public (anonymous, pre-auth)
echo -e "\n[1] GET /System/Info/Public (anonymous)"
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
echo -e "\n[2] GET /System/Ping (anonymous)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/System/Ping")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

if [ "$http_code" = "204" ]; then
    echo "✓ Status: $http_code (No Content)"
else
    echo "✗ Status: $http_code (expected 204)"
fi

# Test 3: POST /System/Ping
echo -e "\n[3] POST /System/Ping (anonymous)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X POST "$BASE_URL/System/Ping")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

if [ "$http_code" = "204" ]; then
    echo "✓ Status: $http_code (No Content)"
else
    echo "✗ Status: $http_code (expected 204)"
fi

# Test 4: Direct /api/jf/System/Info/Public (should also work)
echo -e "\n[4] GET /api/jf/System/Info/Public (direct prefix)"
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
echo -e "\n[5] HEAD /api/jf (direct root)"
http_code=$(curl -s -o /dev/null -w "%{http_code}" -X HEAD "$BASE_URL/api/jf")

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
else
    echo "✗ Status: $http_code (expected 200)"
fi

# Test 6: Authenticated endpoint without token (should fail)
echo -e "\n[6] GET /UserViews (no auth - should fail)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/UserViews")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

if [ "$http_code" = "404" ] || [ "$http_code" = "302" ]; then
    echo "✓ Status: $http_code (correctly rejected - no Jellyfin headers)"
else
    echo "? Status: $http_code"
fi

# Test 7: With MediaBrowser header but no valid token
echo -e "\n[7] GET /UserViews (with MediaBrowser header, invalid token)"
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
echo -e "\n[8] POST /Users/AuthenticateByName (get token)"
USERNAME="${JF_USERNAME:-steven}"
PASSWORD="${JF_PASSWORD:-password}"

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
echo -e "\n[9] GET /UserViews (get libraries)"
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
echo -e "\n[10] GET /Items?includeItemTypes=MusicAlbum (get albums)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$BASE_URL/Items?includeItemTypes=MusicAlbum&limit=10&startIndex=0&recursive=true")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    TOTAL_ALBUMS=$(echo "$body" | grep -o '"TotalRecordCount":[0-9]*' | cut -d: -f2)
    FIRST_ALBUM=$(echo "$body" | grep -o '"Name":"[^"]*"' | head -1 | cut -d'"' -f4)
    echo "  Total albums: ${TOTAL_ALBUMS:-0}"
    echo "  First album: ${FIRST_ALBUM:-none}"
    echo "  Response preview: $(echo "$body" | head -c 500)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 11: Get Artists
echo -e "\n[11] GET /Items?includeItemTypes=MusicArtist (get artists)"
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
echo -e "\n[12] GET /Items?includeItemTypes=Audio (get songs)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$BASE_URL/Items?includeItemTypes=Audio&limit=10&startIndex=0&recursive=true")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    TOTAL_SONGS=$(echo "$body" | grep -o '"TotalRecordCount":[0-9]*' | cut -d: -f2)
    FIRST_SONG=$(echo "$body" | grep -o '"Name":"[^"]*"' | head -1 | cut -d'"' -f4)
    echo "  Total songs: ${TOTAL_SONGS:-0}"
    echo "  First song: ${FIRST_SONG:-none}"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

echo -e "\n============================================="
echo "Test complete!"

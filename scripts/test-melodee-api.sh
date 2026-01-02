#!/usr/bin/env bash
set -euo pipefail

# Melodee API endpoint tester
# Tests the core Melodee REST API endpoints

BASE_URL="${1:-http://localhost:5157}"
API_VERSION="v1"
API_BASE="$BASE_URL/api/$API_VERSION"

echo "Testing Melodee API endpoints on: $API_BASE"
echo "============================================="

# Test 1: System Info (anonymous)
echo -e "\n[1] GET /api/v1/system/info (anonymous)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$API_BASE/system/info")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response: $(echo "$body" | head -c 300)"
else
    echo "✗ Status: $http_code (expected 200)"
    echo "Response: $body"
fi

# Test 2: Attempt to access protected endpoint without auth
echo -e "\n[2] GET /api/v1/user/me (no auth - should fail)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$API_BASE/user/me")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)

if [ "$http_code" = "401" ]; then
    echo "✓ Status: $http_code (correctly unauthorized)"
else
    echo "? Status: $http_code (expected 401)"
fi

# Test 3: Authenticate
echo -e "\n[3] POST /api/v1/auth/authenticate"
USERNAME="${MELODEE_USERNAME:-steven}"
PASSWORD="${MELODEE_PASSWORD:-password}"

login_response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -X POST "$API_BASE/auth/authenticate" \
    -H "Content-Type: application/json" \
    -d "{\"username\":\"$USERNAME\",\"password\":\"$PASSWORD\"}")

http_code=$(echo "$login_response" | grep "HTTP_CODE:" | cut -d: -f2)
login_body=$(echo "$login_response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code (authenticated)"
    ACCESS_TOKEN=$(echo "$login_body" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
    if [ -n "$ACCESS_TOKEN" ]; then
        echo "  Token: ${ACCESS_TOKEN:0:30}..."
    else
        echo "  Warning: Could not extract token from response"
        echo "  Response: $(echo "$login_body" | head -c 200)"
    fi
else
    echo "✗ Status: $http_code (login failed)"
    echo "Response: $login_body"
    echo -e "\n============================================="
    echo "Test complete (stopped at authentication failure)!"
    exit 1
fi

AUTH_HEADER="Bearer $ACCESS_TOKEN"

# Test 4: Get current user
echo -e "\n[4] GET /api/v1/user/me (authenticated)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$API_BASE/user/me")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response: $(echo "$body" | head -c 300)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 5: Get artists
echo -e "\n[5] GET /api/v1/artists (get artists)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$API_BASE/artists?page=1&pageSize=10")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response preview: $(echo "$body" | head -c 400)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 6: Get albums
echo -e "\n[6] GET /api/v1/albums (get albums)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$API_BASE/albums?page=1&pageSize=10")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response preview: $(echo "$body" | head -c 400)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 7: Get songs
echo -e "\n[7] GET /api/v1/songs (get songs)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$API_BASE/songs?page=1&pageSize=10")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response preview: $(echo "$body" | head -c 400)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 8: Search (POST endpoint)
echo -e "\n[8] POST /api/v1/search (search for 'love')"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -X POST \
    -H "Authorization: $AUTH_HEADER" \
    -H "Content-Type: application/json" \
    -d '{"query":"love","maxResults":5}' \
    "$API_BASE/search")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response preview: $(echo "$body" | head -c 400)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 9: Get playlists
echo -e "\n[9] GET /api/v1/playlists (get playlists)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$API_BASE/playlists?page=1&pageSize=10")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response preview: $(echo "$body" | head -c 400)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 10: Get genres
echo -e "\n[10] GET /api/v1/genres (get genres)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -H "Authorization: $AUTH_HEADER" \
    "$API_BASE/genres")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response preview: $(echo "$body" | head -c 400)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

echo -e "\n============================================="
echo "Test complete!"

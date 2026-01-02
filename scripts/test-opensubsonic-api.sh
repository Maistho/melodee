#!/usr/bin/env bash
set -euo pipefail

# OpenSubsonic API endpoint tester
# Tests the OpenSubsonic REST API endpoints
# See: https://opensubsonic.netlify.app/

BASE_URL="${1:-http://localhost:5157}"

echo "Testing OpenSubsonic API endpoints on: $BASE_URL"
echo "============================================="

# OpenSubsonic authentication parameters
USERNAME="${SUBSONIC_USERNAME:-steven}"
PASSWORD="${SUBSONIC_PASSWORD:-password}"

# Subsonic uses either:
# 1. Plain password with 'p' parameter (legacy, not recommended)
# 2. Token-based auth with 't' (token) and 's' (salt) parameters
# Token = md5(password + salt)

# Generate a random salt
SALT=$(head -c 8 /dev/urandom | xxd -p)

# Calculate token: md5(password + salt)
TOKEN=$(echo -n "${PASSWORD}${SALT}" | md5sum | cut -d' ' -f1)

# Common auth parameters
AUTH_PARAMS="u=${USERNAME}&t=${TOKEN}&s=${SALT}&v=1.16.1&c=MelodeeTestScript&f=json"

# Test 1: Ping (basic connectivity test)
echo -e "\n[1] GET /rest/ping (anonymous ping)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/rest/ping?$AUTH_PARAMS")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response: $body"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 2: Get OpenSubsonic Extensions
echo -e "\n[2] GET /rest/getOpenSubsonicExtensions (get supported extensions)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/rest/getOpenSubsonicExtensions?$AUTH_PARAMS")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response: $(echo "$body" | head -c 500)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 3: Get License
echo -e "\n[3] GET /rest/getLicense (get license info)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/rest/getLicense?$AUTH_PARAMS")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response: $body"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 4: Get Music Folders
echo -e "\n[4] GET /rest/getMusicFolders (get music libraries)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/rest/getMusicFolders?$AUTH_PARAMS")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response: $body"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 5: Get Indexes (artists index)
echo -e "\n[5] GET /rest/getIndexes (get artist index)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/rest/getIndexes?$AUTH_PARAMS")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response preview: $(echo "$body" | head -c 500)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 6: Get Artists
echo -e "\n[6] GET /rest/getArtists (get all artists)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/rest/getArtists?$AUTH_PARAMS")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response preview: $(echo "$body" | head -c 500)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 7: Get Album List (recently added)
echo -e "\n[7] GET /rest/getAlbumList2 (get recent albums)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/rest/getAlbumList2?type=newest&size=10&$AUTH_PARAMS")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response preview: $(echo "$body" | head -c 500)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 8: Get Random Songs
echo -e "\n[8] GET /rest/getRandomSongs (get random songs)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/rest/getRandomSongs?size=5&$AUTH_PARAMS")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response preview: $(echo "$body" | head -c 500)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 9: Search3 (search for content)
echo -e "\n[9] GET /rest/search3 (search for 'love')"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/rest/search3?query=love&artistCount=3&albumCount=3&songCount=3&$AUTH_PARAMS")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response preview: $(echo "$body" | head -c 500)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 10: Get Playlists
echo -e "\n[10] GET /rest/getPlaylists (get playlists)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/rest/getPlaylists?$AUTH_PARAMS")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response: $(echo "$body" | head -c 500)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 11: Get Genres
echo -e "\n[11] GET /rest/getGenres (get genres)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/rest/getGenres?$AUTH_PARAMS")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response: $(echo "$body" | head -c 500)"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 12: Get User
echo -e "\n[12] GET /rest/getUser (get current user)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/rest/getUser?username=$USERNAME&$AUTH_PARAMS")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✓ Status: $http_code"
    echo "Response: $body"
else
    echo "✗ Status: $http_code"
    echo "Response: $body"
fi

# Test 13: Invalid auth test
echo -e "\n[13] GET /rest/ping (invalid auth - should fail)"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL/rest/ping?u=baduser&t=badtoken&s=badsalt&v=1.16.1&c=Test&f=json")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

# OpenSubsonic returns 200 with error in body for auth failures
if echo "$body" | grep -q '"status":"failed"'; then
    echo "✓ Authentication correctly rejected"
    echo "Response: $(echo "$body" | head -c 200)"
elif [ "$http_code" = "401" ]; then
    echo "✓ Status: $http_code (correctly unauthorized)"
else
    echo "? Status: $http_code"
    echo "Response: $body"
fi

echo -e "\n============================================="
echo "Test complete!"

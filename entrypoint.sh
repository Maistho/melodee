#!/bin/bash
set -e

echo "=== Melodee Container Startup ==="

# Fix volume permissions (runs as root initially)
echo "Fixing volume permissions..."
chown -R melodee:melodee /app/storage /app/inbound /app/staging /app/user-images /app/playlists /app/templates /app/Logs 2>/dev/null || true
chmod -R 755 /app/storage /app/inbound /app/staging /app/user-images /app/playlists /app/templates 2>/dev/null || true

# Wait for database to be ready
echo "Waiting for database..."
until pg_isready -h melodee-db -p 5432 -U melodeeuser -d melodeedb -q; do
    echo "Database not ready, retrying in 2 seconds..."
    sleep 2
done
echo "Database is ready!"

# Run database migrations using the pre-built bundle
echo "Running database migrations..."
if [ -f /app/efbundle ]; then
    /app/efbundle --connection "Host=melodee-db;Port=5432;Database=melodeedb;Username=melodeeuser;Password=${DB_PASSWORD};SSL Mode=Disable"
    echo "Migrations completed!"
else
    echo "Warning: Migration bundle not found, skipping migrations"
fi

# Switch to melodee user and start the application
echo "Starting Melodee server..."
exec su melodee -c 'cd /app && exec dotnet server.dll'

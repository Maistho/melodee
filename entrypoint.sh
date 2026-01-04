#!/bin/bash
set -e

echo "Fixing volume permissions for melodee user..."
chown -R melodee:melodee /app/storage /app/inbound /app/staging /app/user-images /app/playlists /app/Logs
chmod -R 755 /app/storage /app/inbound /app/staging /app/user-images /app/playlists
mkdir -p /app/Logs
chown melodee:melodee /app/Logs
echo "Permissions fixed!"

echo "Starting application as melodee user..."

# Create a script that melodee user will execute
cat > /tmp/run-melodee.sh << 'EOFSCRIPT'
#!/bin/bash
set -e
export PATH="$PATH:/home/melodee/.dotnet/tools"

echo "Starting container..."
echo "Container environment:"
env | grep -E "(DB|CONNECTION)" | grep -v PASSWORD || true

echo "Testing database connectivity..."
until pg_isready -h melodee-db -p 5432 -U melodeeuser -d melodeedb; do
    echo "Waiting for database..."
    sleep 2
done
echo "Database is ready!"

echo "Running database migrations..."
cd /app/src/Melodee.Blazor
dotnet restore
dotnet-ef database update --context MelodeeDbContext
echo "Migrations completed!"

echo "Starting application..."
cd /app
exec dotnet server.dll
EOFSCRIPT

chmod +x /tmp/run-melodee.sh
exec su melodee -c '/tmp/run-melodee.sh'

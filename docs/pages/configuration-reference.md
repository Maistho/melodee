---
title: Configuration Reference
permalink: /configuration-reference/
---

# Configuration Reference

This page provides a comprehensive reference of all configuration options available in Melodee, organized by category.

## Environment Variables

### Database Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `DB_PASSWORD` | - | Required: Password for PostgreSQL database |
| `DB_MIN_POOL_SIZE` | 10 | Minimum number of database connections in the pool |
| `DB_MAX_POOL_SIZE` | 50 | Maximum number of database connections in the pool |
| `ConnectionStrings__DefaultConnection` | - | Full connection string for PostgreSQL database |

### Application Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `MELODEE_PORT` | 8080 | Port on which Melodee will run |
| `MELODEE_STORAGE_PATH` | /app/storage | Path for processed music files |
| `MELODEE_INBOUND_PATH` | /app/inbound | Path for new music files to be processed |
| `MELODEE_STAGING_PATH` | /app/staging | Path for music files awaiting review |
| `MELODEE_USER_IMAGES_PATH` | /app/user-images | Path for user avatars and images |
| `MELODEE_PLAYLISTS_PATH` | /app/playlists | Path for playlist definitions |
| `MELODEE_TEMPLATES_PATH` | /app/templates | Path for email templates organized by language |
| `SEARCHENGINE_MUSICBRAINZ_STORAGEPATH` | /app/storage/_search-engines/musicbrainz | Path for MusicBrainz database |

### Authentication Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `Auth__Google__Enabled` | false | Enable Google OAuth authentication |
| `Auth__Google__ClientId` | - | Google OAuth Client ID |
| `Auth__Google__AllowedHostedDomains` | - | Restrict Google login to specific domains |
| `Auth__Google__AutoLinkEnabled` | false | Automatically link Google accounts |
| `Auth__Tokens__AccessTokenLifetimeMinutes` | 15 | Access token lifetime in minutes |
| `Auth__Tokens__RefreshTokenLifetimeDays` | 30 | Refresh token lifetime in days |
| `Auth__SelfRegistrationEnabled` | true | Allow user self-registration |

### JWT Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `Jwt__Key` | - | Required: JWT signing key (256-bit secret) |
| `Jwt__Issuer` | melodee | JWT token issuer |
| `Jwt__Audience` | melodee-clients | JWT token audience |

### Streaming Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `Streaming__UseBufferedResponses` | false | Use buffered responses for streaming |
| `Streaming__MaxConcurrentStreamsPerUser` | - | Maximum concurrent streams per user |

### Brave Search API Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `BRAVE_SEARCH__ENABLED` | false | Enable Brave Search API for images |
| `BRAVE_SEARCH__APIKEY` | - | Brave Search API key |
| `BRAVE_SEARCH__BASEURL` | https://api.search.brave.com | Brave Search API base URL |
| `BRAVE_SEARCH__IMAGESEARCHPATH` | /res/v1/images/search | Image search API path |

### Plugin Configuration

Override plugin settings using these environment variables (standard .NET configuration mapping):

| Variable | Description |
|----------|-------------|
| `Plugins__MetadataProviders__Spotify__Enabled` | Enable/Disable Spotify metadata |
| `Plugins__MetadataProviders__Spotify__ClientId` | Spotify Client ID |
| `Plugins__MetadataProviders__Spotify__ClientSecret` | Spotify Client Secret |
| `Plugins__MetadataProviders__LastFm__Enabled` | Enable/Disable Last.FM metadata |
| `Plugins__MetadataProviders__LastFm__ApiKey` | Last.FM API Key |
| `Plugins__MetadataProviders__LastFm__ApiSecret` | Last.FM Shared Secret |
| `Plugins__MetadataProviders__MusicBrainz__Enabled` | Enable/Disable MusicBrainz (local) |
| `Plugins__MetadataProviders__Itunes__Enabled` | Enable/Disable iTunes metadata |
| `Plugins__MetadataProviders__Deezer__Enabled` | Enable/Disable Deezer metadata |

## AppSettings Configuration

### System Configuration

```json
{
  "System": {
    "AppName": "Melodee",
    "AppVersion": "1.0.0",
    "Environment": "Production"
  }
}
```

### Logging Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "/app/Logs/melodee-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

### Cache Configuration

```json
{
  "Caching": {
    "DefaultCacheTimeMinutes": 60,
    "MemoryCacheSizeLimit": 100000000,
    "Redis": {
      "Enabled": false,
      "ConnectionString": "localhost:6379"
    }
  }
}
```

### Job Configuration

```json
{
  "Jobs": {
    "ScanInboundIntervalMinutes": 60,
    "MetadataRefreshIntervalHours": 24,
    "ArtworkRefreshIntervalHours": 168,
    "CleanupIntervalHours": 24
  }
}
```

### Transcoding Configuration

```json
{
  "Transcoding": {
    "Enabled": true,
    "FFmpegPath": "ffmpeg",
    "Presets": {
      "LowQuality": "-b:a 128k",
      "MediumQuality": "-b:a 256k", 
      "HighQuality": "-b:a 320k",
      "Lossless": "-c:a copy"
    }
  }
}
```

### Plugin Configuration

```json
{
  "Plugins": {
    "MetadataProviders": {
      "MusicBrainz": {
        "Enabled": true,
        "CachePath": "/app/storage/_search-engines/musicbrainz"
      },
      "LastFm": {
        "Enabled": false,
        "ApiKey": "",
        "ApiSecret": ""
      },
      "Spotify": {
        "Enabled": false,
        "ClientId": "",
        "ClientSecret": ""
      },
      "Itunes": {
        "Enabled": true
      },
      "Deezer": {
        "Enabled": true
      },
      "MetalApi": {
        "Enabled": false
      }
    }
  }
}
```

### Security Configuration

```json
{
  "Security": {
    "RateLimiting": {
      "Enabled": true,
      "RequestsPerMinute": 100
    },
    "Blacklist": {
      "Enabled": true,
      "CheckEmail": true,
      "CheckIP": true
    }
  }
}
```

## UI Configuration Options

### Theme Configuration

The web UI supports various theming options that can be configured through the application settings:

```json
{
  "Theme": {
    "DefaultTheme": "dark",
    "Themes": ["light", "dark", "auto"],
    "PrimaryColor": "#3498db",
    "SecondaryColor": "#2ecc71"
  }
}
```

### Feature Flags

```json
{
  "Features": {
    "EnableAdvancedSearch": true,
    "EnableUserPlaylists": true,
    "EnableSharing": false,
    "EnablePublicAccess": false
  }
}
```

## File-Based Configuration

In addition to environment variables, Melodee supports configuration through `appsettings.json` files. The configuration hierarchy is:

1. `appsettings.json` (base configuration)
2. `appsettings.{Environment}.json` (environment-specific overrides)
3. Environment variables (highest priority)

### Example Configuration File

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=melodeedb;Username=melodeeuser;Password=yourpassword;Pooling=true;MinPoolSize=10;MaxPoolSize=50;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Key": "your-256-bit-secret-key-here-must-be-exactly-32-bytes-long",
    "Issuer": "melodee",
    "Audience": "melodee-clients",
    "ExpireMinutes": 15
  },
  "Auth": {
    "SelfRegistrationEnabled": true,
    "Google": {
      "Enabled": false,
      "ClientId": "",
      "ClientSecret": "",
      "AllowedHostedDomains": [],
      "AutoLinkEnabled": false
    },
    "Tokens": {
      "AccessTokenLifetimeMinutes": 15,
      "RefreshTokenLifetimeDays": 30,
      "MaxSessionDays": 90
    }
  },
  "Storage": {
    "StoragePath": "/app/storage",
    "InboundPath": "/app/inbound",
    "StagingPath": "/app/staging",
    "UserImagesPath": "/app/user-images",
    "PlaylistsPath": "/app/playlists"
  }
}
```

## Configuration Best Practices

### Security

- Always use strong passwords for database access
- Store sensitive configuration (like JWT keys) in environment variables or secure vaults
- Use HTTPS in production environments
- Regularly rotate API keys and JWT signing keys

### Performance

- Adjust database connection pool sizes based on expected load
- Use SSD storage for database volumes
- Configure appropriate caching strategies
- Set realistic job scheduling intervals based on your library size

### Maintenance

- Regularly review and update configuration as your library grows
- Monitor log levels to balance debugging information with performance
- Test configuration changes in a staging environment when possible
- Document custom configurations for backup and recovery purposes

## Troubleshooting Configuration Issues

### Common Issues

1. **Database Connection Issues**:
   - Verify `DB_PASSWORD` is set correctly
   - Check that PostgreSQL is accessible
   - Confirm connection string format

2. **File Path Issues**:
   - Ensure all configured paths exist and are writable
   - Check volume mounts in containerized deployments
   - Verify permissions for configured directories

3. **Authentication Issues**:
   - Confirm JWT key is properly formatted (32+ bytes)
   - Verify OAuth provider settings are correct
   - Check that authentication providers are enabled

### Configuration Validation

When making configuration changes:
1. Restart the application to apply changes
2. Check application logs for configuration-related errors
3. Test API endpoints to verify functionality
4. Validate that scheduled jobs are running as expected

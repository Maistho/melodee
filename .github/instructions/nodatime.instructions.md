---
description: 'NodaTime usage guidelines - always use NodaTime types instead of .NET DateTime/DateTimeOffset'
applyTo: '**/*.cs'
---

# NodaTime Usage Guidelines

## Overview

This project uses **NodaTime** for all date/time handling. NodaTime provides a cleaner, more explicit API for working with dates and times compared to the built-in .NET types.

## Required Types

Always use NodaTime types instead of .NET built-in types:

| .NET Type | NodaTime Replacement | Use Case |
|-----------|---------------------|----------|
| `DateTime` | `Instant` | Points in time (timestamps, created/updated dates) |
| `DateTime` | `LocalDateTime` | Date and time without timezone |
| `DateTimeOffset` | `Instant` | Timestamps with timezone awareness |
| `DateOnly` | `LocalDate` | Dates without time component |
| `TimeOnly` | `LocalTime` | Times without date component |
| `TimeSpan` | `Duration` | Elapsed time / intervals |
| `TimeSpan` | `Period` | Calendar-based intervals (months, years) |

## Entity Models

All entity date/time properties MUST use `Instant`:

```csharp
// CORRECT
using NodaTime;

public class MyEntity : DataModelBase
{
    public Instant? LastPlayedAt { get; set; }
    public Instant? ExpiresAt { get; set; }
    public Instant PublishedAt { get; set; }
}

// WRONG - Never use these in entity models
public class MyEntity
{
    public DateTime? LastPlayedAt { get; set; }        // NO
    public DateTimeOffset? ExpiresAt { get; set; }    // NO
}
```

## Why This Matters

1. **SQLite Compatibility**: SQLite EF Core provider doesn't support `DateTimeOffset` in ORDER BY clauses. NodaTime's `Instant` type works correctly with SQLite when using `UseNodaTime()`.

2. **Consistency**: The entire codebase uses NodaTime. Mixing types causes conversion issues and maintenance burden.

3. **Clarity**: NodaTime types make the code's intent explicit (e.g., `Instant` is unambiguously a point in time in UTC).

## Database Configuration

The DbContext is configured to use NodaTime:

```csharp
optionsBuilder.UseSqlite(connectionString, x => x.UseNodaTime());
// or
optionsBuilder.UseNpgsql(connectionString, x => x.UseNodaTime());
```

## Common Patterns

### Getting Current Time
```csharp
using NodaTime;

var now = SystemClock.Instance.GetCurrentInstant();
```

### Converting from External Sources
When receiving `DateTimeOffset` from external APIs (RSS feeds, HTTP headers, etc.):

```csharp
// Convert DateTimeOffset to Instant
DateTimeOffset externalDate = GetFromExternalSource();
Instant instant = Instant.FromDateTimeOffset(externalDate);

// Handle potential MinValue (represents "no date")
episode.PublishDate = externalDate != DateTimeOffset.MinValue 
    ? Instant.FromDateTimeOffset(externalDate) 
    : null;
```

### Extracting Date Components
```csharp
Instant instant = SystemClock.Instance.GetCurrentInstant();

// Get year, month, day (requires converting to ZonedDateTime)
int year = instant.InUtc().Year;
int month = instant.InUtc().Month;
int day = instant.InUtc().Day;
```

### Formatting for Display
```csharp
Instant instant = entity.CreatedAt;

// Convert to DateTime for standard formatting
DateTime dateTime = instant.ToDateTimeUtc();
string formatted = dateTime.ToString("yyyy-MM-dd");

// Or use NodaTime patterns
string formatted = instant.InUtc().LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss", null);
```

### Duration and Arithmetic
```csharp
var now = SystemClock.Instance.GetCurrentInstant();
var duration = Duration.FromHours(2);
var twoHoursAgo = now.Minus(duration);
var twoHoursFromNow = now.Plus(duration);
```

## DTOs and API Models

For DTOs that cross API boundaries, prefer `Instant` when possible. If the external API requires `DateTimeOffset`, convert at the boundary:

```csharp
// Internal DTO
public record MyDataInfo(
    int Id,
    Instant CreatedAt,
    Instant? LastModified
);

// API response mapping (if needed)
public DateTimeOffset CreatedAtDto => CreatedAt.ToDateTimeOffset();
```

## Test Data

When creating test data, use NodaTime:

```csharp
var entity = new MyEntity
{
    CreatedAt = SystemClock.Instance.GetCurrentInstant(),
    PublishDate = Instant.FromUtc(2025, 1, 15, 10, 30),
    ExpiresAt = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(30))
};
```

## Exceptions

The only acceptable use of `DateTimeOffset` is when storing HTTP header values that are naturally `DateTimeOffset` and are NOT used in database queries (e.g., `Last-Modified` header for caching):

```csharp
public class PodcastChannel
{
    // OK - This comes from HTTP Last-Modified header and is not queried/sorted
    public DateTimeOffset? LastModified { get; set; }
    
    // These MUST be Instant - they are used in queries
    public Instant? LastSyncAt { get; set; }
    public Instant CreatedAt { get; set; }
}
```

## References

- [NodaTime Documentation](https://nodatime.org/3.1.x/userguide)
- [EF Core NodaTime Provider](https://www.npgsql.org/efcore/mapping/nodatime.html)

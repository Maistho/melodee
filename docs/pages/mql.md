---
title: Melodee Query Language (MQL)
permalink: /mql/
---

# Melodee Query Language (MQL)

MQL is a powerful query language for searching your music library. It allows you to search by specific fields, use comparisons, combine conditions with boolean logic, and more.

## Quick Start

Access MQL search by clicking the **Advanced** button in the Search page. Select an entity type (Songs, Albums, or Artists) and enter your query.

### Basic Examples

```
artist:"Pink Floyd" AND year:>=1970
genre:Jazz rating:>=4
title:/.*remix.*/i
starred:true lastPlayedAt:-30d
```

## Syntax Overview

MQL queries consist of terms that can be combined with boolean operators.

### Term Types

| Type | Syntax | Example |
|------|--------|---------|
| Free text | `word` or `"phrase"` | `Beatles` or `"Abbey Road"` |
| Field filter | `field:value` | `artist:Beatles` |
| Comparison | `field:>value` | `year:>=2000` |
| Range | `field:start-end` | `year:1970-1980` |
| Regex | `field:/pattern/flags` | `title:/.*live.*/i` |

### Boolean Operators

Combine terms using boolean logic:

- **AND**: Both conditions must match (default when omitted)
- **OR**: Either condition must match
- **NOT**: Exclude matching results
- **( )**: Group conditions

**Operator Precedence** (highest to lowest):
1. Parentheses `( )`
2. `NOT`
3. `AND`
4. `OR`

### Examples

```
# All conditions must match (implicit AND)
artist:Beatles album:"Abbey Road"

# Explicit boolean logic
(rock OR metal) AND NOT live

# Complex grouping
(artist:Beatles OR artist:"Rolling Stones") AND year:1965-1970
```

## Field Reference

### Song Fields

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `title` | string | Song title | `title:"Comfortably Numb"` |
| `artist` | string | Artist name | `artist:"Pink Floyd"` |
| `album` | string | Album name | `album:"The Wall"` |
| `genre` | string | Genre tag | `genre:Jazz` |
| `mood` | string | Mood tag | `mood:Chill` |
| `year` | number | Release year | `year:1979` or `year:1970-1980` |
| `duration` | number | Duration in seconds | `duration:<300` (under 5 min) |
| `bpm` | number | Beats per minute | `bpm:>120` |
| `rating` | number | Your rating (0-5) | `rating:>=4` |
| `plays` | number | Your play count | `plays:>10` |
| `starred` | boolean | Is starred/liked | `starred:true` |
| `starredAt` | date | When you starred it | `starredAt:last-week` |
| `lastPlayedAt` | date | When you last played it | `lastPlayedAt:-30d` |

### Album Fields

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `album` / `name` | string | Album name | `album:"Abbey Road"` |
| `artist` | string | Artist name | `artist:Beatles` |
| `year` | number | Release year | `year:1969` |
| `duration` | number | Total duration (seconds) | `duration:<3600` |
| `genre` | string | Genre tag | `genre:Rock` |
| `mood` | string | Mood tag | `mood:Chill` |
| `rating` | number | Your rating (0-5) | `rating:>=4` |
| `plays` | number | Your play count | `plays:>0` |
| `starred` | boolean | Is starred/liked | `starred:true` |
| `starredAt` | date | When you starred it | `starredAt:last-month` |
| `lastPlayedAt` | date | When you last played it | `lastPlayedAt:-30d` |
| `added` | date | When added to library | `added:-30d` |

### Artist Fields

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `artist` / `name` | string | Artist name | `artist:"Miles Davis"` |
| `rating` | number | Your rating (0-5) | `rating:>=4` |
| `starred` | boolean | Is starred/liked | `starred:true` |
| `starredAt` | date | When you starred it | `starredAt:last-year` |
| `plays` | number | Global play count | `plays:>0` |
| `added` | date | When added to library | `added:last-month` |

## Operators

### Comparison Operators

| Operator | Meaning | Example |
|----------|---------|---------|
| `:` | Equals (strings: contains) | `artist:Beatles` |
| `:=` | Exact equals | `year:=1969` |
| `:>` | Greater than | `rating:>3` |
| `:>=` | Greater than or equal | `year:>=2000` |
| `:<` | Less than | `duration:<300` |
| `:<=` | Less than or equal | `bpm:<=100` |
| `:!=` | Not equal | `genre:!=Classical` |

### Range Operator

Use a hyphen for inclusive ranges:

```
year:1970-1980       # 1970 to 1980 inclusive
duration:180-300     # 3 to 5 minutes
rating:3-5           # Ratings 3, 4, or 5
```

### Regex Operator

For advanced pattern matching (case-insensitive with `i` flag):

```
title:/.*remix.*/i           # Contains "remix" (any case)
artist:/^The .*/             # Starts with "The "
album:/.*\(live\)$/i         # Ends with "(live)"
```

**Note**: Regex queries are limited for performance. Use sparingly on large libraries.

## Date Values

### Absolute Dates

Use ISO format: `YYYY-MM-DD`

```
added:2024-01-01
lastPlayedAt:2024-06-15
```

### Relative Dates

**Named shortcuts:**
- `today` - Current day
- `yesterday` - Previous day
- `last-week` - Past 7 days
- `last-month` - Past 30 days
- `last-year` - Past 365 days

**Duration syntax:**
- `-7d` - Past 7 days
- `-3w` - Past 3 weeks
- `-12h` - Past 12 hours
- `-6m` - Past 6 months

```
added:last-week          # Added in past 7 days
lastPlayedAt:-30d        # Played in past 30 days
starredAt:last-month     # Starred in past 30 days
```

## Common Query Patterns

### Finding Unplayed Music

```
# Songs never played
plays:0

# Albums added recently but not played
added:last-month plays:0
```

### Finding Your Favorites

```
# Highly rated songs
rating:>=4

# Starred songs from a specific artist
starred:true artist:"Pink Floyd"

# Your top rated albums
rating:5
```

### Discovery Queries

```
# Jazz you haven't heard in a while
genre:Jazz lastPlayedAt:<-90d

# Short songs (under 3 minutes) you might have missed
duration:<180 plays:0

# High BPM tracks for workout
bpm:>140 genre:(Electronic OR Dance)
```

### By Era

```
# Classic rock from the 70s
year:1970-1979 genre:Rock

# Recent additions to your library
added:-7d

# Music from this millennium
year:>=2000
```

### Complex Searches

```
# Pink Floyd or Roger Waters, but not compilations
(artist:"Pink Floyd" OR artist:"Roger Waters") NOT album:/.*greatest.*/i

# Jazz or Blues albums you've rated highly
(genre:Jazz OR genre:Blues) rating:>=4

# Long prog rock tracks
genre:"Progressive Rock" duration:>600
```

## Tips and Best Practices

### Quote Strings with Spaces

```
# Correct
artist:"Pink Floyd"
album:"The Dark Side of the Moon"

# Incorrect (will search for separate terms)
artist:Pink Floyd
```

### Use Parentheses for Clarity

```
# Clear intent
(rock OR metal) AND year:>=2000

# Ambiguous (AND has higher precedence than OR)
rock OR metal AND year:>=2000
```

### Combine with Simple Search

For quick searches, use the simple search box. Switch to MQL Advanced mode when you need:
- Field-specific searches
- Numeric comparisons
- Date ranges
- Boolean logic

### Performance Tips

- Be specific with fields to narrow results faster
- Avoid regex on very large libraries
- Use date ranges instead of open-ended comparisons when possible

## Error Messages

If your query has a syntax error, MQL will show:
- **Error position**: Where the problem occurred
- **Suggestions**: Possible fixes or similar field names
- **Valid fields**: List of fields available for the entity type

Common errors:
- **Unknown field**: Check spelling (e.g., `artistt` → `artist`)
- **Invalid literal**: Check value format (e.g., `year:abc` should be `year:2024`)
- **Unbalanced parentheses**: Ensure all `(` have matching `)`
- **Invalid date format**: Use ISO dates or relative shortcuts

## API Access

MQL is also available via the API:

### Parse/Validate Query

```http
POST /api/v1/query/parse
Content-Type: application/json

{
  "entity": "songs",
  "query": "artist:\"Pink Floyd\" AND year:>=1970"
}
```

### Query Suggestions (Autocomplete)

```http
POST /api/v1/query/suggest
Content-Type: application/json

{
  "entity": "songs",
  "query": "art",
  "cursorPosition": 3
}
```

---

For more details on the API, see the [API Reference](/api/).

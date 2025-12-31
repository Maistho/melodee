---
title: Charts
permalink: /charts/
---

# Charts

Charts in Melodee allow you to import, manage, and explore curated lists of albums from music publications, websites, or your own custom rankings. Think of charts as "best of" lists—like Rolling Stone's 500 Greatest Albums, Pitchfork's Album of the Year, or your personal top albums of 2024.

## What Are Charts?

Charts are ranked lists of albums that can be:

- **Imported** from external sources (CSV files)
- **Linked** to albums in your library automatically or manually
- **Browsed** to discover new music you might be missing
- **Converted** to playlists for easy listening

Charts help you answer questions like:
- "How many albums from the Rolling Stone 500 do I have?"
- "Which critically acclaimed albums am I missing?"
- "What should I listen to next from this year's best-of lists?"

## Chart Structure

Each chart consists of:

| Field | Description |
|-------|-------------|
| **Title** | The name of the chart (e.g., "Rolling Stone 500 Greatest Albums") |
| **Slug** | URL-friendly identifier (auto-generated from title) |
| **Source Name** | Where the chart came from (e.g., "Rolling Stone", "Pitchfork") |
| **Source URL** | Link to the original chart online |
| **Year** | The year the chart was published |
| **Description** | Optional notes about the chart |
| **Tags** | Categorization tags (e.g., "rock", "2024", "best-of") |
| **Visibility** | Whether the chart appears in public listings |

### Chart Items

Each item in a chart represents an album entry:

| Field | Description |
|-------|-------------|
| **Rank** | Position in the chart (1 = highest) |
| **Artist Name** | The artist as listed in the source |
| **Album Title** | The album name as listed in the source |
| **Release Year** | Optional year the album was released |
| **Link Status** | Whether the item is linked to your library |

## Link Status

Chart items have one of four link statuses:

| Status | Description |
|--------|-------------|
| **Unlinked** | No matching album found in your library |
| **Linked** | Successfully matched to an album in your library |
| **Ambiguous** | Multiple potential matches found—needs manual resolution |
| **Ignored** | Manually marked to skip (e.g., if you don't want the album) |

## Creating Charts

### From CSV Import

The easiest way to create a chart is by importing a CSV file:

1. Navigate to **Charts** in the Melodee UI
2. Click **Create New Chart**
3. Fill in chart metadata (title, source, year, etc.)
4. Upload or paste CSV content

#### CSV Format

The CSV should have columns in this order:

```csv
Rank,Artist,Album,Year (optional)
1,The Beatles,Abbey Road,1969
2,Pink Floyd,The Dark Side of the Moon,1973
3,Nirvana,Nevermind,1991
```

- **Rank**: Positive integer (1 = top of chart)
- **Artist**: Artist name as it appears on the chart
- **Album**: Album title as it appears on the chart
- **Year**: Optional release year

Quotes are supported for values containing commas:

```csv
1,"Crosby, Stills, Nash & Young",Déjà Vu,1970
```

### Manual Creation

You can also create charts manually through the UI by adding items one at a time.

## Automatic Linking

When you import a chart, Melodee automatically attempts to link each item to albums in your library:

1. **Exact Match**: Artist and album names match exactly (normalized)
2. **Fuzzy Match**: Partial matches when exact matching fails
3. **Artist Match Only**: Artist found but no matching album

The linking process assigns confidence scores:

| Confidence | Meaning |
|------------|---------|
| 1.0 | Exact match found |
| 0.7 - 0.8 | Fuzzy match or multiple possibilities |
| 0.5 | Artist found but album not matched |

## Manual Linking

For ambiguous or unlinked items, you can manually resolve them:

1. Click on an unlinked/ambiguous chart item
2. Search for the correct album in your library
3. Select the match to link them

You can also:
- **Ignore** items you don't want to track
- **Re-link** items if the automatic match was wrong

## Generated Playlists

Charts can automatically generate playlists from linked albums:

1. Enable **"Generated Playlist"** on the chart
2. All songs from linked albums are included
3. Songs are ordered by chart rank, then track number

This lets you listen through a "best of" list in ranked order.

## Use Cases

### Tracking Your Collection

Import charts to see how your library compares:

- **Rolling Stone 500**: See which classics you own
- **Grammy Winners**: Track award-winning albums
- **Decade Lists**: Compare your 80s, 90s, 2000s collections

### Music Discovery

Use unlinked items as a shopping/wishlist:

- Filter charts by "Unlinked" status
- See highly-ranked albums you're missing
- Prioritize acquisitions based on chart positions

### Personal Rankings

Create your own charts:

- "My Top 100 Albums of All Time"
- "Best Albums of 2024"
- "Desert Island Discs"

### Playlist Generation

Turn any chart into a listenable playlist:

1. Import a chart (e.g., "Pitchfork Best New Albums 2024")
2. Enable generated playlist
3. Listen through the chart in ranked order

## Managing Charts

### Editing Charts

- Update chart metadata (title, source, description)
- Re-run automatic linking after adding new albums
- Manually adjust link status for individual items

### Chart Images

Upload cover images for charts to make them visually identifiable in the UI.

### Filtering and Search

Filter charts by:
- **Year**: Find charts from a specific year
- **Source**: Show only charts from a particular publication
- **Tags**: Filter by genre or category tags

## Reports

Melodee provides reports to help manage charts:

### Missing Albums Report

Shows all unlinked chart items across all charts, helping you identify:
- Albums you might want to acquire
- Items that need manual linking
- Patterns in what's missing from your library

### Ranked Report

View chart items sorted by their ranking across all charts, highlighting the most acclaimed albums.

## API Access

Charts are accessible via the Melodee API:

```
# List all charts
GET /api/v1/Charts

# Get chart by ID
GET /api/v1/Charts/{id}

# Get chart by slug
GET /api/v1/Charts/slug/{slug}

# Get generated playlist tracks
GET /api/v1/Charts/{id}/playlist
```

## Best Practices

- **Use consistent source names**: Makes filtering easier (e.g., always "Rolling Stone" not "RS")
- **Include source URLs**: Helps verify original rankings
- **Tag charts appropriately**: Use tags like "rock", "2024", "best-of" for organization
- **Re-link periodically**: After adding albums, re-run linking to catch new matches
- **Review ambiguous items**: Manual resolution improves accuracy

## Examples of Charts to Import

| Chart | Source | Items |
|-------|--------|-------|
| 500 Greatest Albums | Rolling Stone | 500 |
| Album of the Year | Pitchfork | ~50/year |
| Mercury Prize Winners | Mercury Prize | ~30 |
| Grammy Album of the Year | Recording Academy | ~65 |
| 1001 Albums You Must Hear | Book | 1001 |
| NME Greatest Albums | NME | Various |

---

Have questions about charts? Open an issue on GitHub or check the [API documentation](/api/) for technical details.

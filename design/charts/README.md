## Chart JSON files

This directory contains ready-to-import **Melodee chart** definitions as JSON.
Each file represents a curated, ranked list of albums (with optional metadata like source, year, tags, and an image URL).

## How to import into Melodee

1. Sign in as an **Administrator**.
2. Open the Charts page: `/charts`.
3. Click **Import JSON**.
4. Either:
   - Click **Upload File** and select one of the `.json` files from this directory, or
   - Paste the JSON content into the text area.
5. Click **Validate**.
6. If validation succeeds, click **Import**.

After import, Melodee will attempt to auto-link chart items to albums in your library based on `artistName` + `albumTitle`.
Use the "Missing Report" / "Ranked Report" buttons on `/charts` to review what did (or did not) link.

## JSON schema (what Melodee expects)

Notes:
- The JSON must be strict JSON (no `//` comments).
- `rank` values must be unique within the chart.

```json
{
  "title": "Best Albums of 2024",
  "sourceName": "Example Source",
  "sourceUrl": "https://example.com",
  "year": 2024,
  "description": "Optional description (supports markdown).",
  "tags": ["rock", "2024"],
  "imageUrl": "https://example.com/chart-image.png",
  "isVisible": true,
  "isGeneratedPlaylistEnabled": true,
  "items": [
    {
      "rank": 1,
      "artistName": "Artist Name",
      "albumTitle": "Album Title",
      "releaseYear": 2024
    }
  ]
}
```

## Important behaviors / limits

- **File upload size limit**: the import dialog reads uploaded files with a 1 MB limit.
- **Import creates a new chart**: importing the same file multiple times will create multiple charts.
- **Image downloading**: if `imageUrl` is set, Melodee will download the image server-side and store it for the chart.
  Only use `imageUrl` values you trust.

## Included charts

| File | Title | Source | Year | Items |
| --- | --- | --- | --- | --- |
| `1001_albums_you_must_hear_before_you_die.json` | 1001 Albums You Must Hear Before You Die (2005 edition) | MusicBrainz | 2005 | 1000 |
| `chatgpt-prog_rock_metal_top100_2025.json` | Top 100 Progressive Rock/Metal Albums of 2025 | Sputnikmusic | 2025 | 100 |
| `chatgpt-prog_rock_metal_top100_all_time_chart.json` | Top 100 Progressive Rock/Metal Albums of All Time | Curated (multi-source) |  | 100 |
| `nmw-the_28_greatest_best_of_albums.json` | The 28 Greatest Best Of Albums | NME | 2020 | 28 |
| `the_guardian-100_best_albums_ever.json` | 100 Best Albums Ever | The Guardian | 1997 | 100 |
| `time-all_time_100_albums_time.json` | All-TIME 100 Albums | TIME | 2006 | 100 |
| `wikipedia-best_1970s_all_years.json` | Best Albums of the 1970s | Wikipedia |  | 246 |
| `wikipedia-best_1980s_all_years.json` | Best Albums of the 1980s (All Years) | Wikipedia |  | 182 |
| `wikipedia-best_1990s_all_years.json` | Best Albums of the 1990s | Wikipedia |  | 223 |
| `wikipedia-best_selling_albums_all_tables.json` | List of best-selling albums | Wikipedia |  | 104 |


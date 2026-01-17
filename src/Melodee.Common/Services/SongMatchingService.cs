using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.Parsing;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Melodee.Common.Services;

/// <summary>
/// Service for matching M3U playlist entries to songs in the library
/// </summary>
public sealed class SongMatchingService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    /// <summary>
    /// Attempt to match an M3U entry to a song in the library
    /// </summary>
    public async Task<SongMatchResult> MatchEntryAsync(
        M3UEntry entry,
        string? libraryPath = null,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        // Strategy 1: Exact path match under library root
        if (!string.IsNullOrEmpty(libraryPath))
        {
            var exactMatch = await TryExactPathMatchAsync(scopedContext, entry, libraryPath, cancellationToken).ConfigureAwait(false);
            if (exactMatch != null)
            {
                return new SongMatchResult
                {
                    Song = exactMatch,
                    MatchStrategy = MatchStrategy.ExactPath,
                    Confidence = 1.0m
                };
            }
        }

        // Strategy 2: Filename with directory hints
        if (!string.IsNullOrEmpty(entry.FileName))
        {
            var filenameMatch = await TryFilenameMatchAsync(scopedContext, entry, cancellationToken).ConfigureAwait(false);
            if (filenameMatch.HasValue)
            {
                return new SongMatchResult
                {
                    Song = filenameMatch.Value.Song,
                    MatchStrategy = MatchStrategy.FilenameWithHints,
                    Confidence = filenameMatch.Value.Confidence
                };
            }
        }

        // Strategy 3: Metadata match (title + artist + album)
        var metadataMatch = await TryMetadataMatchAsync(scopedContext, entry, cancellationToken).ConfigureAwait(false);
        if (metadataMatch.HasValue)
        {
            return new SongMatchResult
            {
                Song = metadataMatch.Value.Song,
                MatchStrategy = MatchStrategy.Metadata,
                Confidence = metadataMatch.Value.Confidence
            };
        }

        // No match found
        return new SongMatchResult
        {
            Song = null,
            MatchStrategy = MatchStrategy.None,
            Confidence = 0m
        };
    }

    private async Task<Song?> TryExactPathMatchAsync(
        MelodeeDbContext context,
        M3UEntry entry,
        string libraryPath,
        CancellationToken cancellationToken)
    {
        try
        {
            // Normalize library path
            var normalizedLibraryPath = libraryPath.Replace('\\', '/').TrimEnd('/');
            var normalizedReference = entry.NormalizedReference;

            // Remove drive letters and leading slashes for comparison
            if (normalizedReference.Length > 2 && normalizedReference[1] == ':')
            {
                // Windows absolute path like "D:/Music/..."
                normalizedReference = normalizedReference[2..].TrimStart('/');
            }
            else if (normalizedReference.StartsWith('/'))
            {
                // Unix absolute path like "/music/..."
                normalizedReference = normalizedReference.TrimStart('/');
            }

            // Try to find song with matching file path
            var candidatePaths = new[]
            {
                normalizedReference,
                Path.Combine(normalizedLibraryPath, normalizedReference).Replace('\\', '/'),
                normalizedReference.Split('/').Last() // Just filename
            };

            foreach (var candidatePath in candidatePaths)
            {
                var song = await context.Songs
                    .Include(s => s.Album)
                        .ThenInclude(a => a.Artist)
                    .FirstOrDefaultAsync(s => s.FileName.Replace('\\', '/').EndsWith(candidatePath), cancellationToken)
                    .ConfigureAwait(false);

                if (song != null)
                {
                    return song;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Error during exact path matching for entry: {Entry}", entry.NormalizedReference);
        }

        return null;
    }

    private async Task<(Song Song, decimal Confidence)?> TryFilenameMatchAsync(
        MelodeeDbContext context,
        M3UEntry entry,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(entry.FileName))
            {
                return null;
            }

            // Remove file extension from filename for matching
            var filenameWithoutExt = Path.GetFileNameWithoutExtension(entry.FileName);

            var query = context.Songs
                .Include(s => s.Album)
                    .ThenInclude(a => a.Artist)
                .AsQueryable();

            // Match by filename (song file path ends with the filename)
            query = query.Where(s => s.FileName.Contains(entry.FileName));

            // Apply album folder hint if available
            if (!string.IsNullOrEmpty(entry.AlbumFolder))
            {
                var albumHint = entry.AlbumFolder;
                query = query.Where(s => s.Album.Name.Contains(albumHint) || s.Album.NameNormalized.Contains(albumHint));
            }

            // Apply artist folder hint if available
            if (!string.IsNullOrEmpty(entry.ArtistFolder))
            {
                var artistHint = entry.ArtistFolder;
                query = query.Where(s => s.Album.Artist.Name.Contains(artistHint) || s.Album.Artist.NameNormalized.Contains(artistHint));
            }

            var candidates = await query.Take(10).ToListAsync(cancellationToken).ConfigureAwait(false);

            if (candidates.Count == 0)
            {
                return null;
            }

            // If multiple matches, score them
            if (candidates.Count == 1)
            {
                return (candidates[0], 0.8m);
            }

            // Score based on how well hints match
            var scored = candidates.Select(song => new
            {
                Song = song,
                Score = CalculateFileMatchScore(song, entry)
            }).OrderByDescending(x => x.Score).First();

            return scored.Score > 0 ? (scored.Song, scored.Score) : null;
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Error during filename matching for entry: {Entry}", entry.FileName);
        }

        return null;
    }

    private async Task<(Song Song, decimal Confidence)?> TryMetadataMatchAsync(
        MelodeeDbContext context,
        M3UEntry entry,
        CancellationToken cancellationToken)
    {
        try
        {
            // Extract potential song title from filename
            if (string.IsNullOrEmpty(entry.FileName))
            {
                return null;
            }

            var potentialTitle = Path.GetFileNameWithoutExtension(entry.FileName);
            
            // Remove common track number prefixes (01 - Title, 01. Title, etc.)
            potentialTitle = System.Text.RegularExpressions.Regex.Replace(potentialTitle, @"^\d+[\s\.\-_]+", "");

            var query = context.Songs
                .Include(s => s.Album)
                    .ThenInclude(a => a.Artist)
                .Where(s => s.TitleNormalized.Contains(potentialTitle) || s.Title.Contains(potentialTitle));

            // Apply album hint if available
            if (!string.IsNullOrEmpty(entry.AlbumFolder))
            {
                query = query.Where(s => s.Album.NameNormalized.Contains(entry.AlbumFolder));
            }

            // Apply artist hint if available
            if (!string.IsNullOrEmpty(entry.ArtistFolder))
            {
                query = query.Where(s => s.Album.Artist.NameNormalized.Contains(entry.ArtistFolder));
            }

            var candidates = await query.Take(5).ToListAsync(cancellationToken).ConfigureAwait(false);

            if (candidates.Count == 0)
            {
                return null;
            }

            if (candidates.Count == 1)
            {
                return (candidates[0], 0.6m);
            }

            // Score candidates based on metadata similarity
            var scored = candidates.Select(song => new
            {
                Song = song,
                Score = CalculateMetadataMatchScore(song, entry, potentialTitle)
            }).OrderByDescending(x => x.Score).First();

            return scored.Score > 0.3m ? (scored.Song, scored.Score) : null;
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Error during metadata matching for entry: {Entry}", entry.FileName);
        }

        return null;
    }

    private static decimal CalculateFileMatchScore(Song song, M3UEntry entry)
    {
        decimal score = 0.5m; // Base score for filename match

        // Bonus for album folder match
        if (!string.IsNullOrEmpty(entry.AlbumFolder) &&
            (song.Album.Name.Contains(entry.AlbumFolder, StringComparison.OrdinalIgnoreCase) ||
             song.Album.NameNormalized.Contains(entry.AlbumFolder, StringComparison.OrdinalIgnoreCase)))
        {
            score += 0.2m;
        }

        // Bonus for artist folder match
        if (!string.IsNullOrEmpty(entry.ArtistFolder) &&
            (song.Album.Artist.Name.Contains(entry.ArtistFolder, StringComparison.OrdinalIgnoreCase) ||
             song.Album.Artist.NameNormalized.Contains(entry.ArtistFolder, StringComparison.OrdinalIgnoreCase)))
        {
            score += 0.2m;
        }

        return Math.Min(score, 0.9m); // Cap at 0.9 (not perfect match like exact path)
    }

    private static decimal CalculateMetadataMatchScore(Song song, M3UEntry entry, string potentialTitle)
    {
        decimal score = 0.3m; // Base score for metadata match

        // Check title similarity
        if (song.TitleNormalized.Equals(potentialTitle, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.3m;
        }
        else if (song.TitleNormalized.Contains(potentialTitle, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.15m;
        }

        // Bonus for album match
        if (!string.IsNullOrEmpty(entry.AlbumFolder) &&
            song.Album.NameNormalized.Equals(entry.AlbumFolder, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.2m;
        }

        // Bonus for artist match
        if (!string.IsNullOrEmpty(entry.ArtistFolder) &&
            song.Album.Artist.NameNormalized.Equals(entry.ArtistFolder, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.2m;
        }

        return Math.Min(score, 0.7m); // Cap metadata matches lower than file matches
    }
}

public sealed class SongMatchResult
{
    public Song? Song { get; init; }
    public MatchStrategy MatchStrategy { get; init; }
    public decimal Confidence { get; init; }
}

public enum MatchStrategy
{
    None = 0,
    ExactPath = 1,
    FilenameWithHints = 2,
    Metadata = 3
}

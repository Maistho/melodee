using System.Text;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Melodee.Common.Extensions;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Materialized;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Staging;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using Directory = System.IO.Directory;

namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;

/// <summary>
/// Streaming import service for MusicBrainz data.
/// Uses SQLite staging tables to avoid loading entire datasets into memory.
/// Memory usage stays constant regardless of dataset size.
/// 
/// Performance optimizations applied:
/// - Aggressive SQLite PRAGMA settings for bulk import (synchronous=OFF, journal_mode=OFF)
/// - Deferred index creation (indexes created after bulk insert)
/// - Raw SQL bulk inserts instead of EF Core AddRange
/// - Large transaction batching (entire file in single transaction)
/// - Optimized Lucene IndexWriter configuration
/// </summary>
public sealed class StreamingMusicBrainzImporter(ILogger logger)
{
    private const int BatchSize = 10000;  // Increased for better throughput with raw SQL
    private const int MaxIndexSize = 255;
    private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

    /// <summary>
    /// Import MusicBrainz data using streaming approach.
    /// Never loads more than one batch of records into memory at a time.
    /// </summary>
    public async Task ImportAsync(
        MusicBrainzDbContext context,
        string storagePath,
        string luceneIndexPath,
        ImportProgressCallback? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        var mbDumpPath = Path.Combine(storagePath, "staging/mbdump");

        // Configure SQLite for maximum bulk import performance
        // WARNING: These settings sacrifice durability for speed - only safe during initial import
        await ConfigureSqliteForBulkImportAsync(context, cancellationToken);

        // Drop staging table indexes before bulk insert for faster writes
        await DropStagingIndexesAsync(context, cancellationToken);

        // Phase 1: Stream artist data to staging tables
        await ImportArtistStagingDataAsync(context, mbDumpPath, progressCallback, cancellationToken);
        ForceGarbageCollection();

        // Recreate staging indexes needed for materialization joins
        await CreateArtistStagingIndexesAsync(context, cancellationToken);

        // Phase 2: Materialize artists using SQL
        await MaterializeArtistsAsync(context, progressCallback, cancellationToken);
        ForceGarbageCollection();

        // Phase 3: Create Lucene index from materialized artists (streaming)
        await CreateLuceneIndexAsync(context, luceneIndexPath, progressCallback, cancellationToken);
        ForceGarbageCollection();

        // Phase 4: Materialize artist relations using SQL
        await MaterializeArtistRelationsAsync(context, progressCallback, cancellationToken);

        // Phase 5: Drop artist staging tables
        await DropArtistStagingTablesAsync(context, progressCallback, cancellationToken);
        ForceGarbageCollection();

        // Phase 6: Stream album support data to staging tables
        await ImportAlbumStagingDataAsync(context, mbDumpPath, progressCallback, cancellationToken);
        ForceGarbageCollection();

        // Recreate staging indexes needed for album materialization
        await CreateAlbumStagingIndexesAsync(context, cancellationToken);

        // Phase 7: Materialize albums using SQL
        await MaterializeAlbumsAsync(context, progressCallback, cancellationToken);

        // Phase 8: Drop album staging tables
        await DropAlbumStagingTablesAsync(context, progressCallback, cancellationToken);

        // Restore safe SQLite settings
        await RestoreSqliteSettingsAsync(context, cancellationToken);

        // Force final GC
        ForceGarbageCollection();
    }

    /// <summary>
    /// Configure SQLite for maximum bulk import throughput.
    /// These settings sacrifice durability for speed and should only be used during initial import.
    /// </summary>
    private static async Task ConfigureSqliteForBulkImportAsync(MusicBrainzDbContext context, CancellationToken cancellationToken)
    {
        // Disable synchronous writes - major performance boost but unsafe if power loss
        await context.Database.ExecuteSqlRawAsync("PRAGMA synchronous = OFF", cancellationToken);
        
        // Disable journaling entirely during bulk import
        await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = OFF", cancellationToken);
        
        // Increase cache size to 64MB for better buffering
        await context.Database.ExecuteSqlRawAsync("PRAGMA cache_size = -65536", cancellationToken);
        
        // Use memory for temp storage (faster than disk)
        await context.Database.ExecuteSqlRawAsync("PRAGMA temp_store = MEMORY", cancellationToken);
        
        // Exclusive locking - no other connections allowed, but faster
        await context.Database.ExecuteSqlRawAsync("PRAGMA locking_mode = EXCLUSIVE", cancellationToken);
        
        // Optimal page size for modern SSDs
        await context.Database.ExecuteSqlRawAsync("PRAGMA page_size = 4096", cancellationToken);
        
        // Enable memory-mapped I/O for 256MB
        await context.Database.ExecuteSqlRawAsync("PRAGMA mmap_size = 268435456", cancellationToken);
    }

    /// <summary>
    /// Restore safe SQLite settings after bulk import.
    /// </summary>
    private static async Task RestoreSqliteSettingsAsync(MusicBrainzDbContext context, CancellationToken cancellationToken)
    {
        await context.Database.ExecuteSqlRawAsync("PRAGMA synchronous = NORMAL", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA locking_mode = NORMAL", cancellationToken);
    }

    /// <summary>
    /// Drop all staging table indexes for faster bulk insert.
    /// </summary>
    private static async Task DropStagingIndexesAsync(MusicBrainzDbContext context, CancellationToken cancellationToken)
    {
        var indexes = new[]
        {
            "IX_ArtistStaging_ArtistId",
            "IX_ArtistAliasStaging_ArtistId",
            "IX_LinkStaging_LinkId",
            "IX_LinkArtistToArtistStaging_Artist0",
            "IX_LinkArtistToArtistStaging_Artist1",
            "IX_LinkArtistToArtistStaging_LinkId",
            "IX_ArtistCreditStaging_ArtistCreditId",
            "IX_ArtistCreditNameStaging_ArtistCreditId",
            "IX_ReleaseCountryStaging_ReleaseId",
            "IX_ReleaseGroupStaging_ReleaseGroupId",
            "IX_ReleaseGroupMetaStaging_ReleaseGroupId",
            "IX_ReleaseStaging_ReleaseId",
            "IX_ReleaseStaging_ReleaseGroupId",
            "IX_ReleaseStaging_ArtistCreditId"
        };

        foreach (var index in indexes)
        {
#pragma warning disable EF1002 // Index names are hardcoded constants, not user input
            await context.Database.ExecuteSqlRawAsync($"DROP INDEX IF EXISTS {index}", cancellationToken);
#pragma warning restore EF1002
        }
    }

    /// <summary>
    /// Create indexes on artist staging tables needed for materialization.
    /// </summary>
    private static async Task CreateArtistStagingIndexesAsync(MusicBrainzDbContext context, CancellationToken cancellationToken)
    {
        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_ArtistStaging_ArtistId ON ArtistStaging(ArtistId)", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_ArtistAliasStaging_ArtistId ON ArtistAliasStaging(ArtistId)", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_LinkStaging_LinkId ON LinkStaging(LinkId)", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_LinkArtistToArtistStaging_Artist0 ON LinkArtistToArtistStaging(Artist0)", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_LinkArtistToArtistStaging_Artist1 ON LinkArtistToArtistStaging(Artist1)", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_LinkArtistToArtistStaging_LinkId ON LinkArtistToArtistStaging(LinkId)", cancellationToken);
    }

    /// <summary>
    /// Create indexes on album staging tables needed for materialization.
    /// </summary>
    private static async Task CreateAlbumStagingIndexesAsync(MusicBrainzDbContext context, CancellationToken cancellationToken)
    {
        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_ArtistCreditStaging_ArtistCreditId ON ArtistCreditStaging(ArtistCreditId)", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_ArtistCreditNameStaging_ArtistCreditId ON ArtistCreditNameStaging(ArtistCreditId)", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_ReleaseCountryStaging_ReleaseId ON ReleaseCountryStaging(ReleaseId)", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_ReleaseGroupStaging_ReleaseGroupId ON ReleaseGroupStaging(ReleaseGroupId)", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_ReleaseGroupMetaStaging_ReleaseGroupId ON ReleaseGroupMetaStaging(ReleaseGroupId)", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_ReleaseStaging_ReleaseId ON ReleaseStaging(ReleaseId)", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_ReleaseStaging_ReleaseGroupId ON ReleaseStaging(ReleaseGroupId)", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_ReleaseStaging_ArtistCreditId ON ReleaseStaging(ArtistCreditId)", cancellationToken);
    }

    private static void ForceGarbageCollection()
    {
        GC.Collect(2, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, true);
    }

    #region Phase 1: Artist Staging Data

    private async Task ImportArtistStagingDataAsync(
        MusicBrainzDbContext context,
        string mbDumpPath,
        ImportProgressCallback? progressCallback,
        CancellationToken cancellationToken)
    {
        using (Operation.At(LogEventLevel.Debug).Time("StreamingImporter: Artist staging data"))
        {
            // Stream artist file to staging using optimized raw SQL
            progressCallback?.Invoke("Loading Artists", 0, 4, "Streaming artist file to staging...");
            var artistCount = await StreamArtistsToStagingAsync(
                context,
                Path.Combine(mbDumpPath, "artist"),
                cancellationToken);
            progressCallback?.Invoke("Loading Artists", 1, 4, $"Streamed {artistCount:N0} artists to staging");

            // Stream artist aliases to staging using optimized raw SQL
            progressCallback?.Invoke("Loading Artists", 1, 4, "Streaming artist aliases to staging...");
            var aliasCount = await StreamArtistAliasesToStagingAsync(
                context,
                Path.Combine(mbDumpPath, "artist_alias"),
                cancellationToken);
            progressCallback?.Invoke("Loading Artists", 2, 4, $"Streamed {aliasCount:N0} artist aliases to staging");

            // Stream links to staging (smaller file, use generic method)
            progressCallback?.Invoke("Loading Artists", 2, 4, "Streaming links to staging...");
            var linkCount = await StreamFileToStagingAsync(
                context,
                Path.Combine(mbDumpPath, "link"),
                parts => new LinkStaging
                {
                    LinkId = SafeParser.ToNumber<long>(parts[0]),
                    BeginDate = ParseMusicBrainzDate(parts[2], parts[3], parts[4]),
                    EndDate = ParseMusicBrainzDate(parts[5], parts[6], parts[7])
                },
                context.LinksStaging,
                cancellationToken);
            progressCallback?.Invoke("Loading Artists", 3, 4, $"Streamed {linkCount:N0} links to staging");

            // Stream artist-to-artist links to staging (smaller file, use generic method)
            progressCallback?.Invoke("Loading Artists", 3, 4, "Streaming artist links to staging...");
            var artistLinkCount = await StreamFileToStagingAsync(
                context,
                Path.Combine(mbDumpPath, "l_artist_artist"),
                parts => new LinkArtistToArtistStaging
                {
                    LinkId = SafeParser.ToNumber<long>(parts[1]),
                    Artist0 = SafeParser.ToNumber<long>(parts[2]),
                    Artist1 = SafeParser.ToNumber<long>(parts[3]),
                    LinkOrder = SafeParser.ToNumber<int>(parts[6])
                },
                context.LinkArtistToArtistsStaging,
                cancellationToken);
            progressCallback?.Invoke("Loading Artists", 4, 4, $"Streamed {artistLinkCount:N0} artist links to staging");
        }
    }

    #endregion

    #region Phase 2: Materialize Artists

    private async Task MaterializeArtistsAsync(
        MusicBrainzDbContext context,
        ImportProgressCallback? progressCallback,
        CancellationToken cancellationToken)
    {
        using (Operation.At(LogEventLevel.Debug).Time("StreamingImporter: Materialize artists"))
        {
            progressCallback?.Invoke("Materializing Artists", 0, 1, "Creating materialized artists from staging...");

            // Use SQL to materialize artists with concatenated aliases
            // SQLite GROUP_CONCAT with DISTINCT doesn't support custom separator, so we use a subquery
            var sql = @"
                INSERT INTO Artist (MusicBrainzArtistId, MusicBrainzIdRaw, Name, NameNormalized, SortName, AlternateNames)
                SELECT 
                    a.ArtistId,
                    a.MusicBrainzIdRaw,
                    a.Name,
                    a.NameNormalized,
                    a.SortName,
                    (SELECT GROUP_CONCAT(NameNormalized, '|') 
                     FROM (SELECT DISTINCT aa.NameNormalized FROM ArtistAliasStaging aa WHERE aa.ArtistId = a.ArtistId))
                FROM ArtistStaging a";

            var rowsAffected = await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            logger.Debug("StreamingImporter: Materialized {Count} artists", rowsAffected);

            progressCallback?.Invoke("Materializing Artists", 1, 1, $"Materialized {rowsAffected:N0} artists");
        }
    }

    #endregion

    #region Phase 3: Create Lucene Index

    private async Task CreateLuceneIndexAsync(
        MusicBrainzDbContext context,
        string luceneIndexPath,
        ImportProgressCallback? progressCallback,
        CancellationToken cancellationToken)
    {
        using (Operation.At(LogEventLevel.Debug).Time("StreamingImporter: Create Lucene index"))
        {
            if (Directory.Exists(luceneIndexPath))
            {
                Directory.Delete(luceneIndexPath, true);
            }

            var totalArtists = await context.Artists.CountAsync(cancellationToken);
            progressCallback?.Invoke("Creating Index", 0, totalArtists, "Building Lucene search index...");

            using var dir = FSDirectory.Open(luceneIndexPath);
            var analyzer = new StandardAnalyzer(AppLuceneVersion);
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer)
            {
                RAMBufferSizeMB = 256,  // Larger buffer for faster indexing
                MaxBufferedDocs = 50000,  // More docs before auto-flush
                UseCompoundFile = false  // Faster writes, more files
            };
            
            // Use concurrent merge scheduler for background segment merging
            indexConfig.MergeScheduler = new ConcurrentMergeScheduler();
            
            using var writer = new IndexWriter(dir, indexConfig);

            var indexed = 0;
            var skip = 0;
            const int luceneBatchSize = 10000;  // Larger batches with bigger RAM buffer

            // Stream artists from database in batches
            while (true)
            {
                var batch = await context.Artists
                    .AsNoTracking()
                    .OrderBy(a => a.Id)
                    .Skip(skip)
                    .Take(luceneBatchSize)
                    .ToListAsync(cancellationToken);

                if (batch.Count == 0)
                    break;

                foreach (var artist in batch)
                {
                    var doc = new Document
                    {
                        new StringField(nameof(Artist.MusicBrainzIdRaw), artist.MusicBrainzIdRaw, Field.Store.YES),
                        new StringField(nameof(Artist.NameNormalized), artist.NameNormalized, Field.Store.YES),
                        new TextField(nameof(Artist.AlternateNames), artist.AlternateNames ?? string.Empty, Field.Store.YES)
                    };

                    writer.AddDocument(doc);
                    indexed++;
                }

                // Clear batch from memory
                batch.Clear();
                skip += luceneBatchSize;

                progressCallback?.Invoke("Creating Index", indexed, totalArtists,
                    $"Indexed {indexed:N0} / {totalArtists:N0} artists");
            }

            writer.Commit();
            logger.Debug("StreamingImporter: Created Lucene index with {Count} artists", indexed);
            progressCallback?.Invoke("Creating Index", totalArtists, totalArtists,
                $"Completed Lucene index with {indexed:N0} artists");
        }
    }

    #endregion

    #region Phase 4: Materialize Artist Relations

    private async Task MaterializeArtistRelationsAsync(
        MusicBrainzDbContext context,
        ImportProgressCallback? progressCallback,
        CancellationToken cancellationToken)
    {
        using (Operation.At(LogEventLevel.Debug).Time("StreamingImporter: Materialize artist relations"))
        {
            progressCallback?.Invoke("Materializing Relations", 0, 1, "Creating artist relations from staging...");

            // Use SQL to materialize artist relations
            // Join staging tables to get the data we need
            var sql = @"
                INSERT INTO ArtistRelation (ArtistId, RelatedArtistId, ArtistRelationType, SortOrder, RelationStart, RelationEnd)
                SELECT 
                    a1.Id,
                    a2.Id,
                    0,  -- Associated relation type
                    laa.LinkOrder,
                    l.BeginDate,
                    l.EndDate
                FROM LinkArtistToArtistStaging laa
                INNER JOIN Artist a1 ON a1.MusicBrainzArtistId = laa.Artist0
                INNER JOIN Artist a2 ON a2.MusicBrainzArtistId = laa.Artist1
                LEFT JOIN LinkStaging l ON l.LinkId = laa.LinkId";

            var rowsAffected = await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            logger.Debug("StreamingImporter: Materialized {Count} artist relations", rowsAffected);

            progressCallback?.Invoke("Materializing Relations", 1, 1, $"Materialized {rowsAffected:N0} artist relations");
        }
    }

    #endregion

    #region Phase 5: Drop Artist Staging Tables

    private async Task DropArtistStagingTablesAsync(
        MusicBrainzDbContext context,
        ImportProgressCallback? progressCallback,
        CancellationToken cancellationToken)
    {
        progressCallback?.Invoke("Cleanup", 0, 1, "Dropping artist staging tables...");

        await context.Database.ExecuteSqlRawAsync("DELETE FROM ArtistStaging", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("DELETE FROM ArtistAliasStaging", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("DELETE FROM LinkStaging", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("DELETE FROM LinkArtistToArtistStaging", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("VACUUM", cancellationToken);

        GC.Collect();
        GC.WaitForPendingFinalizers();

        progressCallback?.Invoke("Cleanup", 1, 1, "Artist staging tables cleared");
    }

    #endregion

    #region Phase 6: Album Staging Data

    private async Task ImportAlbumStagingDataAsync(
        MusicBrainzDbContext context,
        string mbDumpPath,
        ImportProgressCallback? progressCallback,
        CancellationToken cancellationToken)
    {
        using (Operation.At(LogEventLevel.Debug).Time("StreamingImporter: Album staging data"))
        {
            // Stream artist_credit to staging
            progressCallback?.Invoke("Loading Albums", 0, 6, "Streaming artist credits to staging...");
            var creditCount = await StreamFileToStagingAsync(
                context,
                Path.Combine(mbDumpPath, "artist_credit"),
                parts => new ArtistCreditStaging
                {
                    ArtistCreditId = SafeParser.ToNumber<long>(parts[0]),
                    ArtistCount = SafeParser.ToNumber<int>(parts[2])
                },
                context.ArtistCreditsStaging,
                cancellationToken);
            progressCallback?.Invoke("Loading Albums", 1, 6, $"Streamed {creditCount:N0} artist credits");

            // Stream artist_credit_name to staging
            progressCallback?.Invoke("Loading Albums", 1, 6, "Streaming artist credit names to staging...");
            var creditNameCount = await StreamFileToStagingAsync(
                context,
                Path.Combine(mbDumpPath, "artist_credit_name"),
                parts => new ArtistCreditNameStaging
                {
                    ArtistCreditId = SafeParser.ToNumber<long>(parts[0]),
                    Position = SafeParser.ToNumber<int>(parts[1]),
                    ArtistId = SafeParser.ToNumber<long>(parts[2])
                },
                context.ArtistCreditNamesStaging,
                cancellationToken);
            progressCallback?.Invoke("Loading Albums", 2, 6, $"Streamed {creditNameCount:N0} artist credit names");

            // Stream release_country to staging
            progressCallback?.Invoke("Loading Albums", 2, 6, "Streaming release countries to staging...");
            var countryCount = await StreamFileToStagingAsync(
                context,
                Path.Combine(mbDumpPath, "release_country"),
                parts => new ReleaseCountryStaging
                {
                    ReleaseId = SafeParser.ToNumber<long>(parts[0]),
                    DateYear = SafeParser.ToNumber<int>(parts[2]),
                    DateMonth = SafeParser.ToNumber<int>(parts[3]),
                    DateDay = SafeParser.ToNumber<int>(parts[4])
                },
                context.ReleaseCountriesStaging,
                cancellationToken);
            progressCallback?.Invoke("Loading Albums", 3, 6, $"Streamed {countryCount:N0} release countries");

            // Stream release_group to staging
            progressCallback?.Invoke("Loading Albums", 3, 6, "Streaming release groups to staging...");
            var groupCount = await StreamFileToStagingAsync(
                context,
                Path.Combine(mbDumpPath, "release_group"),
                parts => new ReleaseGroupStaging
                {
                    ReleaseGroupId = SafeParser.ToNumber<long>(parts[0]),
                    MusicBrainzIdRaw = parts[1],
                    ArtistCreditId = SafeParser.ToNumber<long>(parts[3]),
                    ReleaseType = SafeParser.ToNumber<int>(parts[4])
                },
                context.ReleaseGroupsStaging,
                cancellationToken);
            progressCallback?.Invoke("Loading Albums", 4, 6, $"Streamed {groupCount:N0} release groups");

            // Stream release_group_meta to staging
            progressCallback?.Invoke("Loading Albums", 4, 6, "Streaming release group meta to staging...");
            var metaCount = await StreamFileToStagingAsync(
                context,
                Path.Combine(mbDumpPath, "release_group_meta"),
                parts => new ReleaseGroupMetaStaging
                {
                    ReleaseGroupId = SafeParser.ToNumber<long>(parts[0]),
                    DateYear = SafeParser.ToNumber<int>(parts[2]),
                    DateMonth = SafeParser.ToNumber<int>(parts[3]),
                    DateDay = SafeParser.ToNumber<int>(parts[4])
                },
                context.ReleaseGroupMetasStaging,
                cancellationToken);
            progressCallback?.Invoke("Loading Albums", 5, 6, $"Streamed {metaCount:N0} release group meta");

            // Stream release to staging using optimized raw SQL (largest file)
            progressCallback?.Invoke("Loading Albums", 5, 6, "Streaming releases to staging...");
            var releaseCount = await StreamReleasesToStagingAsync(
                context,
                Path.Combine(mbDumpPath, "release"),
                cancellationToken);
            progressCallback?.Invoke("Loading Albums", 6, 6, $"Streamed {releaseCount:N0} releases");
        }
    }

    #endregion

    #region Phase 7: Materialize Albums

    private async Task MaterializeAlbumsAsync(
        MusicBrainzDbContext context,
        ImportProgressCallback? progressCallback,
        CancellationToken cancellationToken)
    {
        using (Operation.At(LogEventLevel.Debug).Time("StreamingImporter: Materialize albums"))
        {
            progressCallback?.Invoke("Materializing Albums", 0, 1, "Creating materialized albums from staging...");

            // Complex SQL query that joins all staging tables to create materialized albums
            // This is the key to memory efficiency - SQLite handles all the joins
            var sql = @"
                INSERT INTO Album (MusicBrainzArtistId, MusicBrainzIdRaw, Name, NameNormalized, SortName, 
                                   ReleaseGroupMusicBrainzIdRaw, ReleaseType, ReleaseDate, ContributorIds)
                SELECT 
                    COALESCE(acn_artist.MusicBrainzArtistId, credit_artist.MusicBrainzArtistId),
                    r.MusicBrainzIdRaw,
                    r.Name,
                    r.NameNormalized,
                    r.SortName,
                    rg.MusicBrainzIdRaw,
                    rg.ReleaseType,
                    COALESCE(
                        -- Try release_country first
                        CASE WHEN rc.DateYear > 0 AND rc.DateMonth > 0 AND rc.DateDay > 0 
                             THEN printf('%04d-%02d-%02dT00:00:00', 
                                        CASE WHEN rc.DateYear BETWEEN 1 AND 9999 THEN rc.DateYear ELSE 1 END,
                                        CASE WHEN rc.DateMonth BETWEEN 1 AND 12 THEN rc.DateMonth ELSE 1 END,
                                        CASE WHEN rc.DateDay BETWEEN 1 AND 31 THEN rc.DateDay ELSE 1 END)
                             ELSE NULL END,
                        -- Fall back to release_group_meta
                        CASE WHEN rgm.DateYear > 0 AND rgm.DateMonth > 0 AND rgm.DateDay > 0 
                             THEN printf('%04d-%02d-%02dT00:00:00', 
                                        CASE WHEN rgm.DateYear BETWEEN 1 AND 9999 THEN rgm.DateYear ELSE 1 END,
                                        CASE WHEN rgm.DateMonth BETWEEN 1 AND 12 THEN rgm.DateMonth ELSE 1 END,
                                        CASE WHEN rgm.DateDay BETWEEN 1 AND 31 THEN rgm.DateDay ELSE 1 END)
                             ELSE NULL END
                    ),
                    NULL  -- ContributorIds - can be populated separately if needed
                FROM ReleaseStaging r
                INNER JOIN ReleaseGroupStaging rg ON rg.ReleaseGroupId = r.ReleaseGroupId
                LEFT JOIN ReleaseCountryStaging rc ON rc.ReleaseId = r.ReleaseId
                LEFT JOIN ReleaseGroupMetaStaging rgm ON rgm.ReleaseGroupId = r.ReleaseGroupId
                LEFT JOIN ArtistCreditStaging ac ON ac.ArtistCreditId = r.ArtistCreditId
                LEFT JOIN ArtistCreditNameStaging acn ON acn.ArtistCreditId = ac.ArtistCreditId AND acn.Position = 0
                LEFT JOIN Artist acn_artist ON acn_artist.MusicBrainzArtistId = acn.ArtistId
                LEFT JOIN Artist credit_artist ON credit_artist.MusicBrainzArtistId = r.ArtistCreditId
                WHERE r.Name IS NOT NULL 
                  AND r.Name != ''
                  AND rg.MusicBrainzIdRaw IS NOT NULL
                  AND (acn_artist.MusicBrainzArtistId IS NOT NULL OR credit_artist.MusicBrainzArtistId IS NOT NULL)
                  AND (
                      (rc.DateYear > 0 AND rc.DateMonth > 0 AND rc.DateDay > 0) OR
                      (rgm.DateYear > 0 AND rgm.DateMonth > 0 AND rgm.DateDay > 0)
                  )";

            var rowsAffected = await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            logger.Debug("StreamingImporter: Materialized {Count} albums", rowsAffected);

            progressCallback?.Invoke("Materializing Albums", 1, 1, $"Materialized {rowsAffected:N0} albums");
        }
    }

    #endregion

    #region Phase 8: Drop Album Staging Tables

    private async Task DropAlbumStagingTablesAsync(
        MusicBrainzDbContext context,
        ImportProgressCallback? progressCallback,
        CancellationToken cancellationToken)
    {
        progressCallback?.Invoke("Cleanup", 0, 1, "Dropping album staging tables...");

        await context.Database.ExecuteSqlRawAsync("DELETE FROM ArtistCreditStaging", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("DELETE FROM ArtistCreditNameStaging", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("DELETE FROM ReleaseCountryStaging", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("DELETE FROM ReleaseGroupStaging", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("DELETE FROM ReleaseGroupMetaStaging", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("DELETE FROM ReleaseStaging", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("VACUUM", cancellationToken);

        GC.Collect();
        GC.WaitForPendingFinalizers();

        progressCallback?.Invoke("Cleanup", 1, 1, "Album staging tables cleared");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Streams a file line-by-line to a SQLite staging table using raw SQL bulk inserts.
    /// Uses multi-value INSERT statements for maximum throughput.
    /// </summary>
    private async Task<int> StreamFileToStagingAsync<T>(
        MusicBrainzDbContext context,
        string filePath,
        Func<string[], T> constructor,
        DbSet<T> dbSet,
        CancellationToken cancellationToken) where T : class
    {
        if (!File.Exists(filePath))
        {
            logger.Warning("StreamingImporter: File not found: {FilePath}", filePath);
            return 0;
        }

        // Determine table name and column info from entity type
        var entityType = context.Model.FindEntityType(typeof(T));
        if (entityType == null)
        {
            throw new InvalidOperationException($"Entity type {typeof(T).Name} not found in model");
        }

        var tableName = entityType.GetTableName();
        var properties = entityType.GetProperties()
            .Where(p => !p.IsPrimaryKey() || !p.ValueGenerated.HasFlag(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd))
            .ToList();

        // Use the simpler EF Core approach but with a single transaction for entire file
        var totalCount = 0;
        var batch = new List<T>(BatchSize);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await foreach (var line in File.ReadLinesAsync(filePath, cancellationToken))
            {
                var parts = line.Split('\t');
                try
                {
                    var entity = constructor(parts);
                    batch.Add(entity);
                    totalCount++;

                    if (batch.Count >= BatchSize)
                    {
                        await dbSet.AddRangeAsync(batch, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        context.ChangeTracker.Clear();
                        batch.Clear();
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("StreamingImporter: Skipped malformed line in {File}: {Error}",
                        Path.GetFileName(filePath), ex.Message);
                }
            }

            // Save remaining batch
            if (batch.Count > 0)
            {
                await dbSet.AddRangeAsync(batch, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                context.ChangeTracker.Clear();
                batch.Clear();
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return totalCount;
    }

    /// <summary>
    /// Streams artists from file using optimized raw SQL bulk inserts.
    /// </summary>
    private async Task<int> StreamArtistsToStagingAsync(
        MusicBrainzDbContext context,
        string filePath,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            logger.Warning("StreamingImporter: File not found: {FilePath}", filePath);
            return 0;
        }

        var totalCount = 0;
        var valuesList = new List<string>(BatchSize);
        var sb = new StringBuilder();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await foreach (var line in File.ReadLinesAsync(filePath, cancellationToken))
            {
                var parts = line.Split('\t');
                try
                {
                    var artistId = SafeParser.ToNumber<long>(parts[0]);
                    var mbId = (SafeParser.ToGuid(parts[1]) ?? Guid.Empty).ToString();
                    var name = EscapeSqlString(parts[2].CleanString().TruncateLongString(MaxIndexSize) ?? string.Empty);
                    var nameNormalized = EscapeSqlString(parts[2].CleanString().TruncateLongString(MaxIndexSize)?.ToNormalizedString() ?? parts[2]);
                    var sortName = EscapeSqlString(parts[3].CleanString(true).TruncateLongString(MaxIndexSize) ?? parts[2]);

                    valuesList.Add($"({artistId},'{mbId}','{name}','{nameNormalized}','{sortName}')");
                    totalCount++;

                    if (valuesList.Count >= BatchSize)
                    {
                        sb.Clear();
                        sb.Append("INSERT INTO ArtistStaging (ArtistId, MusicBrainzIdRaw, Name, NameNormalized, SortName) VALUES ");
                        sb.Append(string.Join(",", valuesList));
                        await context.Database.ExecuteSqlRawAsync(sb.ToString(), cancellationToken);
                        valuesList.Clear();
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("StreamingImporter: Skipped malformed artist line: {Error}", ex.Message);
                }
            }

            // Save remaining batch
            if (valuesList.Count > 0)
            {
                sb.Clear();
                sb.Append("INSERT INTO ArtistStaging (ArtistId, MusicBrainzIdRaw, Name, NameNormalized, SortName) VALUES ");
                sb.Append(string.Join(",", valuesList));
                await context.Database.ExecuteSqlRawAsync(sb.ToString(), cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return totalCount;
    }

    /// <summary>
    /// Streams artist aliases from file using optimized raw SQL bulk inserts.
    /// </summary>
    private async Task<int> StreamArtistAliasesToStagingAsync(
        MusicBrainzDbContext context,
        string filePath,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            logger.Warning("StreamingImporter: File not found: {FilePath}", filePath);
            return 0;
        }

        var totalCount = 0;
        var valuesList = new List<string>(BatchSize);
        var sb = new StringBuilder();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await foreach (var line in File.ReadLinesAsync(filePath, cancellationToken))
            {
                var parts = line.Split('\t');
                try
                {
                    var artistId = SafeParser.ToNumber<long>(parts[1]);
                    var nameNormalized = EscapeSqlString(parts[2].CleanString().TruncateLongString(MaxIndexSize)?.ToNormalizedString() ?? parts[2]);

                    valuesList.Add($"({artistId},'{nameNormalized}')");
                    totalCount++;

                    if (valuesList.Count >= BatchSize)
                    {
                        sb.Clear();
                        sb.Append("INSERT INTO ArtistAliasStaging (ArtistId, NameNormalized) VALUES ");
                        sb.Append(string.Join(",", valuesList));
                        await context.Database.ExecuteSqlRawAsync(sb.ToString(), cancellationToken);
                        valuesList.Clear();
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("StreamingImporter: Skipped malformed alias line: {Error}", ex.Message);
                }
            }

            if (valuesList.Count > 0)
            {
                sb.Clear();
                sb.Append("INSERT INTO ArtistAliasStaging (ArtistId, NameNormalized) VALUES ");
                sb.Append(string.Join(",", valuesList));
                await context.Database.ExecuteSqlRawAsync(sb.ToString(), cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return totalCount;
    }

    /// <summary>
    /// Streams releases from file using optimized raw SQL bulk inserts.
    /// </summary>
    private async Task<int> StreamReleasesToStagingAsync(
        MusicBrainzDbContext context,
        string filePath,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            logger.Warning("StreamingImporter: File not found: {FilePath}", filePath);
            return 0;
        }

        var totalCount = 0;
        var valuesList = new List<string>(BatchSize);
        var sb = new StringBuilder();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await foreach (var line in File.ReadLinesAsync(filePath, cancellationToken))
            {
                var parts = line.Split('\t');
                try
                {
                    var releaseId = SafeParser.ToNumber<long>(parts[0]);
                    var mbId = EscapeSqlString(parts[1]);
                    var name = EscapeSqlString(parts[2].CleanString().TruncateLongString(MaxIndexSize) ?? string.Empty);
                    var nameNormalized = EscapeSqlString(parts[2].CleanString().TruncateLongString(MaxIndexSize)?.ToNormalizedString() ?? parts[2]);
                    var sortName = EscapeSqlString(parts[2].CleanString(true).TruncateLongString(MaxIndexSize) ?? parts[2]);
                    var artistCreditId = SafeParser.ToNumber<long>(parts[3]);
                    var releaseGroupId = SafeParser.ToNumber<long>(parts[4]);

                    valuesList.Add($"({releaseId},'{mbId}','{name}','{nameNormalized}','{sortName}',{releaseGroupId},{artistCreditId})");
                    totalCount++;

                    if (valuesList.Count >= BatchSize)
                    {
                        sb.Clear();
                        sb.Append("INSERT INTO ReleaseStaging (ReleaseId, MusicBrainzIdRaw, Name, NameNormalized, SortName, ReleaseGroupId, ArtistCreditId) VALUES ");
                        sb.Append(string.Join(",", valuesList));
                        await context.Database.ExecuteSqlRawAsync(sb.ToString(), cancellationToken);
                        valuesList.Clear();
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("StreamingImporter: Skipped malformed release line: {Error}", ex.Message);
                }
            }

            if (valuesList.Count > 0)
            {
                sb.Clear();
                sb.Append("INSERT INTO ReleaseStaging (ReleaseId, MusicBrainzIdRaw, Name, NameNormalized, SortName, ReleaseGroupId, ArtistCreditId) VALUES ");
                sb.Append(string.Join(",", valuesList));
                await context.Database.ExecuteSqlRawAsync(sb.ToString(), cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return totalCount;
    }

    private static string EscapeSqlString(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        return value.Replace("'", "''");
    }

    private static DateTime? ParseMusicBrainzDate(string? year, string? month, string? day)
    {
        var y = SafeParser.ToNumber<int?>(year);
        var m = SafeParser.ToNumber<int?>(month);
        var d = SafeParser.ToNumber<int?>(day);

        if (y is > 0 and < 9999)
        {
            var actualMonth = m is > 0 and <= 12 ? m.Value : 1;
            var actualDay = d is > 0 and <= 31 ? d.Value : 1;

            try
            {
                return new DateTime(y.Value, actualMonth, actualDay);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    #endregion
}

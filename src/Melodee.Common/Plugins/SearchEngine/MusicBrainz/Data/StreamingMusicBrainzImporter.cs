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
/// </summary>
public sealed class StreamingMusicBrainzImporter(ILogger logger)
{
    private const int BatchSize = 25000;
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
        
        // Configure SQLite for lower memory usage during bulk operations
        await context.Database.ExecuteSqlRawAsync("PRAGMA temp_store = FILE", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA cache_size = -500000", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA synchronous = OFF", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL", cancellationToken);

        // Phase 1: Stream artist data to staging tables
        await ImportArtistStagingDataAsync(context, mbDumpPath, progressCallback, cancellationToken);


        // Phase 2: Materialize artists using SQL
        await MaterializeArtistsAsync(context, progressCallback, cancellationToken);


        // Phase 3: Create Lucene index from materialized artists (streaming)
        await CreateLuceneIndexAsync(context, luceneIndexPath, progressCallback, cancellationToken);


        // Phase 4: Materialize artist relations using SQL
        await MaterializeArtistRelationsAsync(context, progressCallback, cancellationToken);

        // Phase 5: Drop artist staging tables
        await DropArtistStagingTablesAsync(context, progressCallback, cancellationToken);


        // Phase 6: Stream album support data to staging tables
        await ImportAlbumStagingDataAsync(context, mbDumpPath, progressCallback, cancellationToken);


        // Phase 7: Materialize albums using SQL
        await MaterializeAlbumsAsync(context, progressCallback, cancellationToken);

        // Phase 8: Drop album staging tables
        await DropAlbumStagingTablesAsync(context, progressCallback, cancellationToken);
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
            // Stream artist file to staging
            progressCallback?.Invoke("Loading Artists", 0, 4, "Streaming artist file to staging...");
            var artistCount = await StreamFileToStagingAsync(
                context,
                Path.Combine(mbDumpPath, "artist"),
                parts => new ArtistStaging
                {
                    ArtistId = SafeParser.ToNumber<long>(parts[0]),
                    MusicBrainzIdRaw = (SafeParser.ToGuid(parts[1]) ?? Guid.Empty).ToString(),
                    Name = parts[2].CleanString().TruncateLongString(MaxIndexSize) ?? string.Empty,
                    NameNormalized = parts[2].CleanString().TruncateLongString(MaxIndexSize)?.ToNormalizedString() ?? parts[2],
                    SortName = parts[3].CleanString(true).TruncateLongString(MaxIndexSize) ?? parts[2]
                },
                context.ArtistsStaging,
                cancellationToken);
            progressCallback?.Invoke("Loading Artists", 1, 4, $"Streamed {artistCount:N0} artists to staging");

            // Stream artist aliases to staging
            progressCallback?.Invoke("Loading Artists", 1, 4, "Streaming artist aliases to staging...");
            var aliasCount = await StreamFileToStagingAsync(
                context,
                Path.Combine(mbDumpPath, "artist_alias"),
                parts => new ArtistAliasStaging
                {
                    ArtistId = SafeParser.ToNumber<long>(parts[1]),
                    NameNormalized = parts[2].CleanString().TruncateLongString(MaxIndexSize)?.ToNormalizedString() ?? parts[2]
                },
                context.ArtistAliasesStaging,
                cancellationToken);
            progressCallback?.Invoke("Loading Artists", 2, 4, $"Streamed {aliasCount:N0} artist aliases to staging");

            // Stream links to staging
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

            // Stream artist-to-artist links to staging
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
            
            // Add indices to staging tables
            progressCallback?.Invoke("Loading Artists", 4, 4, "Creating staging indices...");
            await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_ArtistStaging_ArtistId ON ArtistStaging(ArtistId)", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_ArtistAliasStaging_ArtistId ON ArtistAliasStaging(ArtistId)", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_ArtistAliasStaging_NameNormalized ON ArtistAliasStaging(NameNormalized)", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_LinkArtistToArtistStaging_Artist0 ON LinkArtistToArtistStaging(Artist0)", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_LinkArtistToArtistStaging_Artist1 ON LinkArtistToArtistStaging(Artist1)", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_LinkArtistToArtistStaging_LinkId ON LinkArtistToArtistStaging(LinkId)", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_LinkStaging_LinkId ON LinkStaging(LinkId)", cancellationToken);
            progressCallback?.Invoke("Loading Artists", 4, 4, "Staging indices created");
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
                RAMBufferSizeMB = 256  // Utilize more memory for indexing to reduce flushes
            };
            using var writer = new IndexWriter(dir, indexConfig);

            var indexed = 0;
            var skip = 0;
            var batchCount = 0;
            const int luceneBatchSize = 5000;  // Smaller batches for Lucene

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
                batchCount++;
                
                // Periodic flush and GC to manage memory
                if (batchCount % 20 == 0)
                {
                    writer.Flush(triggerMerge: false, applyAllDeletes: false);
                }

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

            // Stream release to staging
            progressCallback?.Invoke("Loading Albums", 5, 6, "Streaming releases to staging...");
            var releaseCount = await StreamFileToStagingAsync(
                context,
                Path.Combine(mbDumpPath, "release"),
                parts => new ReleaseStaging
                {
                    ReleaseId = SafeParser.ToNumber<long>(parts[0]),
                    MusicBrainzIdRaw = parts[1],
                    Name = parts[2].CleanString().TruncateLongString(MaxIndexSize) ?? string.Empty,
                    NameNormalized = parts[2].CleanString().TruncateLongString(MaxIndexSize)?.ToNormalizedString() ?? parts[2],
                    SortName = parts[2].CleanString(true).TruncateLongString(MaxIndexSize) ?? parts[2],
                    ReleaseGroupId = SafeParser.ToNumber<long>(parts[4]),
                    ArtistCreditId = SafeParser.ToNumber<long>(parts[3])
                },
                context.ReleasesStaging,
                cancellationToken);
             progressCallback?.Invoke("Loading Albums", 6, 6, $"Streamed {releaseCount:N0} releases");
             
             // Add indices to staging tables
             progressCallback?.Invoke("Loading Albums", 6, 6, "Creating staging indices...");
             await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_ReleaseStaging_ReleaseGroupId ON ReleaseStaging(ReleaseGroupId)", cancellationToken);
             await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_ReleaseStaging_ReleaseId ON ReleaseStaging(ReleaseId)", cancellationToken);
             await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_ReleaseStaging_ArtistCreditId ON ReleaseStaging(ArtistCreditId)", cancellationToken);
             
             await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_ReleaseGroupStaging_ReleaseGroupId ON ReleaseGroupStaging(ReleaseGroupId)", cancellationToken);
             await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_ReleaseGroupMetaStaging_ReleaseGroupId ON ReleaseGroupMetaStaging(ReleaseGroupId)", cancellationToken);
             await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_ReleaseCountryStaging_ReleaseId ON ReleaseCountryStaging(ReleaseId)", cancellationToken);
             
             await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_ArtistCreditStaging_ArtistCreditId ON ArtistCreditStaging(ArtistCreditId)", cancellationToken);
             await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_ArtistCreditNameStaging_ArtistCreditId ON ArtistCreditNameStaging(ArtistCreditId)", cancellationToken);
             await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_ArtistCreditNameStaging_ArtistId ON ArtistCreditNameStaging(ArtistId)", cancellationToken);
             progressCallback?.Invoke("Loading Albums", 6, 6, "Staging indices created");
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





        progressCallback?.Invoke("Cleanup", 1, 1, "Album staging tables cleared");
    }

    #endregion

    #region Helper Methods



    /// <summary>
    /// Streams a file line-by-line to a SQLite staging table.
    /// Never loads more than BatchSize records into memory.
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

        var totalCount = 0;
        var batchCount = 0;
        var batch = new List<T>(BatchSize);

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
                    dbSet.AddRange(batch);
                    await context.SaveChangesAsync(cancellationToken);
                    context.ChangeTracker.Clear();
                    batch.Clear();
                    batchCount++;
                }
            }
            catch (Exception ex)
            {
                logger.Debug("StreamingImporter: Skipped malformed line in {File}: {Error}", 
                    Path.GetFileName(filePath), ex.Message);
            }
        }

        if (batch.Count > 0)
        {
            dbSet.AddRange(batch);
            await context.SaveChangesAsync(cancellationToken);
            context.ChangeTracker.Clear();
            batch.Clear();
        }

        return totalCount;
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

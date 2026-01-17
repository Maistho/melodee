using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Collection;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Caching;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Common.Services;

public class PlaylistService(
    ILogger logger,
    ICacheManager cacheManager,
    ISerializer serializer,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    LibraryService libraryService)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:playlist:apikey:{0}";
    private const string CacheKeyDetailTemplate = "urn:playlist:{0}";

    private async Task ClearCacheAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        var playlist = await GetAsync(playlistId, cancellationToken).ConfigureAwait(false);
        if (playlist.Data != null)
        {
            CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.FormatSmart(playlist.Data.ApiKey));
            CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(playlist.Data.Id));
        }
    }

    /// <summary>
    ///     Returns the total count of all playlists including all enabled dynamic playlists (for admin/system stats).
    /// </summary>
    public async Task<int> GetTotalPlaylistCountAsync(CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        // Count database playlists
        var dbPlaylistCount = await scopedContext.Playlists.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);

        // Count dynamic playlists
        var dynamicPlaylistCount = 0;
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
        var isDynamicPlaylistsDisabled = configuration.GetValue<bool>(SettingRegistry.PlaylistDynamicPlaylistsDisabled);
        if (!isDynamicPlaylistsDisabled)
        {
            var playlistLibrary = await libraryService.GetPlaylistLibraryAsync(cancellationToken).ConfigureAwait(false);
            if (playlistLibrary.Data != null)
            {
                var dynamicPlaylistsPath = Path.Combine(playlistLibrary.Data.Path, "dynamic");
                if (Directory.Exists(dynamicPlaylistsPath))
                {
                    var dynamicPlaylistsJsonFiles = dynamicPlaylistsPath
                        .ToFileSystemDirectoryInfo()
                        .AllFileInfos("*.json").ToArray();

                    // Count all enabled dynamic playlists
                    foreach (var dynamicPlaylistJsonFile in dynamicPlaylistsJsonFiles)
                    {
                        try
                        {
                            var dp = serializer.Deserialize<DynamicPlaylist>(
                                await File.ReadAllTextAsync(dynamicPlaylistJsonFile.FullName, cancellationToken)
                                    .ConfigureAwait(false));
                            if (dp != null && dp.IsEnabled)
                            {
                                dynamicPlaylistCount++;
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Warning(e, "Error reading dynamic playlist file [{File}]", dynamicPlaylistJsonFile.FullName);
                        }
                    }
                }
            }
        }

        return dbPlaylistCount + dynamicPlaylistCount;
    }

    /// <summary>
    ///     Returns the total count of all playlists including dynamic playlists for a specific user.
    /// </summary>
    public async Task<int> GetTotalPlaylistCountAsync(UserInfo userInfo, CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        // Count database playlists
        var dbPlaylistCount = await scopedContext.Playlists.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);

        // Count dynamic playlists
        var dynamicPlaylistCount = 0;
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
        var isDynamicPlaylistsDisabled = configuration.GetValue<bool>(SettingRegistry.PlaylistDynamicPlaylistsDisabled);
        if (!isDynamicPlaylistsDisabled)
        {
            var playlistLibrary = await libraryService.GetPlaylistLibraryAsync(cancellationToken).ConfigureAwait(false);
            if (playlistLibrary.Data != null)
            {
                var dynamicPlaylistsPath = Path.Combine(playlistLibrary.Data.Path, "dynamic");
                if (Directory.Exists(dynamicPlaylistsPath))
                {
                    var dynamicPlaylistsJsonFiles = dynamicPlaylistsPath
                        .ToFileSystemDirectoryInfo()
                        .AllFileInfos("*.json").ToArray();

                    // Count enabled dynamic playlists that are either public or for the specific user
                    foreach (var dynamicPlaylistJsonFile in dynamicPlaylistsJsonFiles)
                    {
                        try
                        {
                            var dp = serializer.Deserialize<DynamicPlaylist>(
                                await File.ReadAllTextAsync(dynamicPlaylistJsonFile.FullName, cancellationToken)
                                    .ConfigureAwait(false));
                            if (dp != null && dp.IsEnabled && (dp.IsPublic || (dp.ForUserId != null && dp.ForUserId == userInfo.ApiKey)))
                            {
                                dynamicPlaylistCount++;
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Warning(e, "Error reading dynamic playlist file [{File}]", dynamicPlaylistJsonFile.FullName);
                        }
                    }
                }
            }
        }

        return dbPlaylistCount + dynamicPlaylistCount;
    }

    /// <summary>
    ///     Return a paginated list of all playlists in the database.
    /// </summary>
    public async Task<PagedResult<Playlist>> ListAsync(
        UserInfo userInfo,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        int playlistCount;
        var playlists = new List<Playlist>();

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var baseQuery = scopedContext.Playlists
                .AsNoTracking()
                .Include(p => p.User);

            // Get total count efficiently
            playlistCount = await baseQuery.CountAsync(cancellationToken).ConfigureAwait(false);

            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                // Apply ordering (default by Name if no order specified)
                var orderBy = pagedRequest.OrderByValue();
                IQueryable<Playlist> orderedQuery;
                if (string.IsNullOrEmpty(orderBy) || orderBy == "\"Id\" ASC")
                {
                    orderedQuery = baseQuery.OrderBy(p => p.Name);
                }
                else
                {
                    // For complex ordering, fall back to the existing pattern if needed
                    orderedQuery = baseQuery.OrderBy(p => p.Id); // Simple fallback
                }

                // Apply pagination at the database level for better performance
                playlists = await orderedQuery
                    .Skip(pagedRequest.SkipValue)
                    .Take(pagedRequest.TakeValue)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        var dynamicPlaylists = await DynamicListAsync(userInfo, pagedRequest, cancellationToken);
        playlists.AddRange(dynamicPlaylists.Data);
        playlistCount += dynamicPlaylists.TotalCount;

        return new PagedResult<Playlist>
        {
            TotalCount = playlistCount,
            TotalPages = pagedRequest.TotalPages(playlistCount),
            Data = playlists
        };
    }

    /// <summary>
    ///     Returns a paginated list of dynamic (those which are file defined) Playlists.
    /// </summary>
    public async Task<PagedResult<Playlist>> DynamicListAsync(
        UserInfo userInfo,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        var playlistCount = 0;
        var playlists = new List<Playlist>();

        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
        var isDynamicPlaylistsDisabled = configuration.GetValue<bool>(SettingRegistry.PlaylistDynamicPlaylistsDisabled);
        if (!isDynamicPlaylistsDisabled)
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var playlistLibrary = await libraryService.GetPlaylistLibraryAsync(cancellationToken).ConfigureAwait(false);
                var dynamicPlaylistsJsonFiles = Path.Combine(playlistLibrary.Data.Path, "dynamic")
                    .ToFileSystemDirectoryInfo()
                    .AllFileInfos("*.json").ToArray();
                if (dynamicPlaylistsJsonFiles.Any())
                {
                    var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
                    var dynamicPlaylists = new List<DynamicPlaylist>();
                    foreach (var dynamicPlaylistJsonFile in dynamicPlaylistsJsonFiles)
                    {
                        dynamicPlaylists.Add(serializer.Deserialize<DynamicPlaylist>(
                            await File.ReadAllTextAsync(dynamicPlaylistJsonFile.FullName, cancellationToken)
                                .ConfigureAwait(false))!);
                    }

                    playlistCount = dynamicPlaylists.Count;

                    foreach (var dp in dynamicPlaylists.Where(x => x.IsEnabled))
                    {
                        try
                        {
                            if (dp.IsPublic || (dp.ForUserId != null && dp.ForUserId == userInfo.ApiKey))
                            {
                                var dbConn = scopedContext.Database.GetDbConnection();
                                var dpWhere = dp.PrepareSongSelectionWhere(userInfo);
                                var sql = $"""
                                           SELECT s."Id", s."ApiKey", s."IsLocked", s."Title", s."TitleNormalized", s."SongNumber", a."ReleaseDate",
                                                  a."Name" as "AlbumName", a."ApiKey" as "AlbumApiKey", ar."Name" as "ArtistName", ar."ApiKey" as "ArtistApiKey",
                                                  s."FileSize", s."Duration", s."CreatedAt", s."Tags", us."IsStarred" as "UserStarred", us."Rating" as "UserRating",
                                                  s."AlbumId", s."LastPlayedAt", s."PlayedCount", s."CalculatedRating"
                                           FROM "Songs" s
                                           join "Albums" a on (s."AlbumId" = a."Id")
                                           join "Artists" ar on (a."ArtistId" = ar."Id")
                                           left join "UserSongs" us on (s."Id" = us."SongId")
                                           where {dpWhere}
                                           """;
                                var songDataInfosForDp = (await dbConn
                                    .QueryAsync<SongDataInfo>(sql)
                                    .ConfigureAwait(false)).ToArray();
                                playlists.Add(new Playlist
                                {
                                    Id = 1,
                                    IsLocked = false,
                                    SortOrder = 0,
                                    ApiKey = dp.Id,
                                    CreatedAt = now,
                                    Description = dp.Comment,
                                    Name = dp.Name,
                                    Comment = dp.Comment,
                                    User = ServiceUser.Instance.Value,
                                    IsDynamic = true,
                                    IsPublic = true,
                                    SongCount = SafeParser.ToNumber<short>(songDataInfosForDp.Count()),
                                    Duration = songDataInfosForDp.Sum(x => x.Duration),
                                    AllowedUserIds = userInfo.UserName,
                                    Songs = []
                                });




                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Warning(e, "[{Name}] error loading dynamic playlist [{Playlist}]",
                                nameof(OpenSubsonicApiService), dp.Name);
                            throw;
                        }
                    }
                }
            }
        }

        playlists = playlists.Skip(pagedRequest.SkipValue).Take(pagedRequest.TakeValue).ToList();

        return new PagedResult<Playlist>
        {
            TotalCount = playlistCount,
            TotalPages = pagedRequest.TotalPages(playlistCount),
            Data = playlists
        };
    }

    public async Task<MelodeeModels.PagedResult<SongDataInfo>> SongsForPlaylistAsync(Guid apiKey, MelodeeModels.UserInfo userInfo, MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(_ => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        var playlistResult = await GetByApiKeyAsync(userInfo, apiKey, cancellationToken).ConfigureAwait(false);
        if (!playlistResult.IsSuccess)
        {
            return new MelodeeModels.PagedResult<SongDataInfo>(["Unknown playlist"])
            {
                Data = [],
                TotalCount = 0,
                TotalPages = 0,
                Type = MelodeeModels.OperationResponseType.NotFound
            };
        }

        var songCount = 0;
        SongDataInfo[] songs;

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            if (playlistResult.Data!.IsDynamic)
            {
                // For dynamic playlists, use EF Core queries instead of raw SQL
                var dynamicPlaylist = await libraryService.GetDynamicPlaylistAsync(apiKey, cancellationToken).ConfigureAwait(false);
                var dp = dynamicPlaylist.Data;
                if (dp == null)
                {
                    return new MelodeeModels.PagedResult<SongDataInfo>(["Unknown playlist"])
                    {
                        Data = [],
                        TotalCount = 0,
                        TotalPages = 0,
                        Type = MelodeeModels.OperationResponseType.NotFound
                    };
                }
                var dbConn = scopedContext.Database.GetDbConnection();
                var dpWhere = dp.PrepareSongSelectionWhere(userInfo);
                var dpOrderBy = dp.SongSelectionOrder ?? "RANDOM()";
                var sql = $"""
                           SELECT COUNT(s."Id")
                           FROM "Songs" s
                           join "Albums" a on (s."AlbumId" = a."Id")
                           join "Artists" ar on (a."ArtistId" = ar."Id")
                           left join "UserSongs" us on (s."Id" = us."SongId")
                           left join "UserSongs" uus on (s."Id" = uus."SongId" and uus."UserId" = {userInfo.Id})
                           where {dpWhere}
                           """;
                songCount = await dbConn
                    .QuerySingleAsync<int>(sql)
                    .ConfigureAwait(false);

                sql = $"""
                       SELECT s."Id", s."ApiKey", s."IsLocked", s."Title", s."TitleNormalized", s."SongNumber", a."ReleaseDate",
                              a."Name" as "AlbumName", a."ApiKey" as "AlbumApiKey", ar."Name" as "ArtistName", ar."ApiKey" as "ArtistApiKey",
                              s."FileSize", s."Duration", s."CreatedAt", s."Tags", uus."IsStarred" as "UserStarred", uus."Rating" as "UserRating",
                              s."AlbumId", s."LastPlayedAt", s."PlayedCount", s."CalculatedRating"
                       FROM "Songs" s
                       join "Albums" a on (s."AlbumId" = a."Id")
                       join "Artists" ar on (a."ArtistId" = ar."Id")
                       left join "UserSongs" us on (s."Id" = us."SongId")
                       left join "UserSongs" uus on (s."Id" = uus."SongId" and uus."UserId" = {userInfo.Id})
                       where {dpWhere}
                       order by {dpOrderBy}
                       offset {pagedRequest.SkipValue} rows fetch next {pagedRequest.TakeValue} rows only;
                       """;
                songs = (await dbConn
                    .QueryAsync<SongDataInfo>(sql)
                    .ConfigureAwait(false)).ToArray();
            }
            else
            {
                // Optimized EF Core query for regular playlists with UserSong data
                var query = scopedContext
                    .PlaylistSong
                    .AsNoTracking()
                    .Where(ps => ps.Playlist.ApiKey == apiKey)
                    .Include(ps => ps.Song)
                    .ThenInclude(s => s.Album)
                    .ThenInclude(a => a.Artist)
                    .Include(ps => ps.Song.UserSongs.Where(us => us.UserId == userInfo.Id))
                    .OrderBy(ps => ps.PlaylistOrder);

                // Get total count efficiently
                songCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

                if (songCount > 0)
                {
                    // Apply pagination at database level for better performance
                    var playlistSongs = await query
                        .Skip(pagedRequest.SkipValue)
                        .Take(pagedRequest.TakeValue)
                        .ToListAsync(cancellationToken)
                        .ConfigureAwait(false);

                    // Create SongDataInfo objects with proper UserSong data
                    songs = playlistSongs.Select(ps =>
                    {
                        var userSong = ps.Song.UserSongs.FirstOrDefault(us => us.UserId == userInfo.Id);
                        return new SongDataInfo(
                            ps.Song.Id,
                            ps.Song.ApiKey,
                            ps.Song.IsLocked,
                            ps.Song.Title,
                            ps.Song.TitleNormalized,
                            ps.Song.SongNumber,
                            ps.Song.Album.ReleaseDate,
                            ps.Song.Album.Name,
                            ps.Song.Album.ApiKey,
                            ps.Song.Album.Artist.Name,
                            ps.Song.Album.Artist.ApiKey,
                            ps.Song.FileSize,
                            ps.Song.Duration,
                            ps.Song.CreatedAt,
                            ps.Song.Tags ?? "",
                            userSong?.IsStarred ?? false,
                            userSong?.Rating ?? 0,
                            ps.Song.AlbumId,
                            ps.Song.LastPlayedAt,
                            ps.Song.PlayedCount,
                            ps.Song.CalculatedRating
                        );
                    }).ToArray();
                }
                else
                {
                    songs = [];
                }
            }
        }

        return new MelodeeModels.PagedResult<SongDataInfo>
        {
            TotalCount = songCount,
            TotalPages = pagedRequest.TotalPages(songCount),
            Data = songs
        };
    }

    public async Task<OperationResult<Playlist?>> GetByApiKeyAsync(
        UserInfo userInfo,
        Guid apiKey,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(_ => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        var id = await CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext.Playlists
                    .AsNoTracking()
                    .Where(p => p.ApiKey == apiKey)
                    .Select(p => (int?)p.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        if ((id ?? 0) < 1)
        {
            // See if Dynamic playlist exists for given ApiKey. If so return it versus calling detail.
            var dynamicPlayLists = await DynamicListAsync(userInfo, new PagedRequest { PageSize = short.MaxValue }, cancellationToken).ConfigureAwait(false);
            var dynamicPlaylist = dynamicPlayLists.Data.FirstOrDefault(x => x.ApiKey == apiKey);
            if (dynamicPlaylist != null)
            {
                return new OperationResult<Playlist?>
                {
                    Data = dynamicPlaylist
                };
            }

            return new OperationResult<Playlist?>("Unknown playlist.")
            {
                Data = null
            };
        }

        return await GetAsync(id!.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task<OperationResult<Playlist?>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext =
                         await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .Playlists
                    .Include(x => x.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        return new OperationResult<Playlist?>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> DeleteAsync(
        int currentUserId,
        int[] playlistIds,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(playlistIds, nameof(playlistIds));


        bool result;
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var user = await scopedContext.Users.FirstOrDefaultAsync(x => x.Id == currentUserId, cancellationToken)
                .ConfigureAwait(false);
            if (user == null)
            {
                return new OperationResult<bool>("Unknown user.")
                {
                    Data = false
                };
            }

            foreach (var playlistId in playlistIds)
            {
                // Load playlist in current context to avoid tracking conflicts
                var playlist = await scopedContext.Playlists
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.Id == playlistId, cancellationToken)
                    .ConfigureAwait(false);

                if (playlist == null)
                {
                    return new OperationResult<bool>("Unknown playlist.")
                    {
                        Data = false
                    };
                }

                if (!user.CanDeletePlaylist(playlist))
                {
                    return new OperationResult<bool>("User does not have access to delete playlist.")
                    {
                        Data = false
                    };
                }

                scopedContext.Playlists.Remove(playlist);
                await ClearCacheAsync(playlistId, cancellationToken).ConfigureAwait(false);
            }

            result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
        }


        return new OperationResult<bool>
        {
            Data = result
        };
    }

    /// <summary>
    /// Gets a playlist by API key for internal operations (no user access control).
    /// </summary>
    public async Task<OperationResult<Playlist?>> GetByApiKeyAsync(Guid apiKey, CancellationToken cancellationToken)
    {
        var id = await CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            return await scopedContext.Playlists
                .AsNoTracking()
                .Where(p => p.ApiKey == apiKey)
                .Select(p => p.Id)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return id > 0 ? await GetAsync(id, cancellationToken).ConfigureAwait(false) : new OperationResult<Playlist?> { Data = null };
    }

    /// <summary>
    /// Gets a playlist by API key for internal operations (no user access control).
    /// </summary>
    private async Task<OperationResult<Playlist?>> GetByApiKeyInternalAsync(Guid apiKey, CancellationToken cancellationToken)
    {
        var id = await CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            return await scopedContext.Playlists
                .AsNoTracking()
                .Where(p => p.ApiKey == apiKey)
                .Select(p => p.Id)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return id > 0 ? await GetAsync(id, cancellationToken).ConfigureAwait(false) : new OperationResult<Playlist?> { Data = null };
    }

    /// <summary>
    /// Gets the image bytes and ETag for a playlist.
    /// </summary>
    public async Task<ImageBytesAndEtag> GetPlaylistImageBytesAndEtagAsync(
        Guid playlistApiKey,
        string? size,
        CancellationToken cancellationToken = default)
    {
        var playlist = await GetByApiKeyInternalAsync(playlistApiKey, cancellationToken).ConfigureAwait(false);

        if (!playlist.IsSuccess || playlist.Data == null)
        {
            return new ImageBytesAndEtag(null, null);
        }

        var playlistLibrary = await libraryService.GetPlaylistLibraryAsync(cancellationToken).ConfigureAwait(false);
        if (!playlistLibrary.IsSuccess || playlistLibrary.Data == null)
        {
            return new ImageBytesAndEtag(null, null);
        }

        var playlistImageFilename = playlist.Data.ToImageFileName(playlistLibrary.Data.Path);
        var playlistImageFileInfo = new FileInfo(playlistImageFilename);

        if (playlistImageFileInfo.Exists)
        {
            var imageBytes = await File.ReadAllBytesAsync(playlistImageFileInfo.FullName, cancellationToken).ConfigureAwait(false);
            var etag = playlistImageFileInfo.LastWriteTimeUtc.ToEtag();
            return new ImageBytesAndEtag(imageBytes, etag);
        }

        return new ImageBytesAndEtag(null, null);
    }

    /// <summary>
    /// Uploads an image for a playlist.
    /// </summary>
    public async Task<OperationResult<bool>> UploadPlaylistImageAsync(
        Guid playlistApiKey,
        int userId,
        byte[] imageBytes,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x == Guid.Empty, playlistApiKey, nameof(playlistApiKey));
        Guard.Against.NullOrEmpty(imageBytes, nameof(imageBytes));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var playlist = await scopedContext.Playlists
            .FirstOrDefaultAsync(x => x.ApiKey == playlistApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (playlist == null)
        {
            return new OperationResult<bool>("Playlist not found.")
            {
                Data = false,
                Type = OperationResponseType.NotFound
            };
        }

        if (playlist.UserId != userId)
        {
            return new OperationResult<bool>("Access denied.")
            {
                Data = false,
                Type = OperationResponseType.AccessDenied
            };
        }

        var playlistLibrary = await libraryService.GetPlaylistLibraryAsync(cancellationToken).ConfigureAwait(false);
        if (!playlistLibrary.IsSuccess || playlistLibrary.Data == null)
        {
            return new OperationResult<bool>("Playlist library not found.")
            {
                Data = false
            };
        }

        var imagesDirectory = Path.Combine(playlistLibrary.Data.Path, Playlist.ImagesDirectoryName);
        if (!Directory.Exists(imagesDirectory))
        {
            Directory.CreateDirectory(imagesDirectory);
        }

        var playlistImageFilename = playlist.ToImageFileName(playlistLibrary.Data.Path);

        // Convert to GIF format to support animations
        var gifImageBytes = await ImageConvertor.ConvertToGifFormat(imageBytes, cancellationToken).ConfigureAwait(false);
        await File.WriteAllBytesAsync(playlistImageFilename, gifImageBytes, cancellationToken).ConfigureAwait(false);

        playlist.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await ClearCacheAsync(playlist.Id, cancellationToken).ConfigureAwait(false);

        Logger.Information("User [{UserId}] uploaded image for playlist [{PlaylistName}]", userId, playlist.Name);

        return new OperationResult<bool>
        {
            Data = true
        };
    }

    /// <summary>
    /// Deletes the image for a playlist.
    /// </summary>
    public async Task<OperationResult<bool>> DeletePlaylistImageAsync(
        Guid playlistApiKey,
        int userId,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x == Guid.Empty, playlistApiKey, nameof(playlistApiKey));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var playlist = await scopedContext.Playlists
            .FirstOrDefaultAsync(x => x.ApiKey == playlistApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (playlist == null)
        {
            return new OperationResult<bool>("Playlist not found.")
            {
                Data = false,
                Type = OperationResponseType.NotFound
            };
        }

        if (playlist.UserId != userId)
        {
            return new OperationResult<bool>("Access denied.")
            {
                Data = false,
                Type = OperationResponseType.AccessDenied
            };
        }

        var playlistLibrary = await libraryService.GetPlaylistLibraryAsync(cancellationToken).ConfigureAwait(false);
        if (!playlistLibrary.IsSuccess || playlistLibrary.Data == null)
        {
            return new OperationResult<bool>("Playlist library not found.")
            {
                Data = false
            };
        }

        var playlistImageFilename = playlist.ToImageFileName(playlistLibrary.Data.Path);

        if (File.Exists(playlistImageFilename))
        {
            File.Delete(playlistImageFilename);
            Logger.Information("User [{UserId}] deleted image for playlist [{PlaylistName}]", userId, playlist.Name);
        }

        playlist.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await ClearCacheAsync(playlist.Id, cancellationToken).ConfigureAwait(false);

        return new OperationResult<bool>
        {
            Data = true
        };
    }

    /// <summary>
    /// Adds songs to a playlist.
    /// </summary>
    public async Task<OperationResult<bool>> AddSongsToPlaylistAsync(
        Guid playlistApiKey,
        IEnumerable<Guid> songApiKeys,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x == Guid.Empty, playlistApiKey, nameof(playlistApiKey));
        Guard.Against.NullOrEmpty(songApiKeys, nameof(songApiKeys));

        var result = false;
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var playlist = await scopedContext.Playlists
            .Include(x => x.Songs)
            .FirstOrDefaultAsync(x => x.ApiKey == playlistApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (playlist != null)
        {
            var songs = await scopedContext.Songs
                .Where(x => songApiKeys.Contains(x.ApiKey))
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var song in songs)
            {
                var existingPlaylistSong = playlist.Songs.FirstOrDefault(x => x.SongId == song.Id);
                if (existingPlaylistSong == null)
                {
                    playlist.Songs.Add(new PlaylistSong
                    {
                        PlaylistOrder = playlist.Songs.Count + 1,
                        Song = song
                    });
                }
            }

            playlist.Duration = playlist.Songs.Sum(x => x.Song.Duration);
            playlist.SongCount = (short)playlist.Songs.Count;
            playlist.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);

            result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

            if (result)
            {
                await ClearCacheAsync(playlist.Id, cancellationToken).ConfigureAwait(false);
            }
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    /// <summary>
    /// Removes songs from a playlist by song API keys.
    /// </summary>
    public async Task<OperationResult<bool>> RemoveSongsFromPlaylistAsync(
        Guid playlistApiKey,
        IEnumerable<Guid> songApiKeys,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x == Guid.Empty, playlistApiKey, nameof(playlistApiKey));
        Guard.Against.NullOrEmpty(songApiKeys, nameof(songApiKeys));

        var result = false;
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var playlist = await scopedContext.Playlists
            .Include(x => x.Songs).ThenInclude(x => x.Song)
            .FirstOrDefaultAsync(x => x.ApiKey == playlistApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (playlist != null)
        {
            // Use HashSet for O(1) lookups
            var keys = new HashSet<Guid>(songApiKeys);
            var songsToRemove = new List<PlaylistSong>();

            // Identify songs to remove in single pass
            foreach (var ps in playlist.Songs)
            {
                if (keys.Contains(ps.Song.ApiKey))
                {
                    songsToRemove.Add(ps);
                }
            }

            // Remove identified songs
            foreach (var playlistSong in songsToRemove)
            {
                playlist.Songs.Remove(playlistSong);
            }

            // Reorder remaining songs in single pass - avoid multiple ToList/OrderBy calls
            var remainingSongs = playlist.Songs.ToArray(); // Single materialization
            for (int i = 0; i < remainingSongs.Length; i++)
            {
                remainingSongs[i].PlaylistOrder = i + 1;
            }

            playlist.Duration = playlist.Songs.Sum(x => x.Song.Duration);
            playlist.SongCount = (short)playlist.Songs.Count;
            playlist.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);

            result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

            if (result)
            {
                await ClearCacheAsync(playlist.Id, cancellationToken).ConfigureAwait(false);
            }
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    /// <summary>
    /// Removes songs from a playlist by playlist song indexes.
    /// </summary>
    public async Task<OperationResult<bool>> RemoveSongsByIndexFromPlaylistAsync(
        Guid playlistApiKey,
        IEnumerable<int> songIndexes,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x == Guid.Empty, playlistApiKey, nameof(playlistApiKey));
        Guard.Against.NullOrEmpty(songIndexes, nameof(songIndexes));

        var result = false;
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var playlist = await scopedContext.Playlists
            .Include(x => x.Songs).ThenInclude(x => x.Song)
            .FirstOrDefaultAsync(x => x.ApiKey == playlistApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (playlist != null)
        {
            var orderedSongs = playlist.Songs.OrderBy(x => x.PlaylistOrder).ToArray();
            var songsToRemove = new List<PlaylistSong>();

            foreach (var index in songIndexes.Where(i => i >= 0 && i < orderedSongs.Length))
            {
                songsToRemove.Add(orderedSongs[index]);
            }

            foreach (var playlistSong in songsToRemove)
            {
                playlist.Songs.Remove(playlistSong);
            }

            // Reorder remaining songs in single pass - avoid multiple ToList/OrderBy calls
            var remainingSongs = playlist.Songs.ToArray(); // Single materialization
            for (int i = 0; i < remainingSongs.Length; i++)
            {
                remainingSongs[i].PlaylistOrder = i + 1;
            }

            playlist.Duration = playlist.Songs.Sum(x => x.Song.Duration);
            playlist.SongCount = (short)playlist.Songs.Count;
            playlist.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);

            result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

            if (result)
            {
                await ClearCacheAsync(playlist.Id, cancellationToken).ConfigureAwait(false);
            }
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    /// <summary>
    /// Reorder songs in a playlist. The songApiKeys array represents the new order of songs.
    /// </summary>
    public async Task<OperationResult<bool>> ReorderPlaylistSongsAsync(
        Guid playlistApiKey,
        int userId,
        Guid[] songApiKeysInNewOrder,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x == Guid.Empty, playlistApiKey, nameof(playlistApiKey));
        Guard.Against.NullOrEmpty(songApiKeysInNewOrder, nameof(songApiKeysInNewOrder));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var playlist = await scopedContext.Playlists
            .Include(x => x.Songs)
            .FirstOrDefaultAsync(x => x.ApiKey == playlistApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (playlist == null)
        {
            return new OperationResult<bool>("Playlist not found.")
            {
                Data = false,
                Type = OperationResponseType.NotFound
            };
        }

        if (playlist.UserId != userId)
        {
            return new OperationResult<bool>("Access denied.")
            {
                Data = false,
                Type = OperationResponseType.AccessDenied
            };
        }

        // Build a map of song API key to playlist song
        var songMap = playlist.Songs.ToDictionary(s => s.SongApiKey, s => s);

        // Validate that all provided song API keys exist in the playlist
        var missingKeys = songApiKeysInNewOrder.Where(k => !songMap.ContainsKey(k)).ToArray();
        if (missingKeys.Length > 0)
        {
            return new OperationResult<bool>($"Songs not found in playlist: {string.Join(", ", missingKeys)}")
            {
                Data = false,
                Type = OperationResponseType.ValidationFailure
            };
        }

        // Update the order based on the new array
        for (var i = 0; i < songApiKeysInNewOrder.Length; i++)
        {
            if (songMap.TryGetValue(songApiKeysInNewOrder[i], out var playlistSong))
            {
                playlistSong.PlaylistOrder = i;
            }
        }

        playlist.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);

        var result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

        if (result)
        {
            await ClearCacheAsync(playlist.Id, cancellationToken).ConfigureAwait(false);
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    /// <summary>
    /// Get playlists for a user with full include structure needed for OpenSubsonic API
    /// </summary>
    public async Task<OperationResult<Playlist[]>> GetPlaylistsForUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        // Only include what's needed for ToApiPlaylist(false): User reference.
        // Avoid loading songs/albums here to prevent heavy graph materialization and N+1 patterns.
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var playlists = await scopedContext
            .Playlists
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .AsNoTracking()
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        sw.Stop();
        Logger.Debug("[PlaylistService] GetPlaylistsForUserAsync loaded {Count} playlists for user {UserId} in {ElapsedMs} ms",
            playlists.Length, userId, sw.ElapsedMilliseconds);

        return new OperationResult<Playlist[]>
        {
            Data = playlists
        };
    }

    /// <summary>
    /// Streams playlists for a user in bounded batches to avoid loading everything in memory.
    /// </summary>
    public async IAsyncEnumerable<Playlist> StreamPlaylistsForUserInBatchesAsync(
        int userId,
        int batchSize = 250,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));
        batchSize = Math.Max(1, Math.Min(batchSize, 1000));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var query = scopedContext
            .Playlists
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Id);

        var skip = 0;
        while (true)
        {
            var page = await query
                .Skip(skip)
                .Take(batchSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (page.Count == 0)
            {
                yield break;
            }

            foreach (var item in page)
            {
                yield return item;
            }

            skip += page.Count;
        }
    }

    /// <summary>
    /// Get playlists for a user with pagination support.
    /// </summary>
    public async Task<PagedResult<Playlist>> GetPlaylistsForUserAsync(
        int userId,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var query = scopedContext
            .Playlists
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        // Default ordering by Name to keep consistent UX
        query = query.OrderBy(p => p.Name);

        var data = await query
            .Skip(pagedRequest.SkipValue)
            .Take(pagedRequest.TakeValue)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<Playlist>
        {
            TotalCount = totalCount,
            TotalPages = pagedRequest.TotalPages(totalCount),
            Data = data
        };
    }

    /// <summary>
    /// Update playlist metadata (name, comment, isPublic) for OpenSubsonic API
    /// </summary>
    public async Task<OperationResult<bool>> UpdatePlaylistMetadataAsync(
        Guid playlistApiKey,
        int currentUserId,
        string? name = null,
        string? comment = null,
        bool? isPublic = null,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(_ => playlistApiKey == Guid.Empty, playlistApiKey, nameof(playlistApiKey));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var playlist = await scopedContext
            .Playlists
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.ApiKey == playlistApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (playlist == null)
        {
            return new OperationResult<bool>("Playlist not found.")
            {
                Data = false,
                Type = OperationResponseType.NotFound
            };
        }

        if (playlist.UserId != currentUserId)
        {
            return new OperationResult<bool>("Access denied.")
            {
                Data = false,
                Type = OperationResponseType.AccessDenied
            };
        }

        // Update playlist metadata
        if (name != null) playlist.Name = name;
        if (comment != null) playlist.Comment = comment;
        if (isPublic.HasValue) playlist.IsPublic = isPublic.Value;

        playlist.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);

        var result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

        if (result)
        {
            await ClearCacheAsync(playlist.Id, cancellationToken).ConfigureAwait(false);
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    /// <summary>
    /// Delete playlist by API key with user authorization check
    /// </summary>
    public async Task<OperationResult<bool>> DeleteByApiKeyAsync(
        Guid playlistApiKey,
        int userId,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var playlist = await scopedContext
            .Playlists
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.ApiKey == playlistApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (playlist == null)
        {
            return new OperationResult<bool>("Playlist not found.")
            {
                Data = false
            };
        }

        if (playlist.UserId != userId)
        {
            return new OperationResult<bool>("User not authorized to delete this playlist.")
            {
                Data = false
            };
        }

        scopedContext.Playlists.Remove(playlist);
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await ClearCacheAsync(playlist.Id, cancellationToken).ConfigureAwait(false);

        Logger.Information("User [{UserId}] deleted playlist [{PlaylistName}]", userId, playlist.Name);

        return new OperationResult<bool>
        {
            Data = true
        };
    }

    /// <summary>
    /// Create a new playlist with songs
    /// </summary>
    /// <param name="returnPrefixedApiKey">When true, returns "playlist|{guid}" format for OpenSubsonic API; when false, returns raw GUID string for Melodee API.</param>
    public async Task<OperationResult<string?>> CreatePlaylistAsync(
        string name,
        int userId,
        string? comment = null,
        bool isPublic = false,
        IEnumerable<Guid>? songApiKeys = null,
        bool returnPrefixedApiKey = true,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        var songApiKeyArray = songApiKeys?.ToArray() ?? [];

        // Get songs for the playlist if provided
        var songsForPlaylist = Array.Empty<Data.Models.Song>();
        if (songApiKeyArray.Length > 0)
        {
            songsForPlaylist = await scopedContext.Songs
                .Where(x => songApiKeyArray.Contains(x.ApiKey))
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        var newPlaylist = new Playlist
        {
            CreatedAt = now,
            Name = name,
            Comment = comment,
            IsPublic = isPublic,
            UserId = userId,
            SongCount = SafeParser.ToNumber<short>(songsForPlaylist.Length),
            Duration = songsForPlaylist.Sum(x => x.Duration),
            Songs = songsForPlaylist.Select((x, i) => new PlaylistSong
            {
                SongId = x.Id,
                SongApiKey = x.ApiKey,
                PlaylistOrder = i
            }).ToArray()
        };

        await scopedContext.Playlists.AddAsync(newPlaylist, cancellationToken).ConfigureAwait(false);
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Logger.Information("Playlist created for user [{UserId}] with [{SongCount}] songs.", userId, songsForPlaylist.Length);

        return new OperationResult<string?>
        {
            Data = returnPrefixedApiKey ? newPlaylist.ToApiKey() : newPlaylist.ApiKey.ToString()
        };
    }

    /// <summary>
    /// Import an M3U/M3U8 playlist file into the system
    /// </summary>
    public async Task<OperationResult<PlaylistImportResult>> ImportPlaylistAsync(
        int userId,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            // Read file bytes for storage
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            var fileBytes = memoryStream.ToArray();
            
            // Parse the M3U file
            memoryStream.Position = 0;
            var parser = new Parsing.M3UParser(Logger);
            var parseResult = await parser.ParseAsync(memoryStream, fileName, cancellationToken).ConfigureAwait(false);

            if (parseResult.Entries.Count == 0)
            {
                return new OperationResult<PlaylistImportResult>
                {
                    Data = new PlaylistImportResult
                    {
                        PlaylistId = 0,
                        PlaylistApiKey = Guid.Empty,
                        TotalEntries = 0,
                        MatchedEntries = 0,
                        MissingEntries = 0,
                        PlaylistName = string.Empty
                    },
                    Type = OperationResponseType.Error,
                    Errors = new[] { new Exception("Playlist file contains no valid entries") }
                };
            }

            // Create uploaded file record
            var uploadedFile = new PlaylistUploadedFile
            {
                UserId = userId,
                OriginalFileName = fileName,
                ContentType = fileName.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase) 
                    ? "audio/x-mpegurl; charset=utf-8" 
                    : "audio/x-mpegurl",
                Length = fileBytes.Length,
                FileData = fileBytes,
                CreatedAt = now
            };

            await scopedContext.PlaylistUploadedFiles.AddAsync(uploadedFile, cancellationToken).ConfigureAwait(false);
            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // Get library path for matching (use first storage library)
            var librariesResult = await libraryService.GetStorageLibrariesAsync(cancellationToken).ConfigureAwait(false);
            var libraryPath = librariesResult.Data?.FirstOrDefault()?.Path;

            // Create song matching service
            var songMatcher = new SongMatchingService(Logger, CacheManager, ContextFactory);

            // Match songs
            var matchedSongs = new List<Data.Models.Song>();
            var missingItems = new List<PlaylistUploadedFileItem>();
            var matchedCount = 0;

            foreach (var entry in parseResult.Entries)
            {
                var matchResult = await songMatcher.MatchEntryAsync(entry, libraryPath, cancellationToken).ConfigureAwait(false);

                var item = new PlaylistUploadedFileItem
                {
                    PlaylistUploadedFileId = uploadedFile.Id,
                    SongId = matchResult.Song?.Id,
                    SortOrder = entry.SortOrder,
                    Status = matchResult.Song != null ? Enums.PlaylistItemStatus.Resolved : Enums.PlaylistItemStatus.Missing,
                    RawReference = entry.RawReference,
                    NormalizedReference = entry.NormalizedReference,
                    HintsJson = serializer.Serialize(new
                    {
                        FileName = entry.FileName,
                        ArtistFolder = entry.ArtistFolder,
                        AlbumFolder = entry.AlbumFolder,
                        MatchStrategy = matchResult.MatchStrategy.ToString(),
                        Confidence = matchResult.Confidence
                    }),
                    LastAttemptUtc = now
                };

                if (matchResult.Song != null)
                {
                    matchedSongs.Add(matchResult.Song);
                    matchedCount++;
                }
                else
                {
                    missingItems.Add(item);
                }

                await scopedContext.PlaylistUploadedFileItems.AddAsync(item, cancellationToken).ConfigureAwait(false);
            }

            // Create the playlist
            var playlistName = Path.GetFileNameWithoutExtension(fileName);
            var playlist = new Playlist
            {
                CreatedAt = now,
                Name = playlistName,
                Comment = $"Imported from {fileName}",
                IsPublic = false,
                UserId = userId,
                SourceType = Enums.PlaylistSourceType.M3UImport,
                PlaylistUploadedFileId = uploadedFile.Id,
                SongCount = SafeParser.ToNumber<short>(matchedSongs.Count),
                Duration = matchedSongs.Sum(x => x.Duration),
                Songs = matchedSongs.Select((song, index) => new PlaylistSong
                {
                    SongId = song.Id,
                    SongApiKey = song.ApiKey,
                    PlaylistOrder = index
                }).ToArray()
            };

            await scopedContext.Playlists.AddAsync(playlist, cancellationToken).ConfigureAwait(false);
            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            Logger.Information(
                "Playlist imported from [{FileName}] for user [{UserId}]. Matched: {Matched}/{Total} songs",
                fileName, userId, matchedCount, parseResult.Entries.Count);

            return new OperationResult<PlaylistImportResult>
            {
                Data = new PlaylistImportResult
                {
                    PlaylistId = playlist.Id,
                    PlaylistApiKey = playlist.ApiKey,
                    TotalEntries = parseResult.Entries.Count,
                    MatchedEntries = matchedCount,
                    MissingEntries = parseResult.Entries.Count - matchedCount,
                    PlaylistName = playlist.Name
                }
            };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error importing playlist from file [{FileName}]", fileName);
            return new OperationResult<PlaylistImportResult>
            {
                Data = new PlaylistImportResult
                {
                    PlaylistId = 0,
                    PlaylistApiKey = Guid.Empty,
                    TotalEntries = 0,
                    MatchedEntries = 0,
                    MissingEntries = 0,
                    PlaylistName = string.Empty
                },
                Type = OperationResponseType.Error,
                Errors = new[] { ex }
            };
        }
    }
}

public sealed class PlaylistImportResult
{
    public int PlaylistId { get; init; }
    public Guid PlaylistApiKey { get; init; }
    public int TotalEntries { get; init; }
    public int MatchedEntries { get; init; }
    public int MissingEntries { get; init; }
    public required string PlaylistName { get; init; }
}

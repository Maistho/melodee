using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;
using Melodee.Common.Plugins.Scrobbling;
using Melodee.Common.Services.Caching;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services;

public class ScrobbleService(
    ILogger logger,
    ICacheManager cacheManager,
    AlbumService? albumService,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory,
    INowPlayingRepository nowPlayingRepository)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);

    private bool _initialized;

    private IScrobbler[] _scrobblers = [];

    public async Task InitializeAsync(IMelodeeConfiguration? configuration = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _configuration = configuration ??
                             await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false) ??
                             new MelodeeConfiguration([]);

            var scrobblers = new List<IScrobbler>();

            // Only create MelodeeScrobbler if all dependencies are available
            if (albumService != null && ContextFactory != null && nowPlayingRepository != null)
            {
                scrobblers.Add(new MelodeeScrobbler(albumService, ContextFactory, nowPlayingRepository, Logger)
                {
                    IsEnabled = true
                });
            }

            scrobblers.Add(new LastFmScrobbler(_configuration, ContextFactory!, Logger)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.ScrobblingLastFmEnabled)
            });

            _scrobblers = scrobblers.ToArray();
            _initialized = true;
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Failed to initialize ScrobbleService");
            _initialized = false;
            throw;
        }
    }

    private void CheckInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Scrobble service is not initialized.");
        }
    }

    /// <summary>
    ///     Returns the actively playing songs and user infos.
    /// </summary>
    public Task<OperationResult<NowPlayingInfo[]>> GetNowPlaying(CancellationToken cancellationToken = default)
    {
        return nowPlayingRepository.GetNowPlayingAsync(cancellationToken);
    }

    public async Task<OperationResult<bool>> NowPlaying(UserInfo user, Guid id, double? time, string playerName, string? userAgent = null, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        Logger.Information("[{ServiceName}] NowPlaying invoked.", nameof(ScrobbleService));

        var result = true;
        var databaseSongScrobbleInfo = await DatabaseSongScrobbleInfoForSongApiKey(id, cancellationToken).ConfigureAwait(false);
        if (databaseSongScrobbleInfo != null)
        {
            Logger.Information("[{ServiceName}] Found song info for NowPlaying.", nameof(ScrobbleService));

            var secondsPlayed = time.HasValue ? (int?)Math.Round(time.Value) : null;

            var scrobble = new ScrobbleInfo
            (
                databaseSongScrobbleInfo.SongApiKey,
                databaseSongScrobbleInfo.ArtistId,
                databaseSongScrobbleInfo.AlbumId,
                databaseSongScrobbleInfo.SongId,
                databaseSongScrobbleInfo.SongTitle,
                databaseSongScrobbleInfo.ArtistName,
                false,
                databaseSongScrobbleInfo.AlbumTitle,
                databaseSongScrobbleInfo.SongDuration.ToSeconds(),
                databaseSongScrobbleInfo.SongMusicBrainzId,
                databaseSongScrobbleInfo.SongNumber,
                null,
                Instant.FromDateTimeUtc(DateTime.UtcNow),
                playerName,
                userAgent,
                ipAddress,
                secondsPlayed
            )
            {
                LastScrobbledAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };

            var enabledScrobblers = _scrobblers.OrderBy(x => x.SortOrder).Where(x => x.IsEnabled).ToArray();
                Logger.Information("[{ServiceName}] Processing NowPlaying with configured scrobblers.",
                    nameof(ScrobbleService));

            foreach (var scrobbler in enabledScrobblers)
            {
                Logger.Information("[{ServiceName}] Calling NowPlaying on scrobbler.", nameof(ScrobbleService));
                var nowPlayingResult = await scrobbler.NowPlaying(user, scrobble, cancellationToken).ConfigureAwait(false);
                result &= nowPlayingResult.IsSuccess;
                Logger.Information("[{ServiceName}] Scrobbler NowPlaying completed.", nameof(ScrobbleService));
            }
        }
        else
        {
            Logger.Warning("[{ServiceName}] Could not find song info for request.",
                nameof(ScrobbleService));
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> Scrobble(UserInfo user, Guid songId, bool isRandomizedScrobble, string playerName, string? userAgent = null, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        Logger.Information("[{ServiceName}] Scrobble invoked.", nameof(ScrobbleService));

        CheckInitialized();

        var result = true;

        var songIds = await DatabaseSongIdsInfoForSongApiKey(songId, cancellationToken).ConfigureAwait(false);
        if (songIds != null)
        {
            var databaseSongScrobbleInfo = await DatabaseSongScrobbleInfoForSongApiKey(songId, cancellationToken).ConfigureAwait(false);
            if (databaseSongScrobbleInfo != null)
            {
                Logger.Information("[{ServiceName}] Found song info for scrobble.",
                    nameof(ScrobbleService));

                // For completed scrobbles, use the full song duration
                var secondsPlayed = databaseSongScrobbleInfo.SongDuration.ToSeconds();

                var scrobble = new ScrobbleInfo
                (
                    databaseSongScrobbleInfo.SongApiKey,
                    databaseSongScrobbleInfo.ArtistId,
                    databaseSongScrobbleInfo.AlbumId,
                    databaseSongScrobbleInfo.SongId,
                    databaseSongScrobbleInfo.SongTitle,
                    databaseSongScrobbleInfo.ArtistName,
                    isRandomizedScrobble,
                    databaseSongScrobbleInfo.AlbumTitle,
                    databaseSongScrobbleInfo.SongDuration.ToSeconds(),
                    databaseSongScrobbleInfo.SongMusicBrainzId,
                    databaseSongScrobbleInfo.SongNumber,
                    null,
                    Instant.FromDateTimeUtc(DateTime.UtcNow),
                    playerName,
                    userAgent,
                    ipAddress,
                    secondsPlayed
                )
                {
                    LastScrobbledAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
                };

                var enabledScrobblers = _scrobblers.OrderBy(x => x.SortOrder).Where(x => x.IsEnabled).ToArray();
                Logger.Information("[{ServiceName}] Processing Scrobble with [{Count}] enabled scrobblers",
                    nameof(ScrobbleService),
                    enabledScrobblers.Length);

                foreach (var scrobbler in enabledScrobblers)
                {
                    try
                    {
                        Logger.Information("[{ServiceName}] Calling Scrobble on scrobbler [{Scrobbler}]",
                            nameof(ScrobbleService),
                            scrobbler.DisplayName);
                        var scrobbleResult = await scrobbler.Scrobble(user, scrobble, cancellationToken).ConfigureAwait(false);
                        result &= scrobbleResult.IsSuccess;
                        Logger.Information("[{ServiceName}] Scrobbler [{Scrobbler}] Scrobble result: [{Success}]",
                            nameof(ScrobbleService),
                            scrobbler.DisplayName,
                            scrobbleResult.IsSuccess);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "[{Plugin}] threw error with song [{Song}]", scrobbler.DisplayName, songId);
                        result = false;
                        break;
                    }
                }

                Logger.Information("[{ServiceName}] Scrobbled song [{Song}] for User [{User}]",
                    nameof(ScrobbleService),
                    songId.ToString(),
                    user.ToString());
            }
            else
            {
                Logger.Warning("[{ServiceName}] Could not find song scrobble info for Song [{SongId}]",
                    nameof(ScrobbleService),
                    songId);
            }
        }
        else
        {
            Logger.Warning("[{ServiceName}] Could not find song IDs info for Song [{SongId}]",
                nameof(ScrobbleService),
                songId);
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }
}

using Hqub.Lastfm;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Scrobble = Hqub.Lastfm.Entities.Scrobble;

namespace Melodee.Common.Plugins.Scrobbling;

public class LastFmScrobbler(
    IMelodeeConfiguration configuration,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ILogger logger) : IScrobbler
{
    public bool StopProcessing { get; } = false;
    public string Id => "7ADF8A80-433C-487A-8359-20FD473F20BB";

    public string DisplayName => nameof(LastFmScrobbler);

    public bool IsEnabled { get; set; } = false;

    public int SortOrder { get; } = 1;

    public async Task<OperationResult<bool>> NowPlaying(UserInfo user, ScrobbleInfo scrobble,
        CancellationToken token = default)
    {
        var apiKey = configuration.GetValue<string>(SettingRegistry.ScrobblingLastFmApiKey);
        var secret = configuration.GetValue<string>(SettingRegistry.ScrobblingLastFmSharedSecret);
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(secret))
        {
            return new OperationResult<bool>("Last.fm scrobbling not configured.") { Data = true };
        }

        var dbUser = await GetUserAsync(user.Id, token).ConfigureAwait(false);
        if (dbUser?.LastFmSessionKey.Nullify() == null)
        {
            logger.Debug("[{Plugin}] user [{User}] missing Last.fm session key", DisplayName, user.Id);
            return new OperationResult<bool>("Last.fm not linked.") { Data = true };
        }

        var result = true;
        try
        {
            var client = new LastfmClient(apiKey, secret);
            client.Session.SessionKey = dbUser.LastFmSessionKey;
            result = await client.Track.UpdateNowPlayingAsync(
                scrobble.SongTitle,
                scrobble.SongArtist,
                scrobble.SongNumber ?? 0,
                scrobble.AlbumTitle,
                scrobble.ArtistName).ConfigureAwait(false);
        }
        catch (Hqub.Lastfm.NotAuthenticatedException)
        {
            await ClearSessionKeyAsync(user.Id, token).ConfigureAwait(false);
            result = false;
        }
        catch (Exception e)
        {
            logger.Error(e, "Attempted now playing for user [{User}]", user.ToString());
            result = false;
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> Scrobble(UserInfo user, ScrobbleInfo scrobble,
        CancellationToken token = default)
    {
        var apiKey = configuration.GetValue<string>(SettingRegistry.ScrobblingLastFmApiKey);
        var secret = configuration.GetValue<string>(SettingRegistry.ScrobblingLastFmSharedSecret);
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(secret))
        {
            return new OperationResult<bool>("Last.fm scrobbling not configured.") { Data = true };
        }

        var dbUser = await GetUserAsync(user.Id, token).ConfigureAwait(false);
        if (dbUser?.LastFmSessionKey.Nullify() == null)
        {
            logger.Debug("[{Plugin}] user [{User}] missing Last.fm session key", DisplayName, user.Id);
            return new OperationResult<bool>("Last.fm not linked.") { Data = true };
        }

        var result = true;
        try
        {
            var client = new LastfmClient(apiKey, secret);
            client.Session.SessionKey = dbUser.LastFmSessionKey;

            var scrobbleResult = await client.Track.ScrobbleAsync([
                new Scrobble
                {
                    Track = scrobble.SongTitle,
                    Artist = scrobble.SongArtist,
                    Date = DateTime.UtcNow,
                    Album = scrobble.AlbumTitle,
                    AlbumArtist = scrobble.ArtistName,
                    MBID = scrobble.SongMusicBrainzId?.ToString(),
                    Duration = SafeParser.ToNumber<int>(scrobble.SongDuration),
                    TrackNumber = scrobble.SongNumber ?? 0,
                    ChosenByUser = !scrobble.IsRandomizedScrobble
                }
            ]).ConfigureAwait(false);
            result = scrobbleResult.Accepted > 0;
        }
        catch (Hqub.Lastfm.NotAuthenticatedException)
        {
            await ClearSessionKeyAsync(user.Id, token).ConfigureAwait(false);
            result = false;
        }
        catch (Exception e)
        {
            logger.Error(e, "Attempted scrobbling user [{User}]", user.ToString());
            result = false;
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    private async Task<Data.Models.User?> GetUserAsync(int userId, CancellationToken token)
    {
        await using var scopedContext = await contextFactory.CreateDbContextAsync(token).ConfigureAwait(false);
        return await scopedContext.Users.FirstOrDefaultAsync(x => x.Id == userId, token).ConfigureAwait(false);
    }

    private async Task ClearSessionKeyAsync(int userId, CancellationToken token)
    {
        await using var scopedContext = await contextFactory.CreateDbContextAsync(token).ConfigureAwait(false);
        var user = await scopedContext.Users.FirstOrDefaultAsync(x => x.Id == userId, token).ConfigureAwait(false);
        if (user != null)
        {
            user.LastFmSessionKey = null;
            user.LastUpdatedAt = NodaTime.Instant.FromDateTimeUtc(DateTime.UtcNow);
            await scopedContext.SaveChangesAsync(token).ConfigureAwait(false);
            logger.Warning("[{Plugin}] cleared Last.fm session key for user [{User}]", DisplayName, userId);
        }
    }
}

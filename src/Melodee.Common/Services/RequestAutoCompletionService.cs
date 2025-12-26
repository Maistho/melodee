using Ardalis.GuardClauses;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services;

public class RequestAutoCompletionService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public async Task<int> ProcessAlbumAddedAsync(
        Album album,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(album, nameof(album));

        var completedCount = 0;

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var openRequests = await scopedContext.Requests
            .Where(r => r.Status == (int)RequestStatus.Pending || r.Status == (int)RequestStatus.InProgress)
            .Where(r => r.Category == (int)RequestCategory.AddAlbum)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!openRequests.Any())
        {
            return 0;
        }

        var albumArtist = await scopedContext.Artists
            .FirstOrDefaultAsync(a => a.Id == album.ArtistId, cancellationToken)
            .ConfigureAwait(false);

        if (albumArtist == null)
        {
            return 0;
        }

        var albumNameNormalized = album.NameNormalized;
        var artistNameNormalized = albumArtist.NameNormalized;
        var albumYear = album.ReleaseDate.Year;

        foreach (var request in openRequests)
        {
            var isMatch = false;

            if (request.TargetArtistApiKey.HasValue && request.TargetArtistApiKey.Value == albumArtist.ApiKey)
            {
                isMatch = true;
            }
            else if (!string.IsNullOrWhiteSpace(request.ArtistNameNormalized) &&
                     !string.IsNullOrWhiteSpace(request.AlbumTitleNormalized))
            {
                if (request.ArtistNameNormalized == artistNameNormalized &&
                    request.AlbumTitleNormalized == albumNameNormalized)
                {
                    isMatch = true;
                }
            }

            if (isMatch && request.ReleaseYear.HasValue && request.ReleaseYear.Value != albumYear)
            {
                isMatch = false;
            }

            if (isMatch)
            {
                var now = SystemClock.Instance.GetCurrentInstant();
                request.Status = (int)RequestStatus.Completed;
                request.LastActivityAt = now;
                request.LastActivityUserId = null;
                request.LastActivityType = (int)RequestActivityType.SystemComment;
                request.UpdatedAt = now;
                request.UpdatedByUserId = request.CreatedByUserId;

                var commentBody = $"✅ **Request automatically completed!**\n\n" +
                                  $"The album **{album.Name}** by **{albumArtist.Name}** has been added to the library.\n\n" +
                                  $"[View Album](/data/album/{album.ApiKey})";

                var systemComment = new RequestComment
                {
                    ApiKey = Guid.NewGuid(),
                    RequestId = request.Id,
                    ParentCommentId = null,
                    Body = commentBody,
                    IsSystem = true,
                    CreatedAt = now,
                    CreatedByUserId = null
                };

                scopedContext.RequestComments.Add(systemComment);

                await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                Logger.Information(
                    "[{ServiceName}] Auto-completed request [{RequestId}] for album [{AlbumName}] by [{ArtistName}]",
                    nameof(RequestAutoCompletionService),
                    request.Id,
                    album.Name,
                    albumArtist.Name);

                completedCount++;
            }
        }

        return completedCount;
    }

    public async Task<int> ProcessSongAddedAsync(
        Song song,
        Album album,
        Artist artist,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(song, nameof(song));
        Guard.Against.Null(album, nameof(album));
        Guard.Against.Null(artist, nameof(artist));

        var completedCount = 0;

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var openRequests = await scopedContext.Requests
            .Where(r => r.Status == (int)RequestStatus.Pending || r.Status == (int)RequestStatus.InProgress)
            .Where(r => r.Category == (int)RequestCategory.AddSong)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!openRequests.Any())
        {
            return 0;
        }

        var songTitleNormalized = song.TitleNormalized;
        var artistNameNormalized = artist.NameNormalized;
        var albumYear = album.ReleaseDate.Year;

        foreach (var request in openRequests)
        {
            var isMatch = false;

            if (request.TargetArtistApiKey.HasValue && request.TargetArtistApiKey.Value == artist.ApiKey)
            {
                isMatch = true;
            }
            else if (!string.IsNullOrWhiteSpace(request.ArtistNameNormalized) &&
                     !string.IsNullOrWhiteSpace(request.SongTitleNormalized))
            {
                if (request.ArtistNameNormalized == artistNameNormalized &&
                    request.SongTitleNormalized == songTitleNormalized)
                {
                    isMatch = true;
                }
            }

            if (isMatch && request.ReleaseYear.HasValue && request.ReleaseYear.Value != albumYear)
            {
                isMatch = false;
            }

            if (isMatch)
            {
                var now = SystemClock.Instance.GetCurrentInstant();
                request.Status = (int)RequestStatus.Completed;
                request.LastActivityAt = now;
                request.LastActivityUserId = null;
                request.LastActivityType = (int)RequestActivityType.SystemComment;
                request.UpdatedAt = now;
                request.UpdatedByUserId = request.CreatedByUserId;

                var commentBody = $"✅ **Request automatically completed!**\n\n" +
                                  $"The song **{song.Title}** by **{artist.Name}** has been added to the library.\n\n" +
                                  $"[View Song](/data/song/{song.ApiKey})";

                var systemComment = new RequestComment
                {
                    ApiKey = Guid.NewGuid(),
                    RequestId = request.Id,
                    ParentCommentId = null,
                    Body = commentBody,
                    IsSystem = true,
                    CreatedAt = now,
                    CreatedByUserId = null
                };

                scopedContext.RequestComments.Add(systemComment);

                await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                Logger.Information(
                    "[{ServiceName}] Auto-completed request [{RequestId}] for song [{SongName}] by [{ArtistName}]",
                    nameof(RequestAutoCompletionService),
                    request.Id,
                    song.Title,
                    artist.Name);

                completedCount++;
            }
        }

        return completedCount;
    }
}

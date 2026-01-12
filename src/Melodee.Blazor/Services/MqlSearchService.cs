using System.Linq.Expressions;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Mql;
using Melodee.Mql.Interfaces;
using Melodee.Mql.Models;
using Microsoft.EntityFrameworkCore;
using Album = Melodee.Common.Data.Models.Album;
using Artist = Melodee.Common.Data.Models.Artist;
using Song = Melodee.Common.Data.Models.Song;

namespace Melodee.Blazor.Services;

public sealed record MqlSearchResult<T> where T : notnull
{
    public required bool IsValid { get; init; }
    public required List<string> Errors { get; init; }
    public required List<string> Warnings { get; init; }
    public required PagedResult<T> Results { get; init; }
}

public sealed record MqlSearchAllResult
{
    public required bool IsValid { get; init; }
    public required List<string> Errors { get; init; }
    public required List<string> Warnings { get; init; }
    public required PagedResult<Song> Songs { get; init; }
    public required PagedResult<Album> Albums { get; init; }
    public required PagedResult<Artist> Artists { get; init; }
    public required PagedResult<Melodee.Common.Data.Models.PodcastEpisode> PodcastEpisodes { get; init; }
}

public interface IMqlSearchService
{
    Task<MqlSearchResult<Song>> SearchSongsAsync(string mqlQuery, int userId, PagedRequest paging, CancellationToken cancellationToken = default);
    Task<MqlSearchResult<Album>> SearchAlbumsAsync(string mqlQuery, int userId, PagedRequest paging, CancellationToken cancellationToken = default);
    Task<MqlSearchResult<Artist>> SearchArtistsAsync(string mqlQuery, int userId, PagedRequest paging, CancellationToken cancellationToken = default);
    Task<MqlSearchResult<Melodee.Common.Data.Models.PodcastEpisode>> SearchPodcastsAsync(string mqlQuery, int userId, PagedRequest paging, CancellationToken cancellationToken = default);
    Task<MqlSearchAllResult> SearchAllAsync(string mqlQuery, int userId, PagedRequest paging, CancellationToken cancellationToken = default);
    MqlValidationResult ValidateQuery(string mqlQuery, string entityType);
}

public sealed class MqlSearchService(
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMqlValidator validator,
    IMelodeeConfigurationFactory configurationFactory) : IMqlSearchService
{
    public MqlValidationResult ValidateQuery(string mqlQuery, string entityType)
    {
        return validator.Validate(mqlQuery, entityType);
    }

    public async Task<MqlSearchResult<Song>> SearchSongsAsync(string mqlQuery, int userId, PagedRequest paging, CancellationToken cancellationToken = default)
    {
        var trimmedQuery = mqlQuery?.Trim() ?? string.Empty;
        var validationResult = validator.Validate(trimmedQuery, "songs");
        if (!validationResult.IsValid)
        {
            return new MqlSearchResult<Song>
            {
                IsValid = false,
                Errors = validationResult.Errors.Select(e => e.Message).ToList(),
                Warnings = validationResult.Warnings,
                Results = new PagedResult<Song> { TotalCount = 0, TotalPages = 0, Data = [] }
            };
        }

        var tokenizer = new MqlTokenizer();
        var tokens = tokenizer.Tokenize(trimmedQuery).ToList();

        var parser = new MqlParser();
        var parseResult = parser.Parse(tokens, "songs");

        if (!parseResult.IsValid || parseResult.Ast == null)
        {
            return new MqlSearchResult<Song>
            {
                IsValid = false,
                Errors = parseResult.Errors.Select(e => e.Message).ToList(),
                Warnings = [],
                Results = new PagedResult<Song> { TotalCount = 0, TotalPages = 0, Data = [] }
            };
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var baseQuery = context.Songs
            .Include(s => s.Album)
            .ThenInclude(a => a.Artist)
            .Include(s => s.UserSongs.Where(us => us.UserId == userId))
            .AsNoTracking();

        var compiler = new MqlSongCompiler();
        Expression<Func<Song, bool>> predicate;
        try
        {
            predicate = compiler.Compile(parseResult.Ast, userId);
        }
        catch (Exception ex)
        {
            return new MqlSearchResult<Song>
            {
                IsValid = false,
                Errors = [$"Compilation error: {ex.Message}"],
                Warnings = [],
                Results = new PagedResult<Song> { TotalCount = 0, TotalPages = 0, Data = [] }
            };
        }

        var filteredQuery = baseQuery.Where(predicate);
        var totalCount = await filteredQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        var songs = await filteredQuery
            .OrderBy(s => s.Album.Artist.Name)
            .ThenBy(s => s.Album.Name)
            .ThenBy(s => s.SongNumber)
            .Skip(paging.SkipValue)
            .Take(paging.TakeValue)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new MqlSearchResult<Song>
        {
            IsValid = true,
            Errors = [],
            Warnings = validationResult.Warnings,
            Results = new PagedResult<Song>
            {
                TotalCount = totalCount,
                TotalPages = paging.TotalPages(totalCount),
                Data = songs
            }
        };
    }

    public async Task<MqlSearchResult<Album>> SearchAlbumsAsync(string mqlQuery, int userId, PagedRequest paging, CancellationToken cancellationToken = default)
    {
        var trimmedQuery = mqlQuery?.Trim() ?? string.Empty;
        var validationResult = validator.Validate(trimmedQuery, "albums");
        if (!validationResult.IsValid)
        {
            return new MqlSearchResult<Album>
            {
                IsValid = false,
                Errors = validationResult.Errors.Select(e => e.Message).ToList(),
                Warnings = validationResult.Warnings,
                Results = new PagedResult<Album> { TotalCount = 0, TotalPages = 0, Data = [] }
            };
        }

        var tokenizer = new MqlTokenizer();
        var tokens = tokenizer.Tokenize(trimmedQuery).ToList();

        var parser = new MqlParser();
        var parseResult = parser.Parse(tokens, "albums");

        if (!parseResult.IsValid || parseResult.Ast == null)
        {
            return new MqlSearchResult<Album>
            {
                IsValid = false,
                Errors = parseResult.Errors.Select(e => e.Message).ToList(),
                Warnings = [],
                Results = new PagedResult<Album> { TotalCount = 0, TotalPages = 0, Data = [] }
            };
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var baseQuery = context.Albums
            .Include(a => a.Artist)
            .Include(a => a.UserAlbums.Where(ua => ua.UserId == userId))
            .AsNoTracking();

        var compiler = new MqlAlbumCompiler();
        Expression<Func<Album, bool>> predicate;
        try
        {
            predicate = compiler.Compile(parseResult.Ast, userId);
        }
        catch (Exception ex)
        {
            return new MqlSearchResult<Album>
            {
                IsValid = false,
                Errors = [$"Compilation error: {ex.Message}"],
                Warnings = [],
                Results = new PagedResult<Album> { TotalCount = 0, TotalPages = 0, Data = [] }
            };
        }

        var filteredQuery = baseQuery.Where(predicate);
        var totalCount = await filteredQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        var albums = await filteredQuery
            .OrderBy(a => a.Artist.Name)
            .ThenBy(a => a.ReleaseDate)
            .ThenBy(a => a.Name)
            .Skip(paging.SkipValue)
            .Take(paging.TakeValue)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new MqlSearchResult<Album>
        {
            IsValid = true,
            Errors = [],
            Warnings = validationResult.Warnings,
            Results = new PagedResult<Album>
            {
                TotalCount = totalCount,
                TotalPages = paging.TotalPages(totalCount),
                Data = albums
            }
        };
    }

    public async Task<MqlSearchResult<Artist>> SearchArtistsAsync(string mqlQuery, int userId, PagedRequest paging, CancellationToken cancellationToken = default)
    {
        var trimmedQuery = mqlQuery?.Trim() ?? string.Empty;
        var validationResult = validator.Validate(trimmedQuery, "artists");
        if (!validationResult.IsValid)
        {
            return new MqlSearchResult<Artist>
            {
                IsValid = false,
                Errors = validationResult.Errors.Select(e => e.Message).ToList(),
                Warnings = validationResult.Warnings,
                Results = new PagedResult<Artist> { TotalCount = 0, TotalPages = 0, Data = [] }
            };
        }

        var tokenizer = new MqlTokenizer();
        var tokens = tokenizer.Tokenize(trimmedQuery).ToList();

        var parser = new MqlParser();
        var parseResult = parser.Parse(tokens, "artists");

        if (!parseResult.IsValid || parseResult.Ast == null)
        {
            return new MqlSearchResult<Artist>
            {
                IsValid = false,
                Errors = parseResult.Errors.Select(e => e.Message).ToList(),
                Warnings = [],
                Results = new PagedResult<Artist> { TotalCount = 0, TotalPages = 0, Data = [] }
            };
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var baseQuery = context.Artists
            .Include(a => a.UserArtists.Where(ua => ua.UserId == userId))
            .AsNoTracking();

        var compiler = new MqlArtistCompiler();
        Expression<Func<Artist, bool>> predicate;
        try
        {
            predicate = compiler.Compile(parseResult.Ast, userId);
        }
        catch (Exception ex)
        {
            return new MqlSearchResult<Artist>
            {
                IsValid = false,
                Errors = [$"Compilation error: {ex.Message}"],
                Warnings = [],
                Results = new PagedResult<Artist> { TotalCount = 0, TotalPages = 0, Data = [] }
            };
        }

        var filteredQuery = baseQuery.Where(predicate);
        var totalCount = await filteredQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        var artists = await filteredQuery
            .OrderBy(a => a.Name)
            .Skip(paging.SkipValue)
            .Take(paging.TakeValue)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new MqlSearchResult<Artist>
        {
            IsValid = true,
            Errors = [],
            Warnings = validationResult.Warnings,
            Results = new PagedResult<Artist>
            {
                TotalCount = totalCount,
                TotalPages = paging.TotalPages(totalCount),
                Data = artists
            }
        };
    }

    public async Task<MqlSearchResult<Melodee.Common.Data.Models.PodcastEpisode>> SearchPodcastsAsync(string mqlQuery, int userId, PagedRequest paging, CancellationToken cancellationToken = default)
    {
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        if (!configuration.GetValue<bool>(SettingRegistry.PodcastEnabled))
        {
            return new MqlSearchResult<Melodee.Common.Data.Models.PodcastEpisode>
            {
                IsValid = true,
                Errors = [],
                Warnings = [],
                Results = new PagedResult<Melodee.Common.Data.Models.PodcastEpisode> { TotalCount = 0, TotalPages = 0, Data = [] }
            };
        }

        var trimmedQuery = mqlQuery?.Trim() ?? string.Empty;
        // Skip validation/search if entity type is not 'podcasts' or 'all' but we are calling directly.
        // Assuming caller handles entity selection logic or we rely on validation.
        var validationResult = validator.Validate(trimmedQuery, "podcasts");
        if (!validationResult.IsValid)
        {
            return new MqlSearchResult<Melodee.Common.Data.Models.PodcastEpisode>
            {
                IsValid = false,
                Errors = validationResult.Errors.Select(e => e.Message).ToList(),
                Warnings = validationResult.Warnings,
                Results = new PagedResult<Melodee.Common.Data.Models.PodcastEpisode> { TotalCount = 0, TotalPages = 0, Data = [] }
            };
        }

        var tokenizer = new MqlTokenizer();
        var tokens = tokenizer.Tokenize(trimmedQuery).ToList();

        var parser = new MqlParser();
        var parseResult = parser.Parse(tokens, "podcasts");

        if (!parseResult.IsValid || parseResult.Ast == null)
        {
            return new MqlSearchResult<Melodee.Common.Data.Models.PodcastEpisode>
            {
                IsValid = false,
                Errors = parseResult.Errors.Select(e => e.Message).ToList(),
                Warnings = [],
                Results = new PagedResult<Melodee.Common.Data.Models.PodcastEpisode> { TotalCount = 0, TotalPages = 0, Data = [] }
            };
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var baseQuery = context.PodcastEpisodes
            .Include(x => x.PodcastChannel)
            .Where(x => x.PodcastChannel != null && x.PodcastChannel.UserId == userId && !x.PodcastChannel.IsDeleted)
            .AsNoTracking();

        var compiler = new MqlPodcastEpisodeCompiler();
        Expression<Func<Melodee.Common.Data.Models.PodcastEpisode, bool>> predicate;
        try
        {
            predicate = compiler.Compile(parseResult.Ast, userId);
        }
        catch (Exception ex)
        {
            return new MqlSearchResult<Melodee.Common.Data.Models.PodcastEpisode>
            {
                IsValid = false,
                Errors = [$"Compilation error: {ex.Message}"],
                Warnings = [],
                Results = new PagedResult<Melodee.Common.Data.Models.PodcastEpisode> { TotalCount = 0, TotalPages = 0, Data = [] }
            };
        }

        var filteredQuery = baseQuery.Where(predicate);
        var totalCount = await filteredQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        var episodes = await filteredQuery
            .OrderByDescending(x => x.PublishDate)
            .Skip(paging.SkipValue)
            .Take(paging.TakeValue)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new MqlSearchResult<Melodee.Common.Data.Models.PodcastEpisode>
        {
            IsValid = true,
            Errors = [],
            Warnings = validationResult.Warnings,
            Results = new PagedResult<Melodee.Common.Data.Models.PodcastEpisode>
            {
                TotalCount = totalCount,
                TotalPages = paging.TotalPages(totalCount),
                Data = episodes
            }
        };
    }

    public async Task<MqlSearchAllResult> SearchAllAsync(string mqlQuery, int userId, PagedRequest paging, CancellationToken cancellationToken = default)
    {
        var songTask = SearchSongsAsync(mqlQuery, userId, paging, cancellationToken);
        var albumTask = SearchAlbumsAsync(mqlQuery, userId, paging, cancellationToken);
        var artistTask = SearchArtistsAsync(mqlQuery, userId, paging, cancellationToken);
        var podcastTask = SearchPodcastsAsync(mqlQuery, userId, paging, cancellationToken);

        await Task.WhenAll(songTask, albumTask, artistTask, podcastTask).ConfigureAwait(false);

        var songResult = await songTask;
        var albumResult = await albumTask;
        var artistResult = await artistTask;
        var podcastResult = await podcastTask;

        var errors = new List<string>();
        var warnings = new List<string>();

        if (!songResult.IsValid) errors.AddRange(songResult.Errors.Select(e => $"Songs: {e}"));
        if (!albumResult.IsValid) errors.AddRange(albumResult.Errors.Select(e => $"Albums: {e}"));
        if (!artistResult.IsValid) errors.AddRange(artistResult.Errors.Select(e => $"Artists: {e}"));
        if (!podcastResult.IsValid) errors.AddRange(podcastResult.Errors.Select(e => $"Podcasts: {e}"));

        warnings.AddRange(songResult.Warnings);
        warnings.AddRange(albumResult.Warnings);
        warnings.AddRange(artistResult.Warnings);
        warnings.AddRange(podcastResult.Warnings);

        return new MqlSearchAllResult
        {
            IsValid = songResult.IsValid || albumResult.IsValid || artistResult.IsValid || podcastResult.IsValid,
            Errors = errors,
            Warnings = warnings.Distinct().ToList(),
            Songs = songResult.Results,
            Albums = albumResult.Results,
            Artists = artistResult.Results,
            PodcastEpisodes = podcastResult.Results
        };
    }
}

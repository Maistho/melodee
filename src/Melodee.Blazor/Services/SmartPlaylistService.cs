using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Blazor.Services;

public sealed record SmartPlaylistDto
{
    public required Guid ApiKey { get; init; }
    public required string Name { get; init; }
    public required string MqlQuery { get; init; }
    public required string EntityType { get; init; }
    public int LastResultCount { get; init; }
    public Instant? LastEvaluatedAt { get; init; }
    public bool IsPublic { get; init; }
    public string? NormalizedQuery { get; init; }
    public Instant CreatedAt { get; init; }
}

public sealed record CreateSmartPlaylistRequest
{
    public required string Name { get; init; }
    public required string MqlQuery { get; init; }
    public required string EntityType { get; init; }
    public bool IsPublic { get; init; }
}

public sealed record UpdateSmartPlaylistRequest
{
    public string? Name { get; init; }
    public string? MqlQuery { get; init; }
    public string? EntityType { get; init; }
    public bool? IsPublic { get; init; }
}

public interface ISmartPlaylistService
{
    Task<OperationResult<SmartPlaylistDto>> CreateAsync(CreateSmartPlaylistRequest request, int userId, CancellationToken cancellationToken = default);
    Task<OperationResult<SmartPlaylistDto>> GetByApiKeyAsync(Guid apiKey, int userId, CancellationToken cancellationToken = default);
    Task<PagedResult<SmartPlaylistDto>> ListByUserAsync(int userId, PagedRequest paging, CancellationToken cancellationToken = default);
    Task<OperationResult<SmartPlaylistDto>> UpdateAsync(Guid playlistApiKey, UpdateSmartPlaylistRequest request, int userId, CancellationToken cancellationToken = default);
    Task<OperationResult<bool>> DeleteAsync(Guid playlistApiKey, int userId, CancellationToken cancellationToken = default);
    Task<PagedResult<dynamic>> EvaluateAsync(Guid playlistApiKey, PagedRequest paging, int userId, CancellationToken cancellationToken = default);
}

public sealed class SmartPlaylistService(
        Serilog.ILogger logger,
        IDbContextFactory<MelodeeDbContext> contextFactory)
    : ISmartPlaylistService
{
    private static readonly string[] ValidEntityTypes = ["songs", "albums", "artists"];

    public async Task<OperationResult<SmartPlaylistDto>> CreateAsync(CreateSmartPlaylistRequest request, int userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new OperationResult<SmartPlaylistDto>(OperationResponseType.ValidationFailure, "Name is required.")
            {
                Data = new SmartPlaylistDto { Name = string.Empty, MqlQuery = string.Empty, EntityType = string.Empty, ApiKey = Guid.Empty, CreatedAt = Instant.MinValue }
            };
        }

        if (string.IsNullOrWhiteSpace(request.MqlQuery))
        {
            return new OperationResult<SmartPlaylistDto>(OperationResponseType.ValidationFailure, "MQL query is required.")
            {
                Data = new SmartPlaylistDto { Name = string.Empty, MqlQuery = string.Empty, EntityType = string.Empty, ApiKey = Guid.Empty, CreatedAt = Instant.MinValue }
            };
        }

        if (string.IsNullOrWhiteSpace(request.EntityType))
        {
            return new OperationResult<SmartPlaylistDto>(OperationResponseType.ValidationFailure, "Entity type is required.")
            {
                Data = new SmartPlaylistDto { Name = string.Empty, MqlQuery = string.Empty, EntityType = string.Empty, ApiKey = Guid.Empty, CreatedAt = Instant.MinValue }
            };
        }

        if (!ValidEntityTypes.Contains(request.EntityType.ToLowerInvariant()))
        {
            return new OperationResult<SmartPlaylistDto>(OperationResponseType.ValidationFailure, $"Invalid entity type '{request.EntityType}'. Must be one of: {string.Join(", ", ValidEntityTypes)}")
            {
                Data = new SmartPlaylistDto { Name = string.Empty, MqlQuery = string.Empty, EntityType = string.Empty, ApiKey = Guid.Empty, CreatedAt = Instant.MinValue }
            };
        }

        await using var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var existingPlaylist = await scopedContext.SmartPlaylists
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Name == request.Name, cancellationToken)
            .ConfigureAwait(false);

        if (existingPlaylist != null)
        {
            return new OperationResult<SmartPlaylistDto>(OperationResponseType.ValidationFailure, $"A smart playlist with name '{request.Name}' already exists.")
            {
                Data = new SmartPlaylistDto { Name = string.Empty, MqlQuery = string.Empty, EntityType = string.Empty, ApiKey = Guid.Empty, CreatedAt = Instant.MinValue }
            };
        }

        var playlist = new SmartPlaylist
        {
            UserId = userId,
            Name = request.Name,
            MqlQuery = request.MqlQuery,
            EntityType = request.EntityType.ToLowerInvariant(),
            IsPublic = request.IsPublic,
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            LastResultCount = 0
        };

        scopedContext.SmartPlaylists.Add(playlist);
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.Information("[SmartPlaylistService] Created smart playlist {Name} for user {UserId}", playlist.Name, userId);

        return new OperationResult<SmartPlaylistDto> { Data = MapToDto(playlist), Type = OperationResponseType.Ok };
    }

    public async Task<OperationResult<SmartPlaylistDto>> GetByApiKeyAsync(Guid apiKey, int userId, CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var playlist = await scopedContext.SmartPlaylists
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ApiKey == apiKey, cancellationToken)
            .ConfigureAwait(false);

        if (playlist == null)
        {
            return new OperationResult<SmartPlaylistDto>(OperationResponseType.NotFound, "Smart playlist not found.")
            {
                Data = new SmartPlaylistDto { Name = string.Empty, MqlQuery = string.Empty, EntityType = string.Empty, ApiKey = Guid.Empty, CreatedAt = Instant.MinValue }
            };
        }

        if (playlist.UserId != userId && !playlist.IsPublic)
        {
            return new OperationResult<SmartPlaylistDto>(OperationResponseType.AccessDenied, "You do not have access to this smart playlist.")
            {
                Data = new SmartPlaylistDto { Name = string.Empty, MqlQuery = string.Empty, EntityType = string.Empty, ApiKey = Guid.Empty, CreatedAt = Instant.MinValue }
            };
        }

        return new OperationResult<SmartPlaylistDto> { Data = MapToDto(playlist), Type = OperationResponseType.Ok };
    }

    public async Task<PagedResult<SmartPlaylistDto>> ListByUserAsync(int userId, PagedRequest paging, CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var query = scopedContext.SmartPlaylists
            .AsNoTracking()
            .Where(p => p.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var orderByClause = paging.OrderByValue("CreatedAt", PagedRequest.OrderDescDirection);
        var isDescending = orderByClause.Contains("DESC", StringComparison.OrdinalIgnoreCase);
        var fieldName = orderByClause.Split(' ')[0].Trim('"').ToLowerInvariant();

        query = fieldName switch
        {
            "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "createdat" => isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            "lastresultcount" => isDescending ? query.OrderByDescending(p => p.LastResultCount) : query.OrderBy(p => p.LastResultCount),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var playlists = await query
            .Skip(paging.SkipValue)
            .Take(paging.TakeValue)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<SmartPlaylistDto>
        {
            TotalCount = totalCount,
            TotalPages = paging.TotalPages(totalCount),
            Data = playlists.Select(MapToDto).ToArray()
        };
    }

    public async Task<OperationResult<SmartPlaylistDto>> UpdateAsync(Guid playlistApiKey, UpdateSmartPlaylistRequest request, int userId, CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var playlist = await scopedContext.SmartPlaylists
            .FirstOrDefaultAsync(p => p.ApiKey == playlistApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (playlist == null)
        {
            return new OperationResult<SmartPlaylistDto>(OperationResponseType.NotFound, "Smart playlist not found.")
            {
                Data = new SmartPlaylistDto { Name = string.Empty, MqlQuery = string.Empty, EntityType = string.Empty, ApiKey = Guid.Empty, CreatedAt = Instant.MinValue }
            };
        }

        if (playlist.UserId != userId)
        {
            return new OperationResult<SmartPlaylistDto>(OperationResponseType.AccessDenied, "You do not have permission to update this smart playlist.")
            {
                Data = new SmartPlaylistDto { Name = string.Empty, MqlQuery = string.Empty, EntityType = string.Empty, ApiKey = Guid.Empty, CreatedAt = Instant.MinValue }
            };
        }

        var hasChanges = false;

        if (request.Name != null && request.Name != playlist.Name)
        {
            var existing = await scopedContext.SmartPlaylists
                .AnyAsync(p => p.UserId == userId && p.Name == request.Name && p.ApiKey != playlistApiKey, cancellationToken)
                .ConfigureAwait(false);

            if (existing)
            {
                return new OperationResult<SmartPlaylistDto>(OperationResponseType.ValidationFailure, $"A smart playlist with name '{request.Name}' already exists.")
                {
                    Data = new SmartPlaylistDto { Name = string.Empty, MqlQuery = string.Empty, EntityType = string.Empty, ApiKey = Guid.Empty, CreatedAt = Instant.MinValue }
                };
            }

            playlist.Name = request.Name;
            hasChanges = true;
        }

        if (request.MqlQuery != null && request.MqlQuery != playlist.MqlQuery)
        {
            playlist.MqlQuery = request.MqlQuery;
            playlist.LastResultCount = 0;
            playlist.LastEvaluatedAt = null;
            hasChanges = true;
        }

        if (request.EntityType != null && request.EntityType != playlist.EntityType)
        {
            if (!ValidEntityTypes.Contains(request.EntityType.ToLowerInvariant()))
            {
                return new OperationResult<SmartPlaylistDto>(OperationResponseType.ValidationFailure, $"Invalid entity type '{request.EntityType}'. Must be one of: {string.Join(", ", ValidEntityTypes)}")
                {
                    Data = new SmartPlaylistDto { Name = string.Empty, MqlQuery = string.Empty, EntityType = string.Empty, ApiKey = Guid.Empty, CreatedAt = Instant.MinValue }
                };
            }

            playlist.EntityType = request.EntityType.ToLowerInvariant();
            hasChanges = true;
        }

        if (request.IsPublic.HasValue && request.IsPublic.Value != playlist.IsPublic)
        {
            playlist.IsPublic = request.IsPublic.Value;
            hasChanges = true;
        }

        if (hasChanges)
        {
            playlist.LastUpdatedAt = SystemClock.Instance.GetCurrentInstant();
            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            logger.Information("[SmartPlaylistService] Updated smart playlist {Name} (API key: {ApiKey})", playlist.Name, playlist.ApiKey);
        }

        return new OperationResult<SmartPlaylistDto> { Data = MapToDto(playlist), Type = OperationResponseType.Ok };
    }

    public async Task<OperationResult<bool>> DeleteAsync(Guid playlistApiKey, int userId, CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var playlist = await scopedContext.SmartPlaylists
            .FirstOrDefaultAsync(p => p.ApiKey == playlistApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (playlist == null)
        {
            return new OperationResult<bool>(OperationResponseType.NotFound, "Smart playlist not found.") { Data = false };
        }

        if (playlist.UserId != userId)
        {
            return new OperationResult<bool>(OperationResponseType.AccessDenied, "You do not have permission to delete this smart playlist.") { Data = false };
        }

        scopedContext.SmartPlaylists.Remove(playlist);
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.Information("[SmartPlaylistService] Deleted smart playlist {Name} (API key: {ApiKey})", playlist.Name, playlistApiKey);

        return new OperationResult<bool> { Data = true, Type = OperationResponseType.Ok };
    }

    public async Task<PagedResult<dynamic>> EvaluateAsync(Guid playlistApiKey, PagedRequest paging, int userId, CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var playlist = await scopedContext.SmartPlaylists
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ApiKey == playlistApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (playlist == null)
        {
            return new PagedResult<dynamic>
            {
                TotalCount = 0,
                TotalPages = 0,
                Data = Array.Empty<dynamic>()
            };
        }

        if (playlist.UserId != userId && !playlist.IsPublic)
        {
            return new PagedResult<dynamic>
            {
                TotalCount = 0,
                TotalPages = 0,
                Data = Array.Empty<dynamic>()
            };
        }

        logger.Information("[SmartPlaylistService] Evaluating smart playlist {Name} (API key: {ApiKey})", playlist.Name, playlistApiKey);

        playlist.LastEvaluatedAt = SystemClock.Instance.GetCurrentInstant();
        playlist.LastResultCount = 0;
        scopedContext.SmartPlaylists.Update(playlist);
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new PagedResult<dynamic>
        {
            TotalCount = 0,
            TotalPages = 0,
            Data = Array.Empty<dynamic>()
        };
    }

    private static SmartPlaylistDto MapToDto(SmartPlaylist playlist) => new()
    {
        ApiKey = playlist.ApiKey,
        Name = playlist.Name,
        MqlQuery = playlist.MqlQuery,
        EntityType = playlist.EntityType,
        LastResultCount = playlist.LastResultCount,
        LastEvaluatedAt = playlist.LastEvaluatedAt,
        IsPublic = playlist.IsPublic,
        NormalizedQuery = playlist.NormalizedQuery,
        CreatedAt = playlist.CreatedAt
    };
}

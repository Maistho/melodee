using Ardalis.GuardClauses;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Services.Caching;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services;

public class RequestService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:request:apikey:{0}";

    public async Task<PagedResult<Request>> ListAsync(
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var baseQuery = scopedContext.Requests
            .Include(r => r.CreatedByUser)
            .Include(r => r.LastActivityUser)
            .AsNoTracking();

        var filteredQuery = ApplyFilters(baseQuery, pagedRequest);
        var totalCount = await filteredQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        Request[] requests = [];
        if (!pagedRequest.IsTotalCountOnlyRequest)
        {
            var orderedQuery = filteredQuery
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.Id);

            requests = await orderedQuery
                .Skip(pagedRequest.SkipValue)
                .Take(pagedRequest.TakeValue)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return new PagedResult<Request>
        {
            TotalCount = totalCount,
            TotalPages = pagedRequest.TotalPages(totalCount),
            Data = requests
        };
    }

    public async Task<OperationResult<Request?>> GetByApiKeyAsync(
        Guid apiKey,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Default(apiKey, nameof(apiKey));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var request = await scopedContext.Requests
            .Include(r => r.CreatedByUser)
            .Include(r => r.LastActivityUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ApiKey == apiKey, cancellationToken)
            .ConfigureAwait(false);

        return new OperationResult<Request?>
        {
            Data = request
        };
    }

    public async Task<OperationResult<Request?>> CreateAsync(
        Request request,
        int createdByUserId,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(request, nameof(request));
        Guard.Against.NegativeOrZero(createdByUserId, nameof(createdByUserId));

        var now = SystemClock.Instance.GetCurrentInstant();

        request.ApiKey = Guid.NewGuid();
        request.Status = (int)RequestStatus.Pending;
        request.CreatedAt = now;
        request.CreatedByUserId = createdByUserId;
        request.UpdatedAt = now;
        request.UpdatedByUserId = createdByUserId;
        request.LastActivityAt = now;
        request.LastActivityUserId = createdByUserId;
        request.LastActivityType = (int)RequestActivityType.UserComment;

        request.ArtistNameNormalized = TextNormalizer.Normalize(request.ArtistName);
        request.AlbumTitleNormalized = TextNormalizer.Normalize(request.AlbumTitle);
        request.SongTitleNormalized = TextNormalizer.Normalize(request.SongTitle);
        request.DescriptionNormalized = TextNormalizer.Normalize(request.Description);

        var validationResult = ValidateModel(request);
        if (!validationResult.IsSuccess)
        {
            return new OperationResult<Request?>(validationResult.Data.Item2
                ?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = null,
                Type = OperationResponseType.ValidationFailure
            };
        }

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        scopedContext.Requests.Add(request);

        var participant = new RequestParticipant
        {
            RequestId = request.Id,
            UserId = createdByUserId,
            IsCreator = true,
            IsCommenter = false,
            CreatedAt = now
        };

        var userState = new RequestUserState
        {
            RequestId = request.Id,
            UserId = createdByUserId,
            LastSeenAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        participant.RequestId = request.Id;
        userState.RequestId = request.Id;

        scopedContext.RequestParticipants.Add(participant);
        scopedContext.RequestUserStates.Add(userState);

        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await GetByApiKeyAsync(request.ApiKey, cancellationToken).ConfigureAwait(false);
    }

    public async Task<OperationResult<Request?>> UpdateAsync(
        Guid apiKey,
        int currentUserId,
        Action<Request> updateAction,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Default(apiKey, nameof(apiKey));
        Guard.Against.NegativeOrZero(currentUserId, nameof(currentUserId));
        Guard.Against.Null(updateAction, nameof(updateAction));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var request = await scopedContext.Requests
            .FirstOrDefaultAsync(r => r.ApiKey == apiKey, cancellationToken)
            .ConfigureAwait(false);

        if (request == null)
        {
            return new OperationResult<Request?>("Request not found.")
            {
                Data = null,
                Type = OperationResponseType.NotFound
            };
        }

        if (request.CreatedByUserId != currentUserId)
        {
            return new OperationResult<Request?>("Only the creator can update this request.")
            {
                Data = null,
                Type = OperationResponseType.AccessDenied
            };
        }

        updateAction(request);

        var now = SystemClock.Instance.GetCurrentInstant();
        request.UpdatedAt = now;
        request.UpdatedByUserId = currentUserId;
        request.LastActivityAt = now;
        request.LastActivityUserId = currentUserId;
        request.LastActivityType = (int)RequestActivityType.Edited;

        request.ArtistNameNormalized = TextNormalizer.Normalize(request.ArtistName);
        request.AlbumTitleNormalized = TextNormalizer.Normalize(request.AlbumTitle);
        request.SongTitleNormalized = TextNormalizer.Normalize(request.SongTitle);
        request.DescriptionNormalized = TextNormalizer.Normalize(request.Description);

        var validationResult = ValidateModel(request);
        if (!validationResult.IsSuccess)
        {
            return new OperationResult<Request?>(validationResult.Data.Item2
                ?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = null,
                Type = OperationResponseType.ValidationFailure
            };
        }

        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.Replace("{0}", apiKey.ToString()));

        return await GetByApiKeyAsync(apiKey, cancellationToken).ConfigureAwait(false);
    }

    public async Task<OperationResult<bool>> CompleteAsync(
        Guid apiKey,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Default(apiKey, nameof(apiKey));
        Guard.Against.NegativeOrZero(currentUserId, nameof(currentUserId));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var request = await scopedContext.Requests
            .FirstOrDefaultAsync(r => r.ApiKey == apiKey, cancellationToken)
            .ConfigureAwait(false);

        if (request == null)
        {
            return new OperationResult<bool>("Request not found.")
            {
                Data = false,
                Type = OperationResponseType.NotFound
            };
        }

        if (request.CreatedByUserId != currentUserId)
        {
            return new OperationResult<bool>("Only the creator can complete this request.")
            {
                Data = false,
                Type = OperationResponseType.AccessDenied
            };
        }

        if (request.StatusValue == RequestStatus.Completed)
        {
            return new OperationResult<bool>
            {
                Data = true
            };
        }

        var now = SystemClock.Instance.GetCurrentInstant();
        request.Status = (int)RequestStatus.Completed;
        request.UpdatedAt = now;
        request.UpdatedByUserId = currentUserId;
        request.LastActivityAt = now;
        request.LastActivityUserId = currentUserId;
        request.LastActivityType = (int)RequestActivityType.StatusChanged;

        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.Replace("{0}", apiKey.ToString()));

        return new OperationResult<bool>
        {
            Data = true
        };
    }

    public async Task<OperationResult<bool>> DeleteAsync(
        Guid apiKey,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Default(apiKey, nameof(apiKey));
        Guard.Against.NegativeOrZero(currentUserId, nameof(currentUserId));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var request = await scopedContext.Requests
            .FirstOrDefaultAsync(r => r.ApiKey == apiKey, cancellationToken)
            .ConfigureAwait(false);

        if (request == null)
        {
            return new OperationResult<bool>("Request not found.")
            {
                Data = false,
                Type = OperationResponseType.NotFound
            };
        }

        if (request.CreatedByUserId != currentUserId)
        {
            return new OperationResult<bool>("Only the creator can delete this request.")
            {
                Data = false,
                Type = OperationResponseType.AccessDenied
            };
        }

        if (request.StatusValue != RequestStatus.Pending)
        {
            return new OperationResult<bool>("Only requests with Pending status can be deleted.")
            {
                Data = false,
                Type = OperationResponseType.ValidationFailure
            };
        }

        scopedContext.Requests.Remove(request);
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.Replace("{0}", apiKey.ToString()));

        return new OperationResult<bool>
        {
            Data = true
        };
    }

    public async Task<int> GetCommentCountAsync(
        int requestId,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await scopedContext.RequestComments
            .CountAsync(c => c.RequestId == requestId, cancellationToken)
            .ConfigureAwait(false);
    }

    private static IQueryable<Request> ApplyFilters(IQueryable<Request> query, PagedRequest pagedRequest)
    {
        if (pagedRequest.FilterBy == null || pagedRequest.FilterBy.Length == 0)
        {
            return query;
        }

        foreach (var filter in pagedRequest.FilterBy)
        {
            query = filter.PropertyName.ToLowerInvariant() switch
            {
                "status" => query.Where(r => r.Status == SafeParser.ToNumber<int>(filter.Value)),
                "createdbyuserid" => query.Where(r => r.CreatedByUserId == SafeParser.ToNumber<int>(filter.Value)),
                "category" => query.Where(r => r.Category == SafeParser.ToNumber<int>(filter.Value)),
                "targetartistApiKey" or "targetartistapikey" => query.Where(r => r.TargetArtistApiKey == SafeParser.ToGuid(filter.Value)),
                "targetalbumApiKey" or "targetalbumapikey" => query.Where(r => r.TargetAlbumApiKey == SafeParser.ToGuid(filter.Value)),
                "targetsongApiKey" or "targetsonguapikey" => query.Where(r => r.TargetSongApiKey == SafeParser.ToGuid(filter.Value)),
                "query" => ApplyQueryFilter(query, filter.Value?.ToString()),
                _ => query
            };
        }

        return query;
    }

    private static IQueryable<Request> ApplyQueryFilter(IQueryable<Request> query, string? searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return query;
        }

        var normalizedQuery = TextNormalizer.Normalize(searchQuery);
        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return query;
        }

        var pattern = $"%{normalizedQuery}%";

        return query.Where(r =>
            (r.DescriptionNormalized != null && EF.Functions.ILike(r.DescriptionNormalized, pattern)) ||
            (r.ArtistNameNormalized != null && EF.Functions.ILike(r.ArtistNameNormalized, pattern)) ||
            (r.AlbumTitleNormalized != null && EF.Functions.ILike(r.AlbumTitleNormalized, pattern)) ||
            (r.SongTitleNormalized != null && EF.Functions.ILike(r.SongTitleNormalized, pattern)));
    }
}

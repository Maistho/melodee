using Ardalis.GuardClauses;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services;

public class RequestActivityService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public async Task<bool> HasUnreadAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(userId, nameof(userId));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var hasUnread = await (
            from p in scopedContext.RequestParticipants
            join r in scopedContext.Requests on p.RequestId equals r.Id
            join s in scopedContext.RequestUserStates
                on new { p.RequestId, p.UserId } equals new { s.RequestId, s.UserId }
                into stateJoin
            from s in stateJoin.DefaultIfEmpty()
            where p.UserId == userId
                  && (s == null || r.LastActivityAt > s.LastSeenAt)
                  && r.LastActivityUserId != userId
            select r.Id
        ).AnyAsync(cancellationToken).ConfigureAwait(false);

        return hasUnread;
    }

    public async Task<PagedResult<Request>> GetUnreadRequestsAsync(
        int userId,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(userId, nameof(userId));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var query =
            from p in scopedContext.RequestParticipants
            join r in scopedContext.Requests.Include(x => x.LastActivityUser) on p.RequestId equals r.Id
            join s in scopedContext.RequestUserStates
                on new { p.RequestId, p.UserId } equals new { s.RequestId, s.UserId }
                into stateJoin
            from s in stateJoin.DefaultIfEmpty()
            where p.UserId == userId
                  && (s == null || r.LastActivityAt > s.LastSeenAt)
                  && r.LastActivityUserId != userId
            select r;

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        Request[] requests = [];
        if (!pagedRequest.IsTotalCountOnlyRequest)
        {
            requests = await query
                .OrderByDescending(r => r.LastActivityAt)
                .ThenByDescending(r => r.Id)
                .Skip(pagedRequest.SkipValue)
                .Take(pagedRequest.TakeValue)
                .AsNoTracking()
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

    public async Task<OperationResult<bool>> MarkSeenAsync(
        Guid requestApiKey,
        int userId,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Default(requestApiKey, nameof(requestApiKey));
        Guard.Against.NegativeOrZero(userId, nameof(userId));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var request = await scopedContext.Requests
            .FirstOrDefaultAsync(r => r.ApiKey == requestApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (request == null)
        {
            return new OperationResult<bool>("Request not found.")
            {
                Data = false,
                Type = OperationResponseType.NotFound
            };
        }

        var isParticipant = await scopedContext.RequestParticipants
            .AnyAsync(p => p.RequestId == request.Id && p.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        if (!isParticipant)
        {
            return new OperationResult<bool>("User is not a participant of this request.")
            {
                Data = false,
                Type = OperationResponseType.AccessDenied
            };
        }

        var now = SystemClock.Instance.GetCurrentInstant();

        var userState = await scopedContext.RequestUserStates
            .FirstOrDefaultAsync(s => s.RequestId == request.Id && s.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        if (userState == null)
        {
            scopedContext.RequestUserStates.Add(new RequestUserState
            {
                RequestId = request.Id,
                UserId = userId,
                LastSeenAt = now,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        else
        {
            userState.LastSeenAt = now;
            userState.UpdatedAt = now;
        }

        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new OperationResult<bool>
        {
            Data = true
        };
    }

    public async Task<OperationResult<bool>> MarkSeenByIdAsync(
        int requestId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(requestId, nameof(requestId));
        Guard.Against.NegativeOrZero(userId, nameof(userId));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var request = await scopedContext.Requests
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken)
            .ConfigureAwait(false);

        if (request == null)
        {
            return new OperationResult<bool>("Request not found.")
            {
                Data = false,
                Type = OperationResponseType.NotFound
            };
        }

        return await MarkSeenAsync(request.ApiKey, userId, cancellationToken).ConfigureAwait(false);
    }
}

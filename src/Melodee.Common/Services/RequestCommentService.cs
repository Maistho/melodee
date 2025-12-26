using Ardalis.GuardClauses;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services;

public class RequestCommentService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public async Task<PagedResult<RequestComment>> ListAsync(
        int requestId,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(requestId, nameof(requestId));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var baseQuery = scopedContext.RequestComments
            .Include(c => c.CreatedByUser)
            .Include(c => c.ParentComment)
            .Where(c => c.RequestId == requestId)
            .AsNoTracking();

        var totalCount = await baseQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        RequestComment[] comments = [];
        if (!pagedRequest.IsTotalCountOnlyRequest)
        {
            comments = await baseQuery
                .OrderBy(c => c.CreatedAt)
                .ThenBy(c => c.Id)
                .Skip(pagedRequest.SkipValue)
                .Take(pagedRequest.TakeValue)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return new PagedResult<RequestComment>
        {
            TotalCount = totalCount,
            TotalPages = pagedRequest.TotalPages(totalCount),
            Data = comments
        };
    }

    public async Task<OperationResult<RequestComment?>> GetByApiKeyAsync(
        Guid apiKey,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Default(apiKey, nameof(apiKey));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var comment = await scopedContext.RequestComments
            .Include(c => c.CreatedByUser)
            .Include(c => c.ParentComment)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ApiKey == apiKey, cancellationToken)
            .ConfigureAwait(false);

        return new OperationResult<RequestComment?>
        {
            Data = comment
        };
    }

    public async Task<OperationResult<RequestComment?>> CreateAsync(
        int requestId,
        int userId,
        string body,
        Guid? parentCommentApiKey,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(requestId, nameof(requestId));
        Guard.Against.NegativeOrZero(userId, nameof(userId));
        Guard.Against.NullOrWhiteSpace(body, nameof(body));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var request = await scopedContext.Requests
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken)
            .ConfigureAwait(false);

        if (request == null)
        {
            return new OperationResult<RequestComment?>("Request not found.")
            {
                Data = null,
                Type = OperationResponseType.NotFound
            };
        }

        int? parentCommentId = null;
        if (parentCommentApiKey.HasValue)
        {
            var parentComment = await scopedContext.RequestComments
                .FirstOrDefaultAsync(c => c.ApiKey == parentCommentApiKey.Value && c.RequestId == requestId, cancellationToken)
                .ConfigureAwait(false);

            if (parentComment == null)
            {
                return new OperationResult<RequestComment?>("Parent comment not found or does not belong to this request.")
                {
                    Data = null,
                    Type = OperationResponseType.ValidationFailure
                };
            }

            parentCommentId = parentComment.Id;
        }

        var now = SystemClock.Instance.GetCurrentInstant();

        var comment = new RequestComment
        {
            ApiKey = Guid.NewGuid(),
            RequestId = requestId,
            ParentCommentId = parentCommentId,
            Body = body,
            IsSystem = false,
            CreatedAt = now,
            CreatedByUserId = userId
        };

        scopedContext.RequestComments.Add(comment);

        request.LastActivityAt = now;
        request.LastActivityUserId = userId;
        request.LastActivityType = (int)RequestActivityType.UserComment;

        var existingParticipant = await scopedContext.RequestParticipants
            .FirstOrDefaultAsync(p => p.RequestId == requestId && p.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        if (existingParticipant == null)
        {
            scopedContext.RequestParticipants.Add(new RequestParticipant
            {
                RequestId = requestId,
                UserId = userId,
                IsCreator = false,
                IsCommenter = true,
                CreatedAt = now
            });
        }
        else if (!existingParticipant.IsCommenter)
        {
            existingParticipant.IsCommenter = true;
        }

        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await GetByApiKeyAsync(comment.ApiKey, cancellationToken).ConfigureAwait(false);
    }

    public async Task<OperationResult<RequestComment?>> CreateSystemCommentAsync(
        int requestId,
        string body,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(requestId, nameof(requestId));
        Guard.Against.NullOrWhiteSpace(body, nameof(body));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var request = await scopedContext.Requests
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken)
            .ConfigureAwait(false);

        if (request == null)
        {
            return new OperationResult<RequestComment?>("Request not found.")
            {
                Data = null,
                Type = OperationResponseType.NotFound
            };
        }

        var now = SystemClock.Instance.GetCurrentInstant();

        var comment = new RequestComment
        {
            ApiKey = Guid.NewGuid(),
            RequestId = requestId,
            ParentCommentId = null,
            Body = body,
            IsSystem = true,
            CreatedAt = now,
            CreatedByUserId = null
        };

        scopedContext.RequestComments.Add(comment);

        request.LastActivityAt = now;
        request.LastActivityUserId = null;
        request.LastActivityType = (int)RequestActivityType.SystemComment;

        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await GetByApiKeyAsync(comment.ApiKey, cancellationToken).ConfigureAwait(false);
    }
}

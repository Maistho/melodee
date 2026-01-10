using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services;

/// <summary>
/// Service for managing party queue operations with optimistic concurrency.
/// </summary>
public sealed class PartyQueueService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory)
    : ServiceBase(logger, cacheManager, contextFactory), IPartyQueueService
{
    private const string CacheKeyTemplate = "urn:party:queue:{0}";

    public async Task<OperationResult<(long Revision, IEnumerable<PartyQueueItem> Items)>> GetQueueAsync(
        Guid sessionApiKey,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<(long, IEnumerable<PartyQueueItem>)>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        var cacheKey = string.Format(CacheKeyTemplate, sessionApiKey);
        if (CacheManager.TryGet(cacheKey, out (long Revision, IEnumerable<PartyQueueItem> Items)? cached))
        {
            return new OperationResult<(long, IEnumerable<PartyQueueItem>)>(cached.Value);
        }

        var items = await scopedContext.PartyQueueItems
            .AsNoTracking()
            .Where(x => x.PartySessionId == session.Id)
            .OrderBy(x => x.SortOrder)
            .Include(x => x.EnqueuedByUser)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = (session.QueueRevision, items.AsEnumerable());
        CacheManager.Set(cacheKey, result, TimeSpan.FromSeconds(30));

        return new OperationResult<(long, IEnumerable<PartyQueueItem>)>(result);
    }

    public async Task<OperationResult<(long NewRevision, IEnumerable<PartyQueueItem> AddedItems)>> AddItemsAsync(
        Guid sessionApiKey,
        IEnumerable<Guid> songApiKeys,
        int enqueuedByUserId,
        string? source,
        long expectedRevision,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .Include(x => x.QueueItems)
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<(long, IEnumerable<PartyQueueItem>)>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        if (session.QueueRevision != expectedRevision)
        {
            return new OperationResult<(long, IEnumerable<PartyQueueItem>)>(
                $"Concurrent modification detected. Current revision: {session.QueueRevision}")
            {
                Type = OperationResponseType.Conflict
            };
        }

        var songKeysList = songApiKeys.ToList();
        if (!songKeysList.Any())
        {
            return new OperationResult<(long, IEnumerable<PartyQueueItem>)>("No song API keys provided.")
            {
                Type = OperationResponseType.BadRequest
            };
        }

        var currentMaxSortOrder = session.QueueItems.Any()
            ? session.QueueItems.Max(x => x.SortOrder)
            : 0;

        var newItems = new List<PartyQueueItem>();
        var enqueuedAt = SystemClock.Instance.GetCurrentInstant();

        foreach (var songApiKey in songKeysList)
        {
            var item = new PartyQueueItem
            {
                PartySessionId = session.Id,
                SongApiKey = songApiKey,
                EnqueuedByUserId = enqueuedByUserId,
                EnqueuedAt = enqueuedAt,
                SortOrder = ++currentMaxSortOrder,
                Source = source
            };

            scopedContext.PartyQueueItems.Add(item);
            newItems.Add(item);
        }

        session.QueueRevision++;
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.RemoveByPrefix(string.Format(CacheKeyTemplate, sessionApiKey));

        Logger.Information("[PartyQueueService] Added {Count} items to session {SessionId}", newItems.Count, session.Id);

        return new OperationResult<(long, IEnumerable<PartyQueueItem>)>((session.QueueRevision, newItems));
    }

    public async Task<OperationResult<long>> RemoveItemAsync(
        Guid sessionApiKey,
        Guid itemApiKey,
        int requestingUserId,
        long expectedRevision,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .Include(x => x.QueueItems)
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<long>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        if (session.QueueRevision != expectedRevision)
        {
            return new OperationResult<long>($"Concurrent modification detected. Current revision: {session.QueueRevision}")
            {
                Type = OperationResponseType.Conflict
            };
        }

        var item = session.QueueItems.FirstOrDefault(x => x.ApiKey == itemApiKey);
        if (item == null)
        {
            return new OperationResult<long>("Queue item not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        scopedContext.PartyQueueItems.Remove(item);

        var itemsToReorder = session.QueueItems
            .Where(x => x.SortOrder > item.SortOrder)
            .OrderBy(x => x.SortOrder)
            .ToList();

        foreach (var itemToReorder in itemsToReorder)
        {
            itemToReorder.SortOrder--;
        }

        session.QueueRevision++;
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.RemoveByPrefix(string.Format(CacheKeyTemplate, sessionApiKey));

        Logger.Information("[PartyQueueService] Removed item {ItemApiKey} from session {SessionId}", itemApiKey, session.Id);

        return new OperationResult<long>(session.QueueRevision);
    }

    public async Task<OperationResult<long>> ReorderItemAsync(
        Guid sessionApiKey,
        Guid itemApiKey,
        int newIndex,
        int requestingUserId,
        long expectedRevision,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .Include(x => x.QueueItems)
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<long>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        if (session.QueueRevision != expectedRevision)
        {
            return new OperationResult<long>($"Concurrent modification detected. Current revision: {session.QueueRevision}")
            {
                Type = OperationResponseType.Conflict
            };
        }

        var item = session.QueueItems.FirstOrDefault(x => x.ApiKey == itemApiKey);
        if (item == null)
        {
            return new OperationResult<long>("Queue item not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        var oldIndex = item.SortOrder;
        if (oldIndex == newIndex)
        {
            return new OperationResult<long>(session.QueueRevision);
        }

        if (newIndex < 0)
        {
            newIndex = 0;
        }

        var maxIndex = session.QueueItems.Count - 1;
        if (newIndex > maxIndex)
        {
            newIndex = maxIndex;
        }

        if (newIndex < oldIndex)
        {
            var itemsBetween = session.QueueItems
                .Where(x => x.SortOrder >= newIndex && x.SortOrder < oldIndex && x.ApiKey != itemApiKey)
                .OrderBy(x => x.SortOrder);

            foreach (var itemBetween in itemsBetween)
            {
                itemBetween.SortOrder++;
            }
        }
        else if (newIndex > oldIndex)
        {
            var itemsBetween = session.QueueItems
                .Where(x => x.SortOrder > oldIndex && x.SortOrder <= newIndex && x.ApiKey != itemApiKey)
                .OrderBy(x => x.SortOrder);

            foreach (var itemBetween in itemsBetween)
            {
                itemBetween.SortOrder--;
            }
        }

        item.SortOrder = newIndex;
        session.QueueRevision++;
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.RemoveByPrefix(string.Format(CacheKeyTemplate, sessionApiKey));

        Logger.Information("[PartyQueueService] Reordered item {ItemApiKey} to index {NewIndex} in session {SessionId}",
            itemApiKey, newIndex, session.Id);

        return new OperationResult<long>(session.QueueRevision);
    }

    public async Task<OperationResult<long>> ClearAsync(
        Guid sessionApiKey,
        int requestingUserId,
        long expectedRevision,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .Include(x => x.QueueItems)
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<long>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        if (session.QueueRevision != expectedRevision)
        {
            return new OperationResult<long>($"Concurrent modification detected. Current revision: {session.QueueRevision}")
            {
                Type = OperationResponseType.Conflict
            };
        }

        scopedContext.PartyQueueItems.RemoveRange(session.QueueItems);
        session.QueueItems.Clear();

        session.QueueRevision++;
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.RemoveByPrefix(string.Format(CacheKeyTemplate, sessionApiKey));

        Logger.Information("[PartyQueueService] Cleared queue for session {SessionId}", session.Id);

        return new OperationResult<long>(session.QueueRevision);
    }

    public async Task<OperationResult<PartyQueueItem?>> GetNextItemAsync(
        Guid sessionApiKey,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<PartyQueueItem?>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        var nextItem = await scopedContext.PartyQueueItems
            .AsNoTracking()
            .Where(x => x.PartySessionId == session.Id)
            .OrderBy(x => x.SortOrder)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return new OperationResult<PartyQueueItem?>(nextItem);
    }
}
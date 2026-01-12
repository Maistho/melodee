using Melodee.Common.Data.Models;
using Melodee.Common.Models;

namespace Melodee.Common.Services;

/// <summary>
/// Interface for party queue operations.
/// </summary>
public interface IPartyQueueService
{
    /// <summary>
    /// Gets the queue for a session.
    /// </summary>
    Task<OperationResult<(long Revision, IEnumerable<PartyQueueItem> Items)>> GetQueueAsync(Guid sessionApiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds songs to the queue.
    /// </summary>
    Task<OperationResult<(long NewRevision, IEnumerable<PartyQueueItem> AddedItems)>> AddItemsAsync(
        Guid sessionApiKey,
        IEnumerable<Guid> songApiKeys,
        int enqueuedByUserId,
        string? source,
        long expectedRevision,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an item from the queue.
    /// </summary>
    Task<OperationResult<long>> RemoveItemAsync(Guid sessionApiKey, Guid itemApiKey, int requestingUserId, long expectedRevision, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorders an item in the queue.
    /// </summary>
    Task<OperationResult<long>> ReorderItemAsync(Guid sessionApiKey, Guid itemApiKey, int newIndex, int requestingUserId, long expectedRevision, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the queue.
    /// </summary>
    Task<OperationResult<long>> ClearAsync(Guid sessionApiKey, int requestingUserId, long expectedRevision, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next item in the queue.
    /// </summary>
    Task<OperationResult<PartyQueueItem?>> GetNextItemAsync(Guid sessionApiKey, CancellationToken cancellationToken = default);
}

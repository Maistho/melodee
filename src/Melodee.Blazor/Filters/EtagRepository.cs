using System.Collections.Concurrent;

namespace Melodee.Blazor.Filters;

public class EtagRepository
{
    private readonly ConcurrentDictionary<string, ETagEntry> _eTags = new();
    private readonly ConcurrentQueue<string> _insertionOrder = new();
    private readonly int _maxEntries;
    private readonly TimeSpan _entryMaxAge;
    private volatile int _currentCount;
    private DateTime _lastCleanup = DateTime.UtcNow;
    private readonly object _cleanupLock = new object();
    private long _hitCount;
    private long _missCount;

    private readonly record struct ETagEntry(string ETag, DateTime CreatedAt);

    public EtagRepository(int maxEntries = 10000, TimeSpan? entryMaxAge = null)
    {
        _maxEntries = maxEntries;
        _entryMaxAge = entryMaxAge ?? TimeSpan.FromHours(24);
    }

    public bool AddEtag(string? apiKeyId, string? etag)
    {
        if (!string.IsNullOrWhiteSpace(apiKeyId) && !string.IsNullOrWhiteSpace(etag))
        {
            var entry = new ETagEntry(etag, DateTime.UtcNow);
            
            // Use AddOrUpdate to ensure the ETag is always current
            _eTags.AddOrUpdate(apiKeyId!, entry, (_, _) => entry);
            
            // Only track in insertion order if this is a new entry
            if (!_insertionOrder.Contains(apiKeyId!))
            {
                _insertionOrder.Enqueue(apiKeyId!);
                Interlocked.Increment(ref _currentCount);
            }

            if (_currentCount > _maxEntries || ShouldCleanup())
            {
                _ = Task.Run(CleanupExpiredEntries);
            }

            return true;
        }

        return false;
    }

    public bool EtagMatch(string? apiKeyId, string? etag)
    {
        if (!string.IsNullOrWhiteSpace(apiKeyId) && !string.IsNullOrWhiteSpace(etag))
        {
            if (_eTags.TryGetValue(apiKeyId!, out var entry))
            {
                if (DateTime.UtcNow - entry.CreatedAt <= _entryMaxAge)
                {
                    var match = entry.ETag == etag;
                    if (match)
                    {
                        Interlocked.Increment(ref _hitCount);
                    }
                    else
                    {
                        Interlocked.Increment(ref _missCount);
                    }
                    return match;
                }
                else
                {
                    if (_eTags.TryRemove(apiKeyId!, out _))
                    {
                        Interlocked.Decrement(ref _currentCount);
                    }
                    Interlocked.Increment(ref _missCount);
                }
            }
            else
            {
                Interlocked.Increment(ref _missCount);
            }
        }

        return false;
    }

    /// <summary>
    /// Removes the ETag entry for a specific API key ID, forcing the next request to fetch fresh content.
    /// </summary>
    /// <param name="apiKeyId">The API key ID to invalidate</param>
    /// <returns>True if the entry was removed, false if it didn't exist</returns>
    public bool InvalidateEtag(string? apiKeyId)
    {
        if (!string.IsNullOrWhiteSpace(apiKeyId) && _eTags.TryRemove(apiKeyId!, out _))
        {
            Interlocked.Decrement(ref _currentCount);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes all ETag entries that start with the specified prefix.
    /// Useful for invalidating all image sizes for an artist or album.
    /// </summary>
    /// <param name="apiKeyIdPrefix">The prefix to match (e.g., artist or album API key)</param>
    /// <returns>Number of entries invalidated</returns>
    public int InvalidateEtagsStartingWith(string? apiKeyIdPrefix)
    {
        if (string.IsNullOrWhiteSpace(apiKeyIdPrefix))
        {
            return 0;
        }

        var count = 0;
        foreach (var key in _eTags.Keys.Where(k => k.StartsWith(apiKeyIdPrefix!, StringComparison.OrdinalIgnoreCase)).ToList())
        {
            if (_eTags.TryRemove(key, out _))
            {
                Interlocked.Decrement(ref _currentCount);
                count++;
            }
        }
        return count;
    }

    private bool ShouldCleanup()
    {
        var now = DateTime.UtcNow;
        return now - _lastCleanup > TimeSpan.FromMinutes(10);
    }

    private void CleanupExpiredEntries()
    {
        if (!Monitor.TryEnter(_cleanupLock, TimeSpan.FromSeconds(1)))
        {
            return;
        }

        try
        {
            var now = DateTime.UtcNow;
            var entriesRemoved = 0;
            var targetCount = (int)(_maxEntries * 0.8);

            // FIFO eviction using queue - O(n) worst case but typically O(k) where k is entries to remove
            while (_currentCount > targetCount && _insertionOrder.TryDequeue(out var oldestKey))
            {
                if (_eTags.TryGetValue(oldestKey, out var entry))
                {
                    // Remove if expired OR if we're over capacity
                    if (now - entry.CreatedAt > _entryMaxAge || _currentCount > _maxEntries)
                    {
                        if (_eTags.TryRemove(oldestKey, out _))
                        {
                            Interlocked.Decrement(ref _currentCount);
                            entriesRemoved++;
                        }
                    }
                    else
                    {
                        // Entry is still valid and we're not over hard limit, re-queue it
                        _insertionOrder.Enqueue(oldestKey);
                        break;
                    }
                }
            }

            _lastCleanup = now;
        }
        finally
        {
            Monitor.Exit(_cleanupLock);
        }
    }

    public int CurrentCount => _currentCount;
    public void ForceCleanup() => CleanupExpiredEntries();
    public long HitCount => Interlocked.Read(ref _hitCount);
    public long MissCount => Interlocked.Read(ref _missCount);
}

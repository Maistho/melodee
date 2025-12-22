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
            var added = _eTags.TryAdd(apiKeyId!, entry);

            if (added)
            {
                _insertionOrder.Enqueue(apiKeyId!);
                Interlocked.Increment(ref _currentCount);

                if (_currentCount > _maxEntries || ShouldCleanup())
                {
                    _ = Task.Run(CleanupExpiredEntries);
                }
            }

            return added;
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

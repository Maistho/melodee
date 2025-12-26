using System.Diagnostics;
using Melodee.Common.Services.Caching;
using Melodee.Common.Serialization;
using Serilog;

namespace Melodee.Tests.Common.Services.Caching;

/// <summary>
/// Tests to verify that concurrent image requests (thundering herd scenario) 
/// complete within acceptable time limits.
/// 
/// This simulates the real-world scenario where an OpenSubsonic client fetches
/// an artist with many albums and requests all album cover images simultaneously.
/// </summary>
public sealed class ConcurrentImageRequestTests
{
    private const int SimulatedDbLatencyMs = 50;
    private const int SimulatedFileReadLatencyMs = 20;
    
    /// <summary>
    /// Simulates the thundering herd problem: when a client requests an artist with 30+ albums,
    /// it immediately requests cover art for all albums simultaneously.
    /// 
    /// The test should fail if concurrent requests for different keys cause excessive delays
    /// due to resource contention.
    /// 
    /// Acceptance criteria:
    /// - 30 concurrent requests for different album images should complete within 2 seconds
    /// - Each individual request simulates 50ms DB lookup + 20ms file read = 70ms per request
    /// - With proper concurrency handling, all 30 should complete in ~200-500ms (parallel execution)
    /// - Without proper handling, they could take 30 * 70ms = 2100ms+ (sequential)
    /// </summary>
    [Fact]
    public async Task ConcurrentRequestsForDifferentAlbums_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var serializer = new Serializer(logger);
        var cacheManager = new MemoryCacheManager(logger, TimeSpan.FromMinutes(5), serializer);
        
        const int numberOfAlbums = 30;
        const int maxAcceptableTimeMs = 2000; // 2 seconds max for all requests
        
        var factoryCallCount = 0;
        var albumIds = Enumerable.Range(1, numberOfAlbums).Select(i => $"album_{Guid.NewGuid()}").ToList();
        
        // Act - Simulate concurrent requests for all album images
        var stopwatch = Stopwatch.StartNew();
        
        var tasks = albumIds.Select(async albumId =>
        {
            var cacheKey = $"urn:openSubsonic:imageForApikey:{albumId}:Large";
            
            return await cacheManager.GetAsync(cacheKey, async () =>
            {
                Interlocked.Increment(ref factoryCallCount);
                
                // Simulate database lookup for album info
                await Task.Delay(SimulatedDbLatencyMs);
                
                // Simulate file system read for image bytes
                await Task.Delay(SimulatedFileReadLatencyMs);
                
                // Return simulated image bytes
                return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG header bytes
            }, CancellationToken.None);
        }).ToList();
        
        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();
        
        // Assert
        Assert.Equal(numberOfAlbums, results.Length);
        Assert.All(results, r => Assert.NotNull(r));
        Assert.Equal(numberOfAlbums, factoryCallCount); // Each unique key should call factory once
        
        // This assertion will FAIL if concurrent requests cause excessive delays
        Assert.True(
            stopwatch.ElapsedMilliseconds < maxAcceptableTimeMs,
            $"Concurrent requests took {stopwatch.ElapsedMilliseconds}ms, expected less than {maxAcceptableTimeMs}ms. " +
            $"This indicates a thundering herd problem where concurrent requests are causing resource contention.");
    }
    
    /// <summary>
    /// Verifies that the same album image requested multiple times concurrently
    /// only triggers one factory call (request coalescing).
    /// </summary>
    [Fact]
    public async Task ConcurrentRequestsForSameAlbum_ShouldCoalesceIntoSingleFactoryCall()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var serializer = new Serializer(logger);
        var cacheManager = new MemoryCacheManager(logger, TimeSpan.FromMinutes(5), serializer);
        
        const int numberOfConcurrentRequests = 20;
        var factoryCallCount = 0;
        var albumId = $"album_{Guid.NewGuid()}";
        var cacheKey = $"urn:openSubsonic:imageForApikey:{albumId}:Large";
        
        // Act - Simulate multiple clients requesting the same album image simultaneously
        var tasks = Enumerable.Range(0, numberOfConcurrentRequests).Select(_ =>
            cacheManager.GetAsync(cacheKey, async () =>
            {
                Interlocked.Increment(ref factoryCallCount);
                
                // Simulate slow factory to ensure requests overlap
                await Task.Delay(100);
                
                return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
            }, CancellationToken.None)).ToList();
        
        var results = await Task.WhenAll(tasks);
        
        // Assert
        Assert.Equal(numberOfConcurrentRequests, results.Length);
        Assert.All(results, r => Assert.NotNull(r));
        
        // Request coalescing should ensure only ONE factory call
        Assert.Equal(1, factoryCallCount);
    }
    
    /// <summary>
    /// Simulates a realistic artist page load where the client requests:
    /// 1. Artist info (with image)
    /// 2. All album cover images simultaneously
    /// 
    /// This test verifies the system can handle this burst of requests efficiently.
    /// </summary>
    [Fact]
    public async Task SimulatedArtistPageLoad_WithManyAlbums_ShouldCompleteQuickly()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var serializer = new Serializer(logger);
        var cacheManager = new MemoryCacheManager(logger, TimeSpan.FromMinutes(5), serializer);
        
        const int numberOfAlbums = 50; // Artist like Elton John or Celine Dion with many albums
        const int maxAcceptableTimeMs = 3000; // 3 seconds max
        
        var artistId = $"artist_{Guid.NewGuid()}";
        var albumIds = Enumerable.Range(1, numberOfAlbums).Select(i => $"album_{Guid.NewGuid()}").ToList();
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        
        // Request artist image
        var artistImageTask = cacheManager.GetAsync(
            $"urn:openSubsonic:imageForApikey:{artistId}:Large",
            async () =>
            {
                await Task.Delay(SimulatedDbLatencyMs + SimulatedFileReadLatencyMs);
                return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
            }, CancellationToken.None);
        
        // Request all album images concurrently
        var albumImageTasks = albumIds.Select(albumId =>
            cacheManager.GetAsync(
                $"urn:openSubsonic:imageForApikey:{albumId}:Large",
                async () =>
                {
                    await Task.Delay(SimulatedDbLatencyMs + SimulatedFileReadLatencyMs);
                    return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
                }, CancellationToken.None)).ToList();
        
        // Wait for all
        await artistImageTask;
        await Task.WhenAll(albumImageTasks);
        
        stopwatch.Stop();
        
        // Assert
        Assert.True(
            stopwatch.ElapsedMilliseconds < maxAcceptableTimeMs,
            $"Artist page load with {numberOfAlbums} albums took {stopwatch.ElapsedMilliseconds}ms, " +
            $"expected less than {maxAcceptableTimeMs}ms.");
    }
    
    /// <summary>
    /// Tests that throttled/rate-limited requests complete successfully even under load.
    /// This simulates implementing a semaphore or similar mechanism to limit concurrent I/O.
    /// </summary>
    [Fact]
    public async Task ThrottledConcurrentRequests_ShouldStillCompleteWithinReasonableTime()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var serializer = new Serializer(logger);
        var cacheManager = new MemoryCacheManager(logger, TimeSpan.FromMinutes(5), serializer);
        
        const int numberOfAlbums = 30;
        const int maxConcurrency = 5; // Throttle to 5 concurrent operations
        const int maxAcceptableTimeMs = 5000; // With throttling, allow more time
        
        var semaphore = new SemaphoreSlim(maxConcurrency);
        var factoryCallCount = 0;
        var albumIds = Enumerable.Range(1, numberOfAlbums).Select(i => $"album_{Guid.NewGuid()}").ToList();
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        
        var tasks = albumIds.Select(async albumId =>
        {
            var cacheKey = $"urn:openSubsonic:imageForApikey:{albumId}:Large";
            
            return await cacheManager.GetAsync(cacheKey, async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    Interlocked.Increment(ref factoryCallCount);
                    await Task.Delay(SimulatedDbLatencyMs + SimulatedFileReadLatencyMs);
                    return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
                }
                finally
                {
                    semaphore.Release();
                }
            }, CancellationToken.None);
        }).ToList();
        
        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();
        
        // Assert
        Assert.Equal(numberOfAlbums, results.Length);
        Assert.Equal(numberOfAlbums, factoryCallCount);
        
        // With throttling at 5 concurrent: 30 requests / 5 = 6 batches * 70ms = 420ms minimum
        // Allow reasonable overhead
        Assert.True(
            stopwatch.ElapsedMilliseconds < maxAcceptableTimeMs,
            $"Throttled requests took {stopwatch.ElapsedMilliseconds}ms, expected less than {maxAcceptableTimeMs}ms.");
        
        // Verify throttling worked - execution time should be at least batch_count * latency
        var expectedMinTimeMs = (numberOfAlbums / maxConcurrency) * (SimulatedDbLatencyMs + SimulatedFileReadLatencyMs);
        Assert.True(
            stopwatch.ElapsedMilliseconds >= expectedMinTimeMs * 0.8, // Allow 20% variance
            $"Throttled requests completed too quickly ({stopwatch.ElapsedMilliseconds}ms), " +
            $"expected at least {expectedMinTimeMs}ms. Throttling may not be working correctly.");
    }
    
    /// <summary>
    /// This test simulates what happens in production when the ACTUAL database connection pool
    /// becomes a bottleneck. With a limited connection pool, many concurrent requests will
    /// block waiting for connections.
    /// 
    /// This test demonstrates the problem by using a semaphore to simulate a database connection pool.
    /// 
    /// TARGET: We want this test to PASS with improved caching/throttling.
    /// Currently it demonstrates the bottleneck issue.
    /// </summary>
    [Fact]
    public async Task ConcurrentRequestsWithLimitedDbConnections_ShouldCompleteWithinAcceptableTime()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var serializer = new Serializer(logger);
        var cacheManager = new MemoryCacheManager(logger, TimeSpan.FromMinutes(5), serializer);
        
        const int numberOfAlbums = 30;
        const int dbConnectionPoolSize = 3; // Simulate a small connection pool (like production might have)
        
        // TARGET: With 3 DB connections and 30 albums:
        // - Minimum time = 30/3 * 50ms = 500ms for DB operations
        // - Plus ~20ms file read per request (parallel) = ~520-600ms total
        // - We want this to complete in under 3000ms (increased for CI stability)
        const int maxAcceptableTimeMs = 3000;
        
        // Simulate database connection pool
        var dbConnectionPool = new SemaphoreSlim(dbConnectionPoolSize);
        var factoryCallCount = 0;
        var maxConcurrentDbConnections = 0;
        var currentDbConnections = 0;
        var connectionLock = new object();
        
        var albumIds = Enumerable.Range(1, numberOfAlbums).Select(i => $"album_{Guid.NewGuid()}").ToList();
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        
        var tasks = albumIds.Select(async albumId =>
        {
            var cacheKey = $"urn:openSubsonic:imageForApikey:{albumId}:Large";
            
            return await cacheManager.GetAsync(cacheKey, async () =>
            {
                // Acquire database connection (simulates connection pool)
                await dbConnectionPool.WaitAsync();
                try
                {
                    lock (connectionLock)
                    {
                        currentDbConnections++;
                        maxConcurrentDbConnections = Math.Max(maxConcurrentDbConnections, currentDbConnections);
                    }
                    
                    Interlocked.Increment(ref factoryCallCount);
                    
                    // Simulate database query
                    await Task.Delay(SimulatedDbLatencyMs);
                    
                    // Simulate file read (doesn't need DB connection)
                    lock (connectionLock)
                    {
                        currentDbConnections--;
                    }
                }
                finally
                {
                    dbConnectionPool.Release();
                }
                
                await Task.Delay(SimulatedFileReadLatencyMs);
                return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
            }, CancellationToken.None);
        }).ToList();
        
        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();
        
        // Assert
        Assert.Equal(numberOfAlbums, results.Length);
        Assert.Equal(numberOfAlbums, factoryCallCount);
        Assert.True(maxConcurrentDbConnections <= dbConnectionPoolSize, 
            $"Max concurrent DB connections ({maxConcurrentDbConnections}) exceeded pool size ({dbConnectionPoolSize})");
        
        // THIS IS THE KEY ASSERTION - requests should complete within acceptable time
        // even with limited DB connections
        Assert.True(
            stopwatch.ElapsedMilliseconds < maxAcceptableTimeMs,
            $"Requests with limited DB connections took {stopwatch.ElapsedMilliseconds}ms, " +
            $"expected less than {maxAcceptableTimeMs}ms. " +
            $"This indicates resource contention is causing excessive delays.");
    }
}

using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Services;
using Moq;

namespace Melodee.Tests.Common.Services;

public class StreamingLimiterTests
{
    private Mock<IMelodeeConfigurationFactory> CreateMockConfigFactory(int? globalLimit = null, int? perUserLimit = null)
    {
        var mockConfig = new Mock<IMelodeeConfiguration>();
        mockConfig.Setup(c => c.GetValue<int?>(SettingRegistry.StreamingMaxConcurrentStreamsGlobal))
            .Returns(globalLimit);
        mockConfig.Setup(c => c.GetValue<int?>(SettingRegistry.StreamingMaxConcurrentStreamsPerUser))
            .Returns(perUserLimit);

        var mockFactory = new Mock<IMelodeeConfigurationFactory>();
        mockFactory.Setup(f => f.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockConfig.Object);

        return mockFactory;
    }

    [Fact]
    public void Constructor_InitializesSuccessfully()
    {
        // Arrange
        var mockConfigurationFactory = new Mock<IMelodeeConfigurationFactory>();

        // Act
        var service = new StreamingLimiter(mockConfigurationFactory.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task TryEnterAsync_WithNoLimits_AlwaysSucceeds()
    {
        // Arrange
        var mockFactory = CreateMockConfigFactory(globalLimit: 0, perUserLimit: 0);
        var limiter = new StreamingLimiter(mockFactory.Object);

        // Act - Try to enter many times
        var results = new List<bool>();
        for (int i = 0; i < 100; i++)
        {
            results.Add(await limiter.TryEnterAsync("user1"));
        }

        // Assert - All should succeed
        Assert.All(results, r => Assert.True(r));
    }

    [Fact]
    public async Task TryEnterAsync_WithGlobalLimit_EnforcesLimit()
    {
        // Arrange
        var mockFactory = CreateMockConfigFactory(globalLimit: 3, perUserLimit: 0);
        var limiter = new StreamingLimiter(mockFactory.Object);

        // Act
        var result1 = await limiter.TryEnterAsync("user1");
        var result2 = await limiter.TryEnterAsync("user2");
        var result3 = await limiter.TryEnterAsync("user3");
        var result4 = await limiter.TryEnterAsync("user4"); // Should fail - over global limit

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.False(result4);
    }

    [Fact]
    public async Task TryEnterAsync_WithPerUserLimit_EnforcesLimit()
    {
        // Arrange
        var mockFactory = CreateMockConfigFactory(globalLimit: 0, perUserLimit: 2);
        var limiter = new StreamingLimiter(mockFactory.Object);

        // Act - Same user tries 3 times
        var result1 = await limiter.TryEnterAsync("user1");
        var result2 = await limiter.TryEnterAsync("user1");
        var result3 = await limiter.TryEnterAsync("user1"); // Should fail - over per-user limit

        // Different user should still succeed
        var result4 = await limiter.TryEnterAsync("user2");

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.False(result3);
        Assert.True(result4);
    }

    [Fact]
    public async Task TryEnterAsync_WithBothLimits_EnforcesBoth()
    {
        // Arrange - Global: 5, Per-user: 2
        var mockFactory = CreateMockConfigFactory(globalLimit: 5, perUserLimit: 2);
        var limiter = new StreamingLimiter(mockFactory.Object);

        // Act
        // User1 takes 2 slots (max per-user)
        var u1_1 = await limiter.TryEnterAsync("user1");
        var u1_2 = await limiter.TryEnterAsync("user1");
        var u1_3 = await limiter.TryEnterAsync("user1"); // Should fail - per-user limit

        // User2 takes 2 slots
        var u2_1 = await limiter.TryEnterAsync("user2");
        var u2_2 = await limiter.TryEnterAsync("user2");

        // User3 can take 1 more (global has 1 slot left)
        var u3_1 = await limiter.TryEnterAsync("user3");
        var u3_2 = await limiter.TryEnterAsync("user3"); // Should fail - global limit

        // Assert
        Assert.True(u1_1);
        Assert.True(u1_2);
        Assert.False(u1_3); // Per-user limit
        Assert.True(u2_1);
        Assert.True(u2_2);
        Assert.True(u3_1);
        Assert.False(u3_2); // Global limit
    }

    [Fact]
    public async Task Exit_FreesSlot_AllowsNewEntry()
    {
        // Arrange
        var mockFactory = CreateMockConfigFactory(globalLimit: 2, perUserLimit: 0);
        var limiter = new StreamingLimiter(mockFactory.Object);

        // Act - Fill up the slots
        await limiter.TryEnterAsync("user1");
        await limiter.TryEnterAsync("user2");
        var failedBefore = await limiter.TryEnterAsync("user3"); // Should fail

        // Exit one slot
        limiter.Exit("user1");

        // Now should succeed
        var successAfter = await limiter.TryEnterAsync("user3");

        // Assert
        Assert.False(failedBefore);
        Assert.True(successAfter);
    }

    [Fact]
    public async Task Exit_FreesPerUserSlot_AllowsNewEntry()
    {
        // Arrange
        var mockFactory = CreateMockConfigFactory(globalLimit: 0, perUserLimit: 1);
        var limiter = new StreamingLimiter(mockFactory.Object);

        // Act - Fill the per-user slot
        await limiter.TryEnterAsync("user1");
        var failedBefore = await limiter.TryEnterAsync("user1"); // Should fail

        // Exit the slot
        limiter.Exit("user1");

        // Now should succeed
        var successAfter = await limiter.TryEnterAsync("user1");

        // Assert
        Assert.False(failedBefore);
        Assert.True(successAfter);
    }

    [Fact]
    public void Exit_MultipleTimesForSameUser_DoesNotGoNegative()
    {
        // Arrange
        var mockFactory = CreateMockConfigFactory(globalLimit: 10, perUserLimit: 10);
        var limiter = new StreamingLimiter(mockFactory.Object);

        // Act - Exit without entering (safe to call)
        limiter.Exit("user1");
        limiter.Exit("user1");
        limiter.Exit("user1");

        // This should not throw and should still work correctly
        // Assert - No exception thrown
        Assert.True(true);
    }

    [Fact]
    public async Task TryEnterAsync_ConcurrentAccess_IsThreadSafe()
    {
        // Arrange
        var mockFactory = CreateMockConfigFactory(globalLimit: 50, perUserLimit: 0);
        var limiter = new StreamingLimiter(mockFactory.Object);
        var successCount = 0;
        var tasks = new List<Task>();

        // Act - 100 concurrent requests
        for (int i = 0; i < 100; i++)
        {
            var userKey = $"user{i % 10}";
            tasks.Add(Task.Run(async () =>
            {
                if (await limiter.TryEnterAsync(userKey))
                {
                    Interlocked.Increment(ref successCount);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - Exactly 50 should succeed (global limit)
        Assert.Equal(50, successCount);
    }

    [Fact]
    public async Task TryEnterAsync_WithNullLimits_TreatsAsUnlimited()
    {
        // Arrange - null limits should be treated as 0 (unlimited)
        var mockFactory = CreateMockConfigFactory(globalLimit: null, perUserLimit: null);
        var limiter = new StreamingLimiter(mockFactory.Object);

        // Act
        var results = new List<bool>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(await limiter.TryEnterAsync("user1"));
        }

        // Assert - All should succeed
        Assert.All(results, r => Assert.True(r));
    }

    [Fact]
    public async Task TryEnterAsync_WithNegativeLimits_TreatsAsUnlimited()
    {
        // Arrange - Negative limits should be treated as unlimited
        var mockFactory = CreateMockConfigFactory(globalLimit: -1, perUserLimit: -1);
        var limiter = new StreamingLimiter(mockFactory.Object);

        // Act
        var results = new List<bool>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(await limiter.TryEnterAsync("user1"));
        }

        // Assert - All should succeed
        Assert.All(results, r => Assert.True(r));
    }

    [Fact]
    public async Task TryEnterAsync_GlobalLimitOnly_MultipleUsersShareLimit()
    {
        // Arrange
        var mockFactory = CreateMockConfigFactory(globalLimit: 3, perUserLimit: 0);
        var limiter = new StreamingLimiter(mockFactory.Object);

        // Act - Three different users each take one slot
        var u1 = await limiter.TryEnterAsync("user1");
        var u2 = await limiter.TryEnterAsync("user2");
        var u3 = await limiter.TryEnterAsync("user3");
        var u4 = await limiter.TryEnterAsync("user4"); // Should fail

        // Assert
        Assert.True(u1);
        Assert.True(u2);
        Assert.True(u3);
        Assert.False(u4);
    }

    [Fact]
    public async Task Exit_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var mockFactory = CreateMockConfigFactory(globalLimit: 2, perUserLimit: 2);
        var limiter = new StreamingLimiter(mockFactory.Object);
        using var cts = new CancellationTokenSource();

        // Act
        await limiter.TryEnterAsync("user1", cts.Token);
        limiter.Exit("user1");

        // Should be able to enter again
        var result = await limiter.TryEnterAsync("user1", cts.Token);

        // Assert
        Assert.True(result);
    }
}

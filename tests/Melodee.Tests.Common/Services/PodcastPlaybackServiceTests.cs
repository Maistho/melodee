using Melodee.Common.Data.Models;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Tests.Common.Services;

public class PodcastPlaybackServiceTests : ServiceTestBase
{
    private async Task<User> GetOrCreateTestUser()
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "podcastuser");
        if (existingUser != null)
        {
            return existingUser;
        }

        var user = new User
        {
            UserName = "podcastuser",
            UserNameNormalized = "PODCASTUSER",
            Email = "podcast@example.com",
            EmailNormalized = "PODCAST@EXAMPLE.COM",
            PublicKey = "test-key",
            PasswordEncrypted = "encrypted",
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private async Task<(PodcastChannel Channel, PodcastEpisode Episode)> CreateTestPodcastData()
    {
        var user = await GetOrCreateTestUser();

        await using var context = await MockFactory().CreateDbContextAsync();
        var channel = new PodcastChannel
        {
            UserId = user.Id,
            FeedUrl = "https://example.com/feed.xml",
            Title = "Test Channel",
            Description = "Test Description",
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        context.PodcastChannels.Add(channel);
        await context.SaveChangesAsync();

        var episode = new PodcastEpisode
        {
            PodcastChannelId = channel.Id,
            Title = "Test Episode",
            Description = "Test Description",
            PublishDate = SystemClock.Instance.GetCurrentInstant(),
            EnclosureUrl = "https://example.com/episode.mp3",
            EpisodeKey = Guid.NewGuid().ToString(),
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        context.PodcastEpisodes.Add(episode);
        await context.SaveChangesAsync();

        return (channel, episode);
    }

    [Fact]
    public async Task NowPlayingAsync_CreatesPlayHistory()
    {
        // Arrange
        var service = new PodcastPlaybackService(Logger, CacheManager, MockFactory());
        var user = await GetOrCreateTestUser();
        var (_, episode) = await CreateTestPodcastData();

        // Act
        var result = await service.NowPlayingAsync(user.Id, episode.Id, 30, "TestClient");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ScrobbleAsync_UpdatesPlayHistory()
    {
        // Arrange
        var service = new PodcastPlaybackService(Logger, CacheManager, MockFactory());
        var user = await GetOrCreateTestUser();
        var (_, episode) = await CreateTestPodcastData();

        // Act
        var result = await service.ScrobbleAsync(user.Id, episode.Id, 120, "TestClient");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SaveBookmarkAsync_CreatesBookmark()
    {
        // Arrange
        var service = new PodcastPlaybackService(Logger, CacheManager, MockFactory());
        var user = await GetOrCreateTestUser();
        var (_, episode) = await CreateTestPodcastData();

        // Act
        var result = await service.SaveBookmarkAsync(user.Id, episode.Id, 60, "Test comment");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetBookmarkAsync_ReturnsBookmark()
    {
        // Arrange
        var service = new PodcastPlaybackService(Logger, CacheManager, MockFactory());
        var user = await GetOrCreateTestUser();
        var (_, episode) = await CreateTestPodcastData();

        // Create a bookmark first
        await service.SaveBookmarkAsync(user.Id, episode.Id, 60, "Test comment");

        // Act
        var result = await service.GetBookmarkAsync(user.Id, episode.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(60, result.Data.PositionSeconds);
    }

    [Fact]
    public async Task DeleteBookmarkAsync_RemovesBookmark()
    {
        // Arrange
        var service = new PodcastPlaybackService(Logger, CacheManager, MockFactory());
        var user = await GetOrCreateTestUser();
        var (_, episode) = await CreateTestPodcastData();

        // Create a bookmark first
        await service.SaveBookmarkAsync(user.Id, episode.Id, 60, "Test comment");

        // Act
        var result = await service.DeleteBookmarkAsync(user.Id, episode.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetPlayHistoryAsync_ReturnsPlayHistory()
    {
        // Arrange
        var service = new PodcastPlaybackService(Logger, CacheManager, MockFactory());
        var user = await GetOrCreateTestUser();
        var (_, episode) = await CreateTestPodcastData();

        // Create a scrobble first
        await service.ScrobbleAsync(user.Id, episode.Id, 120, "TestClient");

        // Act
        var result = await service.GetPlayHistoryAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetNowPlayingAsync_ReturnsNowPlaying()
    {
        // Arrange
        var service = new PodcastPlaybackService(Logger, CacheManager, MockFactory());
        var user = await GetOrCreateTestUser();
        var (_, episode) = await CreateTestPodcastData();

        // Set now playing
        await service.NowPlayingAsync(user.Id, episode.Id, 30, "TestClient");

        // Act
        var result = await service.GetNowPlayingAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }
}

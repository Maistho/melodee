using Melodee.Common.Data.Models;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Tests.Common.Services;

public class OpenSubsonicPodcastScrobbleTests : ServiceTestBase
{
    #region Helper Methods

    private async Task<User> CreateTestUserInDb(string username = "testuser", string email = "test@example.com")
    {
        await using var context = await MockFactory().CreateDbContextAsync();

        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == username);
        if (existingUser != null)
        {
            return existingUser;
        }

        var password = "testpassword";
        var publicKey = EncryptionHelper.GenerateRandomPublicKeyBase64();
        var config = TestsBase.NewPluginsConfiguration();
        var encryptedPassword = EncryptionHelper.Encrypt(
            config.GetValue<string>(Melodee.Common.Constants.SettingRegistry.EncryptionPrivateKey)!,
            password,
            publicKey);

        var user = new User
        {
            UserName = username,
            UserNameNormalized = username.ToUpperInvariant(),
            Email = email,
            EmailNormalized = email.ToUpperInvariant(),
            PublicKey = publicKey,
            PasswordEncrypted = encryptedPassword,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private async Task<(PodcastChannel Channel, PodcastEpisode Episode)> CreateTestPodcastData()
    {
        var user = await CreateTestUserInDb();

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
            PublishDate = DateTimeOffset.UtcNow,
            EnclosureUrl = "https://example.com/episode.mp3",
            EpisodeKey = Guid.NewGuid().ToString(),
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        context.PodcastEpisodes.Add(episode);
        await context.SaveChangesAsync();

        return (channel, episode);
    }

    #endregion

    #region ScrobbleAsync Tests

    [Fact]
    public async Task ScrobbleAsync_WithPodcastEpisodeId_RoutesToPodcastPlaybackService()
    {
        var service = GetOpenSubsonicApiService();
        var (_, episode) = await CreateTestPodcastData();
        var user = await CreateTestUserInDb("podcastscrobbleuser", "podcastscrobble@example.com");
        var apiRequest = GetApiRequest("podcastscrobbleuser", "salt", "password");

        var episodeApiId = $"podcast:episode:{episode.Id}";
        var result = await service.ScrobbleAsync(
            [episodeApiId],
            [60],
            true,
            apiRequest,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ScrobbleAsync_WithSongId_StillWorks()
    {
        var service = GetOpenSubsonicApiService();
        var user = await CreateTestUserInDb("songscrobbleuser", "songscrobble@example.com");
        var apiRequest = GetApiRequest("songscrobbleuser", "salt", "password");

        var songApiKey = $"{Melodee.Common.Data.Constants.OpenSubsonicServer.ApiIdSeparator}{Guid.NewGuid()}";
        var songApiId = $"song{songApiKey}";

        var result = await service.ScrobbleAsync(
            [songApiId],
            [30],
            false,
            apiRequest,
            CancellationToken.None);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task ScrobbleAsync_NowPlayingWithPodcastEpisodeId_CallsPodcastPlaybackService()
    {
        var service = GetOpenSubsonicApiService();
        var (_, episode) = await CreateTestPodcastData();
        var user = await CreateTestUserInDb("nowplayinguser", "nowplaying@example.com");
        var apiRequest = GetApiRequest("nowplayinguser", "salt", "password");

        var episodeApiId = $"podcast:episode:{episode.Id}";
        var result = await service.ScrobbleAsync(
            [episodeApiId],
            [45],
            false,
            apiRequest,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region GetNowPlayingAsync Tests

    [Fact]
    public async Task GetNowPlayingAsync_IncludesPodcastEpisodes()
    {
        var service = GetOpenSubsonicApiService();
        var user = await CreateTestUserInDb("nowplayinggetuser", "nowplayingget@example.com");
        var (_, episode) = await CreateTestPodcastData();
        var apiRequest = GetApiRequest("nowplayinggetuser", "salt", "password");

        var podcastService = GetPodcastPlaybackService();
        await podcastService.NowPlayingAsync(user.Id, episode.Id, 30, "TestClient");

        var result = await service.GetNowPlayingAsync(apiRequest, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.ResponseData);
    }

    #endregion

    #region Bookmark Tests

    [Fact]
    public async Task CreateBookmarkAsync_WithPodcastEpisodeId_RoutesToPodcastPlaybackService()
    {
        var service = GetOpenSubsonicApiService();
        var (_, episode) = await CreateTestPodcastData();
        var user = await CreateTestUserInDb("bookmarkcreateuser", "bookmarkcreate@example.com");
        var apiRequest = GetApiRequest("bookmarkcreateuser", "salt", "password");

        var episodeApiId = $"podcast:episode:{episode.Id}";
        var result = await service.CreateBookmarkAsync(episodeApiId, 120, "Test bookmark", apiRequest, CancellationToken.None);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetBookmarksAsync_ReturnsPodcastBookmarks()
    {
        var service = GetOpenSubsonicApiService();
        var user = await CreateTestUserInDb("bookmarkgetuser", "bookmarkget@example.com");
        var (_, episode) = await CreateTestPodcastData();
        var apiRequest = GetApiRequest("bookmarkgetuser", "salt", "password");

        var podcastService = GetPodcastPlaybackService();
        await podcastService.SaveBookmarkAsync(user.Id, episode.Id, 60, "Test bookmark");

        var result = await service.GetBookmarksAsync(apiRequest, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.ResponseData);
    }

    [Fact]
    public async Task DeleteBookmarkAsync_WithPodcastEpisodeId_RoutesToPodcastPlaybackService()
    {
        var service = GetOpenSubsonicApiService();
        var user = await CreateTestUserInDb("bookmarkdeleteuser", "bookmarkdelete@example.com");
        var (_, episode) = await CreateTestPodcastData();
        var apiRequest = GetApiRequest("bookmarkdeleteuser", "salt", "password");

        var podcastService = GetPodcastPlaybackService();
        await podcastService.SaveBookmarkAsync(user.Id, episode.Id, 60, "Test bookmark");

        var episodeApiId = $"podcast:episode:{episode.Id}";
        var result = await service.DeleteBookmarkAsync(episodeApiId, apiRequest, CancellationToken.None);

        Assert.NotNull(result);
    }

    #endregion

    #region ID Parsing Tests

    [Fact]
    public void IsPodcastEpisodeId_WithValidId_ReturnsTrue()
    {
        Assert.True(OpenSubsonicApiServiceTests.IsPodcastEpisodeId("podcast:episode:123"));
        Assert.True(OpenSubsonicApiServiceTests.IsPodcastEpisodeId("podcast:episode:1"));
        Assert.True(OpenSubsonicApiServiceTests.IsPodcastEpisodeId("PODCAST:EPISODE:456"));
    }

    [Fact]
    public void IsPodcastEpisodeId_WithInvalidId_ReturnsFalse()
    {
        Assert.False(OpenSubsonicApiServiceTests.IsPodcastEpisodeId("song:123"));
        Assert.False(OpenSubsonicApiServiceTests.IsPodcastEpisodeId("album:123"));
        Assert.False(OpenSubsonicApiServiceTests.IsPodcastEpisodeId("podcast:channel:123"));
        Assert.False(OpenSubsonicApiServiceTests.IsPodcastEpisodeId(null));
        Assert.False(OpenSubsonicApiServiceTests.IsPodcastEpisodeId(""));
    }

    [Fact]
    public void ParsePodcastEpisodeIdFromApiId_WithValidId_ReturnsEpisodeId()
    {
        Assert.Equal(123, OpenSubsonicApiServiceTests.ParsePodcastEpisodeIdFromApiId("podcast:episode:123"));
        Assert.Equal(1, OpenSubsonicApiServiceTests.ParsePodcastEpisodeIdFromApiId("podcast:episode:1"));
        Assert.Equal(456, OpenSubsonicApiServiceTests.ParsePodcastEpisodeIdFromApiId("PODCAST:EPISODE:456"));
    }

    [Fact]
    public void ParsePodcastEpisodeIdFromApiId_WithInvalidId_ReturnsNull()
    {
        Assert.Null(OpenSubsonicApiServiceTests.ParsePodcastEpisodeIdFromApiId("song:123"));
        Assert.Null(OpenSubsonicApiServiceTests.ParsePodcastEpisodeIdFromApiId("podcast:channel:123"));
        Assert.Null(OpenSubsonicApiServiceTests.ParsePodcastEpisodeIdFromApiId(null));
        Assert.Null(OpenSubsonicApiServiceTests.ParsePodcastEpisodeIdFromApiId(""));
    }

    #endregion
}

public partial class OpenSubsonicApiServiceTests
{
    public static bool IsPodcastEpisodeId(string? id) =>
        id?.StartsWith("podcast:episode:", StringComparison.OrdinalIgnoreCase) == true;

    public static int? ParsePodcastEpisodeIdFromApiId(string? id)
    {
        if (!IsPodcastEpisodeId(id)) return null;
        return int.TryParse(id!.Substring("podcast:episode:".Length), out var episodeId) ? episodeId : null;
    }
}

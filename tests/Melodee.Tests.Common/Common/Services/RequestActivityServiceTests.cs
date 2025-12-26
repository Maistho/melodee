using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Services;
using NodaTime;

namespace Melodee.Tests.Common.Common.Services;

public class RequestActivityServiceTests : ServiceTestBase
{
    private RequestActivityService GetActivityService()
    {
        return new RequestActivityService(Logger, CacheManager, MockFactory());
    }

    private RequestService GetRequestService()
    {
        return new RequestService(Logger, CacheManager, MockFactory());
    }

    private RequestCommentService GetCommentService()
    {
        return new RequestCommentService(Logger, CacheManager, MockFactory());
    }

    private async Task<User> CreateTestUserAsync()
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var user = new User
        {
            UserName = $"testuser_{uniqueId}",
            UserNameNormalized = $"testuser_{uniqueId}",
            Email = $"test_{uniqueId}@example.com",
            EmailNormalized = $"test_{uniqueId}@example.com",
            PublicKey = "publickey",
            PasswordEncrypted = "password",
            IsAdmin = false,
            ApiKey = Guid.NewGuid(),
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private async Task<Request> CreateTestRequestAsync(int userId)
    {
        var requestService = GetRequestService();
        var request = new Request
        {
            Category = (int)RequestCategory.AddAlbum,
            Description = "Test request"
        };
        var result = await requestService.CreateAsync(request, userId);
        return result.Data!;
    }

    [Fact]
    public async Task HasUnreadAsync_ReturnsFalse_WhenNoRequests()
    {
        var user = await CreateTestUserAsync();
        var service = GetActivityService();

        var hasUnread = await service.HasUnreadAsync(user.Id);

        Assert.False(hasUnread);
    }

    [Fact]
    public async Task HasUnreadAsync_ReturnsFalse_WhenUserCreatedRequestAndNoOtherActivity()
    {
        var user = await CreateTestUserAsync();
        await CreateTestRequestAsync(user.Id);
        var service = GetActivityService();

        var hasUnread = await service.HasUnreadAsync(user.Id);

        Assert.False(hasUnread);
    }

    [Fact]
    public async Task HasUnreadAsync_ReturnsTrue_WhenAnotherUserCommented()
    {
        var creator = await CreateTestUserAsync();
        var commenter = await CreateTestUserAsync();
        var request = await CreateTestRequestAsync(creator.Id);
        var commentService = GetCommentService();
        var activityService = GetActivityService();

        await commentService.CreateAsync(request.Id, commenter.Id, "New comment", null);

        var hasUnread = await activityService.HasUnreadAsync(creator.Id);

        Assert.True(hasUnread);
    }

    [Fact]
    public async Task HasUnreadAsync_ReturnsFalse_AfterMarkingSeen()
    {
        var creator = await CreateTestUserAsync();
        var commenter = await CreateTestUserAsync();
        var request = await CreateTestRequestAsync(creator.Id);
        var commentService = GetCommentService();
        var activityService = GetActivityService();

        await commentService.CreateAsync(request.Id, commenter.Id, "New comment", null);
        await activityService.MarkSeenAsync(request.ApiKey, creator.Id);

        var hasUnread = await activityService.HasUnreadAsync(creator.Id);

        Assert.False(hasUnread);
    }

    [Fact]
    public async Task GetUnreadRequestsAsync_ReturnsUnreadRequests()
    {
        var creator = await CreateTestUserAsync();
        var commenter = await CreateTestUserAsync();
        var request = await CreateTestRequestAsync(creator.Id);
        var commentService = GetCommentService();
        var activityService = GetActivityService();

        await commentService.CreateAsync(request.Id, commenter.Id, "New comment", null);

        var result = await activityService.GetUnreadRequestsAsync(creator.Id, new PagedRequest { PageSize = 10 });

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(request.ApiKey, result.Data.First().ApiKey);
    }

    [Fact]
    public async Task GetUnreadRequestsAsync_ReturnsEmpty_WhenNoUnread()
    {
        var user = await CreateTestUserAsync();
        await CreateTestRequestAsync(user.Id);
        var service = GetActivityService();

        var result = await service.GetUnreadRequestsAsync(user.Id, new PagedRequest { PageSize = 10 });

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task MarkSeenAsync_ReturnsNotFound_WhenRequestDoesNotExist()
    {
        var user = await CreateTestUserAsync();
        var service = GetActivityService();

        var result = await service.MarkSeenAsync(Guid.NewGuid(), user.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.NotFound, result.Type);
    }

    [Fact]
    public async Task MarkSeenAsync_ReturnsAccessDenied_WhenUserNotParticipant()
    {
        var creator = await CreateTestUserAsync();
        var otherUser = await CreateTestUserAsync();
        var request = await CreateTestRequestAsync(creator.Id);
        var service = GetActivityService();

        var result = await service.MarkSeenAsync(request.ApiKey, otherUser.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.AccessDenied, result.Type);
    }

    [Fact]
    public async Task MarkSeenAsync_Succeeds_WhenUserIsParticipant()
    {
        var creator = await CreateTestUserAsync();
        var request = await CreateTestRequestAsync(creator.Id);
        var service = GetActivityService();

        var result = await service.MarkSeenAsync(request.ApiKey, creator.Id);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task MarkSeenAsync_UpdatesExistingUserState()
    {
        var creator = await CreateTestUserAsync();
        var commenter = await CreateTestUserAsync();
        var request = await CreateTestRequestAsync(creator.Id);
        var commentService = GetCommentService();
        var activityService = GetActivityService();

        await commentService.CreateAsync(request.Id, commenter.Id, "Comment 1", null);
        await activityService.MarkSeenAsync(request.ApiKey, creator.Id);

        await Task.Delay(10);
        await commentService.CreateAsync(request.Id, commenter.Id, "Comment 2", null);

        var hasUnreadBefore = await activityService.HasUnreadAsync(creator.Id);
        Assert.True(hasUnreadBefore);

        await activityService.MarkSeenAsync(request.ApiKey, creator.Id);

        var hasUnreadAfter = await activityService.HasUnreadAsync(creator.Id);
        Assert.False(hasUnreadAfter);
    }
}

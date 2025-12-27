using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Tests.Common.Common.Services;

public class RequestCommentServiceTests : ServiceTestBase
{
    private RequestCommentService GetCommentService()
    {
        return new RequestCommentService(Logger, CacheManager, MockFactory());
    }

    private RequestService GetRequestService()
    {
        return new RequestService(Logger, CacheManager, MockFactory());
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
    public async Task ListAsync_ReturnsEmptyResult_WhenNoComments()
    {
        var user = await CreateTestUserAsync();
        var request = await CreateTestRequestAsync(user.Id);
        var service = GetCommentService();

        var result = await service.ListAsync(request.Id, new PagedRequest { PageSize = 10 });

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task CreateAsync_CreatesComment_WithValidData()
    {
        var user = await CreateTestUserAsync();
        var request = await CreateTestRequestAsync(user.Id);
        var service = GetCommentService();

        var result = await service.CreateAsync(request.Id, user.Id, "Test comment", null);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Test comment", result.Data.Body);
        Assert.False(result.Data.IsSystem);
        Assert.Equal(user.Id, result.Data.CreatedByUserId);
    }

    [Fact]
    public async Task CreateAsync_UpdatesRequestLastActivity()
    {
        var user = await CreateTestUserAsync();
        var request = await CreateTestRequestAsync(user.Id);
        var originalActivityAt = request.LastActivityAt;
        var service = GetCommentService();

        await Task.Delay(10);
        await service.CreateAsync(request.Id, user.Id, "Test comment", null);

        await using var context = await MockFactory().CreateDbContextAsync();
        var updatedRequest = await context.Requests.FirstAsync(r => r.Id == request.Id);

        Assert.True(updatedRequest.LastActivityAt > originalActivityAt);
        Assert.Equal(RequestActivityType.UserComment, updatedRequest.LastActivityTypeValue);
    }

    [Fact]
    public async Task CreateAsync_CreatesReply_WhenParentCommentProvided()
    {
        var user = await CreateTestUserAsync();
        var request = await CreateTestRequestAsync(user.Id);
        var service = GetCommentService();

        var parentResult = await service.CreateAsync(request.Id, user.Id, "Parent comment", null);
        var replyResult = await service.CreateAsync(request.Id, user.Id, "Reply comment", parentResult.Data!.ApiKey);

        Assert.True(replyResult.IsSuccess);
        Assert.NotNull(replyResult.Data);
        Assert.Equal(parentResult.Data.Id, replyResult.Data.ParentCommentId);
    }

    [Fact]
    public async Task CreateAsync_ReturnsValidationFailure_WhenParentCommentDoesNotExist()
    {
        var user = await CreateTestUserAsync();
        var request = await CreateTestRequestAsync(user.Id);
        var service = GetCommentService();

        var result = await service.CreateAsync(request.Id, user.Id, "Comment", Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.ValidationFailure, result.Type);
    }

    [Fact]
    public async Task CreateAsync_AddsUserAsParticipant_WhenNotAlreadyParticipant()
    {
        var creator = await CreateTestUserAsync();
        var commenter = await CreateTestUserAsync();
        var request = await CreateTestRequestAsync(creator.Id);
        var service = GetCommentService();

        await service.CreateAsync(request.Id, commenter.Id, "Comment from another user", null);

        await using var context = await MockFactory().CreateDbContextAsync();
        var participant = await context.RequestParticipants
            .FirstOrDefaultAsync(p => p.RequestId == request.Id && p.UserId == commenter.Id);

        Assert.NotNull(participant);
        Assert.True(participant.IsCommenter);
        Assert.False(participant.IsCreator);
    }

    [Fact]
    public async Task CreateSystemCommentAsync_CreatesSystemComment()
    {
        var user = await CreateTestUserAsync();
        var request = await CreateTestRequestAsync(user.Id);
        var service = GetCommentService();

        var result = await service.CreateSystemCommentAsync(request.Id, "System message");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.IsSystem);
        Assert.Null(result.Data.CreatedByUserId);
        Assert.Equal("System message", result.Data.Body);
    }

    [Fact]
    public async Task ListAsync_ReturnsCommentsInChronologicalOrder()
    {
        var user = await CreateTestUserAsync();
        var request = await CreateTestRequestAsync(user.Id);
        var service = GetCommentService();

        await service.CreateAsync(request.Id, user.Id, "First comment", null);
        await Task.Delay(10);
        await service.CreateAsync(request.Id, user.Id, "Second comment", null);
        await Task.Delay(10);
        await service.CreateAsync(request.Id, user.Id, "Third comment", null);

        var result = await service.ListAsync(request.Id, new PagedRequest { PageSize = 10 });

        Assert.Equal(3, result.TotalCount);
        Assert.Equal("First comment", result.Data.First().Body);
        Assert.Equal("Third comment", result.Data.Last().Body);
    }

    [Fact]
    public async Task ListAsync_ReturnsThreadedComments_WithReplies()
    {
        var user = await CreateTestUserAsync();
        var request = await CreateTestRequestAsync(user.Id);
        var service = GetCommentService();

        var parent1Result = await service.CreateAsync(request.Id, user.Id, "Parent comment 1", null);
        await Task.Delay(10);
        var reply1Result = await service.CreateAsync(request.Id, user.Id, "Reply to parent 1", parent1Result.Data!.ApiKey);
        await Task.Delay(10);
        var parent2Result = await service.CreateAsync(request.Id, user.Id, "Parent comment 2", null);
        await Task.Delay(10);
        var reply2Result = await service.CreateAsync(request.Id, user.Id, "Another reply to parent 1", parent1Result.Data!.ApiKey);

        var result = await service.ListAsync(request.Id, new PagedRequest { PageSize = 10 });

        Assert.Equal(4, result.TotalCount);

        var topLevelComments = result.Data.Where(c => c.ParentCommentId == null).ToList();
        Assert.Equal(2, topLevelComments.Count);

        var repliestoParent1 = result.Data.Where(c => c.ParentCommentId == parent1Result.Data.Id).ToList();
        Assert.Equal(2, repliestoParent1.Count);
        Assert.Equal("Reply to parent 1", repliestoParent1[0].Body);
        Assert.Equal("Another reply to parent 1", repliestoParent1[1].Body);
    }
}

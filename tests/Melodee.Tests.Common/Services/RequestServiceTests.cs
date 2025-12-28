using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Tests.Common.Services;

public class RequestServiceTests : ServiceTestBase
{
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

    private Request CreateValidRequest(RequestCategory category = RequestCategory.AddAlbum)
    {
        return new Request
        {
            Category = (int)category,
            Description = "Test request description"
        };
    }

    #region ListAsync Tests

    [Fact]
    public async Task ListAsync_ReturnsEmptyResult_WhenNoRequests()
    {
        var service = GetRequestService();
        var pagedRequest = new PagedRequest { PageSize = 10 };

        var result = await service.ListAsync(pagedRequest);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task ListAsync_ReturnsCorrectPagination_WhenMultipleRequests()
    {
        var service = GetRequestService();
        var user = await CreateTestUserAsync();

        for (var i = 0; i < 5; i++)
        {
            var request = CreateValidRequest();
            request.Description = $"Request {i}";
            await service.CreateAsync(request, user.Id);
        }

        var pagedRequest = new PagedRequest { PageSize = 2, Page = 1 };
        var result = await service.ListAsync(pagedRequest);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(2, result.Data.Count());
    }

    [Fact]
    public async Task ListAsync_FiltersbyStatus_WhenStatusFilterProvided()
    {
        var service = GetRequestService();
        var user = await CreateTestUserAsync();

        var pendingRequest = CreateValidRequest();
        await service.CreateAsync(pendingRequest, user.Id);

        var completedRequest = CreateValidRequest();
        var createResult = await service.CreateAsync(completedRequest, user.Id);
        await service.CompleteAsync(createResult.Data!.ApiKey, user.Id);

        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            FilterBy = [new FilterOperatorInfo("Status", FilterOperator.Equals, (int)RequestStatus.Pending)]
        };

        var result = await service.ListAsync(pagedRequest);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.TotalCount);
        Assert.All(result.Data, r => Assert.Equal(RequestStatus.Pending, r.StatusValue));
    }

    [Fact]
    public async Task ListAsync_FiltersByCreatedByUserId_WhenMineFilterProvided()
    {
        var service = GetRequestService();
        var user1 = await CreateTestUserAsync();
        var user2 = await CreateTestUserAsync();

        await service.CreateAsync(CreateValidRequest(), user1.Id);
        await service.CreateAsync(CreateValidRequest(), user1.Id);
        await service.CreateAsync(CreateValidRequest(), user2.Id);

        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            FilterBy = [new FilterOperatorInfo("CreatedByUserId", FilterOperator.Equals, user1.Id)]
        };

        var result = await service.ListAsync(pagedRequest);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Data, r => Assert.Equal(user1.Id, r.CreatedByUserId));
    }

    [Fact]
    public async Task ListAsync_ReturnsNewlyCreatedRequest_ImmediatelyAfterCreation()
    {
        var service = GetRequestService();
        var user = await CreateTestUserAsync();
        var request = CreateValidRequest();
        request.Description = "Newly created request";
        request.ArtistName = "Test Artist";

        var createResult = await service.CreateAsync(request, user.Id);
        Assert.True(createResult.IsSuccess);
        Assert.NotNull(createResult.Data);

        var listResult = await service.ListAsync(new PagedRequest
        {
            PageSize = 10,
            Page = 1
        });

        Assert.True(listResult.IsSuccess);
        Assert.True(listResult.TotalCount > 0);
        Assert.Contains(listResult.Data, r => r.ApiKey == createResult.Data.ApiKey);
    }

    [Fact]
    public async Task ListAsync_ReturnsUserRequests_WhenFilteringByUserId()
    {
        var service = GetRequestService();
        var user = await CreateTestUserAsync();

        var request1 = CreateValidRequest();
        request1.Description = "First request";
        await service.CreateAsync(request1, user.Id);

        var request2 = CreateValidRequest();
        request2.Description = "Second request";
        await service.CreateAsync(request2, user.Id);

        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            FilterBy = [new FilterOperatorInfo("CreatedByUserId", FilterOperator.Equals, user.Id)]
        };

        var result = await service.ListAsync(pagedRequest);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Data, r => Assert.Equal(user.Id, r.CreatedByUserId));
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_CreatesRequest_WithValidData()
    {
        var service = GetRequestService();
        var user = await CreateTestUserAsync();
        var request = CreateValidRequest();
        request.ArtistName = "Test Artist";
        request.AlbumTitle = "Test Album";

        var result = await service.CreateAsync(request, user.Id);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotEqual(Guid.Empty, result.Data.ApiKey);
        Assert.Equal(RequestStatus.Pending, result.Data.StatusValue);
        Assert.Equal(user.Id, result.Data.CreatedByUserId);
        Assert.Equal("test artist", result.Data.ArtistNameNormalized);
        Assert.Equal("test album", result.Data.AlbumTitleNormalized);
    }

    [Fact]
    public async Task CreateAsync_SetsLastActivity_Correctly()
    {
        var service = GetRequestService();
        var user = await CreateTestUserAsync();
        var request = CreateValidRequest();

        var result = await service.CreateAsync(request, user.Id);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(result.Data.CreatedAt, result.Data.LastActivityAt);
        Assert.Equal(user.Id, result.Data.LastActivityUserId);
    }

    [Fact]
    public async Task CreateAsync_CreatesParticipantRecord_ForCreator()
    {
        var service = GetRequestService();
        var user = await CreateTestUserAsync();
        var request = CreateValidRequest();

        var result = await service.CreateAsync(request, user.Id);

        Assert.True(result.IsSuccess);

        await using var context = await MockFactory().CreateDbContextAsync();
        var participant = await context.RequestParticipants
            .FirstOrDefaultAsync(p => p.RequestId == result.Data!.Id && p.UserId == user.Id);

        Assert.NotNull(participant);
        Assert.True(participant.IsCreator);
    }

    [Fact]
    public async Task CreateAsync_CreatesUserStateRecord_ForCreator()
    {
        var service = GetRequestService();
        var user = await CreateTestUserAsync();
        var request = CreateValidRequest();

        var result = await service.CreateAsync(request, user.Id);

        Assert.True(result.IsSuccess);

        await using var context = await MockFactory().CreateDbContextAsync();
        var userState = await context.RequestUserStates
            .FirstOrDefaultAsync(s => s.RequestId == result.Data!.Id && s.UserId == user.Id);

        Assert.NotNull(userState);
        Assert.Equal(result.Data!.CreatedAt, userState.LastSeenAt);
    }

    #endregion

    #region GetByApiKeyAsync Tests

    [Fact]
    public async Task GetByApiKeyAsync_ReturnsRequest_WhenExists()
    {
        var service = GetRequestService();
        var user = await CreateTestUserAsync();
        var request = CreateValidRequest();

        var createResult = await service.CreateAsync(request, user.Id);
        var result = await service.GetByApiKeyAsync(createResult.Data!.ApiKey);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(createResult.Data.ApiKey, result.Data.ApiKey);
    }

    [Fact]
    public async Task GetByApiKeyAsync_ReturnsNull_WhenNotExists()
    {
        var service = GetRequestService();

        var result = await service.GetByApiKeyAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_UpdatesRequest_WhenCreatorUpdates()
    {
        var service = GetRequestService();
        var user = await CreateTestUserAsync();
        var request = CreateValidRequest();

        var createResult = await service.CreateAsync(request, user.Id);
        var newDescription = "Updated description";

        var result = await service.UpdateAsync(createResult.Data!.ApiKey, user.Id, r =>
        {
            r.Description = newDescription;
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(newDescription, result.Data.Description);
        Assert.Equal(RequestActivityType.Edited, result.Data.LastActivityTypeValue);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsAccessDenied_WhenNonCreatorUpdates()
    {
        var service = GetRequestService();
        var creator = await CreateTestUserAsync();
        var otherUser = await CreateTestUserAsync();
        var request = CreateValidRequest();

        var createResult = await service.CreateAsync(request, creator.Id);

        var result = await service.UpdateAsync(createResult.Data!.ApiKey, otherUser.Id, r =>
        {
            r.Description = "Unauthorized update";
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.AccessDenied, result.Type);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNotFound_WhenRequestDoesNotExist()
    {
        var service = GetRequestService();
        var user = await CreateTestUserAsync();

        var result = await service.UpdateAsync(Guid.NewGuid(), user.Id, r =>
        {
            r.Description = "Update";
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.NotFound, result.Type);
    }

    #endregion

    #region CompleteAsync Tests

    [Fact]
    public async Task CompleteAsync_CompletesRequest_WhenCreatorCompletes()
    {
        var service = GetRequestService();
        var user = await CreateTestUserAsync();
        var request = CreateValidRequest();

        var createResult = await service.CreateAsync(request, user.Id);
        var result = await service.CompleteAsync(createResult.Data!.ApiKey, user.Id);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        var getResult = await service.GetByApiKeyAsync(createResult.Data.ApiKey);
        Assert.Equal(RequestStatus.Completed, getResult.Data!.StatusValue);
    }

    [Fact]
    public async Task CompleteAsync_IsIdempotent_WhenAlreadyCompleted()
    {
        var service = GetRequestService();
        var user = await CreateTestUserAsync();
        var request = CreateValidRequest();

        var createResult = await service.CreateAsync(request, user.Id);
        await service.CompleteAsync(createResult.Data!.ApiKey, user.Id);

        var result = await service.CompleteAsync(createResult.Data.ApiKey, user.Id);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task CompleteAsync_ReturnsAccessDenied_WhenNonCreatorCompletes()
    {
        var service = GetRequestService();
        var creator = await CreateTestUserAsync();
        var otherUser = await CreateTestUserAsync();
        var request = CreateValidRequest();

        var createResult = await service.CreateAsync(request, creator.Id);
        var result = await service.CompleteAsync(createResult.Data!.ApiKey, otherUser.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.AccessDenied, result.Type);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_DeletesRequest_WhenPendingAndCreator()
    {
        var service = GetRequestService();
        var user = await CreateTestUserAsync();
        var request = CreateValidRequest();

        var createResult = await service.CreateAsync(request, user.Id);
        var result = await service.DeleteAsync(createResult.Data!.ApiKey, user.Id);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        var getResult = await service.GetByApiKeyAsync(createResult.Data.ApiKey);
        Assert.Null(getResult.Data);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsValidationFailure_WhenNotPending()
    {
        var service = GetRequestService();
        var user = await CreateTestUserAsync();
        var request = CreateValidRequest();

        var createResult = await service.CreateAsync(request, user.Id);
        await service.CompleteAsync(createResult.Data!.ApiKey, user.Id);

        var result = await service.DeleteAsync(createResult.Data.ApiKey, user.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.ValidationFailure, result.Type);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsAccessDenied_WhenNonCreatorDeletes()
    {
        var service = GetRequestService();
        var creator = await CreateTestUserAsync();
        var otherUser = await CreateTestUserAsync();
        var request = CreateValidRequest();

        var createResult = await service.CreateAsync(request, creator.Id);
        var result = await service.DeleteAsync(createResult.Data!.ApiKey, otherUser.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.AccessDenied, result.Type);
    }

    #endregion

    #region GetCommentCountAsync Tests

    [Fact]
    public async Task GetCommentCountAsync_ReturnsZero_WhenNoComments()
    {
        var service = GetRequestService();
        var user = await CreateTestUserAsync();
        var request = CreateValidRequest();

        var createResult = await service.CreateAsync(request, user.Id);
        var count = await service.GetCommentCountAsync(createResult.Data!.Id);

        Assert.Equal(0, count);
    }

    #endregion
}

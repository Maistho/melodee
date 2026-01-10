using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums.PartyMode;
using Melodee.Common.Models;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Tests.Common.Services;

public class PartySessionServiceTests : ServiceTestBase
{
    private IDbContextFactory<MelodeeDbContext> _dbContextFactory = null!;
    private IMelodeeConfigurationFactory _configurationFactory = null!;
    private PartySessionService _service = null!;

    [SetUp]
    public void Setup()
    {
        _dbContextFactory = MockFactory();
        _configurationFactory = MockConfigurationFactory();
        _service = new PartySessionService(Logger, CacheManager, _dbContextFactory, _configurationFactory);
    }

    [Test]
    public async Task CreateSession_ShouldCreateSessionWithOwnerAsParticipant()
    {
        // Arrange
        var userId = 1;
        var sessionName = "Test Party";

        // Act
        var result = await _service.CreateAsync(sessionName, userId, null);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Name, Is.EqualTo(sessionName));
        Assert.That(result.Data.OwnerUserId, Is.EqualTo(userId));
        Assert.That(result.Data.Status, Is.EqualTo(PartySessionStatus.Active));
        Assert.That(result.Data.JoinCodeHash, Is.Null);
        Assert.That(result.Data.QueueRevision, Is.EqualTo(1));
        Assert.That(result.Data.PlaybackRevision, Is.EqualTo(1));

        // Verify participant was created
        await using var context = new MelodeeDbContext(_dbContextOptions);
        var participant = await context.PartySessionParticipants
            .FirstOrDefaultAsync(p => p.PartySessionId == result.Data.Id && p.UserId == userId);
        Assert.That(participant, Is.Not.Null);
        Assert.That(participant!.Role, Is.EqualTo(PartyRole.Owner));

        // Verify playback state was created
        var playbackState = await context.PartyPlaybackStates
            .FirstOrDefaultAsync(p => p.PartySessionId == result.Data.Id);
        Assert.That(playbackState, Is.Not.Null);
    }

    [Test]
    public async Task CreateSession_WithJoinCode_ShouldHashTheCode()
    {
        // Arrange
        var userId = 1;
        var sessionName = "Private Party";
        var joinCode = "secret123";

        // Act
        var result = await _service.CreateAsync(sessionName, userId, joinCode);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.JoinCodeHash, Is.Not.Null);
        Assert.That(result.Data.JoinCodeHash, Is.Not.EqualTo(joinCode));
        Assert.That(result.Data.JoinCodeHash!.Length, Is.GreaterThan(joinCode.Length));
    }

    [Test]
    public async Task GetSession_ShouldReturnSession_WhenExists()
    {
        // Arrange
        var userId = 1;
        var createResult = await _service.CreateAsync("Test Session", userId, null);
        AssertResultIsSuccessful(createResult);

        // Act
        var getResult = await _service.GetAsync(createResult.Data!.ApiKey);

        // Assert
        AssertResultIsSuccessful(getResult);
        Assert.That(getResult.Data, Is.Not.Null);
        Assert.That(getResult.Data!.Name, Is.EqualTo("Test Session"));
    }

    [Test]
    public async Task GetSession_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentApiKey = Guid.NewGuid();

        // Act
        var result = await _service.GetAsync(nonExistentApiKey);

        // Assert
        Assert.That(result.Data, Is.Null);
    }

    [Test]
    public async Task JoinSession_ShouldAddParticipant()
    {
        // Arrange
        var ownerId = 1;
        var sessionResult = await _service.CreateAsync("Test Session", ownerId, null);
        AssertResultIsSuccessful(sessionResult);

        var joinerId = 2;

        // Act
        var joinResult = await _service.JoinAsync(sessionResult.Data!.ApiKey, joinerId, null);

        // Assert
        AssertResultIsSuccessful(joinResult);
        Assert.That(joinResult.Data, Is.Not.Null);
        Assert.That(joinResult.Data!.UserId, Is.EqualTo(joinerId));
        Assert.That(joinResult.Data.Role, Is.EqualTo(PartyRole.Listener));
    }

    [Test]
    public async Task JoinSession_WithCorrectJoinCode_ShouldSucceed()
    {
        // Arrange
        var ownerId = 1;
        var joinCode = "secret123";
        var sessionResult = await _service.CreateAsync("Private Session", ownerId, joinCode);
        AssertResultIsSuccessful(sessionResult);

        var joinerId = 2;

        // Act
        var joinResult = await _service.JoinAsync(sessionResult.Data!.ApiKey, joinerId, joinCode);

        // Assert
        AssertResultIsSuccessful(joinResult);
    }

    [Test]
    public async Task JoinSession_WithIncorrectJoinCode_ShouldFail()
    {
        // Arrange
        var ownerId = 1;
        var joinCode = "secret123";
        var sessionResult = await _service.CreateAsync("Private Session", ownerId, joinCode);
        AssertResultIsSuccessful(sessionResult);

        var joinerId = 2;

        // Act
        var joinResult = await _service.JoinAsync(sessionResult.Data!.ApiKey, joinerId, "wrongcode");

        // Assert
        Assert.That(joinResult.IsSuccess, Is.False);
        Assert.That(joinResult.Type, Is.EqualTo(OperationResponseType.Unauthorized));
    }

    [Test]
    public async Task JoinSession_AlreadyParticipant_ShouldReturnExistingParticipant()
    {
        // Arrange
        var ownerId = 1;
        var sessionResult = await _service.CreateAsync("Test Session", ownerId, null);
        AssertResultIsSuccessful(sessionResult);

        // Act - owner joins their own session
        var joinResult = await _service.JoinAsync(sessionResult.Data!.ApiKey, ownerId, null);

        // Assert
        AssertResultIsSuccessful(joinResult);
        Assert.That(joinResult.Data!.Role, Is.EqualTo(PartyRole.Owner));
    }

    [Test]
    public async Task LeaveSession_ShouldRemoveParticipant()
    {
        // Arrange
        var ownerId = 1;
        var joinerId = 2;
        var sessionResult = await _service.CreateAsync("Test Session", ownerId, null);
        AssertResultIsSuccessful(sessionResult);

        await _service.JoinAsync(sessionResult.Data!.ApiKey, joinerId, null);

        // Act
        var leaveResult = await _service.LeaveAsync(sessionResult.Data!.ApiKey, joinerId);

        // Assert
        AssertResultIsSuccessful(leaveResult);

        await using var context = new MelodeeDbContext(_dbContextOptions);
        var participant = await context.PartySessionParticipants
            .FirstOrDefaultAsync(p => p.PartySessionId == sessionResult.Data!.Id && p.UserId == joinerId);
        Assert.That(participant, Is.Null);
    }

    [Test]
    public async Task LeaveSession_Owner_ShouldFail()
    {
        // Arrange
        var ownerId = 1;
        var sessionResult = await _service.CreateAsync("Test Session", ownerId, null);
        AssertResultIsSuccessful(sessionResult);

        // Act
        var leaveResult = await _service.LeaveAsync(sessionResult.Data!.ApiKey, ownerId);

        // Assert
        Assert.That(leaveResult.IsSuccess, Is.False);
        Assert.That(leaveResult.Type, Is.EqualTo(OperationResponseType.BadRequest));
    }

    [Test]
    public async Task EndSession_ShouldSetStatusToEnded()
    {
        // Arrange
        var ownerId = 1;
        var sessionResult = await _service.CreateAsync("Test Session", ownerId, null);
        AssertResultIsSuccessful(sessionResult);

        // Act
        var endResult = await _service.EndAsync(sessionResult.Data!.ApiKey, ownerId);

        // Assert
        AssertResultIsSuccessful(endResult);

        await using var context = new MelodeeDbContext(_dbContextOptions);
        var session = await context.PartySessions.FindAsync(sessionResult.Data!.Id);
        Assert.That(session!.Status, Is.EqualTo(PartySessionStatus.Ended));
    }

    [Test]
    public async Task EndSession_NonOwner_ShouldFail()
    {
        // Arrange
        var ownerId = 1;
        var joinerId = 2;
        var sessionResult = await _service.CreateAsync("Test Session", ownerId, null);
        AssertResultIsSuccessful(sessionResult);

        await _service.JoinAsync(sessionResult.Data!.ApiKey, joinerId, null);

        // Act
        var endResult = await _service.EndAsync(sessionResult.Data!.ApiKey, joinerId);

        // Assert
        Assert.That(endResult.IsSuccess, Is.False);
        Assert.That(endResult.Type, Is.EqualTo(OperationResponseType.Forbidden));
    }

    [Test]
    public async Task GetParticipants_ShouldReturnAllParticipants()
    {
        // Arrange
        var ownerId = 1;
        var joinerId = 2;
        var sessionResult = await _service.CreateAsync("Test Session", ownerId, null);
        AssertResultIsSuccessful(sessionResult);

        await _service.JoinAsync(sessionResult.Data!.ApiKey, joinerId, null);

        // Act
        var participantsResult = await _service.GetParticipantsAsync(sessionResult.Data!.ApiKey);

        // Assert
        AssertResultIsSuccessful(participantsResult);
        Assert.That(participantsResult.Data, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetUserRole_ShouldReturnCorrectRole()
    {
        // Arrange
        var ownerId = 1;
        var joinerId = 2;
        var sessionResult = await _service.CreateAsync("Test Session", ownerId, null);
        AssertResultIsSuccessful(sessionResult);

        await _service.JoinAsync(sessionResult.Data!.ApiKey, joinerId, null);

        // Act
        var ownerRole = await _service.GetUserRoleAsync(sessionResult.Data!.ApiKey, ownerId);
        var joinerRole = await _service.GetUserRoleAsync(sessionResult.Data!.ApiKey, joinerId);

        // Assert
        AssertResultIsSuccessful(ownerRole);
        AssertResultIsSuccessful(joinerRole);
        Assert.That(ownerRole.Data, Is.EqualTo(PartyRole.Owner));
        Assert.That(joinerRole.Data, Is.EqualTo(PartyRole.Listener));
    }
}

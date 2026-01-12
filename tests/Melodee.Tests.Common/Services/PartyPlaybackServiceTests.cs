using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Melodee.Tests.Common.Services;

public class PartyPlaybackServiceTests : ServiceTestBase
{
    private IDbContextFactory<MelodeeDbContext> _dbContextFactory = null!;
    private IMelodeeConfigurationFactory _configurationFactory = null!;
    private PartySessionService _sessionService = null!;
    private PartyQueueService _queueService = null!;
    private PartyPlaybackService _playbackService = null!;

    private async Task<PartySession> CreateTestSession(int userId = 1)
    {
        var sessionResult = await _sessionService.CreateAsync("Test Session", userId, null);
        AssertResultIsSuccessful(sessionResult);
        return sessionResult.Data!;
    }

    [SetUp]
    public void Setup()
    {
        _dbContextFactory = MockFactory();
        _configurationFactory = MockConfigurationFactory();
        _sessionService = new PartySessionService(Logger, CacheManager, _dbContextFactory, _configurationFactory);
        _queueService = new PartyQueueService(Logger, CacheManager, _dbContextFactory, _configurationFactory);
        _playbackService = new PartyPlaybackService(Logger, CacheManager, _dbContextFactory, _configurationFactory);
    }

    [Test]
    public async Task GetPlaybackState_ShouldReturnInitialState()
    {
        // Arrange
        var session = await CreateTestSession();

        // Act
        var result = await _playbackService.GetPlaybackStateAsync(session.ApiKey);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.PositionSeconds, Is.EqualTo(0));
        Assert.That(result.Data.IsPlaying, Is.False);
        Assert.That(result.Data.Volume, Is.Null);
        Assert.That(result.Data.CurrentQueueItemApiKey, Is.Null);
    }

    [Test]
    public async Task UpdateFromHeartbeat_ShouldUpdatePlaybackState()
    {
        // Arrange
        var session = await CreateTestSession();
        var queueItemApiKey = Guid.NewGuid();

        // Act
        var result = await _playbackService.UpdateFromHeartbeatAsync(
            session.ApiKey,
            queueItemApiKey,
            45.5,
            true,
            0.7,
            1
        );

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.PositionSeconds, Is.EqualTo(45.5));
        Assert.That(result.Data.IsPlaying, Is.True);
        Assert.That(result.Data.Volume, Is.EqualTo(0.7));
        Assert.That(result.Data.CurrentQueueItemApiKey, Is.EqualTo(queueItemApiKey));
        Assert.That(result.Data.LastHeartbeatAt, Is.Not.Null);
        Assert.That(result.Data.UpdatedByUserId, Is.EqualTo(1));
    }

    [Test]
    public async Task SetCurrentItem_ShouldChangeCurrentItem()
    {
        // Arrange
        var session = await CreateTestSession();
        var queueItemApiKey = Guid.NewGuid();

        // Act
        var result = await _playbackService.SetCurrentItemAsync(session.ApiKey, queueItemApiKey);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.CurrentQueueItemApiKey, Is.EqualTo(queueItemApiKey));
        Assert.That(result.Data.PositionSeconds, Is.EqualTo(0));
        Assert.That(result.Data.IsPlaying, Is.False);
    }

    [Test]
    public async Task UpdateIntent_Play_ShouldStartPlayback()
    {
        // Arrange
        var session = await CreateTestSession();

        // Act
        var result = await _playbackService.UpdateIntentAsync(
            session.ApiKey,
            PlaybackIntent.Play,
            null,
            1,
            session.PlaybackRevision
        );

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data!.IsPlaying, Is.True);
        Assert.That(result.Data.UpdatedByUserId, Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateIntent_Play_WithPosition_ShouldStartPlaybackAtPosition()
    {
        // Arrange
        var session = await CreateTestSession();

        // Act
        var result = await _playbackService.UpdateIntentAsync(
            session.ApiKey,
            PlaybackIntent.Play,
            30.0,
            1,
            session.PlaybackRevision
        );

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data!.IsPlaying, Is.True);
        Assert.That(result.Data.PositionSeconds, Is.EqualTo(30.0));
    }

    [Test]
    public async Task UpdateIntent_Pause_ShouldPausePlayback()
    {
        // Arrange
        var session = await CreateTestSession();
        await _playbackService.UpdateIntentAsync(session.ApiKey, PlaybackIntent.Play, null, 1, session.PlaybackRevision);

        // Act
        var result = await _playbackService.UpdateIntentAsync(
            session.ApiKey,
            PlaybackIntent.Pause,
            45.0,
            1,
            session.PlaybackRevision + 1
        );

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data!.IsPlaying, Is.False);
        Assert.That(result.Data.PositionSeconds, Is.EqualTo(45.0));
    }

    [Test]
    public async Task UpdateIntent_Skip_ShouldMoveToNextTrack()
    {
        // Arrange
        var session = await CreateTestSession();
        var songApiKeys = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        await _queueService.AddItemsAsync(session.ApiKey, songApiKeys, 1, "album", session.QueueRevision);

        // Set first item as current
        await _playbackService.SetCurrentItemAsync(session.ApiKey, songApiKeys[0]);

        // Act
        var result = await _playbackService.UpdateIntentAsync(
            session.ApiKey,
            PlaybackIntent.Skip,
            null,
            1,
            session.PlaybackRevision + 1
        );

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data!.CurrentQueueItemApiKey, Is.EqualTo(songApiKeys[1]));
        Assert.That(result.Data.PositionSeconds, Is.EqualTo(0));
        Assert.That(result.Data.IsPlaying, Is.True);
    }

    [Test]
    public async Task UpdateIntent_Skip_EmptyQueue_ShouldStopPlayback()
    {
        // Arrange
        var session = await CreateTestSession();

        // Act
        var result = await _playbackService.UpdateIntentAsync(
            session.ApiKey,
            PlaybackIntent.Skip,
            null,
            1,
            session.PlaybackRevision
        );

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data!.CurrentQueueItemApiKey, Is.Null);
        Assert.That(result.Data.IsPlaying, Is.True);
    }

    [Test]
    public async Task UpdateIntent_Seek_ShouldChangePosition()
    {
        // Arrange
        var session = await CreateTestSession();

        // Act
        var result = await _playbackService.UpdateIntentAsync(
            session.ApiKey,
            PlaybackIntent.Seek,
            120.5,
            1,
            session.PlaybackRevision
        );

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data!.PositionSeconds, Is.EqualTo(120.5));
    }

    [Test]
    public async Task UpdateIntent_Seek_NegativePosition_ShouldFail()
    {
        // Arrange
        var session = await CreateTestSession();

        // Act
        var result = await _playbackService.UpdateIntentAsync(
            session.ApiKey,
            PlaybackIntent.Seek,
            -10.0,
            1,
            session.PlaybackRevision
        );

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Type, Is.EqualTo(OperationResponseType.BadRequest));
    }

    [Test]
    public async Task UpdateIntent_WithWrongRevision_ShouldFail()
    {
        // Arrange
        var session = await CreateTestSession();

        // Act - try to update with wrong revision
        var result = await _playbackService.UpdateIntentAsync(
            session.ApiKey,
            PlaybackIntent.Play,
            null,
            1,
            999 // wrong revision
        );

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Type, Is.EqualTo(OperationResponseType.Conflict));
    }

    [Test]
    public async Task PlaybackIntent_ShouldIncrementPlaybackRevision()
    {
        // Arrange
        var session = await CreateTestSession();

        // Act
        var playResult = await _playbackService.UpdateIntentAsync(
            session.ApiKey,
            PlaybackIntent.Play,
            null,
            1,
            session.PlaybackRevision
        );

        // Assert
        AssertResultIsSuccessful(playResult);
        Assert.That(playResult.Data!.ApiKey, Is.Not.Null);
    }

    [Test]
    public async Task FullPlaybackWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        var session = await CreateTestSession();
        var songApiKeys = new[] { Guid.NewGuid(), Guid.NewGuid() };
        await _queueService.AddItemsAsync(session.ApiKey, songApiKeys, 1, "album", session.QueueRevision);

        // Act & Assert
        // Start with first track
        var setResult = await _playbackService.SetCurrentItemAsync(session.ApiKey, songApiKeys[0]);
        AssertResultIsSuccessful(setResult);

        // Start playback
        var playResult = await _playbackService.UpdateIntentAsync(
            session.ApiKey,
            PlaybackIntent.Play,
            0,
            1,
            session.PlaybackRevision + 1
        );
        AssertResultIsSuccessful(playResult);
        Assert.That(playResult.Data!.IsPlaying, Is.True);

        // Seek to middle
        var seekResult = await _playbackService.UpdateIntentAsync(
            session.ApiKey,
            PlaybackIntent.Seek,
            60.0,
            1,
            session.PlaybackRevision + 2
        );
        AssertResultIsSuccessful(seekResult);
        Assert.That(seekResult.Data!.PositionSeconds, Is.EqualTo(60.0));

        // Skip to next
        var skipResult = await _playbackService.UpdateIntentAsync(
            session.ApiKey,
            PlaybackIntent.Skip,
            null,
            1,
            session.PlaybackRevision + 3
        );
        AssertResultIsSuccessful(skipResult);
        Assert.That(skipResult.Data!.CurrentQueueItemApiKey, Is.EqualTo(songApiKeys[1]));

        // Pause
        var pauseResult = await _playbackService.UpdateIntentAsync(
            session.ApiKey,
            PlaybackIntent.Pause,
            30.0,
            1,
            session.PlaybackRevision + 4
        );
        AssertResultIsSuccessful(pauseResult);
        Assert.That(pauseResult.Data!.IsPlaying, Is.False);
    }
}

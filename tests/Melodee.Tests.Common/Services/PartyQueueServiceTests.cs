using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Melodee.Tests.Common.Services;

public class PartyQueueServiceTests : ServiceTestBase
{
    private IDbContextFactory<MelodeeDbContext> _dbContextFactory = null!;
    private IMelodeeConfigurationFactory _configurationFactory = null!;
    private PartySessionService _sessionService = null!;
    private PartyQueueService _queueService = null!;

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
    }

    [Test]
    public async Task GetQueue_ShouldReturnEmptyQueue_WhenSessionIsNew()
    {
        // Arrange
        var session = await CreateTestSession();

        // Act
        var result = await _queueService.GetQueueAsync(session.ApiKey);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data.Revision, Is.EqualTo(1));
        Assert.That(result.Data.Items, Is.Empty);
    }

    [Test]
    public async Task AddItems_ShouldAddItemsToQueue()
    {
        // Arrange
        var session = await CreateTestSession();
        var songApiKeys = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var result = await _queueService.AddItemsAsync(
            session.ApiKey,
            songApiKeys,
            1,
            "album",
            session.QueueRevision
        );

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data.AddedItems, Has.Length.EqualTo(3));
        Assert.That(result.Data.NewRevision, Is.EqualTo(2));

        await using var context = new MelodeeDbContext(_dbContextOptions);
        var queueItems = await context.PartyQueueItems
            .Where(x => x.PartySessionId == session.Id)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        Assert.That(queueItems, Has.Count.EqualTo(3));
        Assert.That(queueItems[0].SongApiKey, Is.EqualTo(songApiKeys[0]));
        Assert.That(queueItems[0].SortOrder, Is.EqualTo(0));
        Assert.That(queueItems[1].SortOrder, Is.EqualTo(1));
        Assert.That(queueItems[2].SortOrder, Is.EqualTo(2));
    }

    [Test]
    public async Task AddItems_WithWrongRevision_ShouldFail()
    {
        // Arrange
        var session = await CreateTestSession();
        var songApiKeys = new[] { Guid.NewGuid() };

        // Act - try to add with wrong revision
        var result = await _queueService.AddItemsAsync(
            session.ApiKey,
            songApiKeys,
            1,
            "album",
            999 // wrong revision
        );

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Type, Is.EqualTo(OperationResponseType.Conflict));
    }

    [Test]
    public async Task RemoveItem_ShouldRemoveItemAndReorder()
    {
        // Arrange
        var session = await CreateTestSession();
        var songApiKeys = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var addResult = await _queueService.AddItemsAsync(session.ApiKey, songApiKeys, 1, "album", session.QueueRevision);
        AssertResultIsSuccessful(addResult);

        // Act - remove the middle item
        var items = addResult.Data.AddedItems.ToArray();
        var result = await _queueService.RemoveItemAsync(session.ApiKey, items[1].ApiKey, 1, addResult.Data.NewRevision);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data, Is.EqualTo(3)); // New revision

        await using var context = new MelodeeDbContext(_dbContextOptions);
        var remainingItems = await context.PartyQueueItems
            .Where(x => x.PartySessionId == session.Id)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        Assert.That(remainingItems, Has.Count.EqualTo(2));
        Assert.That(remainingItems[0].SongApiKey, Is.EqualTo(songApiKeys[0]));
        Assert.That(remainingItems[0].SortOrder, Is.EqualTo(0));
        Assert.That(remainingItems[1].SongApiKey, Is.EqualTo(songApiKeys[2]));
        Assert.That(remainingItems[1].SortOrder, Is.EqualTo(1));
    }

    [Test]
    public async Task RemoveItem_NonExistent_ShouldFail()
    {
        // Arrange
        var session = await CreateTestSession();

        // Act
        var result = await _queueService.RemoveItemAsync(session.ApiKey, Guid.NewGuid(), 1, session.QueueRevision);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Type, Is.EqualTo(OperationResponseType.NotFound));
    }

    [Test]
    public async Task ReorderItem_ShouldChangeItemPosition()
    {
        // Arrange
        var session = await CreateTestSession();
        var songApiKeys = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var addResult = await _queueService.AddItemsAsync(session.ApiKey, songApiKeys, 1, "album", session.QueueRevision);
        AssertResultIsSuccessful(addResult);

        // Act - move first item to third position
        var items = addResult.Data.AddedItems.ToArray();
        var result = await _queueService.ReorderItemAsync(session.ApiKey, items[0].ApiKey, 2, 1, addResult.Data.NewRevision);

        // Assert
        AssertResultIsSuccessful(result);

        await using var context = new MelodeeDbContext(_dbContextOptions);
        var orderedItems = await context.PartyQueueItems
            .Where(x => x.PartySessionId == session.Id)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        Assert.That(orderedItems[0].SongApiKey, Is.EqualTo(songApiKeys[1]));
        Assert.That(orderedItems[1].SongApiKey, Is.EqualTo(songApiKeys[2]));
        Assert.That(orderedItems[2].SongApiKey, Is.EqualTo(songApiKeys[0]));
    }

    [Test]
    public async Task ReorderItem_InvalidIndex_ShouldClampToValidRange()
    {
        // Arrange
        var session = await CreateTestSession();
        var songApiKeys = new[] { Guid.NewGuid() };
        var addResult = await _queueService.AddItemsAsync(session.ApiKey, songApiKeys, 1, "album", session.QueueRevision);
        AssertResultIsSuccessful(addResult);

        // Act - try to move to negative index
        var item = addResult.Data.AddedItems.First();
        var result = await _queueService.ReorderItemAsync(session.ApiKey, item.ApiKey, -5, 1, addResult.Data.NewRevision);

        // Assert
        AssertResultIsSuccessful(result);

        await using var context = new MelodeeDbContext(_dbContextOptions);
        var orderedItems = await context.PartyQueueItems
            .Where(x => x.PartySessionId == session.Id)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        Assert.That(orderedItems[0].SortOrder, Is.EqualTo(0));
    }

    [Test]
    public async Task Clear_ShouldRemoveAllItems()
    {
        // Arrange
        var session = await CreateTestSession();
        var songApiKeys = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var addResult = await _queueService.AddItemsAsync(session.ApiKey, songApiKeys, 1, "album", session.QueueRevision);
        AssertResultIsSuccessful(addResult);

        // Act
        var result = await _queueService.ClearAsync(session.ApiKey, 1, addResult.Data.NewRevision);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data, Is.EqualTo(3)); // New revision

        await using var context = new MelodeeDbContext(_dbContextOptions);
        var remainingItems = await context.PartyQueueItems
            .Where(x => x.PartySessionId == session.Id)
            .ToListAsync();

        Assert.That(remainingItems, Is.Empty);
    }

    [Test]
    public async Task GetNextItem_ShouldReturnFirstItemInQueue()
    {
        // Arrange
        var session = await CreateTestSession();
        var songApiKeys = new[] { Guid.NewGuid(), Guid.NewGuid() };
        await _queueService.AddItemsAsync(session.ApiKey, songApiKeys, 1, "album", session.QueueRevision);

        // Act
        var result = await _queueService.GetNextItemAsync(session.ApiKey);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.SongApiKey, Is.EqualTo(songApiKeys[0]));
    }

    [Test]
    public async Task GetNextItem_EmptyQueue_ShouldReturnNull()
    {
        // Arrange
        var session = await CreateTestSession();

        // Act
        var result = await _queueService.GetNextItemAsync(session.ApiKey);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.That(result.Data, Is.Null);
    }

    [Test]
    public async Task MultipleOperations_ShouldMaintainRevision()
    {
        // Arrange
        var session = await CreateTestSession();

        // Act & Assert
        var revision = session.QueueRevision;

        // Add items
        var addResult = await _queueService.AddItemsAsync(session.ApiKey, [Guid.NewGuid()], 1, "album", revision);
        AssertResultIsSuccessful(addResult);
        revision = addResult.Data.NewRevision;

        // Reorder
        var item = addResult.Data.AddedItems.First();
        var reorderResult = await _queueService.ReorderItemAsync(session.ApiKey, item.ApiKey, 0, 1, revision);
        AssertResultIsSuccessful(reorderResult);
        revision = reorderResult.Data;

        // Clear
        var clearResult = await _queueService.ClearAsync(session.ApiKey, 1, revision);
        AssertResultIsSuccessful(clearResult);
        Assert.That(clearResult.Data, Is.EqualTo(3)); // Should be revision 3
    }
}

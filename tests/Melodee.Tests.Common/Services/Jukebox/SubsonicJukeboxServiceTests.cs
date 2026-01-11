using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Services;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.Jukebox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;

namespace Melodee.Tests.Common.Services.Jukebox;

public class SubsonicJukeboxServiceTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<ICacheManager> _cacheManagerMock;
    private readonly Mock<IDbContextFactory<MelodeeDbContext>> _contextFactoryMock;
    private readonly Mock<IOptions<JukeboxOptions>> _jukeboxOptionsMock;
    private readonly Mock<IPartyQueueService> _partyQueueServiceMock;
    private readonly Mock<IPartyPlaybackService> _partyPlaybackServiceMock;

    public SubsonicJukeboxServiceTests()
    {
        _loggerMock = new Mock<ILogger>();
        _cacheManagerMock = new Mock<ICacheManager>();
        _contextFactoryMock = new Mock<IDbContextFactory<MelodeeDbContext>>();
        _jukeboxOptionsMock = new Mock<IOptions<JukeboxOptions>>();
        _jukeboxOptionsMock.Setup(x => x.Value).Returns(new JukeboxOptions { Enabled = false });
        _partyQueueServiceMock = new Mock<IPartyQueueService>();
        _partyPlaybackServiceMock = new Mock<IPartyPlaybackService>();
    }

    private SubsonicJukeboxService CreateService()
    {
        return new SubsonicJukeboxService(
            _loggerMock.Object,
            _cacheManagerMock.Object,
            _contextFactoryMock.Object,
            _jukeboxOptionsMock.Object,
            _partyQueueServiceMock.Object,
            _partyPlaybackServiceMock.Object);
    }

    [Fact]
    public async Task GetStatusAsync_WhenJukeboxDisabled_ReturnsBadRequest()
    {
        var service = CreateService();

        var result = await service.GetStatusAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.BadRequest, result.Type);
    }

    [Fact]
    public async Task GetPlaylistAsync_WhenJukeboxDisabled_ReturnsBadRequest()
    {
        var service = CreateService();

        var result = await service.GetPlaylistAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.BadRequest, result.Type);
    }

    [Fact]
    public async Task SetGainAsync_WhenJukeboxDisabled_ReturnsBadRequest()
    {
        var service = CreateService();

        var result = await service.SetGainAsync(0.8);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.BadRequest, result.Type);
    }

    [Fact]
    public async Task StartAsync_WhenJukeboxDisabled_ReturnsBadRequest()
    {
        var service = CreateService();

        var result = await service.StartAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.BadRequest, result.Type);
    }

    [Fact]
    public async Task StopAsync_WhenJukeboxDisabled_ReturnsBadRequest()
    {
        var service = CreateService();

        var result = await service.StopAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.BadRequest, result.Type);
    }

    [Fact]
    public async Task SkipAsync_WhenJukeboxDisabled_ReturnsBadRequest()
    {
        var service = CreateService();

        var result = await service.SkipAsync(1, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.BadRequest, result.Type);
    }

    [Fact]
    public async Task AddAsync_WhenJukeboxDisabled_ReturnsBadRequest()
    {
        var service = CreateService();

        var result = await service.AddAsync(new[] { "song1-id" });

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.BadRequest, result.Type);
    }

    [Fact]
    public async Task ClearAsync_WhenJukeboxDisabled_ReturnsBadRequest()
    {
        var service = CreateService();

        var result = await service.ClearAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.BadRequest, result.Type);
    }

    [Fact]
    public async Task RemoveAsync_WhenJukeboxDisabled_ReturnsBadRequest()
    {
        var service = CreateService();

        var result = await service.RemoveAsync(0);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.BadRequest, result.Type);
    }

    [Fact]
    public async Task ShuffleAsync_WhenJukeboxDisabled_ReturnsBadRequest()
    {
        var service = CreateService();

        var result = await service.ShuffleAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.BadRequest, result.Type);
    }
}

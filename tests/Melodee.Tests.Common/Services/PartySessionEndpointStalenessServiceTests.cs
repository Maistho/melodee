using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums.PartyMode;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;

namespace Melodee.Tests.Common.Services;

/// <summary>
/// Tests for PartySessionEndpointStalenessService.
/// </summary>
public class PartySessionEndpointStalenessServiceTests : TestsBase
{
    private static PartySessionEndpoint CreateEndpoint(Instant? lastSeenAt, int id = 1)
    {
        return new PartySessionEndpoint
        {
            Id = id,
            Name = "Test Endpoint",
            Type = PartySessionEndpointType.WebPlayer,
            ApiKey = Guid.NewGuid(),
            LastSeenAt = lastSeenAt,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
    }

    [Fact]
    public void IsStale_ReturnsTrue_WhenLastSeenAtIsNull()
    {
        var loggerMock = new Mock<ILogger<PartySessionEndpointStalenessService>>();
        var contextFactoryMock = new Mock<IDbContextFactory<MelodeeDbContext>>();

        var service = new PartySessionEndpointStalenessService(loggerMock.Object, contextFactoryMock.Object);
        var endpoint = CreateEndpoint(null);

        Assert.True(service.IsStale(endpoint));
    }

    [Fact]
    public void IsStale_ReturnsFalse_WhenLastSeenAtIsRecent()
    {
        var loggerMock = new Mock<ILogger<PartySessionEndpointStalenessService>>();
        var contextFactoryMock = new Mock<IDbContextFactory<MelodeeDbContext>>();

        var service = new PartySessionEndpointStalenessService(loggerMock.Object, contextFactoryMock.Object);
        var endpoint = CreateEndpoint(SystemClock.Instance.GetCurrentInstant());

        Assert.False(service.IsStale(endpoint));
    }

    [Fact]
    public void IsStale_ReturnsTrue_WhenLastSeenAtIsOld()
    {
        var loggerMock = new Mock<ILogger<PartySessionEndpointStalenessService>>();
        var contextFactoryMock = new Mock<IDbContextFactory<MelodeeDbContext>>();

        var service = new PartySessionEndpointStalenessService(loggerMock.Object, contextFactoryMock.Object);
        var oldTimestamp = SystemClock.Instance.GetCurrentInstant() - Duration.FromSeconds(60);
        var endpoint = CreateEndpoint(oldTimestamp);

        Assert.True(service.IsStale(endpoint));
    }

    [Fact]
    public void IsStale_ReturnsTrue_WhenLastSeenAtIsAtOrBeforeThreshold()
    {
        var loggerMock = new Mock<ILogger<PartySessionEndpointStalenessService>>();
        var contextFactoryMock = new Mock<IDbContextFactory<MelodeeDbContext>>();

        var service = new PartySessionEndpointStalenessService(loggerMock.Object, contextFactoryMock.Object);
        var thresholdTimestamp = SystemClock.Instance.GetCurrentInstant() - Duration.FromSeconds(30);
        var endpoint = CreateEndpoint(thresholdTimestamp);

        Assert.True(service.IsStale(endpoint));
    }

    [Fact]
    public void IsStale_ReturnsTrue_WhenLastSeenAtIsOneSecondBeforeThreshold()
    {
        var loggerMock = new Mock<ILogger<PartySessionEndpointStalenessService>>();
        var contextFactoryMock = new Mock<IDbContextFactory<MelodeeDbContext>>();

        var service = new PartySessionEndpointStalenessService(loggerMock.Object, contextFactoryMock.Object);
        var thresholdTimestamp = SystemClock.Instance.GetCurrentInstant() - Duration.FromSeconds(31);
        var endpoint = CreateEndpoint(thresholdTimestamp);

        Assert.True(service.IsStale(endpoint));
    }
}

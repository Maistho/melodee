using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;

namespace Melodee.Tests.Common.Services;

/// <summary>
/// Tests for PartySessionEndpointStalenessService.
/// </summary>
public class PartySessionEndpointStalenessServiceTests : TestsBase
{
    private static IMelodeeConfiguration NewPartyModeConfiguration(int staleSeconds = 30)
    {
        var settings = NewConfiguration();
        settings[PartyModeOptions.SectionName + ":" + nameof(PartyModeOptions.EndpointStaleSeconds)] = staleSeconds.ToString();
        return new MelodeeConfiguration(settings);
    }

    private static PartySessionEndpoint CreateEndpoint(Instant? lastSeenAt, int id = 1)
    {
        return new PartySessionEndpoint
        {
            Id = id,
            Name = "Test Endpoint",
            Type = EndpointType.WebPlayer,
            ApiKey = Guid.NewGuid(),
            LastSeenAt = lastSeenAt,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
    }

    [Fact]
    public void IsStale_ReturnsTrue_WhenLastSeenAtIsNull()
    {
        var loggerMock = new Mock<ILogger<PartySessionEndpointStalenessService>>();
        var configFactoryMock = new Mock<IMelodeeConfigurationFactory>();
        configFactoryMock.Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewPartyModeConfiguration());

        var contextFactoryMock = new Mock<IDbContextFactory<MelodeeDbContext>>();

        var service = new PartySessionEndpointStalenessService(loggerMock.Object, contextFactoryMock.Object, configFactoryMock.Object);
        var endpoint = CreateEndpoint(null);

        Assert.True(service.IsStale(endpoint));
    }

    [Fact]
    public void IsStale_ReturnsFalse_WhenLastSeenAtIsRecent()
    {
        var loggerMock = new Mock<ILogger<PartySessionEndpointStalenessService>>();
        var configFactoryMock = new Mock<IMelodeeConfigurationFactory>();
        configFactoryMock.Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewPartyModeConfiguration());

        var contextFactoryMock = new Mock<IDbContextFactory<MelodeeDbContext>>();

        var service = new PartySessionEndpointStalenessService(loggerMock.Object, contextFactoryMock.Object, configFactoryMock.Object);
        var endpoint = CreateEndpoint(SystemClock.Instance.GetCurrentInstant());

        Assert.False(service.IsStale(endpoint));
    }

    [Fact]
    public void IsStale_ReturnsTrue_WhenLastSeenAtIsOld()
    {
        var loggerMock = new Mock<ILogger<PartySessionEndpointStalenessService>>();
        var configFactoryMock = new Mock<IMelodeeConfigurationFactory>();
        configFactoryMock.Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewPartyModeConfiguration());

        var contextFactoryMock = new Mock<IDbContextFactory<MelodeeDbContext>>();

        var service = new PartySessionEndpointStalenessService(loggerMock.Object, contextFactoryMock.Object, configFactoryMock.Object);
        var oldTimestamp = SystemClock.Instance.GetCurrentInstant() - Duration.FromSeconds(60);
        var endpoint = CreateEndpoint(oldTimestamp);

        Assert.True(service.IsStale(endpoint));
    }

    [Fact]
    public void IsStale_ReturnsFalse_WhenLastSeenAtIsExactlyAtThreshold()
    {
        var loggerMock = new Mock<ILogger<PartySessionEndpointStalenessService>>();
        var configFactoryMock = new Mock<IMelodeeConfigurationFactory>();
        configFactoryMock.Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewPartyModeConfiguration());

        var contextFactoryMock = new Mock<IDbContextFactory<MelodeeDbContext>>();

        var service = new PartySessionEndpointStalenessService(loggerMock.Object, contextFactoryMock.Object, configFactoryMock.Object);
        var thresholdTimestamp = SystemClock.Instance.GetCurrentInstant() - TimeSpan.FromSeconds(30);
        var endpoint = CreateEndpoint(thresholdTimestamp);

        Assert.False(service.IsStale(endpoint));
    }

    [Fact]
    public void IsStale_ReturnsTrue_WhenLastSeenAtIsOneSecondBeforeThreshold()
    {
        var loggerMock = new Mock<ILogger<PartySessionEndpointStalenessService>>();
        var configFactoryMock = new Mock<IMelodeeConfigurationFactory>();
        configFactoryMock.Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewPartyModeConfiguration());

        var contextFactoryMock = new Mock<IDbContextFactory<MelodeeDbContext>>();

        var service = new PartySessionEndpointStalenessService(loggerMock.Object, contextFactoryMock.Object, configFactoryMock.Object);
        var thresholdTimestamp = SystemClock.Instance.GetCurrentInstant() - TimeSpan.FromSeconds(31);
        var endpoint = CreateEndpoint(thresholdTimestamp);

        Assert.True(service.IsStale(endpoint));
    }

    [Fact]
    public void IsStale_UsesCustomStaleThreshold()
    {
        var loggerMock = new Mock<ILogger<PartySessionEndpointStalenessService>>();
        var configFactoryMock = new Mock<IMelodeeConfigurationFactory>();
        configFactoryMock.Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewPartyModeConfiguration(staleSeconds: 60));

        var contextFactoryMock = new Mock<IDbContextFactory<MelodeeDbContext>>();

        var service = new PartySessionEndpointStalenessService(loggerMock.Object, contextFactoryMock.Object, configFactoryMock.Object);
        var endpoint = CreateEndpoint(SystemClock.Instance.GetCurrentInstant() - Duration.FromSeconds(45));

        Assert.False(service.IsStale(endpoint));
    }
}

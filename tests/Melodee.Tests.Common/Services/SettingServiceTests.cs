using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Services;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;
using Serilog;

namespace Melodee.Tests.Common.Services;

public class SettingServiceTests : IDisposable
{
    private readonly DbContextOptions<MelodeeDbContext> _dbContextOptions;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<ICacheManager> _cacheManagerMock;
    private readonly Mock<IMelodeeConfigurationFactory> _configFactoryMock;

    public SettingServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<MelodeeDbContext>()
            .UseInMemoryDatabase(databaseName: $"SettingServiceTest_{Guid.NewGuid()}")
            .Options;

        _loggerMock = new Mock<ILogger>();
        _cacheManagerMock = new Mock<ICacheManager>();
        _configFactoryMock = new Mock<IMelodeeConfigurationFactory>();
    }

    public void Dispose()
    {
        using var context = new MelodeeDbContext(_dbContextOptions);
        context.Database.EnsureDeleted();
    }

    [Fact]
    public async Task UpdateAsync_ResetsConfigurationFactory()
    {
        // Arrange
        await SeedTestSettingAsync();
        var service = new SettingService(
            _loggerMock.Object,
            _cacheManagerMock.Object,
            _configFactoryMock.Object,
            CreateContextFactory());

        Setting settingToUpdate;
        await using (var context = new MelodeeDbContext(_dbContextOptions))
        {
            settingToUpdate = await context.Settings.FirstAsync();
            settingToUpdate.Value = "Updated Value";
        }

        // Act
        var result = await service.UpdateAsync(settingToUpdate);

        // Assert
        Assert.True(result.IsSuccess);
        _configFactoryMock.Verify(f => f.Reset(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_TriggersConfigurationChangedEvent()
    {
        // Arrange
        await SeedTestSettingAsync();
        var eventRaised = false;

        _configFactoryMock
            .Setup(f => f.Reset())
            .Callback(() =>
            {
                // Simulate event being raised
                _configFactoryMock.Raise(f => f.ConfigurationChanged += null, EventArgs.Empty);
                eventRaised = true;
            });

        var service = new SettingService(
            _loggerMock.Object,
            _cacheManagerMock.Object,
            _configFactoryMock.Object,
            CreateContextFactory());

        Setting settingToUpdate;
        await using (var context = new MelodeeDbContext(_dbContextOptions))
        {
            settingToUpdate = await context.Settings.FirstAsync();
            settingToUpdate.Value = "Updated Value";
        }

        // Act
        var result = await service.UpdateAsync(settingToUpdate);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(eventRaised, "ConfigurationChanged event should be raised after Reset is called");
    }

    [Fact]
    public async Task UpdateAsync_DoesNotResetConfiguration_WhenUpdateFails()
    {
        // Arrange
        var service = new SettingService(
            _loggerMock.Object,
            _cacheManagerMock.Object,
            _configFactoryMock.Object,
            CreateContextFactory());

        var nonExistentSetting = new Setting
        {
            Id = 99999,
            Key = "non.existent",
            Value = "value",
            ApiKey = Guid.NewGuid(),
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        // Act
        var result = await service.UpdateAsync(nonExistentSetting);

        // Assert
        Assert.False(result.IsSuccess);
        _configFactoryMock.Verify(f => f.Reset(), Times.Never);
    }

    [Fact]
    public async Task GetAllKeysAsync_ReturnsAllSettingKeys()
    {
        // Arrange
        await SeedMultipleSettingsAsync();
        var service = new SettingService(
            _loggerMock.Object,
            _cacheManagerMock.Object,
            _configFactoryMock.Object,
            CreateContextFactory());

        // Act
        var keys = await service.GetAllKeysAsync();

        // Assert
        Assert.NotNull(keys);
        Assert.Equal(3, keys.Count);
        Assert.Contains(SettingRegistry.SystemSiteName, keys);
        Assert.Contains(SettingRegistry.SystemBaseUrl, keys);
        Assert.Contains(SettingRegistry.DefaultsPageSize, keys);
    }

    [Fact]
    public async Task GetAllKeysAsync_ReturnsEmptyList_WhenNoSettings()
    {
        // Arrange
        var service = new SettingService(
            _loggerMock.Object,
            _cacheManagerMock.Object,
            _configFactoryMock.Object,
            CreateContextFactory());

        // Act
        var keys = await service.GetAllKeysAsync();

        // Assert
        Assert.NotNull(keys);
        Assert.Empty(keys);
    }

    [Fact]
    public async Task UpdateAsync_ClearsCacheForUpdatedSetting()
    {
        // Arrange
        await SeedTestSettingAsync();
        var service = new SettingService(
            _loggerMock.Object,
            _cacheManagerMock.Object,
            _configFactoryMock.Object,
            CreateContextFactory());

        Setting settingToUpdate;
        await using (var context = new MelodeeDbContext(_dbContextOptions))
        {
            settingToUpdate = await context.Settings.FirstAsync();
            settingToUpdate.Value = "Updated Value";
        }

        // Act
        var result = await service.UpdateAsync(settingToUpdate);

        // Assert
        Assert.True(result.IsSuccess);
        _cacheManagerMock.Verify(
            c => c.Remove(It.Is<string>(key => key.Contains(settingToUpdate.Id.ToString()))),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_AndReset_EnablesImmediateConfigurationReload()
    {
        // Arrange
        await SeedTestSettingAsync();

        // Use real configuration factory to test full integration
        var realConfigFactory = new MelodeeConfigurationFactory(CreateContextFactory());
        var service = new SettingService(
            _loggerMock.Object,
            _cacheManagerMock.Object,
            realConfigFactory,
            CreateContextFactory());

        // Get initial configuration
        var config1 = await realConfigFactory.GetConfigurationAsync();
        var originalValue = config1.GetValue<string>(SettingRegistry.SystemSiteName);
        Assert.Equal("Test Site", originalValue);

        // Update setting
        Setting settingToUpdate;
        await using (var context = new MelodeeDbContext(_dbContextOptions))
        {
            settingToUpdate = await context.Settings.FirstAsync();
            settingToUpdate.Value = "Updated Site Name";
        }

        // Act
        var updateResult = await service.UpdateAsync(settingToUpdate);

        // Get new configuration after reset
        var config2 = await realConfigFactory.GetConfigurationAsync();
        var updatedValue = config2.GetValue<string>(SettingRegistry.SystemSiteName);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal("Updated Site Name", updatedValue);
        Assert.NotSame(config1, config2); // Should be different instance after reset
    }

    [Fact]
    public async Task UpdateAsync_RaisesEvent_AllowingImmediateUIUpdate()
    {
        // Arrange
        await SeedTestSettingAsync();

        var realConfigFactory = new MelodeeConfigurationFactory(CreateContextFactory());
        var service = new SettingService(
            _loggerMock.Object,
            _cacheManagerMock.Object,
            realConfigFactory,
            CreateContextFactory());

        var eventRaised = false;
        string? eventValue = null;

        realConfigFactory.ConfigurationChanged += async (sender, args) =>
        {
            eventRaised = true;
            var config = await realConfigFactory.GetConfigurationAsync();
            eventValue = config.GetValue<string>(SettingRegistry.SystemSiteName);
        };

        // Update setting
        Setting settingToUpdate;
        await using (var context = new MelodeeDbContext(_dbContextOptions))
        {
            settingToUpdate = await context.Settings.FirstAsync();
            settingToUpdate.Value = "UI Updated Value";
        }

        // Act
        await service.UpdateAsync(settingToUpdate);

        // Small delay for async event handling
        await Task.Delay(50);

        // Assert
        Assert.True(eventRaised, "Event should be raised when configuration is reset");
        Assert.Equal("UI Updated Value", eventValue);
    }

    private async Task SeedTestSettingAsync()
    {
        await using var context = new MelodeeDbContext(_dbContextOptions);

        var setting = new Setting
        {
            Id = 1,
            Key = SettingRegistry.SystemSiteName,
            Value = "Test Site",
            Comment = "Test site name",
            ApiKey = Guid.NewGuid(),
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        await context.Settings.AddAsync(setting);
        await context.SaveChangesAsync();
    }

    private async Task SeedMultipleSettingsAsync()
    {
        await using var context = new MelodeeDbContext(_dbContextOptions);

        var now = SystemClock.Instance.GetCurrentInstant();

        var settings = new[]
        {
            new Setting
            {
                Id = 1,
                Key = SettingRegistry.SystemSiteName,
                Value = "Test Site",
                ApiKey = Guid.NewGuid(),
                CreatedAt = now
            },
            new Setting
            {
                Id = 2,
                Key = SettingRegistry.SystemBaseUrl,
                Value = "http://test.local",
                ApiKey = Guid.NewGuid(),
                CreatedAt = now
            },
            new Setting
            {
                Id = 3,
                Key = SettingRegistry.DefaultsPageSize,
                Value = "50",
                ApiKey = Guid.NewGuid(),
                CreatedAt = now
            }
        };

        await context.Settings.AddRangeAsync(settings);
        await context.SaveChangesAsync();
    }

    private IDbContextFactory<MelodeeDbContext> CreateContextFactory()
    {
        return new TestDbContextFactory(_dbContextOptions);
    }

    private class TestDbContextFactory : IDbContextFactory<MelodeeDbContext>
    {
        private readonly DbContextOptions<MelodeeDbContext> _options;

        public TestDbContextFactory(DbContextOptions<MelodeeDbContext> options)
        {
            _options = options;
        }

        public MelodeeDbContext CreateDbContext()
        {
            return new MelodeeDbContext(_options);
        }

        public Task<MelodeeDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new MelodeeDbContext(_options));
        }
    }
}

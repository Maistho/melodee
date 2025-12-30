using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Tests.Common.Configuration;

public class MelodeeConfigurationFactoryTests : IDisposable
{
    private readonly DbContextOptions<MelodeeDbContext> _dbContextOptions;

    public MelodeeConfigurationFactoryTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<MelodeeDbContext>()
            .UseInMemoryDatabase(databaseName: $"MelodeeConfigFactoryTest_{Guid.NewGuid()}")
            .Options;
    }

    public void Dispose()
    {
        using var context = new MelodeeDbContext(_dbContextOptions);
        context.Database.EnsureDeleted();
    }

    [Fact]
    public async Task GetConfigurationAsync_LoadsSettingsFromDatabase()
    {
        // Arrange
        await SeedTestSettingsAsync();
        var factory = new MelodeeConfigurationFactory(CreateContextFactory());

        // Act
        var config = await factory.GetConfigurationAsync();

        // Assert
        Assert.NotNull(config);
        Assert.Equal("Test Site", config.GetValue<string>(SettingRegistry.SystemSiteName));
        Assert.Equal("http://test.local", config.GetValue<string>(SettingRegistry.SystemBaseUrl));
    }

    [Fact]
    public async Task GetConfigurationAsync_CachesConfiguration()
    {
        // Arrange
        await SeedTestSettingsAsync();
        var factory = new MelodeeConfigurationFactory(CreateContextFactory());

        // Act
        var config1 = await factory.GetConfigurationAsync();
        var config2 = await factory.GetConfigurationAsync();

        // Assert
        Assert.Same(config1, config2); // Should return cached instance
    }

    [Fact]
    public async Task Reset_ClearsCache()
    {
        // Arrange
        await SeedTestSettingsAsync();
        var factory = new MelodeeConfigurationFactory(CreateContextFactory());
        var config1 = await factory.GetConfigurationAsync();

        // Act
        factory.Reset();
        var config2 = await factory.GetConfigurationAsync();

        // Assert
        Assert.NotSame(config1, config2); // Should return new instance after reset
    }

    [Fact]
    public void Reset_RaisesConfigurationChangedEvent()
    {
        // Arrange
        var factory = new MelodeeConfigurationFactory(CreateContextFactory());
        var eventRaised = false;
        factory.ConfigurationChanged += (sender, args) => eventRaised = true;

        // Act
        factory.Reset();

        // Assert
        Assert.True(eventRaised, "ConfigurationChanged event should be raised when Reset is called");
    }

    [Fact]
    public void Reset_PassesCorrectSenderInEvent()
    {
        // Arrange
        var factory = new MelodeeConfigurationFactory(CreateContextFactory());
        object? eventSender = null;
        factory.ConfigurationChanged += (sender, args) => eventSender = sender;

        // Act
        factory.Reset();

        // Assert
        Assert.Same(factory, eventSender);
    }

    [Fact]
    public void Reset_PassesEmptyEventArgsInEvent()
    {
        // Arrange
        var factory = new MelodeeConfigurationFactory(CreateContextFactory());
        EventArgs? eventArgs = null;
        factory.ConfigurationChanged += (sender, args) => eventArgs = args;

        // Act
        factory.Reset();

        // Assert
        Assert.NotNull(eventArgs);
        Assert.Same(EventArgs.Empty, eventArgs);
    }

    [Fact]
    public void Reset_RaisesEventForMultipleSubscribers()
    {
        // Arrange
        var factory = new MelodeeConfigurationFactory(CreateContextFactory());
        var subscriber1Called = false;
        var subscriber2Called = false;
        var subscriber3Called = false;

        factory.ConfigurationChanged += (sender, args) => subscriber1Called = true;
        factory.ConfigurationChanged += (sender, args) => subscriber2Called = true;
        factory.ConfigurationChanged += (sender, args) => subscriber3Called = true;

        // Act
        factory.Reset();

        // Assert
        Assert.True(subscriber1Called, "Subscriber 1 should be notified");
        Assert.True(subscriber2Called, "Subscriber 2 should be notified");
        Assert.True(subscriber3Called, "Subscriber 3 should be notified");
    }

    [Fact]
    public void Reset_DoesNotRaiseEventAfterUnsubscribe()
    {
        // Arrange
        var factory = new MelodeeConfigurationFactory(CreateContextFactory());
        var eventRaisedCount = 0;
        EventHandler handler = (sender, args) => eventRaisedCount++;

        factory.ConfigurationChanged += handler;
        factory.Reset();
        Assert.Equal(1, eventRaisedCount);

        // Act - Unsubscribe and reset again
        factory.ConfigurationChanged -= handler;
        factory.Reset();

        // Assert
        Assert.Equal(1, eventRaisedCount); // Should still be 1, not 2
    }

    [Fact]
    public void Reset_HandlesNullSubscribersGracefully()
    {
        // Arrange
        var factory = new MelodeeConfigurationFactory(CreateContextFactory());

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => factory.Reset());
        Assert.Null(exception);
    }

    [Fact]
    public async Task Reset_AllowsReloadingUpdatedConfiguration()
    {
        // Arrange
        await SeedTestSettingsAsync();
        var factory = new MelodeeConfigurationFactory(CreateContextFactory());

        var config1 = await factory.GetConfigurationAsync();
        var originalSiteName = config1.GetValue<string>(SettingRegistry.SystemSiteName);
        Assert.Equal("Test Site", originalSiteName);

        // Update setting in database
        await using (var context = new MelodeeDbContext(_dbContextOptions))
        {
            var setting = await context.Settings.FirstAsync(s => s.Key == SettingRegistry.SystemSiteName);
            setting.Value = "Updated Site Name";
            await context.SaveChangesAsync();
        }

        // Act
        factory.Reset();
        var config2 = await factory.GetConfigurationAsync();

        // Assert
        var updatedSiteName = config2.GetValue<string>(SettingRegistry.SystemSiteName);
        Assert.Equal("Updated Site Name", updatedSiteName);
    }

    [Fact]
    public async Task MultipleResetsInSequence_RaiseEventEachTime()
    {
        // Arrange
        var factory = new MelodeeConfigurationFactory(CreateContextFactory());
        var eventCount = 0;
        factory.ConfigurationChanged += (sender, args) => eventCount++;

        // Act
        factory.Reset();
        await Task.Delay(10); // Small delay to ensure async handling
        factory.Reset();
        await Task.Delay(10);
        factory.Reset();

        // Assert
        Assert.Equal(3, eventCount);
    }

    private async Task SeedTestSettingsAsync()
    {
        var now = SystemClock.Instance.GetCurrentInstant();

        await using var context = new MelodeeDbContext(_dbContextOptions);

        var settings = new[]
        {
            new Setting
            {
                Id = 1,
                Key = SettingRegistry.SystemSiteName,
                Value = "Test Site",
                Comment = "Test site name",
                ApiKey = Guid.NewGuid(),
                CreatedAt = now
            },
            new Setting
            {
                Id = 2,
                Key = SettingRegistry.SystemBaseUrl,
                Value = "http://test.local",
                Comment = "Test base URL",
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

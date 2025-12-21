using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Melodee.Common.Services;
using NodaTime;

namespace Melodee.Tests.Common.Common.Services;

public class PlaybackSettingsServiceTests : ServiceTestBase
{
    private PlaybackSettingsService CreatePlaybackSettingsService()
    {
        return new PlaybackSettingsService(Logger, CacheManager, MockFactory());
    }

    [Fact]
    public async Task GetByUserIdAsync_WithExistingSettings_ReturnsSettings()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 1;

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var settings = new UserPlaybackSettings
            {
                UserId = userId,
                CrossfadeDuration = 5.0,
                GaplessPlayback = true,
                VolumeNormalization = true,
                ReplayGain = "album",
                AudioQuality = "high",
                EqualizerPreset = "rock",
                LastUsedDevice = "iPhone",
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.UserPlaybackSettings.Add(settings);
            await context.SaveChangesAsync();
        }

        var result = await service.GetByUserIdAsync(userId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(5.0, result.Data.CrossfadeDuration);
        Assert.True(result.Data.GaplessPlayback);
        Assert.True(result.Data.VolumeNormalization);
        Assert.Equal("album", result.Data.ReplayGain);
        Assert.Equal("high", result.Data.AudioQuality);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithNoSettings_ReturnsDefaults()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 999;

        var result = await service.GetByUserIdAsync(userId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(0, result.Data.CrossfadeDuration);
        Assert.True(result.Data.GaplessPlayback);
        Assert.False(result.Data.VolumeNormalization);
        Assert.Equal("none", result.Data.ReplayGain);
        Assert.Equal("high", result.Data.AudioQuality);
        Assert.Null(result.Data.EqualizerPreset);
        Assert.Null(result.Data.LastUsedDevice);
    }

    [Fact]
    public async Task UpdateAsync_WithValidCrossfadeDuration_UpdatesSettings()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 2;

        var result = await service.UpdateAsync(userId, 3.5, null, null, null, null, null);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        var settings = await service.GetByUserIdAsync(userId);
        Assert.Equal(3.5, settings.Data.CrossfadeDuration);
    }

    [Fact]
    public async Task UpdateAsync_WithNegativeCrossfadeDuration_ReturnsValidationFailure()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 3;

        var result = await service.UpdateAsync(userId, -1.0, null, null, null, null, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.ValidationFailure, result.Type);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task UpdateAsync_WithValidReplayGain_UpdatesSettings()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 4;

        var result = await service.UpdateAsync(userId, null, null, null, "track", null, null);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        var settings = await service.GetByUserIdAsync(userId);
        Assert.Equal("track", settings.Data.ReplayGain);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidReplayGain_ReturnsValidationFailure()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 5;

        var result = await service.UpdateAsync(userId, null, null, null, "invalid", null, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.ValidationFailure, result.Type);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task UpdateAsync_WithValidAudioQuality_UpdatesSettings()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 6;

        var result = await service.UpdateAsync(userId, null, null, null, null, "lossless", null);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        var settings = await service.GetByUserIdAsync(userId);
        Assert.Equal("lossless", settings.Data.AudioQuality);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidAudioQuality_ReturnsValidationFailure()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 7;

        var result = await service.UpdateAsync(userId, null, null, null, null, "invalid", null);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.ValidationFailure, result.Type);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task UpdateAsync_WithGaplessPlayback_UpdatesSettings()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 8;

        var result = await service.UpdateAsync(userId, null, false, null, null, null, null);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        var settings = await service.GetByUserIdAsync(userId);
        Assert.False(settings.Data.GaplessPlayback);
    }

    [Fact]
    public async Task UpdateAsync_WithVolumeNormalization_UpdatesSettings()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 9;

        var result = await service.UpdateAsync(userId, null, null, true, null, null, null);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        var settings = await service.GetByUserIdAsync(userId);
        Assert.True(settings.Data.VolumeNormalization);
    }

    [Fact]
    public async Task UpdateAsync_WithEqualizerPreset_UpdatesSettings()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 10;

        var result = await service.UpdateAsync(userId, null, null, null, null, null, "bass-boost");

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        var settings = await service.GetByUserIdAsync(userId);
        Assert.Equal("bass-boost", settings.Data.EqualizerPreset);
    }

    [Fact]
    public async Task UpdateAsync_WithMultipleParameters_UpdatesAllSettings()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 11;

        var result = await service.UpdateAsync(userId, 2.5, true, true, "album", "medium", "custom");

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        var settings = await service.GetByUserIdAsync(userId);
        Assert.Equal(2.5, settings.Data.CrossfadeDuration);
        Assert.True(settings.Data.GaplessPlayback);
        Assert.True(settings.Data.VolumeNormalization);
        Assert.Equal("album", settings.Data.ReplayGain);
        Assert.Equal("medium", settings.Data.AudioQuality);
        Assert.Equal("custom", settings.Data.EqualizerPreset);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingSettings_PreservesOtherFields()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 12;

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var settings = new UserPlaybackSettings
            {
                UserId = userId,
                CrossfadeDuration = 1.0,
                GaplessPlayback = false,
                VolumeNormalization = false,
                ReplayGain = "none",
                AudioQuality = "low",
                EqualizerPreset = "original",
                LastUsedDevice = "iPad",
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.UserPlaybackSettings.Add(settings);
            await context.SaveChangesAsync();
        }

        await service.UpdateAsync(userId, 3.0, null, null, null, null, null);

        var updated = await service.GetByUserIdAsync(userId);
        Assert.Equal(3.0, updated.Data.CrossfadeDuration);
        Assert.False(updated.Data.GaplessPlayback);
        Assert.Equal("original", updated.Data.EqualizerPreset);
        Assert.Equal("iPad", updated.Data.LastUsedDevice);
    }

    [Fact]
    public async Task UpdateLastUsedDeviceAsync_WithNewDevice_UpdatesDevice()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 13;

        var result = await service.UpdateLastUsedDeviceAsync(userId, "Android Phone");

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        var settings = await service.GetByUserIdAsync(userId);
        Assert.Equal("Android Phone", settings.Data.LastUsedDevice);
    }

    [Fact]
    public async Task UpdateLastUsedDeviceAsync_WithExistingSettings_UpdatesDevice()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 14;

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var settings = new UserPlaybackSettings
            {
                UserId = userId,
                CrossfadeDuration = 1.0,
                GaplessPlayback = true,
                VolumeNormalization = false,
                ReplayGain = "none",
                AudioQuality = "high",
                EqualizerPreset = null,
                LastUsedDevice = "Old Device",
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.UserPlaybackSettings.Add(settings);
            await context.SaveChangesAsync();
        }

        var result = await service.UpdateLastUsedDeviceAsync(userId, "New Device");

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        var updated = await service.GetByUserIdAsync(userId);
        Assert.Equal("New Device", updated.Data.LastUsedDevice);
    }

    [Fact]
    public async Task UpdateAsync_WithCaseInsensitiveReplayGain_ConvertsToLowercase()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 15;

        var result = await service.UpdateAsync(userId, null, null, null, "ALBUM", null, null);

        Assert.True(result.IsSuccess);

        var settings = await service.GetByUserIdAsync(userId);
        Assert.Equal("album", settings.Data.ReplayGain);
    }

    [Fact]
    public async Task UpdateAsync_WithCaseInsensitiveAudioQuality_ConvertsToLowercase()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 16;

        var result = await service.UpdateAsync(userId, null, null, null, null, "LOSSLESS", null);

        Assert.True(result.IsSuccess);

        var settings = await service.GetByUserIdAsync(userId);
        Assert.Equal("lossless", settings.Data.AudioQuality);
    }

    [Fact]
    public async Task UpdateAsync_WithNullEqualizerPreset_SetsToNull()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 17;

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var settings = new UserPlaybackSettings
            {
                UserId = userId,
                CrossfadeDuration = 1.0,
                GaplessPlayback = true,
                VolumeNormalization = false,
                ReplayGain = "none",
                AudioQuality = "high",
                EqualizerPreset = "rock",
                LastUsedDevice = null,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.UserPlaybackSettings.Add(settings);
            await context.SaveChangesAsync();
        }

        var result = await service.UpdateAsync(userId, null, null, null, null, null, "");

        Assert.True(result.IsSuccess);

        var updated = await service.GetByUserIdAsync(userId);
        Assert.Null(updated.Data.EqualizerPreset);
    }

    [Fact]
    public void ValidReplayGainValues_ContainsExpectedValues()
    {
        var validValues = PlaybackSettingsService.ValidReplayGainValues;

        Assert.Contains("none", validValues);
        Assert.Contains("track", validValues);
        Assert.Contains("album", validValues);
        Assert.Equal(3, validValues.Length);
    }

    [Fact]
    public void ValidAudioQualityValues_ContainsExpectedValues()
    {
        var validValues = PlaybackSettingsService.ValidAudioQualityValues;

        Assert.Contains("low", validValues);
        Assert.Contains("medium", validValues);
        Assert.Contains("high", validValues);
        Assert.Contains("lossless", validValues);
        Assert.Equal(4, validValues.Length);
    }

    [Fact]
    public async Task UpdateAsync_WithZeroCrossfadeDuration_UpdatesSettings()
    {
        var service = CreatePlaybackSettingsService();
        var userId = 18;

        var result = await service.UpdateAsync(userId, 0.0, null, null, null, null, null);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        var settings = await service.GetByUserIdAsync(userId);
        Assert.Equal(0.0, settings.Data.CrossfadeDuration);
    }

    [Fact]
    public async Task UpdateAsync_WithMultipleUsers_IsolatesSettings()
    {
        var service = CreatePlaybackSettingsService();
        var user1 = 19;
        var user2 = 20;

        await service.UpdateAsync(user1, 2.0, true, false, "track", "high", "preset1");
        await service.UpdateAsync(user2, 4.0, false, true, "album", "low", "preset2");

        var settings1 = await service.GetByUserIdAsync(user1);
        var settings2 = await service.GetByUserIdAsync(user2);

        Assert.Equal(2.0, settings1.Data.CrossfadeDuration);
        Assert.Equal(4.0, settings2.Data.CrossfadeDuration);
        Assert.True(settings1.Data.GaplessPlayback);
        Assert.False(settings2.Data.GaplessPlayback);
        Assert.Equal("preset1", settings1.Data.EqualizerPreset);
        Assert.Equal("preset2", settings2.Data.EqualizerPreset);
    }
}

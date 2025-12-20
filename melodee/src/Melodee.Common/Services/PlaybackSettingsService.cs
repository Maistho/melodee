using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services;

public sealed class PlaybackSettingsService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public static readonly string[] ValidReplayGainValues = ["none", "track", "album"];
    public static readonly string[] ValidAudioQualityValues = ["low", "medium", "high", "lossless"];

    public async Task<OperationResult<UserPlaybackSettings>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var settings = await context.UserPlaybackSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        if (settings == null)
        {
            // Return defaults
            settings = CreateDefaultSettings(userId);
        }

        return new OperationResult<UserPlaybackSettings> { Data = settings };
    }

    public async Task<OperationResult<bool>> UpdateAsync(
        int userId,
        double? crossfadeDuration,
        bool? gaplessPlayback,
        bool? volumeNormalization,
        string? replayGain,
        string? audioQuality,
        string? equalizerPreset,
        CancellationToken cancellationToken = default)
    {
        // Validation
        if (crossfadeDuration.HasValue && crossfadeDuration.Value < 0)
        {
            return new OperationResult<bool>(OperationResponseType.ValidationFailure, "crossfadeDuration must be >= 0")
            {
                Data = false
            };
        }

        if (!string.IsNullOrEmpty(replayGain) && !ValidReplayGainValues.Contains(replayGain, StringComparer.OrdinalIgnoreCase))
        {
            return new OperationResult<bool>(OperationResponseType.ValidationFailure, $"replayGain must be one of: {string.Join(", ", ValidReplayGainValues)}")
            {
                Data = false
            };
        }

        if (!string.IsNullOrEmpty(audioQuality) && !ValidAudioQualityValues.Contains(audioQuality, StringComparer.OrdinalIgnoreCase))
        {
            return new OperationResult<bool>(OperationResponseType.ValidationFailure, $"audioQuality must be one of: {string.Join(", ", ValidAudioQualityValues)}")
            {
                Data = false
            };
        }

        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var existing = await context.UserPlaybackSettings
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        if (existing == null)
        {
            existing = CreateDefaultSettings(userId);
            context.UserPlaybackSettings.Add(existing);
        }

        // Apply partial updates
        if (crossfadeDuration.HasValue)
        {
            existing.CrossfadeDuration = crossfadeDuration.Value;
        }

        if (gaplessPlayback.HasValue)
        {
            existing.GaplessPlayback = gaplessPlayback.Value;
        }

        if (volumeNormalization.HasValue)
        {
            existing.VolumeNormalization = volumeNormalization.Value;
        }

        if (!string.IsNullOrEmpty(replayGain))
        {
            existing.ReplayGain = replayGain.ToLowerInvariant();
        }

        if (!string.IsNullOrEmpty(audioQuality))
        {
            existing.AudioQuality = audioQuality.ToLowerInvariant();
        }

        if (equalizerPreset != null)
        {
            existing.EqualizerPreset = equalizerPreset.Nullify();
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new OperationResult<bool> { Data = true };
    }

    public async Task<OperationResult<bool>> UpdateLastUsedDeviceAsync(int userId, string deviceName, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var existing = await context.UserPlaybackSettings
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        if (existing == null)
        {
            existing = CreateDefaultSettings(userId);
            existing.LastUsedDevice = deviceName;
            context.UserPlaybackSettings.Add(existing);
        }
        else
        {
            existing.LastUsedDevice = deviceName;
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new OperationResult<bool> { Data = true };
    }

    private static UserPlaybackSettings CreateDefaultSettings(int userId)
    {
        return new UserPlaybackSettings
        {
            UserId = userId,
            CrossfadeDuration = 0,
            GaplessPlayback = true,
            VolumeNormalization = false,
            ReplayGain = "none",
            AudioQuality = "high",
            EqualizerPreset = null,
            LastUsedDevice = null,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
    }
}

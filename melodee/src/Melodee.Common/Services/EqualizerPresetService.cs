using System.Text.Json;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services;

public sealed class EqualizerPresetService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public record EqualizerBandDto(double Frequency, double Gain);

    public async Task<PagedResult<UserEqualizerPreset>> ListAsync(int userId, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var query = context.UserEqualizerPresets
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        if (pagedRequest.IsTotalCountOnlyRequest)
        {
            return new PagedResult<UserEqualizerPreset>
            {
                TotalCount = totalCount,
                Data = []
            };
        }

        var data = await query
            .OrderBy(x => x.Name)
            .Skip(pagedRequest.SkipValue)
            .Take(pagedRequest.TakeValue)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<UserEqualizerPreset>
        {
            TotalCount = totalCount,
            Data = data
        };
    }

    public async Task<OperationResult<UserEqualizerPreset?>> GetByIdAsync(int userId, Guid apiKey, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var preset = await context.UserEqualizerPresets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ApiKey == apiKey, cancellationToken)
            .ConfigureAwait(false);

        if (preset == null)
        {
            return new OperationResult<UserEqualizerPreset?>(OperationResponseType.NotFound, "Preset not found")
            {
                Data = null
            };
        }

        return new OperationResult<UserEqualizerPreset?> { Data = preset };
    }

    public async Task<OperationResult<UserEqualizerPreset?>> UpsertAsync(
        int userId,
        string name,
        EqualizerBandDto[] bands,
        bool isDefault,
        CancellationToken cancellationToken = default)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(name))
        {
            return new OperationResult<UserEqualizerPreset?>(OperationResponseType.ValidationFailure, "name is required")
            {
                Data = null
            };
        }

        if (bands == null || bands.Length == 0)
        {
            return new OperationResult<UserEqualizerPreset?>(OperationResponseType.ValidationFailure, "bands is required and must not be empty")
            {
                Data = null
            };
        }

        foreach (var band in bands)
        {
            if (band.Frequency <= 0)
            {
                return new OperationResult<UserEqualizerPreset?>(OperationResponseType.ValidationFailure, "Each band frequency must be > 0")
                {
                    Data = null
                };
            }
        }

        var nameNormalized = name.ToNormalizedString() ?? name.ToUpperInvariant();
        var bandsJson = JsonSerializer.Serialize(bands);

        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        // Check for existing preset with same name
        var existing = await context.UserEqualizerPresets
            .FirstOrDefaultAsync(x => x.UserId == userId && x.NameNormalized == nameNormalized, cancellationToken)
            .ConfigureAwait(false);

        if (existing != null)
        {
            // Update existing
            existing.Name = name;
            existing.BandsJson = bandsJson;
            existing.IsDefault = isDefault;
        }
        else
        {
            // Create new
            existing = new UserEqualizerPreset
            {
                UserId = userId,
                Name = name,
                NameNormalized = nameNormalized,
                BandsJson = bandsJson,
                IsDefault = isDefault,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.UserEqualizerPresets.Add(existing);
        }

        // If this is set as default, clear other defaults for this user
        if (isDefault)
        {
            var otherDefaults = await context.UserEqualizerPresets
                .Where(x => x.UserId == userId && x.IsDefault && x.Id != existing.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var other in otherDefaults)
            {
                other.IsDefault = false;
            }
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new OperationResult<UserEqualizerPreset?> { Data = existing };
    }

    public async Task<OperationResult<bool>> DeleteAsync(int userId, Guid apiKey, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var preset = await context.UserEqualizerPresets
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ApiKey == apiKey, cancellationToken)
            .ConfigureAwait(false);

        if (preset == null)
        {
            return new OperationResult<bool>(OperationResponseType.NotFound, "Preset not found")
            {
                Data = false
            };
        }

        context.UserEqualizerPresets.Remove(preset);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new OperationResult<bool> { Data = true };
    }

    public static EqualizerBandDto[] ParseBands(string bandsJson)
    {
        try
        {
            return JsonSerializer.Deserialize<EqualizerBandDto[]>(bandsJson) ?? [];
        }
        catch
        {
            return [];
        }
    }
}

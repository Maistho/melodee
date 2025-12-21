using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Melodee.Common.Services;
using NodaTime;

namespace Melodee.Tests.Common.Common.Services;

public class EqualizerPresetServiceTests : ServiceTestBase
{
    private EqualizerPresetService CreateEqualizerPresetService()
    {
        return new EqualizerPresetService(Logger, CacheManager, MockFactory());
    }

    private static EqualizerPresetService.EqualizerBandDto[] CreateTestBands()
    {
        return
        [
            new EqualizerPresetService.EqualizerBandDto(32, -5),
            new EqualizerPresetService.EqualizerBandDto(64, -3),
            new EqualizerPresetService.EqualizerBandDto(125, 0),
            new EqualizerPresetService.EqualizerBandDto(250, 3),
            new EqualizerPresetService.EqualizerBandDto(500, 5)
        ];
    }

    [Fact]
    public async Task ListAsync_WithValidUserId_ReturnsPagedResult()
    {
        var service = CreateEqualizerPresetService();
        var userId = 1;

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var preset1 = new UserEqualizerPreset
            {
                UserId = userId,
                Name = "Rock",
                NameNormalized = "ROCK",
                BandsJson = "[{\"Frequency\":32,\"Gain\":5}]",
                IsDefault = false,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            var preset2 = new UserEqualizerPreset
            {
                UserId = userId,
                Name = "Jazz",
                NameNormalized = "JAZZ",
                BandsJson = "[{\"Frequency\":64,\"Gain\":3}]",
                IsDefault = true,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.UserEqualizerPresets.AddRange(preset1, preset2);
            await context.SaveChangesAsync();
        }

        var pagedRequest = new PagedRequest { Page = 1, PageSize = 10 };
        var result = await service.ListAsync(userId, pagedRequest);

        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 2);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task ListAsync_WithTotalCountOnly_ReturnsCountWithoutData()
    {
        var service = CreateEqualizerPresetService();
        var userId = 2;

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var preset = new UserEqualizerPreset
            {
                UserId = userId,
                Name = "Classical",
                NameNormalized = "CLASSICAL",
                BandsJson = "[{\"Frequency\":32,\"Gain\":2}]",
                IsDefault = false,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.UserEqualizerPresets.Add(preset);
            await context.SaveChangesAsync();
        }

        var pagedRequest = new PagedRequest { Page = 1, PageSize = 10, IsTotalCountOnlyRequest = true };
        var result = await service.ListAsync(userId, pagedRequest);

        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidApiKey_ReturnsPreset()
    {
        var service = CreateEqualizerPresetService();
        var userId = 3;
        var apiKey = Guid.NewGuid();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var preset = new UserEqualizerPreset
            {
                UserId = userId,
                ApiKey = apiKey,
                Name = "Pop",
                NameNormalized = "POP",
                BandsJson = "[{\"Frequency\":125,\"Gain\":4}]",
                IsDefault = false,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.UserEqualizerPresets.Add(preset);
            await context.SaveChangesAsync();
        }

        var result = await service.GetByIdAsync(userId, apiKey);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(apiKey, result.Data!.ApiKey);
        Assert.Equal("Pop", result.Data.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidApiKey_ReturnsNotFound()
    {
        var service = CreateEqualizerPresetService();
        var userId = 4;
        var apiKey = Guid.NewGuid();

        var result = await service.GetByIdAsync(userId, apiKey);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.NotFound, result.Type);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpsertAsync_WithValidData_CreatesNewPreset()
    {
        var service = CreateEqualizerPresetService();
        var userId = 5;
        var bands = CreateTestBands();

        var result = await service.UpsertAsync(userId, "Bass Boost", bands, false);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Bass Boost", result.Data!.Name);
        Assert.False(result.Data.IsDefault);
    }

    [Fact]
    public async Task UpsertAsync_WithExistingName_UpdatesPreset()
    {
        var service = CreateEqualizerPresetService();
        var userId = 6;
        var bands = CreateTestBands();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var existingPreset = new UserEqualizerPreset
            {
                UserId = userId,
                Name = "Treble Boost",
                NameNormalized = "TREBLE BOOST",
                BandsJson = "[{\"Frequency\":32,\"Gain\":0}]",
                IsDefault = false,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.UserEqualizerPresets.Add(existingPreset);
            await context.SaveChangesAsync();
        }

        var newBands = CreateTestBands();
        var result = await service.UpsertAsync(userId, "Treble Boost", newBands, false);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Treble Boost", result.Data!.Name);
    }

    [Fact]
    public async Task UpsertAsync_WithEmptyName_ReturnsValidationFailure()
    {
        var service = CreateEqualizerPresetService();
        var userId = 7;
        var bands = CreateTestBands();

        var result = await service.UpsertAsync(userId, "", bands, false);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.ValidationFailure, result.Type);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpsertAsync_WithNullBands_ReturnsValidationFailure()
    {
        var service = CreateEqualizerPresetService();
        var userId = 8;

        var result = await service.UpsertAsync(userId, "Test", null!, false);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.ValidationFailure, result.Type);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpsertAsync_WithEmptyBands_ReturnsValidationFailure()
    {
        var service = CreateEqualizerPresetService();
        var userId = 9;

        var result = await service.UpsertAsync(userId, "Test", [], false);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.ValidationFailure, result.Type);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpsertAsync_WithInvalidFrequency_ReturnsValidationFailure()
    {
        var service = CreateEqualizerPresetService();
        var userId = 10;
        var bands = new[]
        {
            new EqualizerPresetService.EqualizerBandDto(0, 5),
            new EqualizerPresetService.EqualizerBandDto(64, 3)
        };

        var result = await service.UpsertAsync(userId, "Invalid", bands, false);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.ValidationFailure, result.Type);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpsertAsync_WithIsDefault_ClearsOtherDefaults()
    {
        var service = CreateEqualizerPresetService();
        var userId = 11;

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var existingDefault = new UserEqualizerPreset
            {
                UserId = userId,
                Name = "Old Default",
                NameNormalized = "OLD DEFAULT",
                BandsJson = "[{\"Frequency\":32,\"Gain\":0}]",
                IsDefault = true,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.UserEqualizerPresets.Add(existingDefault);
            await context.SaveChangesAsync();
        }

        var bands = CreateTestBands();
        var result = await service.UpsertAsync(userId, "New Default", bands, true);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data!.IsDefault);

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var oldDefault = await context.UserEqualizerPresets
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Name == "Old Default");
            Assert.NotNull(oldDefault);
            Assert.False(oldDefault!.IsDefault);
        }
    }

    [Fact]
    public async Task DeleteAsync_WithValidApiKey_DeletesPreset()
    {
        var service = CreateEqualizerPresetService();
        var userId = 12;
        var apiKey = Guid.NewGuid();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var preset = new UserEqualizerPreset
            {
                UserId = userId,
                ApiKey = apiKey,
                Name = "To Delete",
                NameNormalized = "TO DELETE",
                BandsJson = "[{\"Frequency\":32,\"Gain\":0}]",
                IsDefault = false,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.UserEqualizerPresets.Add(preset);
            await context.SaveChangesAsync();
        }

        var result = await service.DeleteAsync(userId, apiKey);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var deleted = await context.UserEqualizerPresets
                .FirstOrDefaultAsync(x => x.ApiKey == apiKey);
            Assert.Null(deleted);
        }
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidApiKey_ReturnsNotFound()
    {
        var service = CreateEqualizerPresetService();
        var userId = 13;
        var apiKey = Guid.NewGuid();

        var result = await service.DeleteAsync(userId, apiKey);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.NotFound, result.Type);
        Assert.False(result.Data);
    }

    [Fact]
    public void ParseBands_WithValidJson_ReturnsBands()
    {
        var json = "[{\"Frequency\":32,\"Gain\":5},{\"Frequency\":64,\"Gain\":3}]";

        var bands = EqualizerPresetService.ParseBands(json);

        Assert.NotNull(bands);
        Assert.Equal(2, bands.Length);
        Assert.Equal(32, bands[0].Frequency);
        Assert.Equal(5, bands[0].Gain);
    }

    [Fact]
    public void ParseBands_WithInvalidJson_ReturnsEmptyArray()
    {
        var json = "invalid json";

        var bands = EqualizerPresetService.ParseBands(json);

        Assert.NotNull(bands);
        Assert.Empty(bands);
    }

    [Fact]
    public async Task ListAsync_WithPagination_ReturnsCorrectPage()
    {
        var service = CreateEqualizerPresetService();
        var userId = 14;

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            for (var i = 1; i <= 5; i++)
            {
                var preset = new UserEqualizerPreset
                {
                    UserId = userId,
                    Name = $"Preset {i}",
                    NameNormalized = $"PRESET {i}",
                    BandsJson = "[{\"Frequency\":32,\"Gain\":0}]",
                    IsDefault = false,
                    CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
                };
                context.UserEqualizerPresets.Add(preset);
            }
            await context.SaveChangesAsync();
        }

        var pagedRequest = new PagedRequest { Page = 1, PageSize = 2 };
        var result = await service.ListAsync(userId, pagedRequest);

        Assert.NotNull(result);
        Assert.Equal(2, result.Data.Length);
        Assert.True(result.TotalCount >= 5);
    }

    [Fact]
    public async Task UpsertAsync_WithMultipleUsers_IsolatesData()
    {
        var service = CreateEqualizerPresetService();
        var user1 = 15;
        var user2 = 16;
        var bands = CreateTestBands();

        await service.UpsertAsync(user1, "User1 Preset", bands, false);
        await service.UpsertAsync(user2, "User1 Preset", bands, false);

        var user1Presets = await service.ListAsync(user1, new PagedRequest { Page = 1, PageSize = 10 });
        var user2Presets = await service.ListAsync(user2, new PagedRequest { Page = 1, PageSize = 10 });

        Assert.NotEmpty(user1Presets.Data);
        Assert.NotEmpty(user2Presets.Data);
        Assert.All(user1Presets.Data, p => Assert.Equal(user1, p.UserId));
        Assert.All(user2Presets.Data, p => Assert.Equal(user2, p.UserId));
    }
}

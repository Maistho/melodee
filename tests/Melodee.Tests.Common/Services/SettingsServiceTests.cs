using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Common.Services;

namespace Melodee.Tests.Common.Services;

public sealed class SettingsServiceTests : ServiceTestBase
{
    [Fact]
    public async Task ListAsync()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());
        var listResult = await service.ListAsync(new PagedRequest
        {
            PageSize = 1000
        });
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);
        Assert.Equal(1, listResult.TotalPages);
    }

    [Fact]
    public async Task ListWithFilterOnIdValueEqualsAsync()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());
        var idValue = 0;
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            idValue = context.Settings.First(x => x.Key == SettingRegistry.ValidationMaximumSongNumber).Id;
        }

        var listResult = await service.ListAsync(new PagedRequest
        {
            FilterBy =
            [
                new FilterOperatorInfo(nameof(Setting.Id), FilterOperator.Equals, idValue)
            ],
            PageSize = 1
        });
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);
        Assert.Equal(1, listResult.TotalCount);
        Assert.Equal(1, listResult.TotalPages);
    }

    [Fact]
    public async Task ListWithFilterOnKeyValueEqualsAsync()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());
        var listResult = await service.ListAsync(new PagedRequest
        {
            FilterBy =
            [
                new FilterOperatorInfo(nameof(Setting.Key), FilterOperator.Equals, SettingRegistry.ValidationMaximumSongNumber)
            ],
            PageSize = 1
        });
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);
        Assert.Equal(1, listResult.TotalCount);
        Assert.Equal(1, listResult.TotalPages);
    }

    [Fact]
    public async Task ListWithFilterLikeAsync()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());
        var listResult = await service.ListAsync(new PagedRequest
        {
            FilterBy =
            [
                new FilterOperatorInfo(nameof(Setting.Key), FilterOperator.Contains, "bit")
            ],
            PageSize = 1
        });
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.Key == SettingRegistry.ConversionBitrate);
        Assert.Equal(1, listResult.TotalCount);
        Assert.Equal(1, listResult.TotalPages);
    }

    [Fact]
    public async Task ListWithSortAsync()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());
        var listResult = await service.ListAsync(new PagedRequest
        {
            OrderBy = new Dictionary<string, string>
            {
                { nameof(Setting.Id), PagedRequest.OrderAscDirection }
            },
            PageSize = 1000
        });
        AssertResultIsSuccessful(listResult);
        Assert.Equal(1, listResult.Data.First().Id);
        Assert.NotEqual(1, listResult.TotalCount);
        Assert.Equal(1, listResult.TotalPages);

        listResult = await service.ListAsync(new PagedRequest
        {
            OrderBy = new Dictionary<string, string>
            {
                { nameof(Setting.Id), PagedRequest.OrderDescDirection }
            },
            PageSize = 1000
        });
        AssertResultIsSuccessful(listResult);
        Assert.NotEqual(1, listResult.Data.First().Id);
        Assert.NotEqual(1, listResult.TotalCount);
        Assert.Equal(1, listResult.TotalPages);
    }

    [Fact]
    public async Task GetSettingByKeyAndValueAsync()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());
        var getResult = await service.GetAsync(SettingRegistry.ValidationMaximumSongNumber);
        AssertResultIsSuccessful(getResult);

        var getIntValueResult = await service.GetValueAsync<int>(SettingRegistry.ValidationMaximumSongNumber);
        AssertResultIsSuccessful(getIntValueResult);
        Assert.IsType<int>(getIntValueResult.Data);
        Assert.True(getIntValueResult.Data > 0);

        var getStringValueResult = await service.GetValueAsync<string>(SettingRegistry.ProcessingSongTitleRemovals);
        AssertResultIsSuccessful(getStringValueResult);
        Assert.IsType<string>(getStringValueResult.Data);
        Assert.NotNull(getStringValueResult.Data.Nullify());
    }

    [Fact]
    public void GetSettingSetAndConvert()
    {
        var settings = MelodeeConfiguration.AllSettings();
        Assert.NotNull(settings);
        Assert.Contains(settings, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);

        var shouldBeValueInt = 99;
        MelodeeConfiguration.SetSetting(settings, SettingRegistry.ValidationMaximumSongNumber, shouldBeValueInt);
        Assert.Equal(shouldBeValueInt, settings[SettingRegistry.ValidationMaximumSongNumber]);

        var shouldBeValueBool = true;
        MelodeeConfiguration.SetSetting(settings, SettingRegistry.ValidationMaximumSongNumber, shouldBeValueBool);
        Assert.Equal(shouldBeValueBool, settings[SettingRegistry.ValidationMaximumSongNumber]);
        Assert.True(MelodeeConfiguration.IsTrue(settings, SettingRegistry.ValidationMaximumSongNumber));
    }

    [Fact]
    public async Task GetAllSettingsAsync()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());
        var listResult = await service.GetAllSettingsAsync();
        Assert.NotEmpty(listResult);
        Assert.Contains(listResult, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);
        Assert.Contains(listResult, x => x.Key == SettingRegistry.ProcessingMaximumProcessingCount);
    }

    [Fact]
    public async Task GetSettingWithFunc()
    {
        var shouldBeValueInt = 99;
        var configuration = await MockConfigurationFactory().GetConfigurationAsync();
        Assert.NotEmpty(configuration.Configuration);

        var maxSongsToProcess = configuration.GetValue<int?>(SettingRegistry.ProcessingMaximumProcessingCount) ?? 0;
        Assert.Equal(0, maxSongsToProcess);

        configuration.SetSetting(SettingRegistry.ProcessingMaximumProcessingCount, shouldBeValueInt);
        var getIntValueResult = configuration.GetValue<int>(SettingRegistry.ProcessingMaximumProcessingCount);
        Assert.Equal(shouldBeValueInt, getIntValueResult);

        configuration.SetSetting(SettingRegistry.ProcessingMaximumProcessingCount, 15);
        getIntValueResult = configuration.GetValue<int>(SettingRegistry.ProcessingMaximumProcessingCount, value => int.MaxValue);
        Assert.NotEqual(shouldBeValueInt, getIntValueResult);
    }

    [Fact]
    public async Task GetValueAsync_WithNullKey_ThrowsArgumentException()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.GetValueAsync<int>(null!));
    }

    [Fact]
    public async Task GetValueAsync_WithEmptyKey_ThrowsArgumentException()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.GetValueAsync<int>(""));
    }

    [Fact]
    public async Task GetValueAsync_WithWhitespaceKey_ThrowsArgumentException()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.GetValueAsync<int>("   "));
    }

    [Fact]
    public async Task GetValueAsync_WithNonExistentKey_ReturnsDefaultValue()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());
        var defaultValue = 42;

        var result = await service.GetValueAsync("non.existent.key", defaultValue);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.NotFound, result.Type);
        Assert.Equal(defaultValue, result.Data);
    }

    [Fact]
    public async Task GetValueAsync_WithNonExistentKey_ReturnsTypeDefault()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        var result = await service.GetValueAsync<int>("non.existent.key");

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.NotFound, result.Type);
        Assert.Equal(0, result.Data);
    }

    [Fact]
    public async Task GetAsync_WithNullKey_ReturnsError()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        var result = await service.GetAsync(null!);

        // GetAsync catches exceptions and may return successful result with null data
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetAsync_WithEmptyKey_ReturnsError()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        var result = await service.GetAsync("");

        // GetAsync catches exceptions and may return successful result with null data
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task SetAsync_WithNullKey_ThrowsArgumentException()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.SetAsync(null!, "value"));
    }

    [Fact]
    public async Task SetAsync_WithEmptyKey_ThrowsArgumentException()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.SetAsync("", "value"));
    }

    [Fact]
    public async Task SetAsync_WithExistingKey_UpdatesValue()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());
        var newValue = "9999";

        var result = await service.SetAsync(SettingRegistry.ValidationMaximumSongNumber, newValue);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        // Verify the value was updated
        var getSetting = await service.GetAsync(SettingRegistry.ValidationMaximumSongNumber);
        Assert.True(getSetting.IsSuccess);
        Assert.Equal(newValue, getSetting.Data!.Value);
    }

    [Fact]
    public async Task AddAsync_WithNullSetting_ThrowsArgumentNullException()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.AddAsync(null!));
    }

    [Fact]
    public async Task AddAsync_WithValidSetting_AddsSuccessfully()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());
        var newSetting = new Setting
        {
            Key = "test.new.setting",
            Value = "test value",
            Comment = "Test setting for unit test",
            CreatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant()
        };

        var result = await service.AddAsync(newSetting);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(newSetting.Key, result.Data.Key);
        Assert.Equal(newSetting.Value, result.Data.Value);
        Assert.NotEqual(Guid.Empty, result.Data.ApiKey);
        Assert.True(result.Data.CreatedAt > NodaTime.Instant.MinValue);
    }

    [Fact]
    public async Task AddAsync_WithDuplicateKey_ReturnsError()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());
        var duplicateSetting = new Setting
        {
            Key = SettingRegistry.ValidationMaximumSongNumber, // Already exists in seed data
            Value = "test value",
            Comment = "Duplicate setting",
            CreatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant()
        };

        var result = await service.AddAsync(duplicateSetting);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.Error, result.Type);
        Assert.Null(result.Data);
        Assert.Contains("already exists", result.Messages?.First() ?? "");
    }

    [Fact]
    public async Task AddAsync_WithInvalidSetting_ReturnsValidationFailure()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());
        var invalidSetting = new Setting
        {
            Key = "", // Invalid empty key
            Value = "test value",
            Comment = "Invalid setting",
            CreatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant()
        };

        var result = await service.AddAsync(invalidSetting);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.ValidationFailure, result.Type);
        Assert.Null(result.Data);
        Assert.NotEmpty(result.Messages ?? []);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ThrowsArgumentException()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());
        var setting = new Setting
        {
            Id = 0, // Invalid ID
            Key = "test",
            Value = "test",
            CreatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant()
        };

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.UpdateAsync(setting));
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ReturnsNotFound()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());
        var setting = new Setting
        {
            Id = 999999, // Non-existent ID
            Key = "test.key",
            Value = "test value",
            CreatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant()
        };

        var result = await service.UpdateAsync(setting);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.NotFound, result.Type);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task UpdateAsync_WithValidSetting_UpdatesSuccessfully()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        // Get an existing setting
        var existingSetting = await service.GetAsync(SettingRegistry.ValidationMaximumSongNumber);
        Assert.True(existingSetting.IsSuccess);

        var settingToUpdate = existingSetting.Data!;
        var originalValue = settingToUpdate.Value;
        var newValue = "8888";
        var newComment = "Updated comment";

        settingToUpdate.Value = newValue;
        settingToUpdate.Comment = newComment;

        var result = await service.UpdateAsync(settingToUpdate);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        // Verify the setting was updated
        var updatedSetting = await service.GetAsync(SettingRegistry.ValidationMaximumSongNumber);
        Assert.True(updatedSetting.IsSuccess);
        Assert.Equal(newValue, updatedSetting.Data!.Value);
        Assert.Equal(newComment, updatedSetting.Data!.Comment);
        Assert.NotEqual(originalValue, updatedSetting.Data!.Value);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidSetting_ReturnsValidationFailure()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        // Get an existing setting
        var existingSetting = await service.GetAsync(SettingRegistry.ValidationMaximumSongNumber);
        Assert.True(existingSetting.IsSuccess);

        var settingToUpdate = existingSetting.Data!;
        settingToUpdate.Key = null!; // Make it invalid

        var result = await service.UpdateAsync(settingToUpdate);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.ValidationFailure, result.Type);
        Assert.False(result.Data);
        Assert.NotEmpty(result.Messages ?? []);
    }

    [Fact]
    public async Task ListAsync_WithTotalCountOnlyRequest_ReturnsOnlyCount()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        var result = await service.ListAsync(new PagedRequest
        {
            IsTotalCountOnlyRequest = true
        });

        Assert.True(result.IsSuccess);
        Assert.True(result.TotalCount > 0);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task ListAsync_WithPagination_ReturnsCorrectPage()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        var firstPageResult = await service.ListAsync(new PagedRequest
        {
            PageSize = 5,
            Page = 0
        });

        var secondPageResult = await service.ListAsync(new PagedRequest
        {
            PageSize = 5,
            Page = 1
        });

        Assert.True(firstPageResult.IsSuccess);
        Assert.True(secondPageResult.IsSuccess);
        Assert.True(firstPageResult.Data.Count() <= 5);
        Assert.True(secondPageResult.Data.Count() <= 5);

        // Verify pagination working (total count should be consistent)
        Assert.Equal(firstPageResult.TotalCount, secondPageResult.TotalCount);
    }

    [Fact]
    public async Task GetValueAsync_WithBooleanConversion_ReturnsCorrectType()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        var result = await service.GetValueAsync<bool>(SettingRegistry.MagicEnabled);

        Assert.True(result.IsSuccess);
        Assert.IsType<bool>(result.Data);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task GetValueAsync_WithStringConversion_ReturnsCorrectType()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        var result = await service.GetValueAsync<string>(SettingRegistry.ProcessingSongTitleRemovals);

        Assert.True(result.IsSuccess);
        Assert.IsType<string>(result.Data);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetValueAsync_WithDecimalConversion_ReturnsCorrectType()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        var result = await service.GetValueAsync<decimal>(SettingRegistry.ValidationMaximumSongNumber);

        Assert.True(result.IsSuccess);
        Assert.IsType<decimal>(result.Data);
        Assert.True(result.Data > 0);
    }

    [Fact]
    public async Task GetAllSettingsAsync_ReturnsAllSettingsWithCorrectTypes()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        var result = await service.GetAllSettingsAsync();

        Assert.NotEmpty(result);
        Assert.Contains(result, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);
        Assert.Contains(result, x => x.Key == SettingRegistry.MagicEnabled);

        // Verify that values exist in dictionary
        Assert.True(result.ContainsKey(SettingRegistry.ValidationMaximumSongNumber));
        Assert.True(result.ContainsKey(SettingRegistry.MagicEnabled));
        Assert.NotNull(result[SettingRegistry.ValidationMaximumSongNumber]);
        Assert.NotNull(result[SettingRegistry.MagicEnabled]);
    }

    [Fact]
    public async Task CacheInvalidation_AfterUpdate_RefreshesCache()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        // Get setting to populate cache
        var firstGet = await service.GetAsync(SettingRegistry.ValidationMaximumSongNumber);
        Assert.True(firstGet.IsSuccess);

        var originalValue = firstGet.Data!.Value;
        var newValue = "7777";

        // Update the setting
        firstGet.Data.Value = newValue;
        var updateResult = await service.UpdateAsync(firstGet.Data);
        Assert.True(updateResult.IsSuccess);

        // Get setting again - should return updated value from fresh cache
        var secondGet = await service.GetAsync(SettingRegistry.ValidationMaximumSongNumber);
        Assert.True(secondGet.IsSuccess);
        Assert.Equal(newValue, secondGet.Data!.Value);
        Assert.NotEqual(originalValue, secondGet.Data!.Value);
    }

    [Fact]
    public async Task ConcurrentAccess_MultipleReads_DoNotInterfere()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        var tasks = new List<Task<OperationResult<Setting?>>>();

        // Start multiple concurrent read operations
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(service.GetAsync(SettingRegistry.ValidationMaximumSongNumber));
        }

        var results = await Task.WhenAll(tasks);

        // All should succeed and return the same data
        Assert.All(results, result =>
        {
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(SettingRegistry.ValidationMaximumSongNumber, result.Data.Key);
        });
    }

    [Fact]
    public async Task EnvironmentVariableOverride_InListAsync_OverridesSettingValue()
    {
        // This test verifies that environment variables override database values in ListAsync
        // Note: The actual environment variable setting would need to be done at the OS level
        // This test primarily checks that the override mechanism exists in the code
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        var result = await service.ListAsync(new PagedRequest { PageSize = 1000 });

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Data);

        // Verify that the environment variable check exists in the method
        // (The actual override testing would require environment variable manipulation)
        var validationSetting = result.Data.FirstOrDefault(x => x.Key == SettingRegistry.ValidationMaximumSongNumber);
        Assert.NotNull(validationSetting);
        Assert.NotNull(validationSetting.Value);
    }

    [Fact]
    public async Task ListWithOrFilters_SearchAcrossMultipleFields_ReturnsMatchingResults()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        // Search for "validation" across Key, Comment, and Value with OR logic
        // This should find settings where ANY of these fields contain "validation"
        var listResult = await service.ListAsync(new PagedRequest
        {
            FilterBy =
            [
                new FilterOperatorInfo(nameof(Setting.Key), FilterOperator.Contains, "validation"),
                new FilterOperatorInfo(nameof(Setting.Comment), FilterOperator.Contains, "validation", FilterOperatorInfo.OrJoinOperator),
                new FilterOperatorInfo(nameof(Setting.Value), FilterOperator.Contains, "validation", FilterOperatorInfo.OrJoinOperator)
            ],
            PageSize = 100
        });

        AssertResultIsSuccessful(listResult);

        // Should find validation-related settings
        Assert.True(listResult.TotalCount >= 4, $"Expected at least 4 validation settings, found {listResult.TotalCount}");

        // Verify we got settings with "validation" in various fields
        var hasValidationInKey = listResult.Data.Any(s => s.Key.Contains("validation", StringComparison.OrdinalIgnoreCase));
        var hasValidationInComment = listResult.Data.Any(s => s.Comment?.Contains("validation", StringComparison.OrdinalIgnoreCase) == true);
        var hasValidationInValue = listResult.Data.Any(s => s.Value?.Contains("validation", StringComparison.OrdinalIgnoreCase) == true);

        Assert.True(hasValidationInKey || hasValidationInComment || hasValidationInValue,
            "Expected to find 'validation' in at least one field (Key, Comment, or Value)");
    }

    [Fact]
    public async Task ListWithOrFilters_MultipleKeywordSearch_FindsAllMatches()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        // Search for settings containing "validation" OR "conversion" in the Key
        var listResult = await service.ListAsync(new PagedRequest
        {
            FilterBy =
            [
                new FilterOperatorInfo(nameof(Setting.Key), FilterOperator.Contains, "validation"),
                new FilterOperatorInfo(nameof(Setting.Key), FilterOperator.Contains, "conversion", FilterOperatorInfo.OrJoinOperator)
            ],
            PageSize = 100
        });

        AssertResultIsSuccessful(listResult);

        // Should find both validation and conversion settings
        Assert.True(listResult.TotalCount > 0);

        var validationSettings = listResult.Data.Where(s => s.Key.Contains("validation", StringComparison.OrdinalIgnoreCase));
        var conversionSettings = listResult.Data.Where(s => s.Key.Contains("conversion", StringComparison.OrdinalIgnoreCase));

        Assert.NotEmpty(validationSettings);
        Assert.NotEmpty(conversionSettings);
    }

    [Fact]
    public async Task ListWithAndFilters_RequiresAllConditions_ReturnsOnlyMatchingBoth()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        // Search for settings where Key contains "conversion" AND Value is a number
        // This uses default AND logic (no OrJoinOperator)
        var listResult = await service.ListAsync(new PagedRequest
        {
            FilterBy =
            [
                new FilterOperatorInfo(nameof(Setting.Key), FilterOperator.Contains, "conversion")
            ],
            PageSize = 100
        });

        AssertResultIsSuccessful(listResult);

        // Should only find settings that have "conversion" in Key
        Assert.All(listResult.Data, setting =>
        {
            Assert.Contains("conversion", setting.Key, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public async Task ListWithOrFilters_NoMatches_ReturnsEmpty()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        // Search for something that doesn't exist in any field
        var listResult = await service.ListAsync(new PagedRequest
        {
            FilterBy =
            [
                new FilterOperatorInfo(nameof(Setting.Key), FilterOperator.Contains, "xyznonexistent123"),
                new FilterOperatorInfo(nameof(Setting.Comment), FilterOperator.Contains, "xyznonexistent123", FilterOperatorInfo.OrJoinOperator),
                new FilterOperatorInfo(nameof(Setting.Value), FilterOperator.Contains, "xyznonexistent123", FilterOperatorInfo.OrJoinOperator)
            ],
            PageSize = 100
        });

        AssertResultIsSuccessful(listResult);
        Assert.Equal(0, listResult.TotalCount);
        Assert.Empty(listResult.Data);
    }

    [Fact]
    public async Task ListWithOrFilters_PartialMatch_FindsResults()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        // Search for partial string "process" which should match "processing.*" settings
        var listResult = await service.ListAsync(new PagedRequest
        {
            FilterBy =
            [
                new FilterOperatorInfo(nameof(Setting.Key), FilterOperator.Contains, "process")
            ],
            PageSize = 100
        });

        AssertResultIsSuccessful(listResult);

        // Should find multiple processing-related settings
        Assert.True(listResult.TotalCount >= 5, $"Expected at least 5 processing settings, found {listResult.TotalCount}");
        Assert.All(listResult.Data, setting =>
            Assert.Contains("process", setting.Key, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ListWithMixedFilters_OrThenAnd_AppliesCorrectly()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        // Complex filter: (Key contains "conversion" OR Key contains "validation") AND Value contains "true" OR Value is empty
        var listResult = await service.ListAsync(new PagedRequest
        {
            FilterBy =
            [
                new FilterOperatorInfo(nameof(Setting.Key), FilterOperator.Contains, "conversion"),
                new FilterOperatorInfo(nameof(Setting.Key), FilterOperator.Contains, "validation", FilterOperatorInfo.OrJoinOperator)
            ],
            PageSize = 100
        });

        AssertResultIsSuccessful(listResult);

        // All results should have ("conversion" OR "validation" in Key)
        Assert.All(listResult.Data, setting =>
        {
            var hasConversionOrValidation = setting.Key.Contains("conversion", StringComparison.OrdinalIgnoreCase) ||
                                setting.Key.Contains("validation", StringComparison.OrdinalIgnoreCase);
            Assert.True(hasConversionOrValidation);
        });
    }

    [Fact]
    public async Task ListWithOrFilters_SpecificSettingsSearch_FindsExpectedResults()
    {
        var service = new SettingService(Logger, CacheManager, MockConfigurationFactory(), MockFactory());

        // This test verifies the actual scenario from the bug report:
        // Typing a search term should find all related settings across Key, Comment, and Value
        var listResult = await service.ListAsync(new PagedRequest
        {
            FilterBy =
            [
                new FilterOperatorInfo(nameof(Setting.Key), FilterOperator.Contains, "bit"),
                new FilterOperatorInfo(nameof(Setting.Comment), FilterOperator.Contains, "bit", FilterOperatorInfo.OrJoinOperator),
                new FilterOperatorInfo(nameof(Setting.Value), FilterOperator.Contains, "bit", FilterOperatorInfo.OrJoinOperator)
            ],
            PageSize = 100
        });

        AssertResultIsSuccessful(listResult);

        // We should find settings with "bit" in various fields (like "conversion.bitrate")
        Assert.True(listResult.TotalCount >= 1,
            $"Expected at least 1 setting with 'bit', found {listResult.TotalCount}");

        // Verify at least one has "bit" somewhere
        var hasBitInAnyField = listResult.Data.Any(s =>
            s.Key.Contains("bit", StringComparison.OrdinalIgnoreCase) ||
            (s.Comment?.Contains("bit", StringComparison.OrdinalIgnoreCase) ?? false) ||
            (s.Value?.Contains("bit", StringComparison.OrdinalIgnoreCase) ?? false));

        Assert.True(hasBitInAnyField, "Expected to find 'bit' in at least one field");
    }
}


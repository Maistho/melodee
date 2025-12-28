using Melodee.Common.Data.Models;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using NodaTime;

namespace Melodee.Tests.Common.Services;

public class RadioStationServiceFilteringTests : ServiceTestBase
{
    private List<RadioStation> CreateTestRadioStations()
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        return
        [
            new RadioStation
            {
                Id = 1,
                Name = "Classic Rock Station",
                StreamUrl = "http://stream.classicrock.com/live",
                HomePageUrl = "http://classicrock.com",
                Description = "The best classic rock hits",
                Tags = "rock|classic|oldies",
                IsLocked = false,
                SortOrder = 1,
                CreatedAt = now
            },
            new RadioStation
            {
                Id = 2,
                Name = "Jazz FM",
                StreamUrl = "http://stream.jazzfm.com/live",
                HomePageUrl = "http://jazzfm.com",
                Description = "Smooth jazz all day",
                Tags = "jazz|smooth|instrumental",
                IsLocked = true,
                SortOrder = 2,
                CreatedAt = now
            },
            new RadioStation
            {
                Id = 3,
                Name = "Electronic Beats",
                StreamUrl = "http://stream.electronicbeats.net/main",
                HomePageUrl = "http://electronicbeats.net",
                Description = "Electronic and dance music",
                Tags = "electronic|dance|edm",
                IsLocked = false,
                SortOrder = 3,
                CreatedAt = now
            },
            new RadioStation
            {
                Id = 4,
                Name = "Country Classics",
                StreamUrl = "http://stream.country.com/classics",
                HomePageUrl = null,
                Description = null,
                Tags = null,
                IsLocked = false,
                SortOrder = 4,
                CreatedAt = now
            },
            new RadioStation
            {
                Id = 5,
                Name = "Rock Alternative",
                StreamUrl = "http://stream.rockalt.com/live",
                HomePageUrl = "http://rockalt.com",
                Description = "Alternative rock and indie",
                Tags = "rock|alternative|indie",
                IsLocked = true,
                SortOrder = 5,
                CreatedAt = now
            }
        ];
    }

    private async Task<List<RadioStation>> SeedTestStationsAsync()
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var stations = CreateTestRadioStations();
        context.RadioStations.AddRange(stations);
        await context.SaveChangesAsync();
        return stations;
    }

    #region No Filters Tests

    [Fact]
    public async Task ListAsync_NoFilters_ReturnsAllStations()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest { PageSize = 100 };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(5, result.Data.Count());
    }

    [Fact]
    public async Task ListAsync_EmptyFilterArray_ReturnsAllStations()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy = []
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.TotalCount);
    }

    #endregion

    #region Name Filters

    [Fact]
    public async Task ListAsync_FilterByName_Contains_FindsMatches()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("name", FilterOperator.Contains, "rock")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.TotalCount); // Classic Rock Station and Rock Alternative
        var names = result.Data.Select(s => s.Name).ToList();
        Assert.Contains("Classic Rock Station", names);
        Assert.Contains("Rock Alternative", names);
    }

    [Fact]
    public async Task ListAsync_FilterByName_Equals_FindsExactMatch()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("name", FilterOperator.Equals, "jazz fm")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Jazz FM", result.Data.First().Name);
    }

    [Fact]
    public async Task ListAsync_FilterByName_StartsWith_FindsMatches()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("name", FilterOperator.StartsWith, "classic")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Classic Rock Station", result.Data.First().Name);
    }

    [Fact]
    public async Task ListAsync_FilterByName_EndsWith_FindsMatches()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("name", FilterOperator.EndsWith, "fm")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Jazz FM", result.Data.First().Name);
    }

    [Fact]
    public async Task ListAsync_FilterByName_NotEquals_ExcludesMatch()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("name", FilterOperator.NotEquals, "jazz fm")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.TotalCount);
        Assert.DoesNotContain(result.Data, s => s.Name.Equals("Jazz FM", StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region StreamUrl Filters

    [Fact]
    public async Task ListAsync_FilterByStreamUrl_Contains_FindsMatches()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("streamurl", FilterOperator.Contains, "jazzfm")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.TotalCount);
        Assert.Contains("jazzfm", result.Data.First().StreamUrl, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ListAsync_FilterByStreamUrl_Equals_FindsExactMatch()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("streamurl", FilterOperator.Equals, "http://stream.jazzfm.com/live")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task ListAsync_FilterByStreamUrl_StartsWith_FindsMatches()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("streamurl", FilterOperator.StartsWith, "http://stream.jazz")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task ListAsync_FilterByStreamUrl_EndsWith_FindsMatches()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("streamurl", FilterOperator.EndsWith, "/live")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.TotalCount); // Country Classics does not end with /live, only 3 of 5 do
    }

    #endregion

    #region HomePageUrl Filters (Nullable Field)

    [Fact]
    public async Task ListAsync_FilterByHomePageUrl_Contains_HandlesNullValues()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("homepageurl", FilterOperator.Contains, "rock")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.TotalCount); // Classic Rock and Rock Alternative, Country Classics has null HomePageUrl
        Assert.All(result.Data, s => Assert.NotNull(s.HomePageUrl));
    }

    [Fact]
    public async Task ListAsync_FilterByHomePageUrl_NotEquals_IncludesNullValues()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("homepageurl", FilterOperator.NotEquals, "http://jazzfm.com")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.TotalCount); // All except Jazz FM (including null)
        Assert.DoesNotContain(result.Data, s => s.Name == "Jazz FM");
    }

    #endregion

    #region Description Filters (Nullable Field)

    [Fact]
    public async Task ListAsync_FilterByDescription_Contains_HandlesNullValues()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("description", FilterOperator.Contains, "music")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Electronic Beats", result.Data.First().Name);
    }

    [Fact]
    public async Task ListAsync_FilterByDescription_NotEquals_IncludesNullValues()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("description", FilterOperator.NotEquals, "smooth jazz all day")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.TotalCount);
        Assert.DoesNotContain(result.Data, s => s.Name == "Jazz FM");
    }

    #endregion

    #region Tags Filters (Nullable Field)

    [Fact]
    public async Task ListAsync_FilterByTags_Contains_FindsMatches()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("tags", FilterOperator.Contains, "rock")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.TotalCount); // Classic Rock Station and Rock Alternative
    }

    [Fact]
    public async Task ListAsync_FilterByTags_Equals_FindsExactMatch()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("tags", FilterOperator.Equals, "jazz|smooth|instrumental")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Jazz FM", result.Data.First().Name);
    }

    #endregion

    #region IsLocked Filter (Boolean)

    [Fact]
    public async Task ListAsync_FilterByIsLocked_True_FindsLockedStations()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("islocked", FilterOperator.Equals, "true")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.TotalCount); // Jazz FM and Rock Alternative
        Assert.All(result.Data, s => Assert.True(s.IsLocked));
    }

    [Fact]
    public async Task ListAsync_FilterByIsLocked_False_FindsUnlockedStations()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("islocked", FilterOperator.Equals, "false")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.TotalCount);
        Assert.All(result.Data, s => Assert.False(s.IsLocked));
    }

    [Fact]
    public async Task ListAsync_FilterByIsLocked_NotEquals_FiltersCorrectly()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("islocked", FilterOperator.NotEquals, "true")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.TotalCount);
        Assert.All(result.Data, s => Assert.False(s.IsLocked));
    }

    [Fact]
    public async Task ListAsync_FilterByIsLocked_InvalidValue_ReturnsAllStations()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("islocked", FilterOperator.Equals, "not a boolean")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.TotalCount);
    }

    #endregion

    #region Multiple Filters (OR Logic)

    [Fact]
    public async Task ListAsync_MultipleFilters_CombinesWithOrLogic()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("name", FilterOperator.Contains, "jazz"),
                new FilterOperatorInfo("streamurl", FilterOperator.Contains, "electronic")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.TotalCount); // Jazz FM or Electronic Beats
        var names = result.Data.Select(s => s.Name).ToList();
        Assert.Contains("Jazz FM", names);
        Assert.Contains("Electronic Beats", names);
    }

    [Fact]
    public async Task ListAsync_MultipleFiltersOnSameProperty_CombinesWithOrLogic()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("name", FilterOperator.Contains, "jazz"),
                new FilterOperatorInfo("name", FilterOperator.Contains, "electronic")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.TotalCount); // Jazz FM or Electronic Beats
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ListAsync_FilterWithNullValue_TreatsAsEmptyString()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("name", FilterOperator.Contains, null!)
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.TotalCount); // All stations match empty string
    }

    [Fact]
    public async Task ListAsync_FilterWithEmptyValue_MatchesAll()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("name", FilterOperator.Contains, "")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.TotalCount);
    }

    [Fact]
    public async Task ListAsync_FilterByUnknownProperty_ReturnsAllStations()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("unknownproperty", FilterOperator.Contains, "test")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.TotalCount);
    }

    [Fact]
    public async Task ListAsync_FilterWithUnsupportedOperator_ReturnsAllStations()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            FilterBy =
            [
                new FilterOperatorInfo("name", FilterOperator.GreaterThan, "test") // Not supported for string fields
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.TotalCount);
    }

    #endregion

    #region Ordering Tests

    [Theory]
    [InlineData("name", "Classic Rock Station")]
    [InlineData("Name", "Classic Rock Station")]
    [InlineData("NAME", "Classic Rock Station")]
    public async Task ListAsync_OrderByName_Ascending_SortsCorrectly(string orderByField, string expectedFirst)
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            OrderBy = new Dictionary<string, string> { { orderByField, "ASC" } }
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedFirst, result.Data.First().Name);
    }

    [Fact]
    public async Task ListAsync_OrderByName_Descending_SortsCorrectly()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            OrderBy = new Dictionary<string, string> { { "name", "DESC" } }
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("Rock Alternative", result.Data.First().Name);
    }

    [Fact]
    public async Task ListAsync_OrderByStreamUrl_Ascending_SortsCorrectly()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            OrderBy = new Dictionary<string, string> { { "streamurl", "ASC" } }
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.StartsWith("http://stream.classic", result.Data.First().StreamUrl, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ListAsync_OrderByIsLocked_Ascending_SortsCorrectly()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            OrderBy = new Dictionary<string, string> { { "islocked", "ASC" } }
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.False(result.Data.First().IsLocked); // False comes before True
    }

    [Fact]
    public async Task ListAsync_OrderByIsLocked_Descending_SortsCorrectly()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            OrderBy = new Dictionary<string, string> { { "islocked", "DESC" } }
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data.First().IsLocked);
    }

    [Fact]
    public async Task ListAsync_OrderBySortOrder_Ascending_SortsCorrectly()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            OrderBy = new Dictionary<string, string> { { "sortorder", "ASC" } }
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data.First().SortOrder);
        Assert.Equal(5, result.Data.Last().SortOrder);
    }

    [Fact]
    public async Task ListAsync_OrderByUnknownField_DefaultsToId()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 100,
            OrderBy = new Dictionary<string, string> { { "unknownfield", "ASC" } }
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data.First().Id);
    }

    #endregion

    #region Filtering + Ordering + Paging Combined

    [Fact]
    public async Task ListAsync_FilterOrderAndPage_WorksTogether()
    {
        await SeedTestStationsAsync();
        var service = GetRadioStationService();
        var request = new PagedRequest
        {
            PageSize = 2,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "name", "ASC" } },
            FilterBy =
            [
                new FilterOperatorInfo("tags", FilterOperator.Contains, "rock")
            ]
        };

        var result = await service.ListAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(2, result.Data.Count());
        Assert.Equal("Classic Rock Station", result.Data.First().Name); // Alphabetically first
    }

    #endregion
}

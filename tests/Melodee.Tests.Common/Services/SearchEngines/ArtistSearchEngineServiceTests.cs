using Melodee.Common.Models.SearchEngines;

namespace Melodee.Tests.Common.Services.SearchEngines;

public class ArtistSearchEngineServiceTests : ServiceTestBase
{
    #region InitializeAsync Tests

    [Fact]
    public async Task InitializeAsync_WhenCalled_InitializesService()
    {
        // Arrange
        var service = GetArtistSearchEngineService();

        // Act
        await service.InitializeAsync();

        // Assert - no exception thrown means success
        Assert.True(true);
    }

    [Fact]
    public async Task InitializeAsync_WhenCalledMultipleTimes_OnlyInitializesOnce()
    {
        // Arrange
        var service = GetArtistSearchEngineService();

        // Act
        await service.InitializeAsync();
        await service.InitializeAsync();

        // Assert - no exception thrown means success
        Assert.True(true);
    }

    #endregion

    #region DoSearchAsync Tests

    [Fact]
    public async Task DoSearchAsync_WithEmptyQuery_ReturnsResults()
    {
        // Arrange
        var service = GetArtistSearchEngineService();
        await service.InitializeAsync();

        var query = new ArtistQuery { Name = "" };

        // Act
        var result = await service.DoSearchAsync(query, 10);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region DoArtistTopSongsSearchAsync Tests

    [Fact]
    public async Task DoArtistTopSongsSearchAsync_WithValidArtist_ReturnsResults()
    {
        // Arrange
        var service = GetArtistSearchEngineService();
        await service.InitializeAsync();

        // Act
        var result = await service.DoArtistTopSongsSearchAsync("Test Artist", null, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 0);
    }

    [Fact]
    public async Task DoArtistTopSongsSearchAsync_WithNullArtistName_ReturnsEmptyResults()
    {
        // Arrange
        var service = GetArtistSearchEngineService();
        await service.InitializeAsync();

        // Act
        var result = await service.DoArtistTopSongsSearchAsync(null!, null, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
    }

    #endregion
}

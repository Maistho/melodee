using Melodee.Common.Plugins.SearchEngine;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Moq;

namespace Melodee.Tests.Common.Plugins.SearchEngine;

public class SearchEnginePluginSortOrderTests
{
    [Fact]
    public void MelodeeArtistSearchEnginePlugin_SortOrder_IsZero()
    {
        var mockContextFactory = new Mock<Microsoft.EntityFrameworkCore.IDbContextFactory<Melodee.Common.Data.MelodeeDbContext>>();
        var plugin = new MelodeeArtistSearchEnginePlugin(mockContextFactory.Object);

        Assert.Equal(0, plugin.SortOrder);
    }

    [Fact]
    public void MusicBrainzArtistSearchEnginePlugin_SortOrder_IsOne()
    {
        var mockRepository = new Mock<IMusicBrainzRepository>();
        var plugin = new MusicBrainzArtistSearchEnginePlugin(mockRepository.Object);

        Assert.Equal(1, plugin.SortOrder);
    }

    [Fact]
    public void SearchEnginePlugins_SortOrder_MelodeeIsFirst()
    {
        var mockContextFactory = new Mock<Microsoft.EntityFrameworkCore.IDbContextFactory<Melodee.Common.Data.MelodeeDbContext>>();
        var mockRepository = new Mock<IMusicBrainzRepository>();

        var melodeePlugin = new MelodeeArtistSearchEnginePlugin(mockContextFactory.Object);
        var musicBrainzPlugin = new MusicBrainzArtistSearchEnginePlugin(mockRepository.Object);

        var plugins = new IArtistSearchEnginePlugin[] { musicBrainzPlugin, melodeePlugin };
        var orderedPlugins = plugins.OrderBy(p => p.SortOrder).ToArray();

        Assert.Equal(melodeePlugin, orderedPlugins[0]);
        Assert.Equal(musicBrainzPlugin, orderedPlugins[1]);
    }

    [Fact]
    public void SearchEnginePlugins_SortOrder_LocalDatabasesBeforeExternalAPIs()
    {
        var mockContextFactory = new Mock<Microsoft.EntityFrameworkCore.IDbContextFactory<Melodee.Common.Data.MelodeeDbContext>>();
        var mockRepository = new Mock<IMusicBrainzRepository>();

        var melodeePlugin = new MelodeeArtistSearchEnginePlugin(mockContextFactory.Object);
        var musicBrainzPlugin = new MusicBrainzArtistSearchEnginePlugin(mockRepository.Object);

        // Melodee (local DB) should be 0, MusicBrainz (local DB) should be 1
        // External APIs like Spotify should be much higher (100+)
        Assert.True(melodeePlugin.SortOrder < 10, "Local Melodee DB should have low sort order");
        Assert.True(musicBrainzPlugin.SortOrder < 10, "Local MusicBrainz DB should have low sort order");
    }

    [Fact]
    public void MelodeeArtistSearchEnginePlugin_DisplayName_IsCorrect()
    {
        var mockContextFactory = new Mock<Microsoft.EntityFrameworkCore.IDbContextFactory<Melodee.Common.Data.MelodeeDbContext>>();
        var plugin = new MelodeeArtistSearchEnginePlugin(mockContextFactory.Object);

        Assert.Equal("Melodee Database", plugin.DisplayName);
    }

    [Fact]
    public void MusicBrainzArtistSearchEnginePlugin_DisplayName_IsCorrect()
    {
        var mockRepository = new Mock<IMusicBrainzRepository>();
        var plugin = new MusicBrainzArtistSearchEnginePlugin(mockRepository.Object);

        Assert.Equal("Music Brainz Database", plugin.DisplayName);
    }

    [Fact]
    public void MelodeeArtistSearchEnginePlugin_IsEnabled_DefaultsToTrue()
    {
        var mockContextFactory = new Mock<Microsoft.EntityFrameworkCore.IDbContextFactory<Melodee.Common.Data.MelodeeDbContext>>();
        var plugin = new MelodeeArtistSearchEnginePlugin(mockContextFactory.Object);

        Assert.True(plugin.IsEnabled);
    }

    [Fact]
    public void MusicBrainzArtistSearchEnginePlugin_IsEnabled_DefaultsToTrue()
    {
        var mockRepository = new Mock<IMusicBrainzRepository>();
        var plugin = new MusicBrainzArtistSearchEnginePlugin(mockRepository.Object);

        Assert.True(plugin.IsEnabled);
    }

    [Fact]
    public void MelodeeArtistSearchEnginePlugin_StopProcessing_IsFalse()
    {
        var mockContextFactory = new Mock<Microsoft.EntityFrameworkCore.IDbContextFactory<Melodee.Common.Data.MelodeeDbContext>>();
        var plugin = new MelodeeArtistSearchEnginePlugin(mockContextFactory.Object);

        Assert.False(plugin.StopProcessing);
    }

    [Fact]
    public void MusicBrainzArtistSearchEnginePlugin_StopProcessing_IsFalse()
    {
        var mockRepository = new Mock<IMusicBrainzRepository>();
        var plugin = new MusicBrainzArtistSearchEnginePlugin(mockRepository.Object);

        Assert.False(plugin.StopProcessing);
    }
}

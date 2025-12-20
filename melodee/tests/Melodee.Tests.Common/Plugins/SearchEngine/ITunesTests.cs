using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine.ITunes;
using Melodee.Common.Services.Caching;

namespace Melodee.Tests.Common.Plugins.SearchEngine;

public class ITunesTests : TestsBase
{
    [Fact]
    public async Task PerformITunesAlbumSearch()
    {
        using (var httpClient = new HttpClient())
        {
            var itunes = new ITunesSearchEngine(Logger, Serializer, new TestHttpClientFactory(httpClient),
                new FakeCacheManager(Logger, TimeSpan.FromMinutes(5), Serializer));
            var result = await itunes.DoAlbumImageSearch(new AlbumQuery
            {
                Year = 1983,
                Name = "Cargo",
                Artist = "Men At Work"
            }, 10);
            Assert.NotNull(result);
        }
    }
}

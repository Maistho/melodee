using FluentAssertions;
using Xunit.Abstractions;

namespace Melodee.Tests.OpenSubsonic.Endpoints;

public class BrowsingEndpointTests : OpenSubsonicTestBase
{
    public BrowsingEndpointTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetMusicFolders_ReturnsFolders()
    {
        await AssertEndpointConformsToSubsonicSchemaAsync("getMusicFolders", "getMusicFolders", "musicFolders");

        var response = await GetAsync("getMusicFolders");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"musicFolders\"");
    }

    [Fact]
    public async Task GetIndexes_ReturnsIndexes()
    {
        await AssertEndpointConformsToSubsonicSchemaAsync("getIndexes", "getIndexes", "indexes");

        var response = await GetAsync("getIndexes");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"indexes\"");
    }

    [Fact]
    public async Task GetArtists_ReturnsArtistsList()
    {
        await AssertEndpointConformsToSubsonicSchemaAsync("getArtists", "getArtists", "artists");

        var response = await GetAsync("getArtists");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"artists\"");
    }

    [Fact]
    public async Task GetArtists_WithSize_ReturnsPaginated()
    {
        await AssertEndpointConformsToSubsonicSchemaAsync("getArtists (paginated)", "getArtists?size=10", "artists");

        var response = await GetAsync("getArtists?size=10");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"artists\"");
    }

    [Fact]
    public async Task GetGenres_ReturnsGenreList()
    {
        await AssertEndpointConformsToSubsonicSchemaAsync("getGenres", "getGenres", "genres");

        var response = await GetAsync("getGenres");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"genres\"");
    }

    [Fact]
    public async Task GetMusicDirectory_ReturnsDirectory()
    {
        await AssertEndpointConformsToSubsonicSchemaAsync("getMusicDirectory", "getMusicDirectory", "directory");

        var response = await GetAsync("getMusicDirectory");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"directory\"");
    }
}

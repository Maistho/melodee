using FluentAssertions;
using Xunit.Abstractions;

namespace Melodee.Tests.OpenSubsonic.Endpoints;

public class UserDataEndpointTests : OpenSubsonicTestBase
{
    public UserDataEndpointTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetStarred_ReturnsStarredItems()
    {
        await AssertEndpointConformsToSubsonicSchemaAsync("getStarred", "getStarred", "starred");

        var response = await GetAsync("getStarred");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"starred\"");
    }

    [Fact]
    public async Task GetStarred2_ReturnsStarredItems()
    {
        await AssertEndpointConformsToSubsonicSchemaAsync("getStarred2", "getStarred2", "starred2");

        var response = await GetAsync("getStarred2");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"starred2\"");
    }

    [Fact]
    public async Task GetNowPlaying_ReturnsNowPlaying()
    {
        await AssertEndpointConformsToSubsonicSchemaAsync("getNowPlaying", "getNowPlaying", "nowPlaying");

        var response = await GetAsync("getNowPlaying");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"nowPlaying\"");
    }

    [Fact]
    public async Task GetUser_ReturnsUserInfo()
    {
        await AssertEndpointConformsToSubsonicSchemaAsync("getUser", $"getUser?username={TestUserName}", "user");

        var response = await GetAsync($"getUser?username={TestUserName}");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"user\"");
    }

    [Fact]
    public async Task GetAlbumList_ReturnsAlbums()
    {
        await AssertEndpointConformsToSubsonicSchemaAsync("getAlbumList", "getAlbumList?type=newest", "albumList");

        var response = await GetAsync("getAlbumList?type=newest");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"albumList\"");
    }

    [Fact]
    public async Task GetAlbumList2_ReturnsAlbums()
    {
        await AssertEndpointConformsToSubsonicSchemaAsync("getAlbumList2", "getAlbumList2?type=newest", "albumList2");

        var response = await GetAsync("getAlbumList2?type=newest");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"albumList2\"");
    }

    [Fact]
    public async Task GetRandomSongs_ReturnsSongs()
    {
        await AssertEndpointConformsToSubsonicSchemaAsync("getRandomSongs", "getRandomSongs", "randomSongs");

        var response = await GetAsync("getRandomSongs");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"randomSongs\"");
    }
}

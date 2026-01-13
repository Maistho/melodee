using FluentAssertions;
using Xunit.Abstractions;

namespace Melodee.Tests.OpenSubsonic.Endpoints;

public class PlaylistEndpointTests : OpenSubsonicTestBase
{
    public PlaylistEndpointTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetPlaylists_ReturnsPlaylists()
    {
        await AssertEndpointConformsToSubsonicSchemaAsync("getPlaylists", "getPlaylists", "playlists");

        var response = await GetAsync("getPlaylists");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"playlists\"");
    }

    [Fact]
    public async Task CreatePlaylist_WithName_CreatesPlaylist()
    {
        var playlistName = $"Test Playlist {Guid.NewGuid():N}";
        var response = await Client.GetAsync(
            $"/rest/createPlaylist?name={Uri.EscapeDataString(playlistName)}&u={TestUserName}&t={AuthToken}&s={AuthSalt}&v=1.16.1&c=test&f=json");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"playlist\"");
        content.Should().Contain(playlistName);
    }

    [Fact]
    public async Task GetPlaylist_ReturnsPlaylistDetails()
    {
        var playlistName = $"Test Playlist {Guid.NewGuid():N}";
        var createResponse = await Client.GetAsync(
            $"/rest/createPlaylist?name={Uri.EscapeDataString(playlistName)}&u={TestUserName}&t={AuthToken}&s={AuthSalt}&v=1.16.1&c=test&f=json");
        createResponse.EnsureSuccessStatusCode();

        var createContent = await createResponse.Content.ReadAsStringAsync();
        createContent.Should().Contain("\"playlist\"");

        var response = await GetAsync("getPlaylists");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"playlists\"");
        content.Should().Contain(playlistName);
    }
}

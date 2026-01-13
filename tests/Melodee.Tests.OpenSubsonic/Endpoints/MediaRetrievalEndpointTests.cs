using FluentAssertions;
using Xunit.Abstractions;

namespace Melodee.Tests.OpenSubsonic.Endpoints;

public class MediaRetrievalEndpointTests : OpenSubsonicTestBase
{
    public MediaRetrievalEndpointTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetCoverArt_ReturnsImage()
    {
        var response = await GetAsync("getCoverArt?id=album:00000000-0000-0000-0000-000000000001");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().StartWith("image/");
    }

    [Fact]
    public async Task GetCoverArt_WithSize_ReturnsSizedImage()
    {
        var response = await GetAsync("getCoverArt?id=album:00000000-0000-0000-0000-000000000001&size=300");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().StartWith("image/");
    }

    [Fact]
    public async Task GetAvatar_ReturnsAvatar()
    {
        var response = await GetAsync($"getAvatar?username={TestUserName}");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLyrics_ReturnsLyrics()
    {
        await AssertEndpointConformsToSubsonicSchemaAsync("getLyrics", "getLyrics", "lyrics");

        var response = await GetAsync("getLyrics");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"lyrics\"");
    }
}

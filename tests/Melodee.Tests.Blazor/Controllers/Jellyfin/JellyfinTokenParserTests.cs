using FluentAssertions;
using Melodee.Blazor.Controllers.Jellyfin;
using Microsoft.AspNetCore.Http;

namespace Melodee.Tests.Blazor.Controllers.Jellyfin;

public class JellyfinTokenParserTests
{
    [Fact]
    public void ParseFromRequest_AuthorizationHeader_ExtractsAllFields()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Append("Authorization",
            "MediaBrowser Client=\"TestClient\", Device=\"TestDevice\", DeviceId=\"device-123\", Version=\"1.0.0\", Token=\"test-token\"");

        var result = JellyfinTokenParser.ParseFromRequest(context.Request);

        result.Token.Should().Be("test-token");
        result.Client.Should().Be("TestClient");
        result.Device.Should().Be("TestDevice");
        result.DeviceId.Should().Be("device-123");
        result.Version.Should().Be("1.0.0");
    }

    [Fact]
    public void ParseFromRequest_XEmbyAuthorizationHeader_ExtractsAllFields()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Append("X-Emby-Authorization",
            "MediaBrowser Client=\"EmbyClient\", Device=\"EmbyDevice\", DeviceId=\"emby-123\", Version=\"2.0.0\", Token=\"emby-token\"");

        var result = JellyfinTokenParser.ParseFromRequest(context.Request);

        result.Token.Should().Be("emby-token");
        result.Client.Should().Be("EmbyClient");
        result.Device.Should().Be("EmbyDevice");
        result.DeviceId.Should().Be("emby-123");
        result.Version.Should().Be("2.0.0");
    }

    [Fact]
    public void ParseFromRequest_XMediaBrowserToken_ExtractsTokenOnly()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Append("X-MediaBrowser-Token", "simple-token");

        var result = JellyfinTokenParser.ParseFromRequest(context.Request);

        result.Token.Should().Be("simple-token");
        result.Client.Should().BeNull();
        result.Device.Should().BeNull();
        result.DeviceId.Should().BeNull();
        result.Version.Should().BeNull();
    }

    [Fact]
    public void ParseFromRequest_XEmbyToken_ExtractsTokenOnly()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Append("X-Emby-Token", "emby-simple-token");

        var result = JellyfinTokenParser.ParseFromRequest(context.Request);

        result.Token.Should().Be("emby-simple-token");
        result.Client.Should().BeNull();
        result.Device.Should().BeNull();
        result.DeviceId.Should().BeNull();
        result.Version.Should().BeNull();
    }

    [Fact]
    public void ParseFromRequest_NoHeaders_ReturnsNullToken()
    {
        var context = new DefaultHttpContext();

        var result = JellyfinTokenParser.ParseFromRequest(context.Request);

        result.Token.Should().BeNull();
        result.Client.Should().BeNull();
        result.Device.Should().BeNull();
        result.DeviceId.Should().BeNull();
        result.Version.Should().BeNull();
    }

    [Fact]
    public void ParseFromRequest_AuthorizationPriority_UsesAuthorizationFirst()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Append("Authorization", "MediaBrowser Token=\"auth-token\"");
        context.Request.Headers.Append("X-Emby-Token", "emby-token");

        var result = JellyfinTokenParser.ParseFromRequest(context.Request);

        result.Token.Should().Be("auth-token");
    }

    [Fact]
    public void ParseFromRequest_NonMediaBrowserAuthorization_FallsBackToOtherHeaders()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Append("Authorization", "Bearer jwt-token");
        context.Request.Headers.Append("X-Emby-Token", "fallback-token");

        var result = JellyfinTokenParser.ParseFromRequest(context.Request);

        result.Token.Should().Be("fallback-token");
    }

    [Fact]
    public void GenerateToken_ReturnsHexString()
    {
        var token = JellyfinTokenParser.GenerateToken();

        token.Should().NotBeNullOrWhiteSpace();
        token.Should().HaveLength(64); // 32 bytes = 64 hex chars
        token.Should().MatchRegex("^[0-9a-f]+$");
    }

    [Fact]
    public void GenerateSalt_ReturnsBase64String()
    {
        var salt = JellyfinTokenParser.GenerateSalt();

        salt.Should().NotBeNullOrWhiteSpace();
        Action decode = () => Convert.FromBase64String(salt);
        decode.Should().NotThrow();
    }

    [Fact]
    public void HashToken_DifferentSalts_ProduceDifferentHashes()
    {
        var token = "test-token";
        var pepper = "test-pepper";
        var salt1 = JellyfinTokenParser.GenerateSalt();
        var salt2 = JellyfinTokenParser.GenerateSalt();

        var hash1 = JellyfinTokenParser.HashToken(token, salt1, pepper);
        var hash2 = JellyfinTokenParser.HashToken(token, salt2, pepper);

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyToken_ValidToken_ReturnsTrue()
    {
        var token = JellyfinTokenParser.GenerateToken();
        var salt = JellyfinTokenParser.GenerateSalt();
        var pepper = "test-pepper";
        var hash = JellyfinTokenParser.HashToken(token, salt, pepper);

        var result = JellyfinTokenParser.VerifyToken(token, salt, pepper, hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyToken_InvalidToken_ReturnsFalse()
    {
        var token = JellyfinTokenParser.GenerateToken();
        var salt = JellyfinTokenParser.GenerateSalt();
        var pepper = "test-pepper";
        var hash = JellyfinTokenParser.HashToken(token, salt, pepper);

        var result = JellyfinTokenParser.VerifyToken("wrong-token", salt, pepper, hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyToken_WrongPepper_ReturnsFalse()
    {
        var token = JellyfinTokenParser.GenerateToken();
        var salt = JellyfinTokenParser.GenerateSalt();
        var hash = JellyfinTokenParser.HashToken(token, salt, "pepper1");

        var result = JellyfinTokenParser.VerifyToken(token, salt, "pepper2", hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyToken_WrongSalt_ReturnsFalse()
    {
        var token = JellyfinTokenParser.GenerateToken();
        var salt1 = JellyfinTokenParser.GenerateSalt();
        var salt2 = JellyfinTokenParser.GenerateSalt();
        var pepper = "test-pepper";
        var hash = JellyfinTokenParser.HashToken(token, salt1, pepper);

        var result = JellyfinTokenParser.VerifyToken(token, salt2, pepper, hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void GetTokenPrefix_ReturnsFirst8Characters()
    {
        var token = "abcdefghijklmnop";

        var prefix = JellyfinTokenParser.GetTokenPrefix(token);

        prefix.Should().Be("abcdefgh");
        prefix.Should().HaveLength(8);
    }

    [Fact]
    public void GetTokenPrefix_ShortToken_ReturnsFullToken()
    {
        var token = "abc";

        var prefix = JellyfinTokenParser.GetTokenPrefix(token);

        prefix.Should().Be("abc");
    }

    [Fact]
    public void GetTokenPrefix_Exactly8Characters_ReturnsFullToken()
    {
        var token = "12345678";

        var prefix = JellyfinTokenParser.GetTokenPrefix(token);

        prefix.Should().Be("12345678");
    }

    [Fact]
    public void GetTokenPrefix_GeneratedToken_ReturnsConsistentPrefix()
    {
        var token = JellyfinTokenParser.GenerateToken();

        var prefix1 = JellyfinTokenParser.GetTokenPrefix(token);
        var prefix2 = JellyfinTokenParser.GetTokenPrefix(token);

        prefix1.Should().Be(prefix2);
        prefix1.Should().HaveLength(8);
    }
}

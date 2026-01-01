using FluentAssertions;
using Melodee.Blazor.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Melodee.Tests.Blazor.Controllers.Jellyfin;

public class JellyfinRoutingMiddlewareTests
{
    private readonly Mock<ILogger<JellyfinRoutingMiddleware>> _loggerMock;

    public JellyfinRoutingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<JellyfinRoutingMiddleware>>();
    }

    private JellyfinRoutingMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new JellyfinRoutingMiddleware(next, _loggerMock.Object);
    }

    private static DefaultHttpContext CreateHttpContext(string path, Dictionary<string, string>? headers = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = "GET";

        if (headers != null)
        {
            foreach (var header in headers)
            {
                context.Request.Headers.Append(header.Key, header.Value);
            }
        }

        return context;
    }

    [Fact]
    public async Task InvokeAsync_PathAlreadyPrefixed_DoesNotRewrite()
    {
        var originalPath = "/api/jf/System/Info";
        var context = CreateHttpContext(originalPath);
        var nextCalled = false;

        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        context.Request.Path.Value.Should().Be(originalPath);
        nextCalled.Should().BeTrue();
    }

    [Theory]
    [InlineData("/api/v1/something")]
    [InlineData("/rest/ping")]
    [InlineData("/rest/getArtists.view")]
    [InlineData("/song/123/stream")]
    public async Task InvokeAsync_ExcludedPaths_DoesNotRewrite(string path)
    {
        var context = CreateHttpContext(path);
        var nextCalled = false;

        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        context.Request.Path.Value.Should().Be(path);
        nextCalled.Should().BeTrue();
    }

    [Theory]
    [InlineData("/System/Info/Public")]
    [InlineData("/System/Ping")]
    [InlineData("/Users/AuthenticateByName")]
    public async Task InvokeAsync_AllowlistedPreAuthPath_RewritesToJellyfinPrefix(string path)
    {
        var context = CreateHttpContext(path);
        var nextCalled = false;

        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        context.Request.Path.Value.Should().Be($"/api/jf{path}");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_AuthorizationHeaderWithMediaBrowserToken_RewritesPath()
    {
        var headers = new Dictionary<string, string>
        {
            { "Authorization", "MediaBrowser Token=\"abc123\"" }
        };
        var context = CreateHttpContext("/Items", headers);
        var nextCalled = false;

        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        context.Request.Path.Value.Should().Be("/api/jf/Items");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_XEmbyAuthorizationHeader_RewritesPath()
    {
        var headers = new Dictionary<string, string>
        {
            { "X-Emby-Authorization", "MediaBrowser Client=\"TestClient\", Token=\"abc123\"" }
        };
        var context = CreateHttpContext("/UserViews", headers);
        var nextCalled = false;

        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        context.Request.Path.Value.Should().Be("/api/jf/UserViews");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_XMediaBrowserTokenHeader_RewritesPath()
    {
        var headers = new Dictionary<string, string>
        {
            { "X-MediaBrowser-Token", "abc123" }
        };
        var context = CreateHttpContext("/Artists", headers);
        var nextCalled = false;

        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        context.Request.Path.Value.Should().Be("/api/jf/Artists");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_XEmbyTokenHeader_RewritesPath()
    {
        var headers = new Dictionary<string, string>
        {
            { "X-Emby-Token", "abc123" }
        };
        var context = CreateHttpContext("/Audio/123/stream", headers);
        var nextCalled = false;

        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        context.Request.Path.Value.Should().Be("/api/jf/Audio/123/stream");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_NoJellyfinHeaders_DoesNotRewrite()
    {
        var context = CreateHttpContext("/SomeOtherPath");
        var nextCalled = false;

        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        context.Request.Path.Value.Should().Be("/SomeOtherPath");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_NonMediaBrowserAuthorizationHeader_DoesNotRewrite()
    {
        var headers = new Dictionary<string, string>
        {
            { "Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." }
        };
        var context = CreateHttpContext("/SomePath", headers);
        var nextCalled = false;

        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        context.Request.Path.Value.Should().Be("/SomePath");
        nextCalled.Should().BeTrue();
    }
}

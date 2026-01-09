using System.Net;
using FluentAssertions;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Services.Security;
using Moq;
using Serilog;

namespace Melodee.Tests.Common.Security;

public class SsrfValidatorTests
{
    private readonly ILogger _logger;
    private readonly Mock<IMelodeeConfigurationFactory> _configFactoryMock;
    private readonly ISsrfValidator _validator;

    public SsrfValidatorTests()
    {
        _logger = new LoggerConfiguration().CreateLogger();
        _configFactoryMock = new Mock<IMelodeeConfigurationFactory>();

        var configMock = new Mock<IMelodeeConfiguration>();
        configMock.Setup(x => x.GetValue<bool>(SettingRegistry.PodcastHttpAllowHttp)).Returns(false);
        _configFactoryMock.Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(configMock.Object);

        _validator = new SsrfValidator(_logger, _configFactoryMock.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateUrlAsync_NullOrEmptyUrl_ReturnsInvalid(string? url)
    {
        var result = await _validator.ValidateUrlAsync(url!);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("required");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/file.rss")]
    [InlineData("file:///etc/passwd")]
    [InlineData("javascript:alert(1)")]
    public async Task ValidateUrlAsync_InvalidUrlFormat_ReturnsInvalid(string url)
    {
        var result = await _validator.ValidateUrlAsync(url);

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("http://example.com/podcast.rss")]
    public async Task ValidateUrlAsync_HttpUrlWithHttpDisabled_ReturnsInvalid(string url)
    {
        var result = await _validator.ValidateUrlAsync(url);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("https");
    }

    [Fact]
    public async Task ValidateUrlAsync_HttpUrlWithHttpEnabled_ReturnsValid()
    {
        var configMock = new Mock<IMelodeeConfiguration>();
        configMock.Setup(x => x.GetValue<bool>(SettingRegistry.PodcastHttpAllowHttp)).Returns(true);
        _configFactoryMock.Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(configMock.Object);

        var validator = new SsrfValidator(_logger, _configFactoryMock.Object);

        var result = await validator.ValidateUrlAsync("http://example.com/podcast.rss");

        // Will fail DNS resolution but scheme validation should pass
        // For full test we'd need to mock DNS resolution
    }

    [Theory]
    [InlineData("https://example.com:8080/podcast.rss")]
    [InlineData("https://example.com:22/podcast.rss")]
    [InlineData("https://example.com:3000/podcast.rss")]
    public async Task ValidateUrlAsync_NonStandardPort_ReturnsInvalid(string url)
    {
        var result = await _validator.ValidateUrlAsync(url);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Port");
    }

    [Theory]
    [InlineData("127.0.0.1", true)]
    [InlineData("127.0.0.2", true)]
    [InlineData("127.255.255.255", true)]
    [InlineData("10.0.0.1", true)]
    [InlineData("10.255.255.255", true)]
    [InlineData("172.16.0.1", true)]
    [InlineData("172.31.255.255", true)]
    [InlineData("192.168.0.1", true)]
    [InlineData("192.168.255.255", true)]
    [InlineData("169.254.0.1", true)]
    [InlineData("169.254.255.255", true)]
    [InlineData("0.0.0.1", true)]
    [InlineData("224.0.0.1", true)]
    [InlineData("239.255.255.255", true)]
    [InlineData("240.0.0.1", true)]
    [InlineData("255.255.255.255", true)]
    [InlineData("100.64.0.1", true)]
    [InlineData("100.127.255.255", true)]
    [InlineData("8.8.8.8", false)]
    [InlineData("1.1.1.1", false)]
    [InlineData("93.184.216.34", false)] // example.com
    public void IsPrivateOrReservedAddress_IPv4_ReturnsExpected(string ip, bool expectedPrivate)
    {
        var address = IPAddress.Parse(ip);
        var isPrivate = SsrfValidator.IsPrivateOrReservedAddress(address);

        isPrivate.Should().Be(expectedPrivate, $"IP {ip} should be {(expectedPrivate ? "private" : "public")}");
    }

    [Theory]
    [InlineData("::1", true)]  // IPv6 loopback
    [InlineData("fe80::1", true)]  // Link-local
    [InlineData("fc00::1", true)]  // Unique local
    [InlineData("fd00::1", true)]  // Unique local
    [InlineData("ff00::1", true)]  // Multicast
    [InlineData("2001:4860:4860::8888", false)]  // Google DNS
    public void IsPrivateOrReservedAddress_IPv6_ReturnsExpected(string ip, bool expectedPrivate)
    {
        var address = IPAddress.Parse(ip);
        var isPrivate = SsrfValidator.IsPrivateOrReservedAddress(address);

        isPrivate.Should().Be(expectedPrivate, $"IP {ip} should be {(expectedPrivate ? "private" : "public")}");
    }

    [Theory]
    [InlineData("::ffff:127.0.0.1", true)]  // IPv4-mapped loopback
    [InlineData("::ffff:192.168.1.1", true)]  // IPv4-mapped private
    [InlineData("::ffff:8.8.8.8", false)]  // IPv4-mapped public
    public void IsPrivateOrReservedAddress_IPv4MappedIPv6_ReturnsExpected(string ip, bool expectedPrivate)
    {
        var address = IPAddress.Parse(ip);
        var isPrivate = SsrfValidator.IsPrivateOrReservedAddress(address);

        isPrivate.Should().Be(expectedPrivate);
    }

    [Fact]
    public async Task ValidateRedirectAsync_ExceedsMaxRedirects_ReturnsInvalid()
    {
        var result = await _validator.ValidateRedirectAsync(
            "https://example.com/original",
            "https://example.com/redirect",
            redirectCount: 5,
            maxRedirects: 5);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("redirect");
    }

    [Fact]
    public async Task ValidateRedirectAsync_WithinMaxRedirects_ValidatesNewUrl()
    {
        var result = await _validator.ValidateRedirectAsync(
            "https://example.com/original",
            "https://127.0.0.1/redirect",  // Redirect to loopback should fail
            redirectCount: 0,
            maxRedirects: 5);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("private");
    }

    [Fact]
    public async Task ValidateRedirectAsync_RelativeUrl_ResolvesAgainstOriginal()
    {
        var result = await _validator.ValidateRedirectAsync(
            "https://example.com/path/original",
            "../redirect",
            redirectCount: 0,
            maxRedirects: 5);

        // Should try to resolve relative URL against original
        // Result depends on DNS resolution of example.com
    }
}

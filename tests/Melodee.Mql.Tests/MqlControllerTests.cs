using FluentAssertions;
using FluentAssertions.Extensions;
using Melodee.Mql.Api;
using Melodee.Mql.Api.Dto;
using Melodee.Mql.Interfaces;
using Melodee.Mql.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Melodee.Mql.Tests;

public class MqlControllerTests
{
    private readonly Mock<IMqlTokenizer> _tokenizerMock;
    private readonly Mock<IMqlParser> _parserMock;
    private readonly Mock<IMqlValidator> _validatorMock;
    private readonly Mock<ILogger<MqlController>> _loggerMock;
    private readonly MqlController _controller;

    public MqlControllerTests()
    {
        _tokenizerMock = new Mock<IMqlTokenizer>();
        _parserMock = new Mock<IMqlParser>();
        _validatorMock = new Mock<IMqlValidator>();
        _loggerMock = new Mock<ILogger<MqlController>>();

        _controller = new MqlController(
            _tokenizerMock.Object,
            _parserMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);

        SetupControllerContext();
    }

    private void SetupControllerContext()
    {
        var httpContext = new DefaultHttpContext
        {
            Connection =
            {
                RemoteIpAddress = System.Net.IPAddress.Loopback
            }
        };
        httpContext.Request.Headers["X-Forwarded-For"] = "127.0.0.1";

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task ParseAsync_EmptyQuery_ReturnsBadRequest()
    {
        var request = new MqlParseRequest { Entity = "songs", Query = "" };

        var result = await _controller.ParseAsync(request, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var errorResponse = ((BadRequestObjectResult)result.Result!).Value.Should().BeOfType<MqlErrorResponse>().Subject;
        errorResponse.ErrorCode.Should().Be("MQL_EMPTY_QUERY");
    }

    [Fact]
    public async Task ParseAsync_NullQuery_ReturnsBadRequest()
    {
        var request = new MqlParseRequest { Entity = "songs", Query = null! };

        var result = await _controller.ParseAsync(request, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var errorResponse = ((BadRequestObjectResult)result.Result!).Value.Should().BeOfType<MqlErrorResponse>().Subject;
        errorResponse.ErrorCode.Should().Be("MQL_EMPTY_QUERY");
    }

    [Fact]
    public async Task ParseAsync_QueryTooLong_ReturnsBadRequest()
    {
        var request = new MqlParseRequest
        {
            Entity = "songs",
            Query = new string('a', 501)
        };

        var result = await _controller.ParseAsync(request, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var errorResponse = ((BadRequestObjectResult)result.Result!).Value.Should().BeOfType<MqlErrorResponse>().Subject;
        errorResponse.ErrorCode.Should().Be("MQL_QUERY_TOO_LONG");
    }

    [Fact]
    public async Task ParseAsync_ValidationFails_ReturnsBadRequest()
    {
        _validatorMock
            .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new MqlValidationResult(false, new List<MqlError>
            {
                new("MQL_UNKNOWN_FIELD", "Unknown field 'invalidfield'", null)
            }, new List<string>(), 0));

        var request = new MqlParseRequest { Entity = "songs", Query = "invalidfield:test" };

        var result = await _controller.ParseAsync(request, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var errorResponse = ((BadRequestObjectResult)result.Result!).Value.Should().BeOfType<MqlErrorResponse>().Subject;
        errorResponse.ErrorCode.Should().Be("MQL_UNKNOWN_FIELD");
    }

    [Fact]
    public async Task ParseAsync_ParseFails_ReturnsBadRequest()
    {
        _validatorMock
            .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new MqlValidationResult(true, new List<MqlError>(), new List<string>(), 5));

        _tokenizerMock
            .Setup(t => t.Tokenize(It.IsAny<string>()))
            .Returns(new List<MqlToken>());

        _parserMock
            .Setup(p => p.Parse(It.IsAny<IEnumerable<MqlToken>>(), It.IsAny<string>()))
            .Returns(MqlParseResult.Failed(new List<MqlError>
            {
                new("MQL_PARSE_ERROR", "Parse error at position 5", null)
            }, "artist:test"));

        var request = new MqlParseRequest { Entity = "songs", Query = "artist:test" };

        var result = await _controller.ParseAsync(request, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var errorResponse = ((BadRequestObjectResult)result.Result!).Value.Should().BeOfType<MqlErrorResponse>().Subject;
        errorResponse.ErrorCode.Should().Be("MQL_PARSE_ERROR");
    }

    [Fact]
    public async Task ParseAsync_Success_ReturnsOk()
    {
        _validatorMock
            .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new MqlValidationResult(true, new List<MqlError>(), new List<string>(), 3));

        var fieldToken = new MqlToken(MqlTokenType.FieldName, "artist", 0, 6, 1, 1);
        var colonToken = new MqlToken(MqlTokenType.Colon, ":", 6, 7, 1, 7);
        var stringToken = new MqlToken(MqlTokenType.StringLiteral, "test", 7, 11, 1, 8);

        _tokenizerMock
            .Setup(t => t.Tokenize(It.IsAny<string>()))
            .Returns(new List<MqlToken> { fieldToken, colonToken, stringToken });

        _parserMock
            .Setup(p => p.Parse(It.IsAny<IEnumerable<MqlToken>>(), It.IsAny<string>()))
            .Returns(new MqlParseResult(true, new FieldExpressionNode("artist", ":", "test", fieldToken), "artist:\"test\"", new List<MqlError>(), new List<string>()));

        var request = new MqlParseRequest { Entity = "songs", Query = "artist:test" };

        var result = await _controller.ParseAsync(request, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
        var response = ((OkObjectResult)result.Result!).Value.Should().BeOfType<MqlParseResponse>().Subject;
        response.Valid.Should().BeTrue();
        response.NormalizedQuery.Should().Be("artist:\"test\"");
        response.EstimatedComplexity.Should().Be(3);
        response.Ast.Should().NotBeNull();
    }

    [Fact]
    public async Task ParseAsync_ReturnsProcessingTime()
    {
        _validatorMock
            .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new MqlValidationResult(true, new List<MqlError>(), new List<string>(), 1));

        _tokenizerMock
            .Setup(t => t.Tokenize(It.IsAny<string>()))
            .Returns(new List<MqlToken>());

        _parserMock
            .Setup(p => p.Parse(It.IsAny<IEnumerable<MqlToken>>(), It.IsAny<string>()))
            .Returns(new MqlParseResult(true, null, "", new List<MqlError>(), new List<string>()));

        var request = new MqlParseRequest { Entity = "songs", Query = "artist:test" };

        var result = await _controller.ParseAsync(request, CancellationToken.None);

        var response = ((OkObjectResult)result.Result!).Value.Should().BeOfType<MqlParseResponse>().Subject;
        response.ProcessingTimeMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ParseAsync_WithWarnings_ReturnsWarnings()
    {
        _validatorMock
            .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new MqlValidationResult(true, new List<MqlError>(), new List<string> { "Warning: complex query" }, 5));

        _tokenizerMock
            .Setup(t => t.Tokenize(It.IsAny<string>()))
            .Returns(new List<MqlToken>());

        _parserMock
            .Setup(p => p.Parse(It.IsAny<IEnumerable<MqlToken>>(), It.IsAny<string>()))
            .Returns(new MqlParseResult(true, null, "", new List<MqlError>(), new List<string> { "Warning: complex query" }));

        var request = new MqlParseRequest { Entity = "songs", Query = "artist:test" };

        var result = await _controller.ParseAsync(request, CancellationToken.None);

        var response = ((OkObjectResult)result.Result!).Value.Should().BeOfType<MqlParseResponse>().Subject;
        response.Warnings.Should().Contain("Warning: complex query");
    }

    [Fact]
    public void HealthCheck_ReturnsHealthy()
    {
        var result = _controller.HealthCheck();

        result.Result.Should().BeOfType<OkObjectResult>();
        var response = ((OkObjectResult)result.Result!).Value.Should().BeOfType<Dictionary<string, string>>().Subject;
        response["status"].Should().Be("healthy");
        response.Should().ContainKey("timestamp");
    }

    [Fact]
    public async Task ParseAsync_ErrorResponse_HasTimestamp()
    {
        _validatorMock
            .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new MqlValidationResult(false, new List<MqlError>
            {
                new("TEST_ERROR", "Test error", null)
            }, new List<string>(), 0));

        var request = new MqlParseRequest { Entity = "songs", Query = "test" };

        var beforeTime = DateTime.UtcNow.AddSeconds(-1);
        var result = await _controller.ParseAsync(request, CancellationToken.None);
        var afterTime = DateTime.UtcNow.AddSeconds(1);

        var errorResponse = ((BadRequestObjectResult)result.Result!).Value.Should().BeOfType<MqlErrorResponse>().Subject;
        errorResponse.Timestamp.Should().BeAfter(beforeTime);
        errorResponse.Timestamp.Should().BeBefore(afterTime);
    }
}

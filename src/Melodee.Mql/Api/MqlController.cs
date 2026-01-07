using System.Diagnostics;
using Melodee.Mql.Api.Dto;
using Melodee.Mql.Interfaces;
using Melodee.Mql.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Melodee.Mql.Api;

/// <summary>
/// API controller for MQL parsing and validation.
/// </summary>
[ApiController]
[Route("api/v1/query")]
public class MqlController : ControllerBase
{
    private readonly IMqlTokenizer _tokenizer;
    private readonly IMqlParser _parser;
    private readonly IMqlValidator _validator;
    private readonly ILogger<MqlController> _logger;

    private const int MaxQueryLength = 500;
    private const int ParseTimeoutMs = 200;
    private const int RateLimitRequests = 10;
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);

    private static readonly Dictionary<string, (int Count, DateTime WindowStart)> RequestCounts = new();
    private static readonly SemaphoreSlim RateLimitSemaphore = new(1, 1);

    public MqlController(
        IMqlTokenizer tokenizer,
        IMqlParser parser,
        IMqlValidator validator,
        ILogger<MqlController> logger)
    {
        _tokenizer = tokenizer;
        _parser = parser;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Parses and validates an MQL query.
    /// </summary>
    /// <param name="request">The parse request containing entity type and query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parse result with AST, normalized query, and any errors.</returns>
    [HttpPost("parse")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<MqlParseResponse>> ParseAsync(
        [FromBody] MqlParseRequest request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new MqlErrorResponse
                {
                    ErrorCode = "MQL_EMPTY_QUERY",
                    Message = "Query cannot be empty",
                    Timestamp = DateTime.UtcNow
                });
            }

            if (request.Query.Length > MaxQueryLength)
            {
                return BadRequest(new MqlErrorResponse
                {
                    ErrorCode = "MQL_QUERY_TOO_LONG",
                    Message = $"Query exceeds maximum length of {MaxQueryLength} characters",
                    Timestamp = DateTime.UtcNow
                });
            }

            if (!await CheckRateLimitAsync(HttpContext.GetIpAddress() ?? "unknown"))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, new MqlErrorResponse
                {
                    ErrorCode = "RATE_LIMIT_EXCEEDED",
                    Message = $"Rate limit exceeded. Maximum {RateLimitRequests} requests per {RateLimitWindow.TotalMinutes} minute(s).",
                    Timestamp = DateTime.UtcNow
                });
            }

            var validationResult = _validator.Validate(request.Query, request.Entity);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Query validation failed for {Query}: {Errors}",
                    request.Query, string.Join(", ", validationResult.Errors.Select(e => e.Message)));

                var firstError = validationResult.Errors.First();
                return BadRequest(new MqlErrorResponse
                {
                    ErrorCode = firstError.ErrorCode,
                    Message = firstError.Message,
                    Position = firstError.Position != null
                        ? new MqlPositionDto
                        {
                            Position = firstError.Position.Start,
                            Line = firstError.Position.Line,
                            Column = firstError.Position.Column,
                            Length = firstError.Position.End - firstError.Position.Start
                        }
                        : null,
                    Suggestions = firstError.Suggestions?
                        .Select(s => new MqlSuggestionDto
                        {
                            Text = s,
                            Type = "field",
                            Confidence = 0.8
                        })
                        .ToList() ?? new List<MqlSuggestionDto>(),
                    Timestamp = DateTime.UtcNow
                });
            }

            var tokens = _tokenizer.Tokenize(request.Query).ToList();

            using var timeoutCts = new CancellationTokenSource(ParseTimeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            MqlParseResult parseResult;
            try
            {
                parseResult = await Task.Run(() =>
                {
                    return _parser.Parse(tokens, request.Entity);
                }, linkedCts.Token);
            }
            catch (Exception)
            {
                if (timeoutCts.IsCancellationRequested)
                {
                    parseResult = MqlParseResult.Failed(new List<MqlError>
                    {
                        new("MQL_PARSE_TIMEOUT", "Query parsing timed out", null)
                    }, request.Query);
                }
                else
                {
                    throw;
                }
            }

            stopwatch.Stop();

            if (!parseResult.IsValid)
            {
                _logger.LogWarning("Query parsing failed for {Query}: {Errors}",
                    request.Query, string.Join(", ", parseResult.Errors.Select(e => e.Message)));

                var firstError = parseResult.Errors.First();
                return BadRequest(new MqlErrorResponse
                {
                    ErrorCode = firstError.ErrorCode,
                    Message = firstError.Message,
                    Position = firstError.Position != null
                        ? new MqlPositionDto
                        {
                            Position = firstError.Position.Start,
                            Line = firstError.Position.Line,
                            Column = firstError.Position.Column,
                            Length = firstError.Position.End - firstError.Position.Start
                        }
                        : null,
                    Suggestions = firstError.Suggestions?
                        .Select(s => new MqlSuggestionDto
                        {
                            Text = s,
                            Type = "suggestion",
                            Confidence = 0.7
                        })
                        .ToList() ?? new List<MqlSuggestionDto>(),
                    Timestamp = DateTime.UtcNow
                });
            }

            _logger.LogInformation("Query parsed successfully in {TimeMs}ms: {Query}",
                stopwatch.ElapsedMilliseconds, request.Query);

            return Ok(new MqlParseResponse
            {
                NormalizedQuery = parseResult.NormalizedQuery ?? request.Query,
                Ast = parseResult.Ast?.ToDto(),
                Warnings = parseResult.Warnings,
                EstimatedComplexity = validationResult.ComplexityScore,
                Valid = true,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            });
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            return StatusCode(StatusCodes.Status499ClientClosedRequest, new MqlErrorResponse
            {
                ErrorCode = "MQL_REQUEST_CANCELLED",
                Message = "Request was cancelled by client",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error parsing query: {Query}", request.Query);

            return StatusCode(StatusCodes.Status500InternalServerError, new MqlErrorResponse
            {
                ErrorCode = "MQL_INTERNAL_ERROR",
                Message = "An unexpected error occurred while processing the query",
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Health check endpoint for the MQL API.
    /// </summary>
    [HttpGet("health")]
    [Produces("application/json")]
    public ActionResult<Dictionary<string, string>> HealthCheck()
    {
        return Ok(new Dictionary<string, string>
        {
            ["status"] = "healthy",
            ["timestamp"] = DateTime.UtcNow.ToString("O")
        });
    }

    private static async Task<bool> CheckRateLimitAsync(string identifier)
    {
        await RateLimitSemaphore.WaitAsync();
        try
        {
            var now = DateTime.UtcNow;

            if (RequestCounts.TryGetValue(identifier, out var record))
            {
                if (now - record.WindowStart > RateLimitWindow)
                {
                    RequestCounts[identifier] = (1, now);
                    return true;
                }

                if (record.Count >= RateLimitRequests)
                {
                    return false;
                }

                RequestCounts[identifier] = (record.Count + 1, record.WindowStart);
                return true;
            }

            RequestCounts[identifier] = (1, now);
            return true;
        }
        finally
        {
            RateLimitSemaphore.Release();
        }
    }
}

internal static class HttpContextExtensions
{
    public static string? GetIpAddress(this HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    }
}

using System.Text.RegularExpressions;
using Melodee.Common.Models;

namespace Melodee.Common.Utility;

/// <summary>
///     Provides validation and normalization methods for search engine queries.
/// </summary>
public static class SearchEngineQueryNormalization
{
    private const int MaximumQueryLength = 256;
    private const int MaximumAmgIdLength = 20;

    /// <summary>
    ///     Normalizes a search query by trimming whitespace and capping length.
    /// </summary>
    /// <param name="input">The input query to normalize</param>
    /// <param name="collapseWhitespace">Whether to collapse multiple whitespace characters into single space</param>
    /// <returns>A normalized query string, or null if input is null/empty</returns>
    public static string? NormalizeQuery(string? input, bool collapseWhitespace = true)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var result = input.Trim();

        if (collapseWhitespace)
        {
            result = Regex.Replace(result, @"\s+", " ");
        }

        if (result.Length > MaximumQueryLength)
        {
            result = result[..MaximumQueryLength];
        }

        return result;
    }

    /// <summary>
    ///     Validates that an AMG ID contains only digits.
    /// </summary>
    /// <param name="amgId">The AMG ID to validate</param>
    /// <returns>True if valid digits-only string, false otherwise</returns>
    public static bool ValidateAmgId(string? amgId)
    {
        if (string.IsNullOrWhiteSpace(amgId))
        {
            return false;
        }

        if (amgId.Length > MaximumAmgIdLength)
        {
            return false;
        }

        return amgId.All(char.IsDigit);
    }

    /// <summary>
    ///     Validates and normalizes a query, returning an error message if invalid.
    /// </summary>
    /// <param name="input">The input query</param>
    /// <param name="parameterName">Name of the parameter for error messages</param>
    /// <returns>A normalized string, or an error result</returns>
    public static OperationResult<string> ValidateQuery(string? input, string parameterName = "query")
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new OperationResult<string>
            {
                Data = string.Empty,
                Type = OperationResponseType.Error,
                Errors = [new ArgumentException($"[{parameterName}] cannot be empty or whitespace.")]
            };
        }

        var normalized = NormalizeQuery(input);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return new OperationResult<string>
            {
                Data = string.Empty,
                Type = OperationResponseType.Error,
                Errors = [new ArgumentException($"[{parameterName}] is invalid after normalization.")]
            };
        }

        return new OperationResult<string>
        {
            Data = normalized
        };
    }

    /// <summary>
    ///     Validates an AMG ID and returns an error message if invalid.
    /// </summary>
    /// <param name="amgId">The AMG ID to validate</param>
    /// <returns>True if valid, or an error result</returns>
    public static OperationResult<bool> ValidateAmgIdResult(string? amgId)
    {
        if (string.IsNullOrWhiteSpace(amgId))
        {
            return new OperationResult<bool>
            {
                Data = false,
                Type = OperationResponseType.Error,
                Errors = [new ArgumentException("AMG ID cannot be empty or whitespace.")]
            };
        }

        if (!ValidateAmgId(amgId))
        {
            return new OperationResult<bool>
            {
                Data = false,
                Type = OperationResponseType.Error,
                Errors = [new ArgumentException("AMG ID must contain only digits.")]
            };
        }

        return new OperationResult<bool>
        {
            Data = true
        };
    }
}

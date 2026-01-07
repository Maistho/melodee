namespace Melodee.Mql.Constants;

/// <summary>
/// Error codes used in MQL processing.
/// </summary>
public static class MqlErrorCodes
{
    public const string MqlParseError = "MQL_PARSE_ERROR";
    public const string MqlUnknownField = "MQL_UNKNOWN_FIELD";
    public const string MqlInvalidLiteral = "MQL_INVALID_LITERAL";
    public const string MqlRegexTooComplex = "MQL_REGEX_TOO_COMPLEX";
    public const string MqlRegexDangerous = "MQL_REGEX_DANGEROUS";
    public const string MqlQueryTooLong = "MQL_QUERY_TOO_LONG";
    public const string MqlTooManyFields = "MQL_TOO_MANY_FIELDS";
    public const string MqlTooDeep = "MQL_TOO_DEEP";
    public const string MqlUnbalancedParens = "MQL_UNBALANCED_PARENS";
    public const string MqlForbiddenField = "MQL_FORBIDDEN_FIELD";
    public const string MqlEmptyQuery = "MQL_EMPTY_QUERY";
    public const string MqlForbiddenUserData = "MQL_FORBIDDEN_USER_DATA";
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
}

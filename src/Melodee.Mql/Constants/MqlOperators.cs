namespace Melodee.Mql.Constants;

/// <summary>
/// Supported operators in MQL expressions.
/// </summary>
public static class MqlOperators
{
    public static readonly string[] ComparisonOperators =
    {
        ":=",
        ":!=",
        ":<",
        ":<=",
        ":>",
        ":>="
    };

    public static readonly string[] StringOperators =
    {
        "contains",
        "startsWith",
        "endsWith",
        "wildcard",
        "matches"
    };

    public const string And = "AND";
    public const string Or = "OR";
    public const string Not = "NOT";
}

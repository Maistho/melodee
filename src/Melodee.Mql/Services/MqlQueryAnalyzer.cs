using Melodee.Mql.Interfaces;
using Melodee.Mql.Models;

namespace Melodee.Mql.Services;

/// <summary>
/// Utility for analyzing query execution and identifying performance issues.
/// </summary>
public sealed class MqlQueryAnalyzer
{
    private readonly IMqlParser _parser;
    private readonly IMqlValidator _validator;
    private readonly IMqlTokenizer _tokenizer;

    public MqlQueryAnalyzer(IMqlParser parser, IMqlValidator validator, IMqlTokenizer tokenizer)
    {
        _parser = parser;
        _validator = validator;
        _tokenizer = tokenizer;
    }

    /// <summary>
    /// Analyzes a query and returns performance recommendations.
    /// </summary>
    public QueryAnalysisResult Analyze(string query, string entityType)
    {
        var validationResult = _validator.Validate(query, entityType);
        var isValid = validationResult.IsValid;
        var complexityScore = validationResult.ComplexityScore;

        var tokens = _tokenizer.Tokenize(query).ToList();
        var parseResult = _parser.Parse(tokens, entityType);

        var astDepth = parseResult.Ast != null ? CalculateAstDepth(parseResult.Ast) : 0;
        var fieldCount = parseResult.Ast != null ? CountFieldExpressions(parseResult.Ast) : 0;
        var hasRegex = ContainsRegexPatterns(query);
        var hasNestedParentheses = CountParenthesesDepth(query) > 1;

        var recommendations = new List<string>();

        if (!isValid)
        {
            recommendations.Add("Fix validation errors before optimization");
        }
        else if (!parseResult.IsValid)
        {
            recommendations.Add("Fix parsing errors before optimization");
        }
        else
        {
            if (complexityScore > 20)
            {
                recommendations.Add("Query complexity is high. Consider breaking it into simpler queries.");
            }

            if (astDepth > 10)
            {
                recommendations.Add($"Query nesting depth ({astDepth}) is high. Consider simplifying the structure.");
            }

            if (fieldCount > 10)
            {
                recommendations.Add($"Query has {fieldCount} field filters. Consider using broader filters.");
            }

            if (hasRegex)
            {
                recommendations.Add("Query contains regex patterns which may be slow. Consider using simpler patterns.");
            }

            if (hasNestedParentheses)
            {
                recommendations.Add("Query has deep parentheses nesting. Consider restructuring.");
            }

            recommendations.Add("Ensure database indexes exist on queried fields.");
            recommendations.Add("Consider caching frequently used queries.");
        }

        var severity = DetermineSeverity(complexityScore, astDepth, hasRegex, hasNestedParentheses);

        return new QueryAnalysisResult
        {
            Query = query,
            EntityType = entityType,
            Timestamp = DateTime.UtcNow,
            IsValid = isValid && parseResult.IsValid,
            ComplexityScore = complexityScore,
            AstDepth = astDepth,
            FieldCount = fieldCount,
            HasRegex = hasRegex,
            HasNestedParentheses = hasNestedParentheses,
            Severity = severity,
            Recommendations = recommendations
        };
    }

    private static SeverityLevel DetermineSeverity(int complexityScore, int astDepth, bool hasRegex, bool hasNestedParentheses)
    {
        if (complexityScore > 30 || astDepth > 15)
        {
            return SeverityLevel.Critical;
        }

        if (complexityScore > 20 || astDepth > 10 || hasRegex || hasNestedParentheses)
        {
            return SeverityLevel.Warning;
        }

        return SeverityLevel.Ok;
    }

    private int CalculateAstDepth(MqlAstNode? node, int currentDepth = 0)
    {
        if (node == null)
        {
            return currentDepth;
        }

        return node switch
        {
            BinaryExpressionNode binary => Math.Max(
                CalculateAstDepth(binary.Left, currentDepth + 1),
                CalculateAstDepth(binary.Right, currentDepth + 1)),
            UnaryExpressionNode unary => CalculateAstDepth(unary.Operand, currentDepth + 1),
            GroupNode group => CalculateAstDepth(group.Inner, currentDepth + 1),
            _ => currentDepth
        };
    }

    private int CountFieldExpressions(MqlAstNode? node)
    {
        if (node == null)
        {
            return 0;
        }

        return node switch
        {
            FieldExpressionNode => 1,
            BinaryExpressionNode binary => CountFieldExpressions(binary.Left) + CountFieldExpressions(binary.Right),
            UnaryExpressionNode unary => CountFieldExpressions(unary.Operand),
            GroupNode group => CountFieldExpressions(group.Inner),
            _ => 0
        };
    }

    private bool ContainsRegexPatterns(string query)
    {
        return query.Contains("/pattern");
    }

    private int CountParenthesesDepth(string query)
    {
        var depth = 0;
        var maxDepth = 0;

        foreach (var c in query)
        {
            if (c == '(')
            {
                depth++;
                maxDepth = Math.Max(maxDepth, depth);
            }
            else if (c == ')')
            {
                depth--;
            }
        }

        return maxDepth;
    }
}

public sealed record QueryAnalysisResult
{
    public string Query { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public bool IsValid { get; init; }
    public int ComplexityScore { get; init; }
    public int AstDepth { get; init; }
    public int FieldCount { get; init; }
    public bool HasRegex { get; init; }
    public bool HasNestedParentheses { get; init; }
    public SeverityLevel Severity { get; init; }
    public List<string> Recommendations { get; init; } = new();
}

public static class QueryAnalyzerFactory
{
    private static MqlQueryAnalyzer? _analyzer;

    public static MqlQueryAnalyzer GetAnalyzer()
    {
        return _analyzer ??= new MqlQueryAnalyzer(new MqlParser(), new MqlValidator(), new MqlTokenizer());
    }
}

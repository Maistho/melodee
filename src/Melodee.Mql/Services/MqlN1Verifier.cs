using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using Melodee.Mql.Models;

namespace Melodee.Mql.Services;

/// <summary>
/// Utility for detecting potential N+1 query issues in MQL compilation.
/// </summary>
public sealed class MqlN1Verifier
{
    private readonly ConcurrentDictionary<string, long> _propertyAccessCounts = new();
    private readonly ConcurrentBag<string> _potentialIssues = new();

    /// <summary>
    /// Analyzes a compiled expression for potential N+1 query patterns.
    /// </summary>
    public N1VerificationResult VerifyCompiledExpression<T>(Expression<Func<T, bool>> expression)
    {
        _propertyAccessCounts.Clear();
        _potentialIssues.Clear();

        AnalyzeExpression(expression.Body);

        var potentialIssues = _potentialIssues.ToList();
        var severity = potentialIssues.Any() ? SeverityLevel.Warning : SeverityLevel.Ok;
        var recommendation = severity == SeverityLevel.Warning
            ? "Consider using Include() or eager loading for nested navigations"
            : "No obvious N+1 patterns detected";

        return new N1VerificationResult
        {
            Timestamp = DateTime.UtcNow,
            Severity = severity,
            PropertyAccessCounts = _propertyAccessCounts.ToDictionary(),
            PotentialIssues = potentialIssues,
            Recommendation = recommendation
        };
    }

    private void AnalyzeExpression(Expression expression)
    {
        switch (expression)
        {
            case MemberExpression member:
                TrackPropertyAccess(member.Expression?.Type?.Name + "." + member.Member.Name);
                break;

            case MethodCallExpression methodCall:
                foreach (var arg in methodCall.Arguments)
                {
                    AnalyzeExpression(arg);
                }
                break;

            case BinaryExpression binary:
                AnalyzeExpression(binary.Left);
                AnalyzeExpression(binary.Right);
                break;

            case UnaryExpression unary:
                AnalyzeExpression(unary.Operand);
                break;

            case LambdaExpression lambda:
                AnalyzeExpression(lambda.Body);
                break;

            case ConditionalExpression conditional:
                AnalyzeExpression(conditional.Test);
                AnalyzeExpression(conditional.IfTrue);
                AnalyzeExpression(conditional.IfFalse);
                break;
        }
    }

    private void TrackPropertyAccess(string propertyPath)
    {
        if (string.IsNullOrEmpty(propertyPath))
        {
            return;
        }

        _propertyAccessCounts.AddOrIncrement(propertyPath);

        if (propertyPath.Contains("."))
        {
            _potentialIssues.Add($"Nested navigation property access: {propertyPath}");
        }
    }
}

public sealed record N1VerificationResult
{
    public DateTime Timestamp { get; init; }
    public SeverityLevel Severity { get; init; }
    public Dictionary<string, long> PropertyAccessCounts { get; init; } = new();
    public List<string> PotentialIssues { get; init; } = new();
    public string Recommendation { get; init; } = string.Empty;
}

public enum SeverityLevel
{
    Ok,
    Warning,
    Critical
}

public static class N1VerifierFactory
{
    private static readonly MqlN1Verifier Verifier = new();

    public static MqlN1Verifier GetVerifier() => Verifier;
}

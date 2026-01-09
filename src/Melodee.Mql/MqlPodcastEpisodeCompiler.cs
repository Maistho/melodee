using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Melodee.Common.Extensions;
using Melodee.Mql.Interfaces;
using Melodee.Mql.Models;

namespace Melodee.Mql;

/// <summary>
/// Compiles MQL AST into EF Core Expression predicates for PodcastEpisode entities.
/// </summary>
public sealed class MqlPodcastEpisodeCompiler : IMqlCompiler<Melodee.Common.Data.Models.PodcastEpisode>
{
    private readonly IMqlFieldInfoProvider _fieldInfoProvider;
    private readonly MqlOptions _options;

    public MqlPodcastEpisodeCompiler(IMqlFieldInfoProvider? fieldInfoProvider = null, MqlOptions? options = null)
    {
        _fieldInfoProvider = fieldInfoProvider ?? new MqlFieldInfoProvider();
        _options = options ?? new MqlOptions();
    }

    public Expression<Func<Melodee.Common.Data.Models.PodcastEpisode, bool>> Compile(
        MqlAstNode ast,
        int? userId = null)
    {
        var parameter = Expression.Parameter(typeof(Melodee.Common.Data.Models.PodcastEpisode), "pe");
        var body = CompileNode(ast, parameter, userId);
        return Expression.Lambda<Func<Melodee.Common.Data.Models.PodcastEpisode, bool>>(body, parameter);
    }

    private Expression CompileNode(MqlAstNode node, ParameterExpression parameter, int? userId)
    {
        return node switch
        {
            FreeTextNode freeText => CompileFreeText(freeText, parameter),
            FieldExpressionNode field => CompileFieldExpression(field, parameter, userId),
            RegexExpressionNode regex => CompileRegexExpression(regex, parameter, userId),
            BinaryExpressionNode binary => CompileBinaryExpression(binary, parameter, userId),
            UnaryExpressionNode unary => CompileUnaryExpression(unary, parameter, userId),
            GroupNode group => CompileNode(group.Inner, parameter, userId),
            RangeNode range => CompileRangeExpression(range, parameter, userId),
            _ => Expression.Constant(true)
        };
    }

    private Expression CompileFreeText(FreeTextNode node, ParameterExpression parameter)
    {
        var searchTerm = node.Text.ToNormalizedString();
        if (string.IsNullOrEmpty(searchTerm))
        {
            return Expression.Constant(true);
        }

        var titleExpr = Expression.Property(parameter, nameof(Melodee.Common.Data.Models.PodcastEpisode.Title));
        var channelExpr = GetNestedPropertyExpression(parameter, ["PodcastChannel", "Title"]);
        var descriptionExpr = Expression.Property(parameter, nameof(Melodee.Common.Data.Models.PodcastEpisode.Description));

        var searchMethod = typeof(string).GetMethod("Contains", [typeof(string)])!;
        var titleContains = Expression.Call(titleExpr, searchMethod, Expression.Constant(searchTerm));
        var channelContains = Expression.Call(channelExpr, searchMethod, Expression.Constant(searchTerm));

        // Description can be null
        var descriptionNotNull = Expression.NotEqual(descriptionExpr, Expression.Constant(null, typeof(string)));
        var descriptionContains = Expression.Call(descriptionExpr, searchMethod, Expression.Constant(searchTerm));
        var safeDescriptionContains = Expression.AndAlso(descriptionNotNull, descriptionContains);

        return Expression.OrElse(Expression.OrElse(titleContains, channelContains), safeDescriptionContains);
    }

    private Expression CompileFieldExpression(FieldExpressionNode node, ParameterExpression parameter, int? userId)
    {
        var fieldInfo = MqlFieldRegistry.GetField(node.Field, "podcasts");
        if (fieldInfo is null)
        {
            return Expression.Constant(true);
        }

        var effectiveOperator = node.Operator.ToLowerInvariant();
        if (effectiveOperator == "equals" && fieldInfo.DefaultOperator != "equals")
        {
            effectiveOperator = fieldInfo.DefaultOperator;
        }

        return effectiveOperator switch
        {
            "equals" or "exactequals" => CompileEquals(fieldInfo, node.Value, parameter, userId),
            "notequals" => Expression.Not(CompileEquals(fieldInfo, node.Value, parameter, userId)),
            "lessthan" => CompileLessThan(fieldInfo, node.Value, parameter, userId),
            "lessthanorequals" => CompileLessThanOrEquals(fieldInfo, node.Value, parameter, userId),
            "greaterthan" => CompileGreaterThan(fieldInfo, node.Value, parameter, userId),
            "greaterthanorequals" => CompileGreaterThanOrEquals(fieldInfo, node.Value, parameter, userId),
            "contains" => CompileContains(fieldInfo, node.Value, parameter, userId),
            "startswith" => CompileStartsWith(fieldInfo, node.Value, parameter, userId),
            "endswith" => CompileEndsWith(fieldInfo, node.Value, parameter, userId),
            "wildcard" => CompileWildcard(fieldInfo, node.Value, parameter, userId),
            _ => CompileEquals(fieldInfo, node.Value, parameter, userId)
        };
    }

    private Expression CompileEquals(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter, int? userId)
    {
        var propertyPath = fieldInfo.DbMapping.Split('.');
        var actualPath = propertyPath.Skip(1).ToArray();
        Expression propertyExpr = actualPath.Length switch
        {
            1 => Expression.Property(parameter, actualPath[0]),
            2 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1]]),
            3 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1], actualPath[2]]),
            _ => GetNestedPropertyExpression(parameter, actualPath)
        };

        var convertedValue = ConvertValueForComparison(value, fieldInfo.Type, propertyExpr.Type, fieldInfo.ValueMultiplier);
        return Expression.Equal(propertyExpr, Expression.Constant(convertedValue, propertyExpr.Type));
    }

    private Expression CompileLessThan(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter, int? userId)
    {
        return CompileComparison(fieldInfo, value, parameter, Expression.LessThan);
    }

    private Expression CompileLessThanOrEquals(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter, int? userId)
    {
        return CompileComparison(fieldInfo, value, parameter, Expression.LessThanOrEqual);
    }

    private Expression CompileGreaterThan(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter, int? userId)
    {
        return CompileComparison(fieldInfo, value, parameter, Expression.GreaterThan);
    }

    private Expression CompileGreaterThanOrEquals(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter, int? userId)
    {
        return CompileComparison(fieldInfo, value, parameter, Expression.GreaterThanOrEqual);
    }

    private Expression CompileComparison(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter,
        Func<Expression, Expression, Expression> comparisonFactory)
    {
        var propertyPath = fieldInfo.DbMapping.Split('.');
        var actualPath = propertyPath.Skip(1).ToArray();
        Expression propertyExpr = actualPath.Length switch
        {
            1 => Expression.Property(parameter, actualPath[0]),
            2 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1]]),
            3 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1], actualPath[2]]),
            _ => GetNestedPropertyExpression(parameter, actualPath)
        };

        var convertedValue = ConvertValueForComparison(value, fieldInfo.Type, propertyExpr.Type, fieldInfo.ValueMultiplier);

        if (propertyExpr.Type.IsGenericType && propertyExpr.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = propertyExpr.Type.GetGenericArguments()[0];
            var nonNullableProperty = Expression.Convert(propertyExpr, underlyingType);
            var convertedConstant = ConvertToType(convertedValue, underlyingType);
            var rightExpr = Expression.Constant(convertedConstant, underlyingType);
            var comparison = comparisonFactory(nonNullableProperty, rightExpr);
            var hasValueCheck = Expression.NotEqual(propertyExpr, Expression.Constant(null, propertyExpr.Type));
            return Expression.AndAlso(hasValueCheck, comparison);
        }

        return comparisonFactory(propertyExpr, Expression.Constant(convertedValue, propertyExpr.Type));
    }

    private static object ConvertToType(object value, Type targetType)
    {
        if (targetType == typeof(TimeSpan))
        {
            var milliseconds = Convert.ToDouble(value);
            return TimeSpan.FromMilliseconds(milliseconds);
        }
        return Convert.ChangeType(value, targetType);
    }

    private Expression CompileContains(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter, int? userId)
    {
        var stringValue = value.ToString()?.ToNormalizedString() ?? string.Empty;
        var propertyPath = fieldInfo.DbMapping.Split('.');
        var actualPath = propertyPath.Skip(1).ToArray();
        Expression propertyExpr = actualPath.Length switch
        {
            1 => Expression.Property(parameter, actualPath[0]),
            2 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1]]),
            3 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1], actualPath[2]]),
            _ => GetNestedPropertyExpression(parameter, actualPath)
        };

        var containsMethod = typeof(string).GetMethod("Contains", [typeof(string)])!;
        var containsCall = Expression.Call(propertyExpr, containsMethod, Expression.Constant(stringValue));
        var nullCheck = Expression.NotEqual(propertyExpr, Expression.Constant(null, typeof(string)));
        return Expression.AndAlso(nullCheck, containsCall);
    }

    private Expression CompileStartsWith(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter, int? userId)
    {
        var stringValue = value.ToString()?.ToNormalizedString() ?? string.Empty;
        var propertyPath = fieldInfo.DbMapping.Split('.');
        var actualPath = propertyPath.Skip(1).ToArray();
        Expression propertyExpr = actualPath.Length switch
        {
            1 => Expression.Property(parameter, actualPath[0]),
            2 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1]]),
            3 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1], actualPath[2]]),
            _ => GetNestedPropertyExpression(parameter, actualPath)
        };

        var startsWithMethod = typeof(string).GetMethod("StartsWith", [typeof(string)])!;
        return Expression.Call(propertyExpr, startsWithMethod, Expression.Constant(stringValue));
    }

    private Expression CompileEndsWith(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter, int? userId)
    {
        var stringValue = value.ToString()?.ToNormalizedString() ?? string.Empty;
        var propertyPath = fieldInfo.DbMapping.Split('.');
        var actualPath = propertyPath.Skip(1).ToArray();
        Expression propertyExpr = actualPath.Length switch
        {
            1 => Expression.Property(parameter, actualPath[0]),
            2 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1]]),
            3 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1], actualPath[2]]),
            _ => GetNestedPropertyExpression(parameter, actualPath)
        };

        var endsWithMethod = typeof(string).GetMethod("EndsWith", [typeof(string)])!;
        return Expression.Call(propertyExpr, endsWithMethod, Expression.Constant(stringValue));
    }

    private Expression CompileWildcard(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter, int? userId)
    {
        var stringValue = value.ToString()?.ToNormalizedString() ?? string.Empty;
        var likePattern = stringValue.Replace("%", "[%]").Replace("_", "[_]").Replace("*", "%");

        var propertyPath = fieldInfo.DbMapping.Split('.');
        var actualPath = propertyPath.Skip(1).ToArray();
        Expression propertyExpr = actualPath.Length switch
        {
            1 => Expression.Property(parameter, actualPath[0]),
            2 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1]]),
            3 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1], actualPath[2]]),
            _ => GetNestedPropertyExpression(parameter, actualPath)
        };

        var ilikeMethod = typeof(Microsoft.EntityFrameworkCore.DbFunctionsExtensions).GetMethod("ILike",
            [typeof(Microsoft.EntityFrameworkCore.DbFunctions), typeof(string), typeof(string)])!;

        return Expression.Call(ilikeMethod,
            Expression.Property(null, typeof(Microsoft.EntityFrameworkCore.EF), "Functions"),
            propertyExpr,
            Expression.Constant(likePattern));
    }

    private Expression CompileRegexExpression(RegexExpressionNode node, ParameterExpression parameter, int? userId)
    {
        if (!_options.EnableRegex)
        {
            return Expression.Constant(true);
        }

        var fieldInfo = MqlFieldRegistry.GetField(node.Field, "podcasts");
        if (fieldInfo is null)
        {
            return Expression.Constant(true);
        }

        var validationResult = _options.RegexGuard.ValidatePattern(node.Pattern);
        if (!validationResult.IsValid || validationResult.SafePattern == null)
        {
            return Expression.Constant(false);
        }

        var propertyPath = fieldInfo.DbMapping.Split('.');
        var actualPath = propertyPath.Skip(1).ToArray();
        Expression propertyExpr = actualPath.Length switch
        {
            1 => Expression.Property(parameter, actualPath[0]),
            2 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1]]),
            3 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1], actualPath[2]]),
            _ => GetNestedPropertyExpression(parameter, actualPath)
        };

        var isCaseInsensitive = node.Flags.Contains("i", StringComparison.OrdinalIgnoreCase);
        var regexOptions = RegexOptions.Compiled | (isCaseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None);

        try
        {
            var regex = new Regex(validationResult.SafePattern, regexOptions, TimeSpan.FromMilliseconds(100));
            var isMatchMethod = typeof(Regex).GetMethod("IsMatch", [typeof(string)])!;

            return Expression.Call(Expression.Constant(regex), isMatchMethod, propertyExpr);
        }
        catch
        {
            return Expression.Constant(false);
        }
    }

    private Expression CompileBinaryExpression(BinaryExpressionNode node, ParameterExpression parameter, int? userId)
    {
        var left = CompileNode(node.Left, parameter, userId);
        var right = CompileNode(node.Right, parameter, userId);

        return node.Operator.ToUpperInvariant() switch
        {
            "AND" => Expression.AndAlso(left, right),
            "OR" => Expression.OrElse(left, right),
            _ => Expression.AndAlso(left, right)
        };
    }

    private Expression CompileUnaryExpression(UnaryExpressionNode node, ParameterExpression parameter, int? userId)
    {
        var operand = CompileNode(node.Operand, parameter, userId);
        return Expression.Not(operand);
    }

    private Expression CompileRangeExpression(RangeNode node, ParameterExpression parameter, int? userId)
    {
        var fieldInfo = MqlFieldRegistry.GetField(node.Field, "podcasts");
        if (fieldInfo is null)
        {
            return Expression.Constant(true);
        }

        var propertyPath = fieldInfo.DbMapping.Split('.');
        var actualPath = propertyPath.Skip(1).ToArray();
        Expression propertyExpr = actualPath.Length switch
        {
            1 => Expression.Property(parameter, actualPath[0]),
            2 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1]]),
            3 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1], actualPath[2]]),
            _ => GetNestedPropertyExpression(parameter, actualPath)
        };

        var minValue = ConvertValueForComparison(node.Min, fieldInfo.Type, propertyExpr.Type, fieldInfo.ValueMultiplier);
        var maxValue = ConvertValueForComparison(node.Max, fieldInfo.Type, propertyExpr.Type, fieldInfo.ValueMultiplier);

        if (propertyExpr.Type.IsGenericType && propertyExpr.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = propertyExpr.Type.GetGenericArguments()[0];
            var nonNullableProperty = Expression.Convert(propertyExpr, underlyingType);
            var convertedMinValue = ConvertToType(minValue, underlyingType);
            var convertedMaxValue = ConvertToType(maxValue, underlyingType);
            var minComparison = Expression.GreaterThanOrEqual(nonNullableProperty, Expression.Constant(convertedMinValue, underlyingType));
            var maxComparison = Expression.LessThanOrEqual(nonNullableProperty, Expression.Constant(convertedMaxValue, underlyingType));
            var rangeCheck = Expression.AndAlso(minComparison, maxComparison);
            var hasValueCheck = Expression.NotEqual(propertyExpr, Expression.Constant(null, propertyExpr.Type));
            return Expression.AndAlso(hasValueCheck, rangeCheck);
        }

        var minComparisonExpr = Expression.GreaterThanOrEqual(propertyExpr, Expression.Constant(minValue, propertyExpr.Type));
        var maxComparisonExpr = Expression.LessThanOrEqual(propertyExpr, Expression.Constant(maxValue, propertyExpr.Type));

        return Expression.AndAlso(minComparisonExpr, maxComparisonExpr);
    }

    private static Expression GetNestedPropertyExpression(Expression expr, string[] propertyNames)
    {
        Expression result = expr;
        foreach (var propertyName in propertyNames)
        {
            result = Expression.Property(result, propertyName);
        }
        return result;
    }

    private static object ConvertValue(object value, MqlFieldType fieldType, Type targetType)
    {
        return fieldType switch
        {
            MqlFieldType.String => value.ToString()?.ToNormalizedString() ?? string.Empty,
            MqlFieldType.Number => Convert.ToDouble(value),
            MqlFieldType.Date => Convert.ToDateTime(value),
            MqlFieldType.Boolean => Convert.ToBoolean(value),
            _ => Convert.ChangeType(value, targetType)
        };
    }

    private static object ConvertValueForComparison(object value, MqlFieldType fieldType, Type targetType, double valueMultiplier = 1.0)
    {
        if (fieldType == MqlFieldType.Number)
        {
            var doubleValue = Convert.ToDouble(value) * valueMultiplier;
            if (targetType == typeof(double))
            {
                return doubleValue;
            }
            if (targetType == typeof(int))
            {
                return (int)doubleValue;
            }
            return doubleValue;
        }

        return ConvertValue(value, fieldType, targetType);
    }
}

using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Mql.Interfaces;
using Melodee.Mql.Models;

namespace Melodee.Mql;

/// <summary>
/// Compiles MQL AST into EF Core Expression predicates for Artist entities.
/// </summary>
public sealed class MqlArtistCompiler : IMqlCompiler<Artist>
{
    private readonly IMqlFieldInfoProvider _fieldInfoProvider;
    private readonly MqlOptions _options;

    public MqlArtistCompiler(IMqlFieldInfoProvider? fieldInfoProvider = null, MqlOptions? options = null)
    {
        _fieldInfoProvider = fieldInfoProvider ?? new MqlFieldInfoProvider();
        _options = options ?? new MqlOptions();
    }

    public Expression<Func<Artist, bool>> Compile(MqlAstNode ast, int? userId = null)
    {
        var parameter = Expression.Parameter(typeof(Artist), "a");
        var body = CompileNode(ast, parameter, userId);
        return Expression.Lambda<Func<Artist, bool>>(body, parameter);
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

        var nameExpr = Expression.Property(parameter, nameof(Artist.NameNormalized));

        var searchMethod = typeof(string).GetMethod("Contains", [typeof(string)])!;
        var nameContains = Expression.Call(nameExpr, searchMethod, Expression.Constant(searchTerm));

        return nameContains;
    }

    private Expression CompileFieldExpression(FieldExpressionNode node, ParameterExpression parameter, int? userId)
    {
        var fieldInfo = MqlFieldRegistry.GetField(node.Field, "artists");
        if (fieldInfo is null)
        {
            return Expression.Constant(true);
        }

        var effectiveOperator = node.Operator.ToLowerInvariant();
        // Only apply DefaultOperator for unquoted "equals" - "exactequals" means user explicitly quoted the value
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
        if (fieldInfo.IsUserScoped)
        {
            return CompileUserScopedEquals(fieldInfo, value, parameter, userId);
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

        var convertedValue = ConvertValueForComparison(value, fieldInfo.Type, propertyExpr.Type);
        return Expression.Equal(propertyExpr, Expression.Constant(convertedValue, propertyExpr.Type));
    }

    private Expression CompileUserScopedEquals(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter, int? userId)
    {
        if (userId is null)
        {
            return Expression.Constant(false);
        }

        var userArtistsProperty = Expression.Property(parameter, nameof(Artist.UserArtists));
        var userIdConstant = Expression.Constant(userId.Value);

        var userArtistParam = Expression.Parameter(typeof(UserArtist), "ua");
        var userIdMatch = Expression.Equal(
            Expression.Property(userArtistParam, nameof(UserArtist.UserId)),
            userIdConstant);

        Expression valueMatch = fieldInfo.Name switch
        {
            "rating" => CompileUserArtistPropertyEqual(userArtistParam, nameof(UserArtist.Rating), value),
            "starred" => CompileUserArtistPropertyEqual(userArtistParam, nameof(UserArtist.IsStarred), value, MqlFieldType.Boolean),
            "starredat" => CompileUserArtistPropertyEqual(userArtistParam, nameof(UserArtist.StarredAt), value, MqlFieldType.Date),
            _ => Expression.Constant(true)
        };

        var whereLambda = Expression.Lambda<Func<UserArtist, bool>>(
            Expression.AndAlso(userIdMatch, valueMatch),
            userArtistParam);

        var anyCall = Expression.Call(typeof(Enumerable), "Any", [typeof(UserArtist)], userArtistsProperty, whereLambda);
        return anyCall;
    }

    private Expression CompileUserArtistPropertyEqual(Expression userArtistParam, string propertyName, object value, MqlFieldType fieldType = MqlFieldType.Number)
    {
        var propertyExpr = Expression.Property(userArtistParam, propertyName);
        var convertedValue = ConvertValue(value, fieldType, propertyExpr.Type);
        return Expression.Equal(propertyExpr, Expression.Constant(convertedValue, propertyExpr.Type));
    }

    private Expression CompileLessThan(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter, int? userId)
    {
        if (fieldInfo.IsUserScoped)
        {
            return CompileUserScopedComparison(fieldInfo, value, parameter, userId, "LessThan");
        }

        return CompileComparison(fieldInfo, value, parameter, Expression.LessThan);
    }

    private Expression CompileLessThanOrEquals(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter, int? userId)
    {
        if (fieldInfo.IsUserScoped)
        {
            return CompileUserScopedComparison(fieldInfo, value, parameter, userId, "LessThanOrEquals");
        }

        return CompileComparison(fieldInfo, value, parameter, Expression.LessThanOrEqual);
    }

    private Expression CompileGreaterThan(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter, int? userId)
    {
        if (fieldInfo.IsUserScoped)
        {
            return CompileUserScopedComparison(fieldInfo, value, parameter, userId, "GreaterThan");
        }

        return CompileComparison(fieldInfo, value, parameter, Expression.GreaterThan);
    }

    private Expression CompileGreaterThanOrEquals(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter, int? userId)
    {
        if (fieldInfo.IsUserScoped)
        {
            return CompileUserScopedComparison(fieldInfo, value, parameter, userId, "GreaterThanOrEquals");
        }

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

        var convertedValue = ConvertValueForComparison(value, fieldInfo.Type, propertyExpr.Type);

        if (propertyExpr.Type.IsGenericType && propertyExpr.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = propertyExpr.Type.GetGenericArguments()[0];
            var nonNullableProperty = Expression.Convert(propertyExpr, underlyingType);
            var convertedConstant = Convert.ChangeType(convertedValue, underlyingType);
            var rightExpr = Expression.Constant(convertedConstant, underlyingType);
            var comparison = comparisonFactory(nonNullableProperty, rightExpr);
            var hasValueCheck = Expression.NotEqual(propertyExpr, Expression.Constant(null, propertyExpr.Type));
            return Expression.AndAlso(hasValueCheck, comparison);
        }

        return comparisonFactory(propertyExpr, Expression.Constant(convertedValue, propertyExpr.Type));
    }

    private Expression CompileUserScopedComparison(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter, int? userId, string comparisonType)
    {
        if (userId is null)
        {
            return Expression.Constant(false);
        }

        var userArtistsProperty = Expression.Property(parameter, nameof(Artist.UserArtists));
        var userIdConstant = Expression.Constant(userId.Value);

        var userArtistParam = Expression.Parameter(typeof(UserArtist), "ua");
        var userIdMatch = Expression.Equal(
            Expression.Property(userArtistParam, nameof(UserArtist.UserId)),
            userIdConstant);

        Expression valueMatch = fieldInfo.Name switch
        {
            "rating" => CompileUserArtistPropertyComparison(userArtistParam, nameof(UserArtist.Rating), value, comparisonType),
            "starredat" => CompileUserArtistPropertyComparison(userArtistParam, nameof(UserArtist.StarredAt), value, comparisonType),
            _ => Expression.Constant(true)
        };

        var whereLambda = Expression.Lambda<Func<UserArtist, bool>>(
            Expression.AndAlso(userIdMatch, valueMatch),
            userArtistParam);

        var anyCall = Expression.Call(typeof(Enumerable), "Any", [typeof(UserArtist)], userArtistsProperty, whereLambda);
        return anyCall;
    }

    private Expression CompileUserArtistPropertyComparison(Expression userArtistParam, string propertyName, object value, string comparisonType)
    {
        var propertyExpr = Expression.Property(userArtistParam, propertyName);
        var convertedValue = ConvertValueForComparison(value, MqlFieldType.Number, propertyExpr.Type);

        var rightExpr = Expression.Constant(convertedValue, propertyExpr.Type);
        return comparisonType switch
        {
            "LessThan" => Expression.LessThan(propertyExpr, rightExpr),
            "LessThanOrEquals" => Expression.LessThanOrEqual(propertyExpr, rightExpr),
            "GreaterThan" => Expression.GreaterThan(propertyExpr, rightExpr),
            "GreaterThanOrEquals" => Expression.GreaterThanOrEqual(propertyExpr, rightExpr),
            _ => Expression.Equal(propertyExpr, rightExpr)
        };
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
        return Expression.Call(propertyExpr, containsMethod, Expression.Constant(stringValue));
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

        var efFunctionsProperty = Expression.Property(null, typeof(Microsoft.EntityFrameworkCore.EF), "Functions");
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

        var fieldInfo = MqlFieldRegistry.GetField(node.Field, "artists");
        if (fieldInfo is null)
        {
            return Expression.Constant(true);
        }

        var validationResult = _options.RegexGuard.ValidatePattern(node.Pattern);
        if (!validationResult.IsValid)
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
            var regex = new Regex(validationResult.SafePattern ?? node.Pattern, regexOptions);
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
        var fieldInfo = MqlFieldRegistry.GetField(node.Field, "artists");
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

        var minValue = ConvertValueForComparison(node.Min, fieldInfo.Type, propertyExpr.Type);
        var maxValue = ConvertValueForComparison(node.Max, fieldInfo.Type, propertyExpr.Type);

        var minComparison = Expression.GreaterThanOrEqual(propertyExpr, Expression.Constant(minValue, propertyExpr.Type));
        var maxComparison = Expression.LessThanOrEqual(propertyExpr, Expression.Constant(maxValue, propertyExpr.Type));

        return Expression.AndAlso(minComparison, maxComparison);
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

    private static object ConvertValueForComparison(object value, MqlFieldType fieldType, Type targetType)
    {
        if (fieldType == MqlFieldType.Number)
        {
            var doubleValue = Convert.ToDouble(value);
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

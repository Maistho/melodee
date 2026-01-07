using System.Linq.Expressions;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Mql.Interfaces;
using Melodee.Mql.Models;

namespace Melodee.Mql;

/// <summary>
/// Compiles MQL AST into EF Core Expression predicates for Album entities.
/// </summary>
public sealed class MqlAlbumCompiler : IMqlCompiler<Album>
{
    private readonly IMqlFieldInfoProvider _fieldInfoProvider;

    public MqlAlbumCompiler(IMqlFieldInfoProvider? fieldInfoProvider = null)
    {
        _fieldInfoProvider = fieldInfoProvider ?? new MqlFieldInfoProvider();
    }

    public Expression<Func<Album, bool>> Compile(MqlAstNode ast, int? userId = null)
    {
        var parameter = Expression.Parameter(typeof(Album), "a");
        var body = CompileNode(ast, parameter, userId);
        return Expression.Lambda<Func<Album, bool>>(body, parameter);
    }

    private Expression CompileNode(MqlAstNode node, ParameterExpression parameter, int? userId)
    {
        return node switch
        {
            FreeTextNode freeText => CompileFreeText(freeText, parameter),
            FieldExpressionNode field => CompileFieldExpression(field, parameter, userId),
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

        var nameExpr = Expression.Property(parameter, nameof(Album.NameNormalized));
        var artistExpr = GetNestedPropertyExpression(parameter, ["Artist", "NameNormalized"]);

        var searchMethod = typeof(string).GetMethod("Contains", [typeof(string)])!;
        var nameContains = Expression.Call(nameExpr, searchMethod, Expression.Constant(searchTerm));
        var artistContains = Expression.Call(artistExpr, searchMethod, Expression.Constant(searchTerm));

        return Expression.OrElse(nameContains, artistContains);
    }

    private Expression CompileFieldExpression(FieldExpressionNode node, ParameterExpression parameter, int? userId)
    {
        var fieldInfo = MqlFieldRegistry.GetField(node.Field, "albums");
        if (fieldInfo is null)
        {
            return Expression.Constant(true);
        }

        return node.Operator.ToLowerInvariant() switch
        {
            "equals" => CompileEquals(fieldInfo, node.Value, parameter, userId),
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

        if (fieldInfo.Type == MqlFieldType.ArrayString && value is string stringValue)
        {
            return CompileArrayContains(parameter, fieldInfo.DbMapping.Split('.'), stringValue);
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

        var userAlbumsProperty = Expression.Property(parameter, nameof(Album.UserAlbums));
        var userIdConstant = Expression.Constant(userId.Value);

        var userAlbumParam = Expression.Parameter(typeof(UserAlbum), "ua");
        var userIdMatch = Expression.Equal(
            Expression.Property(userAlbumParam, nameof(UserAlbum.UserId)),
            userIdConstant);

        Expression valueMatch = fieldInfo.Name switch
        {
            "rating" => CompileUserAlbumPropertyEqual(userAlbumParam, nameof(UserAlbum.Rating), value),
            "plays" => CompileUserAlbumPropertyEqual(userAlbumParam, nameof(UserAlbum.PlayedCount), value),
            "starred" => CompileUserAlbumPropertyEqual(userAlbumParam, nameof(UserAlbum.IsStarred), value, MqlFieldType.Boolean),
            "starredat" => CompileUserAlbumPropertyEqual(userAlbumParam, nameof(UserAlbum.StarredAt), value, MqlFieldType.Date),
            "lastplayedat" => CompileUserAlbumPropertyEqual(userAlbumParam, nameof(UserAlbum.LastPlayedAt), value, MqlFieldType.Date),
            _ => Expression.Constant(true)
        };

        var whereLambda = Expression.Lambda<Func<UserAlbum, bool>>(
            Expression.AndAlso(userIdMatch, valueMatch),
            userAlbumParam);

        var anyCall = Expression.Call(typeof(Enumerable), "Any", [typeof(UserAlbum)], userAlbumsProperty, whereLambda);
        return anyCall;
    }

    private Expression CompileUserAlbumPropertyEqual(Expression userAlbumParam, string propertyName, object value, MqlFieldType fieldType = MqlFieldType.Number)
    {
        var propertyExpr = Expression.Property(userAlbumParam, propertyName);
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

        var userAlbumsProperty = Expression.Property(parameter, nameof(Album.UserAlbums));
        var userIdConstant = Expression.Constant(userId.Value);

        var userAlbumParam = Expression.Parameter(typeof(UserAlbum), "ua");
        var userIdMatch = Expression.Equal(
            Expression.Property(userAlbumParam, nameof(UserAlbum.UserId)),
            userIdConstant);

        Expression valueMatch = fieldInfo.Name switch
        {
            "rating" => CompileUserAlbumPropertyComparison(userAlbumParam, nameof(UserAlbum.Rating), value, comparisonType),
            "plays" => CompileUserAlbumPropertyComparison(userAlbumParam, nameof(UserAlbum.PlayedCount), value, comparisonType),
            "starredat" => CompileUserAlbumPropertyComparison(userAlbumParam, nameof(UserAlbum.StarredAt), value, comparisonType),
            "lastplayedat" => CompileUserAlbumPropertyComparison(userAlbumParam, nameof(UserAlbum.LastPlayedAt), value, comparisonType),
            _ => Expression.Constant(true)
        };

        var whereLambda = Expression.Lambda<Func<UserAlbum, bool>>(
            Expression.AndAlso(userIdMatch, valueMatch),
            userAlbumParam);

        var anyCall = Expression.Call(typeof(Enumerable), "Any", [typeof(UserAlbum)], userAlbumsProperty, whereLambda);
        return anyCall;
    }

    private Expression CompileUserAlbumPropertyComparison(Expression userAlbumParam, string propertyName, object value, string comparisonType)
    {
        var propertyExpr = Expression.Property(userAlbumParam, propertyName);
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

        if (fieldInfo.Type == MqlFieldType.ArrayString)
        {
            return CompileArrayContains(parameter, propertyPath, stringValue);
        }

        Expression propertyExpr = propertyPath.Length switch
        {
            1 => Expression.Property(parameter, propertyPath[0]),
            2 => GetNestedPropertyExpression(parameter, [propertyPath[1], propertyPath[2]]),
            3 => GetNestedPropertyExpression(parameter, [propertyPath[1], propertyPath[2], propertyPath[3]]),
            _ => GetNestedPropertyExpression(parameter, propertyPath.Skip(1).ToArray())
        };

        var containsMethod = typeof(string).GetMethod("Contains", [typeof(string)])!;
        return Expression.Call(propertyExpr, containsMethod, Expression.Constant(stringValue));
    }

    private Expression CompileArrayContains(ParameterExpression parameter, string[] propertyPath, string value)
    {
        var actualPath = propertyPath.Skip(1).ToArray();
        Expression arrayExpr = actualPath.Length switch
        {
            1 => Expression.Property(parameter, actualPath[0]),
            2 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1]]),
            3 => GetNestedPropertyExpression(parameter, [actualPath[0], actualPath[1], actualPath[2]]),
            _ => GetNestedPropertyExpression(parameter, actualPath)
        };

        var containsCall = Expression.Call(typeof(Enumerable), "Contains", [typeof(string)], arrayExpr, Expression.Constant(value));
        return containsCall;
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
        var fieldInfo = MqlFieldRegistry.GetField(node.Field, "albums");
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

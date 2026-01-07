using System.Linq.Expressions;
using Melodee.Common.Data.Models;
using Melodee.Mql.Interfaces;
using Melodee.Mql.Models;

namespace Melodee.Mql;

/// <summary>
/// Cached MQL compiler that wraps IMqlCompiler with expression caching.
/// </summary>
public sealed class MqlCachedCompiler : IMqlCompiler<Song>
{
    private readonly IMqlExpressionCache _cache;
    private readonly IMqlFieldInfoProvider _fieldInfoProvider;

    public MqlCachedCompiler(
        IMqlExpressionCache? cache = null,
        IMqlFieldInfoProvider? fieldInfoProvider = null)
    {
        _cache = cache ?? new MqlExpressionCache();
        _fieldInfoProvider = fieldInfoProvider ?? new MqlFieldInfoProvider();
    }

    public Expression<Func<Song, bool>> Compile(MqlAstNode ast, int? userId = null)
    {
        var cacheKey = GenerateCacheKey("songs", ast, userId);
        var factory = () => CompileInternal(ast, userId);
        return _cache.GetOrCreate(cacheKey, factory);
    }

    private static string GenerateCacheKey(string entityType, MqlAstNode ast, int? userId)
    {
        var normalizedQuery = NormalizeAst(ast);
        var userIdPart = userId.HasValue ? $":user{userId.Value}" : ":anon";
        return $"{entityType}:{normalizedQuery}{userIdPart}";
    }

    private static string NormalizeAst(MqlAstNode node)
    {
        return node switch
        {
            FreeTextNode freeText => $"ft:{freeText.Text.ToLowerInvariant()}",
            FieldExpressionNode field => $"{field.Field}:{field.Operator}:{field.Value}".ToLowerInvariant(),
            BinaryExpressionNode binary => $"({NormalizeAst(binary.Left)}{binary.Operator.ToUpperInvariant()}{NormalizeAst(binary.Right)})",
            UnaryExpressionNode unary => $"NOT({NormalizeAst(unary.Operand)})",
            GroupNode group => NormalizeAst(group.Inner),
            RangeNode range => $"{range.Field}:{range.Min}-{range.Max}",
            _ => "unknown"
        };
    }

    private Expression<Func<Song, bool>> CompileInternal(MqlAstNode ast, int? userId)
    {
        var parameter = Expression.Parameter(typeof(Song), "s");
        var body = CompileNode(ast, parameter, userId);
        return Expression.Lambda<Func<Song, bool>>(body, parameter);
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
        var searchTerm = node.Text.ToUpperInvariant();
        if (string.IsNullOrEmpty(searchTerm))
        {
            return Expression.Constant(true);
        }

        var titleExpr = Expression.Property(parameter, nameof(Song.TitleNormalized));
        var artistExpr = GetNestedPropertyExpression(parameter, ["Album", "Artist", "NameNormalized"]);
        var albumExpr = GetNestedPropertyExpression(parameter, ["Album", "NameNormalized"]);

        var searchMethod = typeof(string).GetMethod("Contains", [typeof(string)])!;
        var titleContains = Expression.Call(titleExpr, searchMethod, Expression.Constant(searchTerm));
        var artistContains = Expression.Call(artistExpr, searchMethod, Expression.Constant(searchTerm));
        var albumContains = Expression.Call(albumExpr, searchMethod, Expression.Constant(searchTerm));

        return Expression.OrElse(Expression.OrElse(titleContains, artistContains), albumContains);
    }

    private Expression CompileFieldExpression(FieldExpressionNode node, ParameterExpression parameter, int? userId)
    {
        var fieldInfo = MqlFieldRegistry.GetField(node.Field, "songs");
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
            return CompileArrayContains(parameter, fieldInfo.DbMapping.Split('.'), stringValue.ToUpperInvariant());
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

        var userSongsProperty = Expression.Property(parameter, nameof(Song.UserSongs));
        var userIdConstant = Expression.Constant(userId.Value);

        var userSongParam = Expression.Parameter(typeof(UserSong), "us");
        var userIdMatch = Expression.Equal(
            Expression.Property(userSongParam, nameof(UserSong.UserId)),
            userIdConstant);

        Expression valueMatch = fieldInfo.Name switch
        {
            "rating" => CompileUserSongPropertyEqual(userSongParam, nameof(UserSong.Rating), value),
            "plays" => CompileUserSongPropertyEqual(userSongParam, nameof(UserSong.PlayedCount), value),
            "starred" => CompileUserSongPropertyEqual(userSongParam, nameof(UserSong.IsStarred), value, MqlFieldType.Boolean),
            "starredat" => CompileUserSongPropertyEqual(userSongParam, nameof(UserSong.StarredAt), value, MqlFieldType.Date),
            "lastplayedat" => CompileUserSongPropertyEqual(userSongParam, nameof(UserSong.LastPlayedAt), value, MqlFieldType.Date),
            _ => Expression.Constant(true)
        };

        var whereLambda = Expression.Lambda<Func<UserSong, bool>>(
            Expression.AndAlso(userIdMatch, valueMatch),
            userSongParam);

        var anyCall = Expression.Call(typeof(Enumerable), "Any", [typeof(UserSong)], userSongsProperty, whereLambda);
        return anyCall;
    }

    private Expression CompileUserSongPropertyEqual(Expression userSongParam, string propertyName, object value, MqlFieldType fieldType = MqlFieldType.Number)
    {
        var propertyExpr = Expression.Property(userSongParam, propertyName);
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

        var userSongsProperty = Expression.Property(parameter, nameof(Song.UserSongs));
        var userIdConstant = Expression.Constant(userId.Value);

        var userSongParam = Expression.Parameter(typeof(UserSong), "us");
        var userIdMatch = Expression.Equal(
            Expression.Property(userSongParam, nameof(UserSong.UserId)),
            userIdConstant);

        Expression valueMatch = fieldInfo.Name switch
        {
            "rating" => CompileUserSongPropertyComparison(userSongParam, nameof(UserSong.Rating), value, comparisonType),
            "plays" => CompileUserSongPropertyComparison(userSongParam, nameof(UserSong.PlayedCount), value, comparisonType),
            "starredat" => CompileUserSongPropertyComparison(userSongParam, nameof(UserSong.StarredAt), value, comparisonType),
            "lastplayedat" => CompileUserSongPropertyComparison(userSongParam, nameof(UserSong.LastPlayedAt), value, comparisonType),
            _ => Expression.Constant(true)
        };

        var whereLambda = Expression.Lambda<Func<UserSong, bool>>(
            Expression.AndAlso(userIdMatch, valueMatch),
            userSongParam);

        var anyCall = Expression.Call(typeof(Enumerable), "Any", [typeof(UserSong)], userSongsProperty, whereLambda);
        return anyCall;
    }

    private Expression CompileUserSongPropertyComparison(Expression userSongParam, string propertyName, object value, string comparisonType)
    {
        var propertyExpr = Expression.Property(userSongParam, propertyName);
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
        var stringValue = value.ToString()?.ToUpperInvariant() ?? string.Empty;
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

        var itemParam = Expression.Parameter(typeof(string), "item");
        var containsCall = Expression.Call(typeof(Enumerable), "Contains", [typeof(string)], arrayExpr, Expression.Constant(value));

        var lambda = Expression.Lambda<Func<string, bool>>(containsCall, itemParam);
        return Expression.Call(typeof(Enumerable), "Any", [typeof(string)], arrayExpr, lambda);
    }

    private Expression CompileStartsWith(MqlFieldInfo fieldInfo, object value, ParameterExpression parameter, int? userId)
    {
        var stringValue = value.ToString()?.ToUpperInvariant() ?? string.Empty;
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
        var stringValue = value.ToString()?.ToUpperInvariant() ?? string.Empty;
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
        var stringValue = value.ToString()?.ToUpperInvariant() ?? string.Empty;
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
        var fieldInfo = MqlFieldRegistry.GetField(node.Field, "songs");
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
            MqlFieldType.String => value.ToString()?.ToUpperInvariant() ?? string.Empty,
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

    public IMqlExpressionCache Cache => _cache;
}

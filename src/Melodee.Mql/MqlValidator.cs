using System.Text.RegularExpressions;
using Melodee.Mql.Constants;
using Melodee.Mql.Interfaces;
using Melodee.Mql.Models;

namespace Melodee.Mql;

/// <summary>
/// Validates MQL queries for syntax correctness, security limits, and field validity.
/// </summary>
public sealed class MqlValidator : IMqlValidator
{
    private static readonly HashSet<string> BooleanOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        MqlOperators.And,
        MqlOperators.Or,
        MqlOperators.Not
    };

    private static readonly string[] ProhibitedRegexPatterns =
    {
        @"(.*)*",
        @"(.+)+",
        @"([a-z]*)*",
        @"([a-z]+)+"
    };

    public MqlValidationResult Validate(string query, string entityType)
    {
        var errors = new List<MqlError>();
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(query))
        {
            return MqlValidationResult.Invalid(new List<MqlError>
            {
                new MqlError(
                    MqlErrorCodes.MqlEmptyQuery,
                    "Query cannot be empty",
                    null)
            });
        }

        var normalizedQuery = query.Trim();

        if (normalizedQuery.Length > MqlConstants.MaxQueryLength)
        {
            errors.Add(new MqlError(
                MqlErrorCodes.MqlQueryTooLong,
                $"Query exceeds maximum length of {MqlConstants.MaxQueryLength} characters (current: {normalizedQuery.Length})",
                CalculatePosition(normalizedQuery, MqlConstants.MaxQueryLength)));
        }

        var fieldCount = CountFieldFilters(normalizedQuery);
        if (fieldCount > MqlConstants.MaxFieldCount)
        {
            errors.Add(new MqlError(
                MqlErrorCodes.MqlTooManyFields,
                $"Query contains too many field filters ({fieldCount}). Maximum allowed is {MqlConstants.MaxFieldCount}.",
                null));
        }

        var (balanced, unbalancedMessage, position) = CheckBalancedParentheses(normalizedQuery);
        if (!balanced)
        {
            errors.Add(new MqlError(
                MqlErrorCodes.MqlUnbalancedParens,
                unbalancedMessage,
                position));
        }

        var maxDepth = CalculateMaxRecursionDepth(normalizedQuery);
        if (maxDepth > MqlConstants.MaxRecursionDepth)
        {
            errors.Add(new MqlError(
                MqlErrorCodes.MqlTooDeep,
                $"Query nesting depth ({maxDepth}) exceeds maximum allowed depth of {MqlConstants.MaxRecursionDepth}.",
                null));
        }

        var fieldValidationResult = ValidateFields(normalizedQuery, entityType);
        errors.AddRange(fieldValidationResult.Errors);
        warnings.AddRange(fieldValidationResult.Warnings);

        var regexValidationResult = ValidateRegexPatterns(normalizedQuery);
        errors.AddRange(regexValidationResult);

        var complexityScore = CalculateComplexityScore(normalizedQuery, fieldCount, maxDepth, CountRegexPatterns(normalizedQuery));

        if (complexityScore > 20)
        {
            errors.Add(new MqlError(
                MqlErrorCodes.MqlParseError,
                $"Query complexity score ({complexityScore}) exceeds maximum allowed (20). Please simplify your query.",
                null));
        }
        else if (complexityScore > 10)
        {
            warnings.Add($"Query complexity score ({complexityScore}) is high. Consider simplifying.");
        }

        if (errors.Count > 0)
        {
            return MqlValidationResult.Invalid(errors);
        }

        return MqlValidationResult.Valid(warnings, complexityScore);
    }

    private static int CountFieldFilters(string query)
    {
        var count = 0;
        var i = 0;
        while (i < query.Length)
        {
            var colonIndex = query.IndexOf(':', i);
            if (colonIndex == -1)
            {
                break;
            }

            var fieldStart = colonIndex;
            while (fieldStart > 0)
            {
                var prevChar = query[fieldStart - 1];
                if (char.IsWhiteSpace(prevChar) || prevChar == '(')
                {
                    break;
                }

                fieldStart--;
            }

            if (fieldStart < 0)
            {
                fieldStart = 0;
            }

            var fieldLength = colonIndex - fieldStart;
            if (fieldLength <= 0)
            {
                i = colonIndex + 1;
                continue;
            }

            var potentialField = query.Substring(fieldStart, fieldLength).Trim();

            if (!string.IsNullOrEmpty(potentialField) &&
                char.IsLetterOrDigit(potentialField[0]) &&
                !BooleanOperators.Contains(potentialField) &&
                !potentialField.StartsWith("-"))
            {
                count++;
            }

            i = colonIndex + 1;
        }

        return count;
    }

    private static (bool Balanced, string Message, MqlErrorPosition? Position) CheckBalancedParentheses(string query)
    {
        var depth = 0;
        var maxDepth = 0;
        var position = 0;

        for (var i = 0; i < query.Length; i++)
        {
            if (query[i] == '(')
            {
                depth++;
                if (depth > maxDepth)
                {
                    maxDepth = depth;
                    position = i;
                }
            }
            else if (query[i] == ')')
            {
                depth--;
                if (depth < 0)
                {
                    return (false, "Unbalanced parentheses: unexpected closing ')'", CalculatePosition(query, i));
                }
            }
        }

        if (depth > 0)
        {
            return (false, $"Unbalanced parentheses: {depth} missing closing ')' parenthesis", CalculatePosition(query, query.Length - 1));
        }

        return (true, string.Empty, null);
    }

    private static int CalculateMaxRecursionDepth(string query)
    {
        var depth = 0;
        var maxDepth = 0;

        foreach (var ch in query)
        {
            if (ch == '(')
            {
                depth++;
                if (depth > maxDepth)
                {
                    maxDepth = depth;
                }
            }
            else if (ch == ')')
            {
                depth--;
            }
        }

        return maxDepth;
    }

    private static int CountRegexPatterns(string query)
    {
        var regexPattern = new Regex(@"/(.+?)/(i|g|ig)?", RegexOptions.Compiled);
        return regexPattern.Matches(query).Count;
    }

    private static (List<MqlError> Errors, List<string> Warnings) ValidateFields(string query, string entityType)
    {
        var errors = new List<MqlError>();
        var warnings = new List<string>();

        if (!MqlFieldRegistry.GetEntityTypes().Contains(entityType.ToLowerInvariant()))
        {
            return (errors, warnings);
        }

        var i = 0;
        while (i < query.Length)
        {
            var colonIndex = query.IndexOf(':', i);
            if (colonIndex == -1 || colonIndex >= query.Length - 1)
            {
                break;
            }

            var potentialFieldEnd = colonIndex;
            while (potentialFieldEnd > 0 && !char.IsWhiteSpace(query[potentialFieldEnd - 1]) &&
                   query[potentialFieldEnd - 1] != '(')
            {
                potentialFieldEnd--;
            }

            var potentialField = query.Substring(potentialFieldEnd, colonIndex - potentialFieldEnd).Trim();

            if (!string.IsNullOrEmpty(potentialField) &&
                char.IsLetterOrDigit(potentialField[0]) &&
                !BooleanOperators.Contains(potentialField))
            {
                var field = MqlFieldRegistry.GetField(potentialField, entityType);
                if (field is null)
                {
                    var suggestions = GetFieldSuggestions(potentialField, entityType);
                    var suggestionText = suggestions.Length > 0
                        ? $" Did you mean '{string.Join("', '", suggestions)}'?"
                        : string.Empty;

                    errors.Add(new MqlError(
                        MqlErrorCodes.MqlUnknownField,
                        $"Unknown field '{potentialField}' for entity type '{entityType}'.{suggestionText}",
                        CalculatePosition(query, colonIndex - potentialField.Length),
                        suggestions));

                    var nearestField = suggestions.Length > 0 ? suggestions[0] : null;
                    if (nearestField != null)
                    {
                        warnings.Add($"Unknown field '{potentialField}' - assuming you meant '{nearestField}'");
                    }
                }
                else
                {
                    var opStart = colonIndex;
                    while (opStart < query.Length && query[opStart] == ':')
                    {
                        opStart++;
                    }

                    if (opStart < query.Length)
                    {
                        var remaining = query.Substring(opStart);
                        var opMatch = Regex.Match(remaining, @"^[:!<>][=><]*|^[a-zA-Z]+");
                        if (opMatch.Success)
                        {
                            var opValue = opMatch.Value;
                            if (opValue.StartsWith(":") || opValue.StartsWith("!") || opValue.StartsWith("<") || opValue.StartsWith(">"))
                            {
                                if (!IsValidOperatorForField(opValue, field.Type))
                                {
                                    errors.Add(new MqlError(
                                        MqlErrorCodes.MqlParseError,
                                        $"Operator '{opValue}' is not valid for field '{field.Name}' of type {field.Type}",
                                        CalculatePosition(query, colonIndex)));
                                }
                            }
                        }
                    }
                }
            }

            i = colonIndex + 1;
        }

        return (errors, warnings);
    }

    private static string[] GetFieldSuggestions(string fieldName, string entityType)
    {
        var allFields = MqlFieldRegistry.GetFieldNames(entityType).ToList();
        var suggestions = new List<string>();

        foreach (var field in allFields)
        {
            var distance = LevenshteinDistance(fieldName.ToLowerInvariant(), field.ToLowerInvariant());
            if (distance <= 2 && distance > 0)
            {
                suggestions.Add(field);
            }
        }

        return suggestions.OrderBy(s => LevenshteinDistance(fieldName.ToLowerInvariant(), s.ToLowerInvariant())).ToArray();
    }

    private static int LevenshteinDistance(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1))
        {
            return string.IsNullOrEmpty(s2) ? 0 : s2.Length;
        }

        if (string.IsNullOrEmpty(s2))
        {
            return s1.Length;
        }

        var matrix = new int[s1.Length + 1, s2.Length + 1];

        for (var i = 0; i <= s1.Length; i++)
        {
            matrix[i, 0] = i;
        }

        for (var j = 0; j <= s2.Length; j++)
        {
            matrix[0, j] = j;
        }

        for (var i = 1; i <= s1.Length; i++)
        {
            for (var j = 1; j <= s2.Length; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[s1.Length, s2.Length];
    }

    private static bool IsValidOperatorForOperatorString(string op)
    {
        if (string.IsNullOrEmpty(op))
        {
            return true;
        }

        var normalizedOp = op.Trim().ToLowerInvariant();
        if (normalizedOp.StartsWith(':'))
        {
            normalizedOp = normalizedOp.TrimStart(':');
        }

        return MqlOperators.ComparisonOperators.Any(so => so.TrimStart(':').Equals(normalizedOp, StringComparison.OrdinalIgnoreCase)) ||
               MqlOperators.StringOperators.Contains(normalizedOp, StringComparer.OrdinalIgnoreCase) ||
               normalizedOp is "=" or "==" or "!=" or "<" or "<=" or ">" or ">=";
    }

    private static bool IsValidOperatorForField(string op, MqlFieldType fieldType)
    {
        var normalizedOp = op.TrimStart(':').ToLowerInvariant();

        return fieldType switch
        {
            MqlFieldType.String or MqlFieldType.ArrayString => true,
            MqlFieldType.Number or MqlFieldType.Date => normalizedOp is ":=" or ":!=" or ":<" or ":<=" or ":>" or ":>=" or ":contains" or ":startswith" or ":endswith" or ":wildcard" or "=" or "!=" or "<" or "<=" or ">" or ">=" or "contains" or "startswith" or "endswith" or "wildcard",
            MqlFieldType.Boolean => normalizedOp is ":=" or ":!=" or "=" or "!=" or "true" or "false",
            MqlFieldType.Guid => normalizedOp is ":=" or ":!=",
            _ => true
        };
    }

    private static List<MqlError> ValidateRegexPatterns(string query)
    {
        var errors = new List<MqlError>();

        var regexPattern = new Regex(@"/(.+?)/(i|g|ig)?", RegexOptions.Compiled);
        var matches = regexPattern.Matches(query);

        foreach (Match match in matches)
        {
            var pattern = match.Groups[1].Value;

            if (pattern.Length > MqlConstants.MaxRegexPatternLength)
            {
                errors.Add(new MqlError(
                    MqlErrorCodes.MqlRegexTooComplex,
                    $"Regex pattern exceeds maximum length of {MqlConstants.MaxRegexPatternLength} characters (current: {pattern.Length})",
                    CalculatePosition(query, match.Index)));
            }

            foreach (var prohibited in ProhibitedRegexPatterns)
            {
                if (pattern.Contains(prohibited, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(new MqlError(
                        MqlErrorCodes.MqlRegexDangerous,
                        "Regex pattern contains potentially dangerous construct that could cause ReDoS",
                        CalculatePosition(query, match.Index)));
                }
            }
        }

        return errors;
    }

    private static int CalculateComplexityScore(string query, int fieldCount, int maxDepth, int regexCount)
    {
        var score = fieldCount;

        var andCount = CountOccurrences(query, " AND ", true);
        var orCount = CountOccurrences(query, " OR ", true);
        var notCount = CountOccurrences(query, "NOT ", true);

        score += andCount + orCount + notCount;

        score += maxDepth * 2;

        score += regexCount * 5;

        return score;
    }

    private static int CountOccurrences(string source, string pattern, bool wordBoundary)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(pattern))
        {
            return 0;
        }

        var count = 0;
        var startIndex = 0;

        while ((startIndex = source.IndexOf(pattern, startIndex, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            if (wordBoundary)
            {
                var beforeIndex = startIndex - 1;
                var afterIndex = startIndex + pattern.Length;

                var validBefore = beforeIndex < 0 || char.IsWhiteSpace(source[beforeIndex]) || source[beforeIndex] == '(' || source[beforeIndex] == ':' || char.IsLetterOrDigit(source[beforeIndex]);
                var validAfter = afterIndex >= source.Length || char.IsWhiteSpace(source[afterIndex]) || source[afterIndex] == ')' || source[afterIndex] == ':' || char.IsLetterOrDigit(source[afterIndex]);

                if (validBefore && validAfter)
                {
                    count++;
                }
            }
            else
            {
                count++;
            }

            startIndex += pattern.Length;
        }

        return count;
    }

    private static MqlErrorPosition? CalculatePosition(string query, int index)
    {
        if (index < 0 || index >= query.Length)
        {
            return null;
        }

        var line = 1;
        var column = 1;
        var currentIndex = 0;

        while (currentIndex < index && currentIndex < query.Length)
        {
            if (query[currentIndex] == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }

            currentIndex++;
        }

        return new MqlErrorPosition(index, Math.Min(index + 1, query.Length), line, column);
    }
}

using Melodee.Mql.Constants;
using Melodee.Mql.Interfaces;
using Melodee.Mql.Models;

namespace Melodee.Mql;

/// <summary>
/// Parses MQL tokens into an Abstract Syntax Tree (AST).
/// Uses recursive descent parsing with operator precedence:
/// Highest: Parentheses
///       : NOT
///       : AND
/// Lowest : OR
/// </summary>
public sealed class MqlParser : IMqlParser
{
    private readonly List<MqlError> _errors = new();
    private readonly List<string> _warnings = new();
    private IEnumerator<MqlToken>? _tokenEnumerator;
    private MqlToken _currentToken = MqlToken.EndOfInput(1, 1);
    private int _parenthesesDepth;
    private System.Text.StringBuilder _normalizedQuery = new();

    public MqlParseResult Parse(IEnumerable<MqlToken> tokens, string entityType)
    {
        _errors.Clear();
        _warnings.Clear();
        _normalizedQuery.Clear();
        _parenthesesDepth = 0;

        _tokenEnumerator = tokens.GetEnumerator();

        if (!_tokenEnumerator.MoveNext())
        {
            _currentToken = MqlToken.EndOfInput(1, 1);
        }
        else
        {
            _currentToken = _tokenEnumerator.Current;
        }

        try
        {
            var ast = ParseQuery();

            // Consume any remaining tokens
            while (_currentToken.Type != MqlTokenType.EndOfInput)
            {
                if (_currentToken.Type != MqlTokenType.Unknown)
                {
                    AddError(MqlErrorCodes.MqlParseError,
                        $"Unexpected token: {_currentToken.Value}",
                        _currentToken);
                }

                Advance();
            }

            if (_errors.Count > 0)
            {
                return MqlParseResult.Failed(_errors, _normalizedQuery.ToString());
            }

            return MqlParseResult.Success(ast!, _normalizedQuery.ToString(), _warnings);
        }
        catch (Exception ex)
        {
            _errors.Add(new MqlError(
                MqlErrorCodes.MqlParseError,
                $"Parse error: {ex.Message}",
                new MqlErrorPosition(_currentToken.StartPosition, _currentToken.EndPosition, _currentToken.Line, _currentToken.Column)));

            return MqlParseResult.Failed(_errors, _normalizedQuery.ToString());
        }
    }

    private MqlAstNode? ParseQuery()
    {
        return ParseOrExpression();
    }

    private MqlAstNode? ParseOrExpression()
    {
        var left = ParseAndExpression();

        while (Match(MqlTokenType.Or))
        {
            var orToken = _currentToken;
            AppendToNormalized(" OR ");
            Advance();

            var right = ParseAndExpression();

            if (left is not null)
            {
                left = new BinaryExpressionNode("OR", left, right ?? CreateErrorNode(orToken));
            }
            else
            {
                left = right;
            }
        }

        return left;
    }

    private MqlAstNode? ParseAndExpression()
    {
        var left = ParseUnaryExpression();

        while (true)
        {
            // Check for AND keyword (explicit) or implicit AND (next term)
            if (Match(MqlTokenType.And))
            {
                var andToken = _currentToken;
                AppendToNormalized(" AND ");
                Advance();

                var right = ParseUnaryExpression();

                left = left is not null
                    ? new BinaryExpressionNode("AND", left, right ?? CreateErrorNode(andToken))
                    : right;
            }
            else if (CanBeImplicitAnd(_currentToken))
            {
                // Implicit AND - tokens that can follow an expression without explicit operator
                AppendToNormalized(" AND ");
                var andToken = _currentToken;
                var right = ParseUnaryExpression();

                left = left is not null
                    ? new BinaryExpressionNode("AND", left, right ?? CreateErrorNode(andToken))
                    : right;
            }
            else
            {
                break;
            }
        }

        return left;
    }

    private MqlAstNode? ParseUnaryExpression()
    {
        if (Match(MqlTokenType.Not))
        {
            var notToken = _currentToken;
            AppendToNormalized("NOT ");
            Advance();

            var operand = ParseUnaryExpression();

            return new UnaryExpressionNode("NOT", operand ?? CreateErrorNode(notToken));
        }

        return ParsePrimary();
    }

    private MqlAstNode? ParsePrimary()
    {
        // Parenthesized expression
        if (Match(MqlTokenType.LeftParen))
        {
            var lparenToken = _currentToken;
            AppendToNormalized("( ");
            _parenthesesDepth++;
            Advance();

            var inner = ParseQuery();

            if (!Match(MqlTokenType.RightParen))
            {
                AddError(MqlErrorCodes.MqlUnbalancedParens,
                    "Missing closing parenthesis",
                    _currentToken);
            }
            else
            {
                AppendToNormalized(" )");
                _parenthesesDepth--;
                Advance();
            }

            return new GroupNode(inner!);
        }

        // Field expression: FieldName : [Operator] Value
        if (_currentToken.Type == MqlTokenType.FieldName)
        {
            return ParseFieldExpression();
        }

        // Free text
        if (_currentToken.Type == MqlTokenType.FreeText)
        {
            return ParseFreeText();
        }

        // Range token (e.g., 1970-1980)
        if (_currentToken.Type == MqlTokenType.Range)
        {
            return ParseRangeExpression();
        }

        // Handle And/Or tokens as free text when not in valid operator position
        if (_currentToken.Type == MqlTokenType.And || _currentToken.Type == MqlTokenType.Or)
        {
            return ParseFreeText();
        }

        // Handle unexpected tokens
        if (_currentToken.Type != MqlTokenType.EndOfInput)
        {
            AddError(MqlErrorCodes.MqlParseError,
                $"Unexpected token: {_currentToken.Value}",
                _currentToken);
            Advance();
        }

        return null;
    }

    private MqlAstNode ParseFieldExpression()
    {
        var fieldName = _currentToken.Value;
        AppendToNormalized(fieldName.ToLowerInvariant());
        var fieldToken = _currentToken;

        Advance();

        // Expect colon operator
        if (!Match(MqlTokenType.Operator))
        {
            AddError(MqlErrorCodes.MqlParseError,
                "Expected ':' after field name",
                _currentToken);

            return new FieldExpressionNode(fieldName, "Equals", fieldToken.Value, fieldToken);
        }

        var opToken = _currentToken;
        var opValue = _currentToken.Value;
        var op = DetermineOperator(opValue);
        AppendToNormalized(opValue);

        Advance();

        // Parse value
        var value = ParseValue();

        if (value is null)
        {
            // No value after operator (e.g., "artist:") - return field expression with empty value
            return new FieldExpressionNode(fieldName, op, string.Empty, fieldToken);
        }

        Advance(); // Move past the value token

        // Check if this is actually a range (if value is a string containing -)
        if (value is string strValue && strValue.Contains('-') && !strValue.StartsWith("-"))
        {
            var rangeParts = strValue.Split('-', 2);
            if (rangeParts.Length == 2 &&
                double.TryParse(rangeParts[0], out var min) &&
                double.TryParse(rangeParts[1], out var max))
            {
                AppendToNormalized(strValue);
                Advance(); // Move past the range value
                return new RangeNode(fieldName, min, max, fieldToken);
            }
        }

        var valueStr = value.ToString() ?? string.Empty;
        AppendToNormalized(valueStr);

        return new FieldExpressionNode(fieldName, op, value, fieldToken);
    }

    private MqlAstNode ParseFreeText()
    {
        var text = _currentToken.Value;
        AppendToNormalized(text);

        var freeTextToken = _currentToken;
        Advance();

        return new FreeTextNode(text, freeTextToken);
    }

    private MqlAstNode ParseRangeExpression()
    {
        var rangeValue = _currentToken.Value;
        AppendToNormalized(rangeValue);

        var rangeToken = _currentToken;
        Advance();

        // For now, return as free text - ranges should be handled by field:min-max syntax
        return new FreeTextNode(rangeValue, rangeToken);
    }

    private object? ParseValue()
    {
        var token = _currentToken;

        return token.Type switch
        {
            MqlTokenType.StringLiteral => ParseStringValue(token.Value),
            MqlTokenType.NumberLiteral => ParseNumberValue(token.Value),
            MqlTokenType.DateLiteral => ParseDateValue(token.Value),
            MqlTokenType.BooleanLiteral => ParseBooleanValue(token.Value),
            MqlTokenType.FreeText => token.Value,
            MqlTokenType.Range => token.Value,
            MqlTokenType.Regex => token.Value,
            MqlTokenType.FieldName => token.Value,
            _ => null
        };
    }

    private static object ParseStringValue(string value)
    {
        return value;
    }

    private static object ParseNumberValue(string value)
    {
        if (value.Contains('.'))
        {
            return double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        }

        if (int.TryParse(value, out var intValue))
        {
            return intValue;
        }

        return double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static object ParseDateValue(string value)
    {
        // Relative dates like -7d, -3w, -12h
        if (value.StartsWith("-") && value.Length > 2)
        {
            return value;
        }

        // Date keywords
        if (value is "today" or "yesterday" or "last-week" or "last-month" or "last-year")
        {
            return value;
        }

        // YYYY-MM-DD format
        if (value.Length == 10 && value.Contains('-'))
        {
            return value;
        }

        return value;
    }

    private static bool ParseBooleanValue(string value)
    {
        return value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    private static string DetermineOperator(string op)
    {
        return op switch
        {
            ":=" => "Equals",
            ":!=" => "NotEquals",
            ":<" => "LessThan",
            ":<=" => "LessThanOrEquals",
            ":>" => "GreaterThan",
            ":>=" => "GreaterThanOrEquals",
            ":" => "Equals",
            _ => "Equals"
        };
    }

    private bool Match(MqlTokenType type)
    {
        return _currentToken.Type == type;
    }

    private bool CanBeImplicitAnd(MqlToken token)
    {
        return token.Type switch
        {
            MqlTokenType.FreeText => true,
            MqlTokenType.FieldName => true,
            MqlTokenType.LeftParen => true,
            MqlTokenType.Not => true,
            _ => false
        };
    }

    private void Advance()
    {
        if (_tokenEnumerator?.MoveNext() == true)
        {
            _currentToken = _tokenEnumerator.Current;
        }
        else
        {
            _currentToken = MqlToken.EndOfInput(_currentToken.Line, _currentToken.Column);
        }
    }

    private void AddError(string code, string message, MqlToken token)
    {
        _errors.Add(new MqlError(
            code,
            message,
            new MqlErrorPosition(token.StartPosition, token.EndPosition, token.Line, token.Column)));
    }

    private void AppendToNormalized(string text)
    {
        _normalizedQuery.Append(text);
    }

    private static MqlAstNode CreateErrorNode(MqlToken token)
    {
        return new FreeTextNode("[error]", token);
    }
}

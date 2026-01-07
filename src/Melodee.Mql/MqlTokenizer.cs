using Melodee.Mql.Constants;
using Melodee.Mql.Interfaces;
using Melodee.Mql.Models;

namespace Melodee.Mql;

/// <summary>
/// Tokenizes MQL query strings into a stream of tokens with position tracking.
/// </summary>
public sealed class MqlTokenizer : IMqlTokenizer
{
    private static readonly HashSet<string> BooleanKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        MqlOperators.And,
        MqlOperators.Or,
        MqlOperators.Not
    };

    private static readonly HashSet<string> DateKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "today",
        "yesterday",
        "last-week",
        "last-month",
        "last-year"
    };

    public IEnumerable<MqlToken> Tokenize(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            yield return MqlToken.EndOfInput(1, 1);
            yield break;
        }

        var position = 0;
        var line = 1;
        var column = 1;
        var length = query.Length;

        while (position < length)
        {
            var ch = query[position];

            // Whitespace - skip but track position
            if (char.IsWhiteSpace(ch))
            {
                UpdatePosition(ch, ref line, ref column, ref position);
                continue;
            }

            var startPosition = position;
            var startLine = line;
            var startColumn = column;

            // Handle string literals (quoted)
            if (ch == '"')
            {
                var token = ReadStringLiteral(query, ref position, ref line, ref column, startPosition, startLine, startColumn);
                yield return token;
                continue;
            }

            // Handle regex patterns
            if (ch == '/')
            {
                var token = ReadRegexLiteral(query, ref position, ref line, ref column, startPosition, startLine, startColumn);
                yield return token;
                continue;
            }

            // Handle parentheses
            if (ch == '(')
            {
                position++;
                column++;
                yield return new MqlToken(MqlTokenType.LeftParen, "(", startPosition, position, startLine, startColumn);
                continue;
            }

            if (ch == ')')
            {
                position++;
                column++;
                yield return new MqlToken(MqlTokenType.RightParen, ")", startPosition, position, startLine, startColumn);
                continue;
            }

            // Handle negative number or range expression starting with digit
            if (char.IsDigit(ch) || (ch == '-' && position + 1 < length && char.IsDigit(query[position + 1])))
            {
                var token = ReadNumberOrDate(query, ref position, ref line, ref column, startPosition, startLine, startColumn);
                yield return token;
                continue;
            }

            // Check for comparison operators starting with colon
            if (ch == ':')
            {
                var op = ReadOperator(query, ref position, ref line, ref column, startPosition, startLine, startColumn);
                yield return op;
                continue;
            }

            // Handle identifiers (field names, free text, keywords)
            var tokenValue = ReadIdentifier(query, ref position, ref line, ref column);
            if (string.IsNullOrEmpty(tokenValue))
            {
                // Unknown character - emit error token and continue
                yield return new MqlToken(MqlTokenType.Unknown, ch.ToString(), startPosition, position + 1, startLine, startColumn);
                position++;
                column++;
                continue;
            }

            // Determine token type - check if followed by colon for field names
            var tokenType = DetermineTokenType(tokenValue, query, position);
            yield return new MqlToken(tokenType, tokenValue, startPosition, position, startLine, startColumn);
        }

        yield return MqlToken.EndOfInput(line, column);
    }

    private static MqlToken ReadStringLiteral(
        string query,
        ref int position,
        ref int line,
        ref int column,
        int startPosition,
        int startLine,
        int startColumn)
    {
        position++; // Skip opening quote
        column++;
        var sb = new System.Text.StringBuilder();
        var escaped = false;

        while (position < query.Length)
        {
            var ch = query[position];

            if (escaped)
            {
                // Handle escape sequences
                sb.Append(ch switch
                {
                    'n' => '\n',
                    'r' => '\r',
                    't' => '\t',
                    '\\' => '\\',
                    '"' => '"',
                    _ => ch
                });
                escaped = false;
                position++;
                column++;
                continue;
            }

            if (ch == '\\')
            {
                escaped = true;
                position++;
                column++;
                continue;
            }

            if (ch == '"')
            {
                position++;
                column++;
                break;
            }

            if (ch == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }

            sb.Append(ch);
            position++;
        }

        return new MqlToken(MqlTokenType.StringLiteral, sb.ToString(), startPosition, position, startLine, startColumn);
    }

    private static MqlToken ReadRegexLiteral(
        string query,
        ref int position,
        ref int line,
        ref int column,
        int startPosition,
        int startLine,
        int startColumn)
    {
        position++; // Skip opening /
        column++;
        var sb = new System.Text.StringBuilder();
        var escaped = false;

        while (position < query.Length)
        {
            var ch = query[position];

            if (escaped)
            {
                sb.Append(ch);
                escaped = false;
                position++;
                column++;
                continue;
            }

            if (ch == '\\')
            {
                escaped = true;
                position++;
                column++;
                continue;
            }

            if (ch == '/')
            {
                position++;
                column++;
                break;
            }

            if (ch == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }

            sb.Append(ch);
            position++;
        }

        // Read optional flags
        var flags = string.Empty;
        while (position < query.Length && char.IsLetter(query[position]))
        {
            flags += query[position];
            position++;
            column++;
        }

        var pattern = sb.ToString();
        return new MqlToken(MqlTokenType.Regex, $"/{pattern}/{flags}", startPosition, position, startLine, startColumn);
    }

    private static MqlToken ReadOperator(
        string query,
        ref int position,
        ref int line,
        ref int column,
        int startPosition,
        int startLine,
        int startColumn)
    {
        // Start from the colon
        var sb = new System.Text.StringBuilder();
        sb.Append(':');

        position++; // Skip ':'
        column++;

        // Check for 2-character operators
        if (position < query.Length)
        {
            var next = query[position];
            if (next == '=' || next == '!' || next == '<' || next == '>')
            {
                sb.Append(next);
                position++;
                column++;

                // Check for <=, >=, or !=
                if (position < query.Length && query[position] == '=')
                {
                    sb.Append('=');
                    position++;
                    column++;
                }
            }
        }

        var op = sb.ToString();
        return new MqlToken(MqlTokenType.Operator, op, startPosition, position, startLine, startColumn);
    }

    private static string ReadIdentifier(
        string query,
        ref int position,
        ref int line,
        ref int column)
    {
        var start = position;

        while (position < query.Length)
        {
            var ch = query[position];

            // Stop at whitespace, special characters, or operators
            if (char.IsWhiteSpace(ch) ||
                ch == '(' || ch == ')' ||
                ch == ':' ||
                ch == '"' ||
                ch == '/')
            {
                break;
            }

            position++;
            column++;

            // Stop at range dash if surrounded by numbers
            if (ch == '-' && position < query.Length && char.IsDigit(query[position]))
            {
                break;
            }
        }

        return query.Substring(start, position - start);
    }

    private static MqlToken ReadNumberOrDate(
        string query,
        ref int position,
        ref int line,
        ref int column,
        int startPosition,
        int startLine,
        int startColumn)
    {
        var sb = new System.Text.StringBuilder();

        // Handle negative sign
        if (query[position] == '-')
        {
            sb.Append('-');
            position++;
            column++;
        }

        var hasDecimal = false;
        var hasDigit = false;

        while (position < query.Length)
        {
            var ch = query[position];

            if (char.IsDigit(ch))
            {
                sb.Append(ch);
                hasDigit = true;
                position++;
                column++;
                continue;
            }

            if (ch == '.' && !hasDecimal && hasDigit)
            {
                sb.Append(ch);
                hasDecimal = true;
                position++;
                column++;
                continue;
            }

            // Handle range dash (e.g., 1970-1980) but not YYYY-MM-DD dates
            if (ch == '-' && hasDigit)
            {
                // Check if this looks like a date (YYYY-MM-DD) by looking ahead
                var afterDashPos = position + 1;
                var digitsAfterDash = 0;

                while (afterDashPos < query.Length && char.IsDigit(query[afterDashPos]))
                {
                    digitsAfterDash++;
                    afterDashPos++;
                }

                // If we have 2 digits after the dash, it might be a date (MM part of YYYY-MM-DD)
                if (digitsAfterDash == 2)
                {
                    // Check if there's another dash after those 2 digits (DD part)
                    var secondDashPos = position + 1 + digitsAfterDash;
                    if (secondDashPos < query.Length && query[secondDashPos] == '-')
                    {
                        // This is likely a date YYYY-MM-DD, not a range
                        // Append everything up to and including the second dash
                        sb.Append(ch); // first dash
                        position++;
                        column++;

                        // Append MM
                        for (var i = 0; i < 2; i++)
                        {
                            sb.Append(query[position]);
                            position++;
                            column++;
                        }

                        // Append second dash
                        sb.Append(query[position]); // second dash
                        position++;
                        column++;

                        // Read the final 2 digits (DD)
                        for (var i = 0; i < 2 && position < query.Length && char.IsDigit(query[position]); i++)
                        {
                            sb.Append(query[position]);
                            position++;
                            column++;
                        }

                        var dateValue = sb.ToString();
                        return new MqlToken(MqlTokenType.DateLiteral, dateValue, startPosition, position, startLine, startColumn);
                    }
                }

                // This is likely a range (e.g., 1970-1980)
                sb.Append(ch);
                position++;
                column++;

                // Continue reading the range value
                while (position < query.Length)
                {
                    var rangeCh = query[position];
                    if (char.IsDigit(rangeCh))
                    {
                        sb.Append(rangeCh);
                        position++;
                        column++;
                        continue;
                    }

                    // Range dash followed by non-digit ends the range
                    break;
                }

                var rangeValue = sb.ToString();
                return new MqlToken(MqlTokenType.Range, rangeValue, startPosition, position, startLine, startColumn);
            }

            // Check for relative date suffix (d, w, h)
            if (ch is 'd' or 'w' or 'h' && hasDigit)
            {
                sb.Append(ch);
                position++;
                column++;
                break;
            }

            break;
        }

        var value = sb.ToString();
        var tokenType = MqlTokenType.NumberLiteral;

        // Check if this looks like a date (YYYY-MM-DD format)
        if (value.Length == 10 && value[4] == '-' && value[7] == '-')
        {
            tokenType = MqlTokenType.DateLiteral;
        }
        // Check if it's a relative date (-7d, -3w, -12h)
        else if (value.Length > 2 && value.StartsWith('-') && char.IsDigit(value[1]) &&
                 (value[^1] == 'd' || value[^1] == 'w' || value[^1] == 'h'))
        {
            tokenType = MqlTokenType.DateLiteral;
        }

        return new MqlToken(tokenType, value, startPosition, position, startLine, startColumn);
    }

    private static void UpdatePosition(char ch, ref int line, ref int column, ref int position)
    {
        if (ch == '\n')
        {
            line++;
            column = 1;
        }
        else
        {
            column++;
        }
        position++;
    }

    private static MqlTokenType DetermineTokenType(string value, string query, int currentPosition)
    {
        // Check if followed by colon (indicates field name)
        var nextNonWs = SkipWhitespace(query, currentPosition);
        if (nextNonWs < query.Length && query[nextNonWs] == ':')
        {
            return MqlTokenType.FieldName;
        }

        // Check for boolean literals
        if (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            return MqlTokenType.BooleanLiteral;
        }

        // Check for date keywords
        if (DateKeywords.Contains(value))
        {
            return MqlTokenType.DateLiteral;
        }

        // Check for boolean keywords
        if (BooleanKeywords.Contains(value))
        {
            return value.ToUpperInvariant() switch
            {
                "AND" => MqlTokenType.And,
                "OR" => MqlTokenType.Or,
                "NOT" => MqlTokenType.Not,
                _ => MqlTokenType.Unknown
            };
        }

        // Check for range (contains a dash not at the start)
        if (value.Contains('-') && !value.StartsWith('-'))
        {
            return MqlTokenType.Range;
        }

        // Check if it looks like a number or date
        if (IsNumericOrDate(value))
        {
            return IsDateLiteral(value) ? MqlTokenType.DateLiteral : MqlTokenType.NumberLiteral;
        }

        // Default to free text
        return MqlTokenType.FreeText;
    }

    private static int SkipWhitespace(string query, int position)
    {
        while (position < query.Length && char.IsWhiteSpace(query[position]))
        {
            position++;
        }
        return position;
    }

    private static bool IsNumericOrDate(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        var start = 0;
        if (value[0] == '-')
        {
            start = 1;
            if (value.Length == 1)
                return false;
        }

        var hasDecimal = false;
        var hasDigit = false;

        for (var i = start; i < value.Length; i++)
        {
            var ch = value[i];
            if (char.IsDigit(ch))
            {
                hasDigit = true;
                continue;
            }

            if (ch == '.' && !hasDecimal && hasDigit)
            {
                hasDecimal = true;
                continue;
            }

            // Allow date format YYYY-MM-DD (first dash after 4 digits)
            if (ch == '-' && hasDigit && i == 4)
            {
                continue;
            }

            // Allow date format YYYY-MM-DD (second dash after 7 chars)
            if (ch == '-' && hasDigit && i == 7)
            {
                continue;
            }

            // Allow relative date suffixes
            if (ch is 'd' or 'w' or 'h' && hasDigit)
            {
                continue;
            }

            return false;
        }

        return hasDigit;
    }

    private static bool IsDateLiteral(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        // Check for relative date format (-7d, -3w, -12h) - must have d/w/h suffix
        if (value.Length > 2 && value[0] == '-' && char.IsDigit(value[1]))
        {
            return value[^1] is 'd' or 'w' or 'h';
        }

        // Check for absolute date format (YYYY-MM-DD)
        if (value.Length == 10 && value[4] == '-' && value[7] == '-')
        {
            return int.TryParse(value.Substring(0, 4), out var year) &&
                   year >= 1800 && year <= 2100;
        }

        // Check for date keywords
        return DateKeywords.Contains(value);
    }
}

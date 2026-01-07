using System.Text.RegularExpressions;

namespace Melodee.Mql.Security;

/// <summary>
/// Provides text sanitization and dangerous pattern detection for MQL queries.
/// </summary>
public static class MqlTextSanitizer
{
    private static readonly HashSet<char> SpecialCharacters = new()
    {
        '\'', '"', '\\', ';', '-', '(', ')', '[', ']', '{', '}', '|', '*', '?', '.', '+', '^', '$', '<', '>', '#', '&', '%', '~', '`'
    };

    private static readonly HashSet<char> ControlCharacters = new()
    {
        '\n', '\r', '\t', '\0', '\v', '\f'
    };

    private static readonly string[] DangerousPatterns = new[]
    {
        "--",
        "; DROP",
        "; DELETE",
        "; UPDATE",
        "; INSERT",
        "UNION SELECT",
        "EXEC(",
        "xp_",
        "0x",
        "CAST(",
        "CONVERT(",
        "--+",
        "/\\*",
        "\\*/",
        "NVL(",
        "DECODE(",
        "CHR(",
        "ASCII(",
        "LENGTH(",
        "@@",
        "@@version",
        "LOAD_FILE",
        "INTO OUTFILE",
        "INTO DUMPFILE",
        "BENCHMARK(",
        "SLEEP(",
        "PG_SLEEP",
        "WAITFOR DELAY",
        "sp_",
        "xp_",
        "xp_cmdshell",
        "execxp",
        "||",
        "concat(",
        "char(",
        "substring(",
        "mid(",
        "len(",
        "datalength(",
        "hex(",
        "unhex(",
        "md5(",
        "sha1("
    };

    private static readonly Regex ValidFreeTextPattern = new(@"^[a-zA-Z0-9\s\-_.,!?()]+$", RegexOptions.Compiled);

    private static readonly Regex ValidFieldNamePattern = new(@"^[a-zA-Z][a-zA-Z0-9]*$", RegexOptions.Compiled);

    private static readonly Regex ValidNumericPattern = new(@"^-?\d+(\.\d+)?$", RegexOptions.Compiled);

    private static readonly Regex ValidDatePattern = new(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.Compiled);

    /// <summary>
    /// Sanitizes a string for use in free text search.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <returns>The sanitized string with dangerous characters escaped.</returns>
    public static string SanitizeForFreeText(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var result = new System.Text.StringBuilder(input.Length);

        foreach (var c in input)
        {
            if (SpecialCharacters.Contains(c))
            {
                result.Append('\\');
                result.Append(c);
            }
            else if (ControlCharacters.Contains(c))
            {
                result.Append(' ');
            }
            else if (char.IsControl(c))
            {
                result.Append(' ');
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Sanitizes a string for use in regex patterns.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <returns>The sanitized string safe for regex use.</returns>
    public static string SanitizeForRegex(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var result = new System.Text.StringBuilder(input.Length);

        foreach (var c in input)
        {
            if (IsRegexMetaCharacter(c))
            {
                result.Append('\\');
                result.Append(c);
            }
            else if (char.IsControl(c))
            {
                result.Append(' ');
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Checks if the input contains dangerous SQL injection patterns.
    /// </summary>
    /// <param name="input">The input to check.</param>
    /// <returns>True if dangerous patterns are found; otherwise, false.</returns>
    public static bool ContainsDangerousPatterns(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        var normalizedInput = input.ToUpperInvariant();

        foreach (var pattern in DangerousPatterns)
        {
            if (normalizedInput.Contains(pattern.ToUpperInvariant()))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a regex pattern could cause ReDoS (Regular Expression Denial of Service).
    /// </summary>
    /// <param name="pattern">The regex pattern to check.</param>
    /// <returns>True if the pattern is potentially dangerous; otherwise, false.</returns>
    public static bool ContainsRedosPattern(string pattern)
    {
        if (string.IsNullOrEmpty(pattern) || pattern.Length > 100)
        {
            return true;
        }

        var upperPattern = pattern.ToUpperInvariant();

        if (upperPattern.Contains("(A+)+") ||
            upperPattern.Contains("(A*)*") ||
            upperPattern.Contains("(.+)+") ||
            upperPattern.Contains("(.*)*") ||
            upperPattern.Contains("(\\W+)+") ||
            upperPattern.Contains("(\\D+)+") ||
            upperPattern.Contains("(\\S+)+") ||
            upperPattern.Contains("(\\W*)+\\+") ||
            upperPattern.Contains("(\\D*)+\\+") ||
            upperPattern.Contains("(\\S*)+\\+") ||
            upperPattern.Contains("(X+X+)+Y") ||
            upperPattern.Contains("((?:A|B)+)+") ||
            upperPattern.Contains("(A+|B+)*C") ||
            upperPattern.Contains("(A{2,})+") ||
            upperPattern.Contains("(.?)+") ||
            upperPattern.Contains("(.?)*"))
        {
            return true;
        }

        if (CountNestedQuantifiers(pattern) > 2)
        {
            return true;
        }

        if (HasExponentialBacktracking(pattern))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Validates that a string contains only valid free text characters.
    /// </summary>
    /// <param name="input">The input to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValidFreeText(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return true;
        }

        return ValidFreeTextPattern.IsMatch(input);
    }

    /// <summary>
    /// Validates that a string is a valid field name.
    /// </summary>
    /// <param name="fieldName">The field name to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValidFieldName(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
        {
            return false;
        }

        return ValidFieldNamePattern.IsMatch(fieldName);
    }

    /// <summary>
    /// Validates that a string is a valid numeric value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValidNumeric(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return ValidNumericPattern.IsMatch(value);
    }

    /// <summary>
    /// Validates that a string is a valid date value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValidDate(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return ValidDatePattern.IsMatch(value);
    }

    /// <summary>
    /// Removes null bytes and other dangerous control characters from input.
    /// </summary>
    /// <param name="input">The input to clean.</param>
    /// <returns>The cleaned string.</returns>
    public static string RemoveDangerousCharacters(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return new string(input.Where(c => !char.IsControl(c) || char.IsWhiteSpace(c)).ToArray());
    }

    /// <summary>
    /// Checks if a character is a regex meta-character that needs escaping.
    /// </summary>
    private static bool IsRegexMetaCharacter(char c)
    {
        return c is '\\' or '^' or '$' or '.' or '|' or '?' or '*' or '+' or '(' or ')' or '[' or ']' or '{' or '}' or '-' or '/' or '"' or '`' or '\'';
    }

    /// <summary>
    /// Counts the number of nested quantifiers in a pattern.
    /// </summary>
    private static int CountNestedQuantifiers(string pattern)
    {
        var maxNesting = 0;
        var currentNesting = 0;

        foreach (var c in pattern)
        {
            if (c == '(')
            {
                currentNesting++;
                maxNesting = Math.Max(maxNesting, currentNesting);
            }
            else if (c == ')')
            {
                currentNesting = Math.Max(0, currentNesting - 1);
            }
            else if (c is '*' or '+' or '?' or '{')
            {
                if (currentNesting > 0)
                {
                    maxNesting = Math.Max(maxNesting, currentNesting);
                }
            }
        }

        return maxNesting;
    }

    /// <summary>
    /// Checks if a pattern has potential for exponential backtracking.
    /// </summary>
    private static bool HasExponentialBacktracking(string pattern)
    {
        var hasAmbiguousRepeats = false;

        for (var i = 0; i < pattern.Length - 1; i++)
        {
            var current = pattern[i];
            var next = pattern[i + 1];

            if ((current == '.' || current == '\\' || char.IsLetterOrDigit(current)) &&
                (next == '+' || next == '*' || next == '?'))
            {
                if (i < pattern.Length - 2)
                {
                    var following = pattern[i + 2];
                    if (following == '(' || following == '.' || char.IsLetterOrDigit(following))
                    {
                        hasAmbiguousRepeats = true;
                    }
                }
            }

            if (current == ')' && (next == '+' || next == '*' || next == '?'))
            {
                if (i < pattern.Length - 2)
                {
                    var following = pattern[i + 2];
                    if (following == '(' || following == '.' || char.IsLetterOrDigit(following))
                    {
                        hasAmbiguousRepeats = true;
                    }
                }
            }
        }

        return hasAmbiguousRepeats;
    }
}

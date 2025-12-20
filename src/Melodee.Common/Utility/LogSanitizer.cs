namespace Melodee.Common.Utility;

/// <summary>
/// Provides methods to sanitize user-controlled input before logging to prevent log forging attacks.
/// Log forging occurs when an attacker can inject newline characters into log files, 
/// potentially creating fake log entries or corrupting log file structure.
/// </summary>
public static class LogSanitizer
{
    /// <summary>
    /// Sanitizes a string for safe logging by replacing newline and carriage return characters.
    /// This prevents log forging attacks where attackers inject newlines to create fake log entries.
    /// </summary>
    /// <param name="input">The user-controlled input to sanitize</param>
    /// <returns>A sanitized string safe for logging</returns>
    public static string? Sanitize(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return input
            .Replace("\r", "[CR]")
            .Replace("\n", "[LF]")
            .Replace("\u0085", "[NEL]")      // Next Line
            .Replace("\u2028", "[LS]")       // Line Separator
            .Replace("\u2029", "[PS]");      // Paragraph Separator
    }

    /// <summary>
    /// Sanitizes an object for safe logging. If the object is a string, it will be sanitized.
    /// For other types, returns the object as-is (relies on structured logging).
    /// </summary>
    /// <param name="input">The input to sanitize</param>
    /// <returns>A sanitized value safe for logging</returns>
    public static object? SanitizeObject(object? input)
    {
        if (input is string str)
        {
            return Sanitize(str);
        }
        return input;
    }

    /// <summary>
    /// Masks sensitive data (like email addresses) for logging.
    /// Shows first 2 characters and domain but masks the rest.
    /// </summary>
    /// <param name="email">The email to mask</param>
    /// <returns>A masked email safe for logging (e.g., "jo***@example.com")</returns>
    public static string? MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return email;
        }

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
        {
            return "***";
        }

        var localPart = email[..atIndex];
        var domain = email[atIndex..];
        
        if (localPart.Length <= 2)
        {
            return $"{localPart[0]}***{domain}";
        }

        return $"{localPart[..2]}***{domain}";
    }

    /// <summary>
    /// Masks any identifier for logging purposes.
    /// Shows first few characters but masks the rest.
    /// </summary>
    /// <param name="identifier">The identifier to mask</param>
    /// <param name="visibleChars">Number of characters to show at start</param>
    /// <returns>A masked identifier safe for logging</returns>
    public static string? MaskIdentifier(string? identifier, int visibleChars = 4)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return identifier;
        }

        if (identifier.Length <= visibleChars)
        {
            return "***";
        }

        return $"{identifier[..visibleChars]}***";
    }
}

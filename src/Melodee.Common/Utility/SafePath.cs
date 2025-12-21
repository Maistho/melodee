namespace Melodee.Common.Utility;

/// <summary>
/// Security helper for validating and resolving file paths safely.
/// Prevents path traversal attacks by ensuring resolved paths stay within a base directory.
/// </summary>
public static class SafePath
{
    /// <summary>
    /// Resolves a file path ensuring it stays within the specified base directory.
    /// Prevents path traversal attacks using ".." or absolute paths.
    /// </summary>
    /// <param name="baseDirectory">The root directory that the resolved path must stay within.</param>
    /// <param name="relativePath">The relative path or filename provided (potentially from user input).</param>
    /// <returns>The full resolved path if valid, or null if the path would escape the base directory.</returns>
    public static string? ResolveUnderRoot(string baseDirectory, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        // Sanitize the filename to remove path separators and invalid characters
        var sanitizedFileName = SanitizeFileName(relativePath);
        if (string.IsNullOrWhiteSpace(sanitizedFileName))
        {
            return null;
        }

        // Get the full path of the base directory (normalize it)
        var baseFullPath = Path.GetFullPath(baseDirectory);

        // Ensure base directory ends with a directory separator to prevent prefix attacks
        // e.g., /safe/path could match /safe/pathevil without this
        if (!baseFullPath.EndsWith(Path.DirectorySeparatorChar))
        {
            baseFullPath += Path.DirectorySeparatorChar;
        }

        // Combine using the normalized base path (without trailing separator for Path.Combine)
        var baseForCombine = baseFullPath.TrimEnd(Path.DirectorySeparatorChar);
        var combinedPath = Path.Combine(baseForCombine, sanitizedFileName);
        var resolvedPath = Path.GetFullPath(combinedPath);

        // Verify the resolved path is still under the base directory
        if (!resolvedPath.StartsWith(baseFullPath, StringComparison.OrdinalIgnoreCase))
        {
            // Path traversal attempt detected - resolved path escaped base directory
            return null;
        }

        return resolvedPath;
    }

    /// <summary>
    /// Checks if a resolved path is safely contained within a base directory.
    /// </summary>
    /// <param name="baseDirectory">The root directory to validate against.</param>
    /// <param name="pathToCheck">The path to verify.</param>
    /// <returns>True if the path is within the base directory, false otherwise.</returns>
    public static bool IsPathWithinBase(string baseDirectory, string pathToCheck)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory) || string.IsNullOrWhiteSpace(pathToCheck))
        {
            return false;
        }

        var baseFullPath = Path.GetFullPath(baseDirectory);
        if (!baseFullPath.EndsWith(Path.DirectorySeparatorChar))
        {
            baseFullPath += Path.DirectorySeparatorChar;
        }

        var resolvedPath = Path.GetFullPath(pathToCheck);

        return resolvedPath.StartsWith(baseFullPath, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Sanitizes a filename by removing path separators and dangerous characters.
    /// This extracts just the filename component and removes any path traversal sequences.
    /// </summary>
    /// <param name="fileName">The filename to sanitize (may contain malicious path elements).</param>
    /// <returns>A sanitized filename with only the base name, or null if invalid.</returns>
    public static string? SanitizeFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        // Get just the filename, removing any directory components
        // This handles both forward and backward slashes
        var name = Path.GetFileName(fileName);

        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        // Block ".." sequences even if they somehow survived GetFileName
        if (name.Contains(".."))
        {
            return null;
        }

        // Use existing PathSanitizer to clean invalid characters
        var sanitized = PathSanitizer.SanitizeFilename(name, '_');

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return null;
        }

        // Ensure the result is not empty after sanitization
        return sanitized.Trim();
    }
}

using System.Text;
using System.Web;
using Serilog;

namespace Melodee.Common.Services.Parsing;

/// <summary>
/// Parser for M3U and M3U8 playlist files
/// </summary>
public sealed class M3UParser
{
    private readonly ILogger _logger;

    public M3UParser(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parse an M3U/M3U8 file and return a list of normalized entry references
    /// </summary>
    public async Task<M3UParseResult> ParseAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var isM3U8 = fileName.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase);
        var encoding = isM3U8 ? Encoding.UTF8 : DetectEncoding(fileStream);

        fileStream.Position = 0;

        var entries = new List<M3UEntry>();
        var lineNumber = 0;

        using var reader = new StreamReader(fileStream, encoding, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null && !cancellationToken.IsCancellationRequested)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            line = line.Trim();

            // Skip comments (including #EXTM3U and #EXTINF)
            if (line.StartsWith('#'))
            {
                continue;
            }

            try
            {
                var entry = NormalizeEntry(line, lineNumber);
                entries.Add(entry);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to parse line {LineNumber} in {FileName}: {Line}", lineNumber, fileName, line);
                // Add as missing entry with raw reference
                entries.Add(new M3UEntry
                {
                    RawReference = line,
                    NormalizedReference = line,
                    SortOrder = lineNumber,
                    ParseError = ex.Message
                });
            }
        }

        return new M3UParseResult
        {
            Entries = entries,
            TotalLines = lineNumber,
            FileName = fileName,
            Encoding = encoding.WebName
        };
    }

    private static Encoding DetectEncoding(Stream stream)
    {
        var buffer = new byte[4];
        var bytesRead = stream.Read(buffer, 0, 4);
        stream.Position = 0;

        if (bytesRead >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
        {
            return Encoding.UTF8;
        }

        if (bytesRead >= 2 && buffer[0] == 0xFF && buffer[1] == 0xFE)
        {
            return Encoding.Unicode;
        }

        if (bytesRead >= 2 && buffer[0] == 0xFE && buffer[1] == 0xFF)
        {
            return Encoding.BigEndianUnicode;
        }

        // Default to UTF-8 for M3U files
        return Encoding.UTF8;
    }

    private static M3UEntry NormalizeEntry(string rawLine, int sortOrder)
    {
        var normalized = rawLine;

        // Remove surrounding quotes if present
        if ((normalized.StartsWith('"') && normalized.EndsWith('"')) ||
            (normalized.StartsWith('\'') && normalized.EndsWith('\'')))
        {
            normalized = normalized[1..^1];
        }

        // URL decode (handle %xx sequences)
        normalized = SafeUrlDecode(normalized);

        // Normalize path separators (convert backslashes to forward slashes)
        normalized = normalized.Replace('\\', '/');

        // Extract path hints (artist folder, album folder, filename)
        var hints = ExtractPathHints(normalized);

        return new M3UEntry
        {
            RawReference = rawLine,
            NormalizedReference = normalized,
            SortOrder = sortOrder,
            FileName = hints.FileName,
            ArtistFolder = hints.ArtistFolder,
            AlbumFolder = hints.AlbumFolder
        };
    }

    private static string SafeUrlDecode(string input)
    {
        try
        {
            // Only decode if it contains % characters
            if (!input.Contains('%'))
            {
                return input;
            }

            // Use HttpUtility.UrlDecode which handles literal % safely
            var decoded = HttpUtility.UrlDecode(input);
            return decoded ?? input;
        }
        catch
        {
            // If decoding fails, return original
            return input;
        }
    }

    private static (string? FileName, string? ArtistFolder, string? AlbumFolder) ExtractPathHints(string normalizedPath)
    {
        try
        {
            // Remove any protocol/scheme (http://, file://, etc.)
            if (normalizedPath.Contains("://"))
            {
                return (normalizedPath, null, null);
            }

            // Split path into segments
            var segments = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0)
            {
                return (normalizedPath, null, null);
            }

            var fileName = segments[^1];
            string? albumFolder = segments.Length >= 2 ? segments[^2] : null;
            string? artistFolder = segments.Length >= 3 ? segments[^3] : null;

            return (fileName, artistFolder, albumFolder);
        }
        catch
        {
            return (normalizedPath, null, null);
        }
    }
}

public sealed class M3UParseResult
{
    public required List<M3UEntry> Entries { get; init; }
    public required int TotalLines { get; init; }
    public required string FileName { get; init; }
    public required string Encoding { get; init; }
}

public sealed class M3UEntry
{
    public required string RawReference { get; init; }
    public required string NormalizedReference { get; init; }
    public required int SortOrder { get; init; }
    public string? FileName { get; init; }
    public string? ArtistFolder { get; init; }
    public string? AlbumFolder { get; init; }
    public string? ParseError { get; init; }
}

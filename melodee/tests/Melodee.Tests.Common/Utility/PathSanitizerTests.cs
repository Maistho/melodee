using Melodee.Common.Utility;

namespace Melodee.Tests.Common.Utility;

public class PathSanitizerTests
{
    [Fact]
    public void SanitizeFilename_NullInput_ReturnsNull()
    {
        Assert.Null(PathSanitizer.SanitizeFilename(null, '_'));
    }

    [Fact]
    public void SanitizeFilename_ValidFilename_ReturnsSame()
    {
        var result = PathSanitizer.SanitizeFilename("valid_filename.txt", '_');
        Assert.Equal("valid_filename.txt", result);
    }

    [Theory]
    [InlineData("file<name>.txt", "file_name_.txt")]
    [InlineData("file>name.txt", "file_name.txt")]
    [InlineData("file:name.txt", "file_name.txt")]
    [InlineData("file\"name.txt", "file_name.txt")]
    [InlineData("file/name.txt", "file_name.txt")]
    [InlineData("file\\name.txt", "file_name.txt")]
    [InlineData("file|name.txt", "file_name.txt")]
    [InlineData("file?name.txt", "file_name.txt")]
    [InlineData("file*name.txt", "file_name.txt")]
    public void SanitizeFilename_InvalidCharacters_ReplacesWithErrorChar(string input, string expected)
    {
        var result = PathSanitizer.SanitizeFilename(input, '_');
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitizeFilename_CustomErrorChar_UsesCustomChar()
    {
        var result = PathSanitizer.SanitizeFilename("file<name>.txt", '-');
        Assert.Equal("file-name-.txt", result);
    }

    [Fact]
    public void SanitizeFilename_NonAsciiCharacters_RemovesThem()
    {
        var result = PathSanitizer.SanitizeFilename("café.txt", '_');
        // Non-ASCII characters (>127) are removed
        Assert.Equal("caf.txt", result);
    }

    [Fact]
    public void SanitizeFilename_ControlCharacters_RemovesThem()
    {
        var result = PathSanitizer.SanitizeFilename("file\x01name.txt", '_');
        // Control characters (<32) are removed entirely by ReturnCleanAscii
        Assert.Equal("filename.txt", result);
    }

    [Fact]
    public void SanitizePath_NullInput_ReturnsNull()
    {
        Assert.Null(PathSanitizer.SanitizePath(null, '_'));
    }

    [Fact]
    public void SanitizePath_ValidPath_ReturnsSame()
    {
        var result = PathSanitizer.SanitizePath("valid_path", '_');
        Assert.Equal("valid_path", result);
    }

    [Theory]
    [InlineData("path<name>", "path_name_")]
    [InlineData("path|name", "path_name")]
    [InlineData("path\"name", "path_name")]
    public void SanitizePath_InvalidCharacters_ReplacesWithErrorChar(string input, string expected)
    {
        var result = PathSanitizer.SanitizePath(input, '_');
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitizeFilename_PercentCharacter_IsRemoved()
    {
        var result = PathSanitizer.SanitizeFilename("file%20name.txt", '_');
        Assert.Equal("file20name.txt", result);
    }

    [Fact]
    public void SanitizeFilename_EmptyString_ReturnsEmpty()
    {
        var result = PathSanitizer.SanitizeFilename("", '_');
        Assert.Equal("", result);
    }
}

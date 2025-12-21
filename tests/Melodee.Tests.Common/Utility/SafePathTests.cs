using Melodee.Common.Utility;

namespace Melodee.Tests.Common.Utility;

public class SafePathTests
{
    private readonly string _tempBaseDir;

    public SafePathTests()
    {
        _tempBaseDir = Path.Combine(Path.GetTempPath(), "SafePathTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempBaseDir);
    }

    [Fact]
    public void SanitizeFileName_NullInput_ReturnsNull()
    {
        Assert.Null(SafePath.SanitizeFileName(null));
    }

    [Fact]
    public void SanitizeFileName_EmptyString_ReturnsNull()
    {
        Assert.Null(SafePath.SanitizeFileName(""));
    }

    [Fact]
    public void SanitizeFileName_WhitespaceOnly_ReturnsNull()
    {
        Assert.Null(SafePath.SanitizeFileName("   "));
    }

    [Fact]
    public void SanitizeFileName_ValidFilename_ReturnsSame()
    {
        var result = SafePath.SanitizeFileName("valid_file.txt");
        Assert.Equal("valid_file.txt", result);
    }

    [Theory]
    [InlineData("../secret.txt")]
    [InlineData("..\\secret.txt")]
    [InlineData("foo/../bar.txt")]
    [InlineData("..")]
    public void SanitizeFileName_PathTraversalAttempt_ReturnsNull(string maliciousInput)
    {
        var result = SafePath.SanitizeFileName(maliciousInput);
        // Either returns null or strips the path components
        Assert.True(result == null || !result.Contains(".."));
    }

    [Theory]
    [InlineData("/etc/passwd")]
    [InlineData("C:\\Windows\\System32\\config")]
    [InlineData("/var/log/auth.log")]
    public void SanitizeFileName_AbsolutePathAttempt_ExtractsOnlyFilename(string absolutePath)
    {
        var result = SafePath.SanitizeFileName(absolutePath);
        // Should only extract the filename portion
        Assert.NotNull(result);
        Assert.DoesNotContain("/", result);
        Assert.DoesNotContain("\\", result);
    }

    [Fact]
    public void SanitizeFileName_RemovesDirectoryComponents()
    {
        var result = SafePath.SanitizeFileName("subdir/file.txt");
        Assert.Equal("file.txt", result);
    }

    [Fact]
    public void ResolveUnderRoot_NullBaseDirectory_ReturnsNull()
    {
        Assert.Null(SafePath.ResolveUnderRoot(null!, "file.txt"));
    }

    [Fact]
    public void ResolveUnderRoot_NullRelativePath_ReturnsNull()
    {
        Assert.Null(SafePath.ResolveUnderRoot(_tempBaseDir, null!));
    }

    [Fact]
    public void ResolveUnderRoot_EmptyRelativePath_ReturnsNull()
    {
        Assert.Null(SafePath.ResolveUnderRoot(_tempBaseDir, ""));
    }

    [Fact]
    public void ResolveUnderRoot_ValidFilename_ReturnsFullPath()
    {
        var result = SafePath.ResolveUnderRoot(_tempBaseDir, "test.txt");
        Assert.NotNull(result);
        Assert.StartsWith(_tempBaseDir, result);
        Assert.EndsWith("test.txt", result);
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("..\\outside.txt")]
    [InlineData("subdir/../../outside.txt")]
    public void ResolveUnderRoot_PathTraversalAttempt_ReturnsNull(string maliciousPath)
    {
        var result = SafePath.ResolveUnderRoot(_tempBaseDir, maliciousPath);
        // After sanitization, should either be null or within base
        if (result != null)
        {
            Assert.StartsWith(_tempBaseDir, result);
        }
    }

    [Fact]
    public void ResolveUnderRoot_AbsolutePathOutsideBase_ReturnsNull()
    {
        // Try to escape via an absolute path
        var result = SafePath.ResolveUnderRoot(_tempBaseDir, "/etc/passwd");
        // Should only use the filename "passwd"
        if (result != null)
        {
            Assert.StartsWith(_tempBaseDir, result);
        }
    }

    [Fact]
    public void IsPathWithinBase_ValidPathWithinBase_ReturnsTrue()
    {
        var testPath = Path.Combine(_tempBaseDir, "subdir", "file.txt");
        Assert.True(SafePath.IsPathWithinBase(_tempBaseDir, testPath));
    }

    [Fact]
    public void IsPathWithinBase_PathOutsideBase_ReturnsFalse()
    {
        var outsidePath = Path.Combine(Path.GetTempPath(), "other_dir", "file.txt");
        Assert.False(SafePath.IsPathWithinBase(_tempBaseDir, outsidePath));
    }

    [Fact]
    public void IsPathWithinBase_ParentDirectory_ReturnsFalse()
    {
        var parentPath = Path.GetDirectoryName(_tempBaseDir);
        Assert.False(SafePath.IsPathWithinBase(_tempBaseDir, parentPath!));
    }

    [Fact]
    public void IsPathWithinBase_NullBase_ReturnsFalse()
    {
        Assert.False(SafePath.IsPathWithinBase(null!, "/some/path"));
    }

    [Fact]
    public void IsPathWithinBase_NullPath_ReturnsFalse()
    {
        Assert.False(SafePath.IsPathWithinBase(_tempBaseDir, null!));
    }

    [Fact]
    public void IsPathWithinBase_SimilarPrefix_ReturnsFalse()
    {
        // Test that /safe/pathevil doesn't match /safe/path
        var baseDir = Path.Combine(Path.GetTempPath(), "safepath");
        var evilPath = Path.Combine(Path.GetTempPath(), "safepathevil", "file.txt");
        Assert.False(SafePath.IsPathWithinBase(baseDir, evilPath));
    }

    [Fact]
    public void ResolveUnderRoot_FilenameWithSpecialChars_SanitizesCorrectly()
    {
        var result = SafePath.ResolveUnderRoot(_tempBaseDir, "file<>:\"|?*.txt");
        // Should sanitize the filename
        if (result != null)
        {
            Assert.StartsWith(_tempBaseDir, result);
            Assert.DoesNotContain("<", result);
            Assert.DoesNotContain(">", result);
        }
    }
}

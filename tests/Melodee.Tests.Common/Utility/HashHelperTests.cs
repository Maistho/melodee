using Melodee.Common.Utility;

namespace Melodee.Tests.Common.Utility;

public class HashHelperTests
{
    [Fact]
    public void CreateMd5_NullString_ReturnsNull()
    {
        string? input = null;
        Assert.Null(HashHelper.CreateMd5(input));
    }

    [Fact]
    public void CreateMd5_EmptyString_ReturnsNull()
    {
        Assert.Null(HashHelper.CreateMd5(string.Empty));
    }

    [Fact]
    public void CreateMd5_ValidString_ReturnsHash()
    {
        var result = HashHelper.CreateMd5("test");
        Assert.NotNull(result);
        Assert.Equal(32, result.Length); // MD5 produces 32 hex characters
    }

    [Fact]
    public void CreateMd5_SameInput_ReturnsSameHash()
    {
        var result1 = HashHelper.CreateMd5("hello world");
        var result2 = HashHelper.CreateMd5("hello world");
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void CreateMd5_DifferentInput_ReturnsDifferentHash()
    {
        var result1 = HashHelper.CreateMd5("hello");
        var result2 = HashHelper.CreateMd5("world");
        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public void CreateMd5_KnownValue_ReturnsExpectedHash()
    {
        // MD5 of "test" is 098f6bcd4621d373cade4e832627b4f6
        var result = HashHelper.CreateMd5("test");
        Assert.Equal("098f6bcd4621d373cade4e832627b4f6", result);
    }

    [Fact]
    public void CreateMd5_NullBytes_ReturnsNull()
    {
        byte[]? bytes = null;
        Assert.Null(HashHelper.CreateMd5(bytes));
    }

    [Fact]
    public void CreateMd5_EmptyBytes_ReturnsNull()
    {
        Assert.Null(HashHelper.CreateMd5([]));
    }

    [Fact]
    public void CreateMd5_ValidBytes_ReturnsHash()
    {
        var bytes = new byte[] { 0x74, 0x65, 0x73, 0x74 }; // "test" in bytes
        var result = HashHelper.CreateMd5(bytes);
        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    [Fact]
    public void CreateMd5_FileInfo_WithExistingFile_ReturnsHash()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "test content");
            var fileInfo = new FileInfo(tempFile);
            var result = HashHelper.CreateMd5(fileInfo);
            Assert.NotNull(result);
            Assert.Equal(32, result.Length);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}

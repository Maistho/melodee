using System.Text;
using Melodee.Common.Utility;

namespace Melodee.Tests.Common.Utility;

public class Crc32Tests
{
    [Fact]
    public void Calculate_WithValidByteArray_ReturnsValidHash()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes("test data");

        // Act
        var result = Crc32.Calculate(data);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(8, result.Length); // CRC32 produces 8 hex characters
    }

    [Fact]
    public void Calculate_WithEmptyArray_ReturnsZeroHash()
    {
        // Arrange
        var data = Array.Empty<byte>();

        // Act
        var result = Crc32.Calculate(data);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("00000000", result);
    }

    [Fact]
    public void Calculate_WithNullInput_ThrowsArgumentNullException()
    {
        byte[]? data = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Crc32.Calculate(data!));
    }

    [Fact]
    public void Calculate_WithSameInput_ReturnsSameHash()
    {
        // Arrange
        var inputData = "same test data";
        var data1 = Encoding.UTF8.GetBytes(inputData);
        var data2 = Encoding.UTF8.GetBytes(inputData);

        // Act
        var hash1 = Crc32.Calculate(data1);
        var hash2 = Crc32.Calculate(data2);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Calculate_WithDifferentInputs_ReturnsDifferentHashes()
    {
        // Arrange
        var data1 = Encoding.UTF8.GetBytes("test data 1");
        var data2 = Encoding.UTF8.GetBytes("test data 2");

        // Act
        var hash1 = Crc32.Calculate(data1);
        var hash2 = Crc32.Calculate(data2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }
}

using Melodee.Common.Utility;

namespace Melodee.Tests.Common.Utility;

public class Crc32Tests
{
    [Fact]
    public void ComputeAndCompareCrc32Hash()
    {
        var mp3File = @"/melodee_test/tests/test.mp3";
        var fileInfo = new FileInfo(mp3File);
        if (fileInfo.Exists)
        {
            var crc = Crc32.Calculate(fileInfo);
            Assert.NotNull(crc);

            var crc2 = Crc32.Calculate(fileInfo);
            Assert.NotNull(crc2);
            Assert.Equal(crc, crc2);

            fileInfo.LastWriteTime = fileInfo.LastWriteTime.AddHours(1);
            var crc3 = Crc32.Calculate(fileInfo);
            Assert.NotNull(crc3);
            Assert.Equal(crc, crc3);
        }
    }

    [Fact]
    public void Crc32FromFileMatchesCrc32FromBytes()
    {
        // Regression test for CRC hash discrepancy
        // Validates that CRC32.Calculate(file) equals CRC32.Calculate(fileBytes)
        var mp3File = @"/melodee_test/tests/test.mp3";
        var fileInfo = new FileInfo(mp3File);
        if (fileInfo.Exists)
        {
            // Calculate CRC from file
            var crcFromFile = Crc32.Calculate(fileInfo);
            Assert.NotNull(crcFromFile);

            // Calculate CRC from file bytes
            var fileBytes = File.ReadAllBytes(mp3File);
            var crcFromBytes = Crc32.Calculate(fileBytes);
            Assert.NotNull(crcFromBytes);

            // Both methods should produce the same CRC hash
            // Ground truth: Song.FileHash must always equal CRC32.Calculate over entire file bytes
            Assert.Equal(crcFromFile, crcFromBytes);
        }
    }
}

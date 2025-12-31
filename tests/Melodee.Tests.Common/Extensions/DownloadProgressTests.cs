using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Extensions;

public class DownloadProgressTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var progress = new DownloadProgress(1024, 2048, 512, TimeSpan.FromSeconds(2));

        Assert.Equal(1024, progress.BytesDownloaded);
        Assert.Equal(2048, progress.TotalBytes);
        Assert.Equal(512, progress.BytesPerSecond);
        Assert.Equal(TimeSpan.FromSeconds(2), progress.ElapsedTime);
    }

    [Fact]
    public void PercentComplete_WithKnownTotal_ReturnsCorrectPercentage()
    {
        var progress = new DownloadProgress(500, 1000);

        Assert.Equal(50.0, progress.PercentComplete);
    }

    [Fact]
    public void PercentComplete_WithZeroTotal_ReturnsNull()
    {
        var progress = new DownloadProgress(500, 0);

        Assert.Null(progress.PercentComplete);
    }

    [Fact]
    public void PercentComplete_WithNullTotal_ReturnsNull()
    {
        var progress = new DownloadProgress(500, null);

        Assert.Null(progress.PercentComplete);
    }

    [Fact]
    public void PercentComplete_At100Percent_Returns100()
    {
        var progress = new DownloadProgress(1000, 1000);

        Assert.Equal(100.0, progress.PercentComplete);
    }

    [Theory]
    [InlineData(500, "500.0 B")]
    [InlineData(1024, "1.0 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(1_048_576, "1.0 MB")]
    [InlineData(1_073_741_824, "1.0 GB")]
    [InlineData(3_221_225_472, "3.0 GB")]
    public void BytesDownloadedFormatted_FormatsCorrectly(long bytes, string expected)
    {
        var progress = new DownloadProgress(bytes, null);

        Assert.Equal(expected, progress.BytesDownloadedFormatted);
    }

    [Fact]
    public void TotalBytesFormatted_WithKnownTotal_FormatsCorrectly()
    {
        var progress = new DownloadProgress(0, 1_073_741_824);

        Assert.Equal("1.0 GB", progress.TotalBytesFormatted);
    }

    [Fact]
    public void TotalBytesFormatted_WithNullTotal_ReturnsUnknown()
    {
        var progress = new DownloadProgress(0, null);

        Assert.Equal("Unknown", progress.TotalBytesFormatted);
    }

    [Fact]
    public void SpeedFormatted_WithSpeed_ReturnsFormattedSpeed()
    {
        var progress = new DownloadProgress(0, null, 15_728_640); // 15 MB/s

        Assert.Equal("15.0 MB/s", progress.SpeedFormatted);
    }

    [Fact]
    public void SpeedFormatted_WithNullSpeed_ReturnsNull()
    {
        var progress = new DownloadProgress(0, null);

        Assert.Null(progress.SpeedFormatted);
    }

    [Fact]
    public void EstimatedTimeRemaining_WithValidData_CalculatesCorrectly()
    {
        // 500MB downloaded, 1GB total, 50MB/s speed = 10 seconds remaining
        var progress = new DownloadProgress(524_288_000, 1_073_741_824, 52_428_800);

        var eta = progress.EstimatedTimeRemaining;
        Assert.NotNull(eta);
        Assert.True(eta.Value.TotalSeconds is > 9 and < 11);
    }

    [Fact]
    public void EstimatedTimeRemaining_WithNullTotalBytes_ReturnsNull()
    {
        var progress = new DownloadProgress(500, null, 100);

        Assert.Null(progress.EstimatedTimeRemaining);
    }

    [Fact]
    public void EstimatedTimeRemaining_WithZeroSpeed_ReturnsNull()
    {
        var progress = new DownloadProgress(500, 1000, 0);

        Assert.Null(progress.EstimatedTimeRemaining);
    }

    [Fact]
    public void EstimatedTimeRemaining_WhenComplete_ReturnsZero()
    {
        var progress = new DownloadProgress(1000, 1000, 100);

        Assert.Equal(TimeSpan.Zero, progress.EstimatedTimeRemaining);
    }

    [Fact]
    public void EstimatedTimeRemainingFormatted_WithHours_FormatsCorrectly()
    {
        // 1GB remaining at 100KB/s = ~2.9 hours
        var progress = new DownloadProgress(0, 1_073_741_824, 102_400);

        var eta = progress.EstimatedTimeRemainingFormatted;
        Assert.NotNull(eta);
        Assert.Contains("h", eta);
    }

    [Fact]
    public void EstimatedTimeRemainingFormatted_WithMinutes_FormatsCorrectly()
    {
        // 100MB remaining at 1MB/s = 100 seconds = 1m 40s
        var progress = new DownloadProgress(0, 104_857_600, 1_048_576);

        var eta = progress.EstimatedTimeRemainingFormatted;
        Assert.NotNull(eta);
        Assert.Contains("m", eta);
        Assert.Contains("s", eta);
    }

    [Fact]
    public void EstimatedTimeRemainingFormatted_WithSecondsOnly_FormatsCorrectly()
    {
        // 5MB remaining at 1MB/s = 5 seconds
        var progress = new DownloadProgress(0, 5_242_880, 1_048_576);

        var eta = progress.EstimatedTimeRemainingFormatted;
        Assert.NotNull(eta);
        Assert.EndsWith("s", eta);
        Assert.DoesNotContain("m", eta);
    }

    [Fact]
    public void Record_Equality_WorksCorrectly()
    {
        var progress1 = new DownloadProgress(1024, 2048);
        var progress2 = new DownloadProgress(1024, 2048);

        Assert.Equal(progress1, progress2);
    }

    [Fact]
    public void Record_With_CreatesModifiedCopy()
    {
        var original = new DownloadProgress(1024, 2048);
        var modified = original with { BytesDownloaded = 2048 };

        Assert.Equal(2048, modified.BytesDownloaded);
        Assert.Equal(2048, modified.TotalBytes);
        Assert.Equal(1024, original.BytesDownloaded);
    }

    [Fact]
    public void PercentComplete_WithLargeFile_CalculatesCorrectly()
    {
        // 1.5 GB of 3 GB = 50%
        var progress = new DownloadProgress(1_610_612_736, 3_221_225_472);

        Assert.Equal(50.0, progress.PercentComplete);
    }
}

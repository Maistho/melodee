using Melodee.Common.Jobs;

namespace Melodee.Tests.Common.Jobs;

public class JobStageProgressTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var progress = new JobStageProgress("TestStage", 50, 100, "Item 50");

        Assert.Equal("TestStage", progress.StageName);
        Assert.Equal(50, progress.CurrentItem);
        Assert.Equal(100, progress.TotalItems);
        Assert.Equal("Item 50", progress.CurrentItemDescription);
    }

    [Fact]
    public void Constructor_WithOptionalDescription_DefaultsToNull()
    {
        var progress = new JobStageProgress("TestStage", 50, 100);

        Assert.Null(progress.CurrentItemDescription);
    }

    [Fact]
    public void PercentComplete_WithZeroTotalItems_ReturnsZero()
    {
        var progress = new JobStageProgress("TestStage", 0, 0);

        Assert.Equal(0, progress.PercentComplete);
    }

    [Fact]
    public void PercentComplete_WithProgress_ReturnsCorrectPercentage()
    {
        var progress = new JobStageProgress("TestStage", 50, 100);

        Assert.Equal(50.0, progress.PercentComplete);
    }

    [Fact]
    public void PercentComplete_WithFullProgress_Returns100()
    {
        var progress = new JobStageProgress("TestStage", 100, 100);

        Assert.Equal(100.0, progress.PercentComplete);
    }

    [Fact]
    public void PercentComplete_WithPartialProgress_ReturnsCorrectDecimal()
    {
        var progress = new JobStageProgress("TestStage", 33, 100);

        Assert.Equal(33.0, progress.PercentComplete);
    }

    [Fact]
    public void ToString_WithTotalItems_ReturnsProgressFormat()
    {
        var progress = new JobStageProgress("TestStage", 50, 100);

        var result = progress.ToString();

        Assert.Contains("TestStage", result);
        Assert.Contains("50", result);
        Assert.Contains("100", result);
        Assert.Contains("50.0%", result);
    }

    [Fact]
    public void ToString_WithDescription_IncludesDescription()
    {
        var progress = new JobStageProgress("TestStage", 50, 100, "Processing item X");

        var result = progress.ToString();

        Assert.Contains("Processing item X", result);
    }

    [Fact]
    public void ToString_WithZeroTotalItems_ReturnsIndeterminateFormat()
    {
        var progress = new JobStageProgress("TestStage", 0, 0, "Working...");

        var result = progress.ToString();

        Assert.Contains("TestStage", result);
        Assert.Contains("Working...", result);
    }

    [Fact]
    public void ToString_WithZeroTotalItemsAndNoDescription_ReturnsProcessingMessage()
    {
        var progress = new JobStageProgress("TestStage", 0, 0);

        var result = progress.ToString();

        Assert.Contains("TestStage", result);
        Assert.Contains("Processing...", result);
    }

    [Fact]
    public void Record_Equality_WorksCorrectly()
    {
        var progress1 = new JobStageProgress("Stage", 50, 100, "Desc");
        var progress2 = new JobStageProgress("Stage", 50, 100, "Desc");

        Assert.Equal(progress1, progress2);
    }

    [Fact]
    public void Record_With_CreatesModifiedCopy()
    {
        var original = new JobStageProgress("Stage", 50, 100, "Original");
        var modified = original with { CurrentItem = 75, CurrentItemDescription = "Modified" };

        Assert.Equal("Stage", modified.StageName);
        Assert.Equal(75, modified.CurrentItem);
        Assert.Equal(100, modified.TotalItems);
        Assert.Equal("Modified", modified.CurrentItemDescription);
        Assert.Equal(50, original.CurrentItem);
    }

    [Fact]
    public void PercentComplete_WithLargeNumbers_CalculatesCorrectly()
    {
        var progress = new JobStageProgress("Stage", 1_000_000, 2_000_000);

        Assert.Equal(50.0, progress.PercentComplete);
    }

    [Fact]
    public void PercentComplete_OverflowCase_HandlesCorrectly()
    {
        var progress = new JobStageProgress("Stage", 150, 100);

        Assert.Equal(150.0, progress.PercentComplete);
    }
}

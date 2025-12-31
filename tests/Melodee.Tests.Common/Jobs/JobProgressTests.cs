using Melodee.Common.Jobs;

namespace Melodee.Tests.Common.Jobs;

public class JobProgressTests
{
    [Fact]
    public void Initialize_WithStageNames_SetsStageNames()
    {
        var progress = new JobProgress();

        progress.Initialize("Stage1", "Stage2", "Stage3");

        Assert.Equal(3, progress.TotalStages);
        Assert.Equal(new[] { "Stage1", "Stage2", "Stage3" }, progress.StageNames);
    }

    [Fact]
    public void Initialize_ResetsCompletedStages()
    {
        var progress = new JobProgress();
        progress.Initialize("Stage1");
        progress.StartStage("Stage1", 10);
        progress.CompleteStage();

        progress.Initialize("NewStage1", "NewStage2");

        Assert.Empty(progress.CompletedStages);
        Assert.Equal(2, progress.TotalStages);
    }

    [Fact]
    public void StartStage_WithTotalItems_SetsCurrentStageProgress()
    {
        var progress = new JobProgress();
        progress.Initialize("Stage1");

        progress.StartStage("Stage1", 100);

        Assert.NotNull(progress.CurrentStageProgress);
        Assert.Equal("Stage1", progress.CurrentStageProgress.StageName);
        Assert.Equal(0, progress.CurrentStageProgress.CurrentItem);
        Assert.Equal(100, progress.CurrentStageProgress.TotalItems);
    }

    [Fact]
    public void StartStage_WithDescription_SetsIndeterminateProgress()
    {
        var progress = new JobProgress();
        progress.Initialize("Stage1");

        progress.StartStage("Stage1", "Processing...");

        Assert.NotNull(progress.CurrentStageProgress);
        Assert.Equal("Stage1", progress.CurrentStageProgress.StageName);
        Assert.Equal(0, progress.CurrentStageProgress.TotalItems);
        Assert.Equal("Processing...", progress.CurrentStageProgress.CurrentItemDescription);
    }

    [Fact]
    public void UpdateProgress_WithCurrentItem_UpdatesCurrentStageProgress()
    {
        var progress = new JobProgress();
        progress.Initialize("Stage1");
        progress.StartStage("Stage1", 100);

        progress.UpdateProgress(50, "Item 50");

        Assert.Equal(50, progress.CurrentStageProgress!.CurrentItem);
        Assert.Equal("Item 50", progress.CurrentStageProgress.CurrentItemDescription);
    }

    [Fact]
    public void UpdateProgress_WithDescriptionOnly_UpdatesDescription()
    {
        var progress = new JobProgress();
        progress.Initialize("Stage1");
        progress.StartStage("Stage1", "Initial");

        progress.UpdateProgress("Updated description");

        Assert.Equal("Updated description", progress.CurrentStageProgress!.CurrentItemDescription);
    }

    [Fact]
    public void CompleteStage_AddsToCompletedStages()
    {
        var progress = new JobProgress();
        progress.Initialize("Stage1", "Stage2");
        progress.StartStage("Stage1", 10);

        progress.CompleteStage();

        Assert.Single(progress.CompletedStages);
        Assert.Contains("Stage1", progress.CompletedStages);
        Assert.Null(progress.CurrentStageProgress);
    }

    [Fact]
    public void OverallPercentComplete_WithNoStages_ReturnsZero()
    {
        var progress = new JobProgress();

        Assert.Equal(0, progress.OverallPercentComplete);
    }

    [Fact]
    public void OverallPercentComplete_WithOneStageComplete_ReturnsCorrectPercentage()
    {
        var progress = new JobProgress();
        progress.Initialize("Stage1", "Stage2");
        progress.StartStage("Stage1", 10);
        progress.CompleteStage();

        Assert.Equal(50, progress.OverallPercentComplete);
    }

    [Fact]
    public void OverallPercentComplete_WithAllStagesComplete_Returns100()
    {
        var progress = new JobProgress();
        progress.Initialize("Stage1", "Stage2");
        progress.StartStage("Stage1", 10);
        progress.CompleteStage();
        progress.StartStage("Stage2", 10);
        progress.CompleteStage();

        Assert.Equal(100, progress.OverallPercentComplete);
    }

    [Fact]
    public void OverallPercentComplete_WithPartialStageProgress_ReturnsCorrectPercentage()
    {
        var progress = new JobProgress();
        progress.Initialize("Stage1", "Stage2");
        progress.StartStage("Stage1", 100);
        progress.UpdateProgress(50);

        // 0.5 (50% of first stage) / 2 stages = 25%
        Assert.Equal(25, progress.OverallPercentComplete);
    }

    [Fact]
    public void ProgressChanged_Event_IsFiredOnInitialize()
    {
        var progress = new JobProgress();
        var eventFired = false;
        progress.ProgressChanged += _ => eventFired = true;

        progress.Initialize("Stage1");

        Assert.True(eventFired);
    }

    [Fact]
    public void ProgressChanged_Event_IsFiredOnStartStage()
    {
        var progress = new JobProgress();
        progress.Initialize("Stage1");
        var eventFired = false;
        progress.ProgressChanged += _ => eventFired = true;

        progress.StartStage("Stage1", 10);

        Assert.True(eventFired);
    }

    [Fact]
    public void ProgressChanged_Event_IsFiredOnUpdateProgress()
    {
        var progress = new JobProgress();
        progress.Initialize("Stage1");
        progress.StartStage("Stage1", 10);
        var eventFired = false;
        progress.ProgressChanged += _ => eventFired = true;

        progress.UpdateProgress(5);

        Assert.True(eventFired);
    }

    [Fact]
    public void ProgressChanged_Event_IsFiredOnCompleteStage()
    {
        var progress = new JobProgress();
        progress.Initialize("Stage1");
        progress.StartStage("Stage1", 10);
        var eventFired = false;
        progress.ProgressChanged += _ => eventFired = true;

        progress.CompleteStage();

        Assert.True(eventFired);
    }

    [Fact]
    public void ToString_ReturnsReadableFormat()
    {
        var progress = new JobProgress();
        progress.Initialize("Stage1", "Stage2");
        progress.StartStage("Stage1", 100);
        progress.UpdateProgress(50);

        var result = progress.ToString();

        Assert.Contains("Overall:", result);
        Assert.Contains("Stage1", result);
    }

    [Fact]
    public void CurrentStageIndex_UpdatesWhenStageStarts()
    {
        var progress = new JobProgress();
        progress.Initialize("Stage1", "Stage2", "Stage3");

        progress.StartStage("Stage2", 10);

        Assert.Equal(1, progress.CurrentStageIndex);
    }

    [Fact]
    public void CurrentStageIndex_HandlesUnknownStageName()
    {
        var progress = new JobProgress();
        progress.Initialize("Stage1", "Stage2");

        progress.StartStage("UnknownStage", 10);

        Assert.Equal(0, progress.CurrentStageIndex);
    }
}

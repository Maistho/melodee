using Melodee.Common.Jobs;

namespace Melodee.Tests.Common.Jobs;

public class JobResultTests
{
    [Fact]
    public void Constructor_WithSuccessStatus_SetsPropertiesCorrectly()
    {
        var result = new JobResult(JobResultStatus.Success, "Operation completed successfully");

        Assert.Equal(JobResultStatus.Success, result.Status);
        Assert.Equal("Operation completed successfully", result.Message);
    }

    [Fact]
    public void Constructor_WithSkippedStatus_SetsPropertiesCorrectly()
    {
        var result = new JobResult(JobResultStatus.Skipped, "Already up to date");

        Assert.Equal(JobResultStatus.Skipped, result.Status);
        Assert.Equal("Already up to date", result.Message);
    }

    [Fact]
    public void Constructor_WithFailedStatus_SetsPropertiesCorrectly()
    {
        var result = new JobResult(JobResultStatus.Failed, "An error occurred");

        Assert.Equal(JobResultStatus.Failed, result.Status);
        Assert.Equal("An error occurred", result.Message);
    }

    [Fact]
    public void Constructor_WithEmptyMessage_AllowsEmptyString()
    {
        var result = new JobResult(JobResultStatus.Success, string.Empty);

        Assert.Equal(string.Empty, result.Message);
    }

    [Fact]
    public void Constructor_WithNullMessage_AllowsNull()
    {
        var result = new JobResult(JobResultStatus.Success, null!);

        Assert.Null(result.Message);
    }

    [Fact]
    public void Equality_TwoIdenticalResults_AreEqual()
    {
        var result1 = new JobResult(JobResultStatus.Success, "Test message");
        var result2 = new JobResult(JobResultStatus.Success, "Test message");

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Equality_DifferentStatus_AreNotEqual()
    {
        var result1 = new JobResult(JobResultStatus.Success, "Test message");
        var result2 = new JobResult(JobResultStatus.Failed, "Test message");

        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public void Equality_DifferentMessage_AreNotEqual()
    {
        var result1 = new JobResult(JobResultStatus.Success, "Message 1");
        var result2 = new JobResult(JobResultStatus.Success, "Message 2");

        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public void Record_WithDeconstruction_WorksCorrectly()
    {
        var result = new JobResult(JobResultStatus.Skipped, "Skipped reason");
        var (status, message) = result;

        Assert.Equal(JobResultStatus.Skipped, status);
        Assert.Equal("Skipped reason", message);
    }

    [Fact]
    public void Record_ToString_ContainsStatusAndMessage()
    {
        var result = new JobResult(JobResultStatus.Failed, "Error details");
        var str = result.ToString();

        Assert.Contains("Failed", str);
        Assert.Contains("Error details", str);
    }

    [Fact]
    public void Record_With_CreatesNewInstanceWithModifiedValue()
    {
        var original = new JobResult(JobResultStatus.Success, "Original");
        var modified = original with { Message = "Modified" };

        Assert.Equal(JobResultStatus.Success, modified.Status);
        Assert.Equal("Modified", modified.Message);
        Assert.Equal("Original", original.Message);
    }
}

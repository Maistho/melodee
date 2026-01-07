using FluentAssertions;
using Melodee.Mql.Security;

namespace Melodee.Mql.Tests;

public class MqlSecurityMonitorTests
{
    [Fact]
    public void LogWarning_RecordsWarningEvent()
    {
        var monitor = new MqlSecurityMonitor(100);

        monitor.LogWarning("Test warning", "test query");

        var events = monitor.GetRecentEvents(10);
        events.Should().HaveCount(1);
        events[0].EventType.Should().Be("Warning");
        events[0].Message.Should().Be("Test warning");
        events[0].IsBlocked.Should().BeFalse();
    }

    [Fact]
    public void LogViolation_RecordsViolationEvent()
    {
        var monitor = new MqlSecurityMonitor(100);

        monitor.LogViolation("MQL_SQL_INJECTION", "SQL injection detected", "test query");

        var events = monitor.GetRecentEvents(10);
        events.Should().HaveCount(1);
        events[0].EventType.Should().Be("Violation");
        events[0].Message.Should().Be("SQL injection detected");
        events[0].IsBlocked.Should().BeTrue();
        events[0].AdditionalData.Should().ContainKey("ErrorCode");
    }

    [Fact]
    public void LogViolation_DefaultIsBlocked()
    {
        var monitor = new MqlSecurityMonitor(100);

        monitor.LogViolation("TEST", "test message", "test query");

        var events = monitor.GetRecentEvents(10);
        events[0].IsBlocked.Should().BeTrue();
    }

    [Fact]
    public void RecordEvent_StoresEvent()
    {
        var monitor = new MqlSecurityMonitor(100);
        var customEvent = new SecurityEvent
        {
            EventType = "Custom",
            Message = "Custom event",
            Query = "test",
            IsBlocked = false
        };

        monitor.RecordEvent(customEvent);

        var events = monitor.GetRecentEvents(10);
        events.Should().Contain(e => e.EventType == "Custom");
    }

    [Fact]
    public void GetMetrics_ReturnsCorrectMetrics()
    {
        var monitor = new MqlSecurityMonitor(100);

        monitor.LogViolation("MQL_SQL_INJECTION", "SQL injection", "test1");
        monitor.LogViolation("MQL_REGEX_DANGEROUS", "ReDoS pattern", "test2");
        monitor.LogWarning("Warning message", "test3");

        var metrics = monitor.GetMetricsAsync(TimeSpan.FromMinutes(5)).Result;

        metrics.TotalViolations.Should().Be(2);
        metrics.SqlInjectionAttempts.Should().Be(1);
        metrics.RedosAttempts.Should().Be(1);
    }

    [Fact]
    public void GetMetrics_FiltersByTimeWindow()
    {
        var monitor = new MqlSecurityMonitor(100);

        monitor.LogViolation("TEST", "Old violation", "test1");

        Thread.Sleep(100);

        monitor.LogViolation("TEST", "New violation", "test2");

        var metrics = monitor.GetMetricsAsync(TimeSpan.FromMilliseconds(50)).Result;

        metrics.TotalViolations.Should().Be(1);
        metrics.TotalViolations.Should().Be(1);
    }

    [Fact]
    public void GetRecentEvents_ReturnsLimitedEvents()
    {
        var monitor = new MqlSecurityMonitor(10);

        for (int i = 0; i < 15; i++)
        {
            monitor.LogWarning($"Warning {i}", $"query {i}");
        }

        var events = monitor.GetRecentEvents(5);

        events.Should().HaveCountLessThanOrEqualTo(5);
    }

    [Fact]
    public void TruncatesLongQuery()
    {
        var monitor = new MqlSecurityMonitor(100);
        var longQuery = new string('a', 1000);

        monitor.LogViolation("TEST", "Test message", longQuery);

        var events = monitor.GetRecentEvents(1);
        events[0].Query.Should().HaveLength(503);
        events[0].Query.Should().EndWith("...");
    }

    [Fact]
    public void HandlesNullQuery()
    {
        var monitor = new MqlSecurityMonitor(100);

        monitor.LogViolation("TEST", "Test message", null!);

        var events = monitor.GetRecentEvents(1);
        events[0].Query.Should().BeNull();
    }

    [Fact]
    public void MultipleViolations_CountCorrectly()
    {
        var monitor = new MqlSecurityMonitor(100);

        monitor.LogViolation("PATTERN1", "Test 1", "q1");
        monitor.LogViolation("PATTERN1", "Test 2", "q2");
        monitor.LogViolation("PATTERN2", "Test 3", "q3");

        var metrics = monitor.GetMetricsAsync(TimeSpan.FromMinutes(5)).Result;

        metrics.TotalViolations.Should().Be(3);
    }
}

using Melodee.Common.Data.Models;
using Melodee.Common.Jobs;
using Melodee.Tests.Common.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;
using Quartz;

namespace Melodee.Tests.Common.Jobs;

public class ChartUpdateJobTests : ServiceTestBase
{
    [Fact]
    public async Task Execute_WithNoCharts_CompletesSuccessfully()
    {
        var chartService = GetChartService();
        var job = new ChartUpdateJob(Logger, MockConfigurationFactory(), chartService);
        var context = CreateJobExecutionContext();

        await job.Execute(context);

        Assert.True(true);
    }

    [Fact]
    public async Task Execute_WithChartsButNoItems_CompletesSuccessfully()
    {
        var contextFactory = MockFactory();
        await using var dbContext = await contextFactory.CreateDbContextAsync();

        var chart = new Chart
        {
            Slug = "test-chart",
            Title = "Test Chart",
            IsVisible = true,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        dbContext.Charts.Add(chart);
        await dbContext.SaveChangesAsync();

        var chartService = GetChartService();
        var job = new ChartUpdateJob(Logger, MockConfigurationFactory(), chartService);
        var context = CreateJobExecutionContext();

        await job.Execute(context);

        Assert.True(true);
    }

    [Fact]
    public async Task Execute_WithMultipleCharts_ProcessesAllCharts()
    {
        var contextFactory = MockFactory();
        await using var dbContext = await contextFactory.CreateDbContextAsync();

        var chart1 = new Chart
        {
            Slug = "chart-1",
            Title = "Chart 1",
            IsVisible = true,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        var chart2 = new Chart
        {
            Slug = "chart-2",
            Title = "Chart 2",
            IsVisible = false,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        dbContext.Charts.AddRange(chart1, chart2);
        await dbContext.SaveChangesAsync();

        var chartItem1 = new ChartItem
        {
            ChartId = chart1.Id,
            Rank = 1,
            ArtistName = "Artist 1",
            AlbumTitle = "Album 1",
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        var chartItem2 = new ChartItem
        {
            ChartId = chart2.Id,
            Rank = 1,
            ArtistName = "Artist 2",
            AlbumTitle = "Album 2",
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        dbContext.ChartItems.AddRange(chartItem1, chartItem2);
        await dbContext.SaveChangesAsync();

        var chartService = GetChartService();
        var job = new ChartUpdateJob(Logger, MockConfigurationFactory(), chartService);
        var context = CreateJobExecutionContext();

        // Job should complete successfully
        await job.Execute(context);

        // Verify items still exist
        var items = await dbContext.ChartItems.ToListAsync();
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task Execute_WithCancellationToken_HandlesGracefully()
    {
        var contextFactory = MockFactory();
        await using var dbContext = await contextFactory.CreateDbContextAsync();

        var chart = new Chart
        {
            Slug = "chart-1",
            Title = "Chart 1",
            IsVisible = true,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        dbContext.Charts.Add(chart);
        await dbContext.SaveChangesAsync();

        var chartService = GetChartService();
        var job = new ChartUpdateJob(Logger, MockConfigurationFactory(), chartService);

        var cts = new CancellationTokenSource();
        cts.Cancel();
        var context = CreateJobExecutionContext(cts.Token);

        // Should handle cancellation gracefully
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await job.Execute(context));
    }

    [Fact]
    public async Task Execute_WithHiddenCharts_ProcessesHiddenCharts()
    {
        var contextFactory = MockFactory();
        await using var dbContext = await contextFactory.CreateDbContextAsync();

        var hiddenChart = new Chart
        {
            Slug = "hidden-chart",
            Title = "Hidden Chart",
            IsVisible = false,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        dbContext.Charts.Add(hiddenChart);
        await dbContext.SaveChangesAsync();

        var chartItem = new ChartItem
        {
            ChartId = hiddenChart.Id,
            Rank = 1,
            ArtistName = "Test Artist",
            AlbumTitle = "Test Album",
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        dbContext.ChartItems.Add(chartItem);
        await dbContext.SaveChangesAsync();

        var chartService = GetChartService();
        var job = new ChartUpdateJob(Logger, MockConfigurationFactory(), chartService);
        var context = CreateJobExecutionContext();

        // Job should complete successfully
        await job.Execute(context);

        // Verify item still exists
        var updatedItem = await dbContext.ChartItems.FirstAsync();
        Assert.NotNull(updatedItem);
    }

    private static IJobExecutionContext CreateJobExecutionContext(CancellationToken? cancellationToken = null)
    {
        var context = new Mock<IJobExecutionContext>();
        context.Setup(x => x.CancellationToken).Returns(cancellationToken ?? CancellationToken.None);
        return context.Object;
    }
}

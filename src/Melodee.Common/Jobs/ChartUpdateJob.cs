using Melodee.Common.Configuration;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;

namespace Melodee.Common.Jobs;

/// <summary>
///     Background job to update all chart items with album links.
///     Runs nightly to ensure charts reflect newly added albums in the system.
/// </summary>
public class ChartUpdateJob(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    ChartService chartService) : JobBase(logger, configurationFactory)
{
    public override async Task Execute(IJobExecutionContext context)
    {
        Logger.Debug("[{JobName}] Starting chart update process", nameof(ChartUpdateJob));

        var chartListResult = await chartService.ListAsync(
            new Models.PagedRequest { Page = 1, PageSize = 500 },
            includeHidden: true,
            cancellationToken: context.CancellationToken)
            .ConfigureAwait(false);

        var charts = chartListResult.Data.ToArray();
        if (charts.Length == 0)
        {
            Logger.Debug("[{JobName}] No charts found to update", nameof(ChartUpdateJob));
            return;
        }

        var totalLinked = 0;
        var totalAmbiguous = 0;
        var totalUnlinked = 0;
        var totalSkipped = 0;

        foreach (var chart in charts)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                Logger.Information("[{JobName}] Cancellation requested, stopping chart update", nameof(ChartUpdateJob));
                break;
            }

            var linkResult = await chartService.LinkItemsAsync(
                chart.Id,
                overwriteManualLinks: false,
                context.CancellationToken)
                .ConfigureAwait(false);

            if (linkResult.IsSuccess && linkResult.Data != null)
            {
                totalLinked += linkResult.Data.LinkedCount;
                totalAmbiguous += linkResult.Data.AmbiguousCount;
                totalUnlinked += linkResult.Data.UnlinkedCount;
                totalSkipped += linkResult.Data.SkippedCount;

                Logger.Debug(
                    "[{JobName}] Updated chart [{ChartId}] '{ChartTitle}': Linked={Linked}, Ambiguous={Ambiguous}, Unlinked={Unlinked}, Skipped={Skipped}",
                    nameof(ChartUpdateJob),
                    chart.Id,
                    chart.Title,
                    linkResult.Data.LinkedCount,
                    linkResult.Data.AmbiguousCount,
                    linkResult.Data.UnlinkedCount,
                    linkResult.Data.SkippedCount);
            }
        }

        Logger.Information(
            "[{JobName}] Completed chart update for [{ChartCount}] charts: Total Linked={TotalLinked}, Ambiguous={TotalAmbiguous}, Unlinked={TotalUnlinked}, Skipped={TotalSkipped}",
            nameof(ChartUpdateJob),
            charts.Length,
            totalLinked,
            totalAmbiguous,
            totalUnlinked,
            totalSkipped);
    }
}

using Melodee.Common.Configuration;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;

namespace Melodee.Common.Jobs;

/// <summary>
///     Links chart entries to albums in the database by matching artist name and album title.
/// </summary>
/// <remarks>
///     <para>
///         Charts (e.g., Billboard, UK Charts) contain entries with artist and album names but no direct
///         database links. This job attempts to match those entries to albums in the Melodee library,
///         enabling features like "Show chart position" on album details.
///     </para>
///     <para>
///         Processing flow:
///         <list type="number">
///             <item>Retrieves all charts from the database (including hidden charts)</item>
///             <item>For each chart, calls LinkItemsAsync to match unlinked entries to albums</item>
///             <item>Matching uses normalized artist/album names and optional MusicBrainz/Spotify IDs</item>
///             <item>Reports statistics: linked (exact match), ambiguous (multiple matches), unlinked (no match), skipped (already linked)</item>
///         </list>
///     </para>
///     <para>
///         The job respects manual links by setting overwriteManualLinks=false, so user corrections are preserved.
///         Run this job after importing new albums to update chart links for newly added content.
///     </para>
///     <para>
///         Default schedule: Daily at 02:00 AM (configurable via jobs.chartUpdate.cronExpression setting).
///     </para>
/// </remarks>
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

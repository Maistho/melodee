using System.Diagnostics;
using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Jobs;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.Scanning;
using Melodee.Common.Services.SearchEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Rebus.Bus;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

/// <summary>
///     Performs a full library scan workflow: processes inbound files, revalidates staging albums,
///     moves approved albums to storage, and inserts them into the database.
/// </summary>
/// <remarks>
///     This command orchestrates the complete media ingestion pipeline:
///     <list type="number">
///         <item>LibraryInboundProcessJob - Process raw files from inbound → staging</item>
///         <item>StagingAlbumRevalidationJob - Re-check albums with invalid artists</item>
///         <item>StagingAutoMoveJob - Move approved albums from staging → storage</item>
///         <item>LibraryInsertJob - Insert albums from storage into database</item>
///     </list>
/// </remarks>
public class LibraryScanCommand : CommandBase<LibraryScanSettings>
{
    private sealed class ScanSummary
    {
        public int InboundFilesProcessed { get; set; }
        public int NewArtistsDiscovered { get; set; }
        public int NewAlbumsDiscovered { get; set; }
        public int NewSongsDiscovered { get; set; }
        public int AlbumsRevalidated { get; set; }
        public int AlbumsNowValid { get; set; }
        public int AlbumsMovedToStorage { get; set; }
        public int ArtistsInserted { get; set; }
        public int AlbumsInserted { get; set; }
        public int SongsInserted { get; set; }
        public List<string> Errors { get; } = [];
    }

    private static string FormatNumber(int number)
    {
        return number.ToString("N0");
    }

    public override async Task<int> ExecuteAsync(CommandContext context, LibraryScanSettings settings, CancellationToken cancellationToken)
    {
        using var scope = CreateServiceProvider().CreateScope();
        var overallStartTime = Stopwatch.GetTimestamp();

        var configGrid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn();

        configGrid
            .AddRow("[b]Force Mode[/]", settings.ForceMode ? "[yellow]Yes[/]" : "[dim]No[/]")
            .AddRow("[b]Verbose[/]", settings.Verbose ? "[yellow]Yes[/]" : "[dim]No[/]");

        AnsiConsole.Write(
            new Panel(configGrid)
                .Header("[yellow]Library Scan Configuration[/]")
                .RoundedBorder()
                .BorderColor(Color.Blue));

        AnsiConsole.WriteLine();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
        var configFactory = scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>();
        var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();
        var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
        var directoryProcessor = scope.ServiceProvider.GetRequiredService<DirectoryProcessorToStagingService>();
        var albumDiscoveryService = scope.ServiceProvider.GetRequiredService<AlbumDiscoveryService>();
        var artistSearchEngineService = scope.ServiceProvider.GetRequiredService<ArtistSearchEngineService>();
        var serializer = scope.ServiceProvider.GetRequiredService<ISerializer>();
        var fileSystemService = scope.ServiceProvider.GetRequiredService<IFileSystemService>();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>();
        var artistService = scope.ServiceProvider.GetRequiredService<ArtistService>();
        var albumService = scope.ServiceProvider.GetRequiredService<AlbumService>();
        var bus = scope.ServiceProvider.GetRequiredService<IBus>();

        var steps = new (string Name, Func<Task> Execute)[]
        {
            ("Processing inbound files", async () =>
            {
                var job = new LibraryInboundProcessJob(logger, configFactory, libraryService, directoryProcessor, schedulerFactory);
                var jobContext = new MelodeeJobExecutionContext(cancellationToken);
                jobContext.Put(MelodeeJobExecutionContext.ForceMode, settings.ForceMode);
                jobContext.Put(MelodeeJobExecutionContext.Verbose, settings.Verbose);
                await job.Execute(jobContext);
            }),
            ("Revalidating staging albums", async () =>
            {
                var job = new StagingAlbumRevalidationJob(logger, configFactory, libraryService, albumDiscoveryService, artistSearchEngineService, serializer, fileSystemService);
                var jobContext = new MelodeeJobExecutionContext(cancellationToken);
                jobContext.Put(MelodeeJobExecutionContext.ForceMode, settings.ForceMode);
                await job.Execute(jobContext);
            }),
            ("Moving approved albums to storage", async () =>
            {
                var job = new StagingAutoMoveJob(logger, configFactory, libraryService, schedulerFactory);
                var jobContext = new MelodeeJobExecutionContext(cancellationToken);
                await job.Execute(jobContext);
            }),
            ("Inserting albums into database", async () =>
            {
                var job = new LibraryInsertJob(logger, configFactory, libraryService, serializer, dbContextFactory, artistService, albumService, albumDiscoveryService, directoryProcessor, bus);
                var jobContext = new MelodeeJobExecutionContext(cancellationToken);
                jobContext.Put(MelodeeJobExecutionContext.ForceMode, settings.ForceMode);
                jobContext.Put(MelodeeJobExecutionContext.Verbose, settings.Verbose);
                await job.Execute(jobContext);
            })
        };

        await AnsiConsole.Progress()
            .AutoRefresh(true)
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
            [
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn()
            ])
            .StartAsync(async ctx =>
            {
                var overallTask = ctx.AddTask("[bold blue]Full Library Scan[/]", maxValue: steps.Length);

                for (var i = 0; i < steps.Length; i++)
                {
                    var (name, execute) = steps[i];
                    var stepTask = ctx.AddTask($"[green]{name}[/]", autoStart: false);
                    stepTask.IsIndeterminate = true;
                    stepTask.StartTask();

                    var stepStartTime = Stopwatch.GetTimestamp();

                    try
                    {
                        await execute();
                        var elapsed = Stopwatch.GetElapsedTime(stepStartTime);
                        stepTask.Description = $"[green]✓ {name}[/] [dim]({elapsed:mm\\:ss})[/]";
                        stepTask.Value = 100;
                        stepTask.MaxValue = 100;
                        stepTask.IsIndeterminate = false;
                    }
                    catch (Exception ex)
                    {
                        var elapsed = Stopwatch.GetElapsedTime(stepStartTime);
                        stepTask.Description = $"[red]✗ {name}[/] [dim]({elapsed:mm\\:ss})[/]";
                        stepTask.Value = 100;
                        stepTask.MaxValue = 100;
                        stepTask.IsIndeterminate = false;
                        logger.Error(ex, "Error during {StepName}", name);
                    }

                    stepTask.StopTask();
                    overallTask.Increment(1);
                }

                overallTask.Description = "[bold green]✓ Full Library Scan Complete[/]";
            });

        AnsiConsole.WriteLine();

        var totalElapsed = Stopwatch.GetElapsedTime(overallStartTime);
        var rule = new Rule($"[green]Library scan completed in {totalElapsed:hh\\:mm\\:ss}[/]")
        {
            Justification = Justify.Left
        };
        AnsiConsole.Write(rule);

        return 0;
    }
}

using System.Diagnostics;
using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Jobs;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Scanning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Bus;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

/// <summary>
///     This runs the job that scans all the library type libraries
/// </summary>
public class LibraryScanCommand : CommandBase<LibraryScanSettings>
{
    private static string FormatNumber(int number)
    {
        return number.ToString("N0");
    }

    public override async Task<int> ExecuteAsync(CommandContext context, LibraryScanSettings settings, CancellationToken cancellationToken)
    {
        using (var scope = CreateServiceProvider().CreateScope())
        {
            var startTime = Stopwatch.GetTimestamp();

            // Display initial configuration
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

            var job = new LibraryInsertJob
            (
                scope.ServiceProvider.GetRequiredService<ILogger>(),
                scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>(),
                scope.ServiceProvider.GetRequiredService<LibraryService>(),
                scope.ServiceProvider.GetRequiredService<ISerializer>(),
                scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>(),
                scope.ServiceProvider.GetRequiredService<ArtistService>(),
                scope.ServiceProvider.GetRequiredService<AlbumService>(),
                scope.ServiceProvider.GetRequiredService<AlbumDiscoveryService>(),
                scope.ServiceProvider.GetRequiredService<DirectoryProcessorToStagingService>(),
                scope.ServiceProvider.GetRequiredService<IBus>()
            );

            await AnsiConsole.Progress()
                .AutoRefresh(true)
                .AutoClear(false)
                .HideCompleted(false)
                .Columns(
                [
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new SpinnerColumn()
                ])
                .StartAsync(async ctx =>
                {
                    var progressTask = ctx.AddTask("[green]Initializing...[/]", maxValue: 100);

                    job.OnProcessingEvent += (sender, e) =>
                    {
                        switch (e.Type)
                        {
                            case ProcessingEventType.Start:
                                if (e.Max == 0)
                                {
                                    progressTask.Description = "[yellow]No albums found to scan[/]";
                                    progressTask.StopTask();
                                }
                                else
                                {
                                    progressTask.MaxValue = e.Max;
                                    progressTask.Description = $"[green]Scanning:[/] 0/{FormatNumber(e.Max)} (0.0%)";
                                }
                                break;

                            case ProcessingEventType.Processing:
                                if (e.Max > 0)
                                {
                                    var percentComplete = (double)e.Current / e.Max * 100;
                                    progressTask.Value = e.Current;

                                    // Extract album name from message if present
                                    var albumName = e.Message;
                                    if (albumName.StartsWith("Processing [") && albumName.EndsWith("]"))
                                    {
                                        albumName = albumName[12..^1];
                                        if (albumName.Length > 50)
                                        {
                                            albumName = albumName[..47] + "...";
                                        }
                                    }

                                    progressTask.Description = $"[green]Albums:[/] {FormatNumber(e.Current)}/{FormatNumber(e.Max)} ([cyan]{percentComplete:F1}%[/]) | [yellow]{Markup.Escape(albumName)}[/]";
                                }
                                break;

                            case ProcessingEventType.Stop:
                                progressTask.Value = progressTask.MaxValue;
                                progressTask.StopTask();

                                var elapsed = Stopwatch.GetElapsedTime(startTime);
                                progressTask.Description = $"[green]✓ Completed:[/] {FormatNumber(e.Max)} albums scanned | [cyan]Time: {elapsed:hh\\:mm\\:ss}[/]";
                                break;
                        }
                    };

                    var jobExecutionContext = new MelodeeJobExecutionContext(CancellationToken.None);
                    jobExecutionContext.Put(MelodeeJobExecutionContext.ForceMode, settings.ForceMode);
                    jobExecutionContext.Put(MelodeeJobExecutionContext.Verbose, settings.Verbose);
                    await job.Execute(jobExecutionContext);
                });

            AnsiConsole.WriteLine();

            var rule = new Rule("[green]Scan operation completed successfully[/]")
            {
                Justification = Justify.Left
            };
            AnsiConsole.Write(rule);

            return 0;
        }
    }
}

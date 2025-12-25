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

            var currentMessage = "Initializing...";
            var maxValue = 0;
            var currentValue = 0;
            var isComplete = false;

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
                    var progressTask = ctx.AddTask("[green]Initializing...[/]", autoStart: false);
                    progressTask.IsIndeterminate = true;
                    progressTask.StartTask();

                    job.OnProcessingEvent += (_, e) =>
                    {
                        switch (e.Type)
                        {
                            case ProcessingEventType.Start:
                                maxValue = e.Max;
                                currentValue = 0;
                                if (e.Max == 0)
                                {
                                    progressTask.Description = $"[yellow]{Markup.Escape(e.Message)}[/]";
                                }
                                else
                                {
                                    progressTask.IsIndeterminate = false;
                                    progressTask.MaxValue = e.Max;
                                    progressTask.Value = 0;
                                    progressTask.Description = $"[green]{Markup.Escape(e.Message)}[/]";
                                }
                                break;

                            case ProcessingEventType.Processing:
                                currentMessage = e.Message;
                                if (e.Max > 0)
                                {
                                    maxValue = e.Max;
                                    currentValue = e.Current;
                                    progressTask.IsIndeterminate = false;
                                    progressTask.MaxValue = e.Max;
                                    progressTask.Value = e.Current;

                                    var displayMessage = e.Message;
                                    if (displayMessage.Length > 60)
                                    {
                                        displayMessage = displayMessage[..57] + "...";
                                    }

                                    var percentComplete = (double)e.Current / e.Max * 100;
                                    progressTask.Description = $"[green]Scanning:[/] {FormatNumber(e.Current)}/{FormatNumber(e.Max)} ([cyan]{percentComplete:F1}%[/]) [dim]{Markup.Escape(displayMessage)}[/]";
                                }
                                else
                                {
                                    var displayMessage = e.Message;
                                    if (displayMessage.Length > 60)
                                    {
                                        displayMessage = displayMessage[..57] + "...";
                                    }
                                    progressTask.Description = $"[yellow]{Markup.Escape(displayMessage)}[/]";
                                }
                                break;

                            case ProcessingEventType.Stop:
                                isComplete = true;
                                if (progressTask.MaxValue > 0)
                                {
                                    progressTask.Value = progressTask.MaxValue;
                                }
                                var elapsed = Stopwatch.GetElapsedTime(startTime);
                                progressTask.Description = $"[green]✓ Complete:[/] {Markup.Escape(e.Message)} [cyan]({elapsed:hh\\:mm\\:ss})[/]";
                                progressTask.StopTask();
                                break;
                        }
                    };

                    var jobExecutionContext = new MelodeeJobExecutionContext(CancellationToken.None);
                    jobExecutionContext.Put(MelodeeJobExecutionContext.ForceMode, settings.ForceMode);
                    jobExecutionContext.Put(MelodeeJobExecutionContext.Verbose, settings.Verbose);
                    await job.Execute(jobExecutionContext);

                    if (!isComplete)
                    {
                        progressTask.StopTask();
                    }
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

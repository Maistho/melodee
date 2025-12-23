using System.Diagnostics;
using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Models;
using Melodee.Common.Plugins.Processor.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Scanning;
using Melodee.Common.Utility;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace Melodee.Cli.Command;

public class ProcessInboundCommand : CommandBase<LibraryProcessSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LibraryProcessSettings settings, CancellationToken cancellationToken)
    {
        if (settings.Verbose)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }

        using (var scope = CreateServiceProvider().CreateScope())
        {
            var serializer = scope.ServiceProvider.GetRequiredService<ISerializer>();
            var melodeeConfigurationFactory = scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>();
            var melodeeConfiguration = await melodeeConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);

            string directoryInbound;
            string directoryStaging;
            Instant? lastScanAt = null;

            if (settings.IsPathBasedMode)
            {
                directoryInbound = settings.InboundPath!;
                directoryStaging = settings.StagingPath!;
            }
            else
            {
                var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();

                var libraryToProcess = (await libraryService.ListAsync(new PagedRequest(), cancellationToken)).Data?.FirstOrDefault(x => string.Equals(x.Name, settings.LibraryName, StringComparison.OrdinalIgnoreCase));
                if (libraryToProcess == null)
                {
                    throw new Exception($"Library with name [{settings.LibraryName}] not found.");
                }

                directoryInbound = libraryToProcess.Path;
                directoryStaging = (await libraryService.GetStagingLibraryAsync(cancellationToken).ConfigureAwait(false)).Data!.Path;
                lastScanAt = settings.ForceMode ? null : libraryToProcess.LastScanAt;
            }

            var grid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(4))
                .AddColumn()
                .AddRow("[b]Mode[/]", settings.IsPathBasedMode ? "[blue]Path-based[/] (bypassing database)" : "[blue]Library-based[/]")
                .AddRow("[b]Copy Mode[/]", SafeParser.ToBoolean(melodeeConfiguration.Configuration[SettingRegistry.ProcessingDoDeleteOriginal]) ? "[dim]No[/]" : "[yellow]Yes[/]")
                .AddRow("[b]Force Mode[/]", SafeParser.ToBoolean(melodeeConfiguration.Configuration[SettingRegistry.ProcessingDoOverrideExistingMelodeeDataFiles]) ? "[yellow]Yes[/]" : "[dim]No[/]")
                .AddRow("[b]PreDiscovery Script[/]", $"{SafeParser.ToString(melodeeConfiguration.Configuration[SettingRegistry.ScriptingPreDiscoveryScript])}")
                .AddRow("[b]Inbound[/]", $"{directoryInbound.EscapeMarkup()}")
                .AddRow("[b]Staging[/]", $"{directoryStaging.EscapeMarkup()}");

            AnsiConsole.Write(
                new Panel(grid)
                    .Header("[yellow]Process Inbound Configuration[/]")
                    .RoundedBorder()
                    .BorderColor(Color.Blue));

            AnsiConsole.WriteLine();

            var processor = scope.ServiceProvider.GetRequiredService<DirectoryProcessorToStagingService>();
            var dirInfo = new DirectoryInfo(directoryInbound);
            if (!dirInfo.Exists)
            {
                throw new Exception($"Directory [{directoryInbound}] does not exist.");
            }

            var startTicks = Stopwatch.GetTimestamp();

            Log.Debug("\ud83d\udcc1 Processing directory [{Inbound}]", directoryInbound);

            await processor.InitializeAsync(null, settings.IsPathBasedMode ? directoryStaging : null, cancellationToken);

            var fileSystemDirectoryInfo = new FileSystemDirectoryInfo
            {
                Path = directoryInbound,
                Name = dirInfo.Name
            };

            OperationResult<DirectoryProcessorResult>? result = null;

            await AnsiConsole.Status()
                .StartAsync("[yellow]Processing inbound directory...[/]", async ctx =>
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    result = await processor.ProcessDirectoryAsync(fileSystemDirectoryInfo, lastScanAt, settings.ProcessLimit, cancellationToken);
                });

            Log.Debug("ℹ️ Processed directory [{Inbound}] in [{ElapsedTime}]", directoryInbound, Stopwatch.GetElapsedTime(startTicks));

            AnsiConsole.WriteLine();

            if (result != null && result.IsSuccess && result.Data != null)
            {
                var resultData = result.Data;
                var statsGrid = new Grid()
                    .AddColumn(new GridColumn().NoWrap().PadRight(4))
                    .AddColumn();

                statsGrid
                    .AddRow("[b]Albums Processed[/]", $"[green]{resultData.NumberOfAlbumsProcessed:N0}[/]")
                    .AddRow("[b]Valid Albums[/]", $"[green]{resultData.NumberOfValidAlbumsProcessed:N0}[/] ([cyan]{resultData.FormattedValidPercentageProcessed}[/])")
                    .AddRow("[b]New Albums[/]", $"[yellow]{resultData.NewAlbumsCount:N0}[/]")
                    .AddRow("[b]New Songs[/]", $"[yellow]{resultData.NewSongsCount:N0}[/]");

                AnsiConsole.Write(
                    new Panel(statsGrid)
                        .Header("[green]Process Results[/]")
                        .RoundedBorder()
                        .BorderColor(Color.Green));
            }

            if (settings.Verbose && result != null)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.Write(
                    new Panel(new JsonText(serializer.Serialize(result) ?? string.Empty))
                        .Header("[yellow]Detailed Results[/]")
                        .Collapse()
                        .RoundedBorder()
                        .BorderColor(Color.Yellow));
            }

            return result?.IsSuccess == true ? 0 : 1;
        }
    }

    private static string YesNo(bool value)
    {
        return value ? "Yes" : "No";
    }
}

using System.Diagnostics;
using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models;
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

                AnsiConsole.MarkupLine("[blue]Running in path-based mode (bypassing database library lookup)[/]");
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
                .AddRow("[b]Path-based Mode?[/]", $"{YesNo(settings.IsPathBasedMode)}")
                .AddRow("[b]Copy Mode?[/]", $"{YesNo(!SafeParser.ToBoolean(melodeeConfiguration.Configuration[SettingRegistry.ProcessingDoDeleteOriginal]))}")
                .AddRow("[b]Force Mode?[/]", $"{YesNo(SafeParser.ToBoolean(melodeeConfiguration.Configuration[SettingRegistry.ProcessingDoOverrideExistingMelodeeDataFiles]))}")
                .AddRow("[b]PreDiscovery Script[/]", $"{SafeParser.ToString(melodeeConfiguration.Configuration[SettingRegistry.ScriptingPreDiscoveryScript])}")
                .AddRow("[b]Inbound[/]", $"{directoryInbound.EscapeMarkup()}")
                .AddRow("[b]Staging[/]", $"{directoryStaging.EscapeMarkup()}");

            AnsiConsole.Write(
                new Panel(grid)
                    .Header("Configuration"));

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

            var result = await processor.ProcessDirectoryAsync(fileSystemDirectoryInfo, lastScanAt, settings.ProcessLimit, cancellationToken);

            Log.Debug("ℹ️ Processed directory [{Inbound}] in [{ElapsedTime}]", directoryInbound, Stopwatch.GetElapsedTime(startTicks));

            if (settings.Verbose)
            {
                AnsiConsole.Write(
                    new Panel(new JsonText(serializer.Serialize(result) ?? string.Empty))
                        .Header("Process Result")
                        .Collapse()
                        .RoundedBorder()
                        .BorderColor(Color.Yellow));
            }

            return result.IsSuccess ? 0 : 1;
        }
    }

    private static string YesNo(bool value)
    {
        return value ? "Yes" : "No";
    }
}

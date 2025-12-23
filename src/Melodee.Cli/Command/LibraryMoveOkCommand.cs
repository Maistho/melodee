using Melodee.Cli.CommandSettings;
using Melodee.Common.Enums;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace Melodee.Cli.Command;

public class LibraryMoveOkCommand : CommandBase<LibraryMoveOkSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LibraryMoveOkSettings settings, CancellationToken cancellationToken)
    {
        using (var scope = CreateServiceProvider().CreateScope())
        {
            var serializer = scope.ServiceProvider.GetRequiredService<ISerializer>();
            var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();

            libraryService.OnProcessingProgressEvent += (sender, e) =>
            {
                switch (e.Type)
                {
                    case ProcessingEventType.Start:
                        if (e.Max == 0)
                        {
                            AnsiConsole.MarkupLine("[yellow]No albums found.[/]");
                        }
                        else
                        {
                            AnsiConsole.MarkupLine($"[blue]| {e.Max} albums to move.[/]");
                        }

                        break;

                    case ProcessingEventType.Processing:
                        if (e.Max > 0 && e.Current % 10 == 0)
                        {
                            AnsiConsole.MarkupLine($"[blue]- moved {e.Current} albums.[/]");
                        }

                        break;

                    case ProcessingEventType.Stop:
                        if (e.Max > 0)
                        {
                            AnsiConsole.MarkupLine($"[green]= {Markup.Escape(e.Message)}[/]");
                        }

                        break;
                }
            };

            Common.Models.OperationResult<bool> result;

            if (settings.IsPathBasedMode)
            {
                AnsiConsole.MarkupLine("[blue]Running in path-based mode (bypassing database library lookup)[/]");
                AnsiConsole.MarkupLine($"[grey]From: {settings.FromPath!.EscapeMarkup()}[/]");
                AnsiConsole.MarkupLine($"[grey]To: {settings.ToPath!.EscapeMarkup()}[/]");

                result = await libraryService.MoveAlbumsFromPathToPath(
                        settings.FromPath!,
                        settings.ToPath!,
                        b => b.Status == AlbumStatus.Ok,
                        settings.Verbose,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                if (settings.LibraryName == settings.ToLibraryName)
                {
                    AnsiConsole.MarkupLine("[red]Source and destination library are the same.[/]");
                    return 1;
                }

                result = await libraryService.MoveAlbumsFromLibraryToLibrary(
                        settings.LibraryName,
                        settings.ToLibraryName,
                        b => b.Status == AlbumStatus.Ok,
                        settings.Verbose,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            if (!result.IsSuccess)
            {
                AnsiConsole.Write(
                    new Panel(new JsonText(serializer.Serialize(result) ?? string.Empty))
                        .Header("Not successful")
                        .Collapse()
                        .RoundedBorder()
                        .BorderColor(Color.Yellow));
            }

            return result.IsSuccess ? 0 : 1;
        }
    }
}

using Melodee.Cli.CommandSettings;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

/// <summary>
///     Search for albums by name.
/// </summary>
public class AlbumSearchCommand : CommandBase<AlbumSearchSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, AlbumSearchSettings settings, CancellationToken cancellationToken)
    {
        using var scope = CreateServiceProvider().CreateScope();
        var albumService = scope.ServiceProvider.GetRequiredService<AlbumService>();

        var searchTerm = settings.Query.ToNormalizedString() ?? settings.Query;

        var pagedRequest = new PagedRequest
        {
            PageSize = (short)settings.Limit,
            OrderBy = new Dictionary<string, string> { { "Name", "ASC" } },
            FilterBy = [new FilterOperatorInfo("NameNormalized", FilterOperator.Contains, searchTerm)]
        };

        var result = await albumService.ListAsync(pagedRequest, cancellationToken);

        if (!result.IsSuccess || result.Data == null || !result.Data.Any())
        {
            AnsiConsole.MarkupLine($"[yellow]No albums found matching:[/] {settings.Query.EscapeMarkup()}");
            return 0;
        }

        var albums = result.Data.ToList();

        if (settings.ReturnRaw)
        {
            var jsonOutput = albums.Select(a => new
            {
                a.Id,
                a.ApiKey,
                a.Name,
                a.NameNormalized,
                a.ArtistName,
                a.ArtistApiKey,
                a.SongCount,
                a.Duration,
                ReleaseDate = a.ReleaseDate.ToString(),
                Status = a.AlbumStatusValue.ToString(),
                a.IsLocked,
                CreatedAt = a.CreatedAt.ToDateTimeUtc(),
                a.PlayedCount,
                a.CalculatedRating
            });
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(jsonOutput, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            return 0;
        }

        AnsiConsole.MarkupLine($"[blue]Search results for:[/] {settings.Query.EscapeMarkup()}");
        AnsiConsole.WriteLine();

        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.AddColumn(new TableColumn("[bold]Artist[/]"));
        table.AddColumn(new TableColumn("[bold]Album[/]"));
        table.AddColumn(new TableColumn("[bold]Year[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Songs[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold]Status[/]").Centered());

        foreach (var album in albums)
        {
            var statusColor = album.AlbumStatusValue switch
            {
                AlbumStatus.Ok => "green",
                AlbumStatus.New => "cyan",
                AlbumStatus.Invalid => "yellow",
                _ => "grey"
            };

            table.AddRow(
                album.ArtistName.EscapeMarkup().Length > 30
                    ? album.ArtistName.EscapeMarkup()[..27] + "..."
                    : album.ArtistName.EscapeMarkup(),
                album.Name.EscapeMarkup().Length > 40
                    ? album.Name.EscapeMarkup()[..37] + "..."
                    : album.Name.EscapeMarkup(),
                album.ReleaseDate.Year.ToString(),
                album.SongCount.ToString(),
                $"[{statusColor}]{album.AlbumStatusValue}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[grey]Found {result.TotalCount:N0} matching albums (showing {albums.Count})[/]");

        return 0;
    }
}

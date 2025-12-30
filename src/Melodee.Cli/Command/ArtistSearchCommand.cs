using Melodee.Cli.CommandSettings;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

/// <summary>
///     Search for artists by name.
/// </summary>
public class ArtistSearchCommand : CommandBase<ArtistSearchSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ArtistSearchSettings settings, CancellationToken cancellationToken)
    {
        using var scope = CreateServiceProvider().CreateScope();
        var artistService = scope.ServiceProvider.GetRequiredService<ArtistService>();

        var searchTerm = settings.Query.ToNormalizedString() ?? settings.Query;

        var pagedRequest = new PagedRequest
        {
            PageSize = (short)settings.Limit,
            OrderBy = new Dictionary<string, string> { { "Name", "ASC" } },
            FilterBy = [new FilterOperatorInfo("NameNormalized", FilterOperator.Contains, searchTerm)]
        };

        var result = await artistService.ListAsync(pagedRequest, cancellationToken);

        if (!result.IsSuccess || result.Data == null || !result.Data.Any())
        {
            AnsiConsole.MarkupLine($"[yellow]No artists found matching:[/] {settings.Query.EscapeMarkup()}");
            return 0;
        }

        var artists = result.Data.ToList();

        if (settings.ReturnRaw)
        {
            var jsonOutput = artists.Select(a => new
            {
                a.Id,
                a.ApiKey,
                a.Name,
                a.NameNormalized,
                a.AlternateNames,
                a.AlbumCount,
                a.SongCount,
                a.IsLocked,
                a.LibraryId,
                CreatedAt = a.CreatedAt.ToDateTimeUtc(),
                LastPlayedAt = a.LastPlayedAt?.ToDateTimeUtc(),
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
        table.AddColumn(new TableColumn("[bold]Name[/]"));
        table.AddColumn(new TableColumn("[bold]Alternate Names[/]"));
        table.AddColumn(new TableColumn("[bold]Albums[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold]Songs[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold]Rating[/]").Centered());

        foreach (var artist in artists)
        {
            var altNames = string.IsNullOrWhiteSpace(artist.AlternateNames)
                ? "[grey]---[/]"
                : artist.AlternateNames.Length > 40
                    ? artist.AlternateNames[..37].EscapeMarkup() + "..."
                    : artist.AlternateNames.EscapeMarkup();

            var ratingDisplay = artist.CalculatedRating > 0
                ? $"[yellow]{artist.CalculatedRating:F1}★[/]"
                : "[grey]---[/]";

            table.AddRow(
                artist.Name.EscapeMarkup(),
                altNames,
                artist.AlbumCount.ToString(),
                artist.SongCount.ToString(),
                ratingDisplay
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[grey]Found {result.TotalCount:N0} matching artists (showing {artists.Count})[/]");

        return 0;
    }
}

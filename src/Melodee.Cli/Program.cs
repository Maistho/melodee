using Melodee.Cli.Command;
using Melodee.Cli.CommandSettings;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Melodee.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.SetApplicationName("mcli");
            
            config.AddBranch<ConfigurationSetSetting>("configuration", add =>
            {
                add.SetDescription("Manage Melodee configuration settings");
                add.AddCommand<ConfigurationSetCommand>("set")
                    .WithDescription("Modify Melodee configuration.");
            });
            config.AddBranch<ShowMpegInfoSettings>("file", add =>
            {
                add.SetDescription("File analysis and inspection tools");
                add.AddCommand<ShowMpegInfoCommand>("mpeg")
                    .WithDescription("Load given file and show MPEG info and if Melodee thinks this is a valid MPEG file.");
            });
            config.AddBranch<ImportSetting>("import", add =>
            {
                add.SetDescription("Import data from external sources");
                add.AddCommand<ImportUserFavoriteCommand>("user-favorite-songs")
                    .WithDescription("Import user favorite songs from a given CSV file.");
            });
            config.AddBranch<JobSettings>("job", add =>
            {
                add.SetDescription("Run background jobs and maintenance tasks");
                add.AddCommand<JobRunArtistSearchEngineDatabaseHousekeepingJobCommand>("artistsearchengine-refresh")
                    .WithDescription("Run artist search engine refresh job. This updates the local database of artists albums from search engines.");
                add.AddCommand<JobRunMusicBrainzUpdateDatabaseJobCommand>("musicbrainz-update")
                    .WithDescription("Run MusicBrainz update database job. This downloads MusicBrainz data dump and creates local database for Melodee when scanning metadata.");
            });
            config.AddBranch("library", add =>
            {
                add.SetDescription("Library management and operations");
                add.AddCommand<LibraryAlbumStatusReportCommand>("album-report")
                    .WithAlias("ar")
                    .WithDescription("Show report of albums found for library.");
                add.AddCommand<LibraryCleanCommand>("clean")
                    .WithAlias("c")
                    .WithDescription("Clean library and delete any folders without media files. CAUTION: Destructive!");
                add.AddCommand<LibraryListCommand>("list")
                    .WithAlias("ls")
                    .WithDescription("List all libraries with their details.");
                add.AddCommand<ProcessInboundCommand>("process")
                    .WithAlias("p")
                    .WithDescription("Process media in given library into staging library.");
                add.AddCommand<LibraryPurgeCommand>("purge")
                    .WithDescription("Purge library, deleting artists, albums, album songs and resetting library stats. CAUTION: Destructive!");
                add.AddCommand<LibraryMoveOkCommand>("move-ok")
                    .WithAlias("m")
                    .WithDescription("Move 'Ok' status albums into the given library.");
                add.AddCommand<LibraryRebuildCommand>("rebuild")
                    .WithAlias("r")
                    .WithDescription("Rebuild melodee metadata albums in the given library.");
                add.AddCommand<LibraryScanCommand>("scan")
                    .WithAlias("s")
                    .WithDescription("Scan all non inbound and staging libraries for database updates from albums.");
                add.AddCommand<LibraryStatsCommand>("stats")
                    .WithAlias("ss")
                    .WithDescription("Show statistics for given library and library directory.");
            });
            config.AddBranch<ParseSettings>("parser", add =>
            {
                add.SetDescription("Parse and analyze media metadata files");
                add.AddCommand<ParseCommand>("parse")
                    .WithDescription("Parse a given media file (CUE, NFO, SFV, etc.) and show results.");
            });
            config.AddBranch<ValidateSettings>("validate", add =>
            {
                add.SetDescription("Validate media files and metadata");
                add.AddCommand<ValidateCommand>("album")
                    .WithDescription("Validate a metadata album data file (melodee.json).");
            });
            config.AddBranch<ShowTagsSettings>("tags", add =>
            {
                add.SetDescription("Display and manage media file tags");
                add.AddCommand<ShowTagsCommand>("show")
                    .WithDescription("Load given media file and show all known ID3 tags.");
            });
        });


        var doShowVersion = args.Length < 4 || (args.Length > 3 && args[2] == "--verbose" && args[3]?.ToUpper() != "FALSE");
        if (doShowVersion)
        {
            var version = typeof(Program).Assembly.GetName().Version;
            AnsiConsole.MarkupLine($":musical_note: [bold cyan]Melodee Command Line Interface[/] [grey]v{version}[/]");
            AnsiConsole.WriteLine();
            
            if (args.Length == 0)
            {
                var panel = new Panel(
                    "[yellow]Environment Variables:[/]\n" +
                    "  [cyan]MELODEE_APPSETTINGS_PATH[/] - Path to custom appsettings.json file\n" +
                    "  [cyan]ASPNETCORE_ENVIRONMENT[/]   - Environment name (Development, Production, etc.)\n\n" +
                    "[yellow]Examples:[/]\n" +
                    "  [grey]mcli library list[/]\n" +
                    "  [grey]mcli library album-report --library \"Staging\"[/]\n" +
                    "  [grey]mcli library album-report -l \"Staging\" --full[/]\n" +
                    "  [grey]mcli library stats --library \"Staging\"[/]\n" +
                    "  [grey]MELODEE_APPSETTINGS_PATH=\"/path/to/appsettings.json\" mcli library scan -l \"Storage\"[/]")
                {
                    Header = new PanelHeader("[bold]Quick Start[/]", Justify.Left),
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(foreground: Color.Grey)
                };
                AnsiConsole.Write(panel);
                AnsiConsole.WriteLine();
            }
        }

        return app.Run(args);
    }
}

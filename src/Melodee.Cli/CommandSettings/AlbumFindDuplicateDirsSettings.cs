using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class AlbumFindDuplicateDirsSettings : LibrarySettings
{
    [Description("Search MusicBrainz and other metadata sources to determine correct album year.")]
    [CommandOption("--search")]
    [DefaultValue(false)]
    public bool SearchMetadata { get; init; }

    [Description("Output results in JSON format for scripting.")]
    [CommandOption("--json")]
    [DefaultValue(false)]
    public bool JsonOutput { get; init; }

    [Description("Automatically delete duplicate directories that have incorrect metadata (requires --search).")]
    [CommandOption("--delete")]
    [DefaultValue(false)]
    public bool Delete { get; init; }

    [Description("Filter to a specific artist name (case-insensitive, partial match).")]
    [CommandOption("-a|--artist")]
    public string? ArtistFilter { get; init; }

    [Description("Maximum number of duplicate groups to process.")]
    [CommandOption("-n|--limit")]
    public int? Limit { get; init; }
}

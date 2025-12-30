using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class ArtistFindDuplicatesSettings : ArtistSettings
{
    [Description("Minimum similarity score (0.0 to 1.0). Only pairs with score >= this value are returned.")]
    [CommandOption("-m|--min-score")]
    [DefaultValue(0.7)]
    public double MinScore { get; init; }

    [Description("Maximum number of duplicate groups to return.")]
    [CommandOption("-n|--limit")]
    public int? Limit { get; init; }

    [Description("Limit to artists whose external IDs include a given source (e.g., 'musicbrainz', 'spotify', 'discogs').")]
    [CommandOption("-s|--source")]
    public string? Source { get; init; }

    [Description("Restrict search to duplicates of a single artist by database ID.")]
    [CommandOption("-a|--artist-id")]
    public int? ArtistId { get; init; }

    [Description("Output results in JSON format for scripting.")]
    [CommandOption("--json")]
    [DefaultValue(false)]
    public bool JsonOutput { get; init; }

    [Description("Include low-confidence candidates that would normally be filtered out.")]
    [CommandOption("--include-low-confidence")]
    [DefaultValue(false)]
    public bool IncludeLowConfidence { get; init; }

    [Description("Automatically merge the flagged duplicate artists into the suggested primary artist.")]
    [CommandOption("--merge")]
    [DefaultValue(false)]
    public bool Merge { get; init; }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class ArtistSearchSettings : ArtistSettings
{
    [Description("Search query for artist name.")]
    [CommandArgument(0, "<QUERY>")]
    [Required]
    public string Query { get; init; } = string.Empty;

    [Description("Output results in JSON format.")]
    [CommandOption("--raw")]
    [DefaultValue(false)]
    public bool ReturnRaw { get; init; }

    [Description("Maximum number of results to return.")]
    [CommandOption("-n|--limit")]
    [DefaultValue(25)]
    public int Limit { get; init; }
}

using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class ArtistListSettings : ArtistSettings
{
    [Description("Output results in JSON format.")]
    [CommandOption("--raw")]
    [DefaultValue(false)]
    public bool ReturnRaw { get; init; }

    [Description("Maximum number of results to return.")]
    [CommandOption("-n|--limit")]
    [DefaultValue(50)]
    public int Limit { get; init; }

    [Description("Filter by library name.")]
    [CommandOption("-l|--library")]
    public string? LibraryName { get; init; }
}

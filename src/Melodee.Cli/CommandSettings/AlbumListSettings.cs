using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class AlbumListSettings : AlbumSettings
{
    [Description("Maximum number of results to return.")]
    [CommandOption("-n|--limit")]
    [DefaultValue(50)]
    public int Limit { get; init; }

    [Description("Output results in JSON format.")]
    [CommandOption("--raw")]
    [DefaultValue(false)]
    public bool ReturnRaw { get; init; }

    [Description("Filter by album status (Ok, New, NeedsAttention, Duplicate, Invalid).")]
    [CommandOption("-s|--status")]
    public string? Status { get; init; }
}

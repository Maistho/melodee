using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class AlbumStatsSettings : AlbumSettings
{
    [Description("Output results in JSON format.")]
    [CommandOption("--raw")]
    [DefaultValue(false)]
    public bool ReturnRaw { get; init; }
}

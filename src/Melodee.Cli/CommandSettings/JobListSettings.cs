using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class JobListSettings : JobSettings
{
    [Description("Output results in raw format versus pretty table.")]
    [CommandOption("--raw")]
    [DefaultValue(false)]
    public bool ReturnRaw { get; init; }
}

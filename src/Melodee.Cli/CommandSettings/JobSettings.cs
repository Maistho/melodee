using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class JobSettings : Spectre.Console.Cli.CommandSettings
{
    [Description("Use this value for any batch size, overwriting default batch size in configuration.")]
    [CommandOption("-b|--batchsize")]
    public int? BatchSize { get; init; }

    [Description("Output verbose debug and timing results to console. (default: False)")]
    [CommandOption("--verbose")]
    [DefaultValue(false)]
    public bool Verbose { get; init; }
}

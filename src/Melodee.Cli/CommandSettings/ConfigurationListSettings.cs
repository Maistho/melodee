using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class ConfigurationListSettings : ConfigurationSettings
{
    [Description("Output results in JSON format.")]
    [CommandOption("--raw")]
    [DefaultValue(false)]
    public bool ReturnRaw { get; init; }

    [Description("Filter settings by key pattern (supports wildcards like 'imaging.*').")]
    [CommandOption("-f|--filter")]
    public string? Filter { get; init; }
}

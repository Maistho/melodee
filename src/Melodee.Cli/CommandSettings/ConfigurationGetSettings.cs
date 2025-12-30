using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class ConfigurationGetSettings : ConfigurationSettings
{
    [Description("Key of configuration setting to retrieve.")]
    [CommandArgument(0, "<KEY>")]
    [Required]
    public string Key { get; init; } = string.Empty;

    [Description("Output only the value without formatting.")]
    [CommandOption("--raw")]
    [DefaultValue(false)]
    public bool ReturnRaw { get; init; }
}

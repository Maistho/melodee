using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class ConfigurationSetSetting : ConfigurationSettings
{
    [Description("Remove configuration setting.")]
    [CommandOption("--remove")]
    [DefaultValue(false)]
    public bool Remove { get; init; }

    [Description("Key of configuration setting to modify.")]
    [CommandArgument(0, "[KEY]")]
    [Required]
    public string Key { get; init; } = string.Empty;

    [Description("New value of configuration setting")]
    [CommandArgument(0, "[VALUE]")]
    [Required]
    public string Value { get; init; } = string.Empty;
}

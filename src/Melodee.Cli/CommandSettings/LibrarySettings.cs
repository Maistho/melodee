using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class LibrarySettings : Spectre.Console.Cli.CommandSettings
{
    [Description("Name of library to process.")]
    [CommandOption("--library|-l")]
    public string? LibraryName { get; set; }

    [Description("Output verbose debug and timing results to console.")]
    [CommandOption("--verbose")]
    [DefaultValue(true)]
    public bool Verbose { get; set; }
}

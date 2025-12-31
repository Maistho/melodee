using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class DoctorSettings : Spectre.Console.Cli.CommandSettings
{
    [Description("Output results in JSON format for scripting.")]
    [CommandOption("--raw")]
    [DefaultValue(false)]
    public bool ReturnRaw { get; init; }

    [Description("Output verbose debug and timing results to console. (default: False)")]
    [CommandOption("--verbose")]
    [DefaultValue(false)]
    public bool Verbose { get; init; }

    [Description("Perform a non-destructive write test in library directories (create+delete a temp file).")]
    [CommandOption("--write-test")]
    [DefaultValue(false)]
    public bool WriteTest { get; init; }
}

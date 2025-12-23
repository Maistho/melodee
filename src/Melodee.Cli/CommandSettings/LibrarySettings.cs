using System.ComponentModel;
using Spectre.Console.Cli;
using ValidationResult = Spectre.Console.ValidationResult;

namespace Melodee.Cli.CommandSettings;

public class LibrarySettings : Spectre.Console.Cli.CommandSettings
{
    [Description("Name of library to process.")]
    [CommandArgument(0, "[NAME]")]
    public string LibraryName { get; set; } = string.Empty;

    [Description("Output verbose debug and timing results to console.")]
    [CommandOption("--verbose")]
    [DefaultValue(true)]
    public bool Verbose { get; set; }

    public override ValidationResult Validate()
    {
        if (string.IsNullOrEmpty(LibraryName))
        {
            return ValidationResult.Error("Library name is required.");
        }

        return ValidationResult.Success();
    }
}

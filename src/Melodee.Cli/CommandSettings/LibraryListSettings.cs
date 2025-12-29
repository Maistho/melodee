using System.ComponentModel;
using Spectre.Console.Cli;
using ValidationResult = Spectre.Console.ValidationResult;

namespace Melodee.Cli.CommandSettings;

public class LibraryListSettings : LibrarySettings
{
    [Description("Output results in raw format versus pretty table.")]
    [CommandOption("--raw")]
    [DefaultValue(false)]
    public bool ReturnRaw { get; init; }

    public override ValidationResult Validate()
    {
        // List command doesn't require a library name
        return ValidationResult.Success();
    }
}

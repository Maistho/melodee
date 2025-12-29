using System.ComponentModel;
using Spectre.Console.Cli;
using ValidationResult = Spectre.Console.ValidationResult;

namespace Melodee.Cli.CommandSettings;

public class LibraryScanSettings : LibrarySettings
{
    [Description("Ignore last scan at date on Library.")]
    [CommandOption("--force")]
    [DefaultValue(false)]
    public bool ForceMode { get; init; }

    public override ValidationResult Validate()
    {
        if (string.IsNullOrEmpty(LibraryName))
        {
            return ValidationResult.Error("Library name is required. Use --library or -l to specify the library.");
        }

        return ValidationResult.Success();
    }
}

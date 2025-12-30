using System.ComponentModel;
using Spectre.Console.Cli;
using ValidationResult = Spectre.Console.ValidationResult;

namespace Melodee.Cli.CommandSettings;

/// <summary>
///     Settings for the library validate command.
/// </summary>
public class LibraryValidateSettings : LibrarySettings
{
    [Description("Output in JSON format for scripting. (default: False)")]
    [CommandOption("--json")]
    [DefaultValue(false)]
    public bool Json { get; init; }

    [Description("Fix issues by removing orphaned database records. (default: False)")]
    [CommandOption("--fix")]
    [DefaultValue(false)]
    public bool Fix { get; init; }

    public override ValidationResult Validate()
    {
        if (string.IsNullOrEmpty(LibraryName))
        {
            return ValidationResult.Error("Library name is required. Use --library or -l to specify the library.");
        }

        return ValidationResult.Success();
    }
}

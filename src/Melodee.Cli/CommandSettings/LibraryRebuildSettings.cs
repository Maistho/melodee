using System.ComponentModel;
using Spectre.Console.Cli;
using ValidationResult = Spectre.Console.ValidationResult;

namespace Melodee.Cli.CommandSettings;

public class LibraryRebuildSettings : LibrarySettings
{
    [Description("Only create missing Melodee data files, when false, will recreate all files.")]
    [CommandOption("--only-missing")]
    [DefaultValue(true)]
    public bool CreateOnlyMissing { get; init; }

    [Description("Rebuild only this library path.")]
    [CommandArgument(0, "[only-path]")]
    public string? OnlyPath { get; init; }

    public override ValidationResult Validate()
    {
        if (string.IsNullOrEmpty(LibraryName))
        {
            return ValidationResult.Error("Library name is required. Use --library or -l to specify the library.");
        }

        return ValidationResult.Success();
    }
}

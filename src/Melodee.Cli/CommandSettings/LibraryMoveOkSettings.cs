using System.ComponentModel;
using Spectre.Console.Cli;
using ValidationResult = Spectre.Console.ValidationResult;

namespace Melodee.Cli.CommandSettings;

public class LibraryMoveOkSettings : LibrarySettings
{
    [Description("Name of library to move 'Ok' albums into.")]
    [CommandOption("--to-library")]
    public string? ToLibraryName { get; set; }

    [Description("Source path containing albums to move (use with --to-path for path-based mode, bypasses database library lookup).")]
    [CommandOption("--from-path")]
    [DefaultValue(null)]
    public string? FromPath { get; set; }

    [Description("Destination path to move 'Ok' albums into (use with --from-path for path-based mode, bypasses database library lookup).")]
    [CommandOption("--to-path")]
    [DefaultValue(null)]
    public string? ToPath { get; set; }

    public bool IsPathBasedMode => !string.IsNullOrEmpty(FromPath) && !string.IsNullOrEmpty(ToPath);

    public override ValidationResult Validate()
    {
        if (IsPathBasedMode)
        {
            if (!Directory.Exists(FromPath))
            {
                return ValidationResult.Error($"From path does not exist: {FromPath}");
            }

            if (!Directory.Exists(ToPath))
            {
                return ValidationResult.Error($"To path does not exist: {ToPath}");
            }

            if (string.Equals(FromPath, ToPath, StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.Error("From path and To path cannot be the same");
            }

            return ValidationResult.Success();
        }

        if (!string.IsNullOrEmpty(FromPath) || !string.IsNullOrEmpty(ToPath))
        {
            return ValidationResult.Error("Both --from-path and --to-path must be provided together for path-based mode");
        }

        if (string.IsNullOrEmpty(LibraryName))
        {
            return ValidationResult.Error("Library name is required. Use --library or -l to specify the library (or use --from-path and --to-path for path-based mode)");
        }

        if (string.IsNullOrEmpty(ToLibraryName))
        {
            return ValidationResult.Error("To library name is required. Use --to-library to specify the destination library (or use --from-path and --to-path for path-based mode)");
        }

        return ValidationResult.Success();
    }
}

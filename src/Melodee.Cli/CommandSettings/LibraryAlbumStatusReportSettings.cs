using System.ComponentModel;
using Spectre.Console.Cli;
using ValidationResult = Spectre.Console.ValidationResult;

namespace Melodee.Cli.CommandSettings;

public class LibraryAlbumStatusReportSettings : LibrarySettings
{
    [Description("Show full report.")]
    [CommandOption("--full")]
    [DefaultValue(false)]
    public bool Full { get; init; }

    [Description("Output results (where applicable) in raw format versus pretty table.")]
    [CommandOption("--raw")]
    [DefaultValue(false)]
    public bool ReturnRaw { get; init; }

    public override ValidationResult Validate()
    {
        if (string.IsNullOrEmpty(LibraryName))
        {
            return ValidationResult.Error("Library name is required. Use --library or -l to specify the library.");
        }

        return ValidationResult.Success();
    }
}

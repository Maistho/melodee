using System.ComponentModel;
using Spectre.Console.Cli;
using ValidationResult = Spectre.Console.ValidationResult;

namespace Melodee.Cli.CommandSettings;

public class LibraryProcessSettings : LibrarySettings
{
    [Description("Copy or move files from library. If set then processed files are not deleted.")]
    [CommandOption("--copy")]
    [DefaultValue(true)]
    public bool CopyMode { get; init; }

    [Description("Override any existing Melodee data files.")]
    [CommandOption("--force")]
    [DefaultValue(true)]
    public bool ForceMode { get; init; }

    [Description("Maximum number of albums to process and then quit, null is unlimited.")]
    [CommandOption("--limit")]
    [DefaultValue(null)]
    public int? ProcessLimit { get; init; }

    [Description("Script to run before Processing.")]
    [CommandOption("--pre-script")]
    [DefaultValue(null)]
    public string? PreDiscoveryScript { get; set; }

    [Description("Inbound path to process (use with --staging for path-based mode, bypasses database library lookup).")]
    [CommandOption("--inbound")]
    [DefaultValue(null)]
    public string? InboundPath { get; set; }

    [Description("Staging path for processed output (use with --inbound for path-based mode, bypasses database library lookup).")]
    [CommandOption("--staging")]
    [DefaultValue(null)]
    public string? StagingPath { get; set; }

    public bool IsPathBasedMode => !string.IsNullOrEmpty(InboundPath) && !string.IsNullOrEmpty(StagingPath);

    public override ValidationResult Validate()
    {
        if (IsPathBasedMode)
        {
            if (!Directory.Exists(InboundPath))
            {
                return ValidationResult.Error($"Inbound path does not exist: {InboundPath}");
            }

            if (!Directory.Exists(StagingPath))
            {
                return ValidationResult.Error($"Staging path does not exist: {StagingPath}");
            }

            return ValidationResult.Success();
        }

        if (!string.IsNullOrEmpty(InboundPath) || !string.IsNullOrEmpty(StagingPath))
        {
            return ValidationResult.Error("Both --inbound and --staging must be provided together for path-based mode");
        }

        if (string.IsNullOrEmpty(LibraryName))
        {
            return ValidationResult.Error("Library name is required. Use --library or -l to specify the library (or use --inbound and --staging for path-based mode)");
        }

        return ValidationResult.Success();
    }
}

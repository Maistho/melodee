using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

/// <summary>
///     Settings for the library scan command which performs the full ingestion pipeline.
/// </summary>
public class LibraryScanSettings : Spectre.Console.Cli.CommandSettings
{
    [Description("Ignore last scan timestamps and force full processing.")]
    [CommandOption("--force")]
    [DefaultValue(false)]
    public bool ForceMode { get; init; }

    [Description("Output verbose debug and timing results to console. (default: False)")]
    [CommandOption("--verbose")]
    [DefaultValue(false)]
    public bool Verbose { get; set; }

    [Description("Suppress all output (silent mode).")]
    [CommandOption("--silent")]
    [DefaultValue(false)]
    public bool Silent { get; set; }

    [Description("Output results as JSON (implies --silent for progress display).")]
    [CommandOption("--json")]
    [DefaultValue(false)]
    public bool Json { get; set; }
}

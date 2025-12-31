using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class AlbumImageIssuesSettings : AlbumSettings
{
    [Description("Include albums with invalid images (wrong size, not square, etc.).")]
    [CommandOption("--invalid")]
    [DefaultValue(true)]
    public bool IncludeInvalid { get; init; }

    [Description("Maximum number of results to return.")]
    [CommandOption("-n|--limit")]
    [DefaultValue(100)]
    public int Limit { get; init; }

    [Description("Include albums with incorrectly numbered images.")]
    [CommandOption("--misnumbered")]
    [DefaultValue(true)]
    public bool IncludeMisnumbered { get; init; }

    [Description("Include albums with missing images.")]
    [CommandOption("--missing")]
    [DefaultValue(true)]
    public bool IncludeMissing { get; init; }

    [Description("Output results in JSON format.")]
    [CommandOption("--raw")]
    [DefaultValue(false)]
    public bool ReturnRaw { get; init; }
}

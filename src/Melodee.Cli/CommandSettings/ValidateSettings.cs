using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class ValidateSettings : Spectre.Console.Cli.CommandSettings
{
    [Description("ApiKey of Album to Validate.")]
    [CommandOption("--apiKey")]
    public string? ApiKey { get; init; }

    [Description("ApiKey of Artist to Validate all albums for.")]
    [CommandOption("--artistApiKey")]
    public string? ArtistApiKey { get; init; }

    [Description("Path to Melodee Data File (melodee.json) file to Validate.")]
    [CommandOption("--file")]
    public string? PathToMelodeeDataFile { get; init; }

    [Description("Id of Melodee Data File (melodee.json) file to validate.")]
    [CommandOption("--id")]
    public Guid? Id { get; init; }

    [Description("Name of Library.")]
    [CommandOption("--library")]
    public string? LibraryName { get; init; }

    [Description("Output verbose debug and timing results to console. (default: False)")]
    [CommandOption("--verbose")]
    [DefaultValue(false)]
    public bool Verbose { get; init; }

    [Description("Output result as JSON.")]
    [CommandOption("-j|--json")]
    public bool? Json { get; init; }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class ArtistDeleteSettings : ArtistSettings
{
    [Description("Artist ID to delete.")]
    [CommandArgument(0, "<ID>")]
    [Required]
    public int ArtistId { get; init; }

    [Description("Keep the artist directory on disk (do not delete files).")]
    [CommandOption("--keep-files")]
    [DefaultValue(false)]
    public bool KeepFiles { get; init; }

    [Description("Skip confirmation prompt.")]
    [CommandOption("-y|--yes")]
    [DefaultValue(false)]
    public bool SkipConfirmation { get; init; }
}

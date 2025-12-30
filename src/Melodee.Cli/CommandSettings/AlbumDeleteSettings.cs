using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class AlbumDeleteSettings : AlbumSettings
{
    [Description("Album ID to delete.")]
    [CommandArgument(0, "<ID>")]
    [Required]
    public int AlbumId { get; init; }

    [Description("Keep the album directory on disk (do not delete files).")]
    [CommandOption("--keep-files")]
    [DefaultValue(false)]
    public bool KeepFiles { get; init; }

    [Description("Skip confirmation prompt.")]
    [CommandOption("-y|--yes")]
    [DefaultValue(false)]
    public bool SkipConfirmation { get; init; }
}

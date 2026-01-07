using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class UserDeleteSettings : UserSettings
{
    [Description("User ID to delete.")]
    [CommandArgument(0, "<ID>")]
    [Required]
    public int UserId { get; init; }

    [Description("Skip confirmation prompt.")]
    [CommandOption("-y|--yes")]
    [DefaultValue(false)]
    public bool SkipConfirmation { get; init; }
}

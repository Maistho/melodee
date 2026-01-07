using System.ComponentModel;
using Spectre.Console.Cli;
using ValidationResult = Spectre.Console.ValidationResult;

namespace Melodee.Cli.CommandSettings;

public class UserCreateSettings : UserSettings
{
    [Description("Username for the new user.")]
    [CommandOption("-u|--username")]
    public string Username { get; init; } = string.Empty;

    [Description("Email address for the new user.")]
    [CommandOption("-e|--email")]
    public string Email { get; init; } = string.Empty;

    [Description("Password for the new user.")]
    [CommandOption("-p|--password")]
    public string Password { get; init; } = string.Empty;

    [Description("Force creation even if user already exists (deletes and recreates user).")]
    [CommandOption("-f|--force")]
    [DefaultValue(false)]
    public bool Force { get; init; }

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            return ValidationResult.Error("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            return ValidationResult.Error("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            return ValidationResult.Error("Password is required.");
        }

        if (Password.Length < 8)
        {
            return ValidationResult.Error("Password must be at least 8 characters.");
        }

        return ValidationResult.Success();
    }
}

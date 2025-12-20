namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Request model for confirming a password reset with a new password.
/// </summary>
/// <param name="Token">The password reset token received via email/magic link.</param>
/// <param name="NewPassword">The new password to set.</param>
public record PasswordResetConfirmRequest(string Token, string NewPassword);

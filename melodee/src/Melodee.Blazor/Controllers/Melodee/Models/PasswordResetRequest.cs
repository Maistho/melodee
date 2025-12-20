namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Request model for initiating a password reset.
/// </summary>
/// <param name="Email">The email address of the user requesting password reset.</param>
public record PasswordResetRequest(string Email);

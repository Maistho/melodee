namespace Melodee.Blazor.Services.Email;

/// <summary>
/// Email sending abstraction for Melodee application.
/// Supports both plain text and HTML email content.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="textBody">Plain text email body</param>
    /// <param name="htmlBody">HTML email body (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if email was sent successfully, false otherwise</returns>
    Task<bool> SendAsync(
        string toEmail,
        string subject,
        string textBody,
        string? htmlBody = null,
        CancellationToken cancellationToken = default);
}

using MailKit.Net.Smtp;
using MailKit.Security;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Utility;
using MimeKit;
using ILogger = Serilog.ILogger;

namespace Melodee.Blazor.Services.Email;

/// <summary>
/// SMTP email sender implementation using MailKit.
/// Supports SSL/TLS connections and template-based email content.
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly ILogger _logger;
    private readonly IMelodeeConfigurationFactory _configurationFactory;

    public SmtpEmailSender(ILogger logger, IMelodeeConfigurationFactory configurationFactory)
    {
        _logger = logger;
        _configurationFactory = configurationFactory;
    }

    public async Task<bool> SendAsync(
        string toEmail,
        string subject,
        string textBody,
        string? htmlBody = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await _configurationFactory.GetConfigurationAsync(cancellationToken);

            // Check if email is enabled
            var emailEnabled = config.GetValue<bool?>(SettingRegistry.EmailEnabled) ?? false;
            if (!emailEnabled)
            {
                _logger.Warning("Email sending is disabled. Email would have been sent to: {MaskedEmail}",
                    LogSanitizer.MaskEmail(toEmail));
                return false;
            }

            // Get SMTP configuration
            var fromName = config.GetValue<string>(SettingRegistry.EmailFromName) ?? "Melodee";
            var fromEmail = config.GetValue<string>(SettingRegistry.EmailFromEmail);
            var smtpHost = config.GetValue<string>(SettingRegistry.EmailSmtpHost);
            var smtpPort = config.GetValue<int?>(SettingRegistry.EmailSmtpPort) ?? 587;
            var smtpUsername = config.GetValue<string>(SettingRegistry.EmailSmtpUsername);
            var smtpPassword = config.GetValue<string>(SettingRegistry.EmailSmtpPassword);
            var smtpUseSsl = config.GetValue<bool?>(SettingRegistry.EmailSmtpUseSsl) ?? false;
            var smtpUseStartTls = config.GetValue<bool?>(SettingRegistry.EmailSmtpUseStartTls) ?? true;

            // Validate required settings
            if (string.IsNullOrWhiteSpace(fromEmail) || string.IsNullOrWhiteSpace(smtpHost))
            {
                _logger.Warning("Email configuration incomplete. Missing required settings (email.fromEmail or email.smtpHost)");
                return false;
            }

            // Create message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(string.Empty, toEmail));
            message.Subject = subject;

            // Build message body
            var bodyBuilder = new BodyBuilder
            {
                TextBody = textBody
            };

            if (!string.IsNullOrWhiteSpace(htmlBody))
            {
                bodyBuilder.HtmlBody = htmlBody;
            }

            message.Body = bodyBuilder.ToMessageBody();

            // Send email using MailKit
            using var client = new SmtpClient();

            try
            {
                // Determine security options
                var secureSocketOptions = smtpUseSsl
                    ? SecureSocketOptions.SslOnConnect
                    : (smtpUseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

                await client.ConnectAsync(smtpHost, smtpPort, secureSocketOptions, cancellationToken);

                // Authenticate if credentials provided
                if (!string.IsNullOrWhiteSpace(smtpUsername) && !string.IsNullOrWhiteSpace(smtpPassword))
                {
                    await client.AuthenticateAsync(smtpUsername, smtpPassword, cancellationToken);
                }

                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.Information("Email sent successfully to: {MaskedEmail}, Subject: {Subject}",
                    LogSanitizer.MaskEmail(toEmail),
                    LogSanitizer.Sanitize(subject));

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "SMTP error sending email. To: {MaskedEmail}, From: {MaskedFrom}, Host: {Host}, Port: {Port}, SSL: {SSL}, StartTLS: {StartTLS}, Exception: {ExceptionType}, Message: {Message}",
                    LogSanitizer.MaskEmail(toEmail),
                    LogSanitizer.MaskEmail(fromEmail),
                    smtpHost,
                    smtpPort,
                    smtpUseSsl,
                    smtpUseStartTls,
                    ex.GetType().Name,
                    ex.Message);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to send email to: {MaskedEmail}. Exception: {ExceptionType}",
                LogSanitizer.MaskEmail(toEmail),
                ex.GetType().Name);
            return false;
        }
    }
}

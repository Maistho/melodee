using Melodee.Blazor.Services.Email;
using Melodee.Common.Constants;
using Moq;
using Serilog;

namespace Melodee.Tests.Blazor.Services;

/// <summary>
/// Tests for SMTP email sender service.
/// Verifies configuration handling and error scenarios.
/// </summary>
public class SmtpEmailSenderTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IMelodeeConfigurationFactory> _mockConfigFactory;
    private readonly Mock<IMelodeeConfiguration> _mockConfig;

    public SmtpEmailSenderTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockConfigFactory = new Mock<IMelodeeConfigurationFactory>();
        _mockConfig = new Mock<IMelodeeConfiguration>();

        _mockConfigFactory.Setup(f => f.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConfig.Object);
    }

    [Fact]
    public async Task SendAsync_WhenEmailDisabled_ReturnsFalse()
    {
        // Arrange
        _mockConfig.Setup(c => c.GetValue<bool?>(SettingRegistry.EmailEnabled)).Returns(false);

        var sender = new SmtpEmailSender(_mockLogger.Object, _mockConfigFactory.Object);

        // Act
        var result = await sender.SendAsync("test@example.com", "Test", "Body");

        // Assert
        Assert.False(result);

        // Verify warning was logged
        _mockLogger.Verify(
            l => l.Warning(
                It.Is<string>(s => s.Contains("Email sending is disabled")),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_WhenFromEmailMissing_ReturnsFalse()
    {
        // Arrange
        _mockConfig.Setup(c => c.GetValue<bool?>(SettingRegistry.EmailEnabled)).Returns(true);
        _mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.EmailFromEmail)).Returns((string?)null);
        _mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.EmailSmtpHost)).Returns("smtp.example.com");

        var sender = new SmtpEmailSender(_mockLogger.Object, _mockConfigFactory.Object);

        // Act
        var result = await sender.SendAsync("test@example.com", "Test", "Body");

        // Assert
        Assert.False(result);

        // Verify warning about missing configuration
        _mockLogger.Verify(
            l => l.Warning(It.Is<string>(s => s.Contains("configuration incomplete"))),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_WhenSmtpHostMissing_ReturnsFalse()
    {
        // Arrange
        _mockConfig.Setup(c => c.GetValue<bool?>(SettingRegistry.EmailEnabled)).Returns(true);
        _mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.EmailFromEmail)).Returns("sender@example.com");
        _mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.EmailSmtpHost)).Returns((string?)null);

        var sender = new SmtpEmailSender(_mockLogger.Object, _mockConfigFactory.Object);

        // Act
        var result = await sender.SendAsync("test@example.com", "Test", "Body");

        // Assert
        Assert.False(result);

        // Verify warning about missing configuration
        _mockLogger.Verify(
            l => l.Warning(It.Is<string>(s => s.Contains("configuration incomplete"))),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_DoesNotLogSensitiveData()
    {
        // Arrange
        _mockConfig.Setup(c => c.GetValue<bool?>(SettingRegistry.EmailEnabled)).Returns(false);

        var sender = new SmtpEmailSender(_mockLogger.Object, _mockConfigFactory.Object);
        var sensitiveEmail = "sensitive@example.com";

        // Act
        await sender.SendAsync(sensitiveEmail, "Test", "Body with secret token");

        // Assert - verify email is masked in logs
        _mockLogger.Verify(
            l => l.Warning(
                It.IsAny<string>(),
                It.Is<object[]>(args => args.All(a => a == null || a.ToString() != sensitiveEmail))),
            Times.AtLeastOnce);
    }
}

/// <summary>
/// Tests for email template service.
/// Verifies template rendering and variable substitution.
/// </summary>
public class EmailTemplateServiceTests
{
    private readonly Mock<IMelodeeConfigurationFactory> _mockConfigFactory;
    private readonly Mock<IMelodeeConfiguration> _mockConfig;

    public EmailTemplateServiceTests()
    {
        _mockConfigFactory = new Mock<IMelodeeConfigurationFactory>();
        _mockConfig = new Mock<IMelodeeConfiguration>();

        _mockConfigFactory.Setup(f => f.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockConfig.Object);

        _mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.SystemBaseUrl))
            .Returns("https://melodee.test");
    }

    [Fact]
    public async Task RenderPasswordResetEmailAsync_ReplacesResetUrl()
    {
        // Arrange
        var service = new EmailTemplateService(_mockConfigFactory.Object);
        var resetUrl = "https://melodee.test/reset?token=abc123";

        // Act
        var (subject, textBody, htmlBody) = await service.RenderPasswordResetEmailAsync(resetUrl, 60);

        // Assert
        Assert.Contains(resetUrl, textBody);
        Assert.Contains(resetUrl, htmlBody);
    }

    [Fact]
    public async Task RenderPasswordResetEmailAsync_ReplacesExpiryMinutes()
    {
        // Arrange
        var service = new EmailTemplateService(_mockConfigFactory.Object);
        var expiryMinutes = 120;

        // Act
        var (subject, textBody, htmlBody) = await service.RenderPasswordResetEmailAsync("https://test.com", expiryMinutes);

        // Assert
        Assert.Contains("120", textBody);
        Assert.Contains("120", htmlBody);
    }

    [Fact]
    public async Task RenderPasswordResetEmailAsync_UsesCustomTemplateFromSettings()
    {
        // Arrange
        var customSubject = "Custom Reset Subject";
        var customTextTemplate = "Custom text with {resetUrl} and {expiryMinutes}";
        var customHtmlTemplate = "<html>{resetUrl} expires in {expiryMinutes}</html>";

        _mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.EmailResetPasswordSubject))
            .Returns(customSubject);
        _mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.EmailResetPasswordTextBodyTemplate))
            .Returns(customTextTemplate);
        _mockConfig.Setup(c => c.GetValue<string>(SettingRegistry.EmailResetPasswordHtmlBodyTemplate))
            .Returns(customHtmlTemplate);

        var service = new EmailTemplateService(_mockConfigFactory.Object);

        // Act
        var (subject, textBody, htmlBody) = await service.RenderPasswordResetEmailAsync("https://test.com/reset", 60);

        // Assert
        Assert.Equal(customSubject, subject);
        Assert.Contains("https://test.com/reset", textBody);
        Assert.Contains("60", textBody);
        Assert.Contains("https://test.com/reset", htmlBody);
        Assert.Contains("60", htmlBody);
    }
}

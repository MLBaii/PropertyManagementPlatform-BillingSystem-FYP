using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PropertyBill.Api.Services;

// SMTP via MailKit, configured under the "Smtp" section (see appsettings.json). Real
// credentials live only in the gitignored appsettings.Development.json locally.
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string residentName, string resetToken)
    {
        var smtp = _configuration.GetSection("Smtp");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtp["FromName"], smtp["FromEmail"]!));
        message.To.Add(new MailboxAddress(residentName, toEmail));
        message.Subject = "Reset your PropertyBill password";

        var encodedName = WebUtility.HtmlEncode(residentName);
        var encodedToken = WebUtility.HtmlEncode(resetToken);
        message.Body = new BodyBuilder
        {
            TextBody =
                $"Hi {residentName},\n\n" +
                $"Your PropertyBill password reset code is:\n\n{resetToken}\n\n" +
                "This code expires in 30 minutes. Enter it in the app to set a new password.\n\n" +
                "If you didn't request this, you can safely ignore this email.\n\n" +
                "— PropertyBill",
            HtmlBody =
                $"<p>Hi {encodedName},</p>" +
                "<p>Your PropertyBill password reset code is:</p>" +
                $"<p style=\"font-size:22px;font-weight:bold;letter-spacing:2px;\">{encodedToken}</p>" +
                "<p>This code expires in 30 minutes. Enter it in the app to set a new password.</p>" +
                "<p>If you didn't request this, you can safely ignore this email.</p>" +
                "<p>— PropertyBill</p>",
        }.ToMessageBody();

        var secureSocketOptions = smtp.GetValue("EnableTls", true) ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(smtp["Host"]!, smtp.GetValue<int>("Port"), secureSocketOptions);
            await client.AuthenticateAsync(smtp["Username"]!, smtp["Password"]!);
            await client.SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
            throw;
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}

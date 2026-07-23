namespace PropertyBill.Api.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string residentName, string resetToken);
}

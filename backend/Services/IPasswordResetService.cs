namespace PropertyBill.Api.Services;

public interface IPasswordResetService
{
    // Never throws and never reveals whether the email matched a resident — the caller
    // always shows the same generic message regardless of what happens in here.
    Task RequestResetAsync(string email);

    Task<PasswordResetResult> ResetPasswordAsync(string token, string newPassword);
}

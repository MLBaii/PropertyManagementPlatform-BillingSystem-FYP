namespace PropertyBill.Api.Services;

public enum PasswordResetResultStatus
{
    Success,
    InvalidOrExpiredToken,
}

public class PasswordResetResult
{
    public PasswordResetResultStatus Status { get; set; }

    public static PasswordResetResult Success() => new() { Status = PasswordResetResultStatus.Success };

    public static PasswordResetResult InvalidOrExpiredToken() =>
        new() { Status = PasswordResetResultStatus.InvalidOrExpiredToken };
}

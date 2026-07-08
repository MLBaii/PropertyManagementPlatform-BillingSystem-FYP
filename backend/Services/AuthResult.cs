using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public enum AuthResultStatus
{
    Success,
    InvalidCredentials,
    AccountDisabled,
}

public class AuthResult
{
    public AuthResultStatus Status { get; set; }
    public LoginResponse? Response { get; set; }

    public static AuthResult Success(LoginResponse response) =>
        new() { Status = AuthResultStatus.Success, Response = response };

    public static AuthResult InvalidCredentials() =>
        new() { Status = AuthResultStatus.InvalidCredentials };

    public static AuthResult AccountDisabled() =>
        new() { Status = AuthResultStatus.AccountDisabled };
}

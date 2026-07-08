using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public enum ProfileResultStatus
{
    Success,
    NotFound,
    EmailTaken,
}

public class ProfileResult
{
    public ProfileResultStatus Status { get; set; }
    public ProfileDto? Profile { get; set; }

    public static ProfileResult Success(ProfileDto profile) =>
        new() { Status = ProfileResultStatus.Success, Profile = profile };

    public static ProfileResult NotFound() => new() { Status = ProfileResultStatus.NotFound };

    public static ProfileResult EmailTaken() => new() { Status = ProfileResultStatus.EmailTaken };
}

public enum ChangePasswordResultStatus
{
    Success,
    NotFound,
    InvalidCurrentPassword,
}

public class ChangePasswordResult
{
    public ChangePasswordResultStatus Status { get; set; }

    public static ChangePasswordResult Success() => new() { Status = ChangePasswordResultStatus.Success };

    public static ChangePasswordResult NotFound() => new() { Status = ChangePasswordResultStatus.NotFound };

    public static ChangePasswordResult InvalidCurrentPassword() =>
        new() { Status = ChangePasswordResultStatus.InvalidCurrentPassword };
}

using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public enum DisputeSubmitStatus
{
    Success,
    BillNotFound,
    ActiveDisputeExists,
}

public class DisputeSubmitResult
{
    public DisputeSubmitStatus Status { get; private set; }
    public DisputeDto? Dispute { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static DisputeSubmitResult Success(DisputeDto dispute) =>
        new() { Status = DisputeSubmitStatus.Success, Dispute = dispute };

    public static DisputeSubmitResult BillNotFound() =>
        new()
        {
            Status = DisputeSubmitStatus.BillNotFound,
            ErrorMessage = "Bill not found.",
        };

    public static DisputeSubmitResult ActiveDisputeExists(DisputeDto existing) =>
        new()
        {
            Status = DisputeSubmitStatus.ActiveDisputeExists,
            Dispute = existing,
            ErrorMessage = "This bill already has an active dispute. Please wait for it to be resolved.",
        };
}

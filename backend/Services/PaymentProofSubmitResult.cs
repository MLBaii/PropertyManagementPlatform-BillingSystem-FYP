using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public enum PaymentProofSubmitStatus
{
    Success,
    InvalidFile,
    NoBillsTagged,
    BillsNotFound,
    StorageUploadFailed,
}

public class PaymentProofSubmitResult
{
    public PaymentProofSubmitStatus Status { get; private set; }
    public PaymentProofDto? Proof { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static PaymentProofSubmitResult Success(PaymentProofDto proof) =>
        new() { Status = PaymentProofSubmitStatus.Success, Proof = proof };

    public static PaymentProofSubmitResult InvalidFile() =>
        new()
        {
            Status = PaymentProofSubmitStatus.InvalidFile,
            ErrorMessage = "File must be JPG, PNG, or PDF and not exceed 5 MB.",
        };

    public static PaymentProofSubmitResult NoBillsTagged() =>
        new()
        {
            Status = PaymentProofSubmitStatus.NoBillsTagged,
            ErrorMessage = "Select at least one bill to tag this proof to.",
        };

    public static PaymentProofSubmitResult BillsNotFound() =>
        new()
        {
            Status = PaymentProofSubmitStatus.BillsNotFound,
            ErrorMessage = "One or more selected bills could not be found.",
        };

    public static PaymentProofSubmitResult StorageUploadFailed() =>
        new()
        {
            Status = PaymentProofSubmitStatus.StorageUploadFailed,
            ErrorMessage = "Could not upload the file. Please try again.",
        };
}

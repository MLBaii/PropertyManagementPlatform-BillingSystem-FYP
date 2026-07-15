using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public enum PaymentProofSubmitStatus
{
    Success,
    NoFiles,
    TooManyFiles,
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

    public static PaymentProofSubmitResult NoFiles() =>
        new()
        {
            Status = PaymentProofSubmitStatus.NoFiles,
            ErrorMessage = "Attach at least one file.",
        };

    public static PaymentProofSubmitResult TooManyFiles() =>
        new()
        {
            Status = PaymentProofSubmitStatus.TooManyFiles,
            ErrorMessage = "You can upload up to 3 files.",
        };

    public static PaymentProofSubmitResult InvalidFile() =>
        new()
        {
            Status = PaymentProofSubmitStatus.InvalidFile,
            ErrorMessage = "Each file must be JPG, PNG, or PDF and not exceed 5 MB.",
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

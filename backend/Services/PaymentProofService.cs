using PropertyBill.Api.Dtos;
using PropertyBill.Api.Models;
using PropertyBill.Api.Repositories;

namespace PropertyBill.Api.Services;

public class PaymentProofService : IPaymentProofService
{
    // Extension check is a cheap second signal alongside Content-Type — both are
    // client-supplied and therefore spoofable, but requiring both to agree at least rules
    // out the trivial "rename a .exe to .jpg" case. Full magic-byte sniffing is out of scope.
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "application/pdf",
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".pdf",
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private readonly IPaymentProofRepository _paymentProofRepository;
    private readonly ISupabaseStorageService _storageService;

    public PaymentProofService(IPaymentProofRepository paymentProofRepository, ISupabaseStorageService storageService)
    {
        _paymentProofRepository = paymentProofRepository;
        _storageService = storageService;
    }

    public async Task<PaymentProofSubmitResult> SubmitAsync(int residentId, int unitId, IFormFile? file, List<int> billIds)
    {
        if (!IsValidFile(file))
        {
            return PaymentProofSubmitResult.InvalidFile();
        }

        var distinctBillIds = billIds.Distinct().ToList();
        if (distinctBillIds.Count == 0)
        {
            return PaymentProofSubmitResult.NoBillsTagged();
        }

        var taggedBills = await _paymentProofRepository.GetBillsByIdsForUnitAsync(distinctBillIds, unitId);
        if (taggedBills.Count != distinctBillIds.Count)
        {
            return PaymentProofSubmitResult.BillsNotFound();
        }

        string fileUrl;
        try
        {
            fileUrl = await _storageService.UploadAsync(file!, residentId);
        }
        catch (Exception)
        {
            return PaymentProofSubmitResult.StorageUploadFailed();
        }

        var proof = new PaymentProof
        {
            ResidentId = residentId,
            FileUrl = fileUrl,
            FileType = file!.ContentType,
            FileSize = file.Length,
            Status = "Pending",
            SubmittedAt = DateTime.UtcNow,
        };

        var created = await _paymentProofRepository.CreateAsync(proof, taggedBills);
        return PaymentProofSubmitResult.Success(ToDto(created, taggedBills));
    }

    public async Task<List<PaymentProofDto>> GetHistoryAsync(int residentId)
    {
        var proofs = await _paymentProofRepository.GetByResidentIdAsync(residentId);
        return proofs.Select(p => ToDto(p, null)).ToList();
    }

    private static bool IsValidFile(IFormFile? file)
    {
        if (file is null || file.Length == 0 || file.Length > MaxFileSizeBytes)
        {
            return false;
        }

        var extension = Path.GetExtension(file.FileName);
        return AllowedContentTypes.Contains(file.ContentType) && AllowedExtensions.Contains(extension);
    }

    // `taggedBills` is passed straight through on the create path (already loaded, avoids a
    // redundant query); on the history path it's null and TaggedBills comes from `proof.Payments`
    // (Included by the repository) instead.
    private static PaymentProofDto ToDto(PaymentProof proof, List<Bill>? taggedBills)
    {
        return new PaymentProofDto
        {
            ProofId = proof.ProofId,
            FileUrl = proof.FileUrl,
            FileType = proof.FileType,
            FileSize = proof.FileSize,
            Status = proof.Status,
            AdminRemarks = proof.AdminRemarks,
            SubmittedAt = proof.SubmittedAt,
            ReviewedAt = proof.ReviewedAt,
            TaggedBills = taggedBills is not null
                ? taggedBills.Select(b => new TaggedBillDto
                {
                    BillId = b.BillId,
                    ReferenceNumber = b.ReferenceNumber,
                    BillingPeriod = b.BillingPeriod,
                    Amount = b.OutstandingBalance,
                }).ToList()
                : proof.Payments.Select(p => new TaggedBillDto
                {
                    BillId = p.BillId,
                    ReferenceNumber = p.Bill.ReferenceNumber,
                    BillingPeriod = p.Bill.BillingPeriod,
                    Amount = p.Amount,
                }).ToList(),
        };
    }
}

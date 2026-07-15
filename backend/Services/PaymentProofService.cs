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
    private const int MaxFileCount = 3;

    private readonly IPaymentProofRepository _paymentProofRepository;
    private readonly ISupabaseStorageService _storageService;
    private readonly ILogger<PaymentProofService> _logger;

    public PaymentProofService(
        IPaymentProofRepository paymentProofRepository,
        ISupabaseStorageService storageService,
        ILogger<PaymentProofService> logger)
    {
        _paymentProofRepository = paymentProofRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<PaymentProofSubmitResult> SubmitAsync(int residentId, int unitId, List<IFormFile>? files, List<int> billIds)
    {
        if (files is null || files.Count == 0)
        {
            return PaymentProofSubmitResult.NoFiles();
        }

        if (files.Count > MaxFileCount)
        {
            return PaymentProofSubmitResult.TooManyFiles();
        }

        if (!files.All(IsValidFile))
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

        // Upload every file before creating any DB rows — if file 2 of 3 fails, we don't want
        // a partially-created submission (matches the "no partial writes" guarantee the
        // single-file version already had).
        var uploaded = new List<(string Url, string Type, long Size)>();
        try
        {
            foreach (var file in files)
            {
                var url = await _storageService.UploadAsync(file, residentId);
                uploaded.Add((url, file.ContentType, file.Length));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment proof upload failed for resident {ResidentId} ({FileCount} file(s))", residentId, files.Count);
            return PaymentProofSubmitResult.StorageUploadFailed();
        }

        // One PaymentProof row per file (fits the ERD's single FileUrl/FileType/FileSize
        // columns with no migration) sharing one SubmittedAt instant, captured once here —
        // GetHistoryAsync groups rows by exact SubmittedAt equality to reconstruct "this
        // submission" without adding a batch/group column the ERD doesn't have.
        var submittedAt = DateTime.UtcNow;
        var proofs = uploaded.Select(u => new PaymentProof
        {
            ResidentId = residentId,
            FileUrl = u.Url,
            FileType = u.Type,
            FileSize = u.Size,
            Status = "Pending",
            SubmittedAt = submittedAt,
        }).ToList();

        var created = await _paymentProofRepository.CreateAsync(proofs, taggedBills);
        return PaymentProofSubmitResult.Success(ToDtoFromCreate(created, taggedBills));
    }

    public async Task<List<PaymentProofDto>> GetHistoryAsync(int residentId)
    {
        var proofs = await _paymentProofRepository.GetByResidentIdAsync(residentId);
        return proofs
            .GroupBy(p => p.SubmittedAt)
            .OrderByDescending(g => g.Key)
            .Select(g => ToDtoFromHistoryGroup(g.ToList()))
            .ToList();
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
    // redundant query and doesn't depend on EF's navigation-fixup timing).
    private static PaymentProofDto ToDtoFromCreate(List<PaymentProof> proofs, List<Bill> taggedBills)
    {
        var primary = proofs[0];
        return new PaymentProofDto
        {
            ProofId = primary.ProofId,
            Files = proofs.Select(ToFileDto).ToList(),
            Status = primary.Status,
            AdminRemarks = primary.AdminRemarks,
            SubmittedAt = primary.SubmittedAt,
            ReviewedAt = primary.ReviewedAt,
            TaggedBills = taggedBills.Select(b => new TaggedBillDto
            {
                BillId = b.BillId,
                ReferenceNumber = b.ReferenceNumber,
                BillingPeriod = b.BillingPeriod,
                Amount = b.OutstandingBalance,
            }).ToList(),
        };
    }

    // On the history path, TaggedBills come from the primary proof's Payments (Included by
    // the repository) — only the primary (first-created) row in a submission is linked to
    // Payment rows, per CreateAsync.
    private static PaymentProofDto ToDtoFromHistoryGroup(List<PaymentProof> groupProofs)
    {
        var ordered = groupProofs.OrderBy(p => p.ProofId).ToList();
        var primary = ordered[0];
        return new PaymentProofDto
        {
            ProofId = primary.ProofId,
            Files = ordered.Select(ToFileDto).ToList(),
            Status = primary.Status,
            AdminRemarks = primary.AdminRemarks,
            SubmittedAt = primary.SubmittedAt,
            ReviewedAt = primary.ReviewedAt,
            TaggedBills = primary.Payments.Select(p => new TaggedBillDto
            {
                BillId = p.BillId,
                ReferenceNumber = p.Bill.ReferenceNumber,
                BillingPeriod = p.Bill.BillingPeriod,
                Amount = p.Amount,
            }).ToList(),
        };
    }

    private static PaymentProofFileDto ToFileDto(PaymentProof p) => new()
    {
        ProofId = p.ProofId,
        FileUrl = p.FileUrl,
        FileType = p.FileType,
        FileSize = p.FileSize,
    };
}

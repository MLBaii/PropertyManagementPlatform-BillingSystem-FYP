namespace PropertyBill.Api.Dtos;

public class AdminPaymentProofDto
{
    public int ProofId { get; set; }
    public string ResidentName { get; set; } = string.Empty;
    public string UnitNumber { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int FileCount { get; set; }
    public List<AdminPaymentProofFileDto> Files { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string? AdminRemarks { get; set; }
    public bool HasOpenDisputes { get; set; }
    public List<AdminPaymentProofBillDto> Bills { get; set; } = new();
}

public class AdminPaymentProofFileDto
{
    public int ProofId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
}

public class AdminPaymentProofBillDto
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

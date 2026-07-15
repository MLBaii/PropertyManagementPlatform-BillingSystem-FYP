namespace PropertyBill.Api.Dtos;

public class PaymentProofFileDto
{
    public int ProofId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}

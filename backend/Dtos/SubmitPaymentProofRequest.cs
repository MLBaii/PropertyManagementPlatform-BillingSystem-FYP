namespace PropertyBill.Api.Dtos;

public class SubmitPaymentProofRequest
{
    public List<IFormFile> Files { get; set; } = new();
    public List<int> BillIds { get; set; } = new();
}

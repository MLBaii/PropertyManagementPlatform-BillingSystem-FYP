namespace PropertyBill.Api.Dtos;

public class SubmitPaymentProofRequest
{
    public IFormFile File { get; set; } = null!;
    public List<int> BillIds { get; set; } = new();
}

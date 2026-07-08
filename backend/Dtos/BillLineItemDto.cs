namespace PropertyBill.Api.Dtos;

public class BillLineItemDto
{
    public int LineItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string LineItemType { get; set; } = string.Empty;
}

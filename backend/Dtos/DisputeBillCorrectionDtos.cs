using System.ComponentModel.DataAnnotations;

namespace PropertyBill.Api.Dtos;

public class DisputeBillCorrectionDto
{
    public int BillId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public bool CanAdjust { get; set; }
    public string? AdjustmentBlockedReason { get; set; }
    public List<DisputeBillLineItemDto> LineItems { get; set; } = [];
}

public class DisputeBillLineItemDto
{
    public int LineItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string LineItemType { get; set; } = string.Empty;
}

public class UpdateDisputeBillLineItemRequest
{
    [Required, StringLength(250)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999.99")]
    public decimal Amount { get; set; }
}

public class AddDisputeBillLineItemRequest
{
    [Required, StringLength(250)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999.99")]
    public decimal Amount { get; set; }
}

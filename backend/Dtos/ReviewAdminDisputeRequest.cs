using System.ComponentModel.DataAnnotations;

namespace PropertyBill.Api.Dtos;

public class ReviewAdminDisputeRequest
{
    [Required, RegularExpression("^(UnderReview|Resolved|Rejected)$")]
    public string Status { get; set; } = string.Empty;

    [StringLength(500)]
    public string? AdminResponse { get; set; }
}

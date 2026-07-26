using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Dtos;
using PropertyBill.Api.Services;
namespace PropertyBill.Api.Controllers;
[ApiController]
[Authorize(Roles="Admin,AdminManager")]
[Route("api/admin/payment-proofs")]
public class AdminPaymentProofsController(AppDbContext context, INotificationSendingService notificationSending):ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminPaymentProofDto>>> Get()
    {
        var proofs = await context.PaymentProofs.AsNoTracking()
            .Include(proof => proof.Resident).ThenInclude(resident => resident.Unit)
            .Include(proof => proof.Payments).ThenInclude(payment => payment.Bill).ThenInclude(bill => bill.Disputes)
            .OrderByDescending(proof => proof.SubmittedAt)
            .ToListAsync();

        var submissions = proofs.GroupBy(proof => new { proof.ResidentId, proof.SubmittedAt })
            .OrderByDescending(group => group.Key.SubmittedAt)
            .Select(group =>
            {
                var primary = group.OrderBy(proof => proof.ProofId).First();
                return new AdminPaymentProofDto
                {
                    ProofId = primary.ProofId,
                    ResidentName = primary.Resident.Name,
                    UnitNumber = primary.Resident.Unit.UnitNumber,
                    FileUrl = primary.FileUrl,
                    FileType = primary.FileType,
                    FileCount = group.Count(),
                    Files = group.OrderBy(proof => proof.ProofId).Select(proof => new AdminPaymentProofFileDto
                    {
                        ProofId = proof.ProofId,
                        FileUrl = proof.FileUrl,
                        FileType = proof.FileType
                    }).ToList(),
                    Status = primary.Status,
                    SubmittedAt = primary.SubmittedAt,
                    AdminRemarks = primary.AdminRemarks,
                    HasOpenDisputes = group.SelectMany(proof => proof.Payments).Any(payment => payment.Bill.Disputes.Any(dispute => dispute.Status is "Open" or "UnderReview")),
                    Bills = group.SelectMany(proof => proof.Payments).Select(payment => new AdminPaymentProofBillDto
                    {
                        ReferenceNumber = payment.Bill.ReferenceNumber,
                        BillingPeriod = payment.Bill.BillingPeriod,
                        Amount = payment.Amount
                    }).ToList()
                };
            }).ToList();

        return Ok(submissions);
    }
    [HttpPut("{proofId:int}/review")]
    public async Task<IActionResult> Review(int proofId,[FromBody]ReviewPaymentProofRequest request)
    {
        var proof=await context.PaymentProofs.Include(item=>item.Payments).ThenInclude(payment=>payment.Bill).ThenInclude(bill=>bill.Disputes).FirstOrDefaultAsync(item=>item.ProofId==proofId); if(proof is null)return NotFound(); if(proof.Status!="Pending")return Conflict(new{message="This payment proof has already been reviewed."});
        if(request.Decision=="Confirmed"&&proof.Payments.Any(payment=>payment.Bill.Disputes.Any(dispute=>dispute.Status is "Open" or "UnderReview")))return Conflict(new{message="This payment cannot be confirmed while one of its bills has an open dispute. Review and close the dispute first."});
        var relatedProofs=await context.PaymentProofs.Where(item=>item.ResidentId==proof.ResidentId&&item.SubmittedAt==proof.SubmittedAt).ToListAsync();
        foreach(var relatedProof in relatedProofs){relatedProof.Status=request.Decision;relatedProof.AdminRemarks=request.AdminRemarks?.Trim();relatedProof.ReviewedAt=DateTime.UtcNow;}
        foreach(var payment in proof.Payments){payment.Status=request.Decision;if(request.Decision=="Confirmed"){var confirmedAmount=payment.Bill.Payments.Where(item=>item.Status=="Confirmed").Sum(item=>item.Amount);payment.Bill.OutstandingBalance=Math.Max(0m,payment.Bill.TotalAmount-confirmedAmount);payment.Bill.Status=payment.Bill.OutstandingBalance==0m?"Paid":"Unpaid";}else{payment.Bill.Status="Unpaid";}}
        if(int.TryParse(User.FindFirst("AdminUserId")?.Value,out var adminId))context.AuditLogs.Add(new PropertyBill.Api.Models.AuditLog{AdminUserId=adminId,ActionType="Review",AffectedEntity="PaymentProof",AffectedEntityId=proofId,Description=$"Payment proof {proofId} marked {request.Decision}."});
        await context.SaveChangesAsync();

        var billReferences = proof.Payments.Select(payment => payment.Bill.ReferenceNumber).Distinct().ToList();
        var billText = billReferences.Count == 1 ? billReferences[0] : $"{billReferences.Count} bills";
        if (request.Decision == "Confirmed")
            await notificationSending.SendAsync(proof.ResidentId, "PaymentConfirmed", "Payment confirmed", $"Your payment proof for {billText} has been confirmed.", $"/(tabs)/bills/{proof.Payments.FirstOrDefault()?.BillId}");
        else
            await notificationSending.SendAsync(proof.ResidentId, "PaymentRejected", "Payment proof needs attention", $"Your payment proof for {billText} was rejected. Please review the manager's remarks and submit a new proof if needed.", $"/(tabs)/bills/{proof.Payments.FirstOrDefault()?.BillId}");

        return NoContent();
    }
}

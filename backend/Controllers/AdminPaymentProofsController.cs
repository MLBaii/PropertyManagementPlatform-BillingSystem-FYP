using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Dtos;
namespace PropertyBill.Api.Controllers;
[ApiController]
[Authorize(Roles="Admin")]
[Route("api/admin/payment-proofs")]
public class AdminPaymentProofsController(AppDbContext context):ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminPaymentProofDto>>> Get()
    {
        var proofs = await context.PaymentProofs.AsNoTracking()
            .Include(proof => proof.Resident).ThenInclude(resident => resident.Unit)
            .Include(proof => proof.Payments).ThenInclude(payment => payment.Bill)
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
                    Status = primary.Status,
                    SubmittedAt = primary.SubmittedAt,
                    AdminRemarks = primary.AdminRemarks,
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
        var proof=await context.PaymentProofs.Include(item=>item.Payments).ThenInclude(payment=>payment.Bill).FirstOrDefaultAsync(item=>item.ProofId==proofId); if(proof is null)return NotFound(); if(proof.Status!="Pending")return Conflict(new{message="This payment proof has already been reviewed."});
        var relatedProofs=await context.PaymentProofs.Where(item=>item.ResidentId==proof.ResidentId&&item.SubmittedAt==proof.SubmittedAt).ToListAsync();
        foreach(var relatedProof in relatedProofs){relatedProof.Status=request.Decision;relatedProof.AdminRemarks=request.AdminRemarks?.Trim();relatedProof.ReviewedAt=DateTime.UtcNow;}
        foreach(var payment in proof.Payments){payment.Status=request.Decision;if(request.Decision=="Confirmed"){payment.Bill.Status="Paid";payment.Bill.OutstandingBalance=0m;}else{payment.Bill.Status="Unpaid";}}
        if(int.TryParse(User.FindFirst("AdminUserId")?.Value,out var adminId))context.AuditLogs.Add(new PropertyBill.Api.Models.AuditLog{AdminUserId=adminId,ActionType="Review",AffectedEntity="PaymentProof",AffectedEntityId=proofId,Description=$"Payment proof {proofId} marked {request.Decision}."});
        await context.SaveChangesAsync();return NoContent();
    }
}

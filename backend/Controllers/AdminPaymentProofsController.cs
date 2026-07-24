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
    public async Task<ActionResult<IEnumerable<AdminPaymentProofDto>>> Get() => Ok(await context.PaymentProofs.AsNoTracking().Include(proof=>proof.Resident).ThenInclude(resident=>resident.Unit).OrderByDescending(proof=>proof.SubmittedAt).Select(proof=>new AdminPaymentProofDto{ProofId=proof.ProofId,ResidentName=proof.Resident.Name,UnitNumber=proof.Resident.Unit.UnitNumber,FileUrl=proof.FileUrl,FileType=proof.FileType,FileSize=proof.FileSize,Status=proof.Status,SubmittedAt=proof.SubmittedAt,AdminRemarks=proof.AdminRemarks}).ToListAsync());
    [HttpPut("{proofId:int}/review")]
    public async Task<IActionResult> Review(int proofId,[FromBody]ReviewPaymentProofRequest request)
    {
        var proof=await context.PaymentProofs.Include(item=>item.Payments).ThenInclude(payment=>payment.Bill).FirstOrDefaultAsync(item=>item.ProofId==proofId); if(proof is null)return NotFound(); if(proof.Status!="Pending")return Conflict(new{message="This payment proof has already been reviewed."});
        proof.Status=request.Decision;proof.AdminRemarks=request.AdminRemarks?.Trim();proof.ReviewedAt=DateTime.UtcNow;
        foreach(var payment in proof.Payments){payment.Status=request.Decision;if(request.Decision=="Confirmed"){payment.Bill.Status="Paid";payment.Bill.OutstandingBalance=0m;}else{payment.Bill.Status="Unpaid";}}
        if(int.TryParse(User.FindFirst("AdminUserId")?.Value,out var adminId))context.AuditLogs.Add(new PropertyBill.Api.Models.AuditLog{AdminUserId=adminId,ActionType="Review",AffectedEntity="PaymentProof",AffectedEntityId=proofId,Description=$"Payment proof {proofId} marked {request.Decision}."});
        await context.SaveChangesAsync();return NoContent();
    }
}

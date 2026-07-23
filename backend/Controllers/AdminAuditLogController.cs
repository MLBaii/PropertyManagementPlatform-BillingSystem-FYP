using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Dtos;
namespace PropertyBill.Api.Controllers;
[ApiController]
[Authorize(Roles="Admin")]
[Route("api/admin/audit-log")]
public class AdminAuditLogController(AppDbContext context):ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminAuditLogDto>>> Get() => Ok(await context.AuditLogs.AsNoTracking().Include(log=>log.AdminUser).OrderByDescending(log=>log.Timestamp).Take(200).Select(log=>new AdminAuditLogDto{AuditLogId=log.AuditLogId,AdminUsername=log.AdminUser.Username,ActionType=log.ActionType,AffectedEntity=log.AffectedEntity,AffectedEntityId=log.AffectedEntityId,Description=log.Description,Timestamp=log.Timestamp}).ToListAsync());
}

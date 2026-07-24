using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Models;
namespace PropertyBill.Api.Repositories;
public class AdminUserRepository(AppDbContext context) : IAdminUserRepository { public Task<AdminUser?> GetByUsernameAsync(string username) => context.AdminUsers.FirstOrDefaultAsync(user => user.Username == username); }

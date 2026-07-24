using PropertyBill.Api.Models;
namespace PropertyBill.Api.Repositories;
public interface IAdminUserRepository { Task<AdminUser?> GetByUsernameAsync(string username); }

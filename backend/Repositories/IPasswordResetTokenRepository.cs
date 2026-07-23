using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public interface IPasswordResetTokenRepository
{
    Task CreateAsync(PasswordResetToken token);

    // Eager-loads Resident — callers need it to update Resident.PasswordHash in place.
    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash);

    Task<PasswordResetToken?> GetMostRecentForResidentAsync(int residentId);

    // Marks every other still-usable token for this resident as used, so a resident who
    // requests several reset emails in a row can't have an older one redeemed later.
    Task InvalidateOtherTokensForResidentAsync(int residentId, int exceptTokenId);

    Task SaveChangesAsync();
}

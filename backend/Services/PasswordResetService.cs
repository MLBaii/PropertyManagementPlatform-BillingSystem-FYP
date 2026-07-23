using System.Security.Cryptography;
using System.Text;
using PropertyBill.Api.Models;
using PropertyBill.Api.Repositories;

namespace PropertyBill.Api.Services;

public class PasswordResetService : IPasswordResetService
{
    private const int TokenValidityMinutes = 30;

    // A second, email-scoped throttle on top of the controller's per-IP rate limiting — a
    // resident (or anyone spamming their address) can't trigger a fresh email more than once
    // a minute, regardless of how many IPs the requests come from.
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);

    private readonly IResidentRepository _residentRepository;
    private readonly IPasswordResetTokenRepository _tokenRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(
        IResidentRepository residentRepository,
        IPasswordResetTokenRepository tokenRepository,
        IEmailService emailService,
        ILogger<PasswordResetService> logger)
    {
        _residentRepository = residentRepository;
        _tokenRepository = tokenRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task RequestResetAsync(string email)
    {
        var resident = await _residentRepository.GetByEmailAsync(email);
        if (resident is null || !resident.IsActive)
        {
            // No account, or a disabled one — say nothing distinguishable to the caller,
            // and don't even touch the token table or email service.
            return;
        }

        var recentToken = await _tokenRepository.GetMostRecentForResidentAsync(resident.ResidentId);
        if (recentToken is not null && DateTime.UtcNow - recentToken.CreatedAt < ResendCooldown)
        {
            return;
        }

        var rawToken = GenerateRawToken();
        var tokenHash = HashToken(rawToken);

        await _tokenRepository.CreateAsync(new PasswordResetToken
        {
            ResidentId = resident.ResidentId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(TokenValidityMinutes),
        });

        try
        {
            await _emailService.SendPasswordResetEmailAsync(resident.Email, resident.Name, rawToken);
        }
        catch (Exception ex)
        {
            // The token row is already committed — a delivery failure here doesn't roll it
            // back (SMTP issues are usually transient; the token just expires unused if the
            // resident never receives it) and, per the anti-enumeration requirement, must
            // never surface to the caller either way.
            _logger.LogError(ex, "Password reset email failed to send for resident {ResidentId}", resident.ResidentId);
        }
    }

    public async Task<PasswordResetResult> ResetPasswordAsync(string token, string newPassword)
    {
        var tokenHash = HashToken(token);
        var resetToken = await _tokenRepository.GetByTokenHashAsync(tokenHash);

        if (resetToken is null || resetToken.UsedAt is not null || resetToken.ExpiresAt < DateTime.UtcNow)
        {
            return PasswordResetResult.InvalidOrExpiredToken();
        }

        resetToken.Resident.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        resetToken.UsedAt = DateTime.UtcNow;

        // Save the used-flag + new password hash first, then set-based invalidate every
        // other outstanding token for this resident so an older reset email can't still be
        // redeemed after this one succeeds.
        await _tokenRepository.SaveChangesAsync();
        await _tokenRepository.InvalidateOtherTokensForResidentAsync(resetToken.ResidentId, resetToken.PasswordResetTokenId);

        return PasswordResetResult.Success();
    }

    // 32 bytes of CSPRNG output, base64url-encoded — long enough to make guessing
    // infeasible, short enough to copy-paste out of an email into the app.
    private static string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    // Only the hash is ever persisted — see PasswordResetToken.TokenHash.
    private static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}

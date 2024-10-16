using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces
{
 public interface IPasswordHashService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }

    public interface IJwtService
    {
        string GenerateToken(User user);
    }

    public interface IEmailService
    {
        Task SendPasswordRecoveryEmailAsync(string email, string token);
    }

    public interface ITokenBlacklistService
    {
        Task BlacklistTokenForUserAsync(Guid userId);
        Task<bool> IsTokenBlacklistedAsync(string token);
    }
}
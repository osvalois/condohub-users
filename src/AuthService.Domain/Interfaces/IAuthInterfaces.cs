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
        Task SendWelcomeEmailAsync(string email, string name);

    }
}
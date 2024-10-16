using AuthService.Domain.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace AuthService.Infrastructure.Services
{
    public class PasswordHashService : IPasswordHashService
    {
        public string HashPassword(string password)
        {
            return BC.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BC.Verify(password, hashedPassword);
        }
    }
}
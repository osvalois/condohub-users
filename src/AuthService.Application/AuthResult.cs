using AuthService.Domain.Entities;

namespace AuthService.Application.Auth
{
    public class AuthResult
    {
        public string Token { get; set; }
        public User User { get; set; }
    }
}
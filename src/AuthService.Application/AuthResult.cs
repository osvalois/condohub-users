using AuthService.Domain.Entities;

namespace AuthService.Application.Auth
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public Guid UserId { get; set; }
        public string? Message { get; set; }
    }
}
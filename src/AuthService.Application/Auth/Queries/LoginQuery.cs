using MediatR;

namespace AuthService.Application.Auth.Queries
{
    public class LoginQuery : IRequest<AuthResult>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
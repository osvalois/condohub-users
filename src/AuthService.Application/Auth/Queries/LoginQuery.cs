using MediatR;

namespace AuthService.Application.Auth.Queries
{
    public class LoginQuery : IRequest<AuthResult>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
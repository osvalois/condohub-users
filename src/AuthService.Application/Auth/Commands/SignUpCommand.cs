using MediatR;

namespace AuthService.Application.Auth.Commands
{
    public class SignUpCommand : IRequest<AuthResult>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DepartmentNumber { get; set; } = string.Empty;
    }
}
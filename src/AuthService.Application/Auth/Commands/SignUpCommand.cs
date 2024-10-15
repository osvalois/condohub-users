using MediatR;

namespace AuthService.Application.Auth.Commands
{
    public class SignUpCommand : IRequest<AuthResult>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string DepartmentNumber { get; set; }
    }
}
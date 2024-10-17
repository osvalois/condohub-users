using MediatR;

namespace AuthService.Application.Auth.Commands
{
    public class RecoverPasswordCommand : IRequest<bool>
    {
        public string Email { get; set; } = string.Empty;
    }
}
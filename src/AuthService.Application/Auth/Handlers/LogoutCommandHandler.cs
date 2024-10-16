using System.Threading;
using System.Threading.Tasks;
using AuthService.Application.Auth.Commands;
using AuthService.Domain.Interfaces;
using MediatR;

namespace AuthService.Application.Auth.Handlers
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
    {
        private readonly ITokenBlacklistService _tokenBlacklistService;

        public LogoutCommandHandler(ITokenBlacklistService tokenBlacklistService)
        {
            _tokenBlacklistService = tokenBlacklistService;
        }

        public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            // Aquí podrías invalidar el token actual
            await _tokenBlacklistService.BlacklistTokenForUserAsync(request.UserId);
            return true;
        }
    }
}
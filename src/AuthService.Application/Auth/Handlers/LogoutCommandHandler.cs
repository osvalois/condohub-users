using System;
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
            _tokenBlacklistService = tokenBlacklistService ?? throw new ArgumentNullException(nameof(tokenBlacklistService));
        }

        public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.UserId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(request));

            if (string.IsNullOrEmpty(request.Token))
                throw new ArgumentException("Token cannot be null or empty", nameof(request));

            // Blacklist the token
            await _tokenBlacklistService.BlacklistTokenForUserAsync(request.UserId, request.Token);
            return true;
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Application.Auth.Commands;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Interfaces;
using MediatR;

namespace AuthService.Application.Auth.Handlers
{
    public class RecoverPasswordCommandHandler : IRequestHandler<RecoverPasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IEmailService _emailService;

        public RecoverPasswordCommandHandler(IUserRepository userRepository, IPasswordHashService passwordHashService, IEmailService emailService)
        {
            _userRepository = userRepository;
            _passwordHashService = passwordHashService;
            _emailService = emailService;
        }

        public async Task<bool> Handle(RecoverPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                // We don't want to reveal that the email doesn't exist
                return true;
            }

            var newPassword = GenerateRandomPassword();
            user.PasswordHash = _passwordHashService.HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);

            await _emailService.SendPasswordRecoveryEmailAsync(user.Email, newPassword);

            return true;
        }

        private string GenerateRandomPassword()
        {
            // Implement a secure random password generation method
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        }
    }
}
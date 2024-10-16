using System;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Application.Auth.Queries;
using AuthService.Domain.Interfaces;
using MediatR;

namespace AuthService.Application.Auth.Handlers
{
    public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IJwtService _jwtService;

        public LoginQueryHandler(IUserRepository userRepository, IPasswordHashService passwordHashService, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _passwordHashService = passwordHashService;
            _jwtService = jwtService;
        }

        public async Task<AuthResult> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !_passwordHashService.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new Exception("Invalid email or password");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var token = _jwtService.GenerateToken(user);

            return new AuthResult
            {
                Token = token,
                User = user
            };
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Application.Auth.Queries;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Auth.Handlers
{
    public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<LoginQueryHandler> _logger;

        public LoginQueryHandler(
            IUserRepository userRepository,
            IPasswordHashService passwordHashService,
            IJwtService jwtService,
            ILogger<LoginQueryHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHashService = passwordHashService ?? throw new ArgumentNullException(nameof(passwordHashService));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthResult> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing login request for email: {Email}", request.Email);

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Login attempt with empty email or password");
                return AuthResult.FailureResult("Email and password are required.");
            }

            User user;
            try
            {
                user = await _userRepository.GetByEmailAsync(request.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user data for email: {Email}", request.Email);
                return AuthResult.FailureResult("An error occurred during the login process. Please try again.");
            }

            if (user == null || !_passwordHashService.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
                return AuthResult.FailureResult("Invalid email or password.");
            }

            try
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                var token = _jwtService.GenerateToken(user);

                _logger.LogInformation("Successful login for user: {UserId}", user.Id);

                return AuthResult.SuccessResult(token, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login process for user: {UserId}", user.Id);
                return AuthResult.FailureResult("An error occurred during the login process. Please try again.");
            }
        }
    }
}
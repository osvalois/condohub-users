using System;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Application.Auth.Commands;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Auth.Handlers
{
    public class SignUpCommandHandler : IRequestHandler<SignUpCommand, AuthResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ILogger<SignUpCommandHandler> _logger;

        public SignUpCommandHandler(
            IUserRepository userRepository,
            IPasswordHashService passwordHashService,
            IJwtService jwtService,
            IEmailService emailService,
            ILogger<SignUpCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHashService = passwordHashService ?? throw new ArgumentNullException(nameof(passwordHashService));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthResult> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return new AuthResult { Success = false, Message = "Email and password are required." };
                }

                // Check if user already exists
                var existingUser = await _userRepository.GetByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning($"Attempted to create an account with existing email: {request.Email}");
                    return new AuthResult { Success = false, Message = "User with this email already exists." };
                }

                // Create new user
                var newUser = new User(request.FirstName, request.LastName, request.Email, request.DepartmentNumber);
                newUser.PasswordHash = _passwordHashService.HashPassword(request.Password);

                // Save user to database
                await _userRepository.AddAsync(newUser);
                _logger.LogInformation($"New user created: {newUser.Id}");

                // Generate JWT token
                var token = _jwtService.GenerateToken(newUser);

                // Send welcome email
                try
                {
                    await _emailService.SendWelcomeEmailAsync(newUser.Email, newUser.FirstName);
                    _logger.LogInformation($"Welcome email sent to: {newUser.Email}");
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the sign-up process
                    _logger.LogError(ex, $"Failed to send welcome email to: {newUser.Email}");
                }

                return new AuthResult
                {
                    Success = true,
                    Token = token,
                    UserId = newUser.Id,
                    Message = "User successfully created."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user sign-up");
                return new AuthResult
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please try again later."
                };
            }
        }
    }
}
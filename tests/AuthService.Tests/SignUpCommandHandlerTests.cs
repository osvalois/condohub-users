using System;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Application.Auth.Commands;
using AuthService.Application.Auth.Handlers;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AuthService.Tests
{
    public class SignUpCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHashService> _passwordHashServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILogger<SignUpCommandHandler>> _loggerMock;

        public SignUpCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHashServiceMock = new Mock<IPasswordHashService>();
            _jwtServiceMock = new Mock<IJwtService>();
            _emailServiceMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILogger<SignUpCommandHandler>>();
        }

        [Fact]
        public async Task Handle_WithValidRequest_ShouldCreateUserAndReturnToken()
        {
            // Arrange
            var handler = new SignUpCommandHandler(
                _userRepositoryMock.Object,
                _passwordHashServiceMock.Object,
                _jwtServiceMock.Object,
                _emailServiceMock.Object,
                _loggerMock.Object);

            var command = new SignUpCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "StrongPassword123!",
                DepartmentNumber = "D001"
            };

            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            _passwordHashServiceMock.Setup(service => service.HashPassword(It.IsAny<string>()))
                .Returns("hashedPassword");

            _jwtServiceMock.Setup(service => service.GenerateToken(It.IsAny<User>()))
                .Returns("generatedToken");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("generatedToken", result.Token);
            Assert.NotEqual(Guid.Empty, result.UserId);

            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
            _emailServiceMock.Verify(service => service.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("New user created")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithExistingEmail_ShouldReturnFailureResult()
        {
            // Arrange
            var handler = new SignUpCommandHandler(
                _userRepositoryMock.Object,
                _passwordHashServiceMock.Object,
                _jwtServiceMock.Object,
                _emailServiceMock.Object,
                _loggerMock.Object);

            var command = new SignUpCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "existing@example.com",
                Password = "StrongPassword123!",
                DepartmentNumber = "D001"
            };

            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { Email = "existing@example.com" });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.Equal(Guid.Empty, result.UserId);

            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
            _emailServiceMock.Verify(service => service.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Attempted to create an account with existing email")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidEmail_ShouldReturnFailureResult()
        {
            // Arrange
            var handler = new SignUpCommandHandler(
                _userRepositoryMock.Object,
                _passwordHashServiceMock.Object,
                _jwtServiceMock.Object,
                _emailServiceMock.Object,
                _loggerMock.Object);

            var command = new SignUpCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "invalid-email",
                Password = "StrongPassword123!",
                DepartmentNumber = "D001"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.Equal(Guid.Empty, result.UserId);

            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
            _emailServiceMock.Verify(service => service.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Sign-up attempt with invalid email")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }
    }
}
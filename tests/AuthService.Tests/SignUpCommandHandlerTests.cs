using System;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Application.Auth.Queries;
using AuthService.Application.Auth.Handlers;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AuthService.Tests
{
    public class LoginQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHashService> _passwordHashServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<ILogger<LoginQueryHandler>> _loggerMock;

        public LoginQueryHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHashServiceMock = new Mock<IPasswordHashService>();
            _jwtServiceMock = new Mock<IJwtService>();
            _loggerMock = new Mock<ILogger<LoginQueryHandler>>();
        }

        [Fact]
        public async Task Handle_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var handler = new LoginQueryHandler(
                _userRepositoryMock.Object,
                _passwordHashServiceMock.Object,
                _jwtServiceMock.Object,
                _loggerMock.Object);

            var query = new LoginQuery
            {
                Email = "john@example.com",
                Password = "StrongPassword123!"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "john@example.com",
                PasswordHash = "hashedPassword"
            };

            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _passwordHashServiceMock.Setup(service => service.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            _jwtServiceMock.Setup(service => service.GenerateToken(It.IsAny<User>()))
                .Returns("generatedToken");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("generatedToken", result.Token);
            Assert.Equal(user.Id, result.UserId);

            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => string.Contains(o.ToString(), "User logged in successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidEmail_ShouldReturnFailureResult()
        {
            // Arrange
            var handler = new LoginQueryHandler(
                _userRepositoryMock.Object,
                _passwordHashServiceMock.Object,
                _jwtServiceMock.Object,
                _loggerMock.Object);

            var query = new LoginQuery
            {
                Email = "nonexistent@example.com",
                Password = "StrongPassword123!"
            };

            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.Equal(Guid.Empty, result.UserId);

            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => string.Contains(o.ToString(), "Login failed: User not found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidPassword_ShouldReturnFailureResult()
        {
            // Arrange
            var handler = new LoginQueryHandler(
                _userRepositoryMock.Object,
                _passwordHashServiceMock.Object,
                _jwtServiceMock.Object,
                _loggerMock.Object);

            var query = new LoginQuery
            {
                Email = "john@example.com",
                Password = "WrongPassword123!"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "john@example.com",
                PasswordHash = "hashedPassword"
            };

            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _passwordHashServiceMock.Setup(service => service.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.Equal(Guid.Empty, result.UserId);

            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => string.Contains(o.ToString(), "Login failed: Invalid password")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }
    }
}
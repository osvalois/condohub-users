using System;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Application.Auth.Commands;
using AuthService.Application.Auth.Handlers;
using AuthService.Application.Auth.Queries;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using AuthService.Application;

namespace AuthService.Tests
{
    public class SignUpCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WithValidRequest_ShouldCreateUserAndReturnToken()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var passwordHashServiceMock = new Mock<IPasswordHashService>();
            var jwtServiceMock = new Mock<IJwtService>();
            var emailServiceMock = new Mock<IEmailService>();

            var handler = new SignUpCommandHandler(
                userRepositoryMock.Object,
                passwordHashServiceMock.Object,
                jwtServiceMock.Object,
                emailServiceMock.Object);

            var command = new SignUpCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "StrongPassword123!",
                DepartmentNumber = "D001"
            };

            userRepositoryMock.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            passwordHashServiceMock.Setup(service => service.HashPassword(It.IsAny<string>()))
                .Returns("hashedPassword");

            jwtServiceMock.Setup(service => service.GenerateToken(It.IsAny<User>()))
                .Returns("generatedToken");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("generatedToken", result.Token);
            Assert.NotEqual(Guid.Empty, result.UserId);

            userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
            emailServiceMock.Verify(service => service.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }

    public class LoginQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var passwordHashServiceMock = new Mock<IPasswordHashService>();
            var jwtServiceMock = new Mock<IJwtService>();

            var handler = new LoginQueryHandler(
                userRepositoryMock.Object,
                passwordHashServiceMock.Object,
                jwtServiceMock.Object);

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

            userRepositoryMock.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            passwordHashServiceMock.Setup(service => service.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            jwtServiceMock.Setup(service => service.GenerateToken(It.IsAny<User>()))
                .Returns("generatedToken");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("generatedToken", result.Token);
            Assert.Equal(user.Id, result.UserId);

            userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
        }
    }

    public class PasswordHashServiceTests
    {
        [Fact]
        public void HashPassword_ShouldReturnDifferentHashForSamePassword()
        {
            // Arrange
            var configurationMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            configurationMock.Setup(c => c["Security:PasswordHashingIterations"]).Returns("10000");

            var service = new PasswordHashService(configurationMock.Object);
            var password = "StrongPassword123!";

            // Act
            var hash1 = service.HashPassword(password);
            var hash2 = service.HashPassword(password);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
        {
            // Arrange
            var configurationMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            configurationMock.Setup(c => c["Security:PasswordHashingIterations"]).Returns("10000");

            var service = new PasswordHashService(configurationMock.Object);
            var password = "StrongPassword123!";
            var hash = service.HashPassword(password);

            // Act
            var result = service.VerifyPassword(password, hash);

            // Assert
            Assert.True(result);
        }
    }
}
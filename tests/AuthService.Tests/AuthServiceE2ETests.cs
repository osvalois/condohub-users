using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AuthService.Api;
using AuthService.Application.Auth.Commands;
using AuthService.Application.Auth.Queries;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AuthService.Tests
{
    public class AuthServiceE2ETests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AuthServiceE2ETests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var query = new LoginQuery
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/token", query);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<LoginResult>();
            Assert.NotNull(content);
            Assert.NotNull(content.Token);
            Assert.NotEqual(Guid.Empty, content.UserId);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var query = new LoginQuery
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword123!"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/token", query);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Logout_WithValidToken_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var loginQuery = new LoginQuery
            {
                Email = "test@example.com",
                Password = "Password123!"
            };
            var loginResponse = await client.PostAsJsonAsync("/api/auth/token", loginQuery);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

            // Act
            var response = await client.PostAsync("/api/auth/logout", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<object>();
            Assert.Equal("Logout successful", content.GetType().GetProperty("message").GetValue(content, null));
        }

        [Fact]
        public async Task Logout_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsync("/api/auth/logout", null);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task SignUp_WithValidData_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var command = new SignUpCommand
            {
                Email = "newuser@example.com",
                Password = "NewPassword123!",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/signup", command);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<SignUpResult>();
            Assert.NotNull(content);
            Assert.NotNull(content.Token);
            Assert.NotEqual(Guid.Empty, content.UserId);
        }

        [Fact]
        public async Task SignUp_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            var command = new SignUpCommand
            {
                Email = "invalid-email",
                Password = "weak",
                FirstName = "",
                LastName = ""
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/signup", command);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task RecoverPassword_WithValidEmail_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var command = new RecoverPasswordCommand
            {
                Email = "test@example.com"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/recover", command);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<object>();
            Assert.Equal("If the email exists, a password recovery link has been sent.", content.GetType().GetProperty("message").GetValue(content, null));
        }

        [Fact]
        public async Task RecoverPassword_WithInvalidEmail_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var command = new RecoverPasswordCommand
            {
                Email = "nonexistent@example.com"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/recover", command);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<object>();
            Assert.Equal("If the email exists, a password recovery link has been sent.", content.GetType().GetProperty("message").GetValue(content, null));
        }
    }
}
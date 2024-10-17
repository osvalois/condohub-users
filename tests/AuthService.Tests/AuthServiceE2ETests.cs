using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AuthService.Api;
using AuthService.Application.Auth.Commands;
using AuthService.Application.Auth.Queries;
using AuthService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;

namespace AuthService.Tests.E2E
{
    public class AuthServiceE2ETests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly IServiceScope _scope;
        private readonly AuthDbContext _dbContext;

        public AuthServiceE2ETests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AuthDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<AuthDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForTesting");
                    });
                });
            });

            _scope = _factory.Services.CreateScope();
            _dbContext = _scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _scope.Dispose();
        }

        [Fact]
        public async Task FullUserJourney_RegisterAndLogin_ShouldSucceed()
        {
            // Arrange
            var client = _factory.CreateClient();

            var signUpCommand = new SignUpCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "StrongPassword123!",
                DepartmentNumber = "D001"
            };

            // Act - Register
            var signUpResponse = await client.PostAsJsonAsync("/api/auth/signup", signUpCommand);

            // Assert - Register
            signUpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var signUpResult = await signUpResponse.Content.ReadFromJsonAsync<AuthResult>();
            signUpResult.Should().NotBeNull();
            signUpResult.Success.Should().BeTrue();
            signUpResult.Token.Should().NotBeNullOrEmpty();
            signUpResult.UserId.Should().NotBe(Guid.Empty);

            // Act - Login
            var loginQuery = new LoginQuery
            {
                Email = "john.doe@example.com",
                Password = "StrongPassword123!"
            };
            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginQuery);

            // Assert - Login
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResult>();
            loginResult.Should().NotBeNull();
            loginResult.Success.Should().BeTrue();
            loginResult.Token.Should().NotBeNullOrEmpty();
            loginResult.UserId.Should().Be(signUpResult.UserId);

            // Act - Access Protected Resource
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {loginResult.Token}");
            var protectedResponse = await client.GetAsync("/api/protected");

            // Assert - Access Protected Resource
            protectedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task SignUp_WithExistingEmail_ShouldFail()
        {
            // Arrange
            var client = _factory.CreateClient();
            var existingUser = new SignUpCommand
            {
                FirstName = "Existing",
                LastName = "User",
                Email = "existing@example.com",
                Password = "ExistingPassword123!",
                DepartmentNumber = "D002"
            };

            // Act - Register existing user
            await client.PostAsJsonAsync("/api/auth/signup", existingUser);

            // Act - Try to register with the same email
            var signUpResponse = await client.PostAsJsonAsync("/api/auth/signup", existingUser);

            // Assert
            signUpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await signUpResponse.Content.ReadFromJsonAsync<AuthResult>();
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldFail()
        {
            // Arrange
            var client = _factory.CreateClient();
            var loginQuery = new LoginQuery
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword123!"
            };

            // Act
            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginQuery);

            // Assert
            loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var result = await loginResponse.Content.ReadFromJsonAsync<AuthResult>();
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid");
        }

        [Fact]
        public async Task AccessProtectedResource_WithoutToken_ShouldFail()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var protectedResponse = await client.GetAsync("/api/protected");

            // Assert
            protectedResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public Guid UserId { get; set; }
        public string Message { get; set; }
    }
}
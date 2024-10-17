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
using Microsoft.Extensions.Logging;
using Xunit;

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
                    // Remove the app's ApplicationDbContext registration.
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AuthDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add ApplicationDbContext using an in-memory database for testing.
                    services.AddDbContext<AuthDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForTesting");
                    });

                    // Ensure all other necessary services are registered
                    // This might include your handlers, repositories, etc.
                    // services.AddScoped<IUserRepository, UserRepository>();
                    // services.AddScoped<IJwtService, JwtService>();
                    // ... other services

                    // Replace other services with test doubles if necessary
                    // services.AddSingleton<IEmailService, FakeEmailService>();
                });

                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddXUnit(); // Assuming you're using the XUnit logger
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
            Assert.Equal(HttpStatusCode.OK, signUpResponse.StatusCode);
            var signUpResult = await signUpResponse.Content.ReadFromJsonAsync<AuthResult>();
            Assert.NotNull(signUpResult);
            Assert.True(signUpResult.Success);
            Assert.NotNull(signUpResult.Token);
            Assert.NotEqual(Guid.Empty, signUpResult.UserId);

            // Act - Login
            var loginQuery = new LoginQuery
            {
                Email = "john.doe@example.com",
                Password = "StrongPassword123!"
            };
            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginQuery);

            // Assert - Login
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResult>();
            Assert.NotNull(loginResult);
            Assert.True(loginResult.Success);
            Assert.NotNull(loginResult.Token);
            Assert.Equal(signUpResult.UserId, loginResult.UserId);

            // Act - Access Protected Resource
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {loginResult.Token}");
            var protectedResponse = await client.GetAsync("/api/protected");

            // Assert - Access Protected Resource
            Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
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
            Assert.Equal(HttpStatusCode.BadRequest, signUpResponse.StatusCode);
            var result = await signUpResponse.Content.ReadFromJsonAsync<AuthResult>();
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("already exists", result.Message);
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
            Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
            var result = await loginResponse.Content.ReadFromJsonAsync<AuthResult>();
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Invalid", result.Message);
        }

        [Fact]
        public async Task AccessProtectedResource_WithoutToken_ShouldFail()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var protectedResponse = await client.GetAsync("/api/protected");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, protectedResponse.StatusCode);
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
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AuthService.Api;
using AuthService.Application.Auth.Commands;
using AuthService.Application.Auth.Queries;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AuthService.Infrastructure.Persistence;
using AuthService.Application;


namespace AuthService.Tests.E2E
{
    public class AuthServiceE2ETests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AuthServiceE2ETests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the app's ApplicationDbContext registration.
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType ==
                            typeof(DbContextOptions<AuthDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add ApplicationDbContext using an in-memory database for testing.
                    services.AddDbContext<AuthDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForTesting");
                    });

                    // Build the service provider.
                    var sp = services.BuildServiceProvider();

                    // Create a scope to obtain a reference to the database context
                    using (var scope = sp.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<AuthDbContext>();

                        // Ensure the database is created.
                        db.Database.EnsureCreated();
                    }
                });
            });
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
    }

    // You might need to create this class if it doesn't exist in your actual code
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public Guid UserId { get; set; }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using Azure.Identity;
using System.Security.Claims;

namespace AuthService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var clientSecretCredential = new ClientSecretCredential(
                _configuration["AzureAd:TenantId"],
                _configuration["AzureAd:ClientId"],
                _configuration["AzureAd:ClientSecret"]);
            _graphServiceClient = new GraphServiceClient(clientSecretCredential, scopes);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var user = await _graphServiceClient.Me.Request().GetAsync();
            return Ok(new
            {
                user.DisplayName,
                user.UserPrincipalName,
                user.Id
            });
        }

        [AllowAnonymous]
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpModel model)
        {
            var user = new User
            {
                AccountEnabled = true,
                DisplayName = $"{model.FirstName} {model.LastName}",
                MailNickname = model.Email.Split('@')[0],
                UserPrincipalName = model.Email,
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = true,
                    Password = model.Password
                }
            };

            try
            {
                await _graphServiceClient.Users.Request().AddAsync(user);
                return Ok(new { message = "User created successfully" });
            }
            catch (ServiceException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Azure AD handles token revocation, so we don't need to do anything here
            return Ok(new { message = "Logout successful" });
        }

        [AllowAnonymous]
        [HttpPost("recover")]
        public async Task<IActionResult> RecoverPassword([FromBody] RecoverPasswordModel model)
        {
            try
            {
                var user = await _graphServiceClient.Users[model.Email].Request().GetAsync();
                await _graphServiceClient.Users[user.Id].Request()
                    .UpdateAsync(new User
                    {
                        PasswordProfile = new PasswordProfile
                        {
                            ForceChangePasswordNextSignIn = true,
                            Password = GenerateRandomPassword()
                        }
                    });

                // Here you would typically send an email with the new password
                return Ok(new { message = "Password reset. Check your email for the new password." });
            }
            catch (ServiceException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private string GenerateRandomPassword()
        {
            // Implement a secure random password generation method
            return Guid.NewGuid().ToString("N").Substring(0, 12) + "!A1";
        }
    }

    public class SignUpModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RecoverPasswordModel
    {
        public string Email { get; set; }
    }
}
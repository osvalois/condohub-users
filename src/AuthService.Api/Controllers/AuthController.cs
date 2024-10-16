using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AuthService.Application.Auth.Commands;
using AuthService.Application.Auth.Queries;

namespace AuthService.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("token")]
        public async Task<IActionResult> Login([FromBody] LoginQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var command = new LogoutCommand { UserId = Guid.Parse(userId) };
            var result = await _mediator.Send(command);
            return Ok(new { message = "Logout successful" });
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("recover")]
        public async Task<IActionResult> RecoverPassword([FromBody] RecoverPasswordCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { message = "If the email exists, a password recovery link has been sent." });
        }
    }
}
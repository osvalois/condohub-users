using MediatR;
using System;

namespace AuthService.Application.Auth.Commands
{
    public class LogoutCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
    }
}
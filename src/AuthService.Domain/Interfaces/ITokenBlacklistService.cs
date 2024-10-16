using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AuthService.Domain.Interfaces;

namespace AuthService.Domain.Interfaces
{
    public interface ITokenBlacklistService
    {
        Task AddToBlacklistAsync(string token, DateTime expirationTime);
        Task<bool> IsBlacklistedAsync(string token);
        Task RemoveExpiredTokensAsync();
        Task BlacklistTokenForUserAsync(Guid userId, string token);
    }
}
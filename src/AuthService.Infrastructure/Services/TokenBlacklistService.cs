using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AuthService.Domain.Interfaces;

namespace AuthService.Infrastructure.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new ConcurrentDictionary<string, DateTime>();
        private readonly ConcurrentDictionary<Guid, string> _userTokens = new ConcurrentDictionary<Guid, string>();

        public Task AddToBlacklistAsync(string token, DateTime expirationTime)
        {
            _blacklistedTokens.TryAdd(token, expirationTime);
            return Task.CompletedTask;
        }

        public Task<bool> IsBlacklistedAsync(string token)
        {
            return Task.FromResult(_blacklistedTokens.ContainsKey(token));
        }

        public Task RemoveExpiredTokensAsync()
        {
            var now = DateTime.UtcNow;
            foreach (var token in _blacklistedTokens)
            {
                if (token.Value <= now)
                {
                    _blacklistedTokens.TryRemove(token.Key, out _);
                }
            }
            return Task.CompletedTask;
        }

        public Task BlacklistTokenForUserAsync(Guid userId, string token)
        {
            _userTokens.AddOrUpdate(userId, token, (_, _) => token);
            return AddToBlacklistAsync(token, DateTime.UtcNow.AddDays(1)); // Blacklist for 1 day
        }
    }
}
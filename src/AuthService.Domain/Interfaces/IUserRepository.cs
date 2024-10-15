using System;
using System.Threading.Tasks;
using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(Guid id);
        Task<User> GetByEmailAsync(string email);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid id);
    }
}
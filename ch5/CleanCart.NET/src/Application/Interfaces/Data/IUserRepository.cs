using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces.Data;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task CreateUserAsync(User user);
}

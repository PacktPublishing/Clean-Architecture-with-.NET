using Domain.Entities;
using EntityAxis.Abstractions;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces.Data
{
    public interface IUserQueryRepository : IQueryService<User, Guid>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
    }
}
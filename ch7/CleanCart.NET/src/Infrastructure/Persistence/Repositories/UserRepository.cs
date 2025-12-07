using Application.Interfaces.Data;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UserRepository(IDbContextFactory<CoreDbContext> contextFactory, IMapper mapper) : RepositoryBase<CoreDbContext>(contextFactory, mapper), IUserRepository
{
    public async Task<User> GetByIdAsync(Guid userId)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();
        var sqlUser = await dbContext.Users.FindAsync(userId);
        return Mapper.Map<User>(sqlUser);
    }

    public async Task<List<User>> GetAllAsync()
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();
        var sqlUsers = await dbContext.Users.ToListAsync();
        return Mapper.Map<List<User>>(sqlUsers);
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();
        var sqlUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        return Mapper.Map<User>(sqlUser);
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();
        var sqlUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        return Mapper.Map<User>(sqlUser);
    }

    public async Task CreateUserAsync(User user)
    {
        var sqlUser = Mapper.Map<Entities.User>(user);
        var dbContext = await ContextFactory.CreateDbContextAsync();
        await dbContext.Users.AddAsync(sqlUser);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();
        var sqlUser = await dbContext.Users.FindAsync(user.Id);
        if (sqlUser != null)
        {
            Mapper.Map(user, sqlUser);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();
        var sqlUser = await dbContext.Users.FindAsync(userId);
        if (sqlUser != null)
        {
            dbContext.Users.Remove(sqlUser);
            await dbContext.SaveChangesAsync();
        }
    }
}

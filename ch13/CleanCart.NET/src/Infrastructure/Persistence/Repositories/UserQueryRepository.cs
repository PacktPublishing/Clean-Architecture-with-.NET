using Application.Interfaces.Data;
using AutoMapper;
using Domain.Entities;
using EntityAxis.EntityFramework;
using EntityAxis.KeyMappers;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UserQueryRepository(IDbContextFactory<CoreDbContext> contextFactory, IMapper mapper, IKeyMapper<Guid, Guid> keyMapper)
    : EntityFrameworkQueryService<User, Entities.User, CoreDbContext, Guid, Guid>(contextFactory, mapper, keyMapper), IUserQueryRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();
        var sqlUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        return Mapper.Map<User>(sqlUser);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();
        var sqlUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        return Mapper.Map<User>(sqlUser);
    }
}

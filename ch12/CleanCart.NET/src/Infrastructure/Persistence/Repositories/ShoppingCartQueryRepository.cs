using Application.Interfaces.Data;
using AutoMapper;
using Domain.Entities;
using EntityAxis.EntityFramework;
using EntityAxis.KeyMappers;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ShoppingCartQueryRepository(IDbContextFactory<CoreDbContext> contextFactory, IMapper mapper, IKeyMapper<Guid, Guid> keyMapper)
    : EntityFrameworkQueryService<ShoppingCart, Entities.ShoppingCart, CoreDbContext, Guid, Guid>(contextFactory, mapper, keyMapper), IShoppingCartQueryRepository
{
    public async Task<ShoppingCart?> GetByUserIdAsync(Guid userId)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();
        var sqlShoppingCart = await dbContext.ShoppingCarts.FirstOrDefaultAsync(sc => sc.UserId == userId);
        return Mapper.Map<ShoppingCart>(sqlShoppingCart);
    }
}

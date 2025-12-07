using Application.Interfaces.Data;
using AutoMapper;
using Domain.Entities;
using EntityAxis.EntityFramework;
using EntityAxis.KeyMappers;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ShoppingCartCommandRepository(IDbContextFactory<CoreDbContext> contextFactory, IMapper mapper, IKeyMapper<Guid, Guid> keyMapper)
    : EntityFrameworkCommandService<ShoppingCart, Entities.ShoppingCart, CoreDbContext, Guid, Guid>(contextFactory, mapper, keyMapper), IShoppingCartCommandRepository
{
    public async Task SaveAsync(ShoppingCart shoppingCart)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();

        var existingShoppingCart = await dbContext.ShoppingCarts.Include(x => x.Items).FirstOrDefaultAsync(sc => sc.Id == shoppingCart.Id);
        if (existingShoppingCart != null)
        {
            Mapper.Map(shoppingCart, existingShoppingCart);
        }
        else
        {
            var sqlShoppingCart = Mapper.Map<Entities.ShoppingCart>(shoppingCart);
            dbContext.ShoppingCarts.Add(sqlShoppingCart);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteByUserIdAsync(Guid userId)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();

        var sqlShoppingCart = await dbContext.ShoppingCarts.FirstOrDefaultAsync(sc => sc.UserId == userId);

        if (sqlShoppingCart != null)
        {
            dbContext.ShoppingCarts.Remove(sqlShoppingCart);
            await dbContext.SaveChangesAsync();
        }
    }
}

using Application.Interfaces.Data;
using AutoMapper;
using Domain.Entities;
using EntityAxis.EntityFramework;
using EntityAxis.KeyMappers;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class OrderCommandRepository(IDbContextFactory<CoreDbContext> contextFactory, IMapper mapper, IKeyMapper<Guid, Guid> keyMapper)
    : EntityFrameworkCommandService<Order, Entities.Order, CoreDbContext, Guid, Guid>(contextFactory, mapper, keyMapper), IOrderCommandRepository
{
    public async Task<Order> CreateOrderAsync(Order order)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();
        var sqlOrder = Mapper.Map<Entities.Order>(order);

        await dbContext.Orders.AddAsync(sqlOrder);
        await dbContext.SaveChangesAsync();

        return order;
    }
}

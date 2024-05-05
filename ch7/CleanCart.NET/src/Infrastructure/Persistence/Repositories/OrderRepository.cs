using Application.Interfaces.Data;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories;

public class OrderRepository(IDbContextFactory<CoreDbContext> contextFactory, IMapper mapper) : RepositoryBase<CoreDbContext>(contextFactory, mapper), IOrderRepository
{
    public async Task<Order> CreateOrderAsync(Order order)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();
        var sqlOrder = Mapper.Map<Entities.Order>(order);

        await dbContext.Orders.AddAsync(sqlOrder);
        await dbContext.SaveChangesAsync();

        return order;
    }

    public async Task UpdateOrderAsync(Order order)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();
        var sqlOrder = await dbContext.Orders.FindAsync(order.Id);
        if (sqlOrder != null)
        {
            Mapper.Map(order, sqlOrder);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();

        var sqlOrders = await dbContext.Orders
            .Where(o => o.UserId == userId)
            .ToListAsync();

        return Mapper.Map<IEnumerable<Order>>(sqlOrders);
    }
}
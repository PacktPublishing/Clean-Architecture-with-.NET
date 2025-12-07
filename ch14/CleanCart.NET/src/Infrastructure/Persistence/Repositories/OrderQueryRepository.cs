using Application.Interfaces.Data;
using AutoMapper;
using Domain.Entities;
using EntityAxis.EntityFramework;
using EntityAxis.KeyMappers;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;

namespace Infrastructure.Persistence.Repositories;

public class OrderQueryRepository(IDbContextFactory<CoreDbContext> contextFactory, IMapper mapper, IKeyMapper<Guid, Guid> keyMapper, ISystemClock systemClock)
    : EntityFrameworkQueryService<Order, Entities.Order, CoreDbContext, Guid, Guid>(contextFactory, mapper, keyMapper), IOrderQueryRepository
{
    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync(cancellationToken);

        var sqlOrders = await dbContext.Orders
            .Where(o => o.UserId == userId)
            .ToListAsync(cancellationToken);

        return Mapper.Map<IEnumerable<Order>>(sqlOrders);
    }

    public async Task<IEnumerable<Order>> GetRecentOrdersAsync(TimeSpan withinLast, CancellationToken cancellationToken = default)
    {
        var cutoffUtc = systemClock.UtcNow.DateTime.Subtract(withinLast);

        var dbContext = await ContextFactory.CreateDbContextAsync(cancellationToken);

        var sqlOrders = await dbContext.Orders
            .Where(o => o.CreatedOn >= cutoffUtc)
            .ToListAsync(cancellationToken);

        return Mapper.Map<IEnumerable<Order>>(sqlOrders);
    }
}
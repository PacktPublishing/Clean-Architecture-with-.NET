using Application.Interfaces.Data;
using AutoMapper;
using Domain.Entities;
using EntityAxis.EntityFramework;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntityAxis.KeyMappers;

namespace Infrastructure.Persistence.Repositories
{
    public class OrderQueryRepository(IDbContextFactory<CoreDbContext> contextFactory, IMapper mapper, IKeyMapper<Guid, Guid> keyMapper)
        : EntityFrameworkQueryService<Order, Entities.Order, CoreDbContext, Guid, Guid>(contextFactory, mapper, keyMapper), IOrderQueryRepository
    {
        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId)
        {
            var dbContext = await ContextFactory.CreateDbContextAsync();

            var sqlOrders = await dbContext.Orders
                .Where(o => o.UserId == userId)
                .ToListAsync();

            return Mapper.Map<IEnumerable<Order>>(sqlOrders);
        }
    }
}

using Domain.Entities;
using EntityAxis.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Data
{
    public interface IOrderQueryRepository : IQueryService<Order, Guid>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> GetRecentOrdersAsync(TimeSpan withinLast, CancellationToken cancellationToken = default);
    }
}

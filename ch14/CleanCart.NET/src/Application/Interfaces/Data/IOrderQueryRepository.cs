using Domain.Entities;
using EntityAxis.Abstractions;

namespace Application.Interfaces.Data;

public interface IOrderQueryRepository : IQueryService<Order, Guid>
{
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetRecentOrdersAsync(TimeSpan withinLast, CancellationToken cancellationToken = default);
}
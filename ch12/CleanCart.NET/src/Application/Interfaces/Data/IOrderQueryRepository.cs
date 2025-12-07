using Domain.Entities;
using EntityAxis.Abstractions;

namespace Application.Interfaces.Data;

public interface IOrderQueryRepository : IQueryService<Order, Guid>
{
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);
}
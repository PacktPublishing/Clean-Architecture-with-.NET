using Domain.Entities;
using EntityAxis.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Data
{
    public interface IOrderQueryRepository : IQueryService<Order, Guid>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);
    }
}

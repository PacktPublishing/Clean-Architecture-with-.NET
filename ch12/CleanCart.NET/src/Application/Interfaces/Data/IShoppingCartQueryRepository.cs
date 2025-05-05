using Domain.Entities;
using EntityAxis.Abstractions;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces.Data
{
    public interface IShoppingCartQueryRepository : IQueryService<ShoppingCart, Guid>
    {
        Task<ShoppingCart?> GetByUserIdAsync(Guid userId);
    }
}

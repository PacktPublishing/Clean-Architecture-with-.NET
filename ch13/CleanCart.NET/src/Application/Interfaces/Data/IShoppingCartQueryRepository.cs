using Domain.Entities;
using EntityAxis.Abstractions;

namespace Application.Interfaces.Data;

public interface IShoppingCartQueryRepository : IQueryService<ShoppingCart, Guid>
{
    Task<ShoppingCart?> GetByUserIdAsync(Guid userId);
}
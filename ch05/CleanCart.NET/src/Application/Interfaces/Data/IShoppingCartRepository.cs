using Domain.Entities;

namespace Application.Interfaces.Data;

public interface IShoppingCartRepository
{
    Task<ShoppingCart?> GetByUserIdAsync(Guid userId);

    Task SaveAsync(ShoppingCart shoppingCart);
    Task DeleteAsync(Guid userId);
}

using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces.Data
{
    public interface IShoppingCartRepository
    {
        Task<ShoppingCart> GetByUserIdAsync(Guid userId);

        Task SaveAsync(ShoppingCart shoppingCart);
    }
}

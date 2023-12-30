using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces.Data
{
    public interface IShoppingCartRepository
    {
        Task<ShoppingCart> GetByCustomerIdAsync(Guid customerId);

        Task SaveAsync(ShoppingCart shoppingCart);
    }
}

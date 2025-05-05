using Domain.Entities;
using EntityAxis.Abstractions;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces.Data
{
    public interface IShoppingCartCommandRepository : ICommandService<ShoppingCart, Guid>
    {
        Task SaveAsync(ShoppingCart shoppingCart);
        Task DeleteByUserIdAsync(Guid userId);
    }
}

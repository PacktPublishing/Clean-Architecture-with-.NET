using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.UseCases.AccessCustomerData;

public class AccessCustomerDataUseCase(
    IOrderRepository orderRepository,
    IShoppingCartRepository shoppingCartRepository,
    IUserRepository userRepository)
    : IAccessCustomerDataUseCase
{
    public async Task<ShoppingCart?> GetCustomerCartAsync(Guid requestingUserId, Guid targetUserId)
    {
        await AuthorizeUserAsync(requestingUserId);

        // Retrieve the customer's shopping cart by customer ID
        return await shoppingCartRepository.GetByUserIdAsync(targetUserId);
    }

    public async Task<IEnumerable<Order>> GetOrderHistoryAsync(Guid requestingUserId, Guid targetUserId)
    {
        await AuthorizeUserAsync(requestingUserId);

        // Retrieve the order history for the customer by user ID
        return await orderRepository.GetOrdersByUserIdAsync(targetUserId);
    }

    private async Task AuthorizeUserAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        if (!(user.Roles.Contains(UserRole.Administrator) || user.Roles.Contains(UserRole.CustomerService)))
        {
            throw new UnauthorizedAccessException("User is not authorized to access customer data.");
        }
    }
}

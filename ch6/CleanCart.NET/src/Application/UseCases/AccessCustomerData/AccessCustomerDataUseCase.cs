using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using Domain.Entities;
using Domain.Enums;

namespace Application.UseCases.AccessCustomerData;

public class AccessCustomerDataUseCase(
    IOrderRepository orderRepository,
    IShoppingCartRepository shoppingCartRepository,
    IUserRepository userRepository)
    : IAccessCustomerDataUseCase
{
    public async Task<ShoppingCart> GetCustomerCartAsync(Guid authorizationUserId, Guid customerUserId)
    {
        var user = await userRepository.GetByIdAsync(authorizationUserId);
        if (!(user.Roles.Contains(UserRole.Administrator) || user.Roles.Contains(UserRole.CustomerService)))
        {
            throw new UnauthorizedAccessException("User is not authorized to access customer data.");
        }

        // Retrieve the customer's shopping cart by customer ID
        return await shoppingCartRepository.GetByUserIdAsync(customerUserId);
    }

    public async Task<IEnumerable<Order>> GetOrderHistoryAsync(Guid authorizationUserId, Guid customerUserId)
    {
        var user = await userRepository.GetByIdAsync(authorizationUserId);
        if (!(user.Roles.Contains(UserRole.Administrator) || user.Roles.Contains(UserRole.CustomerService)))
        {
            throw new UnauthorizedAccessException("User is not authorized to access customer data.");
        }

        // Retrieve the order history for the customer by user ID
        return await orderRepository.GetOrdersByUserIdAsync(customerUserId);
    }
}
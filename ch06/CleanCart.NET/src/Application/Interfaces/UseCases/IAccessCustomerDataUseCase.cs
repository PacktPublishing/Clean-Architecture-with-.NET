using Domain.Entities;

namespace Application.Interfaces.UseCases;

public interface IAccessCustomerDataUseCase
{
    Task<ShoppingCart> GetCustomerCartAsync(Guid authorizationUserId, Guid customerUserId);
    Task<IEnumerable<Order>> GetOrderHistoryAsync(Guid authorizationUserId, Guid customerUserId);
}
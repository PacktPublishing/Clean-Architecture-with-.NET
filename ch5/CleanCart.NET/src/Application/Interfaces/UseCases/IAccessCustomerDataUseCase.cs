using Domain.Entities;

namespace Application.Interfaces.UseCases;

public interface IAccessCustomerDataUseCase
{
    Task<ShoppingCart?> GetCustomerCartAsync(Guid requestingUserId, Guid targetUserId);
    Task<IEnumerable<Order>> GetOrderHistoryAsync(Guid requestingUserId, Guid targetUserId);
}

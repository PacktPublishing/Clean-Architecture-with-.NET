using Application.Interfaces.Data;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Operations.UseCases.AccessCustomerData;

public class AccessCustomerOrderHistoryQueryHandler(
    IOrderQueryRepository orderQueryRepository,
    IUserQueryRepository userQueryRepository)
    : IRequestHandler<AccessCustomerOrderHistoryQuery, IEnumerable<Order>>
{
    public async Task<IEnumerable<Order>> Handle(AccessCustomerOrderHistoryQuery query, CancellationToken cancellationToken)
    {
        await AuthorizedUserAsync(query.AuthorizationUserId);

        // Retrieve the order history for the customer by user ID
        return await orderQueryRepository.GetOrdersByUserIdAsync(query.CustomerUserId, cancellationToken);
    }

    private async Task AuthorizedUserAsync(Guid userId)
    {
        var user = await userQueryRepository.GetByIdAsync(userId);

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
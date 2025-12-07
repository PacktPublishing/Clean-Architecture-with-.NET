using Application.Interfaces.Data;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Operations.UseCases.AccessCustomerData;

public class AccessCustomerCartQueryHandler(
    IShoppingCartQueryRepository shoppingCartQueryRepository,
    IUserQueryRepository userQueryRepository)
    : IRequestHandler<AccessCustomerCartQuery, ShoppingCart?>
{
    public async Task<ShoppingCart?> Handle(AccessCustomerCartQuery query, CancellationToken cancellationToken)
    {
        await AuthorizedUserAsync(query.AuthorizationUserId);

        // Retrieve the customer's shopping cart by customer ID
        return await shoppingCartQueryRepository.GetByUserIdAsync(query.CustomerUserId);
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
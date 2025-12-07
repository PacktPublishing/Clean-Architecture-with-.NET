using Application.Interfaces.Data;
using MediatR;

namespace Application.Operations.UseCases.AccessCustomerData;

public class AccessCustomerShoppingCartQueryHandler(IShoppingCartQueryRepository shoppingCartQueryRepository)
    : IRequestHandler<AccessCustomerShoppingCartQuery, Domain.Entities.ShoppingCart?>
{
    public Task<Domain.Entities.ShoppingCart?> Handle(AccessCustomerShoppingCartQuery request, CancellationToken cancellationToken)
    {
        return shoppingCartQueryRepository.GetByUserIdAsync(request.CustomerUserId);
    }
}
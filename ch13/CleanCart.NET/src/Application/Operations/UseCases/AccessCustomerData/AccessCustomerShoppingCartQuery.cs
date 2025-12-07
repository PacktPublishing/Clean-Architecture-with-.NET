using MediatR;

namespace Application.Operations.UseCases.AccessCustomerData;

public class AccessCustomerShoppingCartQuery(Guid customerUserId) : IRequest<Domain.Entities.ShoppingCart?>
{
    public Guid CustomerUserId { get; } = customerUserId;
}
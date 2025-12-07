using Domain.Entities;
using MediatR;

namespace Application.Operations.UseCases.AccessCustomerData;

public class AccessCustomerCartQuery(Guid authorizationUserId, Guid customerUserId) : IRequest<ShoppingCart?>
{
    public Guid AuthorizationUserId { get; } = authorizationUserId;

    public Guid CustomerUserId { get; } = customerUserId;
}
using MediatR;
using System;

namespace Application.Operations.UseCases.AccessCustomerData;

public class AccessCustomerShoppingCartQuery : IRequest<Domain.Entities.ShoppingCart?>
{
    public AccessCustomerShoppingCartQuery(Guid customerUserId)
    {
        CustomerUserId = customerUserId;
    }

    public Guid CustomerUserId { get; }
}
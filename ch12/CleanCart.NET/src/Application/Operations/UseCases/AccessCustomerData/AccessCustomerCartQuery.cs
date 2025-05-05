using System;
using Domain.Entities;
using MediatR;

namespace Application.Operations.UseCases.AccessCustomerData
{
    public class AccessCustomerCartQuery : IRequest<ShoppingCart?>
    {
        public AccessCustomerCartQuery(Guid authorizationUserId, Guid customerUserId)
        {
            AuthorizationUserId = authorizationUserId;
            CustomerUserId = customerUserId;
        }

        public Guid AuthorizationUserId { get; }

        public Guid CustomerUserId { get; }
    }
}

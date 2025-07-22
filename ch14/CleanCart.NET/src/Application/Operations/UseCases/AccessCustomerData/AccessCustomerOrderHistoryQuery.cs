using System;
using Domain.Entities;
using System.Collections.Generic;
using MediatR;

namespace Application.Operations.UseCases.AccessCustomerData
{
    public class AccessCustomerOrderHistoryQuery : IRequest<IEnumerable<Order>>
    {
        public Guid AuthorizationUserId { get; }
        public Guid CustomerUserId { get; }

        public AccessCustomerOrderHistoryQuery(Guid authorizationUserId, Guid customerUserId)
        {
            AuthorizationUserId = authorizationUserId;
            CustomerUserId = customerUserId;
        }
    }
}

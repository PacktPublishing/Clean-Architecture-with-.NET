using Application.Interfaces.Data;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Operations.UseCases.AccessCustomerData
{
    public class AccessCustomerOrderHistoryQueryHandler : IRequestHandler<AccessCustomerOrderHistoryQuery, IEnumerable<Order>>
    {
        private readonly IOrderQueryRepository _orderQueryRepository;
        private readonly IUserQueryRepository _userQueryRepository;

        public AccessCustomerOrderHistoryQueryHandler(
            IOrderQueryRepository orderQueryRepository,
            IUserQueryRepository userQueryRepository)
        {
            _orderQueryRepository = orderQueryRepository;
            _userQueryRepository = userQueryRepository;
        }

        public async Task<IEnumerable<Order>> Handle(AccessCustomerOrderHistoryQuery query, CancellationToken cancellationToken)
        {
            await AuthorizedUserAsync(query.AuthorizationUserId);

            // Retrieve the order history for the customer by user ID
            return await _orderQueryRepository.GetOrdersByUserIdAsync(query.CustomerUserId, cancellationToken);
        }

        private async Task AuthorizedUserAsync(Guid userId)
        {
            var user = await _userQueryRepository.GetByIdAsync(userId);

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
}

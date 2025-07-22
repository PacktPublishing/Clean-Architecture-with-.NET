using Application.Interfaces.Data;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Operations.UseCases.AccessCustomerData
{
    public class AccessCustomerCartQueryHandler : IRequestHandler<AccessCustomerCartQuery, ShoppingCart?>
    {
        private readonly IShoppingCartQueryRepository _shoppingCartQueryRepository;
        private readonly IUserQueryRepository _userQueryRepository;

        public AccessCustomerCartQueryHandler(
            IShoppingCartQueryRepository shoppingCartQueryRepository,
            IUserQueryRepository userQueryRepository)
        {
            _shoppingCartQueryRepository = shoppingCartQueryRepository;
            _userQueryRepository = userQueryRepository;
        }

        public async Task<ShoppingCart?> Handle(AccessCustomerCartQuery query, CancellationToken cancellationToken)
        {
            await AuthorizedUserAsync(query.AuthorizationUserId);

            // Retrieve the customer's shopping cart by customer ID
            return await _shoppingCartQueryRepository.GetByUserIdAsync(query.CustomerUserId);
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

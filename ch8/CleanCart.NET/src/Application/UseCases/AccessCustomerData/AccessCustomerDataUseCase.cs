﻿using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.UseCases.AccessCustomerData
{
    public class AccessCustomerDataUseCase : IAccessCustomerDataUseCase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IUserRepository _userRepository;

        public AccessCustomerDataUseCase(
            IOrderRepository orderRepository,
            IShoppingCartRepository shoppingCartRepository,
            IUserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _shoppingCartRepository = shoppingCartRepository;
            _userRepository = userRepository;
        }

        public async Task<ShoppingCart?> GetCustomerCartAsync(Guid authorizationUserId, Guid customerUserId)
        {
            await AuthorizedUserAsync(authorizationUserId);

            // Retrieve the customer's shopping cart by customer ID
            return await _shoppingCartRepository.GetByUserIdAsync(customerUserId);
        }

        public async Task<IEnumerable<Order>> GetOrderHistoryAsync(Guid authorizationUserId, Guid customerUserId)
        {
            await AuthorizedUserAsync(authorizationUserId);

            // Retrieve the order history for the customer by user ID
            return await _orderRepository.GetOrdersByUserIdAsync(customerUserId);
        }

        private async Task AuthorizedUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

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

using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Threading.Tasks;

namespace Application.UseCases.ManageProductInventory
{
    public class ManageProductInventoryUseCase : IManageProductInventoryUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IProductRepository _productRepository;

        public ManageProductInventoryUseCase(IUserRepository userRepository, IProductRepository productRepository)
        {
            _userRepository = userRepository;
            _productRepository = productRepository;
        }

        public async Task UpdateProductInventoryAsync(Guid userId, Guid productId, int stockLevel)
        {
            User user = await _userRepository.GetByIdAsync(userId);
            if (!user.Roles.Contains(UserRole.Administrator))
            {
                throw new UnauthorizedAccessException("User is not authorized to manage product inventory.");
            }

            Product product = await _productRepository.GetByIdAsync(productId);
            product.UpdateStockLevel(stockLevel);
            await _productRepository.UpdateAsync(product);
        }
    }
}

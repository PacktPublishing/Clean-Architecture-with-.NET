using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using Domain.Entities;
using Domain.Enums;

namespace Application.UseCases.ManageProductInventory;

public class ManageProductInventoryUseCase(IUserRepository userRepository, IProductRepository productRepository)
    : IManageProductInventoryUseCase
{
    public async Task UpdateProductInventoryAsync(Guid userId, Guid productId, int stockLevel)
    {
        User? user = await userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new ArgumentException($"User '{userId}' does not exist.");
        }

        if (!user.Roles.Contains(UserRole.Administrator))
        {
            throw new UnauthorizedAccessException("User is not authorized to manage product inventory.");
        }

        Product product = await productRepository.GetByIdAsync(productId);
        product.UpdateStockLevel(stockLevel);
        await productRepository.UpdateAsync(product);
    }
}
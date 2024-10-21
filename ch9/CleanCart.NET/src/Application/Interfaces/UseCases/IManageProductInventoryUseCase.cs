using System;
using System.Threading.Tasks;

namespace Application.Interfaces.UseCases
{
    public interface IManageProductInventoryUseCase
    {
        Task UpdateProductInventoryAsync(Guid userId, Guid productId, int stockLevel);
        // Add other methods for managing inventory as needed
    }
}

using Domain.Entities;

namespace Application.Interfaces.Data;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<Product> GetByIdAsync(Guid id);
    Task UpdateAsync(Product product);
}
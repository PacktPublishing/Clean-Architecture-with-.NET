using Application.Common.Models;
using Domain.Entities;

namespace Application.Interfaces.Data;

public interface IProductRepository
{
    Task<PagedResult<Product>> GetPagedAsync(int page, int pageSize);
    Task<Product?> GetByIdAsync(Guid id);
    Task UpdateAsync(Product product);
}
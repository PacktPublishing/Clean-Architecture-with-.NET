using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces.Data;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task UpdateAsync(Product product);
}

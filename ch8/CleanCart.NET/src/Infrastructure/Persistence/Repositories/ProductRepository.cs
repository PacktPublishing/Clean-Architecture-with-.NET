using Application.Interfaces.Data;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ProductRepository(IDbContextFactory<CoreDbContext> contextFactory, IMapper mapper) : RepositoryBase<CoreDbContext>(contextFactory, mapper), IProductRepository
{
    public async Task<Product> GetByIdAsync(Guid id)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();
        var sqlProduct = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
        return Mapper.Map<Product>(sqlProduct);
    }

    public async Task UpdateAsync(Product product)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();
        var sqlProduct = await dbContext.Products.FindAsync(product.Id);
        if (sqlProduct != null)
        {
            Mapper.Map(product, sqlProduct);
            await dbContext.SaveChangesAsync();
        }
    }
}
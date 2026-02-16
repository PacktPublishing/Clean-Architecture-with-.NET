using Application.Common.Models;
using Application.Interfaces.Data;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ProductRepository(IDbContextFactory<CoreDbContext> contextFactory, IMapper mapper) : RepositoryBase<CoreDbContext>(contextFactory, mapper), IProductRepository
{
    public async Task<PagedResult<Product>> GetPagedAsync(int page, int pageSize)
    {
        var dbContext = await ContextFactory.CreateDbContextAsync();

        var query = dbContext.Products.AsNoTracking();

        var totalCount = await query.CountAsync();

        var sqlProducts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var products = Mapper.Map<List<Product>>(sqlProducts);

        return new PagedResult<Product>(
            products,
            totalCount,
            page,
            pageSize);
    }

    public async Task<Product?> GetByIdAsync(Guid id)
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
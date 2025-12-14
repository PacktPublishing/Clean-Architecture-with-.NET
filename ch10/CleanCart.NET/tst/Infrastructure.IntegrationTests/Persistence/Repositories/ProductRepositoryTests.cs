using AutoMapper;
using FluentAssertions;
using Infrastructure.Persistence.EntityFramework;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntegrationTests.Persistence.Repositories;

[Collection(nameof(SharedTestCollection))]
public class ProductRepositoryTests(TestInitializer testInitializer) : IAsyncLifetime
{
    private readonly IMapper _mapper = testInitializer.Mapper;
    private readonly IDbContextFactory<CoreDbContext> _dbContextFactory = testInitializer.DbContextFactory;
    private readonly Func<Task> _resetDatabase = testInitializer.ResetDatabaseAsync;
    private ProductRepository Sut => new(_dbContextFactory, _mapper);

    [Fact]
    public async Task Can_GetProductByIdAsync()
    {
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var existingProduct = await seeder.SeedProduct();

        var product = await Sut.GetByIdAsync(existingProduct.Id);

        product.Should().BeEquivalentTo(existingProduct);
    }

    [Fact]
    public async Task Can_UpdateAsync()
    {
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var existingProduct = await seeder.SeedProduct();
        existingProduct.UpdateStockLevel(existingProduct.StockLevel + 1000);
        var domainProduct = _mapper.Map<Domain.Entities.Product>(existingProduct);

        await Sut.UpdateAsync(domainProduct);

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var updatedProduct = await dbContext.Products.FirstAsync(p => p.Id == existingProduct.Id);
        updatedProduct.Should().BeEquivalentTo(existingProduct);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        return _resetDatabase();
    }
}
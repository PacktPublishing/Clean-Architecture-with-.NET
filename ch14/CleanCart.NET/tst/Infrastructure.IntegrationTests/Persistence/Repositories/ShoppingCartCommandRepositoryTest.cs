using AutoMapper;
using Domain.Entities;
using EntityAxis.KeyMappers;
using FluentAssertions;
using Infrastructure.Persistence.EntityFramework;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntegrationTests.Persistence.Repositories;

[Collection(nameof(SharedTestCollection))]
public class ShoppingCartCommandRepositoryTest(TestInitializer testInitializer) : IAsyncLifetime
{
    private readonly IMapper _mapper = testInitializer.Mapper;
    private readonly IDbContextFactory<CoreDbContext> _dbContextFactory = testInitializer.DbContextFactory;
    private readonly Func<Task> _resetDatabase = testInitializer.ResetDatabaseAsync;
    private ShoppingCartCommandRepository Sut => new(_dbContextFactory, _mapper, new IdentityKeyMapper<Guid>());

    [Fact]
    public async Task Can_SaveAsync_WhenCartExists()
    {
        // Arrange
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var user = await seeder.SeedUser();
        var product1 = await seeder.SeedProduct();
        var product2 = await seeder.SeedProduct();
        var shoppingCart = await seeder.SeedShoppingCart(user, product1);

        // Modify shopping cart
        shoppingCart.Items.Add(new ShoppingCartItem(product2.Id, product2.Name, product2.Price, 2));

        // Act
        await Sut.SaveAsync(shoppingCart);

        // Assert
        var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var savedShoppingCart = _mapper.Map<ShoppingCart>(await dbContext.ShoppingCarts.FirstOrDefaultAsync(sc => sc.UserId == user.Id));
        savedShoppingCart.Should().BeEquivalentTo(shoppingCart);
    }

    [Fact]
    public async Task Can_SaveAsync_WhenCartDoesNotExist()
    {
        // Arrange
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var user = await seeder.SeedUser();
        var product = await seeder.SeedProduct();
        var shoppingCart = new ShoppingCart(user.Id);
        shoppingCart.Items.Add(new ShoppingCartItem(product.Id, product.Name, product.Price, 1));

        // Act
        await Sut.SaveAsync(shoppingCart);

        // Assert
        var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var sqlShoppingCart = await dbContext.ShoppingCarts.FirstOrDefaultAsync(sc => sc.UserId == user.Id);
        var returnedShoppingCart = _mapper.Map<ShoppingCart>(sqlShoppingCart);
        returnedShoppingCart.Should().BeEquivalentTo(shoppingCart);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        return _resetDatabase();
    }
}
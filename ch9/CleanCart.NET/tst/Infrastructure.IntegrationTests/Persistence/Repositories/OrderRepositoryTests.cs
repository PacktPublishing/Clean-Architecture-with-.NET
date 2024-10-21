using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence.EntityFramework;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntegrationTests.Persistence.Repositories;

[Collection(nameof(SharedTestCollection))]
public class OrderRepositoryTests(TestInitializer testInitializer) : IAsyncLifetime
{
    private readonly IMapper _mapper = testInitializer.Mapper;
    private readonly IDbContextFactory<CoreDbContext> _dbContextFactory = testInitializer.DbContextFactory;
    private readonly Func<Task> _resetDatabase = testInitializer.ResetDatabaseAsync;
    private OrderRepository Sut => new(_dbContextFactory, _mapper);

    [Fact]
    public async Task Can_CreateOrderAsync()
    {
        // Arrange
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var existingUser = await seeder.SeedUser();
        var newOrder = new Order(existingUser.Id, [], 20);

        // Act
        var createdOrder = await Sut.CreateOrderAsync(newOrder);

        // Assert
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var existingOrder = await dbContext.Orders.FirstAsync(o => o.Id == createdOrder.Id);
        existingOrder.Should().BeEquivalentTo(createdOrder);
    }

    [Fact]
    public async Task Can_UpdateOrderAsync()
    {
        // Arrange
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var user = await seeder.SeedUser();
        var product = await seeder.SeedProduct();
        var existingOrder = await seeder.SeedOrder(user, product);
        existingOrder.Status = OrderStatus.Paid;
        var domainOrder = _mapper.Map<Order>(existingOrder);

        // Act
        await Sut.UpdateOrderAsync(domainOrder);

        // Assert
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var updatedOrder = _mapper.Map<Order>(await dbContext.Orders.Include(order => order.Items).FirstAsync(o => o.Id == existingOrder.Id));
        updatedOrder.Should().BeEquivalentTo(existingOrder);
    }

    [Fact]
    public async Task Can_GetOrdersByUserIdAsync()
    {
        // Arrange
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var user = await seeder.SeedUser();
        var product = await seeder.SeedProduct();
        var existingOrders = new List<Order>();
        var orderTasks = Enumerable.Range(0, 5).Select(_ => seeder.SeedOrder(user, product)).ToList();
        existingOrders.AddRange(await Task.WhenAll(orderTasks));
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // Act
        var returnedOrders = await Sut.GetOrdersByUserIdAsync(user.Id);

        // Assert
        returnedOrders.Should().BeEquivalentTo(existingOrders);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        return _resetDatabase();
    }
}
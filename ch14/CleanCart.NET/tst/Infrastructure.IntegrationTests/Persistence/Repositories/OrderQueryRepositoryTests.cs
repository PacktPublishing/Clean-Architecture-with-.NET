using AutoMapper;
using Domain.Entities;
using EntityAxis.KeyMappers;
using FluentAssertions;
using Infrastructure.Persistence.EntityFramework;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;

namespace Infrastructure.IntegrationTests.Persistence.Repositories;

[Collection(nameof(SharedTestCollection))]
public class OrderQueryRepositoryTests(TestInitializer testInitializer) : IAsyncLifetime
{
    private readonly IMapper _mapper = testInitializer.Mapper;
    private readonly ISystemClock _systemClock = new SystemClock();
    private readonly IDbContextFactory<CoreDbContext> _dbContextFactory = testInitializer.DbContextFactory;
    private readonly Func<Task> _resetDatabase = testInitializer.ResetDatabaseAsync;
    private OrderQueryRepository Sut => new(_dbContextFactory, _mapper, new IdentityKeyMapper<Guid>(), _systemClock);

    [Fact]
    public async Task Can_GetOrdersByUserIdAsync()
    {
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var user = await seeder.SeedUser();
        var product = await seeder.SeedProduct();
        var existingOrders = new List<Order>();
        var orderTasks = Enumerable.Range(0, 5).Select(_ => seeder.SeedOrder(user, product)).ToList();
        existingOrders.AddRange(await Task.WhenAll(orderTasks));
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var returnedOrders = await Sut.GetOrdersByUserIdAsync(user.Id);

        returnedOrders.Should().BeEquivalentTo(existingOrders);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        return _resetDatabase();
    }
}
using AutoMapper;
using Domain.Entities;
using EntityAxis.KeyMappers;
using FluentAssertions;
using Infrastructure.Persistence.EntityFramework;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntegrationTests.Persistence.Repositories;

[Collection(nameof(SharedTestCollection))]
public class UserQueryRepositoryTests(TestInitializer testInitializer) : IAsyncLifetime
{
    private readonly IMapper _mapper = testInitializer.Mapper;
    private readonly IDbContextFactory<CoreDbContext> _dbContextFactory = testInitializer.DbContextFactory;
    private readonly Func<Task> _resetDatabase = testInitializer.ResetDatabaseAsync;
    private UserQueryRepository Sut => new(_dbContextFactory, _mapper, new IdentityKeyMapper<Guid>());

    [Fact]
    public async Task Can_GetByIdAsync()
    {
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var existingUser = await seeder.SeedUser();
        var expectedUser = _mapper.Map<User>(existingUser);

        var user = await Sut.GetByIdAsync(existingUser.Id);

        user.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task Can_GetAllAsync()
    {
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var seedTasks = Enumerable.Range(0, 3).Select(_ => seeder.SeedUser()).ToList();
        await Task.WhenAll(seedTasks);
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var existingUsers = dbContext.Users.ToList();
        var expectedUsers = _mapper.Map<List<User>>(existingUsers);

        var users = await Sut.GetAllAsync();

        users.Should().BeEquivalentTo(expectedUsers);
    }

    [Fact]
    public async Task Can_GetByUsernameAsync()
    {
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var existingUser = await seeder.SeedUser();

        var returnedUser = await Sut.GetByUsernameAsync(existingUser.Username);

        returnedUser.Should().BeEquivalentTo(existingUser);
    }

    [Fact]
    public async Task Can_GetByEmailAsync()
    {
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var existingUser = await seeder.SeedUser();

        var returnedUser = await Sut.GetByEmailAsync(existingUser.Email);

        returnedUser.Should().BeEquivalentTo(existingUser);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        return _resetDatabase();
    }
}
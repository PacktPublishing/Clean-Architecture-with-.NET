using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence.EntityFramework;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntegrationTests.Persistence.Repositories;

[Collection(nameof(SharedTestCollection))]
public class UserRepositoryTests(TestInitializer testInitializer) : IAsyncLifetime
{
    private readonly IMapper _mapper = testInitializer.Mapper;
    private readonly IDbContextFactory<CoreDbContext> _dbContextFactory = testInitializer.DbContextFactory;
    private readonly Func<Task> _resetDatabase = testInitializer.ResetDatabaseAsync;
    private UserRepository Sut => new(_dbContextFactory, _mapper);

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

    [Fact]
    public async Task Can_CreateUserAsync()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var newUser = new User($"{Guid.NewGuid()}@email.com", $"{Guid.NewGuid()}@email.com", "test", [UserRole.CustomerService]);

        await Sut.CreateUserAsync(newUser);

        var createdUser = await dbContext.Users.FirstAsync(u => u.Id == newUser.Id);
        createdUser.Should().BeEquivalentTo(newUser);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        return _resetDatabase();
    }
}
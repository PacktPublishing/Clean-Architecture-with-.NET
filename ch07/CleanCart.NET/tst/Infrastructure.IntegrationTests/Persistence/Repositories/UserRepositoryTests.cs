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
        // Arrange
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var existingUser = await seeder.SeedUser();
        var expectedUser = _mapper.Map<User>(existingUser);

        // Act
        var user = await Sut.GetByIdAsync(existingUser.Id);

        // Assert
        user.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task Can_GetAllAsync()
    {
        // Arrange
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var seedTasks = Enumerable.Range(0, 3).Select(_ => seeder.SeedUser()).ToList();
        await Task.WhenAll(seedTasks);
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var existingUsers = dbContext.Users.ToList();
        var expectedUsers = _mapper.Map<List<User>>(existingUsers);

        // Act
        var users = await Sut.GetAllAsync();

        // Assert
        users.Should().BeEquivalentTo(expectedUsers);
    }

    [Fact]
    public async Task Can_GetByUsernameAsync()
    {
        // Arrange
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var existingUser = await seeder.SeedUser();

        // Act
        var returnedUser = await Sut.GetByUsernameAsync(existingUser.Username);

        // Assert
        returnedUser.Should().BeEquivalentTo(existingUser);
    }

    [Fact]
    public async Task Can_GetByEmailAsync()
    {
        // Arrange
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var existingUser = await seeder.SeedUser();

        // Act
        var returnedUser = await Sut.GetByEmailAsync(existingUser.Email);

        // Assert
        returnedUser.Should().BeEquivalentTo(existingUser);
    }

    [Fact]
    public async Task Can_CreateUserAsync()
    {
        // Arrange
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var newUser = new User($"{Guid.NewGuid()}@email.com", $"{Guid.NewGuid()}@email.com", "test", [UserRole.CustomerService]);

        // Act
        await Sut.CreateUserAsync(newUser);

        // Assert
        var createdUser = await dbContext.Users.FirstAsync(u => u.Id == newUser.Id);
        createdUser.Should().BeEquivalentTo(newUser);
    }

    [Fact]
    public async Task Can_UpdateUserAsync()
    {
        // Arrange
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var existingUser = await seeder.SeedUser();
        existingUser.AddRole(UserRole.Administrator);
        var existingDomainUser = _mapper.Map<User>(existingUser);

        // Act
        await Sut.UpdateUserAsync(existingDomainUser);

        // Assert
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var updatedUser = await dbContext.Users.FirstAsync(u => u.Id == existingUser.Id);
        updatedUser.Should().BeEquivalentTo(existingUser);
    }

    [Fact]
    public async Task Can_DeleteUserAsync()
    {
        // Arrange
        var seeder = new DataSeeder(_dbContextFactory, _mapper);
        var existingUser = await seeder.SeedUser();

        // Act
        await Sut.DeleteUserAsync(existingUser.Id);

        // Assert
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var deletedUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == existingUser.Id);
        deletedUser.Should().BeNull();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        return _resetDatabase();
    }
}
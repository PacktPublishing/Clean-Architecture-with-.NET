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

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        return _resetDatabase();
    }
}
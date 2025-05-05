﻿using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using EntityAxis.KeyMappers;
using FluentAssertions;
using Infrastructure.Persistence.EntityFramework;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntegrationTests.Persistence.Repositories;

[Collection(nameof(SharedTestCollection))]
public class UserCommandRepositoryTests(TestInitializer testInitializer) : IAsyncLifetime
{
    private readonly IMapper _mapper = testInitializer.Mapper;
    private readonly IDbContextFactory<CoreDbContext> _dbContextFactory = testInitializer.DbContextFactory;
    private readonly Func<Task> _resetDatabase = testInitializer.ResetDatabaseAsync;
    private UserCommandRepository Sut => new(_dbContextFactory, _mapper, new IdentityKeyMapper<Guid>());

    [Fact]
    public async Task Can_CreateUserAsync()
    {
        // Arrange
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var newUser = new User($"{Guid.NewGuid()}@email.com", $"{Guid.NewGuid()}@email.com", "test", [UserRole.CustomerService]);

        // Act
        await Sut.CreateAsync(newUser);

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
        await Sut.UpdateAsync(existingDomainUser);

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
        await Sut.DeleteAsync(existingUser.Id);

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
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Infrastructure.IntegrationTests.Persistence.DbContext;

[Collection(nameof(SharedTestCollection))]
public class DbSetUserTests : IAsyncLifetime
{
    private readonly IEntityType _entityType;

    public DbSetUserTests(TestInitializer testInitializer)
    {
        using CoreDbContext dbContext = testInitializer.DbContextFactory.CreateDbContext();

        string? entityKey = typeof(User).FullName;
        Assert.NotNull(entityKey);

        _entityType = dbContext.GetService<IDesignTimeModel>().Model.FindEntityType(entityKey)!;
        Assert.NotNull(_entityType);
    }

    [Fact]
    public void PrimaryKey_IsConfigured()
    {
        IProperty? property = _entityType.FindProperty(nameof(User.Id));
        AssertHelper.IsPrimaryKeyValid(property);
    }

    [Theory]
    [InlineData(nameof(User.Username), 50, null, true)]
    [InlineData(nameof(User.Email), 320)]
    [InlineData(nameof(User.FullName), 100)]
    [InlineData(nameof(User.Roles), 4000)]
    public void EntityProperties_AreConfigured(
        string propertyName,
        int? columnLength = null,
        int? columnOrder = null,
        bool isUniqueIndex = false,
        bool isNullable = false,
        string? columnType = null,
        ValueGenerated valueGenerated = ValueGenerated.Never
    )
    {
        IProperty? property = _entityType.FindProperty(propertyName);
        AssertHelper.IsPropertyValid(property, columnLength, columnOrder, isUniqueIndex, isNullable, valueGenerated, columnType:columnType);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;
}
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Infrastructure.IntegrationTests.Persistence.DbContext;

[Collection(nameof(SharedTestCollection))]
public class DbSetProductTests : IAsyncLifetime
{
    private readonly IEntityType _entityType;

    public DbSetProductTests(TestInitializer testInitializer)
    {
        using CoreDbContext dbContext = testInitializer.DbContextFactory.CreateDbContext();

        string? entityKey = typeof(Product).FullName;
        Assert.NotNull(entityKey);

        _entityType = dbContext.GetService<IDesignTimeModel>().Model.FindEntityType(entityKey)!;
        Assert.NotNull(_entityType);
    }

    [Fact]
    public void PrimaryKey_IsConfigured()
    {
        IProperty? property = _entityType.FindProperty(nameof(Product.Id));
        AssertHelper.IsPrimaryKeyValid(property);
    }

    [Theory]
    [InlineData(nameof(Product.Name), 255)]
    [InlineData(nameof(Product.Price), null, null, false, false, "decimal(18,2)")]
    [InlineData(nameof(Product.StockLevel))]
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
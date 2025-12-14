using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Infrastructure.IntegrationTests.Persistence.DbContext;

[Collection(nameof(SharedTestCollection))]
public class DbSetOrderTests : IAsyncLifetime
{
    private readonly IEntityType _entityType;
    private readonly INavigation _orderItemsNav;

    public DbSetOrderTests(TestInitializer testInitializer)
    {
        using CoreDbContext dbContext = testInitializer.DbContextFactory.CreateDbContext();

        string? entityKey = typeof(Order).FullName;
        Assert.NotNull(entityKey);

        _entityType = dbContext.GetService<IDesignTimeModel>().Model.FindEntityType(entityKey)!;
        Assert.NotNull(_entityType);

        const string orderItems = nameof(Order.Items);
        _orderItemsNav = _entityType.FindNavigation(orderItems)!;
        Assert.NotNull(_orderItemsNav);
    }

    [Fact]
    public void PrimaryKey_IsConfigured()
    {
        IProperty? property = _entityType.FindProperty(nameof(Order.Id));
        AssertHelper.IsPrimaryKeyValid(property);
    }

    [Fact]
    public void Order_UserId_IsForeignKey()
    {
        IProperty? property = _entityType.FindProperty(nameof(Order.UserId));
        AssertHelper.HasForeignKey<User>(property);
    }

    [Theory]
    [InlineData(nameof(Order.UserId), null, null)]
    [InlineData(nameof(Order.TotalAmount), null, null)]
    [InlineData(nameof(Order.Status), 20, null)]
    [InlineData(nameof(Order.CreatedOn), null, null)]
    public void EntityProperties_AreConfigured(
        string propertyName,
        int? columnLength,
        int? columnOrder,
        bool isUniqueIndex = false,
        bool isNullable = false,
        ValueGenerated valueGenerated = ValueGenerated.Never
    )
    {
        IProperty? property = _entityType.FindProperty(propertyName);
        AssertHelper.IsPropertyValid(property, columnLength, columnOrder, isUniqueIndex, isNullable, valueGenerated);
    }

    [Fact]
    public void OrderItems_Configured()
    {
        Assert.True(_orderItemsNav.IsCollection);
        Assert.Equal(DeleteBehavior.Cascade, _orderItemsNav.ForeignKey.DeleteBehavior);
        AssertHelper.PropertyNamesMatchColumnNames<OrderItem>(_orderItemsNav.TargetEntityType);
    }

    [Fact]
    public void OrderItem_ProductId_IsForeignKey()
    {
        var property = _orderItemsNav.TargetEntityType.FindProperty(nameof(OrderItem.ProductId));
        AssertHelper.HasForeignKey<Product>(property);
    }

    [Fact]
    public void OrderItem_Quantity_Configured()
    {
        var property = _orderItemsNav.TargetEntityType.FindProperty(nameof(OrderItem.Quantity));
        AssertHelper.IsPropertyValid(property, columnLength: null, columnOrder: null);
    }

    [Fact]
    public void OrderItem_ProductName_Configured()
    {
        var property = _orderItemsNav.TargetEntityType.FindProperty(nameof(OrderItem.ProductName));
        AssertHelper.IsPropertyValid(property, columnLength: 255, columnOrder: null);
    }

    [Fact]
    public void OrderItem_ProductPrice_Configured()
    {
        var property = _orderItemsNav.TargetEntityType.FindProperty(nameof(OrderItem.ProductPrice));
        AssertHelper.IsPropertyValid(property, columnLength: null, columnOrder: null, columnType: "decimal(18,2)");
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;
}
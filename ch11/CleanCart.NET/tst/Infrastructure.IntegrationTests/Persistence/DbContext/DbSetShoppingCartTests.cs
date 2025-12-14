using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Infrastructure.IntegrationTests.Persistence.DbContext;

[Collection(nameof(SharedTestCollection))]
public class DbSetShoppingCartTests : IAsyncLifetime
{
    private readonly IEntityType _entityType;
    private readonly INavigation _cartItemsNav;

    public DbSetShoppingCartTests(TestInitializer testInitializer)
    {
        using CoreDbContext dbContext = testInitializer.DbContextFactory.CreateDbContext();

        string? entityKey = typeof(ShoppingCart).FullName;
        Assert.NotNull(entityKey);

        _entityType = dbContext.GetService<IDesignTimeModel>().Model.FindEntityType(entityKey)!;
        Assert.NotNull(_entityType);

        const string cartItems = nameof(ShoppingCart.Items);
        _cartItemsNav = _entityType.FindNavigation(cartItems)!;
        Assert.NotNull(_cartItemsNav);
    }

    [Fact]
    public void PrimaryKey_IsConfigured()
    {
        IProperty? property = _entityType.FindProperty(nameof(ShoppingCart.Id));
        AssertHelper.IsPrimaryKeyValid(property);
    }

    [Fact]
    public void ShoppingCart_UserId_IsForeignKey()
    {
        IProperty? property = _entityType.FindProperty(nameof(ShoppingCart.UserId));
        AssertHelper.HasForeignKey<User>(property);
    }

    [Theory]
    [InlineData(nameof(ShoppingCart.UserId), null, null, true)]
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
    public void ShoppingCartItems_Configured()
    {
        Assert.True(_cartItemsNav.IsCollection);
        Assert.Equal(DeleteBehavior.Cascade, _cartItemsNav.ForeignKey.DeleteBehavior);
        AssertHelper.PropertyNamesMatchColumnNames<ShoppingCartItem>(_cartItemsNav.TargetEntityType);
    }

    [Fact]
    public void ShoppingCartItem_ProductId_IsForeignKey()
    {
        var property = _cartItemsNav.TargetEntityType.FindProperty(nameof(ShoppingCartItem.ProductId));
        AssertHelper.HasForeignKey<Product>(property);
    }

    [Fact]
    public void ShoppingCartItem_Quantity_Configured()
    {
        var property = _cartItemsNav.TargetEntityType.FindProperty(nameof(ShoppingCartItem.Quantity));
        AssertHelper.IsPropertyValid(property, columnLength: null, columnOrder: null);
    }

    [Fact]
    public void ShoppingCartItem_ProductName_Configured()
    {
        var property = _cartItemsNav.TargetEntityType.FindProperty(nameof(ShoppingCartItem.ProductName));
        AssertHelper.IsPropertyValid(property, columnLength: 255, columnOrder: null);
    }

    [Fact]
    public void ShoppingCartItem_ProductPrice_Configured()
    {
        var property = _cartItemsNav.TargetEntityType.FindProperty(nameof(ShoppingCartItem.ProductPrice));
        AssertHelper.IsPropertyValid(property, columnLength: null, columnOrder: null, columnType: "decimal(18,2)");
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;
}
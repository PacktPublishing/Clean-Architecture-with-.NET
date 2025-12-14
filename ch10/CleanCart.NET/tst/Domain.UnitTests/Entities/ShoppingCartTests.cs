namespace Domain.UnitTests.Entities;

public class ShoppingCartTests
{
    [Fact]
    public void AddItem_AddsNewItemToCart_WhenExistingItemIsNull()
    {
        var userId = Guid.NewGuid();
        var shoppingCart = new ShoppingCart(userId);
        var product = new Product("Test Product", 10.00m, 5, "img.png");

        shoppingCart.AddItem(product.Id, product.Name, product.Price, 1);

        Assert.Single(shoppingCart.Items);
        Assert.Equal(product.Id, shoppingCart.Items.First().ProductId);
        Assert.Equal(1, shoppingCart.Items.First().Quantity);
    }

    [Fact]
    public void AddItem_IncrementsQuantity_WhenExistingItemIsNotNull()
    {
        var userId = Guid.NewGuid();
        var shoppingCart = new ShoppingCart(userId);
        var product = new Product("Test Product", 10.00m, 5, "img.png");

        shoppingCart.AddItem(product.Id, product.Name, product.Price, 1);

        shoppingCart.AddItem(product.Id, product.Name, product.Price, 1);

        Assert.Single(shoppingCart.Items);
        Assert.Equal(product.Id, shoppingCart.Items.First().ProductId);
        Assert.Equal(2, shoppingCart.Items.First().Quantity);
    }

    [Fact]
    public void RemoveItem_RemovesItemFromCart_WhenQuantityIsEqualToExistingItemQuantity()
    {
        var userId = Guid.NewGuid();
        var shoppingCart = new ShoppingCart(userId);
        var product = new Product("Test Product", 10.00m, 5, "img.png");

        shoppingCart.AddItem(product.Id, product.Name, product.Price, 1);

        shoppingCart.RemoveItem(product.Id, 1);

        Assert.Empty(shoppingCart.Items);
    }

    [Fact]
    public void RemoveItem_RemovesItemFromCart_WhenQuantityIsGreaterThanExistingItemQuantity()
    {
        var userId = Guid.NewGuid();
        var shoppingCart = new ShoppingCart(userId);
        var product = new Product("Test Product", 10.00m, 5, "img.png");

        shoppingCart.AddItem(product.Id, product.Name, product.Price, 1);

        shoppingCart.RemoveItem(product.Id, 2);

        Assert.Empty(shoppingCart.Items);
    }

    [Fact]
    public void RemoveItem_RemovesItemFromCart_WhenQuantityIsLessThanExistingItemQuantity()
    {
        var userId = Guid.NewGuid();
        var shoppingCart = new ShoppingCart(userId);
        var product = new Product("Test Product", 10.00m, 5, "img.png");

        shoppingCart.AddItem(product.Id, product.Name, product.Price, 2);

        shoppingCart.RemoveItem(product.Id, 1);

        Assert.Single(shoppingCart.Items);
        Assert.Equal(product.Id, shoppingCart.Items.First().ProductId);
        Assert.Equal(1, shoppingCart.Items.First().Quantity);
    }
}

namespace Domain.UnitTests.Entities;

public class ShoppingCartTests
{
    [Fact]
    public void AddItem_AddsNewItemToCart_WhenExistingItemIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shoppingCart = new ShoppingCart(userId);
        var product = new Product("Test Product", 10.00m);

        // Act
        shoppingCart.AddItem(product.Id, product.Name, product.Price, quantity: 1);

        // Assert
        Assert.Single(shoppingCart.Items);
        Assert.Equal(product.Id, shoppingCart.Items.First().ProductId);
        Assert.Equal(1, shoppingCart.Items.First().Quantity);
    }

    [Fact]
    public void AddItem_IncrementsQuantity_WhenExistingItemIsNotNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shoppingCart = new ShoppingCart(userId);
        var product = new Product("Test Product", 10.00m);

        shoppingCart.AddItem(product.Id, product.Name, product.Price, quantity: 1);

        // Act
        shoppingCart.AddItem(product.Id, product.Name, product.Price, quantity: 1);

        // Assert
        Assert.Single(shoppingCart.Items);
        Assert.Equal(product.Id, shoppingCart.Items.First().ProductId);
        Assert.Equal(2, shoppingCart.Items.First().Quantity);
    }
}

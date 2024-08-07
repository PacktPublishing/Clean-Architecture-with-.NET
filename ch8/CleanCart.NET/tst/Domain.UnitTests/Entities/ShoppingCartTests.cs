﻿namespace Domain.UnitTests.Entities
{
    public class ShoppingCartTests
    {
        [Fact]
        public void AddItem_AddsNewItemToCart_WhenExistingItemIsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var shoppingCart = new ShoppingCart(userId);
            var product = new Product(Guid.NewGuid(), "Test Product", 10.00m, 5, "");

            // Act
            shoppingCart.AddItem(product, 1);

            // Assert
            Assert.Single(shoppingCart.Items);
            Assert.Equal(product.Id, shoppingCart.Items[0].ProductId);
            Assert.Equal(1, shoppingCart.Items[0].Quantity);
        }

        [Fact]
        public void AddItem_IncrementsQuantity_WhenExistingItemIsNotNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var shoppingCart = new ShoppingCart(userId);
            var product = new Product(Guid.NewGuid(), "Test Product", 10.00m, 5, "");

            shoppingCart.AddItem(product, 1);

            // Act
            shoppingCart.AddItem(product, 1);

            // Assert
            Assert.Single(shoppingCart.Items);
            Assert.Equal(product.Id, shoppingCart.Items[0].ProductId);
            Assert.Equal(2, shoppingCart.Items[0].Quantity);
        }

        [Fact]
        public void RemoveItem_RemovesItemFromCart_WhenQuantityIsEqualToExistingItemQuantity()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var shoppingCart = new ShoppingCart(userId);
            var product = new Product(Guid.NewGuid(), "Test Product", 10.00m, 5, "");

            shoppingCart.AddItem(product, 1);

            // Act
            shoppingCart.RemoveItem(product.Id, 1);

            // Assert
            Assert.Empty(shoppingCart.Items);
        }

        [Fact]
        public void RemoveItem_RemovesItemFromCart_WhenQuantityIsGreaterThanExistingItemQuantity()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var shoppingCart = new ShoppingCart(userId);
            var product = new Product(Guid.NewGuid(), "Test Product", 10.00m, 5, "");

            shoppingCart.AddItem(product, 1);

            // Act
            shoppingCart.RemoveItem(product.Id, 2);

            // Assert
            Assert.Empty(shoppingCart.Items);
        }

        [Fact]
        public void RemoveItem_RemovesItemFromCart_WhenQuantityIsLessThanExistingItemQuantity()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var shoppingCart = new ShoppingCart(userId);
            var product = new Product(Guid.NewGuid(), "Test Product", 10.00m, 5, "");

            shoppingCart.AddItem(product, 2);

            // Act
            shoppingCart.RemoveItem(product.Id, 1);

            // Assert
            Assert.Single(shoppingCart.Items);
            Assert.Equal(product.Id, shoppingCart.Items[0].ProductId);
            Assert.Equal(1, shoppingCart.Items[0].Quantity);
        }
    }
}

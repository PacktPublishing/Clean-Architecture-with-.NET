using Application.Interfaces.Data;
using Application.UseCases.RemoveItemFromCart;
using Domain.Entities;
using Moq;

namespace Application.UnitTests.UseCases;

public class RemoveItemFromCartUseCaseTests
{
    [Fact]
    public async Task RemoveItemFromCartAsync_ValidInput_RemovesItemFromCart()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        int removeQuantity = 2;
        int initialQuantity = 5;
        int expectedQuantity = initialQuantity - removeQuantity;

        var shoppingCart = new ShoppingCart(userId);
        var product = new Product(productId, "Product", 10.0m, 5, "");
        shoppingCart.AddItem(product, initialQuantity);

        var mockRepository = new Mock<IShoppingCartRepository>();
        mockRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(shoppingCart);

        var useCase = new RemoveItemFromCartUseCase(mockRepository.Object);
        var input = new RemoveItemFromCartInput(userId, productId, removeQuantity);

        // Act
        await useCase.RemoveItemFromCartAsync(input);

        // Assert
        var item = shoppingCart.Items.SingleOrDefault(i => i.ProductId == productId);
        Assert.NotNull(item);
        Assert.Equal(expectedQuantity, item.Quantity);
    }
}
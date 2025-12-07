using Application.Interfaces.Data;
using Application.Operations.UseCases.RemoveItemFromCart;
using Domain.Entities;
using Moq;

namespace Application.UnitTests.UseCases;

public class RemoveItemFromCartCommandHandlerTests
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

        var mockQueryRepository = new Mock<IShoppingCartQueryRepository>();
        mockQueryRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(shoppingCart);

        var mockCommandRepository = new Mock<IShoppingCartCommandRepository>();
        mockCommandRepository.Setup(repo => repo.UpdateAsync(shoppingCart, It.IsAny<CancellationToken>())).ReturnsAsync(shoppingCart.Id);

        var useCase = new RemoveItemFromCartCommandHandler(mockQueryRepository.Object, mockCommandRepository.Object);
        var command = new RemoveItemFromCartCommand(userId, productId, removeQuantity);

        // Act
        await useCase.Handle(command, CancellationToken.None);

        // Assert
        var item = shoppingCart.Items.SingleOrDefault(i => i.ProductId == productId);
        Assert.NotNull(item);
        Assert.Equal(expectedQuantity, item.Quantity);
    }
}
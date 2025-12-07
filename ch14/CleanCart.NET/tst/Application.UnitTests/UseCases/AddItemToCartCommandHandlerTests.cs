using Application.Interfaces.Data;
using Application.Operations.UseCases.AddItemToCart;
using Domain.Entities;
using Moq;

namespace Application.UnitTests.UseCases;

public class AddItemToCartCommandHandlerTests
{
    [Fact]
    public async Task AddItemToCartAsync_ValidInput_AddsItemToCart()
    {
        // Arrange
        var shoppingCartQueryRepository = new Mock<IShoppingCartQueryRepository>();
        var shoppingCartCommandRepository = new Mock<IShoppingCartCommandRepository>();
        var productRepository = new Mock<IProductQueryRepository>();
        var userId = Guid.NewGuid();

        var shoppingCart = new ShoppingCart(userId);
        var product = new Product(Guid.NewGuid(), "Test Product", 10.00m, 5, "");

        shoppingCartQueryRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(shoppingCart);
        productRepository.Setup(r => r.GetByIdAsync(product.Id, CancellationToken.None))
            .ReturnsAsync(product);

        var useCase = new AddItemToCartCommandHandler(shoppingCartQueryRepository.Object, shoppingCartCommandRepository.Object, productRepository.Object);

        // Act
        await useCase.Handle(new AddItemToCartCommand(userId, product.Id, 1), CancellationToken.None);

        // Assert
        productRepository.Verify(r => r.GetByIdAsync(product.Id, CancellationToken.None), Times.Once);
        shoppingCartQueryRepository.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
        shoppingCartCommandRepository.Verify(r => r.SaveAsync(shoppingCart), Times.Once);
    }
}
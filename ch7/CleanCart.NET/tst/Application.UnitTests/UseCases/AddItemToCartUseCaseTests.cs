using Application.Interfaces.Data;
using Application.UseCases.AddItemToCart;
using Domain.Entities;
using Moq;

namespace Application.UnitTests.UseCases
{
    public class AddItemToCartUseCaseTests
    {
        [Fact]
        public void Can_AddItemToCart()
        {
            // Arrange
            var shoppingCartRepository = new Mock<IShoppingCartRepository>();
            var productRepository = new Mock<IProductRepository>();
            var userId = Guid.NewGuid();

            var shoppingCart = new ShoppingCart(userId);
            var product = new Product(Guid.NewGuid(), "Test Product", 10.00m, 5);

            shoppingCartRepository.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(shoppingCart);
            productRepository.Setup(r => r.GetByIdAsync(product.Id))
                .ReturnsAsync(product);

            var useCase = new AddItemToCartUseCase(shoppingCartRepository.Object, productRepository.Object);

            // Act
            _ = useCase.ExecuteAsync(new AddItemToCartInput(userId, product.Id, 1));

            // Assert
            productRepository.Verify(r => r.GetByIdAsync(product.Id), Times.Once);
            shoppingCartRepository.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
            shoppingCartRepository.Verify(r => r.SaveAsync(shoppingCart), Times.Once);
        }
    }
}
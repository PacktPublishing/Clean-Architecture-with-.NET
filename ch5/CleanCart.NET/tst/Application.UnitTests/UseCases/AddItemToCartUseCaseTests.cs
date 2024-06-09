using Application.Interfaces.Data;
using Application.UseCases.AddItemToCart;
using Domain.Entities;
using Moq;

namespace Application.UnitTests.UseCases
{
    public class AddItemToCartUseCaseTests
    {
        [Fact]
        public void AddItemToCartAsync_ValidInput_AddsItemToCart()
        {
            // Arrange
            var shoppingCartRepository = new Mock<IShoppingCartRepository>();
            var productRepository = new Mock<IProductRepository>();
            var customerId = Guid.NewGuid();

            var shoppingCart = new ShoppingCart(customerId);
            var product = new Product(Guid.NewGuid(), "Test Product", 10.00m, 5);

            shoppingCartRepository.Setup(r => r.GetByCustomerIdAsync(customerId))
                .ReturnsAsync(shoppingCart);
            productRepository.Setup(r => r.GetByIdAsync(product.Id))
                .ReturnsAsync(product);

            var useCase = new AddItemToCartUseCase(shoppingCartRepository.Object, productRepository.Object);

            // Act
            _ = useCase.AddItemToCartAsync(new AddItemToCartInput(customerId, product.Id, 1));

            // Assert
            productRepository.Verify(r => r.GetByIdAsync(product.Id), Times.Once);
            shoppingCartRepository.Verify(r => r.GetByCustomerIdAsync(customerId), Times.Once);
            shoppingCartRepository.Verify(r => r.SaveAsync(shoppingCart), Times.Once);
        }
    }
}
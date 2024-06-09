using Application.Interfaces.Data;
using Application.UseCases.CalculateCartTotal;
using Domain.Entities;
using Moq;

namespace Application.UnitTests.UseCases
{
    public class CalculateCartTotalUseCaseTests
    {
        [Fact]
        public async Task CalculateTotalAsync_ValidInput_CalculatesCartTotal()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var shoppingCart = new ShoppingCart(customerId);
            var product1 = new Product(Guid.NewGuid(), "Product1", 10.0m, 5);
            var product2 = new Product(Guid.NewGuid(), "Product2", 15.0m, 3);
            shoppingCart.AddItem(product1, 2);
            shoppingCart.AddItem(product2, 1);

            var mockRepository = new Mock<IShoppingCartRepository>();
            mockRepository.Setup(repo => repo.GetByCustomerIdAsync(customerId)).ReturnsAsync(shoppingCart);

            var useCase = new CalculateCartTotalUseCase(mockRepository.Object);
            var input = new CalculateCartTotalInput { CustomerId = customerId };

            // Act
            decimal total = await useCase.CalculateTotalAsync(input);

            // Assert
            decimal expectedTotal = (2 * 10.0m + 1 * 15.0m) * (1 + 0.08M); // Subtotal + Tax
            Assert.Equal(expectedTotal, total);
        }
    }
}

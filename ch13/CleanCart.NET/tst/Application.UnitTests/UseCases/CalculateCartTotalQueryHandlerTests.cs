using Application.Interfaces.Data;
using Application.Operations.UseCases.CalculateCartTotal;
using Domain.Entities;
using Moq;

namespace Application.UnitTests.UseCases;

public class CalculateCartTotalQueryHandlerTests
{
    [Fact]
    public async Task CalculateTotalAsync_ValidInput_CalculatesCartTotal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shoppingCart = new ShoppingCart(userId);
        var product1 = new Product(Guid.NewGuid(), "Product1", 10.0m, 5, "");
        var product2 = new Product(Guid.NewGuid(), "Product2", 15.0m, 3, "");
        shoppingCart.AddItem(product1, 2);
        shoppingCart.AddItem(product2, 1);

        var mockRepository = new Mock<IShoppingCartQueryRepository>();
        mockRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(shoppingCart);

        var useCase = new CalculateCartTotalQueryHandler(mockRepository.Object);
        var query = new CalculateCartTotalQuery(userId);

        // Act
        decimal total = await useCase.Handle(query, CancellationToken.None);

        // Assert
        decimal expectedTotal = (2 * 10.0m + 1 * 15.0m) * (1 + 0.08M); // Subtotal + Tax
        Assert.Equal(expectedTotal, total);
    }
}
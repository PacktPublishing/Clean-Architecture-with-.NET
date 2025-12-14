using Application.Interfaces.Data;
using Application.UseCases.CalculateCartTotal;
using Domain.Entities;
using NSubstitute;

namespace Application.UnitTests.UseCases;

public class CalculateCartTotalUseCaseTests
{
    [Fact]
    public async Task CalculateTotalAsync_ValidInput_CalculatesCartTotal()
    {
        var userId = Guid.NewGuid();
        var shoppingCart = new ShoppingCart(userId);
        var product1 = new Product("Product1", 10.0m, 5, "img.png");
        var product2 = new Product("Product2", 15.0m, 3, "img.png");
        shoppingCart.AddItem(product1.Id, product1.Name, product1.Price, 2);
        shoppingCart.AddItem(product2.Id, product2.Name, product2.Price, 1);

        var mockRepository = Substitute.For<IShoppingCartRepository>();
        mockRepository.GetByUserIdAsync(userId).Returns(shoppingCart);

        var useCase = new CalculateCartTotalUseCase(mockRepository);
        var input = new CalculateCartTotalInput(userId);

        // Act
        decimal total = await useCase.CalculateTotalAsync(input);

        // Assert
        decimal expectedTotal = (2 * 10.0m + 1 * 15.0m) * (1 + 0.08M); // Subtotal + Tax
        Assert.Equal(expectedTotal, total);
    }
}
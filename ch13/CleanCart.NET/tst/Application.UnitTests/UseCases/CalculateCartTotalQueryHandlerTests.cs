using Application.Interfaces.Data;
using Application.Operations.UseCases.CalculateCartTotal;
using Domain.Entities;
using NSubstitute;

namespace Application.UnitTests.UseCases;

public class CalculateCartTotalQueryHandlerTests
{
    [Fact]
    public async Task CalculateCartTotalQueryHandler_ValidInput_CalculatesCartTotal()
    {
        var userId = Guid.NewGuid();
        var shoppingCart = new ShoppingCart(userId);
        var product1 = new Product("Product1", 10.0m, 5, "img.png");
        var product2 = new Product("Product2", 15.0m, 3, "img.png");
        shoppingCart.AddItem(product1.Id, product1.Name, product1.Price, 2);
        shoppingCart.AddItem(product2.Id, product2.Name, product2.Price, 1);

        var mockRepository = Substitute.For<IShoppingCartQueryRepository>();
        mockRepository.GetByUserIdAsync(userId).Returns(shoppingCart);

        var useCase = new CalculateCartTotalQueryHandler(mockRepository);
        var query = new CalculateCartTotalQuery(userId);

        decimal total = await useCase.Handle(query, CancellationToken.None);

        decimal expectedTotal = (2 * 10.0m + 1 * 15.0m) * (1 + 0.08M); // Subtotal + Tax
        Assert.Equal(expectedTotal, total);
    }
}
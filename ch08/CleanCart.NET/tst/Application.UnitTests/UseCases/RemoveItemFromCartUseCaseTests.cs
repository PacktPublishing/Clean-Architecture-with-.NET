using Application.Interfaces.Data;
using Application.UseCases.RemoveItemFromCart;
using Domain.Entities;
using NSubstitute;

namespace Application.UnitTests.UseCases;

public class RemoveItemFromCartUseCaseTests
{
    [Fact]
    public async Task RemoveItemFromCartAsync_ValidInput_RemovesItemFromCart()
    {
        var userId = Guid.NewGuid();
        int removeQuantity = 2;
        int initialQuantity = 5;
        int expectedQuantity = initialQuantity - removeQuantity;

        var shoppingCart = new ShoppingCart(userId);
        var product = new Product("Product", 10.0m, 5);
        shoppingCart.AddItem(product.Id, product.Name, product.Price, initialQuantity);

        var mockRepository = Substitute.For<IShoppingCartRepository>();
        mockRepository.GetByUserIdAsync(userId).Returns(shoppingCart);

        var useCase = new RemoveItemFromCartUseCase(mockRepository);
        var input = new RemoveItemFromCartInput(userId, product.Id, removeQuantity);

        await useCase.RemoveItemFromCartAsync(input);

        var item = shoppingCart.Items.SingleOrDefault(i => i.ProductId == product.Id);
        Assert.NotNull(item);
        Assert.Equal(expectedQuantity, item.Quantity);
    }
}
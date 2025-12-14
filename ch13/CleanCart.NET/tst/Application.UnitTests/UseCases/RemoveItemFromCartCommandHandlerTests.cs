using Application.Interfaces.Data;
using Application.Operations.UseCases.RemoveItemFromCart;
using Domain.Entities;
using NSubstitute;

namespace Application.UnitTests.UseCases;

public class RemoveItemFromCartCommandHandlerTests
{
    [Fact]
    public async Task RemoveItemFromCartAsync_ValidInput_RemovesItemFromCart()
    {
        var userId = Guid.NewGuid();
        int removeQuantity = 2;
        int initialQuantity = 5;
        int expectedQuantity = initialQuantity - removeQuantity;

        var shoppingCart = new ShoppingCart(userId);
        var product = new Product("Product", 10.0m, 5, "img.png");
        shoppingCart.AddItem(product.Id, product.Name, product.Price, initialQuantity);

        var mockQueryRepository = Substitute.For<IShoppingCartQueryRepository>();
        mockQueryRepository.GetByUserIdAsync(userId).Returns(shoppingCart);

        var mockCommandRepository = Substitute.For<IShoppingCartCommandRepository>();
        mockCommandRepository.UpdateAsync(shoppingCart, Arg.Any<CancellationToken>()).Returns(shoppingCart.Id);

        var useCase = new RemoveItemFromCartCommandHandler(mockQueryRepository, mockCommandRepository);
        var command = new RemoveItemFromCartCommand(userId, product.Id, removeQuantity);

        await useCase.Handle(command, CancellationToken.None);

        var item = shoppingCart.Items.SingleOrDefault(i => i.ProductId == product.Id);
        Assert.NotNull(item);
        Assert.Equal(expectedQuantity, item.Quantity);
    }
}
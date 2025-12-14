using Application.Interfaces.Data;
using Application.Operations.UseCases.AddItemToCart;
using Domain.Entities;
using NSubstitute;

namespace Application.UnitTests.UseCases;

public class AddItemToCartCommandHandlerTests
{
    [Fact]
    public async Task AddItemToCartCommandHandler_ValidInput_AddsItemToCart()
    {
        var shoppingCartQueryRepository = Substitute.For<IShoppingCartQueryRepository>();
        var shoppingCartCommandRepository = Substitute.For<IShoppingCartCommandRepository>();
        var productRepository = Substitute.For<IProductQueryRepository>();
        var userId = Guid.NewGuid();

        var shoppingCart = new ShoppingCart(userId);
        var product = new Product("Test Product", 10.00m, 5, "img.png");

        shoppingCartQueryRepository.GetByUserIdAsync(userId).Returns(shoppingCart);
        productRepository.GetByIdAsync(product.Id, CancellationToken.None).Returns(product);

        var useCase = new AddItemToCartCommandHandler(shoppingCartQueryRepository, shoppingCartCommandRepository, productRepository);

        await useCase.Handle(new AddItemToCartCommand(userId, product.Id, 1), CancellationToken.None);

        await productRepository.Received(1).GetByIdAsync(product.Id);
        await shoppingCartQueryRepository.Received(1).GetByUserIdAsync(userId);
        await shoppingCartCommandRepository.Received(1).SaveAsync(shoppingCart);
    }

    [Fact]
    public async Task AddItemToCartCommandHandler_ShouldCreateNewCart_WhenCartDoesNotExist()
    {
        var shoppingCartQueryRepository = Substitute.For<IShoppingCartQueryRepository>();
        var shoppingCartCommandRepository = Substitute.For<IShoppingCartCommandRepository>();
        var productRepository = Substitute.For<IProductQueryRepository>();

        var userId = Guid.NewGuid();
        var product = new Product("Test Product", 10.00m, 2, "img.png");

        // Simulate that no cart exists yet
        shoppingCartQueryRepository.GetByUserIdAsync(userId).Returns((ShoppingCart?)null);
        productRepository.GetByIdAsync(product.Id).Returns(product);

        var useCase = new AddItemToCartCommandHandler(shoppingCartQueryRepository, shoppingCartCommandRepository, productRepository);

        await useCase.Handle(new AddItemToCartCommand(userId, product.Id, quantity: 2), CancellationToken.None);

        await shoppingCartCommandRepository.Received(1).SaveAsync(Arg.Is<ShoppingCart>(
            cart => cart.UserId == userId &&
                    cart.Items.Any(i => i.ProductId == product.Id && i.Quantity == 2)
        ));
    }

    [Fact]
    public async Task AddItemToCartCommandHandler_ShouldThrow_WhenRequestedQuantityExceedsStock()
    {
        var shoppingCartQueryRepository = Substitute.For<IShoppingCartQueryRepository>();
        var shoppingCartCommandRepository = Substitute.For<IShoppingCartCommandRepository>();
        var productRepository = Substitute.For<IProductQueryRepository>();
        var userId = Guid.NewGuid();

        var product = new Product("Limited Edition Item", 25.00m, stockLevel: 1, "img.png"); // Only 1 in stock

        productRepository.GetByIdAsync(product.Id).Returns(product);
        shoppingCartQueryRepository.GetByUserIdAsync(userId).Returns(new ShoppingCart(userId));

        var useCase = new AddItemToCartCommandHandler(shoppingCartQueryRepository, shoppingCartCommandRepository, productRepository);

        var input = new AddItemToCartCommand(userId, product.Id, quantity: 2); // Requesting more than available

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.Handle(input, CancellationToken.None));

        Assert.Contains("Not enough stock", exception.Message);

        await shoppingCartCommandRepository.DidNotReceive().SaveAsync(Arg.Any<ShoppingCart>());
    }
}
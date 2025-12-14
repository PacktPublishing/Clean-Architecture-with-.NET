using Application.Interfaces.Data;
using Domain.Entities;
using MediatR;

namespace Application.Operations.UseCases.AddItemToCart;

public class AddItemToCartCommandHandler(
    IShoppingCartQueryRepository shoppingCartQueryRepository,
    IShoppingCartCommandRepository shoppingCartCommandRepository,
    IProductQueryRepository productQueryRepository)
    : IRequestHandler<AddItemToCartCommand>
{
    public async Task Handle(AddItemToCartCommand command, CancellationToken cancellationToken)
    {
        Product? product = await productQueryRepository.GetByIdAsync(command.ProductId, cancellationToken);

        if (product is null)
            return; // Alternatively, throw an exception or handle the error as needed

        if (product.StockLevel < command.Quantity)
        {
            throw new InvalidOperationException(
                $"Not enough stock for '{product.Name}'. Requested: {command.Quantity}, Available: {product.StockLevel}");
        }

        var shoppingCart = await shoppingCartQueryRepository.GetByUserIdAsync(command.UserId)
                           ?? new ShoppingCart(command.UserId);

        shoppingCart.AddItem(product.Id, product.Name, product.Price, command.Quantity);

        await shoppingCartCommandRepository.SaveAsync(shoppingCart);
    }
}
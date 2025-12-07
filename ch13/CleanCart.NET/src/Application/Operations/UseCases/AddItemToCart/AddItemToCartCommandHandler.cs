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
        ShoppingCart cart = await shoppingCartQueryRepository.GetByUserIdAsync(command.UserId) ?? new ShoppingCart(command.UserId);
        Product? product = await productQueryRepository.GetByIdAsync(command.ProductId, cancellationToken);

        if (product is null)
        {
            throw new ArgumentException($"Product '{command.ProductId}' does not exist.");
        }
        
        cart.AddItem(product, command.Quantity);

        await shoppingCartCommandRepository.SaveAsync(cart);
    }
}
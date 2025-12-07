using Application.Interfaces.Data;
using Domain.Entities;
using MediatR;

namespace Application.Operations.UseCases.RemoveItemFromCart;

public class RemoveItemFromCartCommandHandler(
    IShoppingCartQueryRepository shoppingCartQueryRepository,
    IShoppingCartCommandRepository shoppingCartCommandRepository)
    : IRequestHandler<RemoveItemFromCartCommand>
{
    public async Task Handle(RemoveItemFromCartCommand command, CancellationToken cancellationToken)
    {
        ShoppingCart? cart = await shoppingCartQueryRepository.GetByUserIdAsync(command.UserId);

        if (cart != null)
        {
            cart.RemoveItem(command.ProductId, command.Quantity);

            await shoppingCartCommandRepository.UpdateAsync(cart, cancellationToken);

            if (!cart.Items.Any())
            {
                await shoppingCartCommandRepository.DeleteByUserIdAsync(command.UserId);
            }
        }
    }
}
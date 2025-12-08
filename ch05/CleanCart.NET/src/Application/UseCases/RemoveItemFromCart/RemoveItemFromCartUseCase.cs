using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using Domain.Entities;

namespace Application.UseCases.RemoveItemFromCart;

public class RemoveItemFromCartUseCase(IShoppingCartRepository shoppingCartRepository) : IRemoveItemFromCartUseCase
{
    public async Task RemoveItemFromCartAsync(RemoveItemFromCartInput input)
    {
        ShoppingCart? cart = await shoppingCartRepository.GetByUserIdAsync(input.UserId);

        if (cart != null)
        {
            cart.RemoveItem(input.ProductId, input.Quantity);
            await shoppingCartRepository.SaveAsync(cart);
        }
    }
}

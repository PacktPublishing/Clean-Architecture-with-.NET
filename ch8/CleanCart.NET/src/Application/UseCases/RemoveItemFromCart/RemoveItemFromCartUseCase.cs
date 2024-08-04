using System.Linq;
using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.UseCases.RemoveItemFromCart
{
    public class RemoveItemFromCartUseCase : IRemoveItemFromCartUseCase
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;

        public RemoveItemFromCartUseCase(IShoppingCartRepository shoppingCartRepository)
        {
            _shoppingCartRepository = shoppingCartRepository;
        }

        public async Task RemoveItemFromCartAsync(RemoveItemFromCartInput input)
        {
            ShoppingCart? cart = await _shoppingCartRepository.GetByUserIdAsync(input.UserId);

            if (cart != null)
            {
                cart.RemoveItem(input.ProductId, input.Quantity);

                await _shoppingCartRepository.SaveAsync(cart);

                if (!cart.Items.Any())
                {
                    await _shoppingCartRepository.DeleteAsync(input.UserId);
                }
            }
        }
    }
}

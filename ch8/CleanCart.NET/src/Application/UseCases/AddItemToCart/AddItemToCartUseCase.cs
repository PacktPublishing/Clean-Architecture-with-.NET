using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using Domain.Entities;

namespace Application.UseCases.AddItemToCart;

public class AddItemToCartUseCase(
    IShoppingCartRepository shoppingCartRepository,
    IProductRepository productRepository)
    : IAddItemToCartUseCase
{
    public async Task AddItemToCartAsync(AddItemToCartInput input)
    {
        ShoppingCart cart = await shoppingCartRepository.GetByUserIdAsync(input.UserId);
        Product product = await productRepository.GetByIdAsync(input.ProductId);

        cart.AddItem(product, input.Quantity);

        await shoppingCartRepository.SaveAsync(cart);
    }
}
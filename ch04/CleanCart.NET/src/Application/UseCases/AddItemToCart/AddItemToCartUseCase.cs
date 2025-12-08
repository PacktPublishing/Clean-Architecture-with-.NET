using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using Domain.Entities;
using System.Threading.Tasks;

namespace Application.UseCases.AddItemToCart;

public class AddItemToCartUseCase(
    IShoppingCartRepository shoppingCartRepository,
    IProductRepository productRepository)
    : IAddItemToCartUseCase
{
    public async Task AddItemToCartAsync(AddItemToCartInput input)
    {
        var product = await productRepository.GetByIdAsync(input.ProductId);
        if (product is null)
            return; // Alternatively, throw an exception or handle the error as needed

        var shoppingCart = await shoppingCartRepository.GetByUserIdAsync(input.UserId)
                           ?? new ShoppingCart(input.UserId);

        shoppingCart.AddItem(product.Id, product.Name, product.Price, input.Quantity);

        await shoppingCartRepository.SaveAsync(shoppingCart);
    }
}

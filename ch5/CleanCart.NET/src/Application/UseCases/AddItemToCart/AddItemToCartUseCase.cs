using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.UseCases.AddItemToCart
{
    public class AddItemToCartUseCase : IAddItemToCartUseCase
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IProductRepository _productRepository;

        public AddItemToCartUseCase(
            IShoppingCartRepository shoppingCartRepository,
            IProductRepository productRepository)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _productRepository = productRepository;
        }

        public async Task ExecuteAsync(AddItemToCartInput input)
        {
            ShoppingCart cart = await _shoppingCartRepository.GetByCustomerIdAsync(input.CustomerId);
            Product product = await _productRepository.GetByIdAsync(input.ProductId);

            cart.AddItem(product, input.Quantity);

            await _shoppingCartRepository.SaveAsync(cart);
        }
    }
}

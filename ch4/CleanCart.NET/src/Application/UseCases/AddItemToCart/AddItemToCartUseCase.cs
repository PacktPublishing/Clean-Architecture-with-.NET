using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using System.Threading.Tasks;

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

        public async Task AddItemToCartAsync(AddItemToCartInput input)
        {
            var shoppingCart = await _shoppingCartRepository.GetByCustomerIdAsync(input.CustomerId);
            var product = await _productRepository.GetByIdAsync(input.ProductId);

            shoppingCart.AddItem(product, input.Quantity);

            await _shoppingCartRepository.SaveAsync(shoppingCart);
        }
    }
}

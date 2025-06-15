using Application.Interfaces.Data;
using Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Operations.UseCases.AddItemToCart
{
    public class AddItemToCartCommandHandler : IRequestHandler<AddItemToCartCommand>
    {
        private readonly IShoppingCartQueryRepository _shoppingCartQueryRepository;
        private readonly IShoppingCartCommandRepository _shoppingCartCommandRepository;
        private readonly IProductQueryRepository _productQueryRepository;

        public AddItemToCartCommandHandler(
            IShoppingCartQueryRepository shoppingCartQueryRepository,
            IShoppingCartCommandRepository shoppingCartCommandRepository,
            IProductQueryRepository productQueryRepository)
        {
            _shoppingCartQueryRepository = shoppingCartQueryRepository;
            _shoppingCartCommandRepository = shoppingCartCommandRepository;
            _productQueryRepository = productQueryRepository;
        }

        public async Task Handle(AddItemToCartCommand command, CancellationToken cancellationToken)
        {
            ShoppingCart cart = await _shoppingCartQueryRepository.GetByUserIdAsync(command.UserId) ?? new ShoppingCart(command.UserId);
            Product product = await _productQueryRepository.GetByIdAsync(command.ProductId);

            cart.AddItem(product, command.Quantity);

            await _shoppingCartCommandRepository.SaveAsync(cart);
        }
    }
}

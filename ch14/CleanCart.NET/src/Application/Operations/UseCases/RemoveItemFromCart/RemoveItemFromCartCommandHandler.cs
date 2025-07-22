using Application.Interfaces.Data;
using Domain.Entities;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Operations.UseCases.RemoveItemFromCart
{
    public class RemoveItemFromCartCommandHandler : IRequestHandler<RemoveItemFromCartCommand>
    {
        private readonly IShoppingCartQueryRepository _shoppingCartQueryRepository;
        private readonly IShoppingCartCommandRepository _shoppingCartCommandRepository;

        public RemoveItemFromCartCommandHandler(IShoppingCartQueryRepository shoppingCartQueryRepository, IShoppingCartCommandRepository shoppingCartCommandRepository)
        {
            _shoppingCartQueryRepository = shoppingCartQueryRepository;
            _shoppingCartCommandRepository = shoppingCartCommandRepository;
        }

        public async Task Handle(RemoveItemFromCartCommand command, CancellationToken cancellationToken)
        {
            ShoppingCart? cart = await _shoppingCartQueryRepository.GetByUserIdAsync(command.UserId);

            if (cart != null)
            {
                cart.RemoveItem(command.ProductId, command.Quantity);

                await _shoppingCartCommandRepository.UpdateAsync(cart, cancellationToken);

                if (!cart.Items.Any())
                {
                    await _shoppingCartCommandRepository.DeleteByUserIdAsync(command.UserId);
                }
            }
        }
    }
}

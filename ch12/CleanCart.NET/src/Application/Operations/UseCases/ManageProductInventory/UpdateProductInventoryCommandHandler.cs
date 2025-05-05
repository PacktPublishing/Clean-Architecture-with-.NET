using Application.Interfaces.Data;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Operations.UseCases.ManageProductInventory
{
    public class UpdateProductInventoryCommandHandler : IRequestHandler<UpdateProductInventoryCommand>
    {
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IProductQueryRepository _productQueryRepository;
        private readonly IProductCommandRepository _productCommandRepository;

        public UpdateProductInventoryCommandHandler(
            IUserQueryRepository userQueryRepository,
            IProductQueryRepository productQueryRepository,
            IProductCommandRepository productCommandRepository)
        {
            _userQueryRepository = userQueryRepository;
            _productQueryRepository = productQueryRepository;
            _productCommandRepository = productCommandRepository;
        }

        public async Task Handle(UpdateProductInventoryCommand command, CancellationToken cancellationToken)
        {
            User? user = await _userQueryRepository.GetByIdAsync(command.UserId);

            if (user == null)
            {
                throw new ArgumentException($"User '{command.UserId}' does not exist.");
            }

            if (!user.Roles.Contains(UserRole.Administrator))
            {
                throw new UnauthorizedAccessException("User is not authorized to manage product inventory.");
            }

            Product product = await _productQueryRepository.GetByIdAsync(command.ProductId);
            product.UpdateStockLevel(command.StockLevel);
            await _productCommandRepository.UpdateAsync(product, cancellationToken);
        }
    }
}

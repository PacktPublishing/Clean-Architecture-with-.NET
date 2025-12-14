using Application.Interfaces.Data;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Operations.UseCases.ManageProductInventory;

public class UpdateProductInventoryCommandHandler(
    IUserQueryRepository userQueryRepository,
    IProductQueryRepository productQueryRepository,
    IProductCommandRepository productCommandRepository)
    : IRequestHandler<UpdateProductInventoryCommand>
{
    public async Task Handle(UpdateProductInventoryCommand command, CancellationToken cancellationToken)
    {
        User? user = await userQueryRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        if (!user.Roles.Contains(UserRole.Administrator))
        {
            throw new UnauthorizedAccessException("User is not authorized to manage product inventory.");
        }

        Product? product = await productQueryRepository.GetByIdAsync(command.ProductId, cancellationToken);

        if (product is null)
            return; // Log or handle as needed

        product.UpdateStockLevel(command.StockLevel);
        await productCommandRepository.UpdateAsync(product, cancellationToken);
    }
}
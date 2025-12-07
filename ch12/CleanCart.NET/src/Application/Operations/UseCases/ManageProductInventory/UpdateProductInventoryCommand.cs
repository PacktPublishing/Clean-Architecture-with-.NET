using MediatR;

namespace Application.Operations.UseCases.ManageProductInventory;

public class UpdateProductInventoryCommand(Guid userId, Guid productId, int stockLevel) : IRequest
{
    public Guid UserId { get; } = userId;

    public Guid ProductId { get; } = productId;

    public int StockLevel { get; } = stockLevel;
}
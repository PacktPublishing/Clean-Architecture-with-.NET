using MediatR;
using System;

namespace Application.Operations.UseCases.ManageProductInventory
{
    public class UpdateProductInventoryCommand : IRequest
    {
        public UpdateProductInventoryCommand(Guid userId, Guid productId, int stockLevel)
        {
            UserId = userId;
            ProductId = productId;
            StockLevel = stockLevel;
        }

        public Guid UserId { get; }

        public Guid ProductId { get; }

        public int StockLevel { get; }
    }
}

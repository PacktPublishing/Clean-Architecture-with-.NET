using MediatR;
using System;

namespace Application.Operations.UseCases.RemoveItemFromCart
{
    public class RemoveItemFromCartCommand : IRequest
    {
        public RemoveItemFromCartCommand(Guid userId, Guid productId, int quantity)
        {
            UserId = userId;
            ProductId = productId;
            Quantity = quantity;
        }

        public Guid UserId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; }
    }
}

using System;
using MediatR;

namespace Application.Operations.UseCases.AddItemToCart
{
    public class AddItemToCartCommand : IRequest
    {
        public AddItemToCartCommand(Guid userId, Guid productId, int quantity)
        {
            UserId = userId;
            ProductId = productId;
            Quantity = quantity;
        }

        public Guid UserId { get; }

        public Guid ProductId { get; }

        public int Quantity { get; }
    }
}

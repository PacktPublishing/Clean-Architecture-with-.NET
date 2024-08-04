using System;

namespace Application.UseCases.AddItemToCart
{
    public class AddItemToCartInput
    {
        public Guid UserId { get; }
        public Guid ProductId { get; }
        public int Quantity { get; }

        public AddItemToCartInput(Guid userId, Guid productId, int quantity)
        {
            UserId = userId;
            ProductId = productId;
            Quantity = quantity;
        }
    }
}
